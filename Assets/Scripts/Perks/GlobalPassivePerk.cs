using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Global Passive Perk")]
public class GlobalPassivePerk : PerkData
{
    public DicePassive passiveToAdd;

    public override bool Apply()
    {
        if (GameManager.Instance != null && passiveToAdd != null)
        {
            GameManager.Instance.AddGlobalPassive(passiveToAdd);
            Debug.Log($"✨ Perk Applied: Global Passive {passiveToAdd.passiveName} added!");
            return true;
        }
        return false;
    }
}
