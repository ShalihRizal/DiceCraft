using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
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
}
