//ПОКА НЕ ИСПОЛЬЗУЕТСЯ И НЕ ПЛАНИРУЕТСЯ, НЕЙРОСЛОП ГОВНА










//using UnityEngine;

//// Скрипт вешается на объект камеры.
//// Камера держится за спиной персонажа и вращается вокруг него мышью (орбита).
//public class ThirdPersonCamera : MonoBehaviour
//{
//    [Header("Цель слежения")]
//    [SerializeField] private Transform target; // сюда перетащить персонажа (капсулу)

//    [Header("Настройки позиции")]
//    [Tooltip("Смещение камеры относительно персонажа: X — вбок, Y — вверх, Z — назад (отрицательное значение = за спиной)")]
//    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);
//    [SerializeField] private float followSmoothTime = 0.15f;

//    [Header("Настройки вращения (мышь)")]
//    [SerializeField] private float mouseSensitivity = 3f;
//    [SerializeField] private float minPitch = -30f;
//    [SerializeField] private float maxPitch = 60f;

//    private float yaw;
//    private float pitch = 15f;
//    private Vector3 currentVelocity;

//    private void Start()
//    {
//        // Прячем и блокируем курсор — стандарт для управления камерой мышью
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;

//        if (target != null)
//            yaw = target.eulerAngles.y;
//    }

//    private void LateUpdate()
//    {
//        if (target == null) return;

//        HandleRotationInput();

//        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
//        Vector3 desiredPosition = target.position + rotation * offset;

//        transform.position = Vector3.SmoothDamp(
//            transform.position,
//            desiredPosition,
//            ref currentVelocity,
//            followSmoothTime);

//        transform.LookAt(target.position + Vector3.up * 1.5f);
//    }

//    private void HandleRotationInput()
//    {
//        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
//        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
//        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
//    }
//}




































////using UnityEngine;

////// Soulslike-style third person camera.
////// Камера вращается вокруг персонажа, автоматически избегает стен
////// и может смещаться в сторону от препятствий.
////public class ThirdPersonCamera : MonoBehaviour
////{
////    [Header("Цель слежения")]
////    [SerializeField] private Transform target;

////    [Header("Позиция камеры")]
////    [Tooltip("Высота точки, вокруг которой вращается камера.")]
////    [SerializeField] private float targetHeight = 1.5f;

////    [Tooltip("Расстояние камеры от персонажа.")]
////    [SerializeField] private float distance = 4f;

////    [Tooltip("Постоянное боковое смещение камеры. Положительное = вправо.")]
////    [SerializeField] private float sideOffset = 0.6f;

////    [Header("Смещение от стен")]
////    [Tooltip("Радиус камеры при проверке столкновения со стенами.")]
////    [SerializeField] private float cameraRadius = 0.25f;

////    [Tooltip("Минимальное расстояние камеры от стены.")]
////    [SerializeField] private float wallPadding = 0.15f;

////    [Tooltip("Слой стен/окружения.")]
////    [SerializeField] private LayerMask cameraCollisionMask;

////    [Tooltip("Насколько быстро камера реагирует на стены.")]
////    [SerializeField] private float collisionSmoothTime = 0.05f;

////    [Header("Автоматический боковой сдвиг")]
////    [Tooltip("Насколько камера может сместиться в сторону от стены.")]
////    [SerializeField] private float maxSideShift = 0.8f;

////    [Tooltip("Радиус проверки пространства слева/справа.")]
////    [SerializeField] private float sideCheckRadius = 0.3f;

////    [Tooltip("Сила автоматического ухода от стены.")]
////    [SerializeField] private float sideShiftSpeed = 5f;

////    [Header("Поворот мышью")]
////    [SerializeField] private float mouseSensitivity = 3f;

////    [SerializeField] private float minPitch = -30f;
////    [SerializeField] private float maxPitch = 60f;

////    [Header("Следование")]
////    [SerializeField] private float followSmoothTime = 0.08f;

////    private float yaw;
////    private float pitch = 15f;

////    private Vector3 followVelocity;
////    private float collisionDistance;
////    private float collisionVelocity;

////    private float currentSideShift;
////    private float sideShiftVelocity;

////    private void Start()
////    {
////        Cursor.lockState = CursorLockMode.Locked;
////        Cursor.visible = false;

////        if (target != null)
////        {
////            yaw = target.eulerAngles.y;
////        }

////        collisionDistance = distance;
////    }

////    private void LateUpdate()
////    {
////        if (target == null)
////            return;

////        HandleRotationInput();

////        Vector3 lookPoint =
////            target.position +
////            Vector3.up * targetHeight;

////        Quaternion rotation =
////            Quaternion.Euler(pitch, yaw, 0f);

////        // -------------------------------------------------------------
////        // 1. Определяем направление камеры
////        // -------------------------------------------------------------

////        Vector3 backward =
////            -(rotation * Vector3.forward);

////        Vector3 right =
////            rotation * Vector3.right;

////        // -------------------------------------------------------------
////        // 2. Автоматически ищем свободное место слева/справа
////        // -------------------------------------------------------------

////        float desiredSideShift =
////            CalculateSideShift(
////                lookPoint,
////                right
////            );

////        currentSideShift = Mathf.SmoothDamp(
////            currentSideShift,
////            desiredSideShift,
////            ref sideShiftVelocity,
////            1f / sideShiftSpeed
////        );

////        // Ограничиваем автоматический сдвиг
////        currentSideShift = Mathf.Clamp(
////            currentSideShift,
////            -maxSideShift,
////            maxSideShift
////        );

////        // -------------------------------------------------------------
////        // 3. Рассчитываем желаемую позицию камеры
////        // -------------------------------------------------------------

////        Vector3 desiredCameraPosition =
////            lookPoint
////            + right * (sideOffset + currentSideShift)
////            + backward * distance;

////        // -------------------------------------------------------------
////        // 4. Проверяем столкновение камеры со стенами
////        // -------------------------------------------------------------

////        float safeDistance =
////            CalculateSafeDistance(
////                lookPoint,
////                desiredCameraPosition
////            );

////        collisionDistance = Mathf.SmoothDamp(
////            collisionDistance,
////            safeDistance,
////            ref collisionVelocity,
////            collisionSmoothTime
////        );

////        // -------------------------------------------------------------
////        // 5. Финальная позиция
////        // -------------------------------------------------------------

////        Vector3 finalPosition =
////            lookPoint
////            + right * (sideOffset + currentSideShift)
////            + backward * collisionDistance;

////        transform.position = Vector3.SmoothDamp(
////            transform.position,
////            finalPosition,
////            ref followVelocity,
////            followSmoothTime
////        );

////        // -------------------------------------------------------------
////        // 6. Смотрим на персонажа
////        // -------------------------------------------------------------

////        transform.LookAt(lookPoint);
////    }

////    // =================================================================
////    // МЫШЬ
////    // =================================================================

////    private void HandleRotationInput()
////    {
////        yaw +=
////            Input.GetAxis("Mouse X") *
////            mouseSensitivity;

////        pitch -=
////            Input.GetAxis("Mouse Y") *
////            mouseSensitivity;

////        pitch = Mathf.Clamp(
////            pitch,
////            minPitch,
////            maxPitch
////        );
////    }

////    // =================================================================
////    // ПРОВЕРКА СТЕНЫ ПОЗАДИ КАМЕРЫ
////    // =================================================================

////    private float CalculateSafeDistance(
////        Vector3 lookPoint,
////        Vector3 desiredCameraPosition
////    )
////    {
////        Vector3 direction =
////            desiredCameraPosition -
////            lookPoint;

////        float desiredDistance =
////            direction.magnitude;

////        if (desiredDistance <= 0.01f)
////            return 0.1f;

////        direction.Normalize();

////        // SphereCast вместо Raycast:
////        // камера имеет физический объём.
////        if (Physics.SphereCast(
////            lookPoint,
////            cameraRadius,
////            direction,
////            out RaycastHit hit,
////            desiredDistance,
////            cameraCollisionMask,
////            QueryTriggerInteraction.Ignore))
////        {
////            float safeDistance =
////                hit.distance -
////                cameraRadius -
////                wallPadding;

////            return Mathf.Clamp(
////                safeDistance,
////                0.1f,
////                distance
////            );
////        }

////        return distance;
////    }

////    // =================================================================
////    // АВТОМАТИЧЕСКИЙ СДВИГ ОТ СТЕН
////    // =================================================================

////    private float CalculateSideShift(
////        Vector3 lookPoint,
////        Vector3 right
////    )
////    {
////        bool wallRight = Physics.SphereCast(
////            lookPoint,
////            sideCheckRadius,
////            right,
////            out RaycastHit rightHit,
////            maxSideShift,
////            cameraCollisionMask,
////            QueryTriggerInteraction.Ignore
////        );

////        bool wallLeft = Physics.SphereCast(
////            lookPoint,
////            sideCheckRadius,
////            -right,
////            out RaycastHit leftHit,
////            maxSideShift,
////            cameraCollisionMask,
////            QueryTriggerInteraction.Ignore
////        );

////        // -------------------------------------------------------------
////        // Стена справа -> камера уходит влево
////        // -------------------------------------------------------------

////        if (wallRight && !wallLeft)
////        {
////            return -CalculateShiftAmount(
////                rightHit.distance
////            );
////        }

////        // -------------------------------------------------------------
////        // Стена слева -> камера уходит вправо
////        // -------------------------------------------------------------

////        if (wallLeft && !wallRight)
////        {
////            return CalculateShiftAmount(
////                leftHit.distance
////            );
////        }

////        // -------------------------------------------------------------
////        // Стены с двух сторон
////        // -------------------------------------------------------------

////        if (wallLeft && wallRight)
////        {
////            // Смотрим, с какой стороны больше места.
////            if (leftHit.distance > rightHit.distance)
////            {
////                return CalculateShiftAmount(
////                    leftHit.distance
////                );
////            }
////            else
////            {
////                return -CalculateShiftAmount(
////                    rightHit.distance
////                );
////            }
////        }

////        return 0f;
////    }

////    private float CalculateShiftAmount(
////        float wallDistance
////    )
////    {
////        // Чем ближе стена,
////        // тем сильнее камера уходит в противоположную сторону.

////        float normalized =
////            1f -
////            Mathf.Clamp01(
////                wallDistance / maxSideShift
////            );

////        return normalized * maxSideShift;
////    }

////    // =================================================================
////    // DEBUG
////    // =================================================================

////    private void OnDrawGizmosSelected()
////    {
////        if (target == null)
////            return;

////        Gizmos.color = Color.cyan;

////        Vector3 lookPoint =
////            target.position +
////            Vector3.up * targetHeight;

////        Quaternion rotation =
////            Quaternion.Euler(
////                pitch,
////                yaw,
////                0f
////            );

////        Vector3 backward =
////            -(rotation * Vector3.forward);

////        Vector3 desiredPosition =
////            lookPoint +
////            backward * distance;

////        Gizmos.DrawLine(
////            lookPoint,
////            desiredPosition
////        );

////        Gizmos.DrawWireSphere(
////            desiredPosition,
////            cameraRadius
////        );
////    }
////}



////using UnityEngine;

////// Камера от третьего лица в стиле Soulslike.
////// Камера вращается вокруг персонажа мышью,
////// автоматически отодвигается от стен и не проходит сквозь них.
////public class ThirdPersonCamera : MonoBehaviour
////{
////    [Header("========== ЦЕЛЬ СЛЕЖЕНИЯ ==========")]

////    [InspectorName("Персонаж")]
////    [Tooltip("Перетащи сюда объект игрока, за которым должна следовать камера.")]
////    [SerializeField] private Transform target;


////    [Header("========== ПОЛОЖЕНИЕ КАМЕРЫ ==========")]

////    [InspectorName("Высота точки слежения")]
////    [Tooltip(
////        "Высота точки на персонаже, вокруг которой вращается камера.\n" +
////        "1.5 = примерно уровень головы/верхней части тела."
////    )]
////    [SerializeField] private float targetHeight = 1.5f;

////    [InspectorName("Расстояние от персонажа")]
////    [Tooltip(
////        "Расстояние камеры от персонажа.\n" +
////        "Чем больше значение, тем дальше камера находится от игрока."
////    )]
////    [SerializeField] private float distance = 4f;

////    [InspectorName("Постоянное смещение вбок")]
////    [Tooltip(
////        "Постоянное смещение камеры влево или вправо.\n\n" +
////        "0 = камера строго по центру.\n" +
////        "Положительное значение = камера правее персонажа.\n" +
////        "Отрицательное значение = камера левее персонажа."
////    )]
////    [SerializeField] private float sideOffset = 0.6f;


////    [Header("========== ЗАЩИТА ОТ СТЕН ==========")]

////    [InspectorName("Радиус камеры")]
////    [Tooltip(
////        "Условный радиус камеры при проверке столкновения со стенами.\n\n" +
////        "Если камера немного залезает в стену — увеличь это значение."
////    )]
////    [SerializeField] private float cameraRadius = 0.25f;

////    [InspectorName("Отступ от стены")]
////    [Tooltip(
////        "Минимальное расстояние, которое камера оставляет до стены.\n\n" +
////        "Чем больше значение, тем дальше камера останавливается от стены."
////    )]
////    [SerializeField] private float wallPadding = 0.15f;

////    [InspectorName("Слои столкновений")]
////    [Tooltip(
////        "Какие слои считаются препятствиями для камеры.\n\n" +
////        "Обычно здесь должен быть слой стен/окружения."
////    )]
////    [SerializeField] private LayerMask cameraCollisionMask;


////    [Header("========== АВТОМАТИЧЕСКИЙ УХОД ОТ СТЕН ==========")]

////    [InspectorName("Максимальный уход в сторону")]
////    [Tooltip(
////        "Максимальное расстояние, на которое камера может автоматически " +
////        "сместиться влево или вправо, чтобы уйти от стены.\n\n" +
////        "0 = автоматического ухода в сторону не будет."
////    )]
////    [SerializeField] private float maxSideShift = 0.8f;

////    [InspectorName("Радиус боковой проверки")]
////    [Tooltip(
////        "Размер области проверки слева и справа от персонажа.\n\n" +
////        "Если рядом находится стена, камера начинает смещаться от неё."
////    )]
////    [SerializeField] private float sideCheckRadius = 0.3f;

////    [InspectorName("Скорость ухода от стены")]
////    [Tooltip(
////        "Скорость, с которой камера перемещается в сторону от стены.\n\n" +
////        "Больше = быстрее реагирует.\n" +
////        "Меньше = плавнее реагирует."
////    )]
////    [SerializeField] private float sideShiftSpeed = 5f;


////    [Header("========== УПРАВЛЕНИЕ МЫШЬЮ ==========")]

////    [InspectorName("Чувствительность мыши")]
////    [Tooltip("Чувствительность вращения камеры мышью.")]
////    [SerializeField] private float mouseSensitivity = 3f;

////    [InspectorName("Минимальный наклон")]
////    [Tooltip("Насколько сильно камера может смотреть вниз.")]
////    [SerializeField] private float minPitch = -30f;

////    [InspectorName("Максимальный наклон")]
////    [Tooltip("Насколько сильно камера может смотреть вверх.")]
////    [SerializeField] private float maxPitch = 60f;


////    [Header("========== ПЛАВНОСТЬ КАМЕРЫ ==========")]

////    [InspectorName("Плавность следования")]
////    [Tooltip(
////        "Насколько плавно камера следует за персонажем.\n\n" +
////        "Меньше = быстрее и резче.\n" +
////        "Больше = плавнее и 'тяжелее'."
////    )]
////    [SerializeField] private float followSmoothTime = 0.08f;

////    [InspectorName("Плавность столкновения со стеной")]
////    [Tooltip(
////        "Насколько плавно камера приближается к стене или отдаляется от неё.\n\n" +
////        "Меньше = быстрее реакция.\n" +
////        "Больше = плавнее реакция."
////    )]
////    [SerializeField] private float collisionSmoothTime = 0.05f;


////    // ========================================================================
////    // ВНУТРЕННИЕ ПЕРЕМЕННЫЕ
////    // ========================================================================

////    private float yaw;
////    private float pitch = 15f;

////    private Vector3 followVelocity;

////    private float collisionDistance;
////    private float collisionVelocity;

////    private float currentSideShift;
////    private float sideShiftVelocity;


////    // ========================================================================
////    // НАСТРОЙКА
////    // ========================================================================

////    private void Start()
////    {
////        Cursor.lockState = CursorLockMode.Locked;
////        Cursor.visible = false;

////        if (target != null)
////        {
////            yaw = target.eulerAngles.y;
////        }

////        collisionDistance = distance;
////    }


////    // ========================================================================
////    // ОСНОВНОЙ ЦИКЛ КАМЕРЫ
////    // ========================================================================

////    private void LateUpdate()
////    {
////        if (target == null)
////            return;

////        HandleMouseInput();

////        // Точка, на которую смотрит камера.
////        Vector3 lookPoint =
////            target.position +
////            Vector3.up * targetHeight;


////        // --------------------------------------------------------------------
////        // 1. Направление камеры
////        // --------------------------------------------------------------------

////        Quaternion cameraRotation =
////            Quaternion.Euler(
////                pitch,
////                yaw,
////                0f
////            );

////        Vector3 backward =
////            -(cameraRotation * Vector3.forward);

////        Vector3 right =
////            cameraRotation * Vector3.right;


////        // --------------------------------------------------------------------
////        // 2. Проверяем стены слева и справа
////        // --------------------------------------------------------------------

////        float desiredSideShift =
////            CalculateSideShift(
////                lookPoint,
////                right
////            );


////        currentSideShift =
////            Mathf.SmoothDamp(
////                currentSideShift,
////                desiredSideShift,
////                ref sideShiftVelocity,
////                1f / Mathf.Max(sideShiftSpeed, 0.01f)
////            );


////        currentSideShift =
////            Mathf.Clamp(
////                currentSideShift,
////                -maxSideShift,
////                maxSideShift
////            );


////        // --------------------------------------------------------------------
////        // 3. Рассчитываем желаемую позицию камеры
////        // --------------------------------------------------------------------

////        Vector3 desiredCameraPosition =
////            lookPoint
////            + right * (sideOffset + currentSideShift)
////            + backward * distance;


////        // --------------------------------------------------------------------
////        // 4. Проверяем стену между персонажем и камерой
////        // --------------------------------------------------------------------

////        float safeDistance =
////            CalculateSafeDistance(
////                lookPoint,
////                desiredCameraPosition
////            );


////        collisionDistance =
////            Mathf.SmoothDamp(
////                collisionDistance,
////                safeDistance,
////                ref collisionVelocity,
////                collisionSmoothTime
////            );


////        // --------------------------------------------------------------------
////        // 5. Финальная позиция камеры
////        // --------------------------------------------------------------------

////        Vector3 finalPosition =
////            lookPoint
////            + right * (sideOffset + currentSideShift)
////            + backward * collisionDistance;


////        // --------------------------------------------------------------------
////        // 6. Плавное перемещение камеры
////        // --------------------------------------------------------------------

////        transform.position =
////            Vector3.SmoothDamp(
////                transform.position,
////                finalPosition,
////                ref followVelocity,
////                followSmoothTime
////            );


////        // --------------------------------------------------------------------
////        // 7. Камера смотрит на персонажа
////        // --------------------------------------------------------------------

////        transform.LookAt(lookPoint);
////    }


////    // ========================================================================
////    // МЫШЬ
////    // ========================================================================

////    private void HandleMouseInput()
////    {
////        yaw +=
////            Input.GetAxis("Mouse X") *
////            mouseSensitivity;

////        pitch -=
////            Input.GetAxis("Mouse Y") *
////            mouseSensitivity;

////        pitch =
////            Mathf.Clamp(
////                pitch,
////                minPitch,
////                maxPitch
////            );
////    }


////    // ========================================================================
////    // ПРОВЕРКА СТЕНЫ МЕЖДУ ПЕРСОНАЖЕМ И КАМЕРОЙ
////    // ========================================================================

////    private float CalculateSafeDistance(
////        Vector3 lookPoint,
////        Vector3 desiredCameraPosition
////    )
////    {
////        Vector3 direction =
////            desiredCameraPosition -
////            lookPoint;

////        float desiredDistance =
////            direction.magnitude;


////        if (desiredDistance <= 0.01f)
////            return 0.1f;


////        direction.Normalize();


////        // SphereCast вместо обычного Raycast.
////        // Это позволяет учитывать объём камеры.
////        if (Physics.SphereCast(
////            lookPoint,
////            cameraRadius,
////            direction,
////            out RaycastHit hit,
////            desiredDistance,
////            cameraCollisionMask,
////            QueryTriggerInteraction.Ignore
////        ))
////        {
////            float safeDistance =
////                hit.distance -
////                cameraRadius -
////                wallPadding;


////            return Mathf.Clamp(
////                safeDistance,
////                0.1f,
////                distance
////            );
////        }


////        return distance;
////    }


////    // ========================================================================
////    // ПРОВЕРКА ПРОСТРАНСТВА СЛЕВА И СПРАВА
////    // ========================================================================

////    private float CalculateSideShift(
////        Vector3 lookPoint,
////        Vector3 right
////    )
////    {
////        bool wallRight =
////            Physics.SphereCast(
////                lookPoint,
////                sideCheckRadius,
////                right,
////                out RaycastHit rightHit,
////                maxSideShift,
////                cameraCollisionMask,
////                QueryTriggerInteraction.Ignore
////            );


////        bool wallLeft =
////            Physics.SphereCast(
////                lookPoint,
////                sideCheckRadius,
////                -right,
////                out RaycastHit leftHit,
////                maxSideShift,
////                cameraCollisionMask,
////                QueryTriggerInteraction.Ignore
////            );


////        // --------------------------------------------------------------------
////        // Стена справа → камера уходит влево
////        // --------------------------------------------------------------------

////        if (wallRight && !wallLeft)
////        {
////            return -CalculateShiftAmount(
////                rightHit.distance
////            );
////        }


////        // --------------------------------------------------------------------
////        // Стена слева → камера уходит вправо
////        // --------------------------------------------------------------------

////        if (wallLeft && !wallRight)
////        {
////            return CalculateShiftAmount(
////                leftHit.distance
////            );
////        }


////        // --------------------------------------------------------------------
////        // Стены с обеих сторон
////        // --------------------------------------------------------------------

////        if (wallLeft && wallRight)
////        {
////            // Выбираем сторону, где больше свободного места.

////            if (leftHit.distance >
////                rightHit.distance)
////            {
////                return CalculateShiftAmount(
////                    leftHit.distance
////                );
////            }

////            return -CalculateShiftAmount(
////                rightHit.distance
////            );
////        }


////        // --------------------------------------------------------------------
////        // Рядом нет стен
////        // --------------------------------------------------------------------

////        return 0f;
////    }


////    // ========================================================================
////    // СИЛА УХОДА ОТ СТЕНЫ
////    // ========================================================================

////    private float CalculateShiftAmount(
////        float wallDistance
////    )
////    {
////        float normalized =
////            1f -
////            Mathf.Clamp01(
////                wallDistance /
////                Mathf.Max(maxSideShift, 0.01f)
////            );


////        return normalized * maxSideShift;
////    }


////    // ========================================================================
////    // ОТЛАДКА В SCENE VIEW
////    // ========================================================================

////    private void OnDrawGizmosSelected()
////    {
////        if (target == null)
////            return;


////        Gizmos.color = Color.cyan;


////        Vector3 lookPoint =
////            target.position +
////            Vector3.up * targetHeight;


////        Quaternion rotation =
////            Quaternion.Euler(
////                pitch,
////                yaw,
////                0f
////            );


////        Vector3 backward =
////            -(rotation * Vector3.forward);


////        Vector3 desiredPosition =
////            lookPoint +
////            backward * distance;


////        Gizmos.DrawLine(
////            lookPoint,
////            desiredPosition
////        );


////        Gizmos.DrawWireSphere(
////            desiredPosition,
////            cameraRadius
////        );
////    }
////}