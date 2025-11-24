using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Electro")]
public class ElectroPassive : DicePassive
{
    public float chainDamageRatio = 0.3f; // 30% damage

    public override void OnEnemyHit(Dice owner, Enemy enemy, ref float damageDealt)
    {
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
            float chainDamage = damageDealt * chainDamageRatio;
            target.TakeDamage(chainDamage);
            Debug.Log($"⚡ Electro Chain: {target.name} took {chainDamage} damage!");
            
            // Visuals?
            // owner.PlayVFX(VFXType.Passive);
        }
    }
}
