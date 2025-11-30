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

        // Add EventSystem if not present
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
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

        // Add MainMenuManager FIRST
        GameObject managerObj = new GameObject("MainMenuManager");
        managerObj.transform.SetParent(canvas.transform, false);
        MainMenuManager manager = managerObj.AddComponent<MainMenuManager>();
        
        // Add AchievementManager (Singleton)
        if (FindFirstObjectByType<AchievementManager>() == null)
        {
            GameObject achManagerObj = new GameObject("AchievementManager");
            // AchievementManager must be a root object for DontDestroyOnLoad to work
            achManagerObj.AddComponent<AchievementManager>();
        }

        // Create Buttons and wire them up persistently
        Button startButton = CreateMenuButton(buttonContainer, "Start Game", "StartGame").GetComponent<Button>();
        Button settingsButton = CreateMenuButton(buttonContainer, "Settings", "OpenSettings").GetComponent<Button>();
        Button achievementsButton = CreateMenuButton(buttonContainer, "Achievements", "OpenAchievements").GetComponent<Button>();
        Button quitButton = CreateMenuButton(buttonContainer, "Quit", "QuitGame").GetComponent<Button>();

        // Wire up button events using UnityEventTools
        UnityEditor.Events.UnityEventTools.AddPersistentListener(startButton.onClick, manager.StartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(settingsButton.onClick, manager.OpenSettings);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(achievementsButton.onClick, manager.OpenAchievements);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitButton.onClick, manager.QuitGame);

        // Create Settings Panel
        CreateSettingsPanel(canvas, manager);

        // Create Achievement UI
        CreateAchievementUI(canvas, manager);

        Debug.Log("✅ Main Menu UI created successfully!");
        Selection.activeGameObject = mainMenuPanel;
    }

    static void CreateAchievementUI(Canvas canvas, MainMenuManager manager)
    {
        GameObject panelObj = new GameObject("AchievementPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);

        // Content Panel
        GameObject contentObj = new GameObject("ContentPanel");
        contentObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(800, 700);

        Image contentImage = contentObj.AddComponent<Image>();
        contentImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(contentObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.95f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.sizeDelta = new Vector2(400, 50);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "ACHIEVEMENTS";
        titleText.fontSize = 40;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Scroll View
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(contentObj.transform, false);
        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.05f, 0.15f);
        scrollRect.anchorMax = new Vector2(0.95f, 0.85f);
        scrollRect.sizeDelta = Vector2.zero;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(0,0,0,0);
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        scroll.viewport = viewportRect;

        GameObject listContentObj = new GameObject("Content");
        listContentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform listContentRect = listContentObj.AddComponent<RectTransform>();
        listContentRect.anchorMin = new Vector2(0, 1);
        listContentRect.anchorMax = new Vector2(1, 1);
        listContentRect.pivot = new Vector2(0.5f, 1f);
        listContentRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup listLayout = listContentObj.AddComponent<VerticalLayoutGroup>();
        listLayout.childControlHeight = true; // Enable control height so ContentSizeFitter works
        listLayout.childControlWidth = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childForceExpandWidth = true;
        listLayout.spacing = 10;
        listLayout.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter listFitter = listContentObj.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scroll.content = listContentRect;

        // Create Item Template (Prefab)
        GameObject itemTemplate = CreateAchievementItemTemplate(panelObj);
        itemTemplate.SetActive(false); // Hide it

        // Setup List UI
        AchievementListUI listUI = panelObj.AddComponent<AchievementListUI>();
        listUI.contentContainer = listContentRect;
        listUI.itemPrefab = itemTemplate;

        // Close Button
        GameObject closeButton = CreateMenuButton(contentObj, "Close", "ClosePanel");
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.05f);
        closeRect.anchorMax = new Vector2(0.5f, 0.05f);
        closeRect.sizeDelta = new Vector2(200, 50);
        
        Button closeBtn = closeButton.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, listUI.ClosePanel);

        // Assign to Manager
        if (manager != null)
        {
            manager.achievementListUI = listUI;
        }

        panelObj.SetActive(false);
    }

    static GameObject CreateAchievementItemTemplate(GameObject parent)
    {
        GameObject itemObj = new GameObject("AchievementItem_Template");
        itemObj.transform.SetParent(parent.transform, false);

        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 120);

        // Add LayoutElement for proper sizing in the list
        LayoutElement layoutElement = itemObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 120;
        layoutElement.preferredHeight = 120;

        Image itemBg = itemObj.AddComponent<Image>();
        itemBg.color = new Color(0.25f, 0.25f, 0.3f, 1f);

        AchievementItemUI itemUI = itemObj.AddComponent<AchievementItemUI>();

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(itemObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.sizeDelta = new Vector2(80, 80);
        iconRect.anchoredPosition = new Vector2(60, 0);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.gray; // Placeholder
        itemUI.iconImage = iconImage;

        // Info Container
        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(itemObj.transform, false);
        RectTransform infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.offsetMin = new Vector2(120, 10);
        infoRect.offsetMax = new Vector2(-200, -10);

        VerticalLayoutGroup infoLayout = infoObj.AddComponent<VerticalLayoutGroup>();
        infoLayout.childControlHeight = false;
        infoLayout.childControlWidth = true;
        infoLayout.spacing = 5;
        infoLayout.childAlignment = TextAnchor.UpperLeft;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(infoObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(0, 30);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Left;
        itemUI.titleText = titleText;

        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(infoObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(0, 20);
        
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.fontSize = 16;
        descText.color = new Color(0.8f, 0.8f, 0.8f);
        descText.alignment = TextAlignmentOptions.Left;
        itemUI.descriptionText = descText;

        // Progress Container
        GameObject progressObj = new GameObject("Progress");
        progressObj.transform.SetParent(infoObj.transform, false);
        RectTransform progressRect = progressObj.AddComponent<RectTransform>();
        progressRect.sizeDelta = new Vector2(0, 20);
        itemUI.progressContainer = progressObj;

        Slider progressBar = progressObj.AddComponent<Slider>();
        
        // Slider Visuals (Simplified)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(progressObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(progressObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.yellow;
        progressBar.fillRect = fillRect;
        itemUI.progressBar = progressBar;

        // Progress Text
        GameObject progTextObj = new GameObject("ProgressText");
        progTextObj.transform.SetParent(progressObj.transform, false);
        RectTransform progTextRect = progTextObj.AddComponent<RectTransform>();
        progTextRect.anchorMin = Vector2.zero;
        progTextRect.anchorMax = Vector2.one;
        TextMeshProUGUI progText = progTextObj.AddComponent<TextMeshProUGUI>();
        progText.fontSize = 14;
        progText.alignment = TextAlignmentOptions.Center;
        progText.color = Color.white;
        itemUI.progressText = progText;

        // Right Side (Claim/Status)
        GameObject rightObj = new GameObject("RightSide");
        rightObj.transform.SetParent(itemObj.transform, false);
        RectTransform rightRect = rightObj.AddComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(1, 0);
        rightRect.anchorMax = new Vector2(1, 1);
        rightRect.pivot = new Vector2(1, 0.5f);
        rightRect.sizeDelta = new Vector2(180, 0);
        rightRect.anchoredPosition = new Vector2(-10, 0);

        VerticalLayoutGroup rightLayout = rightObj.AddComponent<VerticalLayoutGroup>();
        rightLayout.spacing = 5;
        rightLayout.childAlignment = TextAnchor.MiddleCenter;

        // Rewards
        GameObject rewardsObj = new GameObject("Rewards");
        rewardsObj.transform.SetParent(rightObj.transform, false);
        HorizontalLayoutGroup rewardsLayout = rewardsObj.AddComponent<HorizontalLayoutGroup>();
        rewardsLayout.childControlWidth = true;
        rewardsLayout.childControlHeight = true;
        itemUI.rewardContainer = rewardsObj.transform;

        // Reward Template (Simple Text for now)
        GameObject rewardTmpl = new GameObject("RewardTemplate");
        rewardTmpl.transform.SetParent(rewardsObj.transform, false);
        TextMeshProUGUI rewText = rewardTmpl.AddComponent<TextMeshProUGUI>();
        rewText.fontSize = 14;
        rewText.color = Color.yellow;
        rewText.alignment = TextAlignmentOptions.Center;
        itemUI.rewardTemplate = rewardTmpl;
        rewardTmpl.SetActive(false);

        // Date Cleared
        GameObject dateObj = new GameObject("DateCleared");
        dateObj.transform.SetParent(rightObj.transform, false);
        TextMeshProUGUI dateText = dateObj.AddComponent<TextMeshProUGUI>();
        dateText.fontSize = 12;
        dateText.color = Color.gray;
        dateText.alignment = TextAlignmentOptions.Center;
        itemUI.dateClearedText = dateText;

        // Claim Button
        GameObject claimBtnObj = new GameObject("ClaimButton");
        claimBtnObj.transform.SetParent(rightObj.transform, false);
        LayoutElement claimLayout = claimBtnObj.AddComponent<LayoutElement>();
        claimLayout.minHeight = 40;
        claimLayout.preferredHeight = 40;
        
        Image claimImg = claimBtnObj.AddComponent<Image>();
        claimImg.color = new Color(0.2f, 0.6f, 0.2f);
        Button claimBtn = claimBtnObj.AddComponent<Button>();
        itemUI.claimButton = claimBtn;

        GameObject claimTextObj = new GameObject("Text");
        claimTextObj.transform.SetParent(claimBtnObj.transform, false);
        RectTransform claimTextRect = claimTextObj.AddComponent<RectTransform>();
        claimTextRect.anchorMin = Vector2.zero;
        claimTextRect.anchorMax = Vector2.one;
        TextMeshProUGUI claimText = claimTextObj.AddComponent<TextMeshProUGUI>();
        claimText.text = "Claim";
        claimText.fontSize = 18;
        claimText.alignment = TextAlignmentOptions.Center;
        claimText.color = Color.white;
        itemUI.claimButtonText = claimText;

        return itemObj;
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

    static void CreateSettingsPanel(Canvas canvas, MainMenuManager mainMenuManager)
    {
        GameObject settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = settingsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = settingsPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        // Create settings content panel
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(settingsPanel.transform, false);

        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(700, 600);

        Image contentImage = contentPanel.AddComponent<Image>();
        contentImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Create Title
        GameObject titleObj = new GameObject("SettingsTitle");
        titleObj.transform.SetParent(contentPanel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(600, 60);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SETTINGS";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Create settings container
        GameObject settingsContainer = new GameObject("SettingsContainer");
        settingsContainer.transform.SetParent(contentPanel.transform, false);

        RectTransform containerRect = settingsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(600, 400);
        containerRect.anchoredPosition = new Vector2(0, -20);

        VerticalLayoutGroup layoutGroup = settingsContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 25;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.padding = new RectOffset(50, 50, 20, 20);

        // Add SettingsManager
        SettingsManager settingsManager = settingsPanel.AddComponent<SettingsManager>();
        settingsManager.settingsPanel = settingsPanel;
        
        // Assign to MainMenuManager
        if (mainMenuManager != null)
        {
            mainMenuManager.settingsManager = settingsManager;
        }

        // Create Resolution Dropdown
        TMP_Dropdown resolutionDropdown = CreateDropdownSetting(settingsContainer, "Resolution");
        settingsManager.resolutionDropdown = resolutionDropdown;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resolutionDropdown.onValueChanged, settingsManager.SetResolution);

        // Create Quality Dropdown
        TMP_Dropdown qualityDropdown = CreateDropdownSetting(settingsContainer, "Graphics Quality");
        settingsManager.qualityDropdown = qualityDropdown;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(qualityDropdown.onValueChanged, settingsManager.SetQuality);

        // Create Fullscreen Toggle
        Toggle fullscreenToggle = CreateToggleSetting(settingsContainer, "Fullscreen");
        settingsManager.fullscreenToggle = fullscreenToggle;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(fullscreenToggle.onValueChanged, settingsManager.SetFullscreen);

        // Create Volume Slider
        var volumeResult = CreateSliderSetting(settingsContainer, "Volume");
        settingsManager.volumeSlider = volumeResult.slider;
        settingsManager.volumeText = volumeResult.valueText;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(volumeResult.slider.onValueChanged, settingsManager.SetVolume);

        // Create Close Button
        GameObject closeButton = CreateMenuButton(contentPanel, "Close", "HideSettings");
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.05f);
        closeRect.anchorMax = new Vector2(0.5f, 0.05f);
        closeRect.sizeDelta = new Vector2(200, 50);

        Button closeBtn = closeButton.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, settingsManager.HideSettings);

        settingsPanel.SetActive(false);

        Debug.Log("✅ Settings Panel created with all controls!");
    }

    static TMP_Dropdown CreateDropdownSetting(GameObject parent, string labelText)
    {
        GameObject settingRow = new GameObject(labelText + "Setting");
        settingRow.transform.SetParent(parent.transform, false);

        RectTransform rowRect = settingRow.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 60);

        HorizontalLayoutGroup rowLayout = settingRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;
        rowLayout.spacing = 20;

        // Create Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(settingRow.transform, false);

        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 250;

        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText + ":";
        label.fontSize = 24;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.color = Color.white;

        // Create Dropdown
        GameObject dropdownObj = new GameObject("Dropdown");
        dropdownObj.transform.SetParent(settingRow.transform, false);

        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = dropdownBg;

        // Create Caption Text (Selected Value)
        GameObject captionObj = new GameObject("Caption");
        captionObj.transform.SetParent(dropdownObj.transform, false);

        RectTransform captionRect = captionObj.AddComponent<RectTransform>();
        captionRect.anchorMin = Vector2.zero;
        captionRect.anchorMax = Vector2.one;
        captionRect.offsetMin = new Vector2(10, 0);
        captionRect.offsetMax = new Vector2(-25, 0); // Leave room for arrow

        TextMeshProUGUI captionText = captionObj.AddComponent<TextMeshProUGUI>();
        captionText.fontSize = 24;
        captionText.alignment = TextAlignmentOptions.MidlineLeft;
        captionText.color = Color.white;
        
        dropdown.captionText = captionText;

        // Create Arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchoredPosition = new Vector2(-15, 0);

        Image arrowImage = arrowObj.AddComponent<Image>();
        arrowImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        // Ideally set a sprite here, but color works for now

        // Create dropdown template (required for TMP_Dropdown)
        RectTransform template = CreateDropdownTemplate(dropdownObj, dropdown);
        dropdown.template = template;

        return dropdown;
    }

    static RectTransform CreateDropdownTemplate(GameObject dropdownObj, TMP_Dropdown dropdown)
    {
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropdownObj.transform, false);

        RectTransform templateRect = templateObj.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.sizeDelta = new Vector2(0, 150);
        templateRect.anchoredPosition = new Vector2(0, 2);

        Image templateBg = templateObj.AddComponent<Image>();
        templateBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        
        // Add ScrollRect
        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
        scrollRect.content = null; // Will set later
        scrollRect.viewport = null; // Will set later
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.verticalScrollbar = null;

        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);

        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(0,0,0,0); // Invisible mask
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        scrollRect.viewport = viewportRect;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0, 28);

        // Add Layout Components for proper list display
        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlHeight = true; // Changed to true to enforce item height
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.spacing = 2; // Add slight spacing between items

        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;

        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);

        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 30);

        // Add LayoutElement to ensure item has height in list
        LayoutElement itemLayout = itemObj.AddComponent<LayoutElement>();
        itemLayout.minHeight = 30;
        itemLayout.preferredHeight = 30;
        itemLayout.flexibleHeight = 0;

        Toggle itemToggle = itemObj.AddComponent<Toggle>();
        
        GameObject itemBackgroundObj = new GameObject("Item Background");
        itemBackgroundObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemBgRect = itemBackgroundObj.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.sizeDelta = Vector2.zero;
        Image itemBgImage = itemBackgroundObj.AddComponent<Image>();
        itemBgImage.color = new Color(1, 1, 1, 0.05f); // Slight visibility for debug/hover
        itemToggle.targetGraphic = itemBgImage;

        GameObject itemCheckmarkObj = new GameObject("Item Checkmark");
        itemCheckmarkObj.transform.SetParent(itemObj.transform, false);
        RectTransform itemCheckRect = itemCheckmarkObj.AddComponent<RectTransform>();
        itemCheckRect.anchorMin = new Vector2(0, 0.5f);
        itemCheckRect.anchorMax = new Vector2(0, 0.5f);
        itemCheckRect.sizeDelta = new Vector2(20, 20);
        itemCheckRect.anchoredPosition = new Vector2(10, 0);
        Image itemCheckImage = itemCheckmarkObj.AddComponent<Image>();
        itemCheckImage.color = Color.green;
        itemToggle.graphic = itemCheckImage;

        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);

        RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(30, 0); // Make room for checkmark
        itemLabelRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI itemLabel = itemLabelObj.AddComponent<TextMeshProUGUI>();
        itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
        itemLabel.fontSize = 20;
        itemLabel.color = Color.white;
        itemLabel.textWrappingMode = TextWrappingModes.NoWrap;
        itemLabel.overflowMode = TextOverflowModes.Ellipsis;
        
        // Assign Item Text to Dropdown
        dropdown.itemText = itemLabel;

        templateObj.SetActive(false);

        return templateRect;
    }

    static Toggle CreateToggleSetting(GameObject parent, string labelText)
    {
        GameObject settingRow = new GameObject(labelText + "Setting");
        settingRow.transform.SetParent(parent.transform, false);

        RectTransform rowRect = settingRow.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 60);

        HorizontalLayoutGroup rowLayout = settingRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;
        rowLayout.spacing = 20;

        // Create Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(settingRow.transform, false);

        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 250;

        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText + ":";
        label.fontSize = 24;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.color = Color.white;

        // Create Toggle
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(settingRow.transform, false);

        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.sizeDelta = new Vector2(60, 40);

        Image toggleBg = toggleObj.AddComponent<Image>();
        toggleBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        // Create checkmark
        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(toggleObj.transform, false);

        RectTransform checkRect = checkmarkObj.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = new Vector2(-10, -10);

        Image checkmark = checkmarkObj.AddComponent<Image>();
        checkmark.color = new Color(0.3f, 0.7f, 0.3f, 1f);

        toggle.graphic = checkmark;

        return toggle;
    }

    static (Slider slider, TextMeshProUGUI valueText) CreateSliderSetting(GameObject parent, string labelText)
    {
        GameObject settingRow = new GameObject(labelText + "Setting");
        settingRow.transform.SetParent(parent.transform, false);

        RectTransform rowRect = settingRow.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 60);

        HorizontalLayoutGroup rowLayout = settingRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;
        rowLayout.spacing = 20;

        // Create Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(settingRow.transform, false);

        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 150;

        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText + ":";
        label.fontSize = 24;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.color = Color.white;

        // Create Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(settingRow.transform, false);

        LayoutElement sliderLayout = sliderObj.AddComponent<LayoutElement>();
        sliderLayout.preferredWidth = 300;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -10);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.5f, 0.7f, 1f);

        slider.fillRect = fillRect;

        // Handle
        GameObject handleAreaObj = new GameObject("Handle Slide Area");
        handleAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-10, 0);

        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);

        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);

        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        // Create Value Text
        GameObject valueTextObj = new GameObject("ValueText");
        valueTextObj.transform.SetParent(settingRow.transform, false);

        LayoutElement valueLayout = valueTextObj.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 80;

        TextMeshProUGUI valueText = valueTextObj.AddComponent<TextMeshProUGUI>();
        valueText.text = "100%";
        valueText.fontSize = 24;
        valueText.alignment = TextAlignmentOptions.MidlineRight;
        valueText.color = Color.white;

        return (slider, valueText);
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
