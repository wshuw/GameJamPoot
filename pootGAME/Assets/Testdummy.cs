using System.Collections;
using UnityEngine;

public class TestDummy : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Flash Settings")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("Manual Reference (Optional)")]
    [SerializeField] private MeshRenderer cubeRenderer;

    private float currentHealth;
    private Color originalColor;
    private Coroutine flashRoutine;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        currentHealth = maxHealth;
        propBlock = new MaterialPropertyBlock();

        if (cubeRenderer == null)
            cubeRenderer = GetComponent<MeshRenderer>();

        if (cubeRenderer != null && cubeRenderer.sharedMaterial != null)
        {
            if (cubeRenderer.sharedMaterial.HasProperty(BaseColorID))
                originalColor = cubeRenderer.sharedMaterial.GetColor(BaseColorID);
            else if (cubeRenderer.sharedMaterial.HasProperty(ColorID))
                originalColor = cubeRenderer.sharedMaterial.GetColor(ColorID);
            else
                originalColor = Color.white;
        }
        else
        {
            Debug.LogWarning($"[TestDummy] MeshRenderer не найден на объекте {gameObject.name}");
        }
    }

    // TestDummy Ч просто мишень, стамину/блок не считает, берЄт только hpDamage.
    public void TakeDamage(AttackData attack, GameObject attacker)
    {
        currentHealth -= attack.hpDamage;
        Debug.Log($"{name} получил {attack.hpDamage} урона от {(attacker != null ? attacker.name : "неизвестно")}. HP: {Mathf.Max(currentHealth, 0f)}/{maxHealth}");

        if (cubeRenderer != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        if (currentHealth <= 0f)
            Die();
    }

    private IEnumerator FlashRoutine()
    {
        SetColor(hitFlashColor);
        yield return new WaitForSeconds(hitFlashDuration);
        SetColor(originalColor);
    }

    private void SetColor(Color color)
    {
        cubeRenderer.GetPropertyBlock(propBlock);

        if (cubeRenderer.sharedMaterial.HasProperty(BaseColorID))
            propBlock.SetColor(BaseColorID, color);
        else if (cubeRenderer.sharedMaterial.HasProperty(ColorID))
            propBlock.SetColor(ColorID, color);

        cubeRenderer.SetPropertyBlock(propBlock);
    }

    private void Die()
    {
        Debug.Log($"{name} уничтожен.");
        currentHealth = maxHealth;
    }
}