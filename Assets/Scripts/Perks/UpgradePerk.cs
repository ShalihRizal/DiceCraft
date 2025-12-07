using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Perks/Upgrade Perk")]
public class UpgradePerk : PerkData
{
    public int amountToUpgrade = 1;

    public override bool Apply()
    {
        // Find all dice on board
        List<Dice> boardDice = new List<Dice>(FindObjectsByType<Dice>(FindObjectsSortMode.None));
        
        if (boardDice.Count == 0)
        {
            Debug.LogWarning("No dice to upgrade!");
            return true;
        }

        for (int i = 0; i < amountToUpgrade; i++)
        {
            if (boardDice.Count == 0) break;
            
            Dice randomDice = boardDice[Random.Range(0, boardDice.Count)];
            if (randomDice.runtimeStats != null)
            {
                randomDice.runtimeStats.upgradeLevel++;
                // Update sprite
                if (randomDice.runtimeStats.upgradeLevel < randomDice.diceData.upgradeSprites.Length)
                {
                    randomDice.GetComponent<SpriteRenderer>().sprite = randomDice.diceData.upgradeSprites[randomDice.runtimeStats.upgradeLevel];
                }
                randomDice.PlayVFX(VFXType.Merge); // Reuse merge effect
                Debug.Log($"⬆ Perk Applied: Upgraded {randomDice.name}");
            }
            boardDice.Remove(randomDice);
        }
        return true;
    }
}
