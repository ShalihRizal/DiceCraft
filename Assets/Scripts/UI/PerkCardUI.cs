using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PerkCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button selectButton;
    
    [Header("Rarity Styling")]
    public Image cardBackground; // Main background of the card
    public Image cardBorder;     // Border/frame around the card
    
    [Header("Rarity Sprites")]
    public Sprite commonSprite;
    public Sprite uncommonSprite;  // For dice only
    public Sprite rareSprite;
    public Sprite epicSprite;      // For dice only
    public Sprite legendarySprite;
    public Sprite skillSprite;     // For perks/skills

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

        // Apply rarity-based styling
        ApplyRarityStyle(option);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelect);
        }
    }

    private void ApplyRarityStyle(RewardManager.RewardOption option)
    {
        Sprite targetSprite = null;

        // Determine which sprite to use based on rarity
        if (option.type == RewardManager.RewardType.Relic && option.relic != null)
        {
            // Relics use Common, Rare, or Legendary
            switch (option.relic.rarity)
            {
                case RelicRarity.Common:
                    targetSprite = commonSprite;
                    break;
                case RelicRarity.Rare:
                    targetSprite = rareSprite;
                    break;
                case RelicRarity.Legendary:
                    targetSprite = legendarySprite;
                    break;
            }
        }
        else if (option.type == RewardManager.RewardType.Dice && option.dice != null)
        {
            // Dice use all 5 rarities
            switch (option.dice.rarity)
            {
                case DiceRarity.Common:
                    targetSprite = commonSprite;
                    break;
                case DiceRarity.Uncommon:
                    targetSprite = uncommonSprite;
                    break;
                case DiceRarity.Rare:
                    targetSprite = rareSprite;
                    break;
                case DiceRarity.Epic:
                    targetSprite = epicSprite;
                    break;
                case DiceRarity.Legendary:
                    targetSprite = legendarySprite;
                    break;
            }
        }
        else if (option.type == RewardManager.RewardType.Skill)
        {
            targetSprite = skillSprite;
        }

        // Apply the sprite to both background and border (or just one, depending on your design)
        if (cardBackground != null && targetSprite != null)
        {
            cardBackground.sprite = targetSprite;
            cardBackground.color = Color.white; // Reset color to show sprite properly
        }

        if (cardBorder != null && targetSprite != null)
        {
            cardBorder.sprite = targetSprite;
            cardBorder.color = Color.white; // Reset color to show sprite properly
        }
    }

    void OnSelect()
    {
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.SelectReward(option);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 🚫 Don't show tooltips during reward phase
        if (GameManager.Instance != null && GameManager.Instance.IsRewardPhaseActive)
        {
            return;
        }
        
        if (option == null || TooltipManager.Instance == null) return;

        if (option.type == RewardManager.RewardType.Dice && option.dice != null)
        {
            TooltipManager.Instance.ShowTooltip(option.dice, transform.position);
        }
        else if (option.type == RewardManager.RewardType.Relic && option.relic != null)
        {
            TooltipManager.Instance.ShowTooltip(option.relic, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    void OnDisable()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
