using UnityEngine;

[CreateAssetMenu(fileName = "Trait_Berserker", menuName = "Game/Enemy/Traits/Berserker")]
public class BerserkerTrait : EnemyTrait
{
    [Header("Berserker Settings")]
    public float attackSpeedMultiplier = 2f; // Double attack speed

    private bool hasTriggered = false;

    public override void OnApplied(Enemy owner)
    {
        // Set trigger to On50Percent
        triggerEvent = EnemyEventTrigger.On50Percent;
        hasTriggered = false;
    }

    public override void ExecuteEffect(Enemy owner)
    {
        if (hasTriggered) return; // Only trigger once
        
        hasTriggered = true;
        
        // Reduce fire interval (faster attacks)
        owner.fireInterval /= attackSpeedMultiplier;
        
        Debug.Log($"😡 {owner.name} entered BERSERKER MODE! Attack speed x{attackSpeedMultiplier}!");
    }

    public override string GetDescription()
    {
        return $"When HP drops below 50%, attack speed increases by {attackSpeedMultiplier}x.";
    }
}
