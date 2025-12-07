using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Fire")]
public class FirePassive : DicePassive
{
    public float damageMultiplier = 1.5f;
    public int enemyThreshold = 2;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        if (EnemySpawner.activeEnemies.Count >= enemyThreshold)
        {
            damage *= damageMultiplier;
            // Debug.Log($"🔥 Fire Boost: Damage increased to {damage}!");
        }
    }

    public override float GetProjectedDamageMultiplier(Dice owner)
    {
        // Return the multiplier if conditions are met
        if (EnemySpawner.activeEnemies.Count >= enemyThreshold)
        {
            return damageMultiplier;
        }
        return 1f;
    }
}
