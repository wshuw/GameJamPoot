using UnityEngine;

// Простой маркер "это можно взять в лок-он".
// Вешается на любой объект, который должен быть целью для замка камеры.
public class LockOnTarget : MonoBehaviour
{
    [Tooltip("Точка, в которую целится камера/лок. Если не задана — берётся transform самого объекта.")]
    [SerializeField] private Transform aimPoint;

    [Tooltip(
        "Условный 'радиус' цели для CinemachineTargetGroup (Group Framing). " +
        "Обычный враг ~0.5-0.8, крупный босс — значительно больше, " +
        "чтобы камера сама отъезжала и не упиралась в него кадром."
    )]
    [SerializeField] private float targetRadius = 0.6f;

    public Transform AimPoint => aimPoint != null ? aimPoint : transform;
    public float TargetRadius => targetRadius;

    // Статический реестр всех активных целей в сцене —
    // чтобы LockOnController не делал FindObjectsOfType каждый кадр.
    private static readonly System.Collections.Generic.List<LockOnTarget> active = new();

    public static System.Collections.Generic.IReadOnlyList<LockOnTarget> Active => active;

    private void OnEnable()
    {
        active.Add(this);
    }

    private void OnDisable()
    {
        active.Remove(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AimPoint.position, targetRadius);
    }
}