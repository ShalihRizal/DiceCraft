using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Heal")]
public class HealPassive : DicePassive
{
    public float healAmount = 10f;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        skipProjectile = true; // Don't shoot bullet

        // Find Player Health
        // Assuming GameManager or Player singleton has health
        // For now, let's look for a PlayerHealth component
        var playerHealth = FindFirstObjectByType<PlayerHealth>(); // Need to verify this class exists
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            owner.SpawnFloatingText(healAmount, false, false, true, false);
            Debug.Log($"💚 Healed Player for {healAmount}");
        }
        else
        {
            // Fallback if no PlayerHealth script found yet (might need to create it)
            Debug.LogWarning("HealPassive: No PlayerHealth found!");
        }
    }
}
