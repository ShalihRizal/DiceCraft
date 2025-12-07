using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPrefab;
    private DiceTooltip tooltipInstance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (tooltipInstance != null && tooltipInstance.gameObject.activeSelf)
        {
            SetTooltipPosition(Vector3.zero); // Argument is ignored now
        }
    }

    public void ShowTooltip(Dice dice, Vector3 worldPosition)
    {
        EnsureTooltipInstance();
        if (tooltipInstance == null) return;

        SetTooltipPosition(worldPosition);
        tooltipInstance.SetInfo(dice);
        tooltipInstance.gameObject.SetActive(true);
    }

    public void ShowTooltip(DiceData data, Vector3 worldPosition)
    {
        EnsureTooltipInstance();
        if (tooltipInstance == null) return;

        SetTooltipPosition(worldPosition);
        tooltipInstance.SetInfo(data);
        tooltipInstance.gameObject.SetActive(true);
    }

    public void ShowTooltip(RelicData relic, Vector3 worldPosition)
    {
        EnsureTooltipInstance();
        if (tooltipInstance == null) return;

        SetTooltipPosition(worldPosition);
        tooltipInstance.SetInfo(relic);
        
        // Ensure the tooltip text is updated with the dynamic description
        if (tooltipInstance.passiveDescText != null)
        {
            tooltipInstance.passiveDescText.text = relic.GetDescription();
            tooltipInstance.passiveDescText.gameObject.SetActive(true);
        }
        
        tooltipInstance.gameObject.SetActive(true);
    }

    public void ShowTooltip(Enemy enemy, Vector3 worldPosition)
    {
        if (enemy == null || enemy.enemyData == null) return;

        EnsureTooltipInstance();
        if (tooltipInstance == null) return;

        SetTooltipPosition(worldPosition);
        
        EnemyData data = enemy.enemyData;

        // Show enemy name with type color and level
        if (tooltipInstance.nameText != null)
        {
            string typeBadge = data.enemyType switch
            {
                EnemyType.Elite => "[ELITE]",
                EnemyType.Boss => "[BOSS]",
                _ => ""
            };
            
            string levelText = "";
            if (DifficultyManager.Instance != null)
            {
                levelText = $" {DifficultyManager.Instance.GetLevelDisplay()}";
            }
            
            tooltipInstance.nameText.text = $"{data.enemyName} {typeBadge}{levelText}";
            tooltipInstance.nameText.color = GetEnemyTypeColor(data.enemyType);
        }

        // Show HP (current, not base)
        if (tooltipInstance.fireRateText != null)
        {
            float maxHP = enemy.enemyData.maxHealth;
            if (DifficultyManager.Instance != null)
            {
                maxHP = DifficultyManager.Instance.GetScaledHP(maxHP, data.enemyType);
            }
            
            tooltipInstance.fireRateText.text = $"HP: {Mathf.CeilToInt(enemy.health)} / {Mathf.CeilToInt(maxHP)}";
            tooltipInstance.fireRateText.gameObject.SetActive(true);
        }

        // Show Damage (actual scaled value)
        if (tooltipInstance.damageText != null)
        {
            // Use the enemy's actual damage (already scaled)
            float damage = enemy.projectileDamage;
            
            tooltipInstance.damageText.text = $"Damage: {Mathf.RoundToInt(damage)}";
            tooltipInstance.damageText.color = Color.red;
            tooltipInstance.damageText.gameObject.SetActive(true);
        }

        // Show Attack Speed
        if (tooltipInstance.sidesText != null)
        {
            float interval = data.attackInterval;
            if (DifficultyManager.Instance != null)
            {
                interval = DifficultyManager.Instance.GetScaledAttackInterval(interval);
            }
            
            tooltipInstance.sidesText.text = $"Attack Speed: {interval:F2}s";
            tooltipInstance.sidesText.gameObject.SetActive(true);
        }

        // Show Traits
        if (tooltipInstance.passiveNameText != null && tooltipInstance.passiveDescText != null)
        {
            if (data.traits != null && data.traits.Count > 0)
            {
                tooltipInstance.passiveNameText.text = "Traits:";
                tooltipInstance.passiveNameText.gameObject.SetActive(true);

                string traitList = "";
                foreach (var trait in data.traits)
                {
                    if (trait != null)
                    {
                        traitList += $"• {trait.traitName}: {trait.GetDescription()}\n";
                    }
                }
                
                tooltipInstance.passiveDescText.text = traitList.TrimEnd('\n');
                tooltipInstance.passiveDescText.gameObject.SetActive(true);
            }
            else
            {
                tooltipInstance.passiveNameText.gameObject.SetActive(false);
                tooltipInstance.passiveDescText.gameObject.SetActive(false);
            }
        }

        tooltipInstance.gameObject.SetActive(true);
    }
    
    public void ShowTooltip(EnemyTrait trait, Vector3 worldPosition)
    {
        EnsureTooltipInstance();
        if (tooltipInstance == null) return;

        SetTooltipPosition(worldPosition);
        
        // Clear previous info
        if (tooltipInstance.nameText != null) 
        {
            tooltipInstance.nameText.text = trait.traitName;
            tooltipInstance.nameText.color = Color.white;
        }
        
        // Hide stats
        if (tooltipInstance.fireRateText != null) tooltipInstance.fireRateText.gameObject.SetActive(false);
        if (tooltipInstance.damageText != null) tooltipInstance.damageText.gameObject.SetActive(false);
        if (tooltipInstance.sidesText != null) tooltipInstance.sidesText.gameObject.SetActive(false);
        if (tooltipInstance.passiveNameText != null) tooltipInstance.passiveNameText.gameObject.SetActive(false);
        
        if (tooltipInstance.passiveDescText != null)
        {
            tooltipInstance.passiveDescText.text = trait.GetDescription();
            tooltipInstance.passiveDescText.gameObject.SetActive(true);
        }
        
        tooltipInstance.gameObject.SetActive(true);
    }

    public void ShowTooltip(LootDrop loot, Vector3 worldPosition)
    {
        EnsureTooltipInstance();
        if (tooltipInstance == null) return;

        SetTooltipPosition(worldPosition);

        // Reset text
        if (tooltipInstance.nameText != null)
        {
            tooltipInstance.nameText.text = $"{loot.type}";
            tooltipInstance.nameText.color = Color.yellow; // Default loot color
        }

        // Hide stats
        if (tooltipInstance.fireRateText != null) tooltipInstance.fireRateText.gameObject.SetActive(false);
        if (tooltipInstance.damageText != null) tooltipInstance.damageText.gameObject.SetActive(false);
        if (tooltipInstance.sidesText != null) tooltipInstance.sidesText.gameObject.SetActive(false);
        if (tooltipInstance.passiveNameText != null) tooltipInstance.passiveNameText.gameObject.SetActive(false);

        // Description: Show Amount
        if (tooltipInstance.passiveDescText != null)
        {
            tooltipInstance.passiveDescText.text = $"Amount: {loot.amount}";
            if (loot.type == LootType.Relic)
                tooltipInstance.passiveDescText.text = $"Rarity: {loot.relicRarity}";
            
            tooltipInstance.passiveDescText.gameObject.SetActive(true);
        }

        if (tooltipInstance.GetComponent<CanvasGroup>() != null)
            tooltipInstance.GetComponent<CanvasGroup>().blocksRaycasts = false;

        tooltipInstance.gameObject.SetActive(true);
    }

    Color GetEnemyTypeColor(EnemyType type)
    {
        return type switch
        {
            EnemyType.Normal => Color.white,
            EnemyType.Elite => new Color(1f, 0.84f, 0f), // Gold
            EnemyType.Boss => new Color(1f, 0.2f, 0.2f), // Red
            _ => Color.white
        };
    }

    public void ShowTooltip(MapNode node, Vector3 worldPosition)
    {
        // For map nodes, show node type and description
        EnsureTooltipInstance();
        if (tooltipInstance == null) return;

        // Use DiceTooltip's name/description fields for now
        // Or create a separate MapNodeTooltip instance
        SetTooltipPosition(worldPosition);
        
        // Temporary: Show basic info using dice tooltip
        if (tooltipInstance.nameText != null)
        {
            tooltipInstance.nameText.text = GetNodeTypeName(node.nodeType);
        }
        
        if (tooltipInstance.passiveDescText != null)
        {
            tooltipInstance.passiveDescText.text = GetNodeDescription(node.nodeType);
            tooltipInstance.passiveDescText.gameObject.SetActive(true);
        }
        
        if (tooltipInstance.passiveNameText != null)
        {
            tooltipInstance.passiveNameText.gameObject.SetActive(false);
        }
        
        // Hide dice-specific stats
        if (tooltipInstance.fireRateText != null) tooltipInstance.fireRateText.gameObject.SetActive(false);
        if (tooltipInstance.damageText != null) tooltipInstance.damageText.gameObject.SetActive(false);
        if (tooltipInstance.sidesText != null) tooltipInstance.sidesText.gameObject.SetActive(false);
        
        tooltipInstance.gameObject.SetActive(true);
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

    private void EnsureTooltipInstance()
    {
        if (tooltipInstance == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>()?.gameObject;
            }

            if (canvas == null)
            {
                Debug.LogWarning("⚠ Canvas not found for tooltip!");
                return;
            }

            if (tooltipPrefab != null)
            {
                GameObject tooltipGO = Instantiate(tooltipPrefab, canvas.transform);
                tooltipInstance = tooltipGO.GetComponent<DiceTooltip>();
                
                // ✅ Ensure Tooltip doesn't block raycasts (prevents flickering)
                CanvasGroup cg = tooltipGO.GetComponent<CanvasGroup>();
                if (cg == null) cg = tooltipGO.AddComponent<CanvasGroup>();
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
            else
            {
                Debug.LogError("❌ Tooltip Prefab is missing in TooltipManager!");
            }
        }
    }

    public Vector2 tooltipOffset = new Vector2(50, 50);

    private void SetTooltipPosition(Vector3 worldPosition)
    {
        Canvas canvas = tooltipInstance.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Vector3 mousePos = Input.mousePosition;
        
        // 🧠 Smart Pivot: Flip offset based on screen position
        Vector2 pivot = new Vector2(0, 1); // Default: Top-Left pivot (so tooltip extends Down-Right)
        Vector2 finalOffset = tooltipOffset;

        // If on right side of screen, flip to Left
        if (mousePos.x > Screen.width * 0.7f)
        {
            finalOffset.x = -tooltipOffset.x;
            pivot.x = 1; // Pivot Top-Right (extends Down-Left)
        }

        // If on bottom side of screen, flip to Up
        if (mousePos.y < Screen.height * 0.3f)
        {
            finalOffset.y = -tooltipOffset.y; // Wait, if pivot is Bottom, we want it to extend Up.
            // Let's just adjust pivot and offset direction.
            // Standard: Pivot (0, 1) -> Top Left. Tooltip body is below and right of pivot.
            // We want tooltip to be offset from mouse.
            
            // Actually, simpler approach:
            // Just move the position.
            // If right side, move left.
            // If bottom side, move up.
        }
        
        // Let's stick to modifying position and pivot.
        RectTransform rect = tooltipInstance.rectTransform;
        
        // Determine Pivot based on quadrant
        // Top-Left Quadrant -> Pivot (0, 1) [Top-Left] -> Tooltip goes Right-Down
        // Top-Right Quadrant -> Pivot (1, 1) [Top-Right] -> Tooltip goes Left-Down
        // Bottom-Left Quadrant -> Pivot (0, 0) [Bottom-Left] -> Tooltip goes Right-Up
        // Bottom-Right Quadrant -> Pivot (1, 0) [Bottom-Right] -> Tooltip goes Left-Up
        
        float pivotX = (mousePos.x > Screen.width / 2) ? 1 : 0;
        float pivotY = (mousePos.y < Screen.height / 2) ? 0 : 1;
        
        rect.pivot = new Vector2(pivotX, pivotY);
        
        // Adjust offset direction based on pivot
        // If Pivot X is 1 (Right), we want offset to be negative (Left)
        // If Pivot Y is 0 (Bottom), we want offset to be positive (Up)
        
        float offsetX = (pivotX == 1) ? -tooltipOffset.x : tooltipOffset.x;
        float offsetY = (pivotY == 0) ? tooltipOffset.y : -tooltipOffset.y;
        
        Vector3 finalPos = mousePos + new Vector3(offsetX, offsetY, 0);

        // Convert to Local
        RectTransform parentRect = rect.parent as RectTransform;
        Camera uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        if (uiCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) uiCamera = Camera.main;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, finalPos, uiCamera, out localPoint))
        {
            tooltipInstance.transform.localPosition = localPoint;
            tooltipInstance.transform.SetAsLastSibling();
            
            // Clamp is less needed with smart pivot, but still good for edges
            ClampToParent(rect, parentRect);
        }
    }

    private void ClampToParent(RectTransform tooltip, RectTransform parent)
    {
        Vector3 pos = tooltip.localPosition;
        
        float tooltipWidth = tooltip.rect.width * tooltip.lossyScale.x; // Approximate
        float tooltipHeight = tooltip.rect.height * tooltip.lossyScale.y;

        // Better: Use local bounds
        Vector3 minPosition = parent.rect.min - tooltip.rect.min;
        Vector3 maxPosition = parent.rect.max - tooltip.rect.max;

        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);

        // Simple screen clamp fallback if parent is screen-sized
        // (This is a simplification, robust clamping is complex with pivots/anchors)
        // For now, let's just ensure it doesn't go off screen if parent is the canvas.
        
        tooltip.localPosition = pos;
    }

    public void HideTooltip()
    {
        if (tooltipInstance != null)
            tooltipInstance.gameObject.SetActive(false);
    }
}
