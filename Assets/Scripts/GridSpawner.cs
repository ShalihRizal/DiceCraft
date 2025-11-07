using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 5;
    public int columns = 5;
    public float cellSize = 1f;
    public float spacing = 0.1f;

    [Header("Prefab")]
    public GameObject cellPrefab;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("Cell Prefab is not assigned!");
            return;
        }

        // Calculate the total grid dimensions
        float totalWidth = columns * cellSize + (columns - 1) * spacing;
        float totalHeight = rows * cellSize + (rows - 1) * spacing;

        // Calculate the top-left offset from center
        Vector2 originOffset = new Vector2(totalWidth, totalHeight) * 0.5f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                float xPos = x * (cellSize + spacing);
                float yPos = -y * (cellSize + spacing);
                Vector2 spawnPosition = new Vector2(xPos, yPos) - originOffset + new Vector2(cellSize / 7f, -cellSize / 7f);

                GameObject newCell = Instantiate(cellPrefab, spawnPosition, Quaternion.identity, transform);
                newCell.name = $"Cell_{x}_{y}";
                newCell.transform.localScale = Vector3.one * cellSize;
            }
        }
    }

}
