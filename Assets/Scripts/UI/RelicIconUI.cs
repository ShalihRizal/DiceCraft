using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RelicIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    private RelicData relicData;

    public void Setup(RelicData data)
    {
        relicData = data;
        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
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
