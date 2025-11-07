using UnityEngine;

[CreateAssetMenu(fileName = "NewDiceData", menuName = "Dice/Dice Data")]
public class DiceData : ScriptableObject
{
    public string diceName;
    public Sprite[] upgradeSprites;
    public GameObject prefab;
    public DiceRarity rarity;
    public int sides;
    public float baseDamage;
    public Dice.PassiveAbility passive;
    public float baseFireInterval = 2f;
    public int cost;

    public float luck = 0f;
    public float diceCritChance = 0f;

}
