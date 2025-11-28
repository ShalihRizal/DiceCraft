using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ShopSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Shop Interface")]
    public static void ShowWindow()
    {
        GetWindow<ShopSetupTool>("Shop Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Shop Interface Setup", EditorStyles.boldLabel);

        if (GUILayout.Button("1. Create Shop UI Hierarchy"))
        {
            CreateShopUI();
        }

        if (GUILayout.Button("2. Create Shop Item Prefab"))
        {
            CreateShopItemPrefab();
        }

        if (GUILayout.Button("3. Assign References to ShopManager"))
        {
            AssignReferences();
        }
    }

    private void CreateShopUI()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Canvas not found! Please create a UI Canvas first.");
            return;
        }

        Transform existingShop = canvasObj.transform.Find("ShopUI");
        if (existingShop != null)
        {
            Debug.Log("ShopUI already exists.");
            return;
        }

        // 1. Main Panel (Background)
        GameObject shopPanel = new GameObject("ShopUI");
        shopPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform rect = shopPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Dark overlay for the whole screen
        Image bg = shopPanel.AddComponent<Image>();
        bg.color = new Color(0.0f, 0.0f, 0.0f, 0.8f);

        // 2. Shop Card/Container (Centered)
        GameObject cardObj = new GameObject("ShopCard");
        cardObj.transform.SetParent(shopPanel.transform, false);
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(900, 700); // Fixed size, nice and big
        
        Image cardImg = cardObj.AddComponent<Image>();
        cardImg.color = new Color(0.1f, 0.1f, 0.12f, 1f); // Dark blue-ish grey

        // 3. Header
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(cardObj.transform, false);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 80);
        headerRect.anchoredPosition = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(headerObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "MERCHANT";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.8f, 0.2f); // Gold
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(0, 0);
        titleRect.offsetMax = new Vector2(0, 0);

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(headerObj.transform, false);
        RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(50, 50);
        closeRect.anchoredPosition = new Vector2(-15, -15);

        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        
        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "X";
        closeText.fontSize = 30;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        // 4. Content Area (Vertical Layout)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(cardObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(20, 20); // Padding Bottom/Left
        contentRect.offsetMax = new Vector2(-20, -100); // Padding Top (below header)/Right

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 40; // More spacing between rows
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        // Assign to ShopManager if present
        ShopManager manager = Object.FindFirstObjectByType<ShopManager>();
        if (manager != null)
        {
            manager.shopRootUI = shopPanel;
            manager.shopContentContainer = contentObj.transform;
            
            // Try to add close listener
            // manager.closeButton = closeBtn; // If we had the field accessible here
        }

        Debug.Log("Shop UI Hierarchy created (Fixed Layout).");
    }

    private void CreateShopItemPrefab()
    {
        string path = "Assets/Prefabs/UI/ShopItem.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
        }

        GameObject temp = new GameObject("ShopItem");
        Image bg = temp.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f);
        
        VerticalLayoutGroup vlg = temp.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        vlg.childControlHeight = false;
        // Add LayoutElement
        LayoutElement le = temp.AddComponent<LayoutElement>();
        le.preferredWidth = 150;
        le.preferredHeight = 140;

        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(temp.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Item Description";
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.Center;
        descText.enableWordWrapping = true;
        
        // Button
        GameObject btnObj = new GameObject("BuyButton");
        btnObj.transform.SetParent(temp.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.green;
        Button btn = btnObj.AddComponent<Button>();
        
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Buy";
        btnText.fontSize = 20;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.black;
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(0, 40);

        // Add Script
        ShopItemUI ui = temp.AddComponent<ShopItemUI>();
        ui.descriptionText = descText;
        ui.buyButton = btn;

        PrefabUtility.SaveAsPrefabAsset(temp, path);
        DestroyImmediate(temp);
        Debug.Log("ShopItem Prefab created at " + path);
    }

    private void AssignReferences()
    {
        ShopManager manager = FindFirstObjectByType<ShopManager>();
        if (manager == null)
        {
            Debug.LogError("ShopManager not found!");
            return;
        }

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform shopUI = canvasObj.transform.Find("ShopUI");
            if (shopUI != null)
            {
                manager.shopRootUI = shopUI.gameObject;
                Debug.Log("Assigned Shop Root UI.");
                
                // Find Content Container in ShopCard
                Transform container = shopUI.Find("ShopCard/Content");
                if (container != null)
                {
                    manager.shopContentContainer = container;
                    Debug.Log("Assigned Shop Content Container.");
                }
                else
                {
                    // Fallback for old structure
                    container = shopUI.Find("ContentContainer");
                    if (container != null) manager.shopContentContainer = container;
                }

                // Assign Close Button
                Transform closeBtnTr = shopUI.Find("ShopCard/Header/CloseButton");
                if (closeBtnTr != null)
                {
                    Button closeBtn = closeBtnTr.GetComponent<Button>();
                    // We can't assign to the field via code easily if it's not public serialized or we don't use SerializedObject
                    // But since we are in Editor script, we can do:
                    // manager.closeButton = closeBtn; // This works if manager is a MonoBehaviour instance in scene
                    // But we need to make sure the field exists. It does now.
                    // However, to be safe and persistent, we should use SerializedObject if we want to be "proper", 
                    // but direct assignment works for scene objects.
                    // Let's assume the user will click "Assign References" which calls this.
                    // Wait, manager IS a scene object.
                    
                    // Reflection or just direct assignment if we trust the compilation
                    // Since I can't see if the field is actually compiled yet (it should be), I'll use SerializedObject to be safe and robust.
                    SerializedObject so = new SerializedObject(manager);
                    so.Update();
                    SerializedProperty closeBtnProp = so.FindProperty("closeButton");
                    if (closeBtnProp != null)
                    {
                        closeBtnProp.objectReferenceValue = closeBtn;
                        so.ApplyModifiedProperties();
                        Debug.Log("Assigned Close Button.");
                    }
                }
            }
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ShopItem.prefab");
        if (prefab != null)
        {
            manager.shopItemUIPrefab = prefab;
            Debug.Log("Assigned ShopItem Prefab.");
        }

        EditorUtility.SetDirty(manager);
    }
}
