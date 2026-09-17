//using UnityEngine;

//// Здоровье игрока. Логика реакции на удар зависит от текущего состояния
//// PlayerCombat:
////   - ParryWindow: урона по ХП нет вообще, стамина игрока растёт МЕДЛЕННО
////     и не может подняться выше parryStaminaCapFraction * Max (чтобы игрок,
////     идеально парирующий каждый удар, никогда не станился только за это).
////   - Block: небольшой chip-урон по ХП проходит, но основной эффект —
////     сильное заполнение стамины (может застанить при заполнении до 100%).
////   - Без защиты: полный урон по ХП + небольшое заполнение стамины (по ТЗ
////     "обычные атаки тоже стамину заполняют").
//public class PlayerHealth : MonoBehaviour, IDamageable
//{
//    [SerializeField] private float maxHealth = 100f;
//    [SerializeField] private PlayerCombat combat;
//    [SerializeField] private StaminaSystem stamina;

//    [Tooltip("0.85 = парированием стамина не может подняться выше 85% от максимума")]
//    [Range(0f, 1f)]
//    [SerializeField] private float parryStaminaCapFraction = 0.85f;

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
//        bool isParrying = combat != null && combat.State == PlayerCombat.CombatState.ParryWindow;
//        bool isBlocking = combat != null && combat.State == PlayerCombat.CombatState.Block;

//        if (isParrying)
//        {
//            if (stamina != null)
//            {
//                float cap = stamina.Max * parryStaminaCapFraction;
//                stamina.AddStamina(attack.staminaDamageOnParry, cap);
//            }

//            combat.NotifyParrySuccess(attacker);
//            return; // Урона по ХП при успешном парировании нет вообще
//        }

//        if (isBlocking)
//        {
//            currentHealth -= attack.chipDamageOnBlock;
//            if (stamina != null)
//                stamina.AddStamina(attack.staminaDamageOnBlock);
//        }
//        else
//        {
//            currentHealth -= attack.hpDamage;
//            if (stamina != null)
//                stamina.AddStamina(attack.staminaDamageOnHit);
//        }

//        string staminaInfo = stamina != null ? $", Stamina: {stamina.Current:F0}/{stamina.Max:F0}" : "";
//        Debug.Log($"Игрок получил урон от {(attacker != null ? attacker.name : "неизвестно")}. HP: {Mathf.Max(currentHealth, 0f):F0}/{maxHealth:F0}{staminaInfo}");

//        if (currentHealth <= 0f)
//            Die();
//    }

//    private void HandleStunned()
//    {
//        Debug.Log("Игрок оглушён (стамина заполнена)!");
//        if (combat != null)
//            combat.ForceStun();
//    }

//    private void HandleStunRecovered()
//    {
//        Debug.Log("Игрок вышел из стана.");
//        if (combat != null)
//            combat.ForceRecoverFromStun();
//    }

//    private void Die()
//    {
//        Debug.Log("Игрок погиб (заглушка — позже сделаем респавн/game over)");
//        currentHealth = maxHealth;
//    }
//}


using UnityEngine;

// Здоровье игрока. Логика реакции на удар зависит от текущего состояния
// PlayerCombat:
//   - ParryWindow: урона по ХП нет вообще, стамина игрока растёт МЕДЛЕННО
//     и не может подняться выше parryStaminaCapFraction * Max (чтобы игрок,
//     идеально парирующий каждый удар, никогда не станился только за это).
//   - Block: небольшой chip-урон по ХП проходит, но основной эффект —
//     сильное заполнение стамины (может застанить при заполнении до 100%).
//   - Без защиты: полный урон по ХП + небольшое заполнение стамины (по ТЗ
//     "обычные атаки тоже стамину заполняют").
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("UI")]
    [SerializeField] private PlayerUI playerUI;

    [Header("Параметры здоровья")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private StaminaSystem stamina;

    [Tooltip("0.85 = парированием стамина не может подняться выше 85% от максимума")]
    [Range(0f, 1f)]
    [SerializeField] private float parryStaminaCapFraction = 0.85f;

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

    private void Start()
    {
        // Инициализируем полоску ХП на старт игры (полное ХП)
        RefreshUI();
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
    //    bool isParrying = combat != null && combat.State == PlayerCombat.CombatState.ParryWindow;
    //    bool isBlocking = combat != null && combat.State == PlayerCombat.CombatState.Block;
    //    string attackerName = attacker != null ? attacker.name : "неизвестно";

    //    if (isParrying)
    //    {
    //        float staminaBefore = stamina != null ? stamina.Current : 0f;

    //        if (stamina != null)
    //        {
    //            float cap = stamina.Max * parryStaminaCapFraction;
    //            stamina.AddStamina(attack.staminaDamageOnParry, cap);
    //        }

    //        CombatLog.Damage(name, $"ПАРИРОВАЛ атаку от {attackerName} — 0 урона по ХП, своя стамина {staminaBefore:F0} -> {(stamina != null ? stamina.Current : 0f):F0}");

    //        if (attacker != null)
    //        {
    //            StaminaSystem attackerStamina = attacker.GetComponent<StaminaSystem>();
    //            if (attackerStamina != null)
    //                attackerStamina.AddStamina(attack.staminaDamageOnAttackerWhenParried);
    //        }

    //        combat.NotifyParrySuccess(attacker);
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

    //    RefreshUI();

    //    if (currentHealth <= 0f)
    //        Die();
    //}

    public void TakeDamage(AttackData attack, GameObject attacker)
    {
        bool isParrying = combat != null && combat.State == PlayerCombat.CombatState.ParryWindow;
        bool isBlocking = combat != null && combat.State == PlayerCombat.CombatState.Block;
        string attackerName = attacker != null ? attacker.name : "неизвестно";

        if (isParrying)
        {
            float staminaBefore = stamina != null ? stamina.Current : 0f;

            if (stamina != null)
            {
                float cap = stamina.Max * parryStaminaCapFraction;
                stamina.AddStamina(attack.staminaDamageOnParry, cap);
            }

            CombatLog.Damage(name, $"ПАРИРОВАЛ атаку от {attackerName} — 0 урона по ХП, своя стамина {staminaBefore:F0} -> {(stamina != null ? stamina.Current : 0f):F0}");

            if (attacker != null)
            {
                StaminaSystem attackerStamina = attacker.GetComponent<StaminaSystem>();
                if (attackerStamina != null)
                    attackerStamina.AddStamina(attack.staminaDamageOnAttackerWhenParried);
            }

            combat.NotifyParrySuccess(attacker);
            return;
        }

        if (isBlocking)
        {
            // Блок = ноль урона по ХП, весь эффект уходит в стамину.
            float staminaBefore = stamina != null ? stamina.Current : 0f;
            if (stamina != null)
                stamina.AddStamina(attack.staminaDamageOnBlock);

            CombatLog.Damage(name, $"ЗАБЛОКИРОВАЛ атаку от {attackerName} — 0 урона по ХП, стамина {staminaBefore:F0} -> {(stamina != null ? stamina.Current : 0f):F0}");
            return; // ХП не трогаем вообще
        }

        float before = currentHealth;
        currentHealth -= attack.hpDamage;
        CombatLog.Damage(name, $"ПОЛУЧИЛ ПОЛНЫЙ УДАР от {attackerName}: {attack.hpDamage:F0} ХП ({before:F0} -> {Mathf.Max(currentHealth, 0f):F0})");
        if (stamina != null)
            stamina.AddStamina(attack.staminaDamageOnHit);

        RefreshUI();

        if (currentHealth <= 0f)
            Die();
    }

    private void HandleStunned()
    {
        Debug.Log("Игрок оглушён (стамина заполнена)!");
        if (combat != null)
            combat.ForceStun();
    }

    private void HandleStunRecovered()
    {
        Debug.Log("Игрок вышел из стана.");
        if (combat != null)
            combat.ForceRecoverFromStun();
    }

    private void Die()
    {
        Debug.Log("Игрок погиб (заглушка — позже сделаем респавн/game over)");
        currentHealth = maxHealth;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (playerUI != null)
            playerUI.UpdateHealth(currentHealth, maxHealth);
    }
}
