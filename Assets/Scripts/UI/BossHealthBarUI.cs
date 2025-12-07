using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject healthBarPanel;
    public Image healthFillImage; // Assign "Fill" from Slider or just use Slider component
    public Slider healthSlider; // New: Direct slider reference
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI healthText;
    public Transform traitContainer; // New: For trait icons
    public GameObject traitPrefab; // New: Prefab to spawn

    private float maxHealth;

    public void Setup(Enemy boss)
    {
        if (boss == null || boss.enemyData == null) return;
        
        EnemyData data = boss.enemyData;
        this.maxHealth = boss.health; 
        
        if (healthBarPanel != null) healthBarPanel.SetActive(true);
        if (bossNameText != null) bossNameText.text = data.enemyName;
        
        // Traits
        if (traitContainer != null && traitPrefab != null)
        {
            // Clear existing
            foreach (Transform child in traitContainer) Destroy(child.gameObject);
            
            // Add new
            foreach (var trait in data.traits)
            {
                GameObject go = Instantiate(traitPrefab, traitContainer);
                TraitUI ui = go.GetComponent<TraitUI>();
                if (ui == null) ui = go.AddComponent<TraitUI>();
                ui.Setup(trait);
            }
        }
        
        UpdateHealth(boss.health);
    }

    public void UpdateHealth(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.DOValue(currentHealth / maxHealth, 0.5f).SetEase(Ease.OutCubic);
        }
        else if (healthFillImage != null)
        {
            healthFillImage.DOFillAmount(currentHealth / maxHealth, 0.5f).SetEase(Ease.OutCubic);
        }


        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    public void Hide()
    {
        if (healthBarPanel != null) healthBarPanel.SetActive(false);
    }

    public void ShowImmune()
    {
        if (healthText != null) healthText.text = "IMMUNE";
        if (healthBarPanel != null) healthBarPanel.transform.DOShakePosition(0.5f, 5f);
    }

    private void OnDestroy()
    {
        if (healthSlider != null) healthSlider.DOKill();
        if (healthFillImage != null) healthFillImage.DOKill();
    }
}
