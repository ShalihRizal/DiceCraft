using UnityEngine;

[CreateAssetMenu(fileName = "JokerPassive", menuName = "Game/Dice/Passives/Joker")]
public class JokerPassive : DicePassive
{
    public override void OnSwap(Dice thisDice, Dice otherDice)
    {
        if (thisDice == null || otherDice == null || otherDice.diceData == null) return;

        Debug.Log($"🃏 Joker activated! Transforming into {otherDice.diceData.diceName}");

        // Store the other dice's data
        DiceData targetData = otherDice.diceData;
        int targetLevel = otherDice.runtimeStats.upgradeLevel;

        // Transform this dice into the other dice
        thisDice.diceData = targetData;
        thisDice.runtimeStats = new RuntimeDiceData(targetData);
        thisDice.runtimeStats.upgradeLevel = targetLevel;

        // Update visual
        SpriteRenderer sr = thisDice.GetComponent<SpriteRenderer>();
        if (sr != null && targetData.upgradeSprites != null && targetData.upgradeSprites.Length > 0)
        {
            if (targetLevel < targetData.upgradeSprites.Length)
                sr.sprite = targetData.upgradeSprites[targetLevel];
            else
                sr.sprite = targetData.upgradeSprites[0];
        }

        // Play VFX
        thisDice.PlayVFX(VFXType.Merge);

        Debug.Log($"✨ Joker transformed into {targetData.diceName} Lv.{targetLevel}!");
    }
}
