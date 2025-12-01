using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Ice")]
public class IcePassive : DicePassive
{
    public float shieldAmount = 5f;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        skipProjectile = true; // Don't shoot bullet

        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Get scaled shield amount based on level
            int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
            float scaledShield = GetScaledValue(level);
            if (scaledShield == 0f) scaledShield = shieldAmount; // Fallback to default
            
            playerHealth.AddShield(scaledShield);
            owner.SpawnFloatingText(scaledShield, false, false, false, true);
            Debug.Log($"❄️ Shielded Player for {scaledShield}");
        }
    }

    public override string GetFormattedDescription(Dice owner)
    {
        int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
        float scaledShield = GetScaledValue(level);
        if (scaledShield == 0f) scaledShield = shieldAmount; // Fallback
        
        return $"Grants {scaledShield} shield instead of dealing damage.";
    }
}
