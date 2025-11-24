using UnityEngine;
using UnityEditor;
using System.IO;

public class PerkAssetGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Test Perks")]
    public static void Generate()
    {
        string path = "Assets/ScriptableObjects/Perks";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        // 1. Stat Boosts
        CreateStatPerk("Damage Boost (Common)", "Increases damage by 10%", 0.1f, 0f, 0f, PerkRarity.Common);
        CreateStatPerk("Damage Boost (Rare)", "Increases damage by 25%", 0.25f, 0f, 0f, PerkRarity.Rare);
        CreateStatPerk("Fire Rate Boost (Common)", "Increases fire rate by 10%", 0f, 0.1f, 0f, PerkRarity.Common);
        CreateStatPerk("Crit Chance Boost (Epic)", "Increases crit chance by 5%", 0f, 0f, 0.05f, PerkRarity.Epic);

        // 2. Upgrades
        CreateUpgradePerk("Single Upgrade", "Upgrades 1 random dice", 1, PerkRarity.Common);
        CreateUpgradePerk("Double Upgrade", "Upgrades 2 random dice", 2, PerkRarity.Rare);

        // 3. Dice Perks
        string dicePath = "Assets/ScriptableObjects/DiceDatas";
        if (Directory.Exists(dicePath))
        {
            string[] guids = AssetDatabase.FindAssets("t:DiceData", new[] { dicePath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                DiceData dice = AssetDatabase.LoadAssetAtPath<DiceData>(assetPath);
                if (dice != null)
                {
                    CreateDicePerk(dice);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Test Perks Generated!");
    }

    static void CreateStatPerk(string name, string desc, float dmg, float fire, float crit, PerkRarity rarity)
    {
        StatBoostPerk perk = ScriptableObject.CreateInstance<StatBoostPerk>();
        perk.perkName = name;
        perk.description = desc;
        perk.damageMultiplier = dmg;
        perk.fireRateMultiplier = fire;
        perk.critChanceAdd = crit;
        perk.rarity = rarity;

        string path = $"Assets/ScriptableObjects/Perks/{name.Replace(" ", "")}.asset";
        AssetDatabase.CreateAsset(perk, path);
    }

    static void CreateUpgradePerk(string name, string desc, int amount, PerkRarity rarity)
    {
        UpgradePerk perk = ScriptableObject.CreateInstance<UpgradePerk>();
        perk.perkName = name;
        perk.description = desc;
        perk.amountToUpgrade = amount;
        perk.rarity = rarity;

        string path = $"Assets/ScriptableObjects/Perks/{name.Replace(" ", "")}.asset";
        AssetDatabase.CreateAsset(perk, path);
    }

    static void CreateDicePerk(DiceData dice)
    {
        DicePerk perk = ScriptableObject.CreateInstance<DicePerk>();
        perk.perkName = $"Get {dice.diceName}";
        perk.description = $"Adds a {dice.diceName} to your inventory.";
        perk.diceToGive = dice;
        perk.rarity = PerkRarity.Rare; // Default to Rare for dice
        perk.icon = dice.upgradeSprites != null && dice.upgradeSprites.Length > 0 ? dice.upgradeSprites[0] : null;

        string path = $"Assets/ScriptableObjects/Perks/Get{dice.diceName.Replace(" ", "")}.asset";
        AssetDatabase.CreateAsset(perk, path);
    }
}
