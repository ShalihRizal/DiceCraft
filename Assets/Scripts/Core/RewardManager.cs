using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum RewardType
{
    Gold,
    Heal,
    Dice
}

[System.Serializable]
public struct RewardOption
{
    public RewardType type;
    public string description;
    public int value; // Amount of gold, heal amount, or dice ID (if we had IDs)
    public Sprite icon;
}

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;
    public RewardUI rewardUI;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GenerateRewards()
    {
        List<RewardOption> options = new List<RewardOption>();

        // Option 1: Gold
        int goldAmount = Random.Range(10, 25);
        options.Add(new RewardOption 
        { 
            type = RewardType.Gold, 
            description = $"+{goldAmount} Gold", 
            value = goldAmount 
        });

        // Option 2: Heal (if not full health) or more Gold
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.currentHealth < PlayerHealth.Instance.maxHealth)
        {
            int healAmount = 20;
            options.Add(new RewardOption 
            { 
                type = RewardType.Heal, 
                description = $"Heal {healAmount} HP", 
                value = healAmount 
            });
        }
        else
        {
            int bonusGold = Random.Range(15, 30);
            options.Add(new RewardOption 
            { 
                type = RewardType.Gold, 
                description = $"+{bonusGold} Gold (Bonus)", 
                value = bonusGold 
            });
        }

        // Option 3: Random Dice (Simulated by giving enough gold for a dice or a free dice token)
        // For now, let's give a large gold sum representing a "Dice Fund" or just more gold
        int diceFund = 50;
        options.Add(new RewardOption 
        { 
            type = RewardType.Gold, 
            description = "Big Gold Bag", 
            value = diceFund 
        });

        if (rewardUI != null)
        {
            rewardUI.ShowRewards(options);
        }
    }

    public void SelectReward(RewardOption option)
    {
        switch (option.type)
        {
            case RewardType.Gold:
                PlayerCurrency.Instance.AddGold(option.value);
                break;
            case RewardType.Heal:
                PlayerHealth.Instance.Heal(option.value);
                break;
            case RewardType.Dice:
                // Logic to add a free dice would go here
                break;
        }

        Debug.Log($"🎁 Selected Reward: {option.description}");
        
        // Close UI and Resume Game Flow
        if (rewardUI != null) rewardUI.Hide();
        
        // Advance time or finish combat phase
        GameManager.Instance.FinishCombatPhase();
    }
}
