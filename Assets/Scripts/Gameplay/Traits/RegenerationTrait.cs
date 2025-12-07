using UnityEngine;

[CreateAssetMenu(fileName = "Trait_Regeneration", menuName = "Game/Enemy/Traits/Regeneration")]
public class RegenerationTrait : EnemyTrait
{
    [Header("Regeneration Settings")]
    public float healPercent = 5f; // Heal 5% of max HP

    public override void ExecuteEffect(Enemy owner)
    {
        if (owner.enemyData == null) return;

        // Use runtime MaxHealth to account for difficulty scaling
        float healAmount = owner.MaxHealth * (healPercent / 100f);
        owner.Heal(healAmount);
        
        Debug.Log($"{owner.name} regenerated {healAmount} HP!");
    }
    // Override removed to use base reflection system
    // Default Description update: "Heals {healPercent:F1}% HP"
}
