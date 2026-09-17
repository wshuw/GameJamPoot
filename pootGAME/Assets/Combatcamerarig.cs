using UnityEngine;
using Unity.Cinemachine;

// Полностью самописная боевая камера. НЕ использует Cinemachine Body/Aim и
// CinemachineTargetGroup — вместо этого каждый кадр напрямую вычисляет
// transform.position/rotation этой CinemachineCamera так, чтобы игрок и
// цель ГАРАНТИРОВАННО помещались в кадр (аналитическая проверка углов
// относительно текущего FOV, а не приблизительное демпфированное
// кадрирование).
//
// Cinemachine Body/Aim на этой камере должны быть выставлены в "Do Nothing" —
// мы сами берём на себя их работу. Deoccluder (extension) можно оставить —
// расширения применяются Cinemachine ПОСЛЕ Body/Aim, то есть уже поверх
// вычисленной нами позиции, и будет корректно отталкивать камеру от стен.
[RequireComponent(typeof(CinemachineCamera))]
public class CombatCameraRig : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private LockOnController lockOnController;
    [Tooltip("Точка на игроке, которую камера обязана удерживать в кадре (например грудь/плечи). Если не задано — используется transform игрока")]
    [SerializeField] private Transform playerAimPoint;
    [Tooltip("Камера, с чьего Lens (FOV/aspect) считать безопасную зону кадра. Если не задано — берётся Camera.main")]
    [SerializeField] private Camera lensCamera;

    [Header("Ориентация")]
    [Tooltip("Наклон камеры вниз, градусы. Больше значение — сильнее 'вид сверху'")]
    [SerializeField] private float pitchAngle = 20f;
    [Tooltip("Высота камеры над игроком")]
    [SerializeField] private float heightOffset = 1.8f;
    [Tooltip("Сглаживание поворота камеры вслед за направлением игрок->враг (та самая 'камера крутится, когда игрок уходит вбок')")]
    [SerializeField] private float orbitSmoothTime = 0.15f;

    [Header("Дистанция (адаптивный зум)")]
    [SerializeField] private float minBackDistance = 3f;
    [SerializeField] private float maxBackDistance = 14f;
    [Tooltip("Сглаживание ТОЛЬКО при уменьшении дистанции (приближении). Отдаление, если оно требуется для гарантии кадра, применяется мгновенно")]
    [SerializeField] private float distanceShrinkSmoothTime = 0.25f;

    [Header("Безопасная зона кадра (доля от половины FOV, 0-1)")]
    [Tooltip("Например 0.85 — используем только 85% доступного поля зрения, оставляя запас по краям")]
    [Range(0.5f, 0.95f)][SerializeField] private float horizontalSafeFraction = 0.85f;
    [Range(0.5f, 0.95f)][SerializeField] private float verticalSafeFraction = 0.85f;

    [Header("Точность решения")]
    [Tooltip("Сколько итераций уточнения дистанции делать за кадр. 4-6 обычно более чем достаточно")]
    [SerializeField] private int solverIterations = 6;

    [Header("Приоритеты камер")]
    [SerializeField] private int combatPriority = 20;
    [SerializeField] private int combatIdlePriority = -10;

    private CinemachineCamera combatCamera;
    private bool wasLocked;
    private bool initialized;

    private float smoothedYaw;
    private float yawVelocity;
    private float smoothedDistance;
    private float distanceVelocity;

    private void Awake()
    {
        combatCamera = GetComponent<CinemachineCamera>();
        combatCamera.Priority = combatIdlePriority;

        if (lensCamera == null)
            lensCamera = Camera.main;
    }

    // LateUpdate — до того, как Cinemachine Brain (у неё поздний
    // DefaultExecutionOrder) прочитает финальный transform этой камеры.
    private void LateUpdate()
    {
        bool isLocked = lockOnController != null && lockOnController.IsLocked;

        if (isLocked && !wasLocked)
        {
            combatCamera.Priority = combatPriority;

            // Сброс накопленного состояния — новый лок не должен наследовать
            // ни направление, ни дистанцию от предыдущей боевой сессии.
            initialized = false;
            smoothedDistance = minBackDistance;
            distanceVelocity = 0f;
            yawVelocity = 0f;
        }
        else if (!isLocked && wasLocked)
        {
            combatCamera.Priority = combatIdlePriority;
        }

        wasLocked = isLocked;

        if (!isLocked)
            return;

        UpdateCameraPose();
    }

    private void UpdateCameraPose()
    {
        Transform enemyPoint = lockOnController.CurrentTarget.AimPoint;
        Vector3 playerPos = playerAimPoint != null ? playerAimPoint.position : lockOnController.transform.position;
        Vector3 enemyPos = enemyPoint.position;

        // 1. Направление игрок -> враг (горизонтальное), сглаженное по yaw
        Vector3 flat = enemyPos - playerPos;
        flat.y = 0f;

        Vector3 horizontalDir = flat.sqrMagnitude > 0.0001f
            ? flat.normalized
            : (Quaternion.Euler(0f, smoothedYaw, 0f) * Vector3.forward); // fallback, если враг ровно над/под игроком

        float targetYaw = Mathf.Atan2(horizontalDir.x, horizontalDir.z) * Mathf.Rad2Deg;

        if (!initialized)
        {
            smoothedYaw = targetYaw;
            initialized = true;
        }
        else
        {
            smoothedYaw = Mathf.SmoothDampAngle(smoothedYaw, targetYaw, ref yawVelocity, orbitSmoothTime);
        }

        Vector3 smoothedHorizontalDir = Quaternion.Euler(0f, smoothedYaw, 0f) * Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, smoothedHorizontalDir).normalized;

        float pitchRad = pitchAngle * Mathf.Deg2Rad;
        Vector3 forwardDir = (Mathf.Cos(pitchRad) * smoothedHorizontalDir - Mathf.Sin(pitchRad) * Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, forwardDir).normalized;

        // 2. Безопасные углы по FOV
        float tanH = Mathf.Tan(GetHalfFov(horizontal: true) * horizontalSafeFraction * Mathf.Deg2Rad);
        float tanV = Mathf.Tan(GetHalfFov(horizontal: false) * verticalSafeFraction * Mathf.Deg2Rad);

        // 3. Аналитически (через несколько итераций) находим МИНИМАЛЬНУЮ
        //    дистанцию, при которой и игрок, и враг гарантированно в безопасной зоне
        float requiredDistance = SolveRequiredBackDistance(playerPos, enemyPos, smoothedHorizontalDir, right, forwardDir, tanH, tanV);
        requiredDistance = Mathf.Clamp(requiredDistance, minBackDistance, maxBackDistance);

        // 4. Отдаление — мгновенно (гарантия кадра важнее плавности).
        //    Приближение — плавно.
        if (requiredDistance > smoothedDistance)
        {
            smoothedDistance = requiredDistance;
            distanceVelocity = 0f;
        }
        else
        {
            smoothedDistance = Mathf.SmoothDamp(smoothedDistance, requiredDistance, ref distanceVelocity, distanceShrinkSmoothTime);
        }

        // Финальная страховка: что бы ни случилось со сглаживанием, никогда
        // не опускаемся ниже реально необходимой дистанции.
        smoothedDistance = Mathf.Max(smoothedDistance, requiredDistance);
        smoothedDistance = Mathf.Clamp(smoothedDistance, minBackDistance, maxBackDistance);

        Vector3 cameraPos = playerPos - smoothedHorizontalDir * smoothedDistance + Vector3.up * heightOffset;

        transform.position = cameraPos;
        transform.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
    }

    private float SolveRequiredBackDistance(
        Vector3 playerPos, Vector3 enemyPos,
        Vector3 horizontalDir, Vector3 right, Vector3 forwardDir,
        float tanH, float tanV)
    {
        // ВАЖНО: стартуем всегда с минимума, а не с прошлого smoothedDistance.
        // Решатель умеет только УВЕЛИЧИВать bd (когда ratio > 1) и не умеет
        // уменьшать его в рамках одного решения — если бы стартовали с
        // прошлого (потенциально случайно раздутого одним "плохим" кадром)
        // значения, оно могло бы навсегда застрять завышенным: цикл сразу
        // видит ratio <= 1 и возвращает ту же раздутую цифру, даже если
        // реальный минимум сильно меньше.
        float bd = minBackDistance;

        for (int i = 0; i < solverIterations; i++)
        {
            Vector3 candidatePos = playerPos - horizontalDir * bd + Vector3.up * heightOffset;

            float ratio = 1f;
            ratio = Mathf.Max(ratio, GetContainmentRatio(playerPos, candidatePos, right, forwardDir, tanH, tanV));
            ratio = Mathf.Max(ratio, GetContainmentRatio(enemyPos, candidatePos, right, forwardDir, tanH, tanV));

            if (ratio <= 1.001f)
                break;

            bd *= ratio;
        }

        return bd;
    }

    // Возвращает во сколько раз точка "вылезает" за пределы безопасной зоны
    // кадра (1 = ровно на границе, >1 = за пределами, <1 = с запасом внутри).
    private float GetContainmentRatio(
        Vector3 point, Vector3 cameraPos,
        Vector3 right, Vector3 forwardDir,
        float tanH, float tanV)
    {
        Vector3 v = point - cameraPos;
        float forward = Vector3.Dot(v, forwardDir);

        if (forward <= 0.05f)
            forward = 0.05f; // точка практически за камерой — считаем как предельно близкую, дистанцию нужно увеличивать

        Vector3 up = Vector3.Cross(right, forwardDir).normalized;
        float lateralH = Vector3.Dot(v, right);
        float lateralV = Vector3.Dot(v, up);

        float ratioH = Mathf.Abs(lateralH) / (forward * tanH);
        float ratioV = Mathf.Abs(lateralV) / (forward * tanV);

        return Mathf.Max(ratioH, ratioV);
    }

    private float GetHalfFov(bool horizontal)
    {
        float verticalFov = lensCamera != null ? lensCamera.fieldOfView : 60f;

        if (!horizontal)
            return verticalFov * 0.5f;

        float aspect = lensCamera != null ? lensCamera.aspect : 16f / 9f;
        float vfRad = verticalFov * Mathf.Deg2Rad;
        float hfRad = 2f * Mathf.Atan(Mathf.Tan(vfRad * 0.5f) * aspect);
        return hfRad * Mathf.Rad2Deg * 0.5f;
    }
}