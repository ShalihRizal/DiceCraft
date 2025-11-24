using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DicePool", menuName = "Dice/Dice Pool")]
public class DicePool : ScriptableObject
{
    public List<DiceData> allDice;

    public DiceData GetRandomDice()
{
    if (allDice == null || allDice.Count == 0)
    {
        Debug.LogError("❌ DicePool is empty! Please assign at least one DiceData.");
        return null;
    }

    float roll = Random.value;

    DiceRarity chosenRarity;
    if (roll < 0.03f) chosenRarity = DiceRarity.Legendary;
    else if (roll < 0.15f) chosenRarity = DiceRarity.Epic;
    else if (roll < 0.40f) chosenRarity = DiceRarity.Rare;
    else if (roll < 0.70f) chosenRarity = DiceRarity.Uncommon;
    else chosenRarity = DiceRarity.Common;

    // Filter the list
    var matchingDice = allDice.Where(d => d != null && d.rarity == chosenRarity).ToList();

    // Fallback if none match this rarity
    if (matchingDice.Count == 0)
    {
        matchingDice = allDice.Where(d => d != null).ToList(); // pick from all dice
        Debug.LogWarning($"⚠️ No dice found for rarity {chosenRarity}, using any available dice instead.");
    }

    // If still empty, something's very wrong
    if (matchingDice.Count == 0)
    {
        Debug.LogError("❌ No valid DiceData found in DicePool! Check your asset references.");
        return null;
    }

    int index = Random.Range(0, matchingDice.Count);
    return matchingDice[index];
}

}
