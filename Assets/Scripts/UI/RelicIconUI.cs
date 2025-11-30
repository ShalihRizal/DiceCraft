using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RelicIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public Image borderImage; // Border to show rarity
    public Image backgroundImage; // Background glow
    
    private RelicData relicData;
    private Vector3 originalScale;
    private Tween hoverTween;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Setup(RelicData data)
    {
        relicData = data;
        
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }

        // Apply rarity-based styling
        ApplyRarityStyle(data.rarity);
    }

    private void ApplyRarityStyle(RelicRarity rarity)
    {
        Color rarityColor = GetRarityColor(rarity);

        // Apply border color
        if (borderImage != null)
        {
            borderImage.color = rarityColor;
        }

        // Apply subtle background glow
        if (backgroundImage != null)
        {
            Color glowColor = rarityColor;
            glowColor.a = 0.2f; // Subtle transparency
            backgroundImage.color = glowColor;
        }
    }

    private Color GetRarityColor(RelicRarity rarity)
    {
        return rarity switch
        {
            RelicRarity.Common => new Color(0.7f, 0.7f, 0.7f), // Gray
            RelicRarity.Rare => new Color(0.4f, 0.6f, 1f),    // Blue
            RelicRarity.Legendary => new Color(1f, 0.75f, 0f), // Gold
            _ => Color.white
        };
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Scale up on hover
        if (hoverTween != null) hoverTween.Kill();
        hoverTween = transform.DOScale(originalScale * 1.15f, 0.2f).SetEase(Ease.OutBack);

        // Show tooltip
        if (relicData != null && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(relicData, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale back down
        if (hoverTween != null) hoverTween.Kill();
        hoverTween = transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);

        // Hide tooltip
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        // Clean up tweens
        if (hoverTween != null)
        {
            hoverTween.Kill();
            transform.localScale = originalScale;
        }

        // Hide tooltip
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
