using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool IsCombatActive { get; set; } = false;
    public bool IsRewardPhaseActive { get; set; } = false;
    public bool IsMapActive { get; set; } = false;

    public int currentDay { get; private set; } = 1;
    public int currentHour { get; private set; } = 1;
    private const int maxHoursPerDay = 10;

    [Header("Global Stats")]
    public float globalDamageMultiplier = 1f;
    public float globalFireRateMultiplier = 1f;
    public float globalCritChance = 0f;

    public System.Collections.Generic.List<DicePassive> globalPassives = new System.Collections.Generic.List<DicePassive>();

    public void AddGlobalPassive(DicePassive passive)
    {
        if (!globalPassives.Contains(passive))
        {
            globalPassives.Add(passive);
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        GameEvents.OnDiceMerged += HandleDiceMerged;
    }

    void OnDestroy()
    {
        if (Instance == this)
            GameEvents.OnDiceMerged -= HandleDiceMerged;
    }

    public void StartPreparationPhase()
    {
        Debug.Log("🛡 Entering Preparation Phase...");
        IsCombatActive = false;
        IsMapActive = false;
        
        // Hide Map
        MapUI mapUI = FindFirstObjectByType<MapUI>(FindObjectsInactive.Include);
        if (mapUI != null) mapUI.Hide();
        
        // Notify listeners (UI should show "Start Wave" button)
        GameEvents.RaisePreparationPhaseStarted();
    }

    public void StartCombat()
    {
        if (IsMapActive)
        {
            Debug.LogWarning("⚠ Cannot start combat: Map Phase is active!");
            return;
        }

        // Check if there are any dice on the board
        DiceSpawner spawner = FindFirstObjectByType<DiceSpawner>();
        if (spawner != null)
        {
            if (spawner.OccupiedCellCount == 0)
            {
                Debug.LogWarning("⚠ Cannot start combat: No dice on the board!");
                // Show floating text or UI message?
                return;
            }
        }

        IsCombatActive = true;
        IsMapActive = false;
        
        // Ensure Map is hidden (just in case)
        MapUI mapUI = FindFirstObjectByType<MapUI>(FindObjectsInactive.Include);
        if (mapUI != null) mapUI.Hide();
        
        GameEvents.RaiseCombatStarted();
        
        // EnemySpawner listens to OnCombatStarted event, no need to call directly
    }

    public void EndCombat()
    {
        Debug.Log("Combat finished. Choosing rewards...");
        IsCombatActive = false;
        GameEvents.RaiseCombatEnded();
        
        // Trigger Reward Phase based on Node Type
        if (RewardManager.Instance != null)
        {
            RewardManager.RewardType rewardType = RewardManager.RewardType.Dice; // Default
            
            if (MapManager.Instance != null && MapManager.Instance.currentNode != null)
            {
                NodeType nodeType = MapManager.Instance.currentNode.nodeType;
                if (nodeType == NodeType.Elite) rewardType = RewardManager.RewardType.Relic;
                else if (nodeType == NodeType.Boss) rewardType = RewardManager.RewardType.Skill;
            }
            
            RewardManager.Instance.GenerateRewards(rewardType);
        }
        else
        {
            FinishCombatPhase();
        }
    }

    public void FinishCombatPhase()
    {
        AdvanceTime();
        Debug.Log("Combat Phase Complete. Time Advanced.");
        
        // Check if Map System is active
        if (MapManager.Instance != null && MapManager.Instance.currentMap != null)
        {
            ShowMap();
        }
        else
        {
            // Fallback to old loop if no map
            Debug.Log("Waiting for player to start next wave...");
            GameEvents.RaisePreparationPhaseStarted();
        }
    }

    private void HandleDiceMerged(Dice owner, Dice mergedInto)
    {
        string ownerName = owner != null ? owner.name : "Inventory Dice";
        Debug.Log($"🧬 Dice merged: {ownerName} → {mergedInto.name}");

        // Trigger passive for merged dice
        mergedInto.diceData?.passive?.OnDiceMerged(owner, mergedInto);

        // Visual Effect
        if (mergedInto.diceData != null)
            mergedInto.PlayVFX(VFXType.Merge);

        // Global logic: e.g., track merges, achievements, unlocks
        TrackMerge(owner, mergedInto);
    }
    
    private void TrackMerge(Dice owner, Dice mergedInto)
    {
        string ownerName = owner != null ? owner.name : "Inventory Dice";
        Debug.Log($"📊 Global merge tracker: {ownerName} merged into {mergedInto.name}");
    }

    void AdvanceTime()
    {
        currentHour++;
        if (currentHour > maxHoursPerDay)
        {
            currentHour = 1;
            currentDay++;
        }

        GameEvents.OnTimeChanged?.Invoke(currentDay, currentHour);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        currentDay = 1;
        currentHour = 1;
        IsCombatActive = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void GameOver()
    {
        IsCombatActive = false;
        Time.timeScale = 0f;
        GameEvents.RaiseGameOver();
    }

    public void ShowMap()
    {
        Debug.Log("🗺 Showing Map...");
        IsCombatActive = false;
        IsRewardPhaseActive = false;
        IsMapActive = true;
        
        // Find MapUI and show it (Include inactive!)
        MapUI mapUI = FindFirstObjectByType<MapUI>(FindObjectsInactive.Include);
        if (mapUI != null)
        {
            mapUI.Show();
        }
        else
        {
            Debug.LogError("MapUI not found!");
        }
    }
}
