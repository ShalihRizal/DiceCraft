using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Data")]
    public List<AchievementData> allAchievements;

    // Runtime State
    private Dictionary<string, int> currentProgress = new Dictionary<string, int>();
    private Dictionary<string, bool> isClaimed = new Dictionary<string, bool>();
    private Dictionary<string, string> dateCleared = new Dictionary<string, string>(); // Format: "yyyy-MM-dd"

    // Events
    public event Action<AchievementData> OnAchievementUnlocked;
    public event Action<AchievementData> OnAchievementClaimed;
    public event Action OnProgressUpdated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to game events
        GameEvents.OnPlayerDied += HandlePlayerDied;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    void OnDestroy()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDied;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    // --- Event Handlers ---

    void HandlePlayerDied()
    {
        AddProgress("first_death", 1);
    }

    void HandleEnemyKilled()
    {
        AddProgress("newbie_killer", 1);
    }

    // --- Core Logic ---

    public void AddProgress(string id, int amount)
    {
        AchievementData achievement = GetAchievement(id);
        if (achievement == null) return;
        if (IsUnlocked(id)) return; // Already unlocked

        if (!currentProgress.ContainsKey(id))
            currentProgress[id] = 0;

        currentProgress[id] += amount;

        // Check for unlock
        if (currentProgress[id] >= achievement.targetValue)
        {
            currentProgress[id] = achievement.targetValue;
            Unlock(achievement);
        }

        SaveData();
        OnProgressUpdated?.Invoke();
    }

    void Unlock(AchievementData achievement)
    {
        if (dateCleared.ContainsKey(achievement.id)) return; // Already unlocked

        dateCleared[achievement.id] = DateTime.Now.ToString("yyyy-MM-dd");
        SaveData();
        
        Debug.Log($"🏆 Achievement Unlocked: {achievement.title}");
        OnAchievementUnlocked?.Invoke(achievement);
    }

    public void Claim(string id)
    {
        AchievementData achievement = GetAchievement(id);
        if (achievement == null) return;
        if (!IsUnlocked(id)) return; // Not unlocked yet
        if (IsClaimed(id)) return; // Already claimed

        // Grant Rewards
        foreach (var reward in achievement.rewards)
        {
            GrantReward(reward);
        }

        isClaimed[id] = true;
        SaveData();
        
        OnAchievementClaimed?.Invoke(achievement);
        OnProgressUpdated?.Invoke(); // Update UI
    }

    void GrantReward(AchievementReward reward)
    {
        switch (reward.type)
        {
            case AchievementReward.RewardType.Coin:
                // Assuming PlayerCurrency exists or similar
                // PlayerCurrency.Instance.AddCoins(reward.amount);
                Debug.Log($"Granted {reward.amount} Coins");
                break;
            case AchievementReward.RewardType.DicePip:
                // PlayerCurrency.Instance.AddPips(reward.amount);
                Debug.Log($"Granted {reward.amount} Dice Pips");
                break;
        }
    }

    // --- Helpers ---

    public AchievementData GetAchievement(string id)
    {
        return allAchievements.Find(a => a.id == id);
    }

    public bool IsUnlocked(string id)
    {
        return dateCleared.ContainsKey(id);
    }

    public bool IsClaimed(string id)
    {
        return isClaimed.ContainsKey(id) && isClaimed[id];
    }

    public int GetProgress(string id)
    {
        return currentProgress.ContainsKey(id) ? currentProgress[id] : 0;
    }

    public string GetDateCleared(string id)
    {
        return dateCleared.ContainsKey(id) ? dateCleared[id] : "";
    }

    // --- Persistence ---

    void SaveData()
    {
        // Simple PlayerPrefs save for now. 
        // In a real app, use JSON or binary serialization for better structure.
        
        foreach (var kvp in currentProgress)
        {
            PlayerPrefs.SetInt($"Ach_Prog_{kvp.Key}", kvp.Value);
        }

        foreach (var kvp in dateCleared)
        {
            PlayerPrefs.SetString($"Ach_Date_{kvp.Key}", kvp.Value);
        }

        foreach (var kvp in isClaimed)
        {
            PlayerPrefs.SetInt($"Ach_Claim_{kvp.Key}", kvp.Value ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    void LoadData()
    {
        foreach (var achievement in allAchievements)
        {
            string id = achievement.id;

            if (PlayerPrefs.HasKey($"Ach_Prog_{id}"))
                currentProgress[id] = PlayerPrefs.GetInt($"Ach_Prog_{id}");

            if (PlayerPrefs.HasKey($"Ach_Date_{id}"))
                dateCleared[id] = PlayerPrefs.GetString($"Ach_Date_{id}");

            if (PlayerPrefs.HasKey($"Ach_Claim_{id}"))
                isClaimed[id] = PlayerPrefs.GetInt($"Ach_Claim_{id}") == 1;
        }
    }
}
