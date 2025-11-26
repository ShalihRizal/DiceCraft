using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixProjectilePrefabs : MonoBehaviour
{
    [MenuItem("Debug/Fix Projectile Prefabs")]
    public static void FixPrefabs()
    {
        Debug.Log("--- Starting Projectile Prefab Fix ---");

        // 1. Find all Enemy prefabs or instances in the project/scene
        // It's safer to look for all prefabs in the project that might be projectiles
        // But we don't know which ones are projectiles.
        // Better strategy: Find all Enemy components, get their projectilePrefab, and fix those.

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;

        HashSet<GameObject> checkedPrefabs = new HashSet<GameObject>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            // Check if this prefab has an Enemy script
            Enemy enemy = prefab.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (enemy.projectilePrefab != null)
                {
                    FixProjectile(enemy.projectilePrefab, ref fixedCount, checkedPrefabs);
                }
            }
        }

        // Also check Enemies in the current scene
        Enemy[] sceneEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in sceneEnemies)
        {
             if (enemy.projectilePrefab != null)
            {
                FixProjectile(enemy.projectilePrefab, ref fixedCount, checkedPrefabs);
            }
        }

        Debug.Log($"--- Finished. Fixed {fixedCount} projectile prefabs. ---");
        AssetDatabase.SaveAssets();
    }

    static void FixProjectile(GameObject projPrefab, ref int count, HashSet<GameObject> checkedPrefabs)
    {
        if (checkedPrefabs.Contains(projPrefab)) return;
        checkedPrefabs.Add(projPrefab);

        if (projPrefab.GetComponent<Projectile>() == null)
        {
            Debug.LogWarning($"⚠ Prefab '{projPrefab.name}' is missing Projectile component. Adding it...");
            
            // We need to modify the asset
            string assetPath = AssetDatabase.GetAssetPath(projPrefab);
            if (!string.IsNullOrEmpty(assetPath))
            {
                // It's a prefab asset
                using (var editScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                {
                    GameObject prefabRoot = editScope.prefabContentsRoot;
                    if (prefabRoot.GetComponent<Projectile>() == null)
                    {
                        prefabRoot.AddComponent<Projectile>();
                        Debug.Log($"✅ Added Projectile component to '{projPrefab.name}'");
                        count++;
                    }
                }
            }
            else
            {
                // It's likely a scene object or not a proper asset?
                // If it's a scene object, we can just add it directly?
                // But projectilePrefab should be a prefab asset usually.
                Debug.LogWarning($"Could not edit '{projPrefab.name}' as a prefab asset (Path empty).");
            }
        }
        else
        {
            Debug.Log($"✔ Prefab '{projPrefab.name}' already has Projectile component.");
        }
    }
}
