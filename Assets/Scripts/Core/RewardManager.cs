using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("Perk Database")]
    public List<PerkData> allPerks; // Assign in Inspector or load from Resources

    [Header("UI Reference")]
    public RewardUI rewardUI;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GenerateRewards()
    {
        if (allPerks == null || allPerks.Count == 0)
        {
            Debug.LogWarning("RewardManager: No perks defined!");
            GameManager.Instance.FinishCombatPhase();
            return;
        }




        List<PerkData> options = new List<PerkData>();
        List<PerkData> pool = new List<PerkData>(allPerks);

        // Pick 3 random perks with weighted rarity
        for (int i = 0; i < 3; i++)
        {
            if (pool.Count == 0) break;
            
            PerkData selected = GetWeightedRandomPerk(pool);
            if (selected != null)
            {
                options.Add(selected);
                pool.Remove(selected); // Avoid duplicates
            }
        }

        if (rewardUI != null)
        {
            rewardUI.ShowRewards(options);
        }
        else
        {
            Debug.LogError("RewardManager: RewardUI not assigned!");
            GameManager.Instance.FinishCombatPhase();
        }
    }

    private PerkData GetWeightedRandomPerk(List<PerkData> pool)
    {
        int totalWeight = 0;
        foreach (var p in pool) totalWeight += GetWeight(p.rarity);

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var p in pool)
        {
            currentWeight += GetWeight(p.rarity);
            if (randomValue < currentWeight)
            {
                return p;
            }
        }
        return pool[0]; // Fallback
    }

    private int GetWeight(PerkRarity rarity)
    {
        return rarity switch
        {
            PerkRarity.Common => 60,
            PerkRarity.Rare => 30,
            PerkRarity.Epic => 9,
            PerkRarity.Legendary => 1,
            _ => 10
        };
    }

    public void SelectReward(PerkData perk)
    {
        if (perk != null)
        {
            bool success = perk.Apply();
            if (!success)
            {
                Debug.LogWarning("⚠️ Could not apply perk (e.g. Inventory Full).");
                // Show UI feedback
                if (rewardUI != null)
                {
                    // Assuming RewardUI has a method to show warning, or we can add one.
                    // For now, let's just NOT hide the UI so the user can fix the issue (e.g. sell dice).
                    // But we need to tell them WHY.
                    // Let's add a simple floating text or log.
                }
                return; // 🛑 Don't close UI, don't finish phase
            }
            
            Debug.Log($"🏆 Reward Selected: {perk.perkName}");
        }

        if (rewardUI != null)
        {
            rewardUI.Hide();
        }

        GameManager.Instance.FinishCombatPhase();
    }
}
