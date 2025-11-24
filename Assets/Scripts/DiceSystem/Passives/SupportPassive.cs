using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Support Passive")]
public class SupportPassive : DicePassive
{
    [Header("Support Settings")]
    public float damageBonus = 5f;
    public float critChanceBonus = 0.1f;

    public override void OnNeighborFire(Dice owner, Dice neighbor, ref float damage, ref bool skipProjectile)
    {
        base.OnNeighborFire(owner, neighbor, ref damage, ref skipProjectile);
        
        damage += damageBonus;
        
        // We can't easily modify crit chance via ref here unless we change the signature or access runtimeStats directly
        // But let's check if we can access neighbor.runtimeStats
        if (neighbor.runtimeStats != null)
        {
            // This would be permanent if we just set it, so we should be careful.
            // For now, just damage boost is safe as it's per-shot.
        }

        // Visual feedback
        owner.PlayVFX(VFXType.Passive);
    }

    public override void OnNeighborSpawn(Dice owner, Dice neighbor)
    {
        base.OnNeighborSpawn(owner, neighbor);
        Debug.Log($"🤝 {owner.name} says hello to neighbor {neighbor.name}");
        owner.PlayVFX(VFXType.Idle); // Pulse or something
    }
}
