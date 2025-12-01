using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dice/Passives/Anemo")]
public class AnemoPassive : DicePassive
{
    public float attackSpeedBuff = 0.2f; // 20% faster

    // Track which sources are buffing a target
    // Target -> Set of Sources
    private Dictionary<Dice, HashSet<Dice>> buffSources = new Dictionary<Dice, HashSet<Dice>>();

    public override void OnNeighborSpawn(Dice owner, Dice neighbor)
    {
        ApplyBuff(neighbor, owner);
    }

    public override void OnNeighborRemoved(Dice owner, Dice neighbor)
    {
        RemoveBuff(neighbor, owner);
    }

    public override void OnDiceSpawn(Dice owner, RuntimeDiceData stats)
    {
        // Apply to existing neighbors
        foreach (var neighbor in owner.GetNeighbors())
        {
            ApplyBuff(neighbor, owner);
        }
    }

    public override void OnDiceRemoved(Dice owner)
    {
        // Remove from existing neighbors
        foreach (var neighbor in owner.GetNeighbors())
        {
            RemoveBuff(neighbor, owner);
        }
    }

    private void ApplyBuff(Dice target, Dice source)
    {
        if (target == null || target.runtimeStats == null || source == null) return;

        // Initialize set if needed
        if (!buffSources.ContainsKey(target))
        {
            buffSources[target] = new HashSet<Dice>();
        }

        // Prevent double application from same source
        if (buffSources[target].Contains(source))
        {
            return;
        }

        // Add source
        buffSources[target].Add(source);

        // Recalculate fire interval from base value
        RecalculateFireInterval(target);
    }

    private void RemoveBuff(Dice target, Dice source)
    {
        if (target == null || target.runtimeStats == null || source == null) return;
        
        if (buffSources.ContainsKey(target) && buffSources[target].Contains(source))
        {
            buffSources[target].Remove(source);
            
            // Recalculate fire interval from base value
            RecalculateFireInterval(target);

            // Clean up if no sources remain
            if (buffSources[target].Count == 0)
            {
                buffSources.Remove(target);
            }
        }
    }

    private void RecalculateFireInterval(Dice target)
    {
        if (target == null || target.runtimeStats == null || target.diceData == null) return;

        // Start from base fire interval
        float baseInterval = target.diceData.baseFireInterval;
        
        // Get current buff count (number of unique sources)
        int buffCount = buffSources.ContainsKey(target) ? buffSources[target].Count : 0;
        
        if (buffCount == 0)
        {
            target.runtimeStats.fireInterval = baseInterval;
            return;
        }

        // Get the buff value from the first source (they should all be the same passive type)
        float buffValue = attackSpeedBuff; // Default fallback
        
        foreach (var source in buffSources[target])
        {
            if (source != null && source.diceData != null && source.diceData.passive == this)
            {
                // Use scaled value based on source dice level
                int sourceLevel = source.runtimeStats != null ? source.runtimeStats.upgradeLevel : 1;
                buffValue = GetScaledValue(sourceLevel);
                if (buffValue == 0f) buffValue = attackSpeedBuff; // Fallback to default
                break; // Use first source's level
            }
        }
        
        // Apply buff based on count (stacks multiplicatively)
        float totalMultiplier = Mathf.Pow(1f + buffValue, buffCount);
        target.runtimeStats.fireInterval = baseInterval / totalMultiplier;
    }

    public override System.Collections.Generic.List<Dice> GetAffectedNeighbors(Dice owner)
    {
        return owner.GetNeighbors();
    }

    public override string GetFormattedDescription(Dice owner)
    {
        int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
        float buffValue = GetScaledValue(level);
        if (buffValue == 0f) buffValue = attackSpeedBuff; // Fallback
        
        int percentBonus = Mathf.RoundToInt(buffValue * 100f);
        return $"+{percentBonus}% attack speed for adjacent dice (stacks).";
    }
}
