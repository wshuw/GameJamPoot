//using UnityEngine;

//// State machine врага. Верхний уровень — "не видит игрока" (Idle, патруль
//// работает) / "видит игрока" (всё остальное — погоня и бой). Переключение
//// между ними полностью отдано EnemyPerception, движение в патруле —
//// EnemyPatrol, здесь только бой и переходы.
//public class EnemyCombat : MonoBehaviour
//{
//    public enum EnemyState
//    {
//        Idle,       // не видит игрока — патрулирует/стоит (см. EnemyPatrol)
//        Chasing,    // видит игрока, идёт к нему / ждёт кулдаун атаки в радиусе
//        Windup,
//        Active,
//        Recovery,
//        Staggered,  // короткое прерывание от успешного парирования
//        Stunned     // долгий стан от заполненной стамины
//    }

//    [Header("Восприятие / патруль")]
//    [SerializeField] private EnemyPerception perception;
//    [SerializeField] private EnemyPatrol patrol;

//    [Header("Бой: дистанции и скорость")]
//    [SerializeField] private float attackRange = 2f;
//    [SerializeField] private float moveSpeed = 3f;

//    [Header("Тайминги атаки (секунды)")]
//    [SerializeField] private float windupDuration = 0.5f;
//    [SerializeField] private float activeDuration = 0.2f;
//    [SerializeField] private float recoveryDuration = 0.6f;
//    [SerializeField] private float staggerDuration = 1f;

//    [Header("Частота атак")]
//    [Tooltip("После Recovery враг ждёт случайное время в этом диапазоне, прежде чем атаковать снова (пока игрок в радиусе атаки)")]
//    [SerializeField] private float attackCooldownMin = 0.4f;
//    [SerializeField] private float attackCooldownMax = 1.2f;

//    [Header("Оружие")]
//    [SerializeField] private WeaponHitbox weaponHitbox;

//    [Header("Визуальный телеграф (временно, для тестов)")]
//    [SerializeField] private MeshRenderer bodyRenderer;
//    [SerializeField] private Color idleColor = Color.white;
//    [SerializeField] private Color windupColor = Color.yellow;
//    [SerializeField] private Color activeColor = Color.red;
//    [SerializeField] private Color staggeredColor = Color.gray;
//    [SerializeField] private Color stunnedColor = new Color(0.2f, 0f, 0.3f);

//    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
//    private static readonly int ColorID = Shader.PropertyToID("_Color");
//    private MaterialPropertyBlock propBlock;

//    public EnemyState State => state;

//    private EnemyState state = EnemyState.Idle;
//    private float stateTimer;
//    private float attackCooldownTimer;

//    private Transform Player => perception != null ? perception.Player : null;

//    private void Awake()
//    {
//        propBlock = new MaterialPropertyBlock();

//        if (bodyRenderer == null)
//            bodyRenderer = GetComponent<MeshRenderer>();

//        if (patrol != null)
//            patrol.SetActive(true); // стартуем "не видя" игрока
//    }

//    private void Update()
//    {
//        if (perception == null || Player == null)
//            return;

//        TickState();
//    }

//    private void TickState()
//    {
//        if (state == EnemyState.Stunned)
//            return; // выводит только ForceRecoverFromStun() снаружи

//        switch (state)
//        {
//            case EnemyState.Idle:
//                if (perception.CanSeePlayer)
//                    ChangeState(EnemyState.Chasing);
//                break;

//            case EnemyState.Chasing:
//                if (!perception.CanSeePlayer)
//                {
//                    ChangeState(EnemyState.Idle);
//                    break;
//                }

//                if (perception.DistanceToPlayer <= attackRange)
//                {
//                    attackCooldownTimer -= Time.deltaTime;
//                    if (attackCooldownTimer <= 0f)
//                        ChangeState(EnemyState.Windup);
//                }
//                else
//                {
//                    MoveTowardsPlayer();
//                }
//                break;

//            case EnemyState.Windup:
//                stateTimer -= Time.deltaTime;
//                if (stateTimer <= 0f)
//                    ChangeState(EnemyState.Active);
//                break;

//            case EnemyState.Active:
//                stateTimer -= Time.deltaTime;
//                if (stateTimer <= 0f)
//                    ChangeState(EnemyState.Recovery);
//                break;

//            case EnemyState.Recovery:
//                stateTimer -= Time.deltaTime;
//                if (stateTimer <= 0f)
//                    ChangeState(EnemyState.Chasing); // решение "атаковать снова?" — в Chasing, через кулдаун
//                break;

//            case EnemyState.Staggered:
//                stateTimer -= Time.deltaTime;
//                if (stateTimer <= 0f)
//                    ChangeState(EnemyState.Chasing);
//                break;
//        }
//    }

//    private void MoveTowardsPlayer()
//    {
//        Vector3 dir = Player.position - transform.position;
//        dir.y = 0f;

//        if (dir.sqrMagnitude < 0.01f)
//            return;

//        dir.Normalize();
//        transform.position += dir * moveSpeed * Time.deltaTime;
//        transform.rotation = Quaternion.LookRotation(dir);
//    }

//    private void FaceTarget()
//    {
//        Vector3 dir = Player.position - transform.position;
//        dir.y = 0f;

//        if (dir.sqrMagnitude > 0.01f)
//            transform.rotation = Quaternion.LookRotation(dir.normalized);
//    }

//    private void ChangeState(EnemyState newState)
//    {
//        if (state == EnemyState.Active && weaponHitbox != null)
//            weaponHitbox.SetActive(false);

//        // Патруль активен только в Idle, во всех остальных состояниях — выключен
//        if (patrol != null)
//            patrol.SetActive(newState == EnemyState.Idle);

//        state = newState;

//        switch (newState)
//        {
//            case EnemyState.Windup:
//                stateTimer = windupDuration;
//                FaceTarget();
//                SetColor(windupColor);
//                break;

//            case EnemyState.Active:
//                stateTimer = activeDuration;
//                if (weaponHitbox != null)
//                    weaponHitbox.SetActive(true);
//                SetColor(activeColor);
//                break;

//            case EnemyState.Recovery:
//                stateTimer = recoveryDuration;
//                attackCooldownTimer = Random.Range(attackCooldownMin, attackCooldownMax);
//                SetColor(idleColor);
//                break;

//            case EnemyState.Staggered:
//                stateTimer = staggerDuration;
//                if (weaponHitbox != null)
//                    weaponHitbox.SetActive(false);
//                SetColor(staggeredColor);
//                break;

//            case EnemyState.Stunned:
//                if (weaponHitbox != null)
//                    weaponHitbox.SetActive(false);
//                SetColor(stunnedColor);
//                break;

//            case EnemyState.Idle:
//            case EnemyState.Chasing:
//                SetColor(idleColor);
//                break;
//        }

//        Debug.Log($"{name} state: {state}");
//    }

//    private void SetColor(Color color)
//    {
//        if (bodyRenderer == null || bodyRenderer.sharedMaterial == null)
//            return;

//        bodyRenderer.GetPropertyBlock(propBlock);

//        if (bodyRenderer.sharedMaterial.HasProperty(BaseColorID))
//            propBlock.SetColor(BaseColorID, color);
//        else if (bodyRenderer.sharedMaterial.HasProperty(ColorID))
//            propBlock.SetColor(ColorID, color);

//        bodyRenderer.SetPropertyBlock(propBlock);
//    }

//    // Вызывается PlayerCombat.NotifyParrySuccess при успешном парировании.
//    public void GetStaggered()
//    {
//        Debug.Log($"{name} застаггерен парированием!");
//        ChangeState(EnemyState.Staggered);
//    }

//    // Вызывается EnemyHealth по событиям StaminaSystem.
//    public void ForceStun()
//    {
//        ChangeState(EnemyState.Stunned);
//    }

//    public void ForceRecoverFromStun()
//    {
//        if (state == EnemyState.Stunned)
//            ChangeState(EnemyState.Chasing);
//    }
//}
using System.Collections;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Windup,
        Active,
        Recovery,
        Block,
        ParryWindow,
        Staggered,
        Stunned
    }

    [Header("Восприятие / патруль")]
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyPatrol patrol;

    [Header("Профиль сложности")]
    [SerializeField] private EnemyDifficultyProfile difficultyProfile;

    [Header("Бой: дистанции и скорость")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Тайминги атаки (секунды)")]
    [SerializeField] private float windupDuration = 0.5f;
    [SerializeField] private float activeDuration = 0.2f;
    [SerializeField] private float recoveryDuration = 0.6f;
    [SerializeField] private float staggerDuration = 1f;

    [Header("Защита (тайминги)")]
    [SerializeField] private float parryWindowDuration = 0.35f;

    [Header("Частота атак (фолбэк, если нет профиля)")]
    [SerializeField] private float attackCooldownMin = 0.4f;
    [SerializeField] private float attackCooldownMax = 1.2f;

    [Header("Оружие")]
    [SerializeField] private WeaponHitbox weaponHitbox;

    [Header("Визуальный телеграф (временно, для тестов)")]
    [SerializeField] private MeshRenderer bodyRenderer;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color windupColor = Color.yellow;
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color blockColor = Color.blue;
    [SerializeField] private Color parryWindowColor = Color.cyan;
    [SerializeField] private Color staggeredColor = Color.gray;
    [SerializeField] private Color stunnedColor = new Color(0.2f, 0f, 0.3f);

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock propBlock;

    public EnemyState State => state;

    private EnemyState state = EnemyState.Idle;
    private float stateTimer;
    private float attackCooldownTimer;

    private PlayerCombat playerCombat;
    private bool hasReactedToCurrentWindup;

    private Transform Player => perception != null ? perception.Player : null;

    private float AttackCooldownMin => difficultyProfile != null ? difficultyProfile.attackCooldownMin : attackCooldownMin;
    private float AttackCooldownMax => difficultyProfile != null ? difficultyProfile.attackCooldownMax : attackCooldownMax;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        if (bodyRenderer == null)
            bodyRenderer = GetComponent<MeshRenderer>();

        if (patrol != null)
            patrol.SetActive(true);

        if (difficultyProfile == null)
            CombatLog.Info(name, "ВНИМАНИЕ: профиль сложности не задан — защитный ИИ (блок/парирование) отключён, используются фолбэк-кулдауны атаки");
    }

    private void Update()
    {
        if (perception == null || Player == null)
            return;

        if (playerCombat == null)
        {
            playerCombat = Player.GetComponent<PlayerCombat>();
            if (playerCombat == null)
                CombatLog.Info(name, "ВНИМАНИЕ: на игроке не найден PlayerCombat — защитный ИИ работать не будет");
        }

        TickState();
    }

    private void TickState()
    {
        if (state == EnemyState.Stunned)
            return;

        switch (state)
        {
            case EnemyState.Idle:
                if (perception.CanSeePlayer)
                    ChangeState(EnemyState.Chasing, "заметил игрока");
                break;

            case EnemyState.Chasing:
                if (!perception.CanSeePlayer)
                {
                    ChangeState(EnemyState.Idle, "потерял игрока из виду");
                    break;
                }

                TickDefensiveReaction();

                if (perception.DistanceToPlayer <= attackRange)
                {
                    attackCooldownTimer -= Time.deltaTime;
                    if (attackCooldownTimer <= 0f)
                        ChangeState(EnemyState.Windup, $"кулдаун атаки истёк (дистанция {perception.DistanceToPlayer:F1} <= {attackRange:F1})");
                }
                else
                {
                    MoveTowardsPlayer();
                }
                break;

            case EnemyState.Windup:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    ChangeState(EnemyState.Active, "windup закончился");
                break;

            case EnemyState.Active:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    ChangeState(EnemyState.Recovery, "окно атаки закрылось");
                break;

            case EnemyState.Recovery:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    ChangeState(EnemyState.Chasing, "recovery закончился");
                break;

            case EnemyState.ParryWindow:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    ChangeState(EnemyState.Block, "не поймал атаку в окно парирования, держим блок");
                break;

            case EnemyState.Block:
                if (playerCombat == null || playerCombat.State == PlayerCombat.CombatState.Idle)
                    ChangeState(EnemyState.Chasing, "атака игрока завершилась, блок больше не нужен");
                break;

            case EnemyState.Staggered:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    ChangeState(EnemyState.Chasing, "стаггер прошёл");
                break;
        }
    }

    private void TickDefensiveReaction()
    {
        if (playerCombat == null || difficultyProfile == null)
            return;

        bool playerIsAttacking = playerCombat.State == PlayerCombat.CombatState.AttackWindup;

        if (!playerIsAttacking)
        {
            hasReactedToCurrentWindup = false;
            return;
        }

        if (hasReactedToCurrentWindup)
            return;

        hasReactedToCurrentWindup = true;

        float roll = Random.value;
        float parryThreshold = difficultyProfile.parryChance;
        float blockThreshold = parryThreshold + difficultyProfile.blockChance;

        CombatLog.Decision(name, $"игрок начал windup рядом — бросок {roll:F2} (parry < {parryThreshold:F2}, block < {blockThreshold:F2})");

        if (roll < parryThreshold)
        {
            CombatLog.Decision(name, $"решил ПАРИРОВАТЬ, реакция через {difficultyProfile.reactionTime:F2}с");
            StartCoroutine(ReactAfterDelay(EnemyState.ParryWindow));
        }
        else if (roll < blockThreshold)
        {
            CombatLog.Decision(name, $"решил ЗАБЛОКИРОВАТЬ, реакция через {difficultyProfile.reactionTime:F2}с");
            StartCoroutine(ReactAfterDelay(EnemyState.Block));
        }
        else
        {
            CombatLog.Decision(name, "решил НЕ защищаться — примет удар как есть");
        }
    }

    private IEnumerator ReactAfterDelay(EnemyState defenseState)
    {
        yield return new WaitForSeconds(difficultyProfile.reactionTime);

        if (state != EnemyState.Chasing || playerCombat.State != PlayerCombat.CombatState.AttackWindup)
        {
            CombatLog.Decision(name, $"реакция ({defenseState}) отменена — ситуация изменилась (state={state}, playerState={playerCombat.State})");
            yield break;
        }

        ChangeState(defenseState, "реакция ИИ сработала");
    }

    private void MoveTowardsPlayer()
    {
        Vector3 dir = Player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        dir.Normalize();
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void FaceTarget()
    {
        Vector3 dir = Player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    private void ChangeState(EnemyState newState, string reason = null)
    {
        if (state == EnemyState.Active && weaponHitbox != null)
            weaponHitbox.SetActive(false);

        if (patrol != null)
            patrol.SetActive(newState == EnemyState.Idle);

        EnemyState oldState = state;
        state = newState;

        switch (newState)
        {
            case EnemyState.Windup:
                stateTimer = windupDuration;
                FaceTarget();
                SetColor(windupColor);
                break;

            case EnemyState.Active:
                stateTimer = activeDuration;
                if (weaponHitbox != null)
                    weaponHitbox.SetActive(true);
                SetColor(activeColor);
                break;

            case EnemyState.Recovery:
                stateTimer = recoveryDuration;
                attackCooldownTimer = Random.Range(AttackCooldownMin, AttackCooldownMax);
                SetColor(idleColor);
                CombatLog.Info(name, $"следующая атака не раньше чем через {attackCooldownTimer:F2}с");
                break;

            case EnemyState.ParryWindow:
                stateTimer = parryWindowDuration;
                FaceTarget();
                SetColor(parryWindowColor);
                break;

            case EnemyState.Block:
                FaceTarget();
                SetColor(blockColor);
                break;

            case EnemyState.Staggered:
                stateTimer = staggerDuration;
                if (weaponHitbox != null)
                    weaponHitbox.SetActive(false);
                SetColor(staggeredColor);
                break;

            case EnemyState.Stunned:
                if (weaponHitbox != null)
                    weaponHitbox.SetActive(false);
                SetColor(stunnedColor);
                break;

            case EnemyState.Idle:
            case EnemyState.Chasing:
                SetColor(idleColor);
                break;
        }

        CombatLog.State(name, oldState.ToString(), newState.ToString(), reason);
    }

    private void SetColor(Color color)
    {
        if (bodyRenderer == null || bodyRenderer.sharedMaterial == null)
            return;

        bodyRenderer.GetPropertyBlock(propBlock);

        if (bodyRenderer.sharedMaterial.HasProperty(BaseColorID))
            propBlock.SetColor(BaseColorID, color);
        else if (bodyRenderer.sharedMaterial.HasProperty(ColorID))
            propBlock.SetColor(ColorID, color);

        bodyRenderer.SetPropertyBlock(propBlock);
    }

    public void GetStaggered()
    {
        CombatLog.Decision(name, "застаггерен успешным парированием игрока!");
        ChangeState(EnemyState.Staggered, "парирование игрока");
    }

    public void ForceStun()
    {
        ChangeState(EnemyState.Stunned, "стамина заполнена");
    }

    public void ForceRecoverFromStun()
    {
        if (state == EnemyState.Stunned)
            ChangeState(EnemyState.Chasing, "стан закончился");
    }
}