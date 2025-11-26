using UnityEngine;

public enum PerkRarity { Common, Rare, Epic, Legendary }

public abstract class PerkData : ScriptableObject
{
    public string perkName;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;
    public PerkRarity rarity;

    public abstract bool Apply();
}
