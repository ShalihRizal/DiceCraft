using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Frenzy Fire")]
public class FrenzyFire : DicePassive
{
    [Header("Frenzy Settings")]
    [Range(0.1f, 2f)]
    public float fireRateMultiplier = 0.5f; // 0.5 = 2x faster
    public new float duration = 5f;         // Frenzy lasts this long
    public float cooldown = 8f;             // Cooldown before next frenzy

    private bool isFrenzyActive = false;
    private Coroutine frenzyRoutine;

    private float originalFireInterval;

    public override void OnCombatStart(Dice owner)
    {
        base.OnCombatStart(owner);

        if (owner.runtimeStats == null) return;

        // Store original fire interval
        originalFireInterval = owner.runtimeStats.fireInterval;

        // Start Frenzy coroutine
        frenzyRoutine = owner.StartCoroutine(FrenzyLoop(owner));
    }

    public override void OnCombatEnd(Dice owner)
    {
        base.OnCombatEnd(owner);

        // Stop coroutine if active
        if (frenzyRoutine != null)
        {
            owner.StopCoroutine(frenzyRoutine);
            frenzyRoutine = null;
        }

        // Reset fire interval
        if (owner.runtimeStats != null)
            owner.runtimeStats.fireInterval = originalFireInterval;

        Log(owner, $"💤 FrenzyFire ended, fire interval reset.");
    }

    private IEnumerator FrenzyLoop(Dice owner)
    {
        isFrenzyActive = true;

        while (isFrenzyActive)
        {
            // Activate frenzy
            owner.runtimeStats.fireInterval = originalFireInterval * fireRateMultiplier;
            Log(owner, $"🔥 Frenzy active! Fire interval ×{fireRateMultiplier:0.00} for {duration}s");

            yield return new WaitForSeconds(duration);

            // Revert to normal
            owner.runtimeStats.fireInterval = originalFireInterval;
            Log(owner, $"💤 Frenzy ended. Cooling down for {cooldown}s");

            yield return new WaitForSeconds(cooldown);
        }
    }
}
