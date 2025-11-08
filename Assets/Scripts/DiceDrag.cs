using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DiceDrag: MonoBehaviour {
  private Vector3 offset;
  private Vector3 originalPosition;
  private bool isDragging = false;
  private bool isOverTrashZone = false;
  private Transform parentCell;
  private SpriteRenderer spriteRenderer;
  private Color originalColor;
  private DiceDrag currentHighlightedDice;
  private int originalSortingOrder;
  private Vector3 originalScale;

  private Dice diceScript;

  public GameObject dropEffectPrefab;
  public GameObject mergeEffectPrefab;

  private Vector3 highlightedScale = new Vector3(0.85f, 0.85f, 1f); // slightly bigger

  void Start() {
    originalPosition = transform.position;
    spriteRenderer = GetComponent < SpriteRenderer > ();
    diceScript = GetComponent < Dice > ();

    originalScale = transform.localScale;

    if (spriteRenderer != null) {
      originalColor = spriteRenderer.color;
      originalSortingOrder = spriteRenderer.sortingOrder;
    }
  }

  public void SetParentCell(Transform cell) {
    parentCell = cell;
  }

  void OnMouseDown() {
    if (GameManager.Instance.IsCombatActive) return;
    offset = transform.position - GetMouseWorldPosition();
    isDragging = true;
    spriteRenderer.sortingOrder = 10;
    transform.DOScale(originalScale * 0.85f, 0.15f).SetEase(Ease.OutBack);
  }

  void OnMouseDrag() {
    if (isDragging) {
      transform.position = GetMouseWorldPosition() + offset;

      // Highlight nearby swappable dice
      Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.4f);
      DiceDrag nearestDice = null;

      foreach(var hit in hits) {
        if (hit.CompareTag("Dice") && hit.gameObject != this.gameObject) {
          nearestDice = hit.GetComponent < DiceDrag > ();
          break;
        }
      }

      // If we're over a new dice, highlight it
      if (nearestDice != null && nearestDice != currentHighlightedDice) {
        ClearHighlight(); // clear previous
        HighlightDice(nearestDice);
      } else if (nearestDice == null) {
        ClearHighlight();
      }
    }
  }

  void HighlightDice(DiceDrag dice) {
    currentHighlightedDice = dice;

    if (dice.spriteRenderer != null) {
      dice.spriteRenderer.color = Color.yellow;
    }

    dice.transform.DOScale(originalScale * 0.85f, 0.1f).SetEase(Ease.OutQuad);

  }

  void ClearHighlight() {
    if (currentHighlightedDice != null) {
      if (currentHighlightedDice.spriteRenderer != null)
        currentHighlightedDice.spriteRenderer.color = currentHighlightedDice.originalColor;

      currentHighlightedDice.transform.DOScale(currentHighlightedDice.originalScale, 0.1f).SetEase(Ease.OutQuad);
    }

    currentHighlightedDice = null;
  }

  void OnMouseUp()
{
    ClearHighlight();
    isDragging = false;
    spriteRenderer.sortingOrder = originalSortingOrder;

    DiceSpawner spawner = FindObjectOfType<DiceSpawner>();

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
                    Color gold = new Color(1f, 0.84f, 0f);
                    floating.ShowGold(refund);
                }
            }

            Debug.Log($"🪙 Sold {diceScript.diceData.name} for {refund} gold!");
        }

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
                    if (diceScript != null && otherDice.diceScript != null &&
                        diceScript.diceData == otherDice.diceScript.diceData &&
                        diceScript.runtimeStats.upgradeLevel == otherDice.diceScript.runtimeStats.upgradeLevel &&
                        otherDice.diceScript.runtimeStats.upgradeLevel < otherDice.diceScript.diceData.sides - 1)

                    {
                        otherDice.diceScript.runtimeStats.upgradeLevel++;
                        int newLevel = otherDice.diceScript.runtimeStats.upgradeLevel;
                        if (newLevel < otherDice.diceScript.diceData.upgradeSprites.Length)
                        {
                            otherDice.spriteRenderer.sprite = otherDice.diceScript.diceData.upgradeSprites[newLevel];
                        }

                        if (spawner != null && parentCell != null)
                            spawner.ReleaseCell(parentCell);
                            
                            GameEvents.RaiseDiceMerged(diceScript.diceData);
              Destroy(gameObject);
                            
                            if (mergeEffectPrefab != null)
{
    Vector3 effectPos = otherDice.parentCell != null 
        ? otherDice.parentCell.position 
        : otherDice.transform.position;

    GameObject effect = Instantiate(mergeEffectPrefab, effectPos, Quaternion.identity);
}

                        mergeOrSwapOccurred = true;
                        break;
                    }
                    else
                    {
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

                        this.transform.DOScale(originalScale, 0.1f).SetEase(Ease.OutBack);
              otherDice.transform.DOScale(otherDice.originalScale, 0.1f).SetEase(Ease.OutBack);
                        
                        if (dropEffectPrefab != null)
                {
                    Vector3 effectPos = parentCell != null ? parentCell.position : transform.position;
GameObject effect = Instantiate(dropEffectPrefab, effectPos, Quaternion.identity);

                }

                        mergeOrSwapOccurred = true;
                        break;
                    }
                }
            }
        }

        if (mergeOrSwapOccurred) return;

        // ✅ New logic: Try dropping into empty nearby cell
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

                transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);
                if (dropEffectPrefab != null)
                {
                    Vector3 effectPos = parentCell != null ? parentCell.position : transform.position;
GameObject effect = Instantiate(dropEffectPrefab, effectPos, Quaternion.identity);

                }
                return;
            }
        }
    }

    // ❌ Fallback snap-back if nothing valid happened
    transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);
    transform.position = originalPosition;
}


  Vector3 GetMouseWorldPosition() {
    Vector3 screenPos = Input.mousePosition;
    screenPos.z = 10f; // Adjust based on camera distance
    return Camera.main.ScreenToWorldPoint(screenPos);
  }

  public void SetTrashZoneStatus(bool isInside) {
    isOverTrashZone = isInside;
  }

  public void SetOriginalPosition(Vector3 pos) {
    originalPosition = pos;
  }
}