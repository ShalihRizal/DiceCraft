using UnityEngine;
using UnityEditor;

public class ConfigureEnemySpawner : EditorWindow
{
    [MenuItem("Tools/Configure Enemy Spawner Waves")]
    public static void ConfigureWaves()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        
        if (spawner == null)
        {
            EditorUtility.DisplayDialog("Error", "No EnemySpawner found in the scene!", "OK");
            return;
        }

        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyPrefabs/Enemy.prefab");
        
        if (enemyPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Enemy prefab not found at:\nAssets/Prefabs/EnemyPrefabs/Enemy.prefab", "OK");
            return;
        }

        spawner.waves.Clear();

        // Wave 1: 3 enemies
        spawner.waves.Add(new WaveConfig
        {
            enemyPrefab = enemyPrefab,
            count = 3
        });

        // Wave 2: 5 enemies
        spawner.waves.Add(new WaveConfig
        {
            enemyPrefab = enemyPrefab,
            count = 5
        });

        // Wave 3: 7 enemies
        spawner.waves.Add(new WaveConfig
        {
            enemyPrefab = enemyPrefab,
            count = 7
        });

        EditorUtility.SetDirty(spawner);
        
        Debug.Log("✅ Configured EnemySpawner with 3 waves:");
        Debug.Log("   Wave 1: 3 enemies");
        Debug.Log("   Wave 2: 5 enemies");
        Debug.Log("   Wave 3: 7 enemies");
        
        EditorUtility.DisplayDialog("Success", "EnemySpawner configured with 3 waves!\n\nWave 1: 3 enemies\nWave 2: 5 enemies\nWave 3: 7 enemies\n\nAll enemies spawn immediately per wave.\n1 second delay between waves.\n\nPress Fight to test!", "OK");
    }
}
