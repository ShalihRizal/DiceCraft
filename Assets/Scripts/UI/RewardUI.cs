using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    public GameObject panel;
    public Button[] rewardButtons;
    public TextMeshProUGUI[] rewardTexts;

    private List<RewardOption> currentOptions;

    void Start()
    {
        Hide(); // Start hidden
    }

    public void ShowRewards(List<RewardOption> options)
    {
        currentOptions = options;
        panel.SetActive(true);

        for (int i = 0; i < rewardButtons.Length; i++)
        {
            if (i < options.Count)
            {
                rewardButtons[i].gameObject.SetActive(true);
                if (rewardTexts[i] != null)
                    rewardTexts[i].text = options[i].description;
                
                int index = i; // Capture for lambda
                rewardButtons[i].onClick.RemoveAllListeners();
                rewardButtons[i].onClick.AddListener(() => OnRewardClicked(index));
            }
            else
            {
                rewardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnRewardClicked(int index)
    {
        if (RewardManager.Instance != null && index < currentOptions.Count)
        {
            RewardManager.Instance.SelectReward(currentOptions[index]);
        }
    }
}
