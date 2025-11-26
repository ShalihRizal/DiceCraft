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
            DiceTooltipManager.Instance.ShowTooltip(currentDice.baseData, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DiceTooltipManager.Instance != null)
            DiceTooltipManager.Instance.HideTooltip();
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
                    
                    // Remove source
                    InventoryManager.Instance.RemoveDiceAt(sourceSlot.slotIndex);
                    
                    Debug.Log("✅ Merged inside Inventory!");
                    return;
                }
            }
            return;
        }

        // Case 2: Dropped from Board (DiceDrag)
        DiceDrag diceDrag = droppedObj.GetComponent<DiceDrag>();
        if (diceDrag != null)
        {
            // Logic to move dice from board to inventory
            // This requires DiceDrag to know about this slot or handle it here
            // For now, let's just log
            Debug.Log($"Dropped dice on slot {slotIndex}");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentDice == null) return;
        if (GameManager.Instance != null && (GameManager.Instance.IsCombatActive || GameManager.Instance.IsRewardPhaseActive)) return;

        DiceTooltipManager.Instance.HideTooltip();

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
            Debug.LogWarning("⚠️ Cannot place dice during combat or reward phase!");
            // Optional: Show floating text or shake effect
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
                
                // If GetNearestFreeCell returns null, it might be because all cells are occupied or too far.
                // We need to check distance to ANY cell, not just free ones, to support merging.
                // So let's find the nearest cell regardless of occupation.
                
                Transform bestCell = null;
                float minDist = float.MaxValue;
                // We need access to grid cells. DiceSpawner doesn't expose them publicly as a list, but we can iterate children of GridGenerator?
                // Or we can rely on GetNearestFreeCell logic but modified.
                // Since we can't easily modify DiceSpawner right now without reading it again (I did read it, it has private gridCells),
                // I will use Physics2D to find the cell.
                
                Collider2D hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null)
                {
                    // Assuming cells have colliders or dice have colliders.
                    // If we hit a dice, we get its parent cell.
                    // If we hit a cell, we get the cell.
                    
                    Transform targetCell = null;
                    if (hit.CompareTag("Dice"))
                    {
                        targetCell = hit.transform.parent;
                    }
                    else 
                    {
                        // Check if it's a cell (maybe by name or component?)
                        // DiceSpawner creates cells. Let's assume they are the parents of dice or just empty objects.
                        // Let's assume the grid cells have colliders?
                        // If not, we might need to rely on distance check to all children of GridSpawner.
                        if (spawner.gridGenerator != null)
                        {
                             foreach(Transform cell in spawner.gridGenerator.transform)
                             {
                                 float dist = Vector3.Distance(worldPos, cell.position);
                                 if (dist < 1.0f && dist < minDist)
                                 {
                                     minDist = dist;
                                     bestCell = cell;
                                 }
                             }
                        }
                    }
                    
                    if (bestCell != null) targetCell = bestCell;

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

                                    // Remove from Inventory
                                    InventoryManager.Instance.RemoveDiceAt(slotIndex);
                                    
                                    Debug.Log("✅ Merged from Inventory!");
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Fallback to distance check if Physics fail (e.g. no colliders on cells)
                    if (spawner.gridGenerator != null)
                    {
                         foreach(Transform cell in spawner.gridGenerator.transform)
                         {
                             float dist = Vector3.Distance(worldPos, cell.position);
                             if (dist < 1.0f && dist < minDist)
                             {
                                 minDist = dist;
                                 bestCell = cell;
                             }
                         }
                    }

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
