#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;

public static class DicePassiveCreator
{
    private const string ScriptFolder = "Assets/Scripts/Dice/Passives";
    private const string AssetFolder = "Assets/GameData/Passives";

    private static string lastCreatedAssetPath;
    private static string lastCreatedName = "NewPassive";
    private static double lastCreationTime;

    [MenuItem("Assets/Create/Dice/New Passive", priority = 0)]
    public static void CreateNewPassive()
    {
        if (!Directory.Exists(ScriptFolder)) Directory.CreateDirectory(ScriptFolder);
        if (!Directory.Exists(AssetFolder)) Directory.CreateDirectory(AssetFolder);

        var newPassive = ScriptableObject.CreateInstance<DicePassive>();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{AssetFolder}/{lastCreatedName}.asset");

        ProjectWindowUtil.CreateAsset(newPassive, assetPath);
        AssetDatabase.SaveAssets();

        lastCreatedAssetPath = assetPath;
        lastCreationTime = EditorApplication.timeSinceStartup;

        Debug.Log("🧩 Created new passive placeholder — script will auto-generate after renaming.");

        // Subscribe to detect rename or project update
        EditorApplication.projectChanged -= CheckForRenamedPassive;
        EditorApplication.projectChanged += CheckForRenamedPassive;
    }

    private static void CheckForRenamedPassive()
    {
        if (string.IsNullOrEmpty(lastCreatedAssetPath)) return;
        if (EditorApplication.timeSinceStartup - lastCreationTime < 0.5f) return; // Wait a bit after creation

        string currentPath = lastCreatedAssetPath;
        if (!File.Exists(currentPath))
        {
            // Find renamed asset
            string dir = Path.GetDirectoryName(AssetFolder);
            var candidates = AssetDatabase.FindAssets("t:DicePassive", new[] { AssetFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => File.GetLastWriteTime(p) > DateTime.Now.AddSeconds(-5))
                .ToList();

            if (candidates.Count == 0) return;
            currentPath = candidates.OrderByDescending(File.GetLastWriteTime).First();
        }

        string newName = Path.GetFileNameWithoutExtension(currentPath);
        string scriptPath = $"{ScriptFolder}/{newName}.cs";

        if (File.Exists(scriptPath)) return;

        CreatePassiveScript(newName, scriptPath);
        AssetDatabase.Refresh();
        Debug.Log($"✅ Created script for passive: {newName}.cs");

        // Auto-open in code editor
        var scriptObj = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
        if (scriptObj != null)
            AssetDatabase.OpenAsset(scriptObj);

        // Unsubscribe
        EditorApplication.projectChanged -= CheckForRenamedPassive;
        lastCreatedAssetPath = null;
    }

    private static void CreatePassiveScript(string className, string path)
    {
        string scriptContent =
$@"using UnityEngine;

[CreateAssetMenu(menuName = ""Dice/Passives/{className}"")]
public class {className} : DicePassive
{{
    public override void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {{
        // TODO: Implement {className} logic
        Debug.Log(""{className} triggered OnDiceFire"");
    }}
}}";
        File.WriteAllText(path, scriptContent);
    }
}
#endif
