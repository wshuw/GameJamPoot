using UnityEngine;
using Unity.Cinemachine;

// Мост между игровой логикой лок-она (LockOnController) и Cinemachine.
// Сам НЕ управляет позицией/поворотом камеры — просто:
//   1) поднимает/опускает Priority боевой CinemachineCamera (переход между
//      камерами делает Cinemachine Brain сама, см. Default Blend в её инспекторе);
//   2) наполняет CinemachineTargetGroup игроком и текущей целью с весами —
//      дальше "Group Framing" на боевой камере сам считает кадрирование
//      и адаптивную дистанцию (аналог Arm Length из требований).
[RequireComponent(typeof(LockOnController))]
public class CombatCameraManager : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("CinemachineCamera боевого режима. LookAt у неё должен указывать на combatTargetGroup")]
    [SerializeField] private CinemachineCamera combatCamera;
    [SerializeField] private CinemachineTargetGroup combatTargetGroup;

    [Header("Веса в группе (влияют на то, куда смещён центр кадра)")]
    [Tooltip("Вес игрока в группе. Меньше веса у игрока = сильнее кадр 'утянут' к врагу")]
    [SerializeField] private float playerWeight = 1f;
    [SerializeField] private float playerRadius = 1f;
    [SerializeField] private float enemyWeight = 1f;

    [Header("Приоритеты камер")]
    [Tooltip("Приоритет боевой камеры, когда лок активен")]
    [SerializeField] private int combatPriority = 20;
    [Tooltip("Приоритет боевой камеры в выключенном состоянии. Должен быть ЗАВЕДОМО ниже приоритета свободной камеры (не просто равен), иначе при равенстве Cinemachine может оставить боевую камеру активной по правилу 'последняя изменённая побеждает при ничьей'")]
    [SerializeField] private int combatIdlePriority = -10;

    [Header("Блендинг")]
    [Tooltip("Сколько секунд держать группу заполненной ПОСЛЕ сброса лока, прежде чем очистить. Должно быть чуть больше, чем Default Blend Time в CinemachineBrain — иначе камера читает пустую группу ещё во время перехода и проваливается к transform.position самой группы (обычно это мировой ноль)")]
    [SerializeField] private float clearGroupDelay = 0.4f;

    private LockOnController lockOnController;
    private bool wasLocked;
    private Coroutine clearGroupRoutine;

    private void Awake()
    {
        lockOnController = GetComponent<LockOnController>();

        if (combatCamera != null)
            combatCamera.Priority = combatIdlePriority;
    }

    // LateUpdate — чтобы к этому моменту PlayerMovement и LockOnController
    // уже полностью отработали свой Update за этот кадр.
    private void LateUpdate()
    {
        bool isLocked = lockOnController.IsLocked;

        if (isLocked && !wasLocked)
            EnterCombatCamera();
        else if (!isLocked && wasLocked)
            ExitCombatCamera();

        if (isLocked)
            UpdateTargetGroup();

        wasLocked = isLocked;
    }

    private void EnterCombatCamera()
    {
        // Если только что запустилась отложенная очистка от предыдущего выхода
        // из лока — отменяем её, группа снова актуальна и сразу перезапишется.
        if (clearGroupRoutine != null)
        {
            StopCoroutine(clearGroupRoutine);
            clearGroupRoutine = null;
        }

        if (combatCamera != null)
            combatCamera.Priority = combatPriority;
    }

    private void ExitCombatCamera()
    {
        if (combatCamera != null)
            combatCamera.Priority = combatIdlePriority;

        // НЕ чистим группу мгновенно — Cinemachine Brain ещё несколько кадров
        // блендит FROM combatCamera, и если группа в этот момент пустая,
        // она откатывается к transform.position самого объекта-группы
        // (обычно возле мирового нуля) — камера на миг проваливается туда.
        if (combatTargetGroup != null)
            clearGroupRoutine = StartCoroutine(ClearGroupAfterBlend());
    }

    private System.Collections.IEnumerator ClearGroupAfterBlend()
    {
        yield return new WaitForSeconds(clearGroupDelay);

        if (combatTargetGroup != null)
            combatTargetGroup.Targets.Clear();

        clearGroupRoutine = null;
    }

    //private void UpdateTargetGroup()
    //{
    //    if (combatTargetGroup == null || lockOnController.CurrentTarget == null)
    //        return;

    //    Transform enemyPoint = lockOnController.CurrentTarget.AimPoint;
    //    float enemyRadius = lockOnController.CurrentTarget.TargetRadius;

    //    var targets = combatTargetGroup.Targets;
    //    targets.Clear();

    //    targets.Add(new CinemachineTargetGroup.Target
    //    {
    //        Object = transform,
    //        Weight = playerWeight,
    //        Radius = playerRadius
    //    });

    //    targets.Add(new CinemachineTargetGroup.Target
    //    {
    //        Object = enemyPoint,
    //        Weight = enemyWeight,
    //        Radius = enemyRadius
    //    });
    //}

    //гемини круче придумал
    private void UpdateTargetGroup()
    {
        if (combatTargetGroup == null || lockOnController.CurrentTarget == null) return;

        var targets = combatTargetGroup.Targets;

        // Если список пуст, создаем два пустых слота
        if (targets.Count < 2)
        {
            targets.Clear();
            targets.Add(new CinemachineTargetGroup.Target());
            targets.Add(new CinemachineTargetGroup.Target());
        }

        // Просто обновляем значения существующих элементов
        var playerTarget = targets[0];
        playerTarget.Object = transform;
        playerTarget.Weight = playerWeight;
        playerTarget.Radius = playerRadius;
        targets[0] = playerTarget;

        var enemyTarget = targets[1];
        enemyTarget.Object = lockOnController.CurrentTarget.AimPoint;
        enemyTarget.Weight = enemyWeight;
        enemyTarget.Radius = lockOnController.CurrentTarget.TargetRadius;
        targets[1] = enemyTarget;

        // Поворачиваем саму группу так, чтобы её спина всегда смотрела на игрока
        Vector3 directionToEnemy = lockOnController.CurrentTarget.AimPoint.position - transform.position;
        directionToEnemy.y = 0; // Игнорируем высоту, чтобы камера не ныряла в пол

        if (directionToEnemy != Vector3.zero)
        {
            combatTargetGroup.transform.rotation = Quaternion.LookRotation(directionToEnemy);
        }

    }



}
