public enum ShopItemType
{
    Dice,
    Relic,
    Service // Heal, etc.
}

[System.Serializable]
public class ShopItem
{
    public ShopItemType itemType;
    public DiceData diceData; // used if itemType == Dice
    public RelicData relicData; // used if itemType == Relic
    public string description;
    public int cost;
    public int originalCost; // For sale display
    public bool isOnSale;

    public ShopItem(DiceData dice)
    {
        itemType = ShopItemType.Dice;
        diceData = dice;
        cost = dice.cost;
        originalCost = cost;
        description = $"{dice.diceName} ({dice.rarity})\n{dice.passive} | {dice.sides} sides";
    }

    public ShopItem(RelicData relic)
    {
        itemType = ShopItemType.Relic;
        relicData = relic;
        cost = relic.cost;
        originalCost = cost;
        description = $"{relic.relicName} ({relic.rarity})\n{relic.description}";
    }

    public ShopItem(ShopItemType type, string desc, int price)
    {
        itemType = type;
        description = desc;
        cost = price;
        originalCost = price;
    }
}
