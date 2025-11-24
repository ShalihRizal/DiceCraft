using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Perks/Upgrade Perk")]
public class UpgradePerk : PerkData
{
    public int amountToUpgrade = 1;

    public override void Apply()
    {
        // Find all dice on board and inventory
        List<Dice> boardDice = new List<Dice>(FindObjectsByType<Dice>(FindObjectsSortMode.None));
        
        // Inventory dice are just data, need to handle them differently if we want to upgrade them.
        // Currently InventoryManager holds DiceData. DiceData is ScriptableObject (shared).
        // If we upgrade DiceData, it upgrades ALL instances. That's bad.
        // We need RuntimeDiceData for inventory too? 
        // Or Inventory should hold a wrapper class?
        // For now, let's only upgrade Board Dice (which have RuntimeDiceData).
        
        if (boardDice.Count == 0)
        {
            Debug.LogWarning("No dice to upgrade!");
            return;
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
    }
}
