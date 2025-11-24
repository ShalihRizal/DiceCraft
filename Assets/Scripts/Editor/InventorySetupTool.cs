using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class InventorySetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Inventory UI")]
    public static void SetupInventoryUI()
    {
        // 1. Find or Create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f; // UI at Z=-5 (if Camera at -10). In front of Board (Z=0).
            canvas.sortingOrder = 100; // Ensure UI is drawn above everything else
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }
        else
        {
            // Update existing canvas if needed
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f; // UI at Z=-5
            canvas.sortingOrder = 100;
        }

        // 2. Create Inventory Manager
        InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager == null)
        {
            GameObject managerObj = new GameObject("InventoryManager");
            inventoryManager = managerObj.AddComponent<InventoryManager>();
            Undo.RegisterCreatedObjectUndo(managerObj, "Create Inventory Manager");
        }

        // 3. Create Inventory UI Panel
        GameObject inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = inventoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.5f); // Left Middle
        panelRect.anchorMax = new Vector2(0, 0.5f);
        panelRect.pivot = new Vector2(0, 0.5f);
        panelRect.sizeDelta = new Vector2(350, 280); // Adjusted for 4x3 grid
        panelRect.anchoredPosition = new Vector2(20, 0);
        
        Image panelImage = inventoryPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black

        InventoryUI inventoryUI = inventoryPanel.AddComponent<InventoryUI>();
        Undo.RegisterCreatedObjectUndo(inventoryPanel, "Create Inventory Panel");

        // 4. Create Slots Parent
        GameObject slotsParent = new GameObject("SlotsContainer");
        slotsParent.transform.SetParent(inventoryPanel.transform, false);
        RectTransform slotsRect = slotsParent.AddComponent<RectTransform>();
        slotsRect.anchorMin = Vector2.zero;
        slotsRect.anchorMax = Vector2.one;
        slotsRect.offsetMin = new Vector2(10, 10);
        slotsRect.offsetMax = new Vector2(-10, -10);

        GridLayoutGroup layout = slotsParent.AddComponent<GridLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.cellSize = new Vector2(70, 70);
        layout.spacing = new Vector2(10, 10);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 4;

        inventoryUI.slotsParent = slotsParent.transform;

        // 5. Create Slot Prefab
        string prefabPath = "Assets/Prefabs/InventorySlot.prefab";
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject slotObj = new GameObject("InventorySlot");
        Image slotImage = slotObj.AddComponent<Image>();
        slotImage.color = new Color(1, 1, 1, 0.2f); // Slot background
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(80, 80);

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.raycastTarget = false;
        iconImage.enabled = false; // Hidden by default
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        InventorySlot slotScript = slotObj.AddComponent<InventorySlot>();
        slotScript.icon = iconImage;

        // Save as Prefab
        GameObject slotPrefab = PrefabUtility.SaveAsPrefabAsset(slotObj, prefabPath);
        DestroyImmediate(slotObj); // Remove scene instance

        // 6. Link everything
        inventoryUI.slotPrefab = slotPrefab;
        inventoryManager.inventoryUI = inventoryUI;

        // 7. Create Trash UI
        TrashUI trashUI = FindFirstObjectByType<TrashUI>();
        if (trashUI == null)
        {
            GameObject trashObj = new GameObject("TrashZoneUI");
            trashObj.transform.SetParent(canvas.transform, false);
            
            Image trashImage = trashObj.AddComponent<Image>();
            trashImage.color = new Color(1, 0, 0, 0.5f); // Red semi-transparent
            
            RectTransform trashRect = trashObj.GetComponent<RectTransform>();
            trashRect.anchorMin = new Vector2(1, 0); // Bottom Right
            trashRect.anchorMax = new Vector2(1, 0);
            trashRect.pivot = new Vector2(1, 0);
            trashRect.sizeDelta = new Vector2(100, 100);
            trashRect.anchoredPosition = new Vector2(-20, 20);

            trashObj.AddComponent<TrashUI>();
            Undo.RegisterCreatedObjectUndo(trashObj, "Create Trash UI");
        }

        Selection.activeGameObject = inventoryPanel;
        Debug.Log("Inventory UI Setup Complete!");
    }
}
