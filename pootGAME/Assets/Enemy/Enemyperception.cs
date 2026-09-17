//using UnityEngine;

//public class EnemyPerception : MonoBehaviour
//{
//    [SerializeField] private Transform player;
//    [SerializeField] private float detectionRange = 10f;
//    [Tooltip("Дистанция, на которой враг ТЕРЯЕТ игрока из виду. Должна быть больше detectionRange")]
//    [SerializeField] private float loseSightRange = 15f;

//    public bool CanSeePlayer { get; private set; }
//    public Transform Player => player;
//    public float DistanceToPlayer { get; private set; } = Mathf.Infinity;

//    private void Update()
//    {
//        if (player == null)
//        {
//            CanSeePlayer = false;
//            return;
//        }

//        DistanceToPlayer = Vector3.Distance(transform.position, player.position);

//        if (CanSeePlayer)
//        {
//            if (DistanceToPlayer > loseSightRange)
//            {
//                CanSeePlayer = false;
//                CombatLog.Info(name, $"потерял игрока из виду (дистанция {DistanceToPlayer:F1} > {loseSightRange:F1})");
//            }
//        }
//        else
//        {
//            if (DistanceToPlayer <= detectionRange)
//            {
//                CanSeePlayer = true;
//                CombatLog.Info(name, $"заметил игрока (дистанция {DistanceToPlayer:F1} <= {detectionRange:F1})");
//            }
//        }
//    }

//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
//        Gizmos.DrawWireSphere(transform.position, detectionRange);

//        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
//        Gizmos.DrawWireSphere(transform.position, loseSightRange);
//    }
//}
using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 10f;
    [Tooltip("Дистанция, на которой враг ТЕРЯЕТ игрока из виду. Должна быть больше detectionRange")]
    [SerializeField] private float loseSightRange = 15f;

    [Header("Препятствия (стены и т.п.)")]
    [Tooltip("Какие слои считаются препятствием, перекрывающим обзор. НЕ включай сюда слой самого игрока и слой врагов!")]
    [SerializeField] private LayerMask obstacleMask;
    [Tooltip("Высота \"глаз\" врага над его transform.position")]
    [SerializeField] private float eyeHeight = 1.6f;
    [Tooltip("Высота точки на игроке, в которую целимся лучом (примерно грудь)")]
    [SerializeField] private float playerTargetHeight = 1.4f;

    public bool CanSeePlayer { get; private set; }
    public Transform Player => player;
    public float DistanceToPlayer { get; private set; } = Mathf.Infinity;

    private void Update()
    {
        if (player == null)
        {
            CanSeePlayer = false;
            return;
        }

        DistanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLineOfSight = HasLineOfSight();

        if (CanSeePlayer)
        {
            if (DistanceToPlayer > loseSightRange || !hasLineOfSight)
            {
                CanSeePlayer = false;
                CombatLog.Info(name, !hasLineOfSight
                    ? "потерял игрока из виду (перекрыто препятствием)"
                    : $"потерял игрока из виду (дистанция {DistanceToPlayer:F1} > {loseSightRange:F1})");
            }
        }
        else
        {
            if (DistanceToPlayer <= detectionRange && hasLineOfSight)
            {
                CanSeePlayer = true;
                CombatLog.Info(name, $"заметил игрока (дистанция {DistanceToPlayer:F1} <= {detectionRange:F1}, прямая видимость есть)");
            }
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = player.position + Vector3.up * playerTargetHeight;

        // true = что-то из obstacleMask перекрывает путь -> видимости нет
        return !Physics.Linecast(eyePos, targetPos, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, loseSightRange);

        if (player != null)
        {
            Gizmos.color = CanSeePlayer ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * eyeHeight, player.position + Vector3.up * playerTargetHeight);
        }
    }
}