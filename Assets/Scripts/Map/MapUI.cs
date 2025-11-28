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

    private float lastSpacingX;
    private float lastSpacingY;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        // If map already exists (e.g. returning from scene), draw it immediately
        if (MapManager.Instance != null && MapManager.Instance.currentMap != null && nodeUIs.Count == 0)
        {
            ValidateContainers();
            DrawMap();
        }
    }

    private void Update()
    {
        // Runtime Config Update Check
        if (MapManager.Instance != null && MapManager.Instance.mapConfig != null)
        {
            var config = MapManager.Instance.mapConfig;
            if (Mathf.Abs(config.nodeSpacingX - lastSpacingX) > 0.01f || Mathf.Abs(config.nodeSpacingY - lastSpacingY) > 0.01f)
            {
                lastSpacingX = config.nodeSpacingX;
                lastSpacingY = config.nodeSpacingY;
                RefreshGridPositions();
            }
        }

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
        
        // Load Hex Sprite
        Sprite hexSprite = Resources.Load<Sprite>("HexNode"); 

        // 1. Spawn Nodes
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
                        
                        // Set size to match spacing (slightly smaller for padding)
                        float size = MapManager.Instance.mapConfig.nodeSpacingY * 0.66f; 
                        rect.sizeDelta = new Vector2(size, size);
                    }
                    
                    obj.transform.localPosition = node.position;
                    
                    MapNodeUI ui = obj.GetComponent<MapNodeUI>();
                    if (ui != null)
                    {
                        ui.Setup(node, this);
                        nodeUIs.Add(ui);
                        
                        // Set Hex Background
                        if (hexSprite != null && ui.hexImage != null)
                        {
                             ui.hexImage.sprite = hexSprite;
                        }
                        
                        // Set Type Icon
                        Sprite iconSprite = null;
                        if (MapManager.Instance.mapConfig.nodeIcons != null)
                        {
                            var iconData = MapManager.Instance.mapConfig.nodeIcons.Find(x => x.type == node.nodeType);
                            iconSprite = iconData.icon;
                        }
                        ui.SetIcon(iconSprite);
                    }
                }
            }
        }

        // 2. Draw History Lines (Only for completed/visited nodes)
        RedrawLines();
        
        // Scroll to bottom (start)
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    public void RedrawLines()
    {
        // Clear existing lines
        foreach(var img in lineImages.Values)
        {
            if (img != null) Destroy(img.gameObject);
        }
        lineImages.Clear();

        var map = MapManager.Instance.currentMap;
        if (map == null) return;

        // Draw lines for visited paths
        // A path is visited if 'start' is completed and 'end' is available/completed/locked(but visited?)
        // Actually, we only draw lines that the player HAS taken.
        // So we need to know the path history.
        // Or simpler: If a node is completed, draw lines to its children that are ALSO completed or are the Current Node.
        
        foreach (var layer in map)
        {
            foreach (var node in layer)
            {
                if (node.isCompleted)
                {
                    if (node.layerIndex + 1 < map.Count)
                    {
                        // Iterate through outgoing connections
                        foreach (var connection in node.outgoingConnections)
                        {
                            if (connection.targetLayer < map.Count)
                            {
                                var targetLayer = map[connection.targetLayer];
                                if (connection.targetIndex < targetLayer.Count)
                                {
                                    MapNode target = targetLayer[connection.targetIndex];
                                    
                                    // Draw line if target is the CURRENT node or is COMPLETED
                                    if (target.isCompleted || target == MapManager.Instance.currentNode)
                                    {
                                        DrawLine(node, target, true);
                                    }
                                    else if (target.isAvailable && !target.isLocked)
                                    {
                                         DrawLine(node, target, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void DrawLine(MapNode startNode, MapNode endNode, bool isHistory)
    {
        Vector2 start = startNode.position;
        Vector2 end = endNode.position;

        // Instantiate into contentContainer
        GameObject lineObj = Instantiate(linePrefab, contentContainer);
        lineObj.transform.SetAsFirstSibling(); // Send to back
        
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
            img.color = isHistory ? pathHighlightColor : dimmedLineColor;
            // If it's a future path (not history), maybe make it very subtle
            if (!isHistory) img.color = new Color(1, 1, 1, 0.05f); 
            
            lineImages[(startNode, endNode)] = img;
        }
    }
    
    public void OnNodeClicked(MapNode node)
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.SelectNode(node);
        }
    }

    // Highlight logic removed as per user request
    public void HighlightPath(MapNode startNode) { }
    public void ResetHighlight() { }

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

    private void RefreshGridPositions()
    {
        if (MapManager.Instance == null || MapManager.Instance.currentMap == null) return;
        
        // 1. Recalculate Data Positions
        if (MapManager.Instance.mapGenerator != null)
        {
            MapManager.Instance.mapGenerator.RecalculatePositions(MapManager.Instance.currentMap);
        }

        // 2. Update UI Positions and Sizes
        // User requested "gap of each node in the same column by half a node"
        // If NodeHeight = H, Gap = 0.5H, Spacing = 1.5H.
        // So H = Spacing / 1.5 = Spacing * 0.666f.
        float size = MapManager.Instance.mapConfig.nodeSpacingY * 0.66f; 
        
        foreach (var ui in nodeUIs)
        {
            if (ui != null && ui.node != null)
            {
                ui.transform.localPosition = ui.node.position;
                
                RectTransform rect = ui.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(size, size);
                }
            }
        }
        
        // 3. Redraw Lines (since positions changed)
        RedrawLines();
        
        // 4. Update Content Size
        if (contentContainer != null)
        {
            RectTransform contentRect = contentContainer.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                 float width = (MapManager.Instance.currentMap.Count) * MapManager.Instance.mapConfig.nodeSpacingX + 200f;
                 int maxNodes = 0;
                 foreach(var layer in MapManager.Instance.currentMap) maxNodes = Mathf.Max(maxNodes, layer.Count);
                 float height = maxNodes * MapManager.Instance.mapConfig.nodeSpacingY + 200f;
                 contentRect.sizeDelta = new Vector2(width, height);
            }
        }
    }
}
