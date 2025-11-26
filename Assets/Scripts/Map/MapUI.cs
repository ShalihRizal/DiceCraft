using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public GameObject nodePrefab;
    public Transform contentContainer;
    public ScrollRect scrollRect;
    
    // For drawing lines
    public GameObject linePrefab; // Simple UI Image stretched
    public Transform lineContainer;
    public Vector2 lineOffset; // Manual offset adjustment
    public Vector2 contentOffset; // Manual content position adjustment
    
    [Header("Highlighting")]
    public Color pathHighlightColor = Color.yellow;
    public Color defaultLineColor = Color.white;
    public Color dimmedLineColor = new Color(1, 1, 1, 0.2f);

    private List<MapNodeUI> nodeUIs = new List<MapNodeUI>();
    private Dictionary<(MapNode, MapNode), Image> lineImages = new Dictionary<(MapNode, MapNode), Image>();
    
    private CanvasGroup canvasGroup;
    private bool isPeeking = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (GameManager.Instance.IsMapActive)
        {
            // Toggle with RMB
            if (Input.GetMouseButtonDown(1))
            {
                if (canvasGroup.alpha > 0.5f) Hide();
                else Show();
            }
        }
        else
        {
            // Priority: Clean up peeking if it was active
            if (isPeeking)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    isPeeking = false;
                    Hide();
                }
                return; // Consume input while peeking
            }

            // Blockers:
            // 1. Reward Phase Active (Input blocked)
            if (GameManager.Instance.IsRewardPhaseActive) return;
            
            // 2. Pending Rewards (RMB reserved for toggle)
            if (RewardManager.Instance != null && RewardManager.Instance.rewardUI != null && RewardManager.Instance.rewardUI.HasPendingRewards) return;

            // Peek with RMB
            if (Input.GetMouseButtonDown(1))
            {
                isPeeking = true;
                Show();
                SetInteractive(false);
            }
        }
    }
    
    public void SetInteractive(bool interactive)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = interactive;
            canvasGroup.blocksRaycasts = interactive;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnMapGenerated += OnMapGenerated;
    }

    private void OnDisable()
    {
        GameEvents.OnMapGenerated -= OnMapGenerated;
    }

    private void OnMapGenerated()
    {
        ValidateContainers();
        ClearMap();
        DrawMap();
    }

    private void ValidateContainers()
    {
        if (contentContainer == null)
        {
            GameObject content = new GameObject("Content");
            content.transform.SetParent(transform, false);
            contentContainer = content.transform;
        }
        
        // We no longer use a separate LineContainer to avoid coordinate issues.
        // Lines will be drawn directly in Content, before nodes.
        lineContainer = contentContainer;
    }

    private void ClearMap()
    {
        if (contentContainer != null)
        {
            foreach (Transform child in contentContainer) 
            {
                Destroy(child.gameObject);
            }
        }
        nodeUIs.Clear();
        lineImages.Clear();
    }

    private void DrawMap()
    {
        var map = MapManager.Instance.currentMap;
        if (map == null) return;
        
        lineImages.Clear();

        // 1. Draw Connections (First, so they are behind nodes)
        if (linePrefab != null)
        {
            foreach (var layer in map)
            {
                foreach (var node in layer)
                {
                    foreach (int targetIndex in node.outgoingConnectionIndices)
                    {
                        if (node.layerIndex + 1 < map.Count)
                        {
                            var currentMap = MapManager.Instance.currentMap;
                            var nextLayer = currentMap[node.layerIndex + 1];
                            if (targetIndex < nextLayer.Count)
                            {
                                MapNode target = nextLayer[targetIndex];
                                DrawLine(node, target);
                            }
                        }
                    }
                }
            }
        }

        // 2. Spawn Nodes (Second, so they are on top)
        foreach (var layer in map)
        {
            foreach (var node in layer)
            {
                if (nodePrefab != null)
                {
                    GameObject obj = Instantiate(nodePrefab, contentContainer);
                    
                    // Force Anchor/Pivot to Middle Left
                    RectTransform rect = obj.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = new Vector2(0f, 0.5f);
                        rect.anchorMax = new Vector2(0f, 0.5f);
                        rect.pivot = new Vector2(0.5f, 0.5f);
                        rect.localScale = Vector3.one;
                    }
                    
                    obj.transform.localPosition = node.position;
                    
                    MapNodeUI ui = obj.GetComponent<MapNodeUI>();
                    if (ui != null)
                    {
                        ui.Setup(node, this);
                        nodeUIs.Add(ui);
                    }
                }
            }
        }
        
        // Scroll to bottom (start)
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    private void DrawLine(MapNode startNode, MapNode endNode)
    {
        Vector2 start = startNode.position;
        Vector2 end = endNode.position;

        // Instantiate into contentContainer
        GameObject lineObj = Instantiate(linePrefab, contentContainer);
        RectTransform rect = lineObj.GetComponent<RectTransform>();
        
        // Force Anchor/Pivot to Middle Left
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        
        Vector2 dir = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        
        // Apply offset
        rect.localPosition = (start + lineOffset) + dir * distance * 0.5f;
        rect.sizeDelta = new Vector2(distance, 5f); // Thickness
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.localRotation = Quaternion.Euler(0, 0, angle);
        
        Image img = lineObj.GetComponent<Image>();
        if (img != null)
        {
            img.color = defaultLineColor;
            lineImages[(startNode, endNode)] = img;
        }
    }
    
    public void HighlightPath(MapNode startNode)
    {
        // Find all reachable nodes
        HashSet<MapNode> reachableNodes = new HashSet<MapNode>();
        HashSet<(MapNode, MapNode)> reachableLines = new HashSet<(MapNode, MapNode)>();
        
        Queue<MapNode> queue = new Queue<MapNode>();
        queue.Enqueue(startNode);
        reachableNodes.Add(startNode);
        
        var map = MapManager.Instance.currentMap;

        while (queue.Count > 0)
        {
            MapNode current = queue.Dequeue();
            
            if (current.layerIndex + 1 < map.Count)
            {
                var nextLayer = map[current.layerIndex + 1];
                foreach (int targetIndex in current.outgoingConnectionIndices)
                {
                    if (targetIndex < nextLayer.Count)
                    {
                        MapNode target = nextLayer[targetIndex];
                        
                        // Add line
                        reachableLines.Add((current, target));
                        
                        // Add node if not visited
                        if (!reachableNodes.Contains(target))
                        {
                            reachableNodes.Add(target);
                            queue.Enqueue(target);
                        }
                    }
                }
            }
        }
        
        // Apply Visuals
        foreach (var ui in nodeUIs)
        {
            if (reachableNodes.Contains(ui.node))
            {
                ui.SetDimmed(false); // Reset color first
                ui.SetHighlight(true); // Then apply highlight
            }
            else
            {
                ui.SetHighlight(false);
                ui.SetDimmed(true);
            }
        }
        
        foreach (var kvp in lineImages)
        {
            if (reachableLines.Contains(kvp.Key))
            {
                kvp.Value.color = pathHighlightColor;
            }
            else
            {
                kvp.Value.color = dimmedLineColor;
            }
        }
    }
    
    public void ResetHighlight()
    {
        foreach (var ui in nodeUIs)
        {
            ui.SetHighlight(false);
            ui.SetDimmed(false);
        }
        
        foreach (var img in lineImages.Values)
        {
            img.color = defaultLineColor;
        }
    }

    public void Show()
    {
        // Ensure CanvasGroup exists
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        gameObject.SetActive(true); // Ensure object is active for Update loop
        transform.SetAsLastSibling(); // Bring to front
        
        // Show via CanvasGroup
        canvasGroup.alpha = 1f;
        SetInteractive(true); // Default to interactive
        
        // Ensure RectTransform stretches to fill screen
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        // Align Content to Middle Left for Horizontal Scrolling
        if (contentContainer != null)
        {
            RectTransform contentRect = contentContainer.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = new Vector2(0f, 0.5f); // Middle Left
                contentRect.anchorMax = new Vector2(0f, 0.5f);
                contentRect.pivot = new Vector2(0f, 0.5f);
                contentRect.anchoredPosition = contentOffset; // Apply manual offset
                
                // Calculate Map Size
                float width = 0;
                float height = 0;
                var currentMap = MapManager.Instance.currentMap;
                if (currentMap != null && currentMap.Count > 0)
                {
                    width = (currentMap.Count) * MapManager.Instance.mapConfig.nodeSpacingX + 200f; // Add padding
                    int maxNodes = 0;
                    foreach(var layer in currentMap) maxNodes = Mathf.Max(maxNodes, layer.Count);
                    height = maxNodes * MapManager.Instance.mapConfig.nodeSpacingY + 200f;
                }
                contentRect.sizeDelta = new Vector2(width, height);
            }
        }
        
        // Ensure background exists
        Image bg = GetComponent<Image>();
        if (bg == null)
        {
            bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark background
        }
        
        // Refresh visuals
        foreach(var ui in nodeUIs) ui.UpdateVisuals();
        
        // Scroll to start (Left)
        if (scrollRect != null) 
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.content = contentContainer.GetComponent<RectTransform>(); // Ensure content is assigned
            scrollRect.horizontalNormalizedPosition = 0f;
        }
    }
    
    public void Hide()
    {
        // Hide via CanvasGroup but keep GameObject active for Update loop
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            SetInteractive(false);
        }
        else
        {
            gameObject.SetActive(false); // Fallback
        }
    }
}
