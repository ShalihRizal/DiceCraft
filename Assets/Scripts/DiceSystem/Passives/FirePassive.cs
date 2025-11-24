using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Fire Passive")]
public class FirePassive : DicePassive
{
    [Header("Burst Settings")]
    public float attackSpeedMultiplier = 2f; // 2x speed
    public float burstDuration = 3f;
    // public float cooldown = 5f; // ❌ Removed duplicate, using base.cooldown
    public float bonusDamage = 5f;

    private Coroutine burstRoutine;
    private float originalFireInterval;

    public override void OnCombatStart(Dice owner)
    {
        base.OnCombatStart(owner);
        if (owner.runtimeStats == null) return;

        originalFireInterval = owner.runtimeStats.fireInterval;
        burstRoutine = owner.StartCoroutine(BurstLoop(owner));
    }

    public override void OnCombatEnd(Dice owner)
    {
        base.OnCombatEnd(owner);
        if (burstRoutine != null)
        {
            owner.StopCoroutine(burstRoutine);
            burstRoutine = null;
        }
        if (owner.runtimeStats != null)
            owner.runtimeStats.fireInterval = originalFireInterval;
    }

    private IEnumerator BurstLoop(Dice owner)
    {
        while (true)
        {
            // Burst Phase
            if (owner.runtimeStats != null)
                owner.runtimeStats.fireInterval = originalFireInterval / attackSpeedMultiplier;
            
            owner.SpawnFloatingText(0, false, false, false, false); // Maybe show "BURST!" text?
            // Or play effect
            if (owner.diceData != null) owner.PlayVFX(VFXType.Passive);

            yield return new WaitForSeconds(burstDuration);

            // Cooldown Phase
            if (owner.runtimeStats != null)
                owner.runtimeStats.fireInterval = originalFireInterval; // Back to normal
            
            yield return new WaitForSeconds(cooldown);
        }
    }

    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        base.OnDiceFire(owner, ref damage, ref skipProjectile);
        // damage += bonusDamage; 
    }
}
