using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public Transform inventoryContainer; // UI panel container
    public GameObject inventorySlotPrefab;
    public int maxSlots = 5;

    private List<Transform> slots = new List<Transform>();
    private HashSet<Transform> occupiedSlots = new HashSet<Transform>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventoryContainer);
            slots.Add(slot.transform);
        }
    }

    public bool AddDiceToInventory(GameObject dice)
    {
        foreach (var slot in slots)
        {
            if (!occupiedSlots.Contains(slot))
            {
                dice.transform.SetParent(slot);
                dice.transform.position = slot.position;
                occupiedSlots.Add(slot);

                var drag = dice.GetComponent<DiceDrag>();
                if (drag) drag.SetParentCell(null); // not on grid anymore
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public void RemoveFromInventory(Transform slot)
    {
        occupiedSlots.Remove(slot);
    }

    public bool HasSpace()
    {
        return occupiedSlots.Count < maxSlots;
    }
}
