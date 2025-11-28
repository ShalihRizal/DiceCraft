using UnityEngine;
using UnityEditor;
using System.Text;

public class MapDebugTool : EditorWindow
{
    [MenuItem("Tools/Debug Map Nodes")]
    public static void DebugNodes()
    {
        if (MapManager.Instance == null || MapManager.Instance.currentMap == null)
        {
            Debug.LogError("No Map Found!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("--- Map Node Status ---");
        
        var map = MapManager.Instance.currentMap;
        for (int i = 0; i < map.Count; i++)
        {
            sb.AppendLine($"Layer {i}:");
            foreach (var node in map[i])
            {
                string status = "";
                if (node.isCompleted) status += "[COMPLETED] ";
                if (node.isLocked) status += "[LOCKED] ";
                if (node.isAvailable) status += "[AVAILABLE] ";
                
                sb.AppendLine($"  Node {node.nodeIndex} (Row {node.rowIndex}): {status}");
                
                if (node.isCompleted)
                {
                    sb.Append("    -> Connects to: ");
                    foreach(var conn in node.outgoingConnections)
                    {
                        sb.Append($"L{conn.targetLayer}:N{conn.targetIndex} ");
                    }
                    sb.AppendLine();
                }
            }
        }
        Debug.Log(sb.ToString());
    }
}
