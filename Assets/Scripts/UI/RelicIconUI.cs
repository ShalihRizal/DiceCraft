using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RelicIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public Image backgroundImage;
    public Image borderImage;
    public TextMeshProUGUI counterText; // For progress display (e.g., "3/6")
    
    [System.Serializable]
    public struct RarityTheme
    {
        public RelicRarity rarity;
        public Sprite background;
        public Sprite border;
    }

    public System.Collections.Generic.List<RarityTheme> rarityThemes;

    private RelicData relicData;

    public void Setup(RelicData data)
    {
        relicData = data;
        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
        }

        // Apply Rarity Visuals
        if (rarityThemes != null && rarityThemes.Count > 0)
        {
            var theme = rarityThemes.Find(t => t.rarity == data.rarity);
            
            // If theme found (or default/fallback logic if you want)
            if (backgroundImage != null) backgroundImage.sprite = theme.background;
            if (borderImage != null) borderImage.sprite = theme.border;
        }

        UpdateCounterText();
    }

    public void UpdateCounterText()
    {
        if (counterText == null || relicData == null) return;

        // Check if this is the Duplicator relic
        if (relicData.relicName.Contains("Duplicator") && RelicManager.Instance != null)
        {
            counterText.text = RelicManager.Instance.GetDuplicatorProgress();
            counterText.gameObject.SetActive(true);
        }
        else
        {
            // Hide counter for relics that don't use it
            counterText.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (relicData != null && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(relicData, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
