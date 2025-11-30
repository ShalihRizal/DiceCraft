using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TooltipSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Tooltip System")]
    public static void SetupTooltipSystem()
    {
        // 1. Find or Create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Tooltips usually work best in Overlay or Camera
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // 2. Find or Create TooltipManager
        TooltipManager manager = FindFirstObjectByType<TooltipManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("TooltipManager");
            manager = managerObj.AddComponent<TooltipManager>();
            Undo.RegisterCreatedObjectUndo(managerObj, "Create TooltipManager");
        }

        // 3. Create Tooltip Prefab if it doesn't exist
        string prefabPath = "Assets/Prefabs/DiceTooltip.prefab";
        GameObject tooltipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (tooltipPrefab == null)
        {
            if (!System.IO.Directory.Exists("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Create UI Hierarchy
            GameObject tooltipObj = new GameObject("DiceTooltip");
            tooltipObj.transform.SetParent(canvas.transform, false);
            
            // Background Image
            Image bgImage = tooltipObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark background
            
            RectTransform rect = tooltipObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 150);
            
            // Layout Group
            VerticalLayoutGroup layout = tooltipObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // Content Size Fitter
            ContentSizeFitter fitter = tooltipObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Helper to create Text
            TextMeshProUGUI CreateText(string name, string defaultText, float fontSize, FontStyles style)
            {
                GameObject textObj = new GameObject(name);
                textObj.transform.SetParent(tooltipObj.transform, false);
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = defaultText;
                tmp.fontSize = fontSize;
                tmp.fontStyle = style;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Left;
                return tmp;
            }

            // Create Text Elements
            DiceTooltip tooltipScript = tooltipObj.AddComponent<DiceTooltip>();
            
            tooltipScript.nameText = CreateText("NameText", "Dice Name", 18, FontStyles.Bold);
            tooltipScript.damageText = CreateText("DamageText", "Damage: 0", 14, FontStyles.Normal);
            tooltipScript.fireRateText = CreateText("FireRateText", "Fire Rate: 0.0s", 14, FontStyles.Normal);
            tooltipScript.sidesText = CreateText("SidesText", "Sides: 6", 14, FontStyles.Normal);
            
            // Passive Section
            tooltipScript.passiveNameText = CreateText("PassiveName", "Passive Name", 16, FontStyles.Bold);
            tooltipScript.passiveNameText.color = new Color(1f, 0.8f, 0.2f); // Gold color
            
            tooltipScript.passiveDescText = CreateText("PassiveDesc", "Passive Description...", 12, FontStyles.Italic);
            tooltipScript.passiveDescText.color = new Color(0.8f, 0.8f, 0.8f); // Light gray

            // Assign RectTransform
            tooltipScript.rectTransform = rect;

            // Save as Prefab
            tooltipPrefab = PrefabUtility.SaveAsPrefabAsset(tooltipObj, prefabPath);
            DestroyImmediate(tooltipObj); // Remove from scene
            
            Debug.Log("Created DiceTooltip Prefab at " + prefabPath);
        }

        // 4. Assign Prefab to Manager
        if (manager.tooltipPrefab == null)
        {
            manager.tooltipPrefab = tooltipPrefab;
            EditorUtility.SetDirty(manager);
            Debug.Log("Assigned Tooltip Prefab to Manager");
        }

        Debug.Log("Tooltip System Setup Complete!");
    }
}
