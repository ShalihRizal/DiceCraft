using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Crit Booster")]
public class CritBooster : DicePassive
{
    [Header("Crit Settings")]
    [Range(0f, 1f)]
    public float boostedCritChance = 0.7f; // 70% crit

    private float originalCritChance;

    public override void OnCombatStart(Dice owner)
    {
        base.OnCombatStart(owner);

        if (owner.runtimeStats == null) return;

        // Store original crit chance
        originalCritChance = owner.runtimeStats.critChance;

        // Apply boosted crit chance immediately
        owner.runtimeStats.critChance = boostedCritChance;

        Log(owner, $"🔥 CritBooster active! Crit chance set to {boostedCritChance * 100}%");
    }

    public override void OnCombatEnd(Dice owner)
    {
        base.OnCombatEnd(owner);

        if (owner.runtimeStats == null) return;

        // Reset crit chance
        owner.runtimeStats.critChance = originalCritChance;

        Log(owner, $"💤 CritBooster ended. Crit chance reset to {originalCritChance * 100}%");
    }
}
