using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Здоровье")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthCatchupFill;
    [SerializeField] private float healthCatchupSpeed = 0.8f;
    [SerializeField] private float healthCatchupDelay = 0.5f;

    [Header("Стамина (Sekiro Style Width & Fade)")]
    [SerializeField] private CanvasGroup staminaCanvasGroup; // Компонент Canvas Group на объекте Stamina
    [SerializeField] private RectTransform staminaFillRect;   // RectTransform объекта Fill
    [SerializeField] private Image staminaFillImage;         // Image объекта Fill

    [Header("Параметры Стамины")]
    [SerializeField] private float maxStaminaWidth = 622f;    // Максимальная ширина при 100%
    [SerializeField] private float staminaFadeSpeed = 3f;      // Скорость проявления/исчезновения
    [SerializeField] private float staminaHideDelay = 2f;      // Задержка перед исчезновением после обнуления

    [Header("Цвета Стамины")]
    [SerializeField] private Color staminaEmptyColor = new Color(1f, 0.8f, 0f, 1f); // Желтый в начале
    [SerializeField] private Color staminaFullColor = new Color(1f, 0.1f, 0f, 1f);  // Красный при заполнении

    private float healthDelayTimer;
    private float staminaHideTimer;
    private float targetStaminaAlpha = 0f;

    private void Update()
    {
        UpdateHealthCatchup();
        UpdateStaminaFade();
    }

    private void UpdateHealthCatchup()
    {
        if (healthCatchupFill == null) return;

        if (healthDelayTimer > 0f)
        {
            healthDelayTimer -= Time.deltaTime;
            return;
        }

        if (healthCatchupFill.fillAmount > healthFill.fillAmount)
        {
            healthCatchupFill.fillAmount = Mathf.MoveTowards(
                healthCatchupFill.fillAmount,
                healthFill.fillAmount,
                healthCatchupSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateStaminaFade()
    {
        if (staminaCanvasGroup == null) return;

        // Таймер удержания UI видимым после того, как стамина спала до 0
        if (staminaHideTimer > 0f)
        {
            staminaHideTimer -= Time.deltaTime;
            targetStaminaAlpha = 1f;
        }

        // Плавная альфа (появление / исчезновение)
        staminaCanvasGroup.alpha = Mathf.MoveTowards(
            staminaCanvasGroup.alpha,
            targetStaminaAlpha,
            staminaFadeSpeed * Time.deltaTime
        );
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f || healthFill == null) return;

        float newFill = Mathf.Clamp01(currentHealth / maxHealth);

        if (newFill < healthFill.fillAmount)
            healthDelayTimer = healthCatchupDelay;

        healthFill.fillAmount = newFill;

        if (healthCatchupFill != null && newFill > healthCatchupFill.fillAmount)
            healthCatchupFill.fillAmount = newFill;
    }

    public void UpdateStamina(float currentStamina, float maxStamina)
    {
        if (maxStamina <= 0f || staminaFillRect == null) return;

        float fillFraction = Mathf.Clamp01(currentStamina / maxStamina);

        // 1. Изменяем ширину (Width) от 0 до 622
        staminaFillRect.sizeDelta = new Vector2(fillFraction * maxStaminaWidth, staminaFillRect.sizeDelta.y);

        // 2. Смена цвета: от желтого (0%) к красному (100%)
        if (staminaFillImage != null)
        {
            staminaFillImage.color = Color.Lerp(staminaEmptyColor, staminaFullColor, fillFraction);
        }

        // 3. Управление видимостью: если стамина > 0 — показываем, если 0 — скрываем с задержкой
        if (fillFraction > 0f)
        {
            targetStaminaAlpha = 1f;
            staminaHideTimer = staminaHideDelay;
        }
        else if (staminaHideTimer <= 0f)
        {
            targetStaminaAlpha = 0f;
        }
    }
}