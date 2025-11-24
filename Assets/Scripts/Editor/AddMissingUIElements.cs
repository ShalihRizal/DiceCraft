using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AddMissingUIElements : EditorWindow
{
    [MenuItem("Tools/Add Missing UI Elements to SampleScene")]
    public static void AddMissingElements()
    {
        // Find Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene! Please add a Canvas first.");
            return;
        }

        GameObject canvasGO = canvas.gameObject;

        // 1. Add CurrencyUI wrapper if needed
        SetupCurrencyUI(canvasGO);

        // 2. Add CombatUI wrapper if needed
        SetupCombatUI(canvasGO);

        // 3. Add Wave Progress UI
        SetupWaveProgressUI(canvasGO);

        // 4. Add Reward System
        SetupRewardSystem(canvasGO);

        Debug.Log("✅ Missing UI elements added successfully!");
        EditorUtility.DisplayDialog("Success", "Missing UI elements have been added to SampleScene!\n\nCheck the Console for details.", "OK");
    }

    private static void SetupCurrencyUI(GameObject canvas)
    {
        // Check if CurrencyUI already exists
        CurrencyUI existingCurrencyUI = FindFirstObjectByType<CurrencyUI>();
        if (existingCurrencyUI != null)
        {
            Debug.Log("CurrencyUI already exists, skipping...");
            return;
        }

        // Find existing CurrencyText
        TextMeshProUGUI currencyText = null;
        foreach (TextMeshProUGUI tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp.name == "CurrencyText")
            {
                currencyText = tmp;
                break;
            }
        }

        if (currencyText == null)
        {
            Debug.LogWarning("CurrencyText not found, creating new one...");
            
            GameObject currencyUIGO = new GameObject("CurrencyUI");
            currencyUIGO.transform.SetParent(canvas.transform, false);
            CurrencyUI currencyUI = currencyUIGO.AddComponent<CurrencyUI>();

            GameObject currencyTextGO = new GameObject("CurrencyText");
            currencyTextGO.transform.SetParent(currencyUIGO.transform, false);
            currencyText = currencyTextGO.AddComponent<TextMeshProUGUI>();
            currencyText.text = "Gold: 0";
            currencyText.fontSize = 28;
            currencyText.alignment = TextAlignmentOptions.TopRight;
            currencyText.color = Color.yellow;
            
            RectTransform currencyRect = currencyTextGO.GetComponent<RectTransform>();
            currencyRect.anchorMin = new Vector2(1, 1);
            currencyRect.anchorMax = new Vector2(1, 1);
            currencyRect.pivot = new Vector2(1, 1);
            currencyRect.anchoredPosition = new Vector2(-20, -20);
            currencyRect.sizeDelta = new Vector2(200, 40);

            currencyUI.currencyText = currencyText;
            Debug.Log("✅ Created CurrencyUI");
        }
        else
        {
            // Wrap existing CurrencyText
            GameObject currencyUIGO = new GameObject("CurrencyUI");
            currencyUIGO.transform.SetParent(canvas.transform, false);
            currencyUIGO.transform.SetSiblingIndex(currencyText.transform.GetSiblingIndex());
            
            currencyText.transform.SetParent(currencyUIGO.transform, true);
            
            CurrencyUI currencyUI = currencyUIGO.AddComponent<CurrencyUI>();
            currencyUI.currencyText = currencyText;
            Debug.Log("✅ Wrapped existing CurrencyText with CurrencyUI");
        }
    }

    private static void SetupCombatUI(GameObject canvas)
    {
        // Check if CombatUI already exists
        CombatUI existingCombatUI = FindFirstObjectByType<CombatUI>();
        if (existingCombatUI != null)
        {
            Debug.Log("CombatUI already exists, skipping...");
            return;
        }

        // Find existing StartCombatButton
        Button startButton = null;
        foreach (Button btn in canvas.GetComponentsInChildren<Button>(true))
        {
            if (btn.name == "StartCombatButton")
            {
                startButton = btn;
                break;
            }
        }

        if (startButton != null)
        {
            // Wrap existing button
            GameObject combatUIGO = new GameObject("CombatUI");
            combatUIGO.transform.SetParent(canvas.transform, false);
            combatUIGO.transform.SetSiblingIndex(startButton.transform.GetSiblingIndex());
            
            startButton.transform.SetParent(combatUIGO.transform, true);
            
            CombatUI combatUI = combatUIGO.AddComponent<CombatUI>();
            combatUI.startCombatButton = startButton;
            Debug.Log("✅ Wrapped existing StartCombatButton with CombatUI");
        }
        else
        {
            Debug.LogWarning("StartCombatButton not found, skipping CombatUI setup");
        }
    }

    private static void SetupWaveProgressUI(GameObject canvas)
    {
        // Check if WaveProgressUI already exists
        WaveProgressUI existingWaveUI = FindFirstObjectByType<WaveProgressUI>();
        if (existingWaveUI != null)
        {
            Debug.Log("WaveProgressUI already exists, skipping...");
            return;
        }

        GameObject waveProgressGO = new GameObject("WaveProgressUI");
        waveProgressGO.transform.SetParent(canvas.transform, false);
        WaveProgressUI waveProgressUI = waveProgressGO.AddComponent<WaveProgressUI>();

        // Wave Text
        GameObject waveTextGO = new GameObject("WaveText");
        waveTextGO.transform.SetParent(waveProgressGO.transform, false);
        TextMeshProUGUI waveText = waveTextGO.AddComponent<TextMeshProUGUI>();
        waveText.text = "Wave 1";
        waveText.fontSize = 24;
        waveText.alignment = TextAlignmentOptions.TopLeft;
        waveText.color = Color.white;
        
        RectTransform waveTextRect = waveTextGO.GetComponent<RectTransform>();
        waveTextRect.anchorMin = new Vector2(0, 1);
        waveTextRect.anchorMax = new Vector2(0, 1);
        waveTextRect.pivot = new Vector2(0, 1);
        waveTextRect.anchoredPosition = new Vector2(20, -80);
        waveTextRect.sizeDelta = new Vector2(150, 30);

        // Progress Slider
        GameObject sliderGO = new GameObject("ProgressSlider");
        sliderGO.transform.SetParent(waveProgressGO.transform, false);
        
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(0, 1);
        sliderRect.pivot = new Vector2(0, 1);
        sliderRect.anchoredPosition = new Vector2(20, -115);
        sliderRect.sizeDelta = new Vector2(200, 20);

        Slider slider = sliderGO.AddComponent<Slider>();
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = background.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;

        slider.fillRect = fillRect;
        slider.value = 0f;
        slider.interactable = false;

        // Enemy Count Text
        GameObject countTextGO = new GameObject("EnemyCountText");
        countTextGO.transform.SetParent(waveProgressGO.transform, false);
        TextMeshProUGUI countText = countTextGO.AddComponent<TextMeshProUGUI>();
        countText.text = "0/0";
        countText.fontSize = 18;
        countText.alignment = TextAlignmentOptions.Center;
        countText.color = Color.white;
        
        RectTransform countTextRect = countTextGO.GetComponent<RectTransform>();
        countTextRect.anchorMin = new Vector2(0, 1);
        countTextRect.anchorMax = new Vector2(0, 1);
        countTextRect.pivot = new Vector2(0, 1);
        countTextRect.anchoredPosition = new Vector2(230, -115);
        countTextRect.sizeDelta = new Vector2(60, 20);

        // Assign to WaveProgressUI
        waveProgressUI.progressSlider = slider;
        waveProgressUI.waveText = waveText;
        waveProgressUI.enemyCountText = countText;

        Debug.Log("✅ Created WaveProgressUI");
    }

    private static void SetupRewardSystem(GameObject canvas)
    {
        // Check if RewardManager already exists
        RewardManager existingRewardManager = FindFirstObjectByType<RewardManager>();
        if (existingRewardManager != null)
        {
            Debug.Log("RewardManager already exists, skipping...");
            return;
        }

        // Create RewardManager
        GameObject rewardManagerGO = new GameObject("RewardManager");
        RewardManager rewardManager = rewardManagerGO.AddComponent<RewardManager>();

        // Create Reward Panel
        GameObject rewardPanelGO = new GameObject("RewardPanel");
        rewardPanelGO.transform.SetParent(canvas.transform, false);
        Image panelImg = rewardPanelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.9f);
        RectTransform panelRect = rewardPanelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = new Vector2(50, 50);
        panelRect.offsetMax = new Vector2(-50, -50);

        RewardUI rewardUI = rewardPanelGO.AddComponent<RewardUI>();
        rewardUI.panel = rewardPanelGO;
        rewardUI.rewardButtons = new Button[3];
        rewardUI.rewardTexts = new TextMeshProUGUI[3];

        rewardManager.rewardUI = rewardUI;

        // Create 3 reward buttons
        for (int i = 0; i < 3; i++)
        {
            GameObject btnGO = new GameObject($"RewardButton_{i}");
            btnGO.transform.SetParent(rewardPanelGO.transform, false);
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = Color.white;
            Button btn = btnGO.AddComponent<Button>();
            
            RectTransform btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.sizeDelta = new Vector2(200, 60);
            btnRect.anchoredPosition = new Vector2(0, 100 - (i * 80));

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Reward";
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            rewardUI.rewardButtons[i] = btn;
            rewardUI.rewardTexts[i] = tmp;
        }

        rewardPanelGO.SetActive(false);
        Debug.Log("✅ Created Reward System (RewardManager + RewardUI)");
    }
}
