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
    
    public int currentPlane = 1;
    public const int MaxPlanes = 3;

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
        currentMap = mapGenerator.GenerateMap(currentPlane);
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
        
        // Also lock the previous layer if we moved forward
        if (node.layerIndex > 0)
        {
            foreach(var n in currentMap[node.layerIndex - 1])
            {
                n.isAvailable = false;
            }
        }

        // Unlock connected nodes in next layers
        foreach (var connection in node.outgoingConnections)
        {
            if (connection.targetLayer < currentMap.Count)
            {
                var targetLayer = currentMap[connection.targetLayer];
                if (connection.targetIndex < targetLayer.Count)
                {
                    MapNode nextNode = targetLayer[connection.targetIndex];
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
                if (ShopManager.Instance != null)
                {
                    GameManager.Instance.IsMapActive = false;
                    GameManager.Instance.IsCombatActive = false;
                    // Hide Map
                    MapUI mapUI = FindFirstObjectByType<MapUI>(FindObjectsInactive.Include);
                    if (mapUI != null) mapUI.Hide();
                    
                    ShopManager.Instance.ShowShop();
                }
                else
                {
                    Debug.LogError("ShopManager not found!");
                    CompleteCurrentNode();
                }
                break;
            case NodeType.Reward:
                if (RewardManager.Instance != null)
                {
                    GameManager.Instance.IsMapActive = false;
                    GameManager.Instance.IsCombatActive = false;
                    MapUI mapUI = FindFirstObjectByType<MapUI>(FindObjectsInactive.Include);
                    if (mapUI != null) mapUI.Hide();
                    
                    // Treasure Room: Maybe better rewards? For now, Dice.
                    RewardManager.Instance.GenerateRewards(RewardManager.RewardType.Dice);
                }
                else
                {
                    CompleteCurrentNode();
                }
                break;
            case NodeType.Event:
            case NodeType.Campfire:
                // Placeholder: Just complete immediately for now
                Debug.Log($"Visited {type} node. (Placeholder)");
                CompleteCurrentNode();
                break;
        }
    }

    public void CompleteCurrentNode()
    {
        // Called when player wins combat or finishes shop
        // Return to Map View
        
        if (currentNode != null && currentNode.nodeType == NodeType.Boss)
        {
            OnBossCompleted();
        }
        else
        {
            GameManager.Instance.ShowMap();
        }
    }
    
    private void OnBossCompleted()
    {
        Debug.Log($"🏆 Boss of Plane {currentPlane} Defeated!");
        
        if (currentPlane < MaxPlanes)
        {
            currentPlane++;
            Debug.Log($"✈️ Advancing to Plane {currentPlane}...");
            
            // Increase Difficulty (Example)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.globalDamageMultiplier += 0.5f; // Harder enemies
            }
            
            // Regenerate Map
            StartNewRun(); 
        }
        else
        {
            Debug.Log("🎉 VICTORY! All Planes Cleared!");
            GameEvents.RaiseGameOver(); // Or RaiseVictory()
        }
    }
}
