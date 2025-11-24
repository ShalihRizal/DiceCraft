using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Shield Passive")]
public class ShieldPassive : DicePassive
{
    public float shieldAmount = 5f;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        base.OnDiceFire(owner, ref damage, ref skipProjectile);

        // Grant shield
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.AddShield(shieldAmount);
            owner.SpawnFloatingText(shieldAmount, false, false, false, true);
        }
    }
}
