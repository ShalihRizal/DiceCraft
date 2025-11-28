using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (rewardUI == null)
        {
            rewardUI = FindFirstObjectByType<RewardUI>(FindObjectsInactive.Include);
        }

        // Auto-find DicePool
        if (dicePool == null)
        {
            var spawner = FindFirstObjectByType<DiceSpawner>();
            if (spawner != null) dicePool = spawner.dicePool;
        }

        // Auto-load Perks if empty
        if (allPerks == null || allPerks.Count == 0)
        {
            allPerks = new List<PerkData>(Resources.LoadAll<PerkData>(""));
        }
    }

    public enum RewardType
    {
        Dice,
        Relic,
        Skill
    }

    public class RewardOption
    {
        public RewardType type;
        public DiceData dice;
        public RelicData relic;
        public PerkData perk;
        public string description;
        public Sprite icon;
    }

    [Header("Databases")]
    public DicePool dicePool;
    public List<PerkData> allPerks;
    // public List<RelicData> allRelics; // Access via RelicManager or assign here

    [Header("UI Reference")]
    public RewardUI rewardUI;

    [Header("Settings")]
    public int skipGoldReward = 25;
    public int rerollCost = 50;

    private RewardType currentRewardType;

    public void GenerateRewards(RewardType type)
    {
        currentRewardType = type;
        List<RewardOption> options = new List<RewardOption>();

        for (int i = 0; i < 3; i++)
        {
            RewardOption option = new RewardOption();
            option.type = type;
            bool isValid = false;

            if (type == RewardType.Dice)
            {
                if (dicePool != null)
                {
                    option.dice = dicePool.GetRandomDice();
                    if (option.dice != null)
                    {
                        option.description = option.dice.diceName;
                        // option.icon = option.dice.icon;
                        isValid = true;
                    }
                }
            }
            else if (type == RewardType.Relic)
            {
                if (ShopManager.Instance != null && ShopManager.Instance.relicPool != null && ShopManager.Instance.relicPool.Count > 0)
                {
                    var pool = ShopManager.Instance.relicPool;
                    option.relic = pool[Random.Range(0, pool.Count)];
                    if (option.relic != null)
                    {
                        option.description = option.relic.relicName;
                        option.icon = option.relic.icon;
                        isValid = true;
                    }
                }
            }
            else if (type == RewardType.Skill)
            {
                if (allPerks != null && allPerks.Count > 0)
                {
                    option.perk = allPerks[Random.Range(0, allPerks.Count)];
                    if (option.perk != null)
                    {
                        option.description = option.perk.perkName;
                        option.icon = option.perk.icon;
                        isValid = true;
                    }
                }
            }
            
            if (isValid)
            {
                options.Add(option);
            }
        }

        if (rewardUI != null)
        {
            rewardUI.ShowRewards(options, skipGoldReward, rerollCost);
        }
    }

    public void SelectReward(RewardOption option)
    {
        Debug.Log($"SelectReward called. Type: {option.type}");
        
        if (option.type == RewardType.Dice)
        {
            Debug.Log($"Attempting to add dice to inventory: {option.dice?.diceName}");
            if (InventoryManager.Instance != null)
            {
                bool success = InventoryManager.Instance.AddDice(option.dice);
                if (success) Debug.Log("Dice added to inventory successfully.");
                else Debug.LogWarning("Failed to add dice to inventory (Full?).");
            }
            else
            {
                Debug.LogError("InventoryManager not found! Fallback to Spawner.");
                // Fallback to spawning on board if no inventory
                var spawner = FindFirstObjectByType<DiceSpawner>();
                if (spawner != null) spawner.TrySpawnSpecificDice(option.dice);
            }
        }
        else if (option.type == RewardType.Relic)
        {
            Debug.Log($"Adding relic: {option.relic?.relicName}");
            if (RelicManager.Instance == null)
            {
                Debug.LogWarning("RelicManager instance not found. Creating one dynamically.");
                GameObject go = new GameObject("RelicManager");
                go.AddComponent<RelicManager>();
            }
            
            if (RelicManager.Instance != null) 
            {
                RelicManager.Instance.AddRelic(option.relic);
            }
            else
            {
                Debug.LogError("Failed to create RelicManager!");
            }
        }
        else if (option.type == RewardType.Skill)
        {
            Debug.Log($"Applying perk: {option.perk?.perkName}");
            if (option.perk != null) option.perk.Apply();
            else Debug.LogError("Perk data is null!");
        }

        CloseRewards();
    }

    public void SkipReward()
    {
        if (PlayerCurrency.Instance != null)
        {
            PlayerCurrency.Instance.AddGold(skipGoldReward);
        }
        CloseRewards();
    }

    public void RerollRewards()
    {
        if (PlayerCurrency.Instance.SpendGold(rerollCost))
        {
            GenerateRewards(currentRewardType);
        }
    }

    private void CloseRewards()
    {
        if (rewardUI != null) rewardUI.Hide();
        GameManager.Instance.FinishCombatPhase();
    }
}
