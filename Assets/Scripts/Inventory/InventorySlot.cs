using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int slotIndex;
    public Image icon;
    public RuntimeDiceData currentDice;

    private GameObject dragIcon;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnDestroy()
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }
    }

    public void SetDice(RuntimeDiceData dice)
    {
        currentDice = dice;
        if (dice.baseData.upgradeSprites.Length > dice.upgradeLevel)
            icon.sprite = dice.baseData.upgradeSprites[dice.upgradeLevel];
        else if (dice.baseData.upgradeSprites.Length > 0)
            icon.sprite = dice.baseData.upgradeSprites[0]; // Fallback
        else
            icon.sprite = null;
        
        icon.enabled = true;
        icon.raycastTarget = true; // ✅ Ensure raycast target for tooltips
    }

    public void ClearSlot()
    {
        currentDice = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentDice != null && dragIcon == null) // Don't show tooltip if dragging
        {
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.ShowTooltip(currentDice.baseData, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip();
    }

    void OnDisable()
    {
        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Handle dropping dice from board to inventory OR inventory to inventory
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        // Case 1: Dropped from another Inventory Slot
        InventorySlot sourceSlot = droppedObj.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot != this)
        {
            // Merge Logic
            if (currentDice != null && sourceSlot.currentDice != null)
            {
                if (currentDice.baseData == sourceSlot.currentDice.baseData && 
                    currentDice.upgradeLevel == sourceSlot.currentDice.upgradeLevel)
                {
                    // Upgrade THIS slot
                    currentDice.upgradeLevel++;
                    SetDice(currentDice); // Refresh UI
                    
                    // 🪞 Increment Duplicator counter for inventory-to-inventory merge
                    if (RelicManager.Instance != null)
                    {
                        RelicManager.Instance.IncrementDuplicatorCounter();
                    }
                    
                    // Remove source
                    InventoryManager.Instance.RemoveDiceAt(sourceSlot.slotIndex);

                    return;
                }
            }
            return;
        }

        // Case 2: Dropped from Board (DiceDrag)
        DiceDrag diceDrag = droppedObj.GetComponent<DiceDrag>();
        if (diceDrag != null && diceDrag.diceScript != null)
        {
            // Check if we can merge with current dice in this slot
            if (currentDice != null && diceDrag.diceScript.diceData == currentDice.baseData && 
                diceDrag.diceScript.runtimeStats.upgradeLevel == currentDice.upgradeLevel)
            {
                // Merge: Upgrade this inventory slot
                currentDice.upgradeLevel++;
                SetDice(currentDice); // Refresh UI
                
                // 🪞 Increment Duplicator counter for board-to-inventory merge
                if (RelicManager.Instance != null)
                {
                    RelicManager.Instance.IncrementDuplicatorCounter();
                }
                
                // Remove dice from board
                DiceSpawner spawner = FindFirstObjectByType<DiceSpawner>();
                if (spawner != null && diceDrag.parentCell != null)
                {
                    spawner.ReleaseCell(diceDrag.parentCell);
                }
                
                // Destroy the board dice
                if (diceDrag.diceScript != null)
                {
                    diceDrag.diceScript.NotifyRemoval(diceDrag.transform.position);
                }
                Destroy(diceDrag.gameObject);
                
                Debug.Log("✅ Merged board dice into inventory!");
                return;
            }
            
            // If no merge, just move to inventory (existing logic would handle this in DiceDrag)
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentDice == null) return;
        if (GameManager.Instance != null && (GameManager.Instance.IsCombatActive || GameManager.Instance.IsRewardPhaseActive)) return;

        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip();

        // Create drag icon
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.transform.SetAsLastSibling();
        
        Image img = dragIcon.AddComponent<Image>();
        img.sprite = icon.sprite;
        img.raycastTarget = false;
        
        RectTransform rect = dragIcon.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100); // Fixed size for now

        if (canvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out localPoint);
            dragIcon.transform.localPosition = localPoint;
        }
        else
        {
            dragIcon.transform.position = icon.transform.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null && canvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out localPoint);
            dragIcon.transform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
        }

        if (currentDice == null) return;

        // 🛑 Restrict placement during combat or reward phase
        if (GameManager.Instance != null && (GameManager.Instance.IsCombatActive || GameManager.Instance.IsRewardPhaseActive))
        {

            // Optional: Show floating text or shake effect
            return;
        }

        // 🗑️ Check if dropped on Trash Zone
        if (eventData.hovered.Exists(g => g.GetComponent<TrashUI>() != null))
        {
            // Sell the dice for its actual cost
            if (PlayerCurrency.Instance != null && currentDice != null && currentDice.baseData != null)
            {
                int sellValue = currentDice.baseData.cost;
                PlayerCurrency.Instance.AddGold(sellValue);
                Debug.Log($"💰 Sold {currentDice.baseData.diceName} for {sellValue} gold!");
            }
            
            InventoryManager.Instance.RemoveDiceAt(slotIndex);
            return;
        }

        // Check if dropped on world
        if (!eventData.hovered.Exists(g => g.GetComponent<InventorySlot>() != null)) // If not hovering over another slot
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            DiceSpawner spawner = FindFirstObjectByType<DiceSpawner>();
            if (spawner != null)
            {
                Transform nearestCell = spawner.GetNearestFreeCell(worldPos);
                
                // Use Physics2D to find the cell or dice at the drop position
                Collider2D hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    Transform targetCell = null;
                    if (hit.CompareTag("Dice"))
                    {
                        targetCell = hit.transform.parent;
                    }
                    else 
                    {
                        // Use DiceSpawner's GetNearestCell method
                        targetCell = spawner.GetNearestCell(worldPos, 1.0f);
                    }
                    
                    if (targetCell == null)
                    {
                        // Fallback: use GetNearestCell
                        targetCell = spawner.GetNearestCell(worldPos, 1.0f);
                    }

                    if (targetCell != null)
                    {
                        if (!spawner.IsCellOccupied(targetCell))
                        {
                            // Spawn Dice (Existing Logic)
                            // We need to spawn with SPECIFIC RuntimeDiceData
                            // DiceSpawner.SpawnDiceAt takes DiceData and creates new RuntimeDiceData.
                            // We need a method to spawn with existing RuntimeDiceData.
                            // Or we spawn and then overwrite stats.
                            
                            Dice newDice = spawner.SpawnDiceAt(currentDice.baseData, targetCell);
                            if (newDice != null)
                            {
                                newDice.runtimeStats = currentDice; // Transfer stats (level, etc.)
                                // Update sprite
                                if (newDice.diceData.upgradeSprites.Length > newDice.runtimeStats.upgradeLevel)
                                    newDice.GetComponent<SpriteRenderer>().sprite = newDice.diceData.upgradeSprites[newDice.runtimeStats.upgradeLevel];
                            }
                            
                            InventoryManager.Instance.RemoveDiceAt(slotIndex);
                        }
                        else
                        {
                            // 🧬 Merge Logic
                            Dice diceOnBoard = targetCell.GetComponentInChildren<Dice>();
                            if (diceOnBoard != null)
                            {
                                // Check if mergeable: Same Data AND Same Level
                                if (diceOnBoard.diceData == currentDice.baseData && diceOnBoard.runtimeStats.upgradeLevel == currentDice.upgradeLevel)
                                {
                                    // Perform Merge
                                    diceOnBoard.runtimeStats.upgradeLevel++;
                                    
                                    // Update Sprite
                                    if (diceOnBoard.diceData.upgradeSprites.Length > diceOnBoard.runtimeStats.upgradeLevel)
                                    {
                                        diceOnBoard.GetComponent<SpriteRenderer>().sprite = diceOnBoard.diceData.upgradeSprites[diceOnBoard.runtimeStats.upgradeLevel];
                                    }

                                    // VFX & Events
                                    diceOnBoard.PlayVFX(VFXType.Merge);
                                    GameEvents.RaiseDiceMerged(null, diceOnBoard); // Owner is null (from inventory)
                                    
                                    // 🪞 Increment Duplicator counter
                                    if (RelicManager.Instance != null)
                                    {
                                        RelicManager.Instance.IncrementDuplicatorCounter();
                                    }

                                    // Remove from Inventory
                                    InventoryManager.Instance.RemoveDiceAt(slotIndex);

                                }
                            }
                        }
                    }
                }
                else
                {
                    // Fallback to distance check if Physics fail (e.g. no colliders on cells)
                    Transform bestCell = spawner.GetNearestCell(worldPos, 1.0f);

                    if (bestCell != null)
                    {
                         if (!spawner.IsCellOccupied(bestCell))
                        {
                            Dice newDice = spawner.SpawnDiceAt(currentDice.baseData, bestCell);
                            if (newDice != null)
                            {
                                newDice.runtimeStats = currentDice;
                                if (newDice.diceData.upgradeSprites.Length > newDice.runtimeStats.upgradeLevel)
                                    newDice.GetComponent<SpriteRenderer>().sprite = newDice.diceData.upgradeSprites[newDice.runtimeStats.upgradeLevel];
                            }
                            InventoryManager.Instance.RemoveDiceAt(slotIndex);
                        }
                        else
                        {
                            // Duplicate Merge Logic (Refactor if possible, but inline is fine for now)
                            Dice diceOnBoard = bestCell.GetComponentInChildren<Dice>();
                            if (diceOnBoard != null && diceOnBoard.diceData == currentDice.baseData && diceOnBoard.runtimeStats.upgradeLevel == currentDice.upgradeLevel)
                            {
                                diceOnBoard.runtimeStats.upgradeLevel++;
                                if (diceOnBoard.diceData.upgradeSprites.Length > diceOnBoard.runtimeStats.upgradeLevel)
                                    diceOnBoard.GetComponent<SpriteRenderer>().sprite = diceOnBoard.diceData.upgradeSprites[diceOnBoard.runtimeStats.upgradeLevel];
                                
                                diceOnBoard.PlayVFX(VFXType.Merge);
                                GameEvents.RaiseDiceMerged(null, diceOnBoard);
                                
                                // 🪞 Increment Duplicator counter
                                if (RelicManager.Instance != null)
                                {
                                    RelicManager.Instance.IncrementDuplicatorCounter();
                                }
                                
                                InventoryManager.Instance.RemoveDiceAt(slotIndex);
                                Debug.Log("✅ Merged from Inventory (Distance Check)!");
                            }
                        }
                    }
                }
            }
        }
    }
}
