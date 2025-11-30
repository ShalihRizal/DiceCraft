using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI descriptionText;
    public Button buyButton;

    private ShopItem item;
    private ShopManager shop;

    public void Setup(ShopItem item, ShopManager manager)
    {
        this.item = item;
        this.shop = manager;

        descriptionText.text = item.description;
        
        string costText = $"{item.cost}g";
        if (item.isOnSale)
        {
            costText = $"<color=red><s>{item.originalCost}g</s></color> <color=green>{item.cost}g</color>";
        }
        
        // Assuming button has a text child
        Text btnText = buyButton.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            btnText.text = $"Buy ({costText})";
        }
        else
        {
            // Try TMP
            TextMeshProUGUI tmpText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null) tmpText.text = $"Buy ({costText})";
        }
        
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => shop.AttemptPurchase(item));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null || TooltipManager.Instance == null) return;

        if (item.itemType == ShopItemType.Dice && item.diceData != null)
        {
            TooltipManager.Instance.ShowTooltip(item.diceData, transform.position);
        }
        else if (item.itemType == ShopItemType.Relic && item.relicData != null)
        {
            TooltipManager.Instance.ShowTooltip(item.relicData, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    void OnDisable()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
