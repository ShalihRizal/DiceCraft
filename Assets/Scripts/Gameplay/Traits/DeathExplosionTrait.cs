using UnityEngine;

[CreateAssetMenu(fileName = "Trait_DeathExplosion", menuName = "Game/Enemy/Traits/Death Explosion")]
public class DeathExplosionTrait : EnemyTrait
{
    [Header("Explosion Settings")]
    public float damage = 20f;
    public float radius = 3f;
    public GameObject explosionVFX;

    public override void ExecuteEffect(Enemy owner)
    {
        // Find all dice in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(owner.transform.position, radius);
        
        int diceHit = 0;
        foreach (var hit in hits)
        {
            Dice dice = hit.GetComponent<Dice>();
            if (dice != null)
            {
                // Damage dice (future implementation - dice don't have HP yet)
                diceHit++;
            }
        }

        // Spawn explosion VFX
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, owner.transform.position, Quaternion.identity);
        }

        Debug.Log($"{owner.name} exploded! Hit {diceHit} dice!");
    }
}
