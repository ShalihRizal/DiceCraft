using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("References")]
    public MapGenerator mapGenerator;
    public MapConfig mapConfig;

    [Header("State")]
    public List<List<MapNode>> currentMap;
    public MapNode currentNode;
    public int currentLayerIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // For testing, generate map on start if none exists
        if (currentMap == null || currentMap.Count == 0)
        {
            StartNewRun();
        }
    }

    public void StartNewRun()
    {
        currentMap = mapGenerator.GenerateMap();
        currentLayerIndex = 0;
        currentNode = null;
        
        // Unlock first layer
        if (currentMap != null && currentMap.Count > 0)
        {
            foreach (var node in currentMap[0])
            {
                node.isAvailable = true;
                node.isLocked = false;
            }
        }
        
        GameEvents.RaiseMapGenerated();
        
        // Ensure Map is shown immediately
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowMap();
        }
    }

    public void SelectNode(MapNode node)
    {
        if (node.isLocked || !node.isAvailable) return;

        currentNode = node;
        currentLayerIndex = node.layerIndex;
        node.isCompleted = true;

        // Lock all nodes in current layer
        foreach(var n in currentMap[node.layerIndex])
        {
            n.isAvailable = false;
        }

        // Unlock connected nodes in next layer
        if (currentLayerIndex + 1 < currentMap.Count)
        {
            List<MapNode> nextLayer = currentMap[currentLayerIndex + 1];
            foreach (int nextNodeIndex in node.outgoingConnectionIndices)
            {
                // Find the node in the next layer with this index
                // Note: MapNode.nodeIndex is just its index in the list
                if (nextNodeIndex < nextLayer.Count)
                {
                    MapNode nextNode = nextLayer[nextNodeIndex];
                    nextNode.isLocked = false;
                    nextNode.isAvailable = true;
                }
            }
        }

        Debug.Log($"Selected Node: {node.nodeType} at Layer {node.layerIndex}");
        
        // Transition to Gameplay Scene based on Type
        LoadNodeScene(node.nodeType);
    }

    private void LoadNodeScene(NodeType type)
    {
        switch (type)
        {
            case NodeType.Combat:
            case NodeType.Elite:
            case NodeType.Boss:
                // Enter Preparation Phase first
                GameManager.Instance.StartPreparationPhase();
                break;
            case NodeType.Shop:
                // Start Shop
                // GameManager.Instance.StartShop();
                break;
            case NodeType.Event:
            case NodeType.Campfire:
                // TODO: Implement Event/Campfire
                break;
        }
    }
    
    public void CompleteCurrentNode()
    {
        // Called when player wins combat or finishes shop
        // Return to Map View
        GameManager.Instance.ShowMap();
    }
}
