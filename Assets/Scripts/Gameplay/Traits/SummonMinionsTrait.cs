using UnityEngine;

[CreateAssetMenu(fileName = "Trait_SummonMinions", menuName = "Game/Enemy/Traits/Summon Minions")]
public class SummonMinionsTrait : EnemyTrait
{
    [Header("Summon Settings")]
    public GameObject minionPrefab;
    public int minionCount = 2;
    public float spawnRadius = 2f;

    public override void ExecuteEffect(Enemy owner)
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning($"{traitName}: No minion prefab assigned!");
            return;
        }

        for (int i = 0; i < minionCount; i++)
        {
            Vector3 spawnPos = owner.transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                Random.Range(-spawnRadius, spawnRadius),
                0f
            );

            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            Enemy minionEnemy = minion.GetComponent<Enemy>();
            
            if (minionEnemy != null)
            {
                EnemySpawner.RegisterEnemy(minionEnemy);
            }
        }

        Debug.Log($"{owner.name} summoned {minionCount} minions!");
    }
}
