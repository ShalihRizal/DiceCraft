using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<WaveConfig> waves = new List<WaveConfig>();

    public static HashSet<Enemy> activeEnemies = new HashSet<Enemy>();
    private int currentWaveIndex = 0;
    private int enemiesKilledThisWave = 0;
    private int totalEnemiesThisWave = 0;
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

            return;
        }

        isSpawning = true;
        currentWaveIndex = 0;
        enemiesKilledThisWave = 0;
        activeEnemies.Clear();
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (currentWaveIndex < waves.Count)
        {
            WaveConfig wave = waves[currentWaveIndex];

            // Reset wave progress
            enemiesKilledThisWave = 0;
            totalEnemiesThisWave = wave.count;
            
            // Raise wave started event
            GameEvents.RaiseWaveStarted(currentWaveIndex + 1, totalEnemiesThisWave);
            GameEvents.RaiseWaveProgressChanged(0, totalEnemiesThisWave);

            // Spawn ALL enemies immediately
            for (int i = 0; i < wave.count; i++)
            {
                SpawnEnemy(wave.enemyPrefab);
            }

            // Wait for all enemies to be defeated
            yield return new WaitUntil(() => activeEnemies.Count == 0);

            currentWaveIndex++;

            // Small delay between waves (1 second)
            if (currentWaveIndex < waves.Count)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        // All waves complete

        isSpawning = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCombat();
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 spawnPos = Vector3.zero;
        bool validPos = false;
        int attempts = 0;

        // Try to find a non-overlapping position
        while (!validPos && attempts < 10)
        {
            spawnPos = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(3.5f, 4.5f), 0f); // Added Y variation
            validPos = true;

            foreach (var existingEnemy in activeEnemies)
            {
                if (Vector3.Distance(spawnPos, existingEnemy.transform.position) < 1.2f) // Minimum distance
                {
                    validPos = false;
                    break;
                }
            }
            attempts++;
        }

        if (!validPos) Debug.LogWarning("⚠️ Could not find non-overlapping spawn position, spawning anyway.");

        GameObject enemyGO = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            RegisterEnemy(enemy);
        }
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
        enemiesKilledThisWave++;

        GameEvents.RaiseWaveProgressChanged(enemiesKilledThisWave, totalEnemiesThisWave);
    }

    public static Enemy GetRandomEnemy()
    {
        if (activeEnemies.Count == 0) return null;
        
        List<Enemy> enemyList = new List<Enemy>(activeEnemies);
        return enemyList[Random.Range(0, enemyList.Count)];
    }
}
