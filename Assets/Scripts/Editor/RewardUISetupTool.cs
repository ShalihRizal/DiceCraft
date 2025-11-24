using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class RewardUISetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Reward UI")]
    public static void Setup()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found! Please create a Canvas first.");
            return;
        }

        // 1. Setup RewardManager
        RewardManager manager = FindFirstObjectByType<RewardManager>();
        if (manager == null)
        {
            GameObject managerGO = new GameObject("RewardManager");
            manager = managerGO.AddComponent<RewardManager>();
            Undo.RegisterCreatedObjectUndo(managerGO, "Create RewardManager");
        }

        // 2. Setup RewardPanel
        RewardUI rewardUI = FindFirstObjectByType<RewardUI>();
        GameObject panelGO;
        
        if (rewardUI == null)
        {
            panelGO = new GameObject("RewardPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvas.transform, false);
            rewardUI = panelGO.AddComponent<RewardUI>();
            Undo.RegisterCreatedObjectUndo(panelGO, "Create RewardPanel");
        }
        else
        {
            panelGO = rewardUI.gameObject;
        }

        // Configure Panel
        Image panelImg = panelGO.GetComponent<Image>();
        if (panelImg == null) panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.95f);
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        rewardUI.panel = panelGO;
        manager.rewardUI = rewardUI;

        // 3. Setup Cards Container
        Transform container = null;
        if (rewardUI.cardsContainer != null) container = rewardUI.cardsContainer;
        
        if (container == null) container = panelGO.transform.Find("CardsContainer");

        if (container == null)
        {
            GameObject containerGO = new GameObject("CardsContainer", typeof(RectTransform));
            containerGO.transform.SetParent(panelGO.transform, false);
            container = containerGO.transform;
            Undo.RegisterCreatedObjectUndo(containerGO, "Create CardsContainer");
        }

        RectTransform containerRect = container.GetComponent<RectTransform>();
        if (containerRect == null) containerRect = container.gameObject.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0.2f);
        containerRect.anchorMax = new Vector2(1, 0.8f);
        containerRect.offsetMin = new Vector2(50, 0);
        containerRect.offsetMax = new Vector2(-50, 0);

        HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = container.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 50;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        rewardUI.cardsContainer = container;

        // 4. Setup Card Prefab
        Transform prefabTrans = panelGO.transform.Find("PerkCardPrefab");
        GameObject cardPrefab;
        if (prefabTrans == null)
        {
            cardPrefab = new GameObject("PerkCardPrefab", typeof(RectTransform));
            cardPrefab.transform.SetParent(panelGO.transform, false);
            Undo.RegisterCreatedObjectUndo(cardPrefab, "Create PerkCardPrefab");
        }
        else
        {
            cardPrefab = prefabTrans.gameObject;
        }

        SetupCardPrefab(cardPrefab);
        rewardUI.cardPrefab = cardPrefab;
        cardPrefab.SetActive(false);

        // 5. Setup Toggle Buttons
        SetupToggleButtons(panelGO, canvas.gameObject, rewardUI);

        // 6. Assign Perks Database (Try to load all perks)
        string[] perkGuids = AssetDatabase.FindAssets("t:PerkData");
        manager.allPerks = new System.Collections.Generic.List<PerkData>();
        foreach (var guid in perkGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PerkData perk = AssetDatabase.LoadAssetAtPath<PerkData>(path);
            if (perk != null) manager.allPerks.Add(perk);
        }

        Selection.activeGameObject = panelGO;
        Debug.Log("✅ Reward UI Setup Complete!");
    }

    static void SetupCardPrefab(GameObject go)
    {
        // Background
        Image bg = go.GetComponent<Image>();
        if (bg == null) bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f); // Dark Grey

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220, 320);

        PerkCardUI cardUI = go.GetComponent<PerkCardUI>();
        if (cardUI == null) cardUI = go.AddComponent<PerkCardUI>();

        // Name
        TextMeshProUGUI nameText = CreateText(go, "NameText", "Perk Name", 20, new Vector2(0, 120), new Vector2(200, 40));
        nameText.fontStyle = FontStyles.Bold;
        cardUI.nameText = nameText;

        // Icon
        GameObject iconGO = GetChild(go, "Icon");
        Image iconImg = iconGO.GetComponent<Image>();
        if (iconImg == null) iconImg = iconGO.AddComponent<Image>();
        iconImg.color = Color.white;
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(80, 80);
        iconRect.anchoredPosition = new Vector2(0, 40);
        cardUI.iconImage = iconImg;

        // Description
        TextMeshProUGUI descText = CreateText(go, "DescText", "Description goes here...", 14, new Vector2(0, -40), new Vector2(200, 80));
        cardUI.descText = descText;

        // Select Button
        GameObject btnGO = GetChild(go, "SelectButton");
        Image btnImg = btnGO.GetComponent<Image>();
        if (btnImg == null) btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.8f, 0.2f); // Green
        
        Button btn = btnGO.GetComponent<Button>();
        if (btn == null) btn = btnGO.AddComponent<Button>();
        
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(180, 40);
        btnRect.anchoredPosition = new Vector2(0, -120);

        TextMeshProUGUI btnText = CreateText(btnGO, "Text", "SELECT", 18, Vector2.zero, new Vector2(180, 40));
        
        cardUI.selectButton = btn;
    }

    static void SetupToggleButtons(GameObject panel, GameObject canvas, RewardUI rewardUI)
    {
        // Minimize Button (Inside Panel)
        GameObject minBtnGO = GetChild(panel, "MinimizeButton");
        RectTransform minRect = minBtnGO.GetComponent<RectTransform>();
        if (minRect == null) minRect = minBtnGO.AddComponent<RectTransform>();
        
        minRect.anchorMin = new Vector2(1, 1);
        minRect.anchorMax = new Vector2(1, 1);
        minRect.pivot = new Vector2(1, 1);
        minRect.anchoredPosition = new Vector2(-20, -20);
        minRect.sizeDelta = new Vector2(100, 40);

        Image minImg = minBtnGO.GetComponent<Image>();
        if (minImg == null) minImg = minBtnGO.AddComponent<Image>();
        minImg.color = Color.gray;

        Button minBtn = minBtnGO.GetComponent<Button>();
        if (minBtn == null) minBtn = minBtnGO.AddComponent<Button>();

        CreateText(minBtnGO, "Text", "Minimize", 16, Vector2.zero, new Vector2(100, 40));
        rewardUI.minimizeButton = minBtn;

        // Open Button (Outside Panel, on Canvas)
        Transform openTrans = canvas.transform.Find("OpenRewardsButton");
        GameObject openBtnGO;
        if (openTrans == null)
        {
            openBtnGO = new GameObject("OpenRewardsButton", typeof(RectTransform));
            openBtnGO.transform.SetParent(canvas.transform, false);
        }
        else
        {
            openBtnGO = openTrans.gameObject;
        }

        RectTransform openRect = openBtnGO.GetComponent<RectTransform>();
        if (openRect == null) openRect = openBtnGO.AddComponent<RectTransform>();
        
        openRect.anchorMin = new Vector2(0.5f, 1);
        openRect.anchorMax = new Vector2(0.5f, 1);
        openRect.pivot = new Vector2(0.5f, 1);
        openRect.anchoredPosition = new Vector2(0, -100);
        openRect.sizeDelta = new Vector2(150, 50);

        Image openImg = openBtnGO.GetComponent<Image>();
        if (openImg == null) openImg = openBtnGO.AddComponent<Image>();
        openImg.color = Color.yellow;

        Button openBtn = openBtnGO.GetComponent<Button>();
        if (openBtn == null) openBtn = openBtnGO.AddComponent<Button>();

        CreateText(openBtnGO, "Text", "OPEN REWARDS", 18, Vector2.zero, new Vector2(150, 50)).color = Color.black;
        
        rewardUI.openButton = openBtn;
        openBtnGO.SetActive(false);
    }

    static GameObject GetChild(GameObject parent, string name)
    {
        Transform t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static TextMeshProUGUI CreateText(GameObject parent, string name, string content, float fontSize, Vector2 pos, Vector2 size)
    {
        GameObject go = GetChild(parent, name);
        TextMeshProUGUI txt = go.GetComponent<TextMeshProUGUI>();
        if (txt == null) txt = go.AddComponent<TextMeshProUGUI>();
        
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        return txt;
    }
}
