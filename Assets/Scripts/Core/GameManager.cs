using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool IsCombatActive { get; set; } = false;

    public int currentDay { get; private set; } = 1;
    public int currentHour { get; private set; } = 1;
    private const int maxHoursPerDay = 10;

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
        Debug.Log("Combat Phase Complete. Time Advanced.");
    }

    private void HandleDiceMerged(Dice owner, Dice mergedInto)
    {
        Debug.Log($"🧬 Dice merged: {owner.name} → {mergedInto.name}");

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
        Debug.Log($"📊 Global merge tracker: {owner.name} merged into {mergedInto.name}");
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
