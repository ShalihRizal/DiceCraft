#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AchievementSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Achievements")]
    static void SetupAchievements()
    {
        string path = "Assets/Resources/Achievements";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // 1. First Death
        AchievementData firstDeath = CreateAchievement(path, "first_death", "First Blood", "Die for the first time.", true, 1);
        firstDeath.rewards = new List<AchievementReward>
        {
            new AchievementReward { type = AchievementReward.RewardType.Coin, amount = 100 }
        };
        EditorUtility.SetDirty(firstDeath);

        // 2. Newbie Killer
        AchievementData newbieKiller = CreateAchievement(path, "newbie_killer", "Newbie Killer", "Defeat 10 enemies.", false, 10);
        newbieKiller.rewards = new List<AchievementReward>
        {
            new AchievementReward { type = AchievementReward.RewardType.DicePip, amount = 5 },
            new AchievementReward { type = AchievementReward.RewardType.Coin, amount = 50 }
        };
        EditorUtility.SetDirty(newbieKiller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assign to Manager
        AchievementManager manager = FindFirstObjectByType<AchievementManager>();
        if (manager != null)
        {
            manager.allAchievements = new List<AchievementData> { firstDeath, newbieKiller };
            EditorUtility.SetDirty(manager);
            Debug.Log("✅ Achievements created and assigned to Manager!");
        }
        else
        {
            Debug.LogWarning("⚠️ Achievements created, but AchievementManager not found in scene. Please run Main Menu Setup first.");
        }
    }

    static AchievementData CreateAchievement(string path, string id, string title, string desc, bool single, int target)
    {
        string assetPath = $"{path}/{id}.asset";
        AchievementData data = AssetDatabase.LoadAssetAtPath<AchievementData>(assetPath);
        
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<AchievementData>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        data.id = id;
        data.title = title;
        data.description = desc;
        data.isSingleTrigger = single;
        data.targetValue = target;
        
        return data;
    }
}
#endif
