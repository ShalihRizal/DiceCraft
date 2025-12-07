using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;

public class BossUISetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Boss UI")]
    static void ShowWindow()
    {
        GetWindow<BossUISetupTool>("Boss UI Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Boss UI Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Boss Health Bar", GUILayout.Height(40)))
        {
            CreateBossHealthBarUI();
        }
    }

    static void CreateBossHealthBarUI()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create Boss Health Panel
        GameObject bossPanel = new GameObject("BossHealthPanel");
        bossPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = bossPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f); // Top Center
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -50);
        panelRect.sizeDelta = new Vector2(800, 100);

        // Add BossHealthBarUI script
        BossHealthBarUI bossUI = bossPanel.AddComponent<BossHealthBarUI>();
        bossUI.healthBarPanel = bossPanel;

        // Create ID Text (Boss Name)
        GameObject nameObj = new GameObject("BossNameText");
        nameObj.transform.SetParent(bossPanel.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.anchoredPosition = new Vector2(0, 30);
        nameRect.sizeDelta = new Vector2(0, 30);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "BOSS NAME";
        nameText.fontSize = 24;
        nameText.alignment = TextAlignmentOptions.BottomLeft;
        nameText.color = Color.red;
        bossUI.bossNameText = nameText;

        // Health Bar Container
        GameObject container = new GameObject("HealthBarContainer");
        container.transform.SetParent(bossPanel.transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>(); // ✅ Fixing the reported error
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(container.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Fill
        GameObject fillObj = new GameObject("ReadHealthFill");
        fillObj.transform.SetParent(container.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-4, -4); // Padding
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.red;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        bossUI.healthFillImage = fillImage;

        // Health Text
        GameObject hpTextObj = new GameObject("HealthText");
        hpTextObj.transform.SetParent(container.transform, false);
        RectTransform hpTextRect = hpTextObj.AddComponent<RectTransform>();
        hpTextRect.anchorMin = Vector2.zero;
        hpTextRect.anchorMax = Vector2.one;
        hpTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI hpText = hpTextObj.AddComponent<TextMeshProUGUI>();
        hpText.text = "1000/1000";
        hpText.fontSize = 20;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = Color.white;
        bossUI.healthText = hpText;

        Debug.Log("✅ Boss UI Created Successfully!");
        Selection.activeGameObject = bossPanel;
    }
}
#endif
