using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MapNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MapNode node;
    public Button button;
    public Image icon;
    public Image border;
    
    [Header("Colors")]
    public Color lockedColor = Color.gray;
    public Color availableColor = Color.white;
    public Color completedColor = Color.green;
    public Color bossColor = Color.red;
    public Color highlightColor = Color.yellow;

    private MapUI mapUI;

    public void Setup(MapNode mapNode, MapUI ui)
    {
        node = mapNode;
        mapUI = ui;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
        
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        transform.localScale = Vector3.one; // Reset scale
        
        if (node.isCompleted)
        {
            icon.color = completedColor;
            button.interactable = false;
        }
        else if (node.isAvailable && !node.isLocked)
        {
            icon.color = node.nodeType == NodeType.Boss ? bossColor : availableColor;
            button.interactable = true;
        }
        else
        {
            icon.color = lockedColor;
            button.interactable = false;
        }
    }

    private void OnClick()
    {
        MapManager.Instance.SelectNode(node);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (node.isAvailable && !node.isLocked && !node.isCompleted)
        {
            mapUI.HighlightPath(node);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mapUI.ResetHighlight();
    }
    
    public void SetHighlight(bool active)
    {
        if (active)
        {
            transform.localScale = Vector3.one * 1.2f; // Scale up
            icon.color = highlightColor;
        }
        else
        {
            transform.localScale = Vector3.one;
            // Color reset is handled by UpdateVisuals or MapUI calling SetDimmed(false)
        }
    }
    
    public void SetDimmed(bool dimmed)
    {
        if (dimmed)
        {
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0.3f);
        }
        else
        {
            UpdateVisuals(); // Restore original color
        }
    }
}
