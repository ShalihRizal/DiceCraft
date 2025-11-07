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

    public void HideTooltip()
    {
        if (tooltipInstance != null)
            tooltipInstance.gameObject.SetActive(false);
    }
}
