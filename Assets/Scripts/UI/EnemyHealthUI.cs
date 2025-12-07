using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject canvasGO;
    public Image fillImage;
    public UnityEngine.UI.Slider healthSlider; // Add Slider support
    public TMPro.TextMeshProUGUI healthText;
    
    // Optional: Offset for the health bar
    public Vector3 offset = new Vector3(0, 0.8f, 0);

    private float maxHealth;

    public void Setup(float maxHealth)
    {
        this.maxHealth = maxHealth;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        // If canvas is assigned, ensure it's positioned correctly
        if (canvasGO != null)
        {
            UpdateHealth(maxHealth);
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        if (fillImage != null)
        {
            float fill = currentHealth / maxHealth;
            fillImage.DOFillAmount(fill, 0.3f).SetEase(Ease.OutQuad);
        }
        
        if (healthSlider != null)
        {
            healthSlider.DOValue(currentHealth, 0.3f).SetEase(Ease.OutQuad);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }

    public void ShowImmune()
    {
        if (healthText != null) healthText.text = "IMMUNE";
        // Optional: Shake or flash effect
        if (fillImage != null) fillImage.transform.DOShakePosition(0.5f, 2f);
    }

    private void OnDestroy()
    {
        // Prevent DOTween errors if destroyed while animating
        if (fillImage != null) fillImage.DOKill();
        if (healthSlider != null) healthSlider.DOKill();
    }
}
