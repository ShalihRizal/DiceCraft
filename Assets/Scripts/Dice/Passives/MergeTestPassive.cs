using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Passives/Merge Test")]
public class MergeTestPassive : DicePassive
{
    public float bonusDamage = 10f;

    public override void OnDiceMerged(Dice owner, Dice mergedInto)
    {
        base.OnDiceMerged(owner, mergedInto);

        Debug.Log($"🧩 Passive Triggered: {owner.name} → {mergedInto.name} on merge");

        if (mergedInto.runtimeStats != null)
        {
            Debug.Log($" RuntimeDiceData Before: {mergedInto.runtimeStats.baseDamage}");
            mergedInto.runtimeStats.baseDamage += bonusDamage;
            Debug.Log($" RuntimeDiceData After: {mergedInto.runtimeStats.baseDamage}");
        }
    }
}
