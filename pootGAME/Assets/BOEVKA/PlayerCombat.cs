//using UnityEngine;

//public class PlayerCombat : MonoBehaviour
//{
//    public enum CombatState
//    {
//        Idle,
//        AttackWindup,
//        AttackActive,
//        AttackRecovery,
//        Block,
//        ParryWindow,
//        Stunned
//    }

//    [Header("Настройки ввода")]
//    [SerializeField] private InputSettings input;

//    [Header("Тайминги лёгкой атаки (секунды)")]
//    [SerializeField] private float windupDuration = 0.2f;
//    [SerializeField] private float activeDuration = 0.15f;
//    [SerializeField] private float recoveryDuration = 0.25f;

//    [Header("Буфер ввода")]
//    [Tooltip("Если нажать атаку за это время до конца текущей фазы — атака гарантированно запустится сама, как только освободится")]
//    [SerializeField] private float inputBufferTime = 0.2f;

//    [Header("Парирование")]
//    [Tooltip("Короткое окно активного парирования сразу после нажатия блока")]
//    [SerializeField] private float parryWindowDuration = 0.35f;

//    [Header("Оружие / хитбокс")]
//    [SerializeField] private WeaponHitbox weaponHitbox;

//    public CombatState State => state;

//    public bool CanMove => state == CombatState.Idle || state == CombatState.Block;

//    private CombatState state = CombatState.Idle;
//    private float stateTimer;

//    private bool attackBuffered;
//    private float attackBufferTimer;

//    private void Update()
//    {
//        if (input == null)
//            return;

//        ReadInput();
//        TickState();
//    }

//    // -------------------------------------------------------------------------
//    // Ввод
//    // -------------------------------------------------------------------------

//    private void ReadInput()
//    {
//        // В стане игрок полностью не отвечает на ввод боя.
//        if (state == CombatState.Stunned)
//            return;

//        if (Input.GetKeyDown(input.attack))
//            TryStartAttack();

//        if (Input.GetKeyDown(input.block))
//            TryStartBlock();

//        if (Input.GetKeyUp(input.block) && (state == CombatState.Block || state == CombatState.ParryWindow))
//            ChangeState(CombatState.Idle);

//        if (attackBuffered)
//        {
//            attackBufferTimer -= Time.deltaTime;
//            if (attackBufferTimer <= 0f)
//                attackBuffered = false;
//        }
//    }

//    private void TryStartAttack()
//    {
//        switch (state)
//        {
//            case CombatState.Idle:
//                ChangeState(CombatState.AttackWindup);
//                break;

//            case CombatState.AttackWindup:
//            case CombatState.AttackActive:
//            case CombatState.AttackRecovery:
//                BufferAttack();
//                break;

//            case CombatState.Block:
//            case CombatState.ParryWindow:
//                break;
//        }
//    }

//    private void BufferAttack()
//    {
//        attackBuffered = true;
//        attackBufferTimer = inputBufferTime;
//    }

//    private void TryStartBlock()
//    {
//        switch (state)
//        {
//            case CombatState.Idle:
//                ChangeState(CombatState.ParryWindow);
//                break;

//            case CombatState.AttackWindup:
//                // Cancel Priority: Block/Parry > Light Attack.
//                ChangeState(CombatState.ParryWindow);
//                break;

//            case CombatState.AttackActive:
//            case CombatState.AttackRecovery:
//                break;

//            case CombatState.Block:
//            case CombatState.ParryWindow:
//                break;
//        }
//    }

//    // -------------------------------------------------------------------------
//    // Тайминги состояний
//    // -------------------------------------------------------------------------

//    private void TickState()
//    {
//        // Stunned не тикает сам по себе — из него выводит ForceRecoverFromStun(),
//        // вызванный снаружи по событию StaminaSystem.StunRecovered.
//        if (state == CombatState.Stunned)
//            return;

//        stateTimer -= Time.deltaTime;

//        switch (state)
//        {
//            case CombatState.AttackWindup:
//                if (stateTimer <= 0f)
//                    ChangeState(CombatState.AttackActive);
//                break;

//            case CombatState.AttackActive:
//                if (stateTimer <= 0f)
//                    ChangeState(CombatState.AttackRecovery);
//                break;

//            case CombatState.AttackRecovery:
//                if (stateTimer <= 0f)
//                {
//                    ChangeState(CombatState.Idle);

//                    if (attackBuffered)
//                    {
//                        attackBuffered = false;
//                        ChangeState(CombatState.AttackWindup);
//                    }
//                }
//                break;

//            case CombatState.ParryWindow:
//                if (stateTimer <= 0f)
//                    ChangeState(CombatState.Block);
//                break;

//            case CombatState.Idle:
//            case CombatState.Block:
//                break;
//        }
//    }

//    private void ChangeState(CombatState newState)
//    {
//        if (state == CombatState.AttackActive && weaponHitbox != null)
//            weaponHitbox.SetActive(false);

//        state = newState;

//        switch (newState)
//        {
//            case CombatState.AttackWindup:
//                stateTimer = windupDuration;
//                break;

//            case CombatState.AttackActive:
//                stateTimer = activeDuration;
//                if (weaponHitbox != null)
//                    weaponHitbox.SetActive(true);
//                break;

//            case CombatState.AttackRecovery:
//                stateTimer = recoveryDuration;
//                break;

//            case CombatState.ParryWindow:
//                stateTimer = parryWindowDuration;
//                break;

//            case CombatState.Block:
//            case CombatState.Idle:
//            case CombatState.Stunned:
//                stateTimer = 0f;
//                break;
//        }

//        Debug.Log($"Combat state: {state}");
//    }

//    // Вызывается PlayerHealth, когда игрок получает удар, находясь в
//    // состоянии ParryWindow. Реагируем на атакующего.
//    public void NotifyParrySuccess(GameObject attacker)
//    {
//        Debug.Log("Parry! Атака отражена.");

//        if (attacker == null)
//            return;

//        EnemyCombat enemy = attacker.GetComponent<EnemyCombat>();
//        if (enemy != null)
//            enemy.GetStaggered();
//    }

//    // Вызывается PlayerHealth по событиям StaminaSystem.
//    public void ForceStun()
//    {
//        ChangeState(CombatState.Stunned);
//    }

//    public void ForceRecoverFromStun()
//    {
//        if (state == CombatState.Stunned)
//            ChangeState(CombatState.Idle);
//    }
//}

using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public enum CombatState
    {
        Idle,
        AttackWindup,
        AttackActive,
        AttackRecovery,
        Block,
        ParryWindow,
        Stunned
    }

    [Header("Настройки ввода")]
    [SerializeField] private InputSettings input;

    [Header("Тайминги лёгкой атаки (секунды)")]
    [SerializeField] private float windupDuration = 0.2f;
    [SerializeField] private float activeDuration = 0.15f;
    [SerializeField] private float recoveryDuration = 0.25f;

    [Header("Буфер ввода")]
    [SerializeField] private float inputBufferTime = 0.2f;

    [Header("Парирование")]
    [SerializeField] private float parryWindowDuration = 0.35f;

    [Header("Оружие / хитбокс")]
    [SerializeField] private WeaponHitbox weaponHitbox;

    public CombatState State => state;
    public bool CanMove => state == CombatState.Idle || state == CombatState.Block;

    private CombatState state = CombatState.Idle;
    private float stateTimer;

    private bool attackBuffered;
    private float attackBufferTimer;

    private void Update()
    {
        if (input == null)
            return;

        ReadInput();
        TickState();
    }

    private void ReadInput()
    {
        if (state == CombatState.Stunned)
            return;

        if (Input.GetKeyDown(input.attack))
        {
            CombatLog.Info(name, "ввод: АТАКА нажата");
            TryStartAttack();
        }

        if (Input.GetKeyDown(input.block))
        {
            CombatLog.Info(name, "ввод: БЛОК нажат");
            TryStartBlock();
        }

        if (Input.GetKeyUp(input.block) && (state == CombatState.Block || state == CombatState.ParryWindow))
        {
            CombatLog.Info(name, "ввод: БЛОК отпущен");
            ChangeState(CombatState.Idle, "игрок отпустил блок");
        }

        if (attackBuffered)
        {
            attackBufferTimer -= Time.deltaTime;
            if (attackBufferTimer <= 0f)
            {
                attackBuffered = false;
                CombatLog.Info(name, "буфер атаки истёк без применения");
            }
        }
    }

    private void TryStartAttack()
    {
        switch (state)
        {
            case CombatState.Idle:
                ChangeState(CombatState.AttackWindup, "старт атаки из Idle");
                break;

            case CombatState.AttackWindup:
            case CombatState.AttackActive:
            case CombatState.AttackRecovery:
                BufferAttack();
                break;

            case CombatState.Block:
            case CombatState.ParryWindow:
                CombatLog.Info(name, "атака проигнорирована — игрок держит блок/парирование");
                break;
        }
    }

    private void BufferAttack()
    {
        attackBuffered = true;
        attackBufferTimer = inputBufferTime;
        CombatLog.Info(name, $"атака забуферена (state={state}), сработает если {inputBufferTime:F2}с не истекут раньше");
    }

    private void TryStartBlock()
    {
        switch (state)
        {
            case CombatState.Idle:
                ChangeState(CombatState.ParryWindow, "старт защиты из Idle");
                break;

            case CombatState.AttackWindup:
                ChangeState(CombatState.ParryWindow, "отмена своей атаки ради защиты (Cancel Priority)");
                break;

            case CombatState.AttackActive:
            case CombatState.AttackRecovery:
                CombatLog.Info(name, "попытка защититься проигнорирована — уже в фазе Active/Recovery своей атаки");
                break;

            case CombatState.Block:
            case CombatState.ParryWindow:
                break;
        }
    }

    private void TickState()
    {
        if (state == CombatState.Stunned)
            return;

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case CombatState.AttackWindup:
                if (stateTimer <= 0f)
                    ChangeState(CombatState.AttackActive, "windup закончился");
                break;

            case CombatState.AttackActive:
                if (stateTimer <= 0f)
                    ChangeState(CombatState.AttackRecovery, "окно атаки закрылось");
                break;

            case CombatState.AttackRecovery:
                if (stateTimer <= 0f)
                {
                    ChangeState(CombatState.Idle, "recovery закончился");

                    if (attackBuffered)
                    {
                        attackBuffered = false;
                        CombatLog.Info(name, "применяю забуференную атаку");
                        ChangeState(CombatState.AttackWindup, "буферизованная атака");
                    }
                }
                break;

            case CombatState.ParryWindow:
                if (stateTimer <= 0f)
                    ChangeState(CombatState.Block, "окно парирования истекло, держим блок дальше");
                break;

            case CombatState.Idle:
            case CombatState.Block:
                break;
        }
    }

    private void ChangeState(CombatState newState, string reason = null)
    {
        if (state == CombatState.AttackActive && weaponHitbox != null)
            weaponHitbox.SetActive(false);

        CombatState oldState = state;
        state = newState;

        switch (newState)
        {
            case CombatState.AttackWindup:
                stateTimer = windupDuration;
                break;

            case CombatState.AttackActive:
                stateTimer = activeDuration;
                if (weaponHitbox != null)
                    weaponHitbox.SetActive(true);
                break;

            case CombatState.AttackRecovery:
                stateTimer = recoveryDuration;
                break;

            case CombatState.ParryWindow:
                stateTimer = parryWindowDuration;
                break;

            case CombatState.Block:
            case CombatState.Idle:
            case CombatState.Stunned:
                stateTimer = 0f;
                break;
        }

        CombatLog.State(name, oldState.ToString(), newState.ToString(), reason);
    }

    public void NotifyParrySuccess(GameObject attacker)
    {
        CombatLog.Decision(name, $"ПАРИРОВАНИЕ УСПЕШНО против {(attacker != null ? attacker.name : "неизвестно")}!");

        if (attacker == null)
            return;

        EnemyCombat enemy = attacker.GetComponent<EnemyCombat>();
        if (enemy != null)
            enemy.GetStaggered();
    }

    public void ForceStun()
    {
        ChangeState(CombatState.Stunned, "стамина заполнена");
    }

    public void ForceRecoverFromStun()
    {
        if (state == CombatState.Stunned)
            ChangeState(CombatState.Idle, "стан закончился");
    }
}