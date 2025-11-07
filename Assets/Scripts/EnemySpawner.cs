using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public int maxEnemies = 10;
    public int enemiesPerFight = 5;

    private int enemiesSpawned = 0;
    private int enemiesDefeated = 0;
    private bool isCombatActive = false;

    public static List<Enemy> activeEnemies = new List<Enemy>();

    public void StartCombat()
    {
        enemiesSpawned = 0;
        enemiesDefeated = 0;
        isCombatActive = true;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (isCombatActive)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (activeEnemies.Count < maxEnemies && enemiesSpawned < enemiesPerFight)
            {
                Vector3 spawnPos = new Vector3(Random.Range(-4f, 4f), 3f, 0f);
                GameObject enemyGO = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                Enemy enemy = enemyGO.GetComponent<Enemy>();
                activeEnemies.Add(enemy);
                enemiesSpawned++;
            }
        }
    }

    public static Enemy GetRandomEnemy()
    {
        if (activeEnemies.Count == 0) return null;
        return activeEnemies[Random.Range(0, activeEnemies.Count)];
    }

    public void NotifyEnemyDefeated()
    {
        enemiesDefeated++;

        if (enemiesDefeated >= enemiesPerFight && activeEnemies.Count == 0)
        {
            EndCombat();
        }
    }

    void EndCombat()
    {
        if (!isCombatActive) return;
        isCombatActive = false;
        GameManager.Instance?.EndCombat();
        Debug.Log("Combat ended. Show reward screen or transition.");
    }
}
