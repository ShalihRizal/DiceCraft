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
        if (GameManager.Instance.IsCombatActive) return;
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

        if (nearestDice != null && nearestDice != currentHighlightedDice)
        {
            ClearHighlight();
            HighlightDice(nearestDice);
        }
        else if (nearestDice == null)
        {
            ClearHighlight();
        }
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
        ClearHighlight();
        isDragging = false;
        if (spriteRenderer != null) spriteRenderer.sortingOrder = originalSortingOrder;

        DiceSpawner spawner = FindFirstObjectByType<DiceSpawner>();

        // Handle trash zone
        if (isOverTrashZone)
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

                Debug.Log($"🪙 Sold {diceScript.diceData.diceName} for {refund} gold!");
                diceScript.PlayVFX(VFXType.Sold);
            }

            DOTween.Kill(transform);
            Destroy(gameObject);
            return;
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
                            if (newLevel < otherDice.diceScript.diceData.upgradeSprites.Length)
                                otherDice.spriteRenderer.sprite = otherDice.diceScript.diceData.upgradeSprites[newLevel];

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

                    transform.position = nearestCell.position;
                    SetOriginalPosition(nearestCell.position);
                    SetParentCell(nearestCell);

                    DOTween.Kill(transform);
                    transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);

                    if (diceScript != null) diceScript.PlayVFX(VFXType.Drop);
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
}
