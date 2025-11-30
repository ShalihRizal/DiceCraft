using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RelicListUI : MonoBehaviour
{
    public GameObject relicIconPrefab;
    public Transform container;
    public TextMeshProUGUI relicCounterText; // Optional: "3/20 Relics"

    private void Start()
    {
        // Subscribe to event
        RelicManager.OnRelicAdded += AddRelicIcon;

        // Populate existing relics
        if (RelicManager.Instance != null)
        {
            foreach (var relic in RelicManager.Instance.collectedRelics)
            {
                AddRelicIcon(relic);
            }
        }

        UpdateCounter();
    }

    private void OnDestroy()
    {
        RelicManager.OnRelicAdded -= AddRelicIcon;
    }

    private void AddRelicIcon(RelicData relic)
    {
        if (relicIconPrefab == null || container == null) return;

        GameObject iconObj = Instantiate(relicIconPrefab, container);
        RelicIconUI iconUI = iconObj.GetComponent<RelicIconUI>();
        if (iconUI != null)
        {
            iconUI.Setup(relic);
        }

        UpdateCounter();
    }

    private void UpdateCounter()
    {
        if (relicCounterText != null && RelicManager.Instance != null)
        {
            int count = RelicManager.Instance.collectedRelics.Count;
            relicCounterText.text = $"{count} Relics";
        }
    }
}
