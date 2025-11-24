using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool IsCombatActive { get; set; } = false;
    public bool IsRewardPhaseActive { get; set; } = false;

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

    public void StartCombat()
    {
        // Check if there are any dice on the board
        DiceSpawner spawner = FindFirstObjectByType<DiceSpawner>();
        if (spawner != null)
        {
            // We can check occupied cells count directly if we expose it, or check child count of grid cells
            // Since occupiedCells is private, let's check if we can find any Dice object in the scene that is on the board
            // Or better, let's add a public property to DiceSpawner or check active dice count here.
            
            // Quick check: Find objects of type Dice that are not in inventory (inventory dice don't have Dice component usually, they are data, BUT drag object has Dice component)
            // Actually, Dice component is on the prefab.
            // Let's rely on DiceSpawner having a way to tell us.
            // For now, let's check if there are any Dice objects that are children of Grid Cells.
            
            // Better approach: Add a method to DiceSpawner to get active dice count.
            // But since I can't modify DiceSpawner easily without context switching, let's try to find Dice objects.
            
            Dice[] allDice = FindObjectsByType<Dice>(FindObjectsSortMode.None);
            bool hasDiceOnBoard = false;
            foreach(var d in allDice)
            {
                // Check if it's not a drag placeholder or UI element (if any)
                // Dice on board are usually parented to Grid Cells.
                // Grid Cells are children of GridSpawner (or whatever object has GridSpawner)
                
                if (d.transform.parent != null && d.transform.parent.parent == spawner.gridGenerator.transform)
                {
                    hasDiceOnBoard = true;
                    break;
                }
                
                // Alternative: Check if parent name starts with "Cell_"
                if (d.transform.parent != null && d.transform.parent.name.StartsWith("Cell_"))
                {
                    hasDiceOnBoard = true;
                    break;
                }
            }

            if (!hasDiceOnBoard)
            {
                Debug.LogWarning("⚠ Cannot start combat: No dice on the board!");
                // Show floating text or UI message?
                return;
            }
        }

        IsCombatActive = true;
        GameEvents.RaiseCombatStarted();
        
        // EnemySpawner listens to OnCombatStarted event, no need to call directly
    }

    public void EndCombat()
    {
        Debug.Log("Combat finished. Choosing rewards...");
        IsCombatActive = false;
        GameEvents.RaiseCombatEnded();
        
        // Trigger Reward Phase
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.GenerateRewards();
        }
        else
        {
            FinishCombatPhase();
        }
    }

    public void FinishCombatPhase()
    {
        AdvanceTime();
        Debug.Log("Combat Phase Complete. Time Advanced. Waiting for player to start next wave...");
        
        GameEvents.RaisePreparationPhaseStarted();
        
        // Auto-start removed. Player must manually start.
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
}
