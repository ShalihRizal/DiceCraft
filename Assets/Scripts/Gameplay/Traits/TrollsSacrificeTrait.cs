using UnityEngine;

[CreateAssetMenu(fileName = "Trait_TrollsSacrifice", menuName = "Game/Enemy/Traits/Troll's Sacrifice")]
public class TrollsSacrificeTrait : EnemyTrait
{
    [Header("Sacrifice Settings")]
    public GameObject pawnPrefab; // Troll's Pawn Prefab
    public int pawnCount = 2;
    public float spawnRadius = 2f;

    public override void ExecuteEffect(Enemy owner)
    {
        if (pawnPrefab == null)
        {
            Debug.LogWarning($"{traitName}: No pawn prefab assigned!");
            return;
        }

        int successSpawns = 0;
        for (int i = 0; i < pawnCount; i++)
        {
            Vector3 spawnPos = owner.transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                Random.Range(-spawnRadius, spawnRadius),
                0f
            );

            GameObject minionGO = Instantiate(pawnPrefab, spawnPos, Quaternion.identity);
            
            // Ensure minion is properly registered logic-wise
            Enemy minionEnemy = minionGO.GetComponent<Enemy>();
            if (minionEnemy != null)
            {
                // Assign Data if missing!
                if (minionEnemy.enemyData == null)
                {
                    EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
                    if (spawner != null && spawner.randomEnemyPool.Count > 0)
                    {
                        // Find a "Normal" enemy data to assign
                        var normalEnemies = spawner.randomEnemyPool.FindAll(e => e.enemyType == EnemyType.Normal);
                        if (normalEnemies.Count > 0)
                        {
                            // Clone the data so we can modify stats without affecting the asset
                            EnemyData cloneData = Instantiate(normalEnemies[Random.Range(0, normalEnemies.Count)]);
                            
                            // Scale HP to 30% of the Boss's CURRENT Max Health
                            cloneData.maxHealth = owner.MaxHealth * 0.3f;
                            
                            // Optional: Scale damage too if needed? User only asked for Health.
                            
                            minionEnemy.enemyData = cloneData;
                        }
                        else
                        {
                            // Fallback to any
                             EnemyData cloneData = Instantiate(spawner.randomEnemyPool[0]);
                             cloneData.maxHealth = owner.MaxHealth * 0.3f;
                             minionEnemy.enemyData = cloneData;
                        }
                    }
                }
                // Fallback: If minion has no Health Bar assigned, try to find one
                if (minionEnemy.healthBarPrefab == null)
                {
                    // 1. Try copying from Owner (if owner has one)
                    if (owner.healthBarPrefab != null)
                    {
                        minionEnemy.healthBarPrefab = owner.healthBarPrefab;
                    }
                    // 2. Try getting from Spawner's default enemy
                    else
                    {
                        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
                        if (spawner != null && spawner.enemyPrefab != null)
                        {
                            Enemy defaultEnemy = spawner.enemyPrefab.GetComponent<Enemy>();
                            if (defaultEnemy != null)
                            {
                                minionEnemy.healthBarPrefab = defaultEnemy.healthBarPrefab;
                            }
                        }
                    }
                }

                // Register with spawner for Wave logic
                EnemySpawner.RegisterEnemy(minionEnemy);
                
                // LINK TO OWNER FOR IMMUNITY
                owner.RegisterLinkedMinion(minionEnemy);
                successSpawns++;
            }
        }

        if (successSpawns > 0)
        {
             Debug.Log($"{owner.name} activated Troll's Sacrifice! IMMUNE until pawns perform the ultimate sacrifice.");
        }
    }
}
