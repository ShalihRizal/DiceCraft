using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class CameraSetupTool
{
    [MenuItem("Tools/Fix Camera Warning (Select Camera First)")]
    static void ShowInstructions()
    {
        string message = "To fix the camera warning:\n\n" +
                        "1. Select 'Main Camera' in the Hierarchy\n" +
                        "2. In the Inspector, click 'Add Component'\n" +
                        "3. Search for 'Universal Additional Camera Data'\n" +
                        "4. Click to add it\n\n" +
                        "If you can't find that component, you might not be using URP.\n" +
                        "In that case, you can safely ignore the warning.";
        
        EditorUtility.DisplayDialog("Fix Camera Warning", message, "OK");
    }
}
#endif
