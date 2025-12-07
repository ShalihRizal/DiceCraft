using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Encounter System")]
    [Tooltip("Current encounter to spawn")]
    public EncounterConfig currentEncounter;
    
    [Tooltip("Fallback: Spawn random enemies if no encounter is set")]
    public List<EnemyData> randomEnemyPool = new List<EnemyData>();
    public int randomEnemyCount = 3;

    [Header("Enemy Prefab")]
    [Tooltip("Base enemy prefab (will be configured with EnemyData)")]
    public GameObject enemyPrefab;

    public static HashSet<Enemy> activeEnemies = new HashSet<Enemy>();
    private bool isSpawning = false;

    void Awake()
    {
        activeEnemies.Clear();
    }

    void Start()
    {
        GameEvents.OnCombatStarted += StartCombat;
    }

    void OnDestroy()
    {
        GameEvents.OnCombatStarted -= StartCombat;
    }

    public void StartCombat()
    {
        if (isSpawning)
        {
            Debug.LogWarning("⚠️ Combat already in progress, ignoring duplicate call");
            return;
        }

        isSpawning = true;
        activeEnemies.Clear();
        
        // Determine encounter based on current node
        DetermineEncounter();
        
        StartCoroutine(SpawnEncounter());
    }

    void DetermineEncounter()
    {
        // Check if MapManager has a specific encounter for this node
        if (MapManager.Instance != null && MapManager.Instance.currentNode != null)
        {
            NodeType nodeType = MapManager.Instance.currentNode.nodeType;
            
            // Generate encounter based on node type
            currentEncounter = GenerateEncounterForNode(nodeType);
        }
        
        // If still no encounter, use fallback
        if (currentEncounter == null)
        {
            Debug.LogWarning("⚠️ No encounter configured, using random enemies");
        }
    }

    EncounterConfig GenerateEncounterForNode(NodeType nodeType)
    {
        // Create a runtime encounter based on node type
        EncounterConfig encounter = ScriptableObject.CreateInstance<EncounterConfig>();
        
        switch (nodeType)
        {
            case NodeType.Combat:
                // Normal combat: 3-5 normal enemies
                encounter.encounterName = "Normal Combat";
                encounter.enemies = GenerateRandomEnemies(Random.Range(3, 6), EnemyType.Normal);
                break;
                
            case NodeType.Elite:
                // Elite combat: 1 elite + 2-3 normal enemies
                encounter.encounterName = "Elite Combat";
                encounter.enemies = new List<EnemySpawnEntry>();
                encounter.enemies.AddRange(GenerateRandomEnemies(1, EnemyType.Elite));
                encounter.enemies.AddRange(GenerateRandomEnemies(Random.Range(2, 4), EnemyType.Normal));
                break;
                
            case NodeType.Boss:
                // Boss combat: 1 boss
                encounter.encounterName = "Boss Combat";
                encounter.enemies = GenerateRandomEnemies(1, EnemyType.Boss);
                break;
                
            default:
                return null;
        }
        


        // Apply global spawn settings
        encounter.spawnAreaMin = new Vector2(-7f, 3f);
        encounter.spawnAreaMax = new Vector2(7f, 5f);
        encounter.minSpawnDistance = 3.5f; // Large spacing to prevent overlap
        encounter.spawnDelay = 0f; // No delay
        
        return encounter;
    }

    List<EnemySpawnEntry> GenerateRandomEnemies(int count, EnemyType type)
    {
        List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();
        
        // Debug pool state
        int bossCount = 0;
        foreach(var e in randomEnemyPool) if(e != null && e.enemyType == EnemyType.Boss) bossCount++;
        
        // Filter enemy pool by type
        List<EnemyData> validEnemies = new List<EnemyData>();
        foreach (var enemyData in randomEnemyPool)
        {
            if (enemyData != null && enemyData.enemyType == type)
            {
                validEnemies.Add(enemyData);
            }
        }
        
        if (validEnemies.Count == 0)
        {
            Debug.LogWarning($"⚠️ No {type} enemies in pool! Pool Size: {randomEnemyPool.Count}, Bosses in pool: {bossCount}");
            return entries;
        }
        
        for (int i = 0; i < count; i++)
        {
            EnemyData randomEnemy = validEnemies[Random.Range(0, validEnemies.Count)];
            entries.Add(new EnemySpawnEntry
            {
                enemyData = randomEnemy,
                count = 1
            });
        }
        
        return entries;
    }

    IEnumerator SpawnEncounter()
    {
        if (currentEncounter != null)
        {
            Debug.Log($"🎯 Spawning Encounter: {currentEncounter.encounterName}. Enemies count: {currentEncounter.enemies.Count}");
            
            if (currentEncounter.enemies.Count == 0)
            {
                Debug.LogError("❌ Encounter has 0 enemies! Ending combat immediately.");
            }
            
            int totalEnemies = 0;
            foreach (var entry in currentEncounter.enemies)
            {
                totalEnemies += entry.count;
            }
            
            // Raise encounter started event
            GameEvents.RaiseWaveStarted(1, totalEnemies);
            GameEvents.RaiseWaveProgressChanged(0, totalEnemies);
            
            // Spawn all enemies
            foreach (var entry in currentEncounter.enemies)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry);
                    
                    // Small delay between spawns if configured
                    // Delay removed as per user request
                    /*
                    if (currentEncounter.spawnDelay > 0)
                    {
                        yield return new WaitForSeconds(currentEncounter.spawnDelay);
                    }
                    */
                }
            }
        }
        else
        {
            // Fallback: Spawn random enemies
            Debug.Log($"🎲 Spawning {randomEnemyCount} random enemies");
            
            GameEvents.RaiseWaveStarted(1, randomEnemyCount);
            GameEvents.RaiseWaveProgressChanged(0, randomEnemyCount);
            
            for (int i = 0; i < randomEnemyCount; i++)
            {
                if (randomEnemyPool.Count > 0)
                {
                    EnemyData randomEnemy = randomEnemyPool[Random.Range(0, randomEnemyPool.Count)];
                    SpawnEnemy(new EnemySpawnEntry { enemyData = randomEnemy, count = 1 });
                }
            }
        }

        // Wait for all enemies to be defeated
        yield return new WaitUntil(() => activeEnemies.Count == 0);

        // Combat complete
        Debug.Log("✅ All enemies defeated!");
        isSpawning = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCombat();
        }
    }

    void SpawnEnemy(EnemySpawnEntry entry)
    {
        if (entry.enemyData == null)
        {
            Debug.LogWarning("⚠️ EnemySpawnEntry has no EnemyData!");
            return;
        }

        // Determine spawn position
        Vector3 spawnPos;
        
        if (entry.useSpecificPosition)
        {
            spawnPos = new Vector3(entry.specificPosition.x, entry.specificPosition.y, 0f);
        }
        else
        {
            spawnPos = FindValidSpawnPosition();
        }

        // Spawn enemy prefab or use data's prefab
        GameObject prefabToSpawn = entry.enemyData.prefab != null ? entry.enemyData.prefab : enemyPrefab;
        
        if (prefabToSpawn == null)
        {
            Debug.LogError("❌ No enemy prefab available!");
            return;
        }

        GameObject enemyGO = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            // Assign enemy data
            enemy.enemyData = entry.enemyData;
            RegisterEnemy(enemy);
        }
        else
        {
            Debug.LogError("❌ Spawned prefab has no Enemy component!");
            Destroy(enemyGO);
        }
    }

    Vector3 FindValidSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        bool validPos = false;
        int attempts = 0;
        
        Vector2 areaMin = currentEncounter != null ? currentEncounter.spawnAreaMin : new Vector2(-2.5f, 3.5f);
        Vector2 areaMax = currentEncounter != null ? currentEncounter.spawnAreaMax : new Vector2(2.5f, 4.5f);
        float minDist = currentEncounter != null ? currentEncounter.minSpawnDistance : 1.2f;

        // Try to find a non-overlapping position
        while (!validPos && attempts < 10)
        {
            spawnPos = new Vector3(
                Random.Range(areaMin.x, areaMax.x),
                4.0f, // Fixed Y position for alignment
                0f
            );
            
            validPos = true;

            foreach (var existingEnemy in activeEnemies)
            {
                if (existingEnemy != null && Vector3.Distance(spawnPos, existingEnemy.transform.position) < minDist)
                {
                    validPos = false;
                    break;
                }
            }
            attempts++;
        }

        if (!validPos)
        {
            Debug.LogWarning("⚠️ Could not find non-overlapping spawn position, spawning anyway.");
        }

        return spawnPos;
    }

    public static void RegisterEnemy(Enemy enemy)
    {
        activeEnemies.Add(enemy);
    }

    public static void UnregisterEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }

    public void OnEnemyKilled()
    {
        int totalEnemies = currentEncounter != null ? GetTotalEnemyCount() : randomEnemyCount;
        int killed = totalEnemies - activeEnemies.Count;
        
        GameEvents.RaiseWaveProgressChanged(killed, totalEnemies);
    }

    int GetTotalEnemyCount()
    {
        int total = 0;
        foreach (var entry in currentEncounter.enemies)
        {
            total += entry.count;
        }
        return total;
    }

    public static Enemy GetRandomEnemy()
    {
        if (activeEnemies.Count == 0) return null;
        
        List<Enemy> enemyList = new List<Enemy>(activeEnemies);
        return enemyList[Random.Range(0, enemyList.Count)];
    }

    /// <summary>
    /// Set a specific encounter (useful for testing or scripted encounters)
    /// </summary>
    public void SetEncounter(EncounterConfig encounter)
    {
        currentEncounter = encounter;
    }
}
