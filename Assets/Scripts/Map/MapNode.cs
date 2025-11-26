using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Combat,
    Elite,
    Shop,
    Campfire,
    Event,
    Boss
}

[System.Serializable]
public class MapNode
{
    public NodeType nodeType;
    public Vector2 position; // Grid position (Layer, Index) or UI position
    public int layerIndex;
    public int nodeIndex;
    
    public List<int> outgoingConnectionIndices = new List<int>();
    public List<int> incomingConnectionIndices = new List<int>();

    public bool isCompleted = false;
    public bool isLocked = true; // True if not reachable yet
    public bool isAvailable = false; // True if player can move here

    public MapNode(NodeType type, int layer, int index)
    {
        nodeType = type;
        layerIndex = layer;
        nodeIndex = index;
    }
}
