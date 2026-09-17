using UnityEngine;

// Вешается на пустышку cameraTarget (Follow/Look At в CinemachineCamera).
// Скрипт НЕ двигает и не вращает саму камеру — этим занимается Cinemachine
// (Orbital Follow / Third Person Follow), опираясь на трансформ этого объекта.
//
// ВАЖНО: применение поворота вынесено в LateUpdate(). Если cameraTarget —
// дочерний объект персонажа, а тело персонажа само поворачивается в
// PlayerMovement.Update() (Free-режим), то порядок выполнения Update()
// между разными скриптами не гарантирован Unity. Если тело провернётся
// ПОСЛЕ того, как мы уже выставили мировой поворот cameraTarget, его
// эффективный мировой поворот "уедет" вслед за родителем — это и даёт
// тряску камеры именно во время движения. LateUpdate() гарантированно
// выполняется у всех объектов уже после всех Update(), поэтому мы всегда
// применяем поворот после того, как тело окончательно довернулось за кадр.
// Cinemachine Brain читает трансформ ещё позже (у неё свой поздний
// DefaultExecutionOrder), так что получит уже финальное значение.
public class CameraTargetController : MonoBehaviour
{
    [Header("Настройки ввода")]
    [SerializeField] private InputSettings input;

    [Header("Ограничения по вертикали (питч)")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Плавность (опционально)")]
    [Tooltip("0 = мгновенный отклик мыши без сглаживания")]
    [SerializeField] private float rotationSmoothTime = 0f;

    private float yaw;
    private float pitch;
    private float yawVelocity;
    private float pitchVelocity;

    private float pendingMouseX;
    private float pendingMouseY;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = NormalizeAngle(angles.x);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (input == null)
            Debug.LogError($"{name}: не назначен InputSettings ассет в CameraTargetController!", this);
    }

    private void Update()
    {
        if (input == null) return;

        // Считываем ввод мыши в Update (так он не теряется и не дублируется
        // между кадрами), а применяем результат позже, в LateUpdate.
        pendingMouseX += Input.GetAxis("Mouse X") * input.mouseSensitivityX;

        float rawY = Input.GetAxis("Mouse Y") * input.mouseSensitivityY;
        pendingMouseY += input.invertY ? rawY : -rawY;
    }

    private void LateUpdate()
    {
        if (input == null) return;

        float targetYaw = yaw + pendingMouseX;
        float targetPitch = Mathf.Clamp(pitch + pendingMouseY, minPitch, maxPitch);

        pendingMouseX = 0f;
        pendingMouseY = 0f;

        if (rotationSmoothTime > 0f)
        {
            yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);
        }
        else
        {
            yaw = targetYaw;
            pitch = targetPitch;
        }

        // Мировой поворот — применяется здесь, уже после того как тело
        // персонажа окончательно довернулось за этот кадр.
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private float NormalizeAngle(float rawAngle)
    {
        return rawAngle > 180f ? rawAngle - 360f : rawAngle;
    }
}