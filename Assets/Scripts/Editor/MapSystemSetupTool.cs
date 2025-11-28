using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class MapSystemSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Map System")]
    public static void ShowWindow()
    {
        GetWindow<MapSystemSetupTool>("Map System Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Map System Setup", EditorStyles.boldLabel);

        if (GUILayout.Button("Setup Map System Hierarchy"))
        {
            SetupMapSystem();
        }
        
        if (GUILayout.Button("Apply Default Node Weights"))
        {
            ApplyDefaultWeights();
        }
    }

    private void ApplyDefaultWeights()
    {
        MapManager mapManager = FindFirstObjectByType<MapManager>();
        if (mapManager != null && mapManager.mapConfig != null)
        {
            var config = mapManager.mapConfig;
            config.nodeWeights = new System.Collections.Generic.List<NodeWeight>
            {
                new NodeWeight { type = NodeType.Combat, weight = 50 },
                new NodeWeight { type = NodeType.Elite, weight = 15 },
                new NodeWeight { type = NodeType.Shop, weight = 10 },
                new NodeWeight { type = NodeType.Campfire, weight = 15 },
                new NodeWeight { type = NodeType.Event, weight = 10 }
            };
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("Applied Default Node Weights!");
        }
        else
        {
            Debug.LogError("MapManager or MapConfig not found!");
        }
    }

    private void SetupMapSystem()
    {
        // 1. Setup MapManager
        MapManager mapManager = FindFirstObjectByType<MapManager>();
        if (mapManager == null)
        {
            GameObject managerObj = new GameObject("MapManager");
            mapManager = managerObj.AddComponent<MapManager>();
            managerObj.AddComponent<MapGenerator>();
            Debug.Log("Created MapManager.");
        }

        // Ensure MapGenerator reference
        if (mapManager.mapGenerator == null)
        {
            mapManager.mapGenerator = mapManager.GetComponent<MapGenerator>();
        }

        // Create/Assign MapConfig
        if (mapManager.mapConfig == null)
        {
            string configPath = "Assets/Resources/DefaultMapConfig.asset";
            MapConfig config = AssetDatabase.LoadAssetAtPath<MapConfig>(configPath);
            if (config == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                config = ScriptableObject.CreateInstance<MapConfig>();
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Created DefaultMapConfig.");
            }
            mapManager.mapConfig = config;
            
            // Also assign to generator
            if (mapManager.mapGenerator != null)
            {
                mapManager.mapGenerator.config = config;
            }
        }

        // 2. Setup MapUI
        MapUI mapUI = FindFirstObjectByType<MapUI>();
        if (mapUI == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas");
                canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            GameObject uiObj = new GameObject("MapUI");
            uiObj.transform.SetParent(canvasObj.transform, false);
            mapUI = uiObj.AddComponent<MapUI>();
            
            // Ensure MapUI stretches
            RectTransform uiRect = uiObj.AddComponent<RectTransform>();
            uiRect.anchorMin = Vector2.zero;
            uiRect.anchorMax = Vector2.one;
            uiRect.sizeDelta = Vector2.zero;
            
            // Create Hierarchy
            // Scroll View
            GameObject scrollView = new GameObject("Scroll View");
            scrollView.transform.SetParent(uiObj.transform, false);
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            RectTransform scrollRectTrans = scrollView.GetComponent<RectTransform>();
            scrollRectTrans.anchorMin = Vector2.zero;
            scrollRectTrans.anchorMax = Vector2.one;
            scrollRectTrans.sizeDelta = Vector2.zero;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform viewRect = viewport.AddComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.sizeDelta = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = new Color(1,1,1,0.01f); // Invisible hit target

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 1f); // Top Center
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(1000, 2000); // Arbitrary large size

            // Line Container (Behind nodes)
            GameObject lineContainer = new GameObject("LineContainer");
            lineContainer.transform.SetParent(content.transform, false);
            RectTransform lineRect = lineContainer.AddComponent<RectTransform>();
            lineRect.anchorMin = Vector2.zero;
            lineRect.anchorMax = Vector2.one;
            lineRect.sizeDelta = Vector2.zero;

            // Assign References
            mapUI.scrollRect = scrollRect;
            mapUI.contentContainer = content.transform;
            mapUI.lineContainer = lineContainer.transform;
            scrollRect.content = contentRect;
            scrollRect.viewport = viewRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            Debug.Log("Created MapUI Hierarchy.");
        }
        
        // 3. Create Prefabs (Placeholder)
        // 3. Create/Update Prefabs
        string nodePrefabPath = "Assets/Prefabs/UI/MapNode.prefab";
        GameObject existingNodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(nodePrefabPath);
        
        if (existingNodePrefab == null)
        {
             // Create new if doesn't exist
             if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
             {
                 if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
                 AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
             }
             
             GameObject temp = new GameObject("MapNode");
             Image mainImage = temp.AddComponent<Image>();
             // temp.AddComponent<Button>(); // Optional, using PointerClick now
             
             // Child for Icon
             GameObject iconObj = new GameObject("TypeIcon");
             iconObj.transform.SetParent(temp.transform, false);
             Image iconImage = iconObj.AddComponent<Image>();
             RectTransform iconRect = iconObj.GetComponent<RectTransform>();
             iconRect.anchorMin = new Vector2(0.2f, 0.2f);
             iconRect.anchorMax = new Vector2(0.8f, 0.8f);
             iconRect.offsetMin = Vector2.zero;
             iconRect.offsetMax = Vector2.zero;
             
             MapNodeUI nodeUI = temp.AddComponent<MapNodeUI>();
             nodeUI.hexImage = mainImage;
             nodeUI.typeIcon = iconImage;
             
             existingNodePrefab = PrefabUtility.SaveAsPrefabAsset(temp, nodePrefabPath);
             DestroyImmediate(temp);
             Debug.Log("Created MapNode Prefab.");
        }
        else
        {
            // Fix existing prefab
            using (var scope = new PrefabUtility.EditPrefabContentsScope(nodePrefabPath))
            {
                GameObject prefabRoot = scope.prefabContentsRoot;
                Transform iconTrans = prefabRoot.transform.Find("TypeIcon");
                if (iconTrans == null)
                {
                    GameObject iconObj = new GameObject("TypeIcon");
                    iconObj.transform.SetParent(prefabRoot.transform, false);
                    Image iconImage = iconObj.AddComponent<Image>();
                    RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0.2f, 0.2f);
                    iconRect.anchorMax = new Vector2(0.8f, 0.8f);
                    iconRect.offsetMin = Vector2.zero;
                    iconRect.offsetMax = Vector2.zero;
                    
                    MapNodeUI ui = prefabRoot.GetComponent<MapNodeUI>();
                    if (ui != null) ui.typeIcon = iconImage;
                    
                    Debug.Log("Fixed existing MapNode prefab: Added TypeIcon.");
                }
            }
        }
        mapUI.nodePrefab = existingNodePrefab;

        if (mapUI.linePrefab == null)
        {
            string prefabPath = "Assets/Prefabs/UI/MapLine.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                GameObject temp = new GameObject("MapLine");
                Image img = temp.AddComponent<Image>();
                img.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                
                prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
                DestroyImmediate(temp);
                Debug.Log("Created Placeholder MapLine Prefab.");
            }
            mapUI.linePrefab = prefab;
        }

        EditorUtility.SetDirty(mapManager);
        EditorUtility.SetDirty(mapUI);
        Debug.Log("Map System Setup Complete!");
    }
}
