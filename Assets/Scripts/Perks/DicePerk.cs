using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Dice Perk")]
public class DicePerk : PerkData
{
    public DiceData diceToGive;

    public override bool Apply()
    {
        if (InventoryManager.Instance != null && diceToGive != null)
        {
            if (InventoryManager.Instance.AddDice(diceToGive))
            {
                Debug.Log($"🎁 Perk Applied: Added {diceToGive.diceName} to inventory.");
                return true;
            }
            else
            {
                Debug.LogWarning("🎁 Perk Failed: Inventory Full!");
                return false;
            }
        }
        return false;
    }
}
