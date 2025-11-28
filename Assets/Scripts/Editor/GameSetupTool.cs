using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class GameSetupTool : EditorWindow
{
    [MenuItem("Tools/DiceCraft/Validate & Setup")]
    public static void ShowWindow()
    {
        GetWindow<GameSetupTool>("Game Setup Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("DiceCraft Game Setup", EditorStyles.boldLabel);

        if (GUILayout.Button("Validate Scene"))
        {
            ValidateScene();
        }

        if (GUILayout.Button("Fix / Setup Scene"))
        {
            SetupScene();
        }
    }

    private static void ValidateScene()
    {
        Debug.Log("🔍 Starting Scene Validation...");

        CheckManager<GameManager>("GameManager");
        CheckManager<GridSpawner>("GridSpawner");
        CheckManager<DiceSpawner>("DiceSpawner");
        CheckManager<EnemySpawner>("EnemySpawner");
        CheckManager<RelicManager>("RelicManager");
        // CheckManager<ShopManager>("ShopManager"); // Shop disabled

        CheckManager<UIManager>("UIManager");

        ValidateDiceSpawner();
        ValidateGridSpawner();
        ValidateEnemySpawner();
        ValidateDiceData();

        Debug.Log("✅ Validation Complete.");
    }

    private static void SetupScene()
    {
        Debug.Log("🛠 Starting Scene Setup...");

        EnsureManager<GameManager>("GameManager");
        EnsureManager<GridSpawner>("GridSpawner");
        EnsureManager<DiceSpawner>("DiceSpawner");
        EnsureManager<EnemySpawner>("EnemySpawner");
        EnsureManager<RelicManager>("RelicManager");
        // EnsureManager<ShopManager>("ShopManager"); // Shop disabled
        EnsureManager<UIManager>("UIManager");

        AutoAssignReferences();

        Debug.Log("✨ Setup Complete.");
    }

    private static void AutoAssignReferences()
    {
        var diceSpawner = FindFirstObjectByType<DiceSpawner>();
        var gridSpawner = FindFirstObjectByType<GridSpawner>();
        var enemySpawner = FindFirstObjectByType<EnemySpawner>();

        // 1. Link GridSpawner to DiceSpawner
        if (diceSpawner != null && gridSpawner != null)
        {
            if (diceSpawner.gridGenerator == null)
            {
                diceSpawner.gridGenerator = gridSpawner;
                Debug.Log("🔗 Linked GridSpawner to DiceSpawner.");
                EditorUtility.SetDirty(diceSpawner);
            }
        }

        // 2. Assign DicePool
        if (diceSpawner != null && diceSpawner.dicePool == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:DicePool");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                diceSpawner.dicePool = AssetDatabase.LoadAssetAtPath<DicePool>(path);
                Debug.Log($"🔗 Assigned DicePool: {diceSpawner.dicePool.name}");
                EditorUtility.SetDirty(diceSpawner);
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find any DicePool asset to assign!");
            }
        }

        // 3. Assign Cell Prefab
        if (gridSpawner != null && gridSpawner.cellPrefab == null)
        {
            GameObject cellPrefab = FindPrefab("Cell");
            if (cellPrefab != null)
            {
                gridSpawner.cellPrefab = cellPrefab;
                Debug.Log($"🔗 Assigned Cell Prefab: {cellPrefab.name}");
                EditorUtility.SetDirty(gridSpawner);
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find a prefab named 'Cell'!");
            }
        }

        // 4. Setup Default Wave if empty
        if (enemySpawner != null && enemySpawner.waves.Count == 0)
        {
            GameObject enemyPrefab = FindPrefab("Enemy");
            if (enemyPrefab != null)
            {
                WaveConfig defaultWave = new WaveConfig
                {
                    enemyPrefab = enemyPrefab,
                    count = 5
                };
                enemySpawner.waves.Add(defaultWave);
                Debug.Log($"🔗 Added Default Wave with Enemy: {enemyPrefab.name}");
                EditorUtility.SetDirty(enemySpawner);
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find a prefab named 'Enemy' to create a default wave!");
            }
        }
    }

    private static GameObject FindPrefab(string name)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Prefab {name}");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go != null && go.name.Contains(name)) // Double check name match
            {
                return go;
            }
        }
        return null;
    }

    private static void CheckManager<T>(string name) where T : MonoBehaviour
    {
        var obj = FindFirstObjectByType<T>();
        if (obj == null)
        {
            Debug.LogError($"❌ Missing {name} in the scene!");
        }
        else
        {
            Debug.Log($"✅ Found {name}.");
        }
    }

    private static void EnsureManager<T>(string name) where T : MonoBehaviour
    {
        var obj = FindFirstObjectByType<T>();
        if (obj == null)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
            Debug.Log($"🛠 Created missing {name}.");
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        }
    }

    private static void ValidateDiceSpawner()
    {
        var spawner = FindFirstObjectByType<DiceSpawner>();
        if (spawner != null)
        {
            if (spawner.gridGenerator == null) Debug.LogWarning("⚠️ DiceSpawner is missing GridGenerator reference.");
            if (spawner.dicePool == null) Debug.LogError("❌ DiceSpawner is missing DicePool assignment!");
        }
    }

    private static void ValidateGridSpawner()
    {
        var spawner = FindFirstObjectByType<GridSpawner>();
        if (spawner != null)
        {
            if (spawner.cellPrefab == null) Debug.LogError("❌ GridSpawner is missing Cell Prefab!");
        }
    }

    private static void ValidateEnemySpawner()
    {
        var spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            if (spawner.waves.Count == 0) Debug.LogWarning("⚠️ EnemySpawner has no waves configured!");
        }
    }

    private static void ValidateDiceData()
    {
        string[] guids = AssetDatabase.FindAssets("t:DiceData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DiceData data = AssetDatabase.LoadAssetAtPath<DiceData>(path);
            
            if (data != null)
            {
                if (data.prefab == null) Debug.LogWarning($"⚠️ DiceData '{data.name}' is missing Prefab!");
                if (data.passive == null) Debug.LogWarning($"⚠️ DiceData '{data.name}' is missing Passive!");
                
                // Check VFX
                if (data.vfxSpawn == null) Debug.LogWarning($"ℹ️ DiceData '{data.name}' has no Spawn VFX.");
                // Add more checks if needed
            }
        }
    }
}
