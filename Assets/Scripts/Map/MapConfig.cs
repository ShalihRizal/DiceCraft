using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Map/Map Config")]
public class MapConfig : ScriptableObject
{
    [Header("Grid Settings")]
    public int layers = 10;
    public int minNodesPerLayer = 3;
    public int maxNodesPerLayer = 5;
    public float nodeSpacingX = 200f;
    public float nodeSpacingY = 150f;

    [Header("Node Weights")]
    public List<NodeTypeWeight> nodeWeights;

    [Header("Boss Settings")]
    public NodeType bossNodeType = NodeType.Boss;
}

[System.Serializable]
public struct NodeTypeWeight
{
    public NodeType type;
    public int weight;
}
