using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RelicSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Relics")]
    public static void ShowWindow()
    {
        GetWindow<RelicSetupTool>("Relic Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Relic System Setup", EditorStyles.boldLabel);

        if (GUILayout.Button("1. Create Default Relics"))
        {
            CreateDefaultRelics();
        }

        if (GUILayout.Button("2. Assign Relics to ShopManager"))
        {
            AssignRelicsToShop();
        }
    }

    private void CreateDefaultRelics()
    {
        string path = "Assets/Resources/Relics";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // 1. Vampire's Fang: Heal 2 HP on Kill (Upgradeable: +1 HP/lvl, Max 3)
        CreateRelic("Vampire's Fang", "Heals 2 HP on enemy kill.", RelicRarity.Rare, 150, 
            RelicTrigger.OnKill, RelicEffect.Heal, 2f, 1f, 3, path);

        // 2. Merchant's Wallet: +10 Gold on Combat Start (Upgradeable: +5 Gold/lvl, Max 5)
        CreateRelic("Merchant's Wallet", "Gain +10 Gold on combat start.", RelicRarity.Common, 75, 
            RelicTrigger.OnCombatStart, RelicEffect.GainGold, 10f, 5f, 5, path);

        // 3. Warrior's Ring: +10% Damage (Upgradeable: +5%/lvl, Max 3)
        CreateRelic("Warrior's Ring", "Increases damage by 10%.", RelicRarity.Rare, 200, 
            RelicTrigger.Passive, RelicEffect.DamageMultiplier, 0.1f, 0.05f, 3, path);

        // 4. Titan's Heart: +20 Max HP (Not Upgradeable)
        CreateRelic("Titan's Heart", "Increases Max HP by 20.", RelicRarity.Epic, 300, 
            RelicTrigger.Passive, RelicEffect.MaxHealth, 20f, 0f, 1, path);

        // 5. Gambler's Dice: +5% Crit Chance (Not Upgradeable)
        CreateRelic("Gambler's Dice", "Increases Critical Chance by 5%.", RelicRarity.Uncommon, 120, 
            RelicTrigger.Passive, RelicEffect.CritChance, 5f, 0f, 1, path);

        // 6. Ancient Lighter: +10% Fire Dice Passive Boost (Upgradeable: +5%/lvl, Max 3)
        DiceData fireDice = AssetDatabase.LoadAssetAtPath<DiceData>("Assets/ScriptableObjects/DiceDatas/FireDice.asset");
        if (fireDice != null)
        {
            CreateDiceSpecificRelic("Ancient Lighter", "Increases Fire Dice passive damage bonus by 10%.", 
                RelicRarity.Rare, 180, RelicTrigger.Passive, RelicEffect.DicePassiveBoost, 
                0.1f, 0.05f, 3, fireDice, path);
        }
        else
        {
            Debug.LogWarning("FireDice asset not found! Skipping Ancient Lighter creation.");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Default Relics created in " + path);
    }

    private void CreateRelic(string name, string desc, RelicRarity rarity, int cost, 
        RelicTrigger trigger, RelicEffect effect, float baseVal, float valPerLvl, int maxLvl, string path)
    {
        string assetPath = $"{path}/{name.Replace(" ", "").Replace("'", "")}.asset";
        RelicData relic = AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
        
        if (relic == null)
        {
            relic = ScriptableObject.CreateInstance<RelicData>();
            AssetDatabase.CreateAsset(relic, assetPath);
        }

        relic.relicName = name;
        relic.description = desc;
        relic.rarity = rarity;
        relic.cost = cost;
        
        relic.trigger = trigger;
        relic.effect = effect;
        relic.baseValue = baseVal;
        relic.valuePerLevel = valPerLvl;
        relic.maxLevel = maxLvl;
        relic.currentLevel = 1;

        EditorUtility.SetDirty(relic);
    }

    private void CreateDiceSpecificRelic(string name, string desc, RelicRarity rarity, int cost, 
        RelicTrigger trigger, RelicEffect effect, float baseVal, float valPerLvl, int maxLvl, 
        DiceData targetDice, string path)
    {
        string assetPath = $"{path}/{name.Replace(" ", "").Replace("'", "")}.asset";
        RelicData relic = AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
        
        if (relic == null)
        {
            relic = ScriptableObject.CreateInstance<RelicData>();
            AssetDatabase.CreateAsset(relic, assetPath);
        }

        relic.relicName = name;
        relic.description = desc;
        relic.rarity = rarity;
        relic.cost = cost;
        
        relic.trigger = trigger;
        relic.effect = effect;
        relic.baseValue = baseVal;
        relic.valuePerLevel = valPerLvl;
        relic.maxLevel = maxLvl;
        relic.currentLevel = 1;
        relic.targetDiceData = targetDice; // Set the dice data reference

        EditorUtility.SetDirty(relic);
    }

    private void AssignRelicsToShop()
    {
        ShopManager shop = FindFirstObjectByType<ShopManager>();
        if (shop == null)
        {
            // Try to find prefab or open scene? 
            // For now, just warn.
            Debug.LogError("ShopManager not found in the scene! Please open the Gameplay scene.");
            return;
        }

        // Find all RelicData in project
        string[] guids = AssetDatabase.FindAssets("t:RelicData");
        List<RelicData> relics = new List<RelicData>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RelicData relic = AssetDatabase.LoadAssetAtPath<RelicData>(path);
            if (relic != null)
            {
                relics.Add(relic);
            }
        }

        shop.relicPool = relics;
        EditorUtility.SetDirty(shop);
        Debug.Log($"Assigned {relics.Count} relics to ShopManager.");
    }
}
