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
            Debug.LogWarning("⚠️ Combat already in progress, ignoring duplicate call");
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

            Debug.Log($"🌊 Spawning Wave {currentWaveIndex + 1}/{waves.Count}: {wave.count} enemies");

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

            Debug.Log($"✅ Wave {currentWaveIndex + 1} cleared!");
            currentWaveIndex++;

            // Small delay between waves (1 second)
            if (currentWaveIndex < waves.Count)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        // All waves complete
        Debug.Log("✅ All waves cleared!");
        isSpawning = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCombat();
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 spawnPos = new Vector3(Random.Range(-2.5f, 2.5f), 4f, 0f);
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
        Debug.Log($"Enemy killed! Progress: {enemiesKilledThisWave}/{totalEnemiesThisWave}");
        GameEvents.RaiseWaveProgressChanged(enemiesKilledThisWave, totalEnemiesThisWave);
    }

    public static Enemy GetRandomEnemy()
    {
        if (activeEnemies.Count == 0) return null;
        
        List<Enemy> enemyList = new List<Enemy>(activeEnemies);
        return enemyList[Random.Range(0, enemyList.Count)];
    }
}
