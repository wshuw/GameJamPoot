using UnityEngine;

// Профиль сложности врага. Один ассет = один "характер" поведения.
// Создаёшь через правый клик в Project -> Create -> Combat -> Enemy Difficulty
// Profile, делаешь Easy/Medium/Hard и перетаскиваешь нужный на конкретного
// врага в инспекторе EnemyCombat. Никакого кода менять не нужно.
[CreateAssetMenu(fileName = "EnemyDifficultyProfile", menuName = "Combat/Enemy Difficulty Profile")]
public class EnemyDifficultyProfile : ScriptableObject
{
    [Header("Частота атак")]
    [Tooltip("После Recovery враг ждёт случайное время в этом диапазоне, прежде чем атаковать снова")]
    public float attackCooldownMin = 0.4f;
    public float attackCooldownMax = 1.2f;

    [Header("Защита")]
    [Range(0f, 1f)]
    [Tooltip("Шанс, что при виде windup игрока враг попытается ПАРИРОВАТЬ")]
    public float parryChance = 0.2f;

    [Range(0f, 1f)]
    [Tooltip("Шанс, что враг попытается ЗАБЛОКИРОВАТЬ (проверяется, если парирование не выпало). " +
             "parryChance + blockChance не должно превышать 1 — остаток шанса = враг просто примет удар")]
    public float blockChance = 0.3f;

    [Tooltip("Задержка реакции (сек) между моментом, когда враг замечает windup игрока, и началом своей защиты")]
    public float reactionTime = 0.15f;
}