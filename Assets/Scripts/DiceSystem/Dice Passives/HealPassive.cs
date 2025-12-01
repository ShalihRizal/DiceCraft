using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Heal")]
public class HealPassive : DicePassive
{
    public float healAmount = 10f;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        skipProjectile = true; // Don't shoot bullet

        // Get scaled heal amount based on level
        int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
        float scaledHeal = GetScaledValue(level);
        if (scaledHeal == 0f) scaledHeal = healAmount; // Fallback to default
        
        // Find Player Health
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(scaledHeal);
            owner.SpawnFloatingText(scaledHeal, false, false, true, false);
            Debug.Log($"💚 Healed Player for {scaledHeal}");
        }
        else
        {
            Debug.LogWarning("HealPassive: No PlayerHealth found!");
        }
    }

    public override string GetFormattedDescription(Dice owner)
    {
        int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
        float scaledHeal = GetScaledValue(level);
        if (scaledHeal == 0f) scaledHeal = healAmount; // Fallback
        
        return $"Heals {scaledHeal} HP instead of dealing damage.";
    }
}
