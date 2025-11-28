using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MapNodeUI : MonoBehaviour, IPointerClickHandler
{
    public Image hexImage; // The background hex
    public Image typeIcon; // The icon on top
    
    [Header("Colors")]
    public Color lockedColor = Color.gray;
    public Color availableColor = Color.white;
    public Color completedColor = Color.green;
    public Color bossColor = Color.red;
    public Color highlightColor = Color.yellow;
    public Color currentNodeColor = Color.cyan; // New color for current node

    private MapUI mapUI;
    public MapNode node;

    private void Awake()
    {
        // Auto-assign references if missing
        if (hexImage == null) hexImage = GetComponent<Image>();
        
        if (typeIcon == null)
        {
            var iconObj = transform.Find("TypeIcon");
            if (iconObj != null) typeIcon = iconObj.GetComponent<Image>();
        }
    }

    public void Setup(MapNode mapNode, MapUI ui)
    {
        node = mapNode;
        mapUI = ui;
        UpdateVisuals();
    }

    public void SetIcon(Sprite sprite)
    {
        if (typeIcon != null)
        {
            typeIcon.sprite = sprite;
            typeIcon.gameObject.SetActive(sprite != null);
        }
    }

    public void UpdateVisuals()
    {
        if (node == null) return;
        
        // Base color logic
        Color color = node.isLocked ? lockedColor : availableColor;
        
        // Highlight available nodes (Next Steps)
        if (node.isAvailable && !node.isLocked && !node.isCompleted)
        {
            color = highlightColor;
        }
        
        if (node.isCompleted) color = completedColor;
        
        // Current Node Highlight (Overrides Completed)
        if (MapManager.Instance != null && MapManager.Instance.currentNode == node)
        {
            color = currentNodeColor;
        }

        if (node.nodeType == NodeType.Boss && !node.isCompleted && !node.isLocked) color = bossColor;
        
        if (hexImage != null) hexImage.color = color;
        
        // Icon color logic
        if (typeIcon != null)
        {
            typeIcon.color = node.isLocked ? new Color(1,1,1,0.5f) : Color.white;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (node.isAvailable && !node.isLocked)
        {
            mapUI.OnNodeClicked(node);
        }
    }
}
