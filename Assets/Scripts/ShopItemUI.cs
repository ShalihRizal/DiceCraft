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
        buyButton.onClick.AddListener(() => shop.AttemptPurchase(item));
    }
}
