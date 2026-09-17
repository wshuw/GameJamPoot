using UnityEngine;

public class LineMovementWithPause : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость перемещения куба")]
    public float speed = 2.0f;

    [Tooltip("Расстояние от центра в каждую сторону (влево и вправо)")]
    public float movementDistance = 3.0f;

    [Tooltip("Время паузы в центре (в секундах)")]
    public float pauseDuration = 3.0f;

    // Точки маршрута
    private Vector3 centerPosition;
    private Vector3 leftPosition;
    private Vector3 rightPosition;

    // Внутреннее состояние
    private Vector3 currentTarget;
    private Vector3 nextTargetAfterCenter; // Куда идти после паузы в центре
    private float pauseTimer = 0f;
    private bool isPaused = false;

    void Start()
    {
        // Запоминаем центр как стартовую позицию
        centerPosition = transform.position;

        // Рассчитываем крайние точки влево и вправо по оси X
        leftPosition = centerPosition + new Vector3(-movementDistance, 0f, 0f);
        rightPosition = centerPosition + new Vector3(movementDistance, 0f, 0f);

        // Начинаем движение из центра к правой точке
        currentTarget = rightPosition;
        nextTargetAfterCenter = leftPosition;
    }

    void Update()
    {
        // Если куб находится на паузе в центре
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime; // Отсчитываем время назад
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                currentTarget = nextTargetAfterCenter; // Задаем следующую крайнюю точку
            }
            return; // Пропускаем движение в этом кадре
        }

        // Плавно перемещаем куб к текущей цели
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);

        // Проверяем, достиг ли куб цели
        if (Vector3.Distance(transform.position, currentTarget) < 0.001f)
        {
            // СЛУЧАЙ 1: Куб дошел до крайней точки (левой или правой) -> разворачиваем его к центру
            if (currentTarget == leftPosition || currentTarget == rightPosition)
            {
                // Запоминаем, куда куб должен пойти ПОСЛЕ того, как постоит в центре
                nextTargetAfterCenter = (currentTarget == leftPosition) ? rightPosition : leftPosition;

                // Направляем куб в центр
                currentTarget = centerPosition;
            }
            // СЛУЧАЙ 2: Куб дошел до центра -> включаем паузу
            else if (currentTarget == centerPosition)
            {
                isPaused = true;
                pauseTimer = pauseDuration; // Сбрасываем таймер на 3 секунды
            }
        }
    }

    // Визуализация линии движения в окне Scene
    private void OnDrawGizmos()
    {
        Vector3 center = Application.isPlaying ? centerPosition : transform.position;
        Vector3 left = center + new Vector3(-movementDistance, 0f, 0f);
        Vector3 right = center + new Vector3(movementDistance, 0f, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.15f);
        Gizmos.DrawSphere(right, 0.15f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(center, 0.15f); // Голубая сфера отмечает зону паузы
    }
}
