using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Anemo")]
public class AnemoPassive : DicePassive
{
    public float attackSpeedBuff = 0.2f; // 20% faster

    public override void OnNeighborSpawn(Dice owner, Dice neighbor)
    {
        ApplyBuff(neighbor);
    }

    public override void OnNeighborRemoved(Dice owner, Dice neighbor)
    {
        RemoveBuff(neighbor);
    }

    public override void OnDiceSpawn(Dice owner, RuntimeDiceData stats)
    {
        // Apply to existing neighbors
        foreach (var neighbor in owner.GetNeighbors())
        {
            ApplyBuff(neighbor);
        }
    }

    public override void OnDiceRemoved(Dice owner)
    {
        // Remove from existing neighbors
        foreach (var neighbor in owner.GetNeighbors())
        {
            RemoveBuff(neighbor);
        }
    }

    private void ApplyBuff(Dice target)
    {
        if (target.runtimeStats != null)
        {
            target.runtimeStats.fireInterval /= (1f + attackSpeedBuff);
            Debug.Log($"🍃 Anemo Buff: {target.name} fire rate increased!");
        }
    }

    private void RemoveBuff(Dice target)
    {
        if (target.runtimeStats != null)
        {
            target.runtimeStats.fireInterval *= (1f + attackSpeedBuff);
        }
    }
}
