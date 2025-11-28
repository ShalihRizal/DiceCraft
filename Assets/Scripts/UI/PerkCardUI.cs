using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkCardUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button selectButton;

    private RewardManager.RewardOption option;

    public void Setup(RewardManager.RewardOption option)
    {
        this.option = option;
        if (option == null) return;

        // Name
        if (nameText != null)
        {
            if (option.type == RewardManager.RewardType.Dice) nameText.text = option.dice != null ? option.dice.diceName : "Unknown Dice";
            else if (option.type == RewardManager.RewardType.Relic) nameText.text = option.relic != null ? option.relic.relicName : "Unknown Relic";
            else if (option.type == RewardManager.RewardType.Skill) nameText.text = option.perk != null ? option.perk.perkName : "Unknown Skill";
        }
        
        // Description
        if (descText != null) descText.text = option.description;
        
        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = option.icon;
            iconImage.enabled = option.icon != null;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelect);
        }
    }

    void OnSelect()
    {
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.SelectReward(option);
        }
    }
}
