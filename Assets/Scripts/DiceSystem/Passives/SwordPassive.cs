using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Sword Passive")]
public class SwordPassive : DicePassive
{
    public float damageMultiplier = 1.5f;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        base.OnDiceFire(owner, ref damage, ref skipProjectile);
        
        damage *= damageMultiplier;
    }
}
