using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<DiceData> inventoryDice = new List<DiceData>();
    public int maxSlots = 12;

    public InventoryUI inventoryUI;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (inventoryUI != null)
            inventoryUI.UpdateUI();
    }

    public bool AddDice(DiceData dice)
    {
        if (inventoryDice.Count >= maxSlots)
        {
            Debug.Log("Inventory Full!");
            return false;
        }

        inventoryDice.Add(dice);
        inventoryUI.UpdateUI();
        return true;
    }

    public void RemoveDice(DiceData dice)
    {
        if (inventoryDice.Contains(dice))
        {
            inventoryDice.Remove(dice);
            inventoryUI.UpdateUI();
        }
    }

    public void RemoveDiceAt(int index)
    {
        if (index >= 0 && index < inventoryDice.Count)
        {
            inventoryDice.RemoveAt(index);
            inventoryUI.UpdateUI();
        }
    }

    public void SetMaxSlots(int newSize)
    {
        if (newSize < 0) return;
        
        maxSlots = newSize;
        
        // Optional: Handle case where newSize < inventoryDice.Count
        // For now, we keep the dice but they won't be shown if UI doesn't support pagination
        // Or UI will just show what it can.
        // InventoryUI loop is based on maxSlots, so it will truncate the view.
        
        if (inventoryUI != null)
            inventoryUI.UpdateUI();
            
        Debug.Log($"Inventory size updated to {maxSlots}");
    }

    void OnValidate()
    {
        if (inventoryUI != null)
        {
            // Delay update to avoid errors during serialization
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () => 
            {
                if (this != null && inventoryUI != null)
                    inventoryUI.UpdateUI();
            };
            #endif
        }
    }
}
