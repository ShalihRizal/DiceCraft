using UnityEngine;
using UnityEditor;
using System.IO;

public class DiceAssetGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Elemental Dice")]
    public static void Generate()
    {
        string path = "Assets/ScriptableObjects/DiceDatas";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        CreateDice("Anemo", "AnemoPassive", "Buffs adjacent dice attack speed.", 10, 0.5f, 100);
        CreateDice("Electro", "ElectroPassive", "Attacks chain to another enemy.", 12, 0.6f, 120);
        CreateDice("Fire", "FirePassive", "Deals more damage if > 2 enemies.", 15, 0.8f, 150);
        CreateDice("Heal", "HealPassive", "Heals the player.", 0, 1.0f, 200);
        CreateDice("Ice", "IcePassive", "Shields the player.", 0, 1.2f, 180);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Elemental Dice Generated!");
    }

    static void CreateDice(string name, string passiveScript, string desc, int damage, float fireRate, int cost)
    {
        // 1. Create Passive
        DicePassive passive = ScriptableObject.CreateInstance(passiveScript) as DicePassive;
        if (passive == null)
        {
            Debug.LogError($"Could not create passive: {passiveScript}");
            return;
        }
        passive.passiveName = $"{name} Passive";
        passive.description = desc;
        
        string passivePath = $"Assets/ScriptableObjects/DiceDatas/{name}Passive.asset";
        AssetDatabase.CreateAsset(passive, passivePath);

        // 2. Create DiceData
        DiceData data = ScriptableObject.CreateInstance<DiceData>();
        data.diceName = $"{name} Dice";
        data.description = desc;
        data.baseDamage = damage;
        data.baseFireInterval = fireRate;
        data.cost = cost;
        data.sides = 6;
        data.passive = passive;
        data.canAttack = damage > 0; // Heal/Ice might not attack in traditional sense, but they "fire"

        // Assign default sprites/vfx if available (placeholders)
        // data.icon = ...

        string dataPath = $"Assets/ScriptableObjects/DiceDatas/{name}Dice.asset";
        AssetDatabase.CreateAsset(data, dataPath);
    }
}
