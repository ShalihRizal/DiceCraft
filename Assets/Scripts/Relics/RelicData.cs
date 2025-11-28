using UnityEngine;

public enum RelicRarity
{
    Common,
    Rare,
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
    public RelicEffectType effectType;
    public float effectValue; // Generic value for the effect (e.g., +10% damage, +5 gold)
}
