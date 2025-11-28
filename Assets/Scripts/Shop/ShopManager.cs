using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public DicePool dicePool;
    public List<RelicData> relicPool; // Pool of relics to sell
    
    public Transform diceContainer;
    public Transform relicContainer;
    public Transform serviceContainer;
    
    public GameObject shopItemUIPrefab;
    // public GameObject serviceButtonPrefab; // Prefab for Heal/Gamble buttons (using code for now)

    public int rerollCost = 50;
    public int healCost = 30;
    public int gambleCost = 150;

    private List<ShopItem> diceOffers = new List<ShopItem>();
    private List<ShopItem> relicOffers = new List<ShopItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (shopItemUIPrefab == null) return;
        
        // Initial Generation
        GenerateDiceOffers();
        GenerateRelicOffers();
        
        HideShop();
    }

    public void ShowShop()
    {
        shopUIContainer.gameObject.SetActive(true);
        DisplayShop();
    }

    public void GenerateDiceOffers()
    {
        diceOffers.Clear();
        for (int i = 0; i < 3; i++)
        {
            DiceData dice = dicePool.GetRandomDice();
            ShopItem item = new ShopItem(dice);
            ApplySale(item);
            diceOffers.Add(item);
        }
    }

    public void GenerateRelicOffers()
    {
        relicOffers.Clear();
        if (relicPool == null || relicPool.Count == 0) return;

        for (int i = 0; i < 2; i++)
        {
            RelicData relic = relicPool[Random.Range(0, relicPool.Count)];
            ShopItem item = new ShopItem(relic);
            ApplySale(item);
            relicOffers.Add(item);
        }
    }
    
    private void ApplySale(ShopItem item)
    {
        // 20% chance for 25% off
        if (Random.value < 0.2f)
        {
            item.isOnSale = true;
            item.cost = Mathf.RoundToInt(item.cost * 0.75f);
            item.description += "\n<color=green>SALE!</color>";
        }
    }

    public void DisplayShop()
    {
        // Clear containers
        // Note: shopUIContainer is the main parent. We need to find or create sub-containers if they don't exist.
        // For simplicity, let's assume shopUIContainer has a VerticalLayoutGroup.
        // We will create "Row" objects for Dice, Relics, Services.
        
        foreach (Transform child in shopUIContainer) Destroy(child.gameObject);

        // 1. Dice Row
        CreateSectionHeader("Dice");
        GameObject diceRow = CreateRow("DiceRow");
        foreach (var item in diceOffers)
        {
            GameObject go = Instantiate(shopItemUIPrefab, diceRow.transform);
            ShopItemUI ui = go.GetComponent<ShopItemUI>();
            ui.Setup(item, this);
        }

        // 2. Relic Row
        CreateSectionHeader("Relics");
        GameObject relicRow = CreateRow("RelicRow");
        foreach (var item in relicOffers)
        {
            GameObject go = Instantiate(shopItemUIPrefab, relicRow.transform);
            ShopItemUI ui = go.GetComponent<ShopItemUI>();
            ui.Setup(item, this);
        }
        
        // 3. Services Row
        CreateSectionHeader("Services");
        GameObject serviceRow = CreateRow("ServiceRow");
        
        CreateServiceButton(serviceRow.transform, $"Heal ({healCost}g)", healCost, HealPlayer);
        CreateServiceButton(serviceRow.transform, $"Gamble Dice ({gambleCost}g)", gambleCost, GambleDice);
        CreateServiceButton(serviceRow.transform, $"Reroll Dice ({rerollCost}g)", rerollCost, RerollDice);
        CreateServiceButton(serviceRow.transform, $"Reroll Relics ({rerollCost}g)", rerollCost, RerollRelics);
        
        // Leave Button
        CreateLeaveButton();
    }
    
    private void CreateSectionHeader(string title)
    {
        GameObject textObj = new GameObject(title);
        textObj.transform.SetParent(shopUIContainer, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = title;
        text.alignment = TextAlignmentOptions.Left;
        text.color = Color.yellow;
        text.fontSize = 24;
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
    }
    
    private GameObject CreateRow(string name)
    {
        GameObject row = new GameObject(name);
        row.transform.SetParent(shopUIContainer, false);
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 150); // Height for items
        return row;
    }
    
    private void CreateServiceButton(Transform parent, string label, int cost, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(label);
        btnObj.transform.SetParent(parent, false);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f);
        
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            if (PlayerCurrency.Instance.SpendGold(cost))
            {
                action.Invoke();
            }
            else
            {
                Debug.Log("Not enough gold!");
            }
        });
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 18;
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 50);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void CreateLeaveButton()
    {
        GameObject btnObj = new GameObject("LeaveButton");
        btnObj.transform.SetParent(shopUIContainer, false);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.red;
        
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(CloseShop);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Leave Shop";
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 18;
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 50);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    public void HealPlayer()
    {
        // Assuming PlayerHealth exists
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            health.Heal(10); // Fixed heal amount for now
            Debug.Log("Player Healed!");
        }
    }
    
    public void GambleDice()
    {
        DiceData dice = dicePool.GetRandomDice(); // Should be weighted in DicePool
        // Spawn it immediately
        var diceSpawner = Object.FindFirstObjectByType<DiceSpawner>();
        if (diceSpawner != null)
        {
            diceSpawner.TrySpawnSpecificDice(dice);
            Debug.Log($"Gambled and got: {dice.diceName}");
        }
    }
    
    public void RerollDice()
    {
        GenerateDiceOffers();
        DisplayShop();
    }
    
    public void RerollRelics()
    {
        GenerateRelicOffers();
        DisplayShop();
    }

    public void AttemptPurchase(ShopItem item)
    {
        if (PlayerCurrency.Instance.CurrentGold < item.cost)
        {
            Debug.Log("❌ Not enough gold!");
            return;
        }

        bool success = false;

        if (item.itemType == ShopItemType.Dice)
        {
            var diceSpawner = Object.FindFirstObjectByType<DiceSpawner>();
            Dice spawnedDice = diceSpawner != null ? diceSpawner.TrySpawnSpecificDice(item.diceData) : null;
            if (spawnedDice != null)
            {
                spawnedDice.PlayVFX(VFXType.Bought);
                success = true;
            }
        }
        else if (item.itemType == ShopItemType.Relic)
        {
            if (RelicManager.Instance != null)
            {
                RelicManager.Instance.AddRelic(item.relicData);
                success = true;
            }
        }

        if (success)
        {
            PlayerCurrency.Instance.SpendGold(item.cost);
            if (item.itemType == ShopItemType.Dice) diceOffers.Remove(item);
            else if (item.itemType == ShopItemType.Relic) relicOffers.Remove(item);
            DisplayShop();
        }
    }

    public Transform shopUIContainer; // Main container

    public void HideShop()
    {
        shopUIContainer.gameObject.SetActive(false);
    }

    public void CloseShop()
    {
        HideShop();
        if (MapManager.Instance != null)
        {
            MapManager.Instance.CompleteCurrentNode();
        }
    }
}
