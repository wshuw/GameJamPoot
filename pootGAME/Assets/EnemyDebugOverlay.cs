using UnityEngine;

// ВРЕМЕННЫЙ дебаг-оверлей для тестирования боёвки без моделек/анимаций.
// Рисует над головой врага: полоску ХП, полоску стамины, видит ли игрока,
// и текущее боевое состояние. Работает через OnGUI — Canvas/TMP не нужны.
// Когда появится нормальный UI/модельки — просто удали/выключи этот
// компонент на врагах.
[RequireComponent(typeof(EnemyHealth))]
public class EnemyDebugOverlay : MonoBehaviour
{
    [Header("Ссылки (если не заданы — берутся с этого же объекта)")]
    [SerializeField] private EnemyHealth health;
    [SerializeField] private StaminaSystem stamina;
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyCombat combat;

    [Header("Настройки отображения")]
    [SerializeField] private float heightOffset = 2.2f;
    [SerializeField] private float barWidth = 1000f; // Увеличено под массивный шрифт
    [SerializeField] private float barHeight = 120f;  // Увеличено под массивный шрифт
    [Tooltip("Дальше этой дистанции от камеры панель не рисуется (0 = без ограничения)")]
    [SerializeField] private float maxDrawDistance = 30f;

    private Camera cam;
    private GUIStyle largeTextStyle;

    private void Awake()
    {
        if (health == null) health = GetComponent<EnemyHealth>();
        if (stamina == null) stamina = GetComponent<StaminaSystem>();
        if (perception == null) perception = GetComponent<EnemyPerception>();
        if (combat == null) combat = GetComponent<EnemyCombat>();
    }

    private void InitStyle()
    {
        if (largeTextStyle == null)
        {
            largeTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 130, // Увеличено в 10 раз (стандартный размер Unity OnGUI ~13pt)
                fontStyle = FontStyle.Bold
            };
        }
    }

    private void OnGUI()
    {
        InitStyle();

        if (cam == null) cam = Camera.main;
        if (cam == null || health == null) return;

        Vector3 worldPos = transform.position + Vector3.up * heightOffset;
        Vector3 toCam = cam.transform.position - worldPos;

        if (maxDrawDistance > 0f && toCam.sqrMagnitude > maxDrawDistance * maxDrawDistance)
            return;

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        if (screenPos.z <= 0f) return; // враг за спиной камеры

        float x = screenPos.x - barWidth * 0.5f;
        float y = Screen.height - screenPos.y; // GUI-координаты Y перевёрнуты относительно экрана

        largeTextStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y - 340f, barWidth, 160f), name, largeTextStyle);

        DrawBar(x, y - 180f, health.CurrentHealth, health.MaxHealth, Color.red);

        if (stamina != null)
            DrawBar(x, y - 40f, stamina.Current, stamina.Max, Color.yellow);

        string seeInfo = perception != null ? (perception.CanSeePlayer ? "ВИДИТ игрока" : "не видит") : "?";
        string stateInfo = combat != null ? combat.State.ToString() : "?";

        largeTextStyle.normal.textColor = perception != null && perception.CanSeePlayer ? Color.green : Color.gray;
        GUI.Label(new Rect(x, y + 100f, barWidth, 160f), seeInfo, largeTextStyle);

        largeTextStyle.normal.textColor = Color.cyan;
        GUI.Label(new Rect(x, y + 260f, barWidth, 160f), stateInfo, largeTextStyle);

        GUI.color = Color.white;
    }

    private void DrawBar(float x, float y, float current, float max, Color fillColor)
    {
        float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), Texture2D.whiteTexture);

        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(x, y, barWidth * ratio, barHeight), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }
}