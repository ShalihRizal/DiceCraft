using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TestSceneSetup : EditorWindow
{
    [MenuItem("Tools/Generate Test Scene")]
    public static void GenerateScene()
    {
        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 1. Setup Camera
        GameObject cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        Camera cam = cameraGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.2f, 0.3f, 0.5f);
        cameraGO.transform.position = new Vector3(0, 0, -10);
        cameraGO.AddComponent<AudioListener>();

        // 2. Setup EventSystem
        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();

        // 3. Setup GameManager
        GameObject gmGO = new GameObject("GameManager");
        GameManager gm = gmGO.AddComponent<GameManager>();

        // 3.5 Setup PlayerCurrency
        GameObject currencyGO = new GameObject("PlayerCurrency");
        currencyGO.AddComponent<PlayerCurrency>();

        // 4. Setup ObjectPooler
        GameObject poolerGO = new GameObject("ObjectPooler");
        ObjectPooler pooler = poolerGO.AddComponent<ObjectPooler>();
        
        GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProjectilePrefabs/Projectile.prefab");
        if (projectilePrefab == null) projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProjectilePrefabs/Bullet.prefab");
        
        pooler.pools = new List<ObjectPooler.Pool>();
        if (projectilePrefab != null)
        {
            ObjectPooler.Pool enemyProjectilePool = new ObjectPooler.Pool
            {
                tag = "EnemyProjectile",
                prefab = projectilePrefab,
                size = 20
            };
            pooler.pools.Add(enemyProjectilePool);

            if (projectilePrefab.GetComponent<Collider2D>() == null)
                Debug.LogWarning("⚠️ Projectile prefab missing Collider2D!");
            
            if (projectilePrefab.GetComponent<Projectile>() == null)
                Debug.LogWarning("⚠️ Projectile prefab missing Projectile script!");
        }
        else
        {
            Debug.LogError("Could not find Projectile prefab!");
        }

        // 5. Setup UI Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 6. Setup Grid & Dice Spawner
        GameObject gridGO = new GameObject("GridSpawner");
        GridSpawner gridSpawner = gridGO.AddComponent<GridSpawner>();
        gridSpawner.cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Miscellaneous/Cell.prefab");
        gridSpawner.rows = 3;
        gridSpawner.columns = 3;
        gridSpawner.cellSize = 1.2f;
        gridSpawner.spacing = 0.1f;
        gridGO.transform.position = new Vector3(0, -2, 0);

        ConfigureDiceAssets();

        GameObject diceSpawnerGO = new GameObject("DiceSpawner");
        DiceSpawner diceSpawner = diceSpawnerGO.AddComponent<DiceSpawner>();
        diceSpawner.gridGenerator = gridSpawner;
        diceSpawner.dicePool = AssetDatabase.LoadAssetAtPath<DicePool>("Assets/ScriptableObjects/Miscellaneous/DicePool.asset");
        diceSpawner.startWithDiceCount = 2;

        // 7. Setup TrashZone
        GameObject trashGO = new GameObject("TrashZone");
        trashGO.AddComponent<TrashZone>();
        BoxCollider2D trashCol = trashGO.AddComponent<BoxCollider2D>();
        trashCol.isTrigger = true;
        trashCol.size = new Vector2(2.5f, 2.5f);
        trashGO.transform.position = new Vector3(-3, -2, 0);
        
        GameObject trashVisual = new GameObject("Visual");
        trashVisual.transform.SetParent(trashGO.transform, false);
        SpriteRenderer trashSr = trashVisual.AddComponent<SpriteRenderer>();
        trashSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/40+ Simple Icons - Free/TrashCan_Simple_Icons_UI.png");
        trashSr.color = Color.white;
        trashVisual.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        trashSr.sortingOrder = -1;

        // 9. Setup Currency UI
        GameObject currencyUIGO = new GameObject("CurrencyUI");
        currencyUIGO.transform.SetParent(canvasGO.transform, false);
        CurrencyUI currencyUI = currencyUIGO.AddComponent<CurrencyUI>();

        GameObject currencyTextGO = new GameObject("CurrencyText");
        currencyTextGO.transform.SetParent(currencyUIGO.transform, false);
        TextMeshProUGUI currencyText = currencyTextGO.AddComponent<TextMeshProUGUI>();
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

        // 10. Setup Player
        GameObject playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.transform.position = new Vector3(0, 3, 0);
        
        BoxCollider2D playerCol = playerGO.AddComponent<BoxCollider2D>();
        playerCol.isTrigger = true;
        playerCol.size = new Vector2(1f, 1f);
        
        Rigidbody2D rb = playerGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        PlayerHealth ph = playerGO.AddComponent<PlayerHealth>();
        ph.maxHealth = 100;

        // UI: Health & Shield
        GameObject healthSliderGO = CreateSlider("HealthSlider", canvasGO.transform, Color.red, new Vector2(0, -15), false);
        ph.healthSlider = healthSliderGO.GetComponent<Slider>();

        GameObject shieldSliderGO = CreateSlider("ShieldSlider", canvasGO.transform, Color.blue, new Vector2(0, -15), true);
        shieldSliderGO.transform.SetAsLastSibling();
        ph.shieldSlider = shieldSliderGO.GetComponent<Slider>();

        GameObject textGO = new GameObject("PercentageText");
        textGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "100%";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1);
        textRect.anchorMax = new Vector2(0.5f, 1);
        textRect.anchoredPosition = new Vector2(0, -50);
        ph.percentageText = tmp;
        textGO.transform.SetAsLastSibling();

        // 11. Setup EnemySpawner
        GameObject spawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyPrefabs/Enemy.prefab");
        if (enemyPrefab == null)
        {
            Debug.LogError("Could not find Enemy prefab!");
        }

        // 12. Game Over UI
        SetupGameOverUI(canvasGO, gmGO);

        // 13. Start Combat UI
        SetupCombatUI(canvasGO);

        // 14. Reward UI
        SetupRewardUI(canvasGO);

        // 15. Wave Progress UI
        SetupWaveProgressUI(canvasGO);

        // Configure Waves
        if (enemyPrefab != null)
        {
            spawner.waves = new List<WaveConfig>();
            
            spawner.waves.Add(new WaveConfig 
            { 
                enemyPrefab = enemyPrefab, 
                count = 3
            });

            spawner.waves.Add(new WaveConfig 
            { 
                enemyPrefab = enemyPrefab, 
                count = 5
            });
        }

        Debug.Log("Test Scene Generated! Press Play to test.");
    }

    private static void SetupCombatUI(GameObject canvas)
    {
        GameObject combatUIGO = new GameObject("CombatUI");
        combatUIGO.transform.SetParent(canvas.transform, false);
        CombatUI combatUI = combatUIGO.AddComponent<CombatUI>();

        GameObject btnGO = new GameObject("StartCombatButton");
        btnGO.transform.SetParent(combatUIGO.transform, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = Color.green;
        Button btn = btnGO.AddComponent<Button>();
        
        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 100);
        rect.sizeDelta = new Vector2(200, 50);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "START COMBAT";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        combatUI.startCombatButton = btn;
    }

    private static void SetupRewardUI(GameObject canvas)
    {
        GameObject rewardManagerGO = new GameObject("RewardManager");
        RewardManager rewardManager = rewardManagerGO.AddComponent<RewardManager>();

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
    }

    private static void SetupGameOverUI(GameObject canvas, GameObject gmGO)
    {
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        Image panelImg = gameOverPanel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        gameOverPanel.SetActive(false);

        GameObject gameOverTextGO = new GameObject("GameOverText");
        gameOverTextGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI goText = gameOverTextGO.AddComponent<TextMeshProUGUI>();
        goText.text = "GAME OVER";
        goText.fontSize = 48;
        goText.alignment = TextAlignmentOptions.Center;
        goText.color = Color.white;

        GameObject restartButtonGO = new GameObject("RestartButton");
        restartButtonGO.transform.SetParent(gameOverPanel.transform, false);
        Image btnImg = restartButtonGO.AddComponent<Image>();
        btnImg.color = Color.red;
        Button restartBtn = restartButtonGO.AddComponent<Button>();

        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(restartButtonGO.transform, false);
        TextMeshProUGUI btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnText.text = "RESTART";
        btnText.fontSize = 24;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        GameOverUI gameOverUI = gmGO.AddComponent<GameOverUI>();
        gameOverUI.gameOverPanel = gameOverPanel;
        gameOverUI.restartButton = restartBtn;
        gameOverUI.gameOverText = goText;
    }

    private static GameObject CreateSlider(string name, Transform parent, Color fillColor, Vector2 position, bool isOverlay)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent, false);
        
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1);
        sliderRect.anchorMax = new Vector2(1, 1);
        sliderRect.pivot = new Vector2(0.5f, 1);
        sliderRect.anchoredPosition = position;
        sliderRect.sizeDelta = new Vector2(0, 30);

        Slider slider = sliderGO.AddComponent<Slider>();
        
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = background.AddComponent<Image>();
        bgImg.color = isOverlay ? new Color(fillColor.r, fillColor.g, fillColor.b, 0.3f) : new Color(0.2f, 0.2f, 0.2f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;

        slider.fillRect = fillRect;
        slider.value = 1f;
        slider.interactable = false;

        return sliderGO;
    }

    private static void ConfigureDiceAssets()
    {
        string[] diceDataPaths = AssetDatabase.FindAssets("t:DiceData");
        
        foreach (string guid in diceDataPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DiceData data = AssetDatabase.LoadAssetAtPath<DiceData>(path);
            
            if (data != null)
            {
                UpdateDiceData(data);
                EditorUtility.SetDirty(data);
            }
        }
        
        AssetDatabase.SaveAssets();
    }

    private static void UpdateDiceData(DiceData data)
    {
        // Set canAttack flag for all dice
        data.canAttack = true;
        
        // Note: Visual effects are assigned directly in the DiceData ScriptableObject assets
    }

    private static void SetupWaveProgressUI(GameObject canvas)
    {
        GameObject waveProgressGO = new GameObject("WaveProgressUI");
        waveProgressGO.transform.SetParent(canvas.transform, false);
        WaveProgressUI waveProgressUI = waveProgressGO.AddComponent<WaveProgressUI>();

        // Wave Text (e.g., "Wave 1")
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

        // Enemy Count Text (e.g., "0/5")
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
    }
}

