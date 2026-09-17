using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public enum MovementMode
    {
        Free,
        Strafe
    }

    [Header("Настройки ввода")]
    [SerializeField] private InputSettings input;

    [Header("Режим управления")]
    [SerializeField] private MovementMode currentMode = MovementMode.Free;

    [Header("Настройки ходьбы")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Tooltip("Скорость поворота персонажа в градусах в секунду")]
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Настройки разворота относительно камеры")]
    [Tooltip(
        "До этого угла персонаж старается НЕ проходить положение лицом к камере. " +
        "Например 130 означает, что после ±130° уже разрешён разворот через лицо."
    )]
    [Range(90f, 170f)]
    [SerializeField] private float frontTurnStartAngle = 130f;

    [Tooltip(
        "Угол около положения лицом к камере, который считается непосредственно целью. " +
        "Например 10 означает ±10°."
    )]
    [Range(1f, 30f)]
    [SerializeField] private float frontTargetThreshold = 10f;

    [Tooltip(
        "Насколько сильным должен быть разворот, чтобы вообще включался обход через спину."
    )]
    [Range(30f, 180f)]
    [SerializeField] private float minimumTurnForAvoidance = 90f;

    [Header("Настройки прыжка")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -19.62f;

    [Header("Проверка земли")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Ссылка на камеру")]
    [SerializeField] private Transform cameraTransform;

    [Header("Lock-on / цель")]
    [SerializeField] private Transform lockOnTarget;

    [Header("Респавн")]
    [SerializeField] private Vector3 respawnPoint = new Vector3(0f, 5f, 0f);

    private CharacterController controller;

    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    public MovementMode CurrentMode => currentMode;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (input == null)
            Debug.LogError(
                $"{name}: не назначен InputSettings ассет в PlayerMovement!",
                this
            );
    }

    private void Update()
    {
        HandleModeToggle();
        GroundCheck();
        HandleMovement();
        HandleJumpAndGravity();
        HandleRespawn();
    }

    // -------------------------------------------------------------------------
    // Переключение режима
    // -------------------------------------------------------------------------

    private void HandleModeToggle()
    {
        if (input == null)
            return;

        if (Input.GetKeyDown(input.toggleMovementMode))
        {
            SetMode(
                currentMode == MovementMode.Free
                    ? MovementMode.Strafe
                    : MovementMode.Free
            );
        }
    }

    public void SetMode(MovementMode newMode)
    {
        currentMode = newMode;

        Debug.Log($"Movement mode: {currentMode}");
    }

    // Вызывается системой лок-она (LockOnController) при захвате/потере цели.
    // Передавай сюда AimPoint цели при локе и null при снятии лока.
    public void SetLockOnTarget(Transform target)
    {
        lockOnTarget = target;
    }

    // -------------------------------------------------------------------------
    // Движение
    // -------------------------------------------------------------------------

    private void HandleMovement()
    {
        if (input == null)
            return;

        float horizontal = GetAxis(
            input.moveLeft,
            input.moveRight
        );

        float vertical = GetAxis(
            input.moveBackward,
            input.moveForward
        );

        Vector3 inputDir = Vector3.ClampMagnitude(
            new Vector3(horizontal, 0f, vertical),
            1f
        );

        currentSpeed = Input.GetKey(input.sprint)
            ? sprintSpeed
            : walkSpeed;

        if (inputDir.sqrMagnitude < 0.01f)
            return;

        switch (currentMode)
        {
            case MovementMode.Free:
                HandleFreeMovement(inputDir);
                break;

            case MovementMode.Strafe:
                HandleStrafeMovement(horizontal, vertical);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Free movement
    // -------------------------------------------------------------------------

    private void HandleFreeMovement(Vector3 inputDir)
    {
        float cameraYaw = cameraTransform != null
            ? cameraTransform.eulerAngles.y
            : transform.eulerAngles.y;

        float inputAngle =
            Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;

        float targetAngle = cameraYaw + inputAngle;

        float currentAngle = transform.eulerAngles.y;

        float turnDelta = ResolveTurnDelta(
            currentAngle,
            targetAngle,
            cameraYaw
        );

        RotateByDelta(turnDelta);

        Vector3 moveDir =
            Quaternion.Euler(0f, targetAngle, 0f) *
            Vector3.forward;

        controller.Move(
            moveDir.normalized *
            currentSpeed *
            Time.deltaTime
        );
    }

    // -------------------------------------------------------------------------
    // Strafe movement
    // -------------------------------------------------------------------------

    private void HandleStrafeMovement(
        float horizontal,
        float vertical
    )
    {
        float targetYaw;

        if (lockOnTarget != null)
        {
            targetYaw = GetYawTowards(
                lockOnTarget.position
            );
        }
        else
        {
            targetYaw = cameraTransform != null
                ? cameraTransform.eulerAngles.y
                : transform.eulerAngles.y;
        }

        float cameraYaw = cameraTransform != null
            ? cameraTransform.eulerAngles.y
            : targetYaw;

        float currentAngle = transform.eulerAngles.y;

        float turnDelta = ResolveTurnDelta(
            currentAngle,
            targetYaw,
            cameraYaw
        );

        RotateByDelta(turnDelta);

        Vector3 moveDir =
            transform.right * horizontal +
            transform.forward * vertical;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            controller.Move(
                moveDir.normalized *
                currentSpeed *
                Time.deltaTime
            );
        }
    }

    // -------------------------------------------------------------------------
    // Поворот
    // -------------------------------------------------------------------------

    private void RotateByDelta(float delta)
    {
        float maxStep =
            rotationSpeed * Time.deltaTime;

        float step =
            Mathf.Clamp(delta, -maxStep, maxStep);

        transform.Rotate(
            0f,
            step,
            0f,
            Space.Self
        );
    }

    private float ResolveTurnDelta(
        float current,
        float target,
        float cameraYaw
    )
    {
        float frontAngle = cameraYaw + 180f;

        float delta = Mathf.DeltaAngle(
            current,
            target
        );

        if (Mathf.Abs(delta) < minimumTurnForAvoidance)
            return delta;

        float targetToFront = Mathf.Abs(
            Mathf.DeltaAngle(
                target,
                frontAngle
            )
        );

        if (targetToFront <= frontTargetThreshold)
            return delta;

        float currentToFront = Mathf.Abs(
            Mathf.DeltaAngle(
                current,
                frontAngle
            )
        );

        float frontBoundary =
            180f - frontTurnStartAngle;

        if (currentToFront <= frontBoundary)
            return delta;

        float currentToFrontSigned = Mathf.DeltaAngle(
            current,
            frontAngle
        );

        bool sameDirection =
            Mathf.Sign(currentToFrontSigned) ==
            Mathf.Sign(delta);

        bool frontInsideSweep =
            Mathf.Abs(currentToFrontSigned) <
            Mathf.Abs(delta);

        if (sameDirection && frontInsideSweep)
        {
            return -Mathf.Sign(delta) *
                   (360f - Mathf.Abs(delta));
        }

        return delta;
    }

    // -------------------------------------------------------------------------
    // Получение направления на цель
    // -------------------------------------------------------------------------

    private float GetYawTowards(Vector3 worldPosition)
    {
        Vector3 direction =
            worldPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return transform.eulerAngles.y;

        return Quaternion
            .LookRotation(direction)
            .eulerAngles.y;
    }

    // -------------------------------------------------------------------------
    // Прыжок / гравитация
    // -------------------------------------------------------------------------

    private void HandleJumpAndGravity()
    {
        if (input != null &&
            Input.GetKeyDown(input.jump) &&
            isGrounded)
        {
            velocity.y =
                Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
        }

        velocity.y +=
            gravity * Time.deltaTime;

        controller.Move(
            velocity * Time.deltaTime
        );
    }

    // -------------------------------------------------------------------------
    // Проверка земли
    // -------------------------------------------------------------------------

    private void GroundCheck()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundDistance,
                groundMask
            );
        }
        else
        {
            isGrounded = controller.isGrounded;
        }

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    // -------------------------------------------------------------------------
    // Респавн
    // -------------------------------------------------------------------------

    private void HandleRespawn()
    {
        if (input == null)
            return;

        if (Input.GetKeyDown(input.respawn))
        {
            controller.enabled = false;

            transform.position = respawnPoint;

            velocity = Vector3.zero;

            controller.enabled = true;
        }
    }

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------

    private float GetAxis(
        KeyCode negative,
        KeyCode positive
    )
    {
        float value = 0f;

        if (Input.GetKey(positive))
            value += 1f;

        if (Input.GetKey(negative))
            value -= 1f;

        return value;
    }

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundDistance
            );
        }
    }
}