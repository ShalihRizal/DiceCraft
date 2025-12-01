using UnityEngine;
using System.Collections.Generic;

public enum RelicRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum RelicEffectType
{
    None,
    StatBoost,
    OnCombatStart,
    OnTurnStart,
    OnKill,
    OnDiceMerge
}

[CreateAssetMenu(fileName = "New Relic", menuName = "DiceCraft/Relic Data")]
public class RelicData : ScriptableObject
{
    public string relicName;
    [TextArea] public string description;
    public Sprite icon;
    public RelicRarity rarity;
    public int cost = 100;
    
    [Header("Effect Logic")]
    public RelicTrigger trigger;
    public RelicEffect effect;
    public float baseValue; // Base value at level 1
    public float valuePerLevel; // Increase per level
    
    [Header("Dice-Specific Effects")]
    public DiceData targetDiceData; // Reference to the specific dice this relic affects
    
    [Header("Upgrade System")]
    public int maxLevel = 1;
    public int currentLevel = 1;
    public List<Sprite> upgradeIcons; // Optional: Icons for each level

    public float GetCurrentValue()
    {
        return baseValue + (valuePerLevel * (currentLevel - 1));
    }

    public void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            // Update icon if available
            if (upgradeIcons != null && upgradeIcons.Count >= currentLevel)
            {
                icon = upgradeIcons[currentLevel - 1];
            }
        }
    }
}

public enum RelicTrigger
{
    Passive,
    OnCombatStart,
    OnTurnStart,
    OnKill,
    OnDiceMerge
}

public enum RelicEffect
{
    None,
    Heal,
    GainGold,
    DamageMultiplier,
    MaxHealth,
    CritChance,
    DicePassiveBoost // Boosts a specific dice type's passive
}
