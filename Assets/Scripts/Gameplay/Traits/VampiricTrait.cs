using UnityEngine;

[CreateAssetMenu(fileName = "Trait_Vampiric", menuName = "Game/Enemy/Traits/Vampiric")]
public class VampiricTrait : EnemyTrait
{
    [Header("Vampiric Settings")]
    public float lifestealPercent = 10f; // Heal for 10% of damage dealt

    public override void OnApplied(Enemy owner)
    {
        // Set trigger to OnAttack so it heals when attacking
        triggerEvent = EnemyEventTrigger.OnAttack;
    }

    public override void ExecuteEffect(Enemy owner)
    {
        float healAmount = owner.projectileDamage * (lifestealPercent / 100f);
        
        owner.Heal(healAmount);
        
        Debug.Log($"🩸 {owner.name} healed {healAmount} HP from vampiric attack!");
    }

    public override string GetDescription()
    {
        return $"Heals for {lifestealPercent}% of damage dealt on each attack.";
    }
}
