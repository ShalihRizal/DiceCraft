using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceTooltipManager : MonoBehaviour
{
    public static DiceTooltipManager Instance;

    public GameObject tooltipPrefab;
    private DiceTooltip tooltipInstance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowTooltip(Dice dice, Vector3 worldPosition)
    {
        if (tooltipInstance == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                Debug.LogWarning("⚠ Canvas not found for tooltip!");
                return;
            }

            GameObject tooltipGO = Instantiate(tooltipPrefab, canvas.transform);
            tooltipInstance = tooltipGO.GetComponent<DiceTooltip>();
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        if (float.IsInfinity(screenPos.x) || float.IsInfinity(screenPos.y))
            return;

        tooltipInstance.transform.position = screenPos;
        tooltipInstance.SetInfo(dice);
        tooltipInstance.gameObject.SetActive(true);
    }

    public void ShowTooltip(DiceData data, Vector3 worldPosition)
    {
        if (tooltipInstance == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null) return;
            GameObject tooltipGO = Instantiate(tooltipPrefab, canvas.transform);
            tooltipInstance = tooltipGO.GetComponent<DiceTooltip>();
        }

        Vector3 screenPos = worldPosition; // Inventory slots are already in screen/canvas space usually, or we need to convert
        // If worldPosition is from UI (InventorySlot), it might be screen space already or world space of UI element.
        // Let's assume the caller handles conversion or we check.
        // Actually, InventorySlot is UI, so transform.position is World Position of the UI element.
        // WorldToScreenPoint on a UI element's world position usually gives the screen position.
        
        // However, if the canvas is Overlay, World position IS screen position (mostly).
        // If Canvas is Camera, we need WorldToScreenPoint.
        // Let's stick to WorldToScreenPoint for safety as it handles both 3D world and UI world if set up right.
        
        // But wait, for UI elements in Overlay canvas, transform.position is in pixel coordinates (Screen Space).
        // For Camera canvas, it's in World Space.
        // Let's try using the same logic as above for now.
        
        Vector3 finalScreenPos = Camera.main.WorldToScreenPoint(worldPosition);
        
        // If the UI is overlay, WorldToScreenPoint might be weird if we pass the UI position directly?
        // Actually for Overlay, transform.position IS the screen position.
        // Let's check if we are in overlay or camera.
        // For now, let's just try setting it directly if it looks like screen coords, or converting.
        
        // A safer bet for UI to UI tooltip is just setting position if they are in same canvas space.
        // But Tooltip might be parented to Canvas, and Slot is also in Canvas.
        
        tooltipInstance.transform.position = finalScreenPos; 
        // NOTE: This might need adjustment based on Canvas mode.
        
        tooltipInstance.SetInfo(data);
        tooltipInstance.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipInstance != null)
            tooltipInstance.gameObject.SetActive(false);
    }
}
