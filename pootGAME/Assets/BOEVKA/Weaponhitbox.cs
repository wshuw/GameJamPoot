//using UnityEngine;
//using System.Collections.Generic;

//// ¬ешаетс€ на коллайдер оружи€ (например на дочерний объект меча/руки).
////  оллайдер должен быть Trigger Ч компонент сам это выставит.
//// PlayerCombat/EnemyCombat включает/выключает этот хитбокс через
//// SetActive() строго на врем€ фазы Active.
////
//// ¬ј∆Ќќ: Rigidbody здесь об€зателен, хоть мы и не используем физику как
//// таковую. ¬ Unity OnTriggerEnter срабатывает, только если ’ќ“я Ѕџ у
//// одного из двух пересекающихс€ объектов есть Rigidbody.
//[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(Rigidbody))]
//public class WeaponHitbox : MonoBehaviour
//{
//    [Tooltip("”рон этой конкретной атаки Ч свой дл€ каждого оружи€/типа врага")]
//    [SerializeField] private AttackData attackData = new AttackData();

//    [Tooltip(" то владелец этого оружи€ (корневой объект атакующего Ч игрок или враг). ≈сли не задано Ч берЄтс€ transform.root")]
//    [SerializeField] private GameObject owner;

//    // „тобы один и тот же взмах не ударил одну и ту же цель несколько раз
//    // подр€д (коллайдер может триггернутьс€ не один раз за Active-фазу).
//    private readonly HashSet<IDamageable> hitThisSwing = new();

//    private Collider col;

//    private void Awake()
//    {
//        col = GetComponent<Collider>();
//        col.isTrigger = true;
//        col.enabled = false;

//        if (owner == null)
//            owner = transform.root.gameObject;

//        Rigidbody rb = GetComponent<Rigidbody>();
//        rb.isKinematic = true;
//        rb.useGravity = false;
//    }

//    public void SetActive(bool active)
//    {
//        col.enabled = active;

//        if (active)
//            hitThisSwing.Clear();
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        IDamageable damageable = other.GetComponentInParent<IDamageable>();
//        if (damageable == null)
//            return;

//        if (hitThisSwing.Contains(damageable))
//            return;

//        hitThisSwing.Add(damageable);
//        damageable.TakeDamage(attackData, owner);
//    }

//    private void OnDrawGizmos()
//    {
//        if (col == null) col = GetComponent<Collider>();
//        Gizmos.color = col != null && col.enabled ? new Color(1f, 0f, 0f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);

//        if (col is BoxCollider box)
//            Gizmos.DrawCube(transform.TransformPoint(box.center), Vector3.Scale(box.size, transform.lossyScale));
//        else if (col is SphereCollider sphere)
//            Gizmos.DrawSphere(transform.TransformPoint(sphere.center), sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
//    }
//}

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponHitbox : MonoBehaviour
{
    [Tooltip("”рон этой конкретной атаки Ч свой дл€ каждого оружи€/типа врага")]
    [SerializeField] private AttackData attackData = new AttackData();

    [Tooltip(" то владелец этого оружи€ (корневой объект атакующего Ч игрок или враг). ≈сли не задано Ч берЄтс€ transform.root")]
    [SerializeField] private GameObject owner;

    private readonly HashSet<IDamageable> hitThisSwing = new();

    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
        col.enabled = false;

        if (owner == null)
            owner = transform.root.gameObject;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void SetActive(bool active)
    {
        col.enabled = active;

        string ownerName = owner != null ? owner.name : name;
        CombatLog.Info(ownerName, active ? "хитбокс оружи€ ќ“ –џ“ (окно атаки)" : "хитбокс оружи€ закрыт");

        if (active)
            hitThisSwing.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        GameObject targetRoot = other.transform.root.gameObject;
        if (owner != null && targetRoot == owner)
            return; // не бьЄм сами себ€ Ч коллайдер оружи€ пересЄкс€ с телом владельца

        if (hitThisSwing.Contains(damageable))
            return;

        hitThisSwing.Add(damageable);

        string ownerName = owner != null ? owner.name : name;
        CombatLog.Info(ownerName, $"оружие коснулось {targetRoot.name} (базовые цифры атаки: hp {attackData.hpDamage:F0}, stamina {attackData.staminaDamageOnHit:F0}) Ч дальше решает TakeDamage цели");

        damageable.TakeDamage(attackData, owner);
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider>();
        Gizmos.color = col != null && col.enabled ? new Color(1f, 0f, 0f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);

        if (col is BoxCollider box)
            Gizmos.DrawCube(transform.TransformPoint(box.center), Vector3.Scale(box.size, transform.lossyScale));
        else if (col is SphereCollider sphere)
            Gizmos.DrawSphere(transform.TransformPoint(sphere.center), sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}