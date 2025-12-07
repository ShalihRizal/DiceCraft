using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;

public class MainMenuSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Main Menu")]
    static void ShowWindow()
    {
        GetWindow<MainMenuSetupTool>("Main Menu Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Main Menu Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Main Menu UI", GUILayout.Height(40)))
        {
            CreateMainMenuUI();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Create Game Over UI", GUILayout.Height(40)))
        {
            CreateGameOverUI();
        }
    }

    static void CreateMainMenuUI()
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

        // Create Main Menu Panel
        GameObject mainMenuPanel = new GameObject("MainMenuPanel");
        mainMenuPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = mainMenuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = mainMenuPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        // Create Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainMenuPanel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(600, 100);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "DICECRAFT";
        titleText.fontSize = 72;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Create Button Container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(mainMenuPanel.transform, false);
        
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.4f);
        containerRect.anchorMax = new Vector2(0.5f, 0.4f);
        containerRect.sizeDelta = new Vector2(400, 400);

        VerticalLayoutGroup layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        // Create Buttons
        CreateMenuButton(buttonContainer, "Start Game", "StartGame");
        CreateMenuButton(buttonContainer, "Settings", "OpenSettings");
        CreateMenuButton(buttonContainer, "Achievements", "OpenAchievements");
        CreateMenuButton(buttonContainer, "Quit", "QuitGame");

        // Add MainMenuManager
        GameObject managerObj = new GameObject("MainMenuManager");
        managerObj.transform.SetParent(canvas.transform, false);
        MainMenuManager manager = managerObj.AddComponent<MainMenuManager>();

        // Create Settings Panel
        CreateSettingsPanel(canvas);

        Debug.Log("✅ Main Menu UI created successfully!");
        Selection.activeGameObject = mainMenuPanel;
    }

    static GameObject CreateMenuButton(GameObject parent, string buttonText, string methodName)
    {
        GameObject buttonObj = new GameObject(buttonText + "Button");
        buttonObj.transform.SetParent(parent.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, 60);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.3f, 0.5f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        
        // Set up button colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.3f, 0.5f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.4f, 0.6f, 1f);
        colors.pressedColor = new Color(0.15f, 0.25f, 0.45f, 1f);
        button.colors = colors;

        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI buttonTextComponent = textObj.AddComponent<TextMeshProUGUI>();
        buttonTextComponent.text = buttonText;
        buttonTextComponent.fontSize = 24;
        buttonTextComponent.alignment = TextAlignmentOptions.Center;
        buttonTextComponent.color = Color.white;

        return buttonObj;
    }

    static void CreateSettingsPanel(Canvas canvas)
    {
        GameObject settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = settingsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = settingsPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        // Create settings content
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(settingsPanel.transform, false);

        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(600, 500);

        Image contentImage = contentPanel.AddComponent<Image>();
        contentImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Add SettingsManager
        SettingsManager settingsManager = settingsPanel.AddComponent<SettingsManager>();
        settingsManager.settingsPanel = settingsPanel;

        // Create Close Button
        GameObject closeButton = CreateMenuButton(contentPanel, "Close", "HideSettings");
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.1f);
        closeRect.anchorMax = new Vector2(0.5f, 0.1f);
        closeRect.sizeDelta = new Vector2(200, 50);

        settingsPanel.SetActive(false);

        Debug.Log("✅ Settings Panel created!");
    }

    static void CreateGameOverUI()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found! Please create a Canvas first.");
            return;
        }

        // Create Game Over Panel
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);

        // Create Content Panel
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(gameOverPanel.transform, false);

        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(500, 400);

        // Create Title
        GameObject titleObj = new GameObject("GameOverText");
        titleObj.transform.SetParent(contentPanel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(400, 100);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 60;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.3f, 0.3f);

        // Create Button Container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(contentPanel.transform, false);

        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.3f);
        containerRect.anchorMax = new Vector2(0.5f, 0.3f);
        containerRect.sizeDelta = new Vector2(300, 150);

        VerticalLayoutGroup layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;

        // Create Buttons
        CreateMenuButton(buttonContainer, "Main Menu", "ReturnToMainMenu");
        CreateMenuButton(buttonContainer, "Restart", "RestartGame");

        // Add GameOverManager
        GameObject managerObj = new GameObject("GameOverManager");
        managerObj.transform.SetParent(canvas.transform, false);
        GameOverManager manager = managerObj.AddComponent<GameOverManager>();
        manager.gameOverPanel = gameOverPanel;

        gameOverPanel.SetActive(false);

        Debug.Log("✅ Game Over UI created successfully!");
        Selection.activeGameObject = gameOverPanel;
    }
}
#endif
