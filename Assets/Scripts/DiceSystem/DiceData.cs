using UnityEngine;

[CreateAssetMenu(fileName = "NewDiceData", menuName = "Dice/Dice Data")]
public class DiceData : ScriptableObject
{
    public string diceName;
    [TextArea(2, 5)]
    public string description;
    public Sprite[] upgradeSprites;
    public GameObject prefab;
    public DiceRarity rarity;
    public int sides;
    public float baseDamage;
    public float baseFireInterval = 2f;
    public int cost;
    public float luck = 0f;
    public float diceCritChance = 0f;

    [Header("Passive Behavior")]
    public DicePassive passive; // 🎯 Reference to a ScriptableObject

    [Header("Upgrade")]
    public int maxUpgradeLevel = 3;

    [Header("Behavior")]
    public bool canAttack = true;

    [Header("Visual Effects")]
    public GameObject vfxIdle;
    public GameObject vfxSpawn;
    public GameObject vfxDestroy;
    public GameObject vfxSold;
    public GameObject vfxBought;
    public GameObject vfxDrag;
    public GameObject vfxDrop;
    public GameObject vfxMerge;
    public GameObject vfxPassive;
}
