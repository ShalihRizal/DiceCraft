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

        CreateRelic("Healing Potion", "Heals 10 HP on pickup.", RelicRarity.Common, 50, RelicEffectType.None, 10, path);
        CreateRelic("Vampiric Dagger", "Heals 1 HP on enemy kill.", RelicRarity.Rare, 150, RelicEffectType.OnKill, 1, path);
        CreateRelic("Lucky Coin", "Gain +10 Gold on combat start.", RelicRarity.Common, 75, RelicEffectType.OnCombatStart, 10, path);
        CreateRelic("Warrior's Ring", "Increases damage by 10%.", RelicRarity.Rare, 200, RelicEffectType.StatBoost, 0.1f, path);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Default Relics created in " + path);
    }

    private void CreateRelic(string name, string desc, RelicRarity rarity, int cost, RelicEffectType type, float value, string path)
    {
        string assetPath = $"{path}/{name.Replace(" ", "")}.asset";
        if (AssetDatabase.LoadAssetAtPath<RelicData>(assetPath) != null)
        {
            Debug.Log($"Relic {name} already exists. Skipping.");
            return;
        }

        RelicData relic = ScriptableObject.CreateInstance<RelicData>();
        relic.relicName = name;
        relic.description = desc;
        relic.rarity = rarity;
        relic.cost = cost;
        relic.effectType = type;
        relic.effectValue = value;

        AssetDatabase.CreateAsset(relic, assetPath);
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
