using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AchievementItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    
    [Header("Progress")]
    public GameObject progressContainer; // To hide for single trigger
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    [Header("Status")]
    public Button claimButton;
    public TextMeshProUGUI claimButtonText;
    public TextMeshProUGUI dateClearedText;
    public GameObject completedOverlay; // Optional visual for completed

    [Header("Rewards")]
    public Transform rewardContainer;
    public GameObject rewardTemplate; // Prefab for reward icon

    private AchievementData data;

    public void Setup(AchievementData achievement)
    {
        data = achievement;
        
        // Basic Info
        if (achievement.icon != null) iconImage.sprite = achievement.icon;
        titleText.text = achievement.title;
        descriptionText.text = achievement.description;

        UpdateState();
    }

    public void UpdateState()
    {
        if (data == null) return;

        int current = AchievementManager.Instance.GetProgress(data.id);
        bool isUnlocked = AchievementManager.Instance.IsUnlocked(data.id);
        bool isClaimed = AchievementManager.Instance.IsClaimed(data.id);

        // Progress Bar
        if (data.isSingleTrigger)
        {
            progressContainer.SetActive(false);
        }
        else
        {
            progressContainer.SetActive(true);
            progressBar.maxValue = data.targetValue;
            progressBar.value = current;
            progressText.text = $"{current} / {data.targetValue}";
        }

        // Date Cleared
        if (isUnlocked)
        {
            dateClearedText.text = "Cleared: " + AchievementManager.Instance.GetDateCleared(data.id);
            dateClearedText.gameObject.SetActive(true);
        }
        else
        {
            dateClearedText.gameObject.SetActive(false);
        }

        // Claim Button Logic
        if (isClaimed)
        {
            claimButton.interactable = false;
            claimButtonText.text = "Claimed";
            if (completedOverlay != null) completedOverlay.SetActive(true);
        }
        else if (isUnlocked)
        {
            claimButton.interactable = true;
            claimButtonText.text = "Claim";
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClaimClicked);
            if (completedOverlay != null) completedOverlay.SetActive(false);
        }
        else
        {
            claimButton.interactable = false;
            claimButtonText.text = "Locked";
            if (completedOverlay != null) completedOverlay.SetActive(false);
        }

        // Rewards (Only populate once)
        if (rewardContainer.childCount == 0 && rewardTemplate != null)
        {
            foreach (var reward in data.rewards)
            {
                GameObject rewardObj = Instantiate(rewardTemplate, rewardContainer);
                rewardObj.SetActive(true);
                // Setup reward visual (simple text for now, can be expanded)
                TextMeshProUGUI rewardText = rewardObj.GetComponentInChildren<TextMeshProUGUI>();
                if (rewardText != null)
                {
                    rewardText.text = $"{reward.amount} {reward.type}";
                }
            }
        }
    }

    void OnClaimClicked()
    {
        AchievementManager.Instance.Claim(data.id);
        UpdateState(); // Refresh UI immediately
    }
}
