public enum ShopItemType
{
    Dice,
    StatBoost,
    Skill
}

[System.Serializable]
public class ShopItem
{
    public ShopItemType itemType;
    public DiceData diceData; // used if itemType == Dice
    public string description;
    public int cost;

    public ShopItem(DiceData dice)
    {
        itemType = ShopItemType.Dice;
        diceData = dice;
        cost = dice.cost;
        description = $"{dice.diceName} ({dice.rarity})\n{dice.passive} | {dice.sides} sides";
    }

    public ShopItem(ShopItemType type, string desc, int price)
    {
        itemType = type;
        description = desc;
        cost = price;
    }
}
