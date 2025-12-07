using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapNodeTooltip : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nodeTypeText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;
    public RectTransform rectTransform;

    public void SetInfo(MapNode node)
    {
        if (node == null) return;

        // Node Type
        if (nodeTypeText != null)
        {
            nodeTypeText.text = GetNodeTypeName(node.nodeType);
            nodeTypeText.color = GetNodeTypeColor(node.nodeType);
        }

        // Description
        if (descriptionText != null)
        {
            descriptionText.text = GetNodeDescription(node.nodeType);
        }

        // Icon (optional)
        if (iconImage != null)
        {
            iconImage.sprite = GetNodeIcon(node.nodeType);
        }
    }

    string GetNodeTypeName(NodeType type)
    {
        return type switch
        {
            NodeType.Combat => "Combat",
            NodeType.Elite => "Elite Enemy",
            NodeType.Shop => "Shop",
            NodeType.Campfire => "Campfire",
            NodeType.Event => "Random Event",
            NodeType.Boss => "BOSS",
            NodeType.Reward => "Reward",
            _ => "Unknown"
        };
    }

    string GetNodeDescription(NodeType type)
    {
        return type switch
        {
            NodeType.Combat => "Fight normal enemies.\nReward: Dice or Perk",
            NodeType.Elite => "Fight a powerful elite enemy.\nReward: Relic",
            NodeType.Shop => "Purchase items, dice, and upgrades.\nCost: Gold",
            NodeType.Campfire => "Rest and heal.\nRestore HP or upgrade dice.",
            NodeType.Event => "Encounter a random event.\nRisk and reward!",
            NodeType.Boss => "Face a mighty boss!\nReward: Skill Point",
            NodeType.Reward => "Claim your rewards!",
            _ => "A mysterious node..."
        };
    }

    Color GetNodeTypeColor(NodeType type)
    {
        return type switch
        {
            NodeType.Combat => Color.white,
            NodeType.Elite => new Color(1f, 0.84f, 0f), // Gold
            NodeType.Shop => new Color(0.4f, 0.8f, 1f), // Light Blue
            NodeType.Campfire => new Color(1f, 0.6f, 0.2f), // Orange
            NodeType.Event => new Color(0.8f, 0.4f, 1f), // Purple
            NodeType.Boss => new Color(1f, 0.2f, 0.2f), // Red
            NodeType.Reward => Color.green,
            _ => Color.gray
        };
    }

    Sprite GetNodeIcon(NodeType type)
    {
        // TODO: Load actual icons from resources
        // For now, return null and let the caller handle it
        return null;
    }
}
