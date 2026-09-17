//using UnityEngine;

//// Здоровье врага. Сейчас у EnemyCombat ещё нет Block/ParryWindow (это
//// следующий этап — интеллект врага), поэтому пока любой удар обрабатывается
//// как "обычное попадание": полный урон по ХП + небольшое заполнение
//// стамины. Когда враг научится блокировать/парировать — сюда добавятся
//// такие же ветки, как в PlayerHealth.
//public class EnemyHealth : MonoBehaviour, IDamageable
//{
//    [SerializeField] private float maxHealth = 100f;
//    [SerializeField] private EnemyCombat combat;
//    [SerializeField] private StaminaSystem stamina;

//    private float currentHealth;

//    private void Awake()
//    {
//        currentHealth = maxHealth;

//        if (stamina != null)
//        {
//            stamina.Stunned += HandleStunned;
//            stamina.StunRecovered += HandleStunRecovered;
//        }
//    }

//    private void OnDestroy()
//    {
//        if (stamina != null)
//        {
//            stamina.Stunned -= HandleStunned;
//            stamina.StunRecovered -= HandleStunRecovered;
//        }
//    }

//    public void TakeDamage(AttackData attack, GameObject attacker)
//    {
//        // TODO: когда у EnemyCombat появятся Block/ParryWindow, здесь нужно
//        // будет проверять combat.State так же, как в PlayerHealth.TakeDamage.
//        currentHealth -= attack.hpDamage;

//        if (stamina != null)
//            stamina.AddStamina(attack.staminaDamageOnHit);

//        string staminaInfo = stamina != null ? $", Stamina: {stamina.Current:F0}/{stamina.Max:F0}" : "";
//        Debug.Log($"{name} получил {attack.hpDamage} урона от {(attacker != null ? attacker.name : "неизвестно")}. HP: {Mathf.Max(currentHealth, 0f):F0}/{maxHealth:F0}{staminaInfo}");

//        if (currentHealth <= 0f)
//            Die();
//    }

//    private void HandleStunned()
//    {
//        if (combat != null)
//            combat.ForceStun();
//    }

//    private void HandleStunRecovered()
//    {
//        if (combat != null)
//            combat.ForceRecoverFromStun();
//    }

//    private void Die()
//    {
//        Debug.Log($"{name} уничтожен.");
//        currentHealth = maxHealth;
//    }
//}


using UnityEngine;

// Здоровье врага. Учитывает собственные Block/ParryWindow врага (Этап 3):
//   - ParryWindow: урона по ХП нет, СВОЯ стамина врага не растёт вообще —
//     зато атакующий (игрок) получает урон по своей стамине как "наказание"
//     за парированную атаку.
//   - Block: небольшой chip-урон по ХП проходит, и свою стамину враг всё же
//     немного получает (вес regулируется полем AttackData на оружии игрока).
//   - Без защиты: полный урон по ХП + обычное заполнение стамины.
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private EnemyCombat combat;
    [SerializeField] private StaminaSystem stamina;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (stamina != null)
        {
            stamina.Stunned += HandleStunned;
            stamina.StunRecovered += HandleStunRecovered;
        }
    }

    private void OnDestroy()
    {
        if (stamina != null)
        {
            stamina.Stunned -= HandleStunned;
            stamina.StunRecovered -= HandleStunRecovered;
        }
    }

    //public void TakeDamage(AttackData attack, GameObject attacker)
    //{
    //    bool isParrying = combat != null && combat.State == EnemyCombat.EnemyState.ParryWindow;
    //    bool isBlocking = combat != null && combat.State == EnemyCombat.EnemyState.Block;
    //    string attackerName = attacker != null ? attacker.name : "неизвестно";

    //    if (isParrying)
    //    {
    //        CombatLog.Damage(name, $"ПАРИРОВАЛ атаку от {attackerName} — 0 урона по ХП, своя стамина не тронута");

    //        if (attacker != null)
    //        {
    //            StaminaSystem attackerStamina = attacker.GetComponent<StaminaSystem>();
    //            if (attackerStamina != null)
    //                attackerStamina.AddStamina(attack.staminaDamageOnAttackerWhenParried);
    //            else
    //                CombatLog.Info(name, $"не нашёл StaminaSystem на {attackerName} — отражённый урон по стамине не применён");
    //        }
    //        return;
    //    }

    //    float before = currentHealth;

    //    if (isBlocking)
    //    {
    //        currentHealth -= attack.chipDamageOnBlock;
    //        CombatLog.Damage(name, $"ЗАБЛОКИРОВАЛ атаку от {attackerName}: chip {attack.chipDamageOnBlock:F0} ХП ({before:F0} -> {Mathf.Max(currentHealth, 0f):F0})");
    //        if (stamina != null)
    //            stamina.AddStamina(attack.staminaDamageOnBlock);
    //    }
    //    else
    //    {
    //        currentHealth -= attack.hpDamage;
    //        CombatLog.Damage(name, $"ПОЛУЧИЛ ПОЛНЫЙ УДАР от {attackerName}: {attack.hpDamage:F0} ХП ({before:F0} -> {Mathf.Max(currentHealth, 0f):F0})");
    //        if (stamina != null)
    //            stamina.AddStamina(attack.staminaDamageOnHit);
    //    }

    //    if (currentHealth <= 0f)
    //        Die();
    //}

    public void TakeDamage(AttackData attack, GameObject attacker)
    {
        bool isParrying = combat != null && combat.State == EnemyCombat.EnemyState.ParryWindow;
        bool isBlocking = combat != null && combat.State == EnemyCombat.EnemyState.Block;
        string attackerName = attacker != null ? attacker.name : "неизвестно";

        if (isParrying)
        {
            CombatLog.Damage(name, $"ПАРИРОВАЛ атаку от {attackerName} — 0 урона по ХП, своя стамина не тронута");

            if (attacker != null)
            {
                StaminaSystem attackerStamina = attacker.GetComponent<StaminaSystem>();
                if (attackerStamina != null)
                    attackerStamina.AddStamina(attack.staminaDamageOnAttackerWhenParried);
                else
                    CombatLog.Info(name, $"не нашёл StaminaSystem на {attackerName} — отражённый урон по стамине не применён");
            }
            return;
        }

        if (isBlocking)
        {
            // Блок = ноль урона по ХП, только стамина (немного, тюнится весом
            // staminaDamageOnBlock на оружии ИГРОКА — см. ниже, где что настраивать).
            float staminaBefore = stamina != null ? stamina.Current : 0f;
            if (stamina != null)
                stamina.AddStamina(attack.staminaDamageOnBlock);

            CombatLog.Damage(name, $"ЗАБЛОКИРОВАЛ атаку от {attackerName} — 0 урона по ХП, стамина {staminaBefore:F0} -> {(stamina != null ? stamina.Current : 0f):F0}");
            return;
        }

        float before = currentHealth;
        currentHealth -= attack.hpDamage;
        CombatLog.Damage(name, $"ПОЛУЧИЛ ПОЛНЫЙ УДАР от {attackerName}: {attack.hpDamage:F0} ХП ({before:F0} -> {Mathf.Max(currentHealth, 0f):F0})");
        if (stamina != null)
            stamina.AddStamina(attack.staminaDamageOnHit);

        if (currentHealth <= 0f)
            Die();
    }

    private void HandleStunned()
    {
        if (combat != null)
            combat.ForceStun();
    }

    private void HandleStunRecovered()
    {
        if (combat != null)
            combat.ForceRecoverFromStun();
    }

    private void Die()
    {
        CombatLog.State(name, combat != null ? combat.State.ToString() : "?", "DEAD");
        Debug.Log($"{name} уничтожен.");
        currentHealth = maxHealth;
    }
}
