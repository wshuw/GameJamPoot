using UnityEngine;

public class SquareMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость перемещения куба")]
    public float speed = 2.0f;

    [Tooltip("Длина стороны квадрата в метрах")]
    public float sideLength = 3.0f;

    // Массив для хранения вершин квадрата
    private Vector3[] waypoints = new Vector3[4];
    // Индекс точки, к которой сейчас движется куб
    private int currentTargetIndex = 0;

    void Start()
    {
        // Запоминаем стартовую позицию куба как начальную точку
        Vector3 startPosition = transform.position;

        // Рассчитываем вершины квадрата динамически, используя sideLength:
        waypoints[0] = startPosition + new Vector3(sideLength, 0f, 0f);  // Вправо (по оси X)
        waypoints[1] = waypoints[0] + new Vector3(0f, 0f, -sideLength); // Назад (по оси Z)
        waypoints[2] = waypoints[1] + new Vector3(-sideLength, 0f, 0f); // Влево (по оси X)
        waypoints[3] = startPosition;                                   // Вперед (возврат в начало)
    }

    void Update()
    {
        // Определяем целевую точку, к которой нужно двигаться
        Vector3 targetPosition = waypoints[currentTargetIndex];

        // Плавно перемещаем куб в сторону целевой точки
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Если куб вплотную подошел к текущей точке, переключаемся на следующую
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Length;
        }
    }

    // Рисует траекторию движения в окне Scene для удобства настройки
    private void OnDrawGizmos()
    {
        // Если игра не запущена, рисуем примерный квадрат от текущей позиции в редакторе
        if (!Application.isPlaying)
        {
            Vector3 start = transform.position;
            Vector3 p1 = start + new Vector3(sideLength, 0f, 0f);
            Vector3 p2 = p1 + new Vector3(0f, 0f, -sideLength);
            Vector3 p3 = p2 + new Vector3(-sideLength, 0f, 0f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, start);
            return;
        }

        // Если игра запущена, подсвечиваем реальные точки зеленым
        if (waypoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector3 current = waypoints[i];
                Vector3 next = waypoints[(i + 1) % waypoints.Length];
                Gizmos.DrawLine(current, next);
                Gizmos.DrawSphere(current, 0.1f);
            }
        }
    }
}
    