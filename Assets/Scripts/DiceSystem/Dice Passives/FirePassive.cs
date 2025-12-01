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
            // Get scaled multiplier based on level
            int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
            float scaledValue = GetScaledValue(level);
            
            // If scaling configured, convert percentage to multiplier (e.g., 0.5 -> 1.5)
            // If not configured, use damageMultiplier directly
            float multiplier = (scaledValue > 0f) ? (1f + scaledValue) : damageMultiplier;
            
            // Apply relic boost if available (relic boost is also a percentage)
            if (RelicManager.Instance != null && owner != null && owner.diceData != null)
            {
                float relicBoost = RelicManager.Instance.GetDicePassiveBoost(owner.diceData);
                multiplier += relicBoost; // Add the percentage boost
            }
            
            damage *= multiplier;
            // Debug.Log($"🔥 Fire Boost: Damage increased to {damage}!");
        }
    }

    public override float GetProjectedDamageMultiplier(Dice owner)
    {
        float multiplier = 1f;
        
        // Return the multiplier if conditions are met
        if (EnemySpawner.activeEnemies.Count >= enemyThreshold)
        {
            // Get scaled multiplier based on level
            int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
            float scaledValue = GetScaledValue(level);
            
            // If scaling configured, convert percentage to multiplier (e.g., 0.5 -> 1.5)
            // If not configured, use damageMultiplier directly
            multiplier = (scaledValue > 0f) ? (1f + scaledValue) : damageMultiplier;
            
            // Apply relic boost if available (relic boost is also a percentage)
            if (RelicManager.Instance != null && owner != null && owner.diceData != null)
            {
                float relicBoost = RelicManager.Instance.GetDicePassiveBoost(owner.diceData);
                multiplier += relicBoost; // Add the percentage boost
            }
        }
        
        return multiplier;
    }

    public override string GetFormattedDescription(Dice owner)
    {
        // Get scaled multiplier based on level
        int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
        float scaledValue = GetScaledValue(level);
        
        // If scaling configured, convert percentage to multiplier (e.g., 0.5 -> 1.5)
        // If not configured, use damageMultiplier directly
        float multiplier = (scaledValue > 0f) ? (1f + scaledValue) : damageMultiplier;
        
        // Apply relic boost if available (relic boost is also a percentage)
        if (RelicManager.Instance != null && owner != null && owner.diceData != null)
        {
            float relicBoost = RelicManager.Instance.GetDicePassiveBoost(owner.diceData);
            multiplier += relicBoost; // Add the percentage boost
        }

        int percentBonus = Mathf.RoundToInt((multiplier - 1f) * 100f);
        return $"+{percentBonus}% damage when there are {enemyThreshold}+ enemies.";
    }
}
