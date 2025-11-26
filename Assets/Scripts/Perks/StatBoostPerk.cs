using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Stat Boost Perk")]
public class StatBoostPerk : PerkData
{
    public float damageMultiplier = 0f; // Additive (e.g. +0.1 for +10%)
    public float fireRateMultiplier = 0f; // Additive (e.g. -0.1 for 10% faster)
    public float critChanceAdd = 0f;

    public override bool Apply()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.globalDamageMultiplier += damageMultiplier;
            GameManager.Instance.globalFireRateMultiplier += fireRateMultiplier; // Careful with logic here, usually 1.0 is base
            GameManager.Instance.globalCritChance += critChanceAdd;
            
            Debug.Log($"💪 Perk Applied: Stats Boosted!");
            return true;
        }
        return false;
    }
}
