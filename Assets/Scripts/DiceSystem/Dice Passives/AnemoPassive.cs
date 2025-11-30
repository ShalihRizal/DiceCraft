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
        
        // Apply buff based on count (stacks multiplicatively)
        float totalMultiplier = Mathf.Pow(1f + attackSpeedBuff, buffCount);
        target.runtimeStats.fireInterval = baseInterval / totalMultiplier;
    }

    public override System.Collections.Generic.List<Dice> GetAffectedNeighbors(Dice owner)
    {
        return owner.GetNeighbors();
    }
}
