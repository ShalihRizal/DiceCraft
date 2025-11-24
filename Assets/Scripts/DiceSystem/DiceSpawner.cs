using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSpawner : MonoBehaviour
{
    [Header("References")]
    public GridSpawner gridGenerator;

    private List<Transform> gridCells = new List<Transform>();
    private HashSet<Transform> occupiedCells = new HashSet<Transform>();
    public DicePool dicePool; // Reference to your ScriptableObject pool

    [Header("Starting Dice")]
    public int startWithDiceCount = 1;

    void Start()
    {
        StartCoroutine(InitializeAfterGridReady());
    }

    IEnumerator InitializeAfterGridReady()
    {
        // wait one frame so gridGenerator can populate children
        yield return null;

        if (gridGenerator == null || dicePool == null)
        {
            Debug.LogError("DiceSpawner is missing references (gridGenerator or dicePool)!");
            yield break;
        }

        gridCells.Clear();
        foreach (Transform child in gridGenerator.transform)
        {
            gridCells.Add(child);
        }

        // Spawn starting dice
        for (int i = 0; i < startWithDiceCount; i++)
        {
            SpawnDiceOnRandomFreeCell();
        }
    }

    public void SpawnDiceOnRandomFreeCell()
    {
        if (dicePool == null) return;

        List<Transform> availableCells = new List<Transform>();
        foreach (var cell in gridCells)
        {
            if (!occupiedCells.Contains(cell))
                availableCells.Add(cell);
        }

        if (availableCells.Count == 0) return;

        Transform chosenCell = availableCells[Random.Range(0, availableCells.Count)];
        DiceData randomDiceData = dicePool.GetRandomDice();
        if (randomDiceData == null) return;

        GameObject dice = Instantiate(randomDiceData.prefab, chosenCell.position, Quaternion.identity);
        dice.transform.SetParent(chosenCell);
        occupiedCells.Add(chosenCell);

        DiceDrag drag = dice.GetComponent<DiceDrag>();
        if (drag != null)
        {
            drag.SetOriginalPosition(chosenCell.position);
            drag.SetParentCell(chosenCell);
        }

        Dice diceScript = dice.GetComponent<Dice>();
        if (diceScript != null)
        {
            diceScript.diceData = randomDiceData; // Assign DiceData
        }
    }

    public Dice TrySpawnSpecificDice(DiceData data)
    {
        if (data == null) return null;

        List<Transform> availableCells = new List<Transform>();
        foreach (var cell in gridCells)
        {
            if (!occupiedCells.Contains(cell))
                availableCells.Add(cell);
        }

        if (availableCells.Count == 0)
        {
            Debug.LogWarning("Cannot place dice: Board is full!");
            return null;
        }

        Transform chosenCell = availableCells[Random.Range(0, availableCells.Count)];
        GameObject dice = Instantiate(data.prefab, chosenCell.position, Quaternion.identity);
        dice.transform.SetParent(chosenCell);
        occupiedCells.Add(chosenCell);

        DiceDrag drag = dice.GetComponent<DiceDrag>();
        if (drag != null)
        {
            drag.SetOriginalPosition(chosenCell.position);
            drag.SetParentCell(chosenCell);
        }

        Dice diceScript = dice.GetComponent<Dice>();
        if (diceScript != null)
        {
            diceScript.diceData = data; // Assign DiceData
        }

        return diceScript;
    }

    public Dice SpawnDiceAt(DiceData data, Transform cell)
    {
        if (data == null || cell == null) return null;

        if (occupiedCells.Contains(cell))
        {
            Debug.LogWarning("Cannot spawn dice: Cell is occupied!");
            return null;
        }

        GameObject dice = Instantiate(data.prefab, cell.position, Quaternion.identity);
        dice.transform.SetParent(cell);
        occupiedCells.Add(cell);

        DiceDrag drag = dice.GetComponent<DiceDrag>();
        if (drag != null)
        {
            drag.SetOriginalPosition(cell.position);
            drag.SetParentCell(cell);
        }

        Dice diceScript = dice.GetComponent<Dice>();
        if (diceScript != null)
        {
            diceScript.diceData = data;
        }

        return diceScript;
    }

    public void ReleaseCell(Transform cell)
    {
        occupiedCells.Remove(cell);
    }

    public bool IsCellOccupied(Transform cell)
    {
        return occupiedCells.Contains(cell);
    }

    public void OccupyCell(Transform cell)
    {
        occupiedCells.Add(cell);
    }

    public Transform GetNearestFreeCell(Vector3 pos)
    {
        Transform best = null;
        float minDist = float.MaxValue;

        foreach (var cell in gridCells)
        {
            float dist = Vector3.Distance(pos, cell.position);
            if (dist < minDist && !occupiedCells.Contains(cell))
            {
                minDist = dist;
                best = cell;
            }
        }

        return best;
    }
}
