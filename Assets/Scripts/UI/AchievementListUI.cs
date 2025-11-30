using System.Collections.Generic;
using UnityEngine;

public class AchievementListUI : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform contentContainer;

    private List<AchievementItemUI> items = new List<AchievementItemUI>();

    void OnEnable()
    {
        RefreshList();
        AchievementManager.Instance.OnProgressUpdated += RefreshAllItems;
    }

    void OnDisable()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnProgressUpdated -= RefreshAllItems;
        }
    }

    public void RefreshList()
    {
        // Clear existing
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        items.Clear();

        if (AchievementManager.Instance == null) return;

        Debug.Log($"[AchievementListUI] Refreshing list. Found {AchievementManager.Instance.allAchievements.Count} achievements.");

        foreach (var achievement in AchievementManager.Instance.allAchievements)
        {
            GameObject obj = Instantiate(itemPrefab, contentContainer);
            obj.SetActive(true); // Ensure the item is visible
            
            AchievementItemUI ui = obj.GetComponent<AchievementItemUI>();
            if (ui != null)
            {
                ui.Setup(achievement);
                items.Add(ui);
            }
        }
    }

    void RefreshAllItems()
    {
        foreach (var item in items)
        {
            item.UpdateState();
        }
    }
    
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
