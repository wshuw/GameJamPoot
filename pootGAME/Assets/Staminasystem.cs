//using UnityEngine;
//using System;

//// Ўкала стамины/постуры. «аполн€етс€ от блоков/парирований/обычных попаданий
//// через AddStamina(). ѕри заполнении до максимума Ч событие Stunned, стан
//// держитс€ stunDuration, потом StunRecovered и обнуление. ѕока не в стане и
//// давно не получала добавок Ч медленно регенерирует сама.
//public class StaminaSystem : MonoBehaviour
//{
//    [SerializeField] private float maxStamina = 100f;
//    [SerializeField] private float regenPerSecond = 15f;
//    [SerializeField] private float regenDelayAfterHit = 2f;
//    [SerializeField] private float stunDuration = 2f;

//    public float Current { get; private set; }
//    public float Max => maxStamina;
//    public bool IsStunned { get; private set; }

//    public event Action Stunned;
//    public event Action StunRecovered;

//    private float regenDelayTimer;
//    private float stunTimer;

//    private void Update()
//    {
//        if (IsStunned)
//        {
//            stunTimer -= Time.deltaTime;
//            if (stunTimer <= 0f)
//                RecoverFromStun();
//            return;
//        }

//        if (regenDelayTimer > 0f)
//        {
//            regenDelayTimer -= Time.deltaTime;
//        }
//        else if (Current > 0f)
//        {
//            Current = Mathf.Max(0f, Current - regenPerSecond * Time.deltaTime);
//        }
//    }

//    // cap (0..max, опционально) Ч Ё“ќ конкретное добавление не поднимет
//    // стамину выше cap. ≈сли стамина уже была выше cap (от предыдущего,
//    // некапнутого источника вроде блока) Ч вниз не откатываем, просто не
//    // даЄм Ё“ќћ” добавлению подн€ть еЄ ещЄ выше. »менно так работает
//    // "парированием нельз€ дойти до 100%, но блок/удар всЄ равно могут".
//    public void AddStamina(float amount, float? cap = null)
//    {
//        if (IsStunned || amount <= 0f)
//            return;

//        float ceiling = cap.HasValue ? Mathf.Min(cap.Value, maxStamina) : maxStamina;
//        ceiling = Mathf.Max(ceiling, Current);

//        Current = Mathf.Min(Current + amount, ceiling);
//        regenDelayTimer = regenDelayAfterHit;

//        if (Current >= maxStamina)
//            EnterStun();
//    }

//    private void EnterStun()
//    {
//        IsStunned = true;
//        Current = maxStamina;
//        stunTimer = stunDuration;
//        Stunned?.Invoke();
//    }

//    private void RecoverFromStun()
//    {
//        IsStunned = false;
//        Current = 0f;
//        StunRecovered?.Invoke();
//    }
//}
using UnityEngine;
using System;

public class StaminaSystem : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private PlayerUI playerUI;

    [Header("ѕараметры —тамины")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenPerSecond = 15f;
    [SerializeField] private float regenDelayAfterHit = 2f;
    [SerializeField] private float stunDuration = 2f;

    public float Current { get; private set; }
    public float Max => maxStamina;
    public bool IsStunned { get; private set; }

    public event Action Stunned;
    public event Action StunRecovered;

    private float regenDelayTimer;
    private float stunTimer;

    private void Start()
    {
        SendUITrigger();
    }

    private void Update()
    {
        if (IsStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                RecoverFromStun();
            return;
        }

        if (regenDelayTimer > 0f)
        {
            regenDelayTimer -= Time.deltaTime;
        }
        else if (Current > 0f)
        {
            Current = Mathf.Max(0f, Current - regenPerSecond * Time.deltaTime);
            SendUITrigger();
        }
    }

    public void AddStamina(float amount, float? cap = null)
    {
        if (IsStunned || amount <= 0f)
            return;

        float before = Current;
        float ceiling = cap.HasValue ? Mathf.Min(cap.Value, maxStamina) : maxStamina;
        ceiling = Mathf.Max(ceiling, Current);

        Current = Mathf.Min(Current + amount, ceiling);
        regenDelayTimer = regenDelayAfterHit;

        SendUITrigger();

        string capInfo = cap.HasValue ? $", кэп на этом добавлении {ceiling:F0}" : "";
        CombatLog.Stamina(name, $"+{amount:F0} ({before:F0} -> {Current:F0}/{maxStamina:F0}{capInfo})");

        if (Current >= maxStamina)
            EnterStun();
    }

    private void EnterStun()
    {
        IsStunned = true;
        Current = maxStamina;
        stunTimer = stunDuration;
        SendUITrigger();
        CombatLog.State(name, "Ч", "STUNNED", $"стамина заполнена, стан на {stunDuration:F1}с");
        Stunned?.Invoke();
    }

    private void RecoverFromStun()
    {
        IsStunned = false;
        Current = 0f;
        SendUITrigger();
        CombatLog.State(name, "STUNNED", "Ч", "стан закончилс€, стамина обнулена");
        StunRecovered?.Invoke();
    }

    private void SendUITrigger()
    {
        if (playerUI != null)
            playerUI.UpdateStamina(Current, maxStamina);
    }
}