using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootEntry
{
    public LootType lootType;
    public int weight = 100; // Higher = more likely
    public int minAmount = 1;
    public int maxAmount = 1;
    
    [Header("Conditional (Optional)")]
    public RelicRarity relicRarity = RelicRarity.Common;
    [Header("Visuals (Optional)")]
    public GameObject prefab; // Specific prefab for this drop
}

public enum LootType
{
    Gold,
    DicePips,
    Relic,
    Dice,
    HealthOrb
}

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    [Header("Loot Entries")]
    public List<LootEntry> entries = new List<LootEntry>();

    /// <summary>
    /// Roll for loot drops based on weighted probabilities
    /// </summary>
    public List<LootDrop> RollLoot()
    {
        List<LootDrop> drops = new List<LootDrop>();

        foreach (var entry in entries)
        {
            // Roll against weight (treat as percentage if weight <= 100)
            float roll = Random.Range(0f, 100f);
            if (roll <= entry.weight)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                drops.Add(new LootDrop
                {
                    type = entry.lootType,
                    amount = amount,
                    relicRarity = entry.relicRarity,
                    prefab = entry.prefab
                });
            }
        }

        return drops;
    }
}

/// <summary>
/// Represents a single loot drop result
/// </summary>
[System.Serializable]
public class LootDrop
{
    public LootType type;
    public int amount;
    public RelicRarity relicRarity; // Only used for Relic drops
    public GameObject prefab;
}
