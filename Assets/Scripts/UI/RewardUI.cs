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

    private CanvasGroup canvasGroup;
    private bool hasPendingRewards = false;
    public bool HasPendingRewards => hasPendingRewards;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        // Initial state
        if (panel != null) panel.SetActive(true); // Keep active for Update loop
        Hide(); // Start hidden via CanvasGroup
        
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

    void Update()
    {
        if (hasPendingRewards)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ToggleVisibility();
            }
        }
    }

    public Button skipButton;
    public Button rerollButton;

    public void ShowRewards(List<RewardManager.RewardOption> options, int skipGold, int rerollCost)
    {
        hasPendingRewards = true;
        
        // Show via CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (panel != null) panel.SetActive(true);
        if (openButton != null) openButton.gameObject.SetActive(false);
        
        SetInteractionState(false);

        // Clear old cards
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new cards
        foreach (var option in options)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            cardObj.SetActive(true);
            PerkCardUI cardUI = cardObj.GetComponent<PerkCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(option);
            }
        }
        
        // Setup Buttons
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => RewardManager.Instance.SkipReward());
            
            var txt = skipButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.text = $"Skip (+{skipGold}g)";
        }
        
        if (rerollButton != null)
        {
            rerollButton.gameObject.SetActive(true);
            rerollButton.onClick.RemoveAllListeners();
            rerollButton.onClick.AddListener(() => RewardManager.Instance.RerollRewards());
            
            var txt = rerollButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.text = $"Reroll (-{rerollCost}g)";
        }
    }

    public void Hide()
    {
        hasPendingRewards = false;
        
        // Hide via CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (openButton != null) openButton.gameObject.SetActive(false);
        SetInteractionState(true);
    }

    void ToggleVisibility()
    {
        if (canvasGroup == null) return;

        if (canvasGroup.alpha > 0.5f)
        {
            // Hide temporarily
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            SetInteractionState(true); // Allow board interaction
        }
        else
        {
            // Show
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            SetInteractionState(false); // Block board interaction
        }
    }

    void Minimize()
    {
        // Deprecated or can reuse ToggleVisibility logic
        ToggleVisibility();
    }

    void Maximize()
    {
        // Deprecated or can reuse ToggleVisibility logic
        ToggleVisibility();
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

    public void ShowWarning(string message)
    {
        Debug.LogWarning($"[UI Warning] {message}");
        // TODO: Implement proper UI feedback
    }
}
