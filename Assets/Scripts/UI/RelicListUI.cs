using UnityEngine;
using System.Collections.Generic;

public class RelicListUI : MonoBehaviour
{
    public GameObject relicIconPrefab;
    public Transform container;

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
    }
}
