using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Dice Perk")]
public class DicePerk : PerkData
{
    public DiceData diceToGive;

    public override void Apply()
    {
        if (InventoryManager.Instance != null && diceToGive != null)
        {
            if (InventoryManager.Instance.AddDice(diceToGive))
            {
                Debug.Log($"🎁 Perk Applied: Added {diceToGive.diceName} to inventory.");
            }
            else
            {
                Debug.LogWarning("🎁 Perk Failed: Inventory Full! (Should handle this case)");
                // Fallback: Drop on ground? Or give gold?
                PlayerCurrency.Instance.AddGold(diceToGive.cost);
            }
        }
    }
}
