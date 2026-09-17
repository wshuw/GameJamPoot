//using UnityEngine;

//// Патрулирование: список точек по порядку (ходит по кругу между ними),
//// ИЛИ, если точек нет — просто стоит на месте. Настраивается перетаскиванием
//// пустых Transform-объектов в массив Waypoints в инспекторе — никакого кода
//// менять не нужно, чтобы добавить/поменять маршрут.
////
//// Сам себя не включает/выключает — активируется снаружи (EnemyCombat) через
//// SetActive(), когда враг НЕ видит игрока.
//public class EnemyPatrol : MonoBehaviour
//{
//    [Tooltip("Точки патруля по порядку. Если пусто — враг просто стоит на месте")]
//    [SerializeField] private Transform[] waypoints;
//    [SerializeField] private float moveSpeed = 2f;
//    [SerializeField] private float waypointReachDistance = 0.2f;
//    [Tooltip("Сколько секунд стоять на точке, прежде чем идти к следующей")]
//    [SerializeField] private float waitTimeAtWaypoint = 1.5f;

//    private int currentWaypointIndex;
//    private float waitTimer;
//    private bool isActive;

//    public void SetActive(bool active)
//    {
//        isActive = active;
//        waitTimer = 0f;
//    }

//    private void Update()
//    {
//        if (!isActive || waypoints == null || waypoints.Length == 0)
//            return;

//        Transform target = waypoints[currentWaypointIndex];
//        if (target == null)
//            return;

//        Vector3 dir = target.position - transform.position;
//        dir.y = 0f;
//        float distance = dir.magnitude;

//        if (distance <= waypointReachDistance)
//        {
//            waitTimer += Time.deltaTime;
//            if (waitTimer >= waitTimeAtWaypoint)
//            {
//                waitTimer = 0f;
//                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
//            }
//            return;
//        }

//        dir.Normalize();
//        transform.position += dir * moveSpeed * Time.deltaTime;
//        transform.rotation = Quaternion.LookRotation(dir);
//    }

//    private void OnDrawGizmosSelected()
//    {
//        if (waypoints == null || waypoints.Length < 2)
//            return;

//        Gizmos.color = Color.cyan;
//        for (int i = 0; i < waypoints.Length; i++)
//        {
//            if (waypoints[i] == null) continue;
//            Transform next = waypoints[(i + 1) % waypoints.Length];
//            if (next == null) continue;

//            Gizmos.DrawLine(waypoints[i].position, next.position);
//            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);
//        }
//    }
//}

using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Tooltip("Точки патруля по порядку. Если пусто — враг просто стоит на месте")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waypointReachDistance = 0.2f;
    [Tooltip("Сколько секунд стоять на точке, прежде чем идти к следующей")]
    [SerializeField] private float waitTimeAtWaypoint = 1.5f;

    private int currentWaypointIndex;
    private float waitTimer;
    private bool isActive;

    public void SetActive(bool active)
    {
        if (isActive != active)
            CombatLog.Info(name, active ? "патруль включён (не видит игрока)" : "патруль выключен (занят боем/погоней)");

        isActive = active;
        waitTimer = 0f;
    }

    private void Update()
    {
        if (!isActive || waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypointIndex];
        if (target == null)
            return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        float distance = dir.magnitude;

        if (distance <= waypointReachDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                int prev = currentWaypointIndex;
                waitTimer = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                CombatLog.Info(name, $"патруль: точка {prev} -> {currentWaypointIndex}");
            }
            return;
        }

        dir.Normalize();
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Transform next = waypoints[(i + 1) % waypoints.Length];
            if (next == null) continue;

            Gizmos.DrawLine(waypoints[i].position, next.position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);
        }
    }
}