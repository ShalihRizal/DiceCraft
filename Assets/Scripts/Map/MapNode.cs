using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Combat,
    Elite,
    Shop,
    Campfire,
    Event,
    Boss,
    Reward
}

[System.Serializable]
public class MapNode
{
    public NodeType nodeType;
    public Vector2 position; // Grid position (Layer, Index) or UI position
    public int layerIndex;
    public int nodeIndex; // Index in the layer list
    public int rowIndex; // Grid row index (0 to maxRows)
    
    public List<NodeConnection> outgoingConnections = new List<NodeConnection>();
    public List<NodeConnection> incomingConnections = new List<NodeConnection>();

    public bool isCompleted = false;
    public bool isLocked = true; // True if not reachable yet
    public bool isAvailable = false; // True if player can move here

    public MapNode(NodeType type, int layer, int index)
    {
        nodeType = type;
        layerIndex = layer;
        nodeIndex = index;
        rowIndex = index; // Default
    }
}

[System.Serializable]
public struct NodeConnection
{
    public int targetLayer;
    public int targetIndex;

    public NodeConnection(int layer, int index)
    {
        targetLayer = layer;
        targetIndex = index;
    }
}
