using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayerMovement))]
public class LockOnController : MonoBehaviour
{
    [Header("Настройки лока")]
    [SerializeField] private float lockRange = 15f;
    [SerializeField] private KeyCode lockKey = KeyCode.Mouse2; // средняя кнопка мыши

    [Header("Line of Sight")]
    [Tooltip("Сколько секунд враг может быть скрыт за препятствием, прежде чем лок снимется сам")]
    [SerializeField] private float loseSightUnlockDelay = 1.5f;
    [Tooltip("Что считается препятствием для луча видимости. НЕ включай сюда слой игрока и слой врагов")]
    [SerializeField] private LayerMask obstructionMask = ~0;
    [Tooltip("Откуда стреляем лучом видимости (например, камера или голова игрока). Если не задано — берётся позиция игрока + 1.5 по Y")]
    [SerializeField] private Transform sightOrigin;

    private PlayerMovement playerMovement;
    private LockOnTarget currentTarget;
    private float obstructedTimer;

    public LockOnTarget CurrentTarget => currentTarget;
    public bool IsLocked => currentTarget != null;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(lockKey))
        {
            HandleLockKeyPressed();
        }

        if (currentTarget != null)
        {
            // Цель разлочилась сама (уничтожена, отключена) или вышла за радиус
            if (!IsTargetValid(currentTarget))
            {
                ClearLock();
                return;
            }

            HandleLineOfSight();
        }
    }

    private void HandleLockKeyPressed()
    {
        if (!IsLocked)
        {
            LockOnTarget nearest = FindNearestTarget();

            if (nearest != null)
                SetLock(nearest);
        }
        else
        {
            LockOnTarget next = FindNextTarget();

            if (next != null && next != currentTarget)
                SetLock(next);
            else
                ClearLock(); // если больше некого выбрать — снимаем лок
        }
    }

    private LockOnTarget FindNearestTarget()
    {
        return LockOnTarget.Active
            .Where(IsTargetValid)
            .OrderBy(t => SqrDistanceTo(t))
            .FirstOrDefault();
    }

    private LockOnTarget FindNextTarget()
    {
        List<LockOnTarget> candidates = LockOnTarget.Active
            .Where(IsTargetValid)
            .OrderBy(t => SqrDistanceTo(t))
            .ToList();

        if (candidates.Count == 0)
            return null;

        int currentIndex = candidates.IndexOf(currentTarget);

        // Если текущей цели уже нет в списке (например, вышла из радиуса) —
        // начинаем сначала.
        int nextIndex = (currentIndex + 1) % candidates.Count;

        return candidates[nextIndex];
    }

    private bool IsTargetValid(LockOnTarget target)
    {
        if (target == null) return false;
        return SqrDistanceTo(target) <= lockRange * lockRange;
    }

    private float SqrDistanceTo(LockOnTarget target)
    {
        return (target.AimPoint.position - transform.position).sqrMagnitude;
    }

    // --- Line of Sight -----------------------------------------------------

    private void HandleLineOfSight()
    {
        if (IsLineOfSightBlocked(currentTarget))
        {
            obstructedTimer += Time.deltaTime;

            if (obstructedTimer >= loseSightUnlockDelay)
                ClearLock();
        }
        else
        {
            obstructedTimer = 0f;
        }
    }

    private bool IsLineOfSightBlocked(LockOnTarget target)
    {
        Vector3 origin = sightOrigin != null
            ? sightOrigin.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 targetPos = target.AimPoint.position;
        Vector3 toTarget = targetPos - origin;
        float distance = toTarget.magnitude;

        if (distance < 0.01f)
            return false;

        if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, distance, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            // Если луч упёрся не в саму цель (и не в её дочерние объекты) — видимость перекрыта
            return hit.transform != target.transform && !hit.transform.IsChildOf(target.transform);
        }

        return false;
    }

    // --- Lock control --------------------------------------------------------

    private void SetLock(LockOnTarget target)
    {
        currentTarget = target;
        obstructedTimer = 0f;
        playerMovement.SetLockOnTarget(target.AimPoint);
        playerMovement.SetMode(PlayerMovement.MovementMode.Strafe);
    }

    private void ClearLock()
    {
        currentTarget = null;
        obstructedTimer = 0f;
        playerMovement.SetLockOnTarget(null);
        playerMovement.SetMode(PlayerMovement.MovementMode.Free);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, lockRange);
    }
}