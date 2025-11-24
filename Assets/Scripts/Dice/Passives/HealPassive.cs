using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Heal Passive")]
public class HealPassive : DicePassive
{
    public float healAmount = 5f;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        base.OnDiceFire(owner, ref damage, ref skipProjectile);

        // Heal the player
        if (PlayerHealth.Instance != null) // Use FindFirstObjectByType if Instance is not reliable, but Instance should be fine
        {
            PlayerHealth.Instance.Heal(healAmount);
            owner.SpawnFloatingText(healAmount, false, false, true, false);
        }
    }
}
