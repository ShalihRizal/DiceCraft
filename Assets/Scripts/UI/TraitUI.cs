using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles individual trait icons in the Boss Health Bar UI
/// Shows tooltip on hover
/// </summary>
public class TraitUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EnemyTrait trait;
    private Image iconImage;

    public void Setup(EnemyTrait trait)
    {
        this.trait = trait;
        
        // Get Image component (assume it exists on the same object)
        iconImage = GetComponent<Image>();
        if (iconImage == null)
            iconImage = gameObject.AddComponent<Image>();
            
        // Set icon
        if (trait != null && trait.icon != null)
        {
            iconImage.sprite = trait.icon;
            iconImage.color = Color.white;
        }
        else
        {
            // Fallback or hide if no icon
            iconImage.color = Color.clear;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (trait != null && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(trait, transform.position);
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
