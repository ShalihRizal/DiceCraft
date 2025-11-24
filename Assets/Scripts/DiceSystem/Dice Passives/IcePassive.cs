using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Ice")]
public class IcePassive : DicePassive
{
    public float shieldAmount = 5f;

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        skipProjectile = true; // Don't shoot bullet

        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.AddShield(shieldAmount);
            owner.SpawnFloatingText(shieldAmount, false, false, false, true);
            Debug.Log($"❄️ Shielded Player for {shieldAmount}");
        }
    }
}
