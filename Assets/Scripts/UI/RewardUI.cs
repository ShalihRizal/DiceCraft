using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardUI : MonoBehaviour
{
    public GameObject panel;
    public Transform cardsContainer;
    public GameObject cardPrefab;
    
    [Header("Toggle Controls")]
    public Button minimizeButton;
    public Button openButton;

    void Start()
    {
        // Initial state
        if (panel != null) panel.SetActive(false);
        if (openButton != null)
        {
            openButton.gameObject.SetActive(false);
            openButton.onClick.AddListener(Maximize);
        }
        if (minimizeButton != null)
        {
            minimizeButton.onClick.AddListener(Minimize);
        }
    }

    public void ShowRewards(List<PerkData> perks)
    {
        if (panel != null) panel.SetActive(true);
        if (openButton != null) openButton.gameObject.SetActive(false);
        
        SetInteractionState(false);

        // Clear old cards
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new cards
        foreach (var perk in perks)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            cardObj.SetActive(true);
            PerkCardUI cardUI = cardObj.GetComponent<PerkCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(perk);
            }
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        if (openButton != null) openButton.gameObject.SetActive(false);
        SetInteractionState(true);
    }

    void Minimize()
    {
        if (panel != null) panel.SetActive(false);
        if (openButton != null) openButton.gameObject.SetActive(true);
        SetInteractionState(true);
    }

    void Maximize()
    {
        if (panel != null) panel.SetActive(true);
        if (openButton != null) openButton.gameObject.SetActive(false);
        SetInteractionState(false);
    }

    private void SetInteractionState(bool interactable)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsRewardPhaseActive = !interactable;
        }

        // Toggle Inventory Interaction
        if (InventoryManager.Instance != null && InventoryManager.Instance.inventoryUI != null)
        {
            CanvasGroup group = InventoryManager.Instance.inventoryUI.GetComponent<CanvasGroup>();
            if (group == null) group = InventoryManager.Instance.inventoryUI.gameObject.AddComponent<CanvasGroup>();
            
            group.interactable = interactable;
            group.blocksRaycasts = interactable;
        }
    }
}
