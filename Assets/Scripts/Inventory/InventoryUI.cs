using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform slotsParent;
    public GameObject slotPrefab;

    public List<InventorySlot> slots = new List<InventorySlot>();

    public void UpdateUI()
    {
        // Find manager if Instance is null (Edit Mode)
        InventoryManager manager = InventoryManager.Instance;
        if (manager == null)
        {
            manager = FindFirstObjectByType<InventoryManager>();
        }

        if (manager == null) return;

        // Clear existing slots
        int childCount = slotsParent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(slotsParent.GetChild(i).gameObject);
            else
                DestroyImmediate(slotsParent.GetChild(i).gameObject);
        }
        slots.Clear();

        // Create new slots
        for (int i = 0; i < manager.maxSlots; i++)
        {
            GameObject slotObj = null;
            if (Application.isPlaying)
                slotObj = Instantiate(slotPrefab, slotsParent);
            else
                slotObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(slotPrefab, slotsParent);
                
            if (slotObj == null) continue;

            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.slotIndex = i;
            slots.Add(slot);

            if (i < manager.inventoryDice.Count)
            {
                slot.SetDice(manager.inventoryDice[i]);
            }
            else
            {
                slot.ClearSlot();
            }
        }
    }
}
