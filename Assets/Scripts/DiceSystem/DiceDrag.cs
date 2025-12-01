using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DiceDrag : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private bool isOverTrashZone = false;
    private Transform parentCell;
    public SpriteRenderer spriteRenderer;
    private Color originalColor;
    private DiceDrag currentHighlightedDice;
    private int originalSortingOrder;
    public Vector3 originalScale;

    private Dice diceScript;

    public GameObject dropEffectPrefab;
    public GameObject mergeEffectPrefab;

    private Vector3 highlightedScale = new Vector3(0.85f, 0.85f, 1f); // slightly bigger

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        diceScript = GetComponent<Dice>();
        originalScale = transform.localScale;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }

    public void SetParentCell(Transform cell)
    {
        parentCell = cell;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.IsCombatActive || GameManager.Instance.IsRewardPhaseActive || GameManager.Instance.IsMapActive) return;
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
        if (spriteRenderer != null) spriteRenderer.sortingOrder = 10;
        transform.DOScale(originalScale * 0.85f, 0.15f).SetEase(Ease.OutBack);
        if (diceScript != null) diceScript.PlayVFX(VFXType.Drag);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        transform.position = GetMouseWorldPosition() + offset;

        // Highlight nearby swappable dice
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.4f);
        DiceDrag nearestDice = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Dice") && hit.gameObject != this.gameObject)
            {
                nearestDice = hit.GetComponent<DiceDrag>();
                break;
            }
        }

        if (nearestDice != null)
        {
            if (nearestDice != currentHighlightedDice)
            {
                ClearHighlight();
                HighlightDice(nearestDice);
            }
            
            // Always update tooltip based on merge status
            if (CanMergeWith(nearestDice))
            {
                int nextLevel = (nearestDice.diceScript?.runtimeStats?.upgradeLevel ?? 1) + 1;
                if (TooltipManager.Instance != null && nearestDice.diceScript != null)
                {
                    TooltipManager.Instance.ShowMergePreview(
                        nearestDice.diceScript, 
                        nextLevel, 
                        nearestDice.transform.position
                    );
                }
            }
            else
            {
                // Hide tooltip if dice is not mergeable
                if (TooltipManager.Instance != null)
                {
                    TooltipManager.Instance.HideTooltip();
                }
            }
        }
        else
        {
            ClearHighlight();
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideTooltip();
            }
        }
    }

    private bool CanMergeWith(DiceDrag otherDice)
    {
        if (diceScript == null || otherDice == null || otherDice.diceScript == null) return false;
        
        int myLevel = diceScript?.runtimeStats?.upgradeLevel ?? 0;
        int otherLevel = otherDice.diceScript?.runtimeStats?.upgradeLevel ?? 0;
        int otherMax = otherDice.diceScript?.diceData?.maxUpgradeLevel ?? int.MaxValue;

        return diceScript.diceData == otherDice.diceScript.diceData &&
               myLevel == otherLevel &&
               otherLevel < otherMax;
    }

    void HighlightDice(DiceDrag dice)
    {
        currentHighlightedDice = dice;
        // Scale UP to indicate selection
        dice.transform.DOScale(dice.originalScale * 1.15f, 0.15f).SetEase(Ease.OutBack);
    }

    void ClearHighlight()
    {
        if (currentHighlightedDice != null)
        {
            // Reset scale
            currentHighlightedDice.transform.DOScale(currentHighlightedDice.originalScale, 0.15f).SetEase(Ease.OutBack);
        }
        currentHighlightedDice = null;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        ClearHighlight();
        
        // Hide merge preview tooltip
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
        
        isDragging = false;
        if (spriteRenderer != null) spriteRenderer.sortingOrder = originalSortingOrder;

        DiceSpawner spawner = FindFirstObjectByType<DiceSpawner>();

        // Handle dropping into Inventory or Trash
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // Raycast to find what UI element we are over
            UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<TrashUI>() != null)
                {
                    SellDice(spawner);
                    return;
                }
            }

            // If not trash, assume Inventory
            if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.AddDice(diceScript.runtimeStats))
                {
                    if (spawner != null && parentCell != null)
                        spawner.ReleaseCell(parentCell);
                    
                    // VFX?
                    if (diceScript != null) diceScript.NotifyRemoval(originalPosition); // 🔄 Notify removal at OLD position
                    DOTween.Kill(transform);
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    Debug.Log("Inventory Full!");
                    // Bounce back?
                }
            }
        }

        float dragDistance = Vector3.Distance(originalPosition, transform.position);
        bool wasActuallyDragged = dragDistance > 0.2f;

        if (wasActuallyDragged)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.4f);
            bool mergeOrSwapOccurred = false;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Dice") && hit.gameObject != this.gameObject)
                {
                    DiceDrag otherDice = hit.GetComponent<DiceDrag>();
                    if (otherDice != null && otherDice != this)
                    {
                        int myLevel = diceScript?.runtimeStats?.upgradeLevel ?? 0;
                        int otherLevel = otherDice.diceScript?.runtimeStats?.upgradeLevel ?? 0;
                        int otherMax = otherDice.diceScript?.diceData?.maxUpgradeLevel ?? int.MaxValue;

                        // ✅ Merge if same type & level, and not maxed
                        if (diceScript != null && otherDice.diceScript != null &&
                            diceScript.diceData == otherDice.diceScript.diceData &&
                            myLevel == otherLevel &&
                            otherLevel < otherMax)
                        {
                            // Increase upgrade level
                            otherDice.diceScript.runtimeStats.upgradeLevel++;

                            // Update sprite if available
                            int newLevel = otherDice.diceScript.runtimeStats.upgradeLevel;
                            int spriteIndex = newLevel - 1; // Convert to 0-based index
                            if (spriteIndex >= 0 && spriteIndex < otherDice.diceScript.diceData.upgradeSprites.Length)
                                otherDice.spriteRenderer.sprite = otherDice.diceScript.diceData.upgradeSprites[spriteIndex];

                            // Trigger merge passives
                            diceScript.diceData.passive?.OnDiceMerged(diceScript, otherDice.diceScript);
                            otherDice.diceScript.diceData.passive?.OnDiceMerged(diceScript, otherDice.diceScript);

                            // Raise global merge event
                            GameEvents.RaiseDiceMerged(diceScript, otherDice.diceScript);

                            // Release parent cell
                            if (spawner != null && parentCell != null)
                                spawner.ReleaseCell(parentCell);

                            // Spawn merge visual effect
                            diceScript.PlayVFX(VFXType.Merge);
                            otherDice.diceScript.PlayVFX(VFXType.Merge);

                            if (diceScript != null) diceScript.NotifyRemoval(originalPosition); // 🔄 Notify removal at OLD position
                            DOTween.Kill(transform);
                            Destroy(gameObject);
                            mergeOrSwapOccurred = true;
                            break;
                        }
                        else
                        {
                            // Swap positions
                            Transform otherCell = otherDice.parentCell;
                            Transform thisCell = this.parentCell;

                            Vector3 tempPos = otherDice.transform.position;
                            Vector3 myOldPos = originalPosition; // The position I started dragging from
                            Vector3 otherOldPos = otherDice.transform.position; // The position other dice was at

                            // Actually, originalPosition is where I started.
                            // But if I am swapping, I am at mouse pos.
                            // I want to swap my START position with other dice's CURRENT position.
                            
                            // Wait, logic in DiceDrag:
                            // otherDice.transform.position = originalPosition;
                            // this.transform.position = tempPos;
                            
                            // So:
                            // Me: originalPosition -> tempPos (other's pos)
                            // Other: tempPos -> originalPosition
                            
                            otherDice.transform.position = originalPosition;
                            this.transform.position = tempPos;

                            otherDice.SetOriginalPosition(originalPosition);
                            this.SetOriginalPosition(tempPos);

                            otherDice.SetParentCell(thisCell);
                            this.SetParentCell(otherCell);

                            if (spawner != null)
                            {
                                spawner.ReleaseCell(thisCell);
                                spawner.ReleaseCell(otherCell);
                                spawner.OccupyCell(otherCell);
                                spawner.OccupyCell(thisCell);
                            }

                            DOTween.Kill(this.transform);
                            DOTween.Kill(otherDice.transform);

                            this.transform.DOScale(originalScale, 0.1f).SetEase(Ease.OutBack);
                            otherDice.transform.DOScale(otherDice.originalScale, 0.1f).SetEase(Ease.OutBack);

                            // Drop effect
                            diceScript.PlayVFX(VFXType.Drop);

                            // 🔄 Notify Dice of Move
                            diceScript.OnMove(myOldPos);
                            otherDice.diceScript.OnMove(otherOldPos);

                            mergeOrSwapOccurred = true;
                            break;
                        }
                    }
                }
            }

            if (mergeOrSwapOccurred) return;

            // Drop into nearest free cell
            if (spawner != null)
            {
                Transform nearestCell = spawner.GetNearestFreeCell(transform.position);
                if (nearestCell != null && !spawner.IsCellOccupied(nearestCell))
                {
                    spawner.ReleaseCell(parentCell);
                    spawner.OccupyCell(nearestCell);

                    Vector3 oldPos = originalPosition;
                    transform.position = nearestCell.position;
                    SetOriginalPosition(nearestCell.position);
                    SetParentCell(nearestCell);

                    DOTween.Kill(transform);
                    transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);

                    if (diceScript != null) 
                    {
                        diceScript.PlayVFX(VFXType.Drop);
                        diceScript.OnMove(oldPos); // 🔄 Notify Move
                    }
                    return;
                }
            }
        }

        // Snap back if nothing valid happened
        DOTween.Kill(transform);
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);
        transform.position = originalPosition;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 10f; // camera distance
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    public void SetTrashZoneStatus(bool isInside) => isOverTrashZone = isInside;
    public void SetOriginalPosition(Vector3 pos) => originalPosition = pos;

    private void SellDice(DiceSpawner spawner)
    {
        if (spawner != null && parentCell != null)
            spawner.ReleaseCell(parentCell);

        if (diceScript != null && diceScript.diceData != null)
        {
            int refund = diceScript.diceData.cost;
            PlayerCurrency.Instance.AddGold(refund);

            if (diceScript.floatingTextPrefab_Normal != null)
            {
                GameObject text = Instantiate(diceScript.floatingTextPrefab_Normal, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                FloatingText floating = text.GetComponent<FloatingText>();
                if (floating != null)
                {
                    floating.ShowGold(refund);
                }
            }

            diceScript.PlayVFX(VFXType.Sold);
        }

        if (diceScript != null) 
        {
            diceScript.NotifyRemoval(originalPosition);
        }
        DOTween.Kill(transform);
        Destroy(gameObject);
    }
}
