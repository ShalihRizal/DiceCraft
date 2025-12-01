using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Electro")]
public class ElectroPassive : DicePassive
{
    public float chainDamageRatio = 0.3f; // 30% damage

    public override void OnEnemyHit(Dice owner, Enemy enemy, ref float damageDealt)
    {
        // Get scaled chain damage ratio based on level
        int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
        float scaledRatio = GetScaledValue(level);
        if (scaledRatio == 0f) scaledRatio = chainDamageRatio; // Fallback to default
        
        // Find another enemy
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy target = null;
        float minDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (e == enemy) continue; // Skip hit enemy
            float dist = Vector3.Distance(enemy.transform.position, e.transform.position);
            if (dist < minDist && dist < 5f) // Range check
            {
                minDist = dist;
                target = e;
            }
        }

        if (target != null)
        {
            float chainDamage = damageDealt * scaledRatio;
            target.TakeDamage(chainDamage);
            Debug.Log($"⚡ Electro Chain: {target.name} took {chainDamage} damage!");
            
            // Visuals?
            // owner.PlayVFX(VFXType.Passive);
        }
    }

    public override string GetFormattedDescription(Dice owner)
    {
        int level = owner != null && owner.runtimeStats != null ? owner.runtimeStats.upgradeLevel : 1;
        float scaledRatio = GetScaledValue(level);
        if (scaledRatio == 0f) scaledRatio = chainDamageRatio; // Fallback
        
        int percentDamage = Mathf.RoundToInt(scaledRatio * 100f);
        return $"Chains {percentDamage}% damage to a nearby enemy (5m range).";
    }
}
