using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance { get; private set; }

    public List<RelicData> collectedRelics = new List<RelicData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // cleanup nulls from broken references
        collectedRelics.RemoveAll(r => r == null);
        
        // Subscribe to events
        GameEvents.OnCombatStarted += HandleCombatStart;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
        GameEvents.OnDiceMerged += HandleDiceMerged;
    }

    private void OnDestroy()
    {
        GameEvents.OnCombatStarted -= HandleCombatStart;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.OnDiceMerged -= HandleDiceMerged;
    }

    public static event System.Action<RelicData> OnRelicAdded;

    public void AddRelic(RelicData relic)
    {
        if (relic == null) return;

        if (!collectedRelics.Contains(relic))
        {
            collectedRelics.Add(relic);
            Debug.Log($"💎 Relic Acquired: {relic.relicName}");
            
            OnRelicAdded?.Invoke(relic);

            // Apply immediate effects (On Pickup)
            if (relic.relicName.Contains("Potion") || relic.relicName.Contains("Heal"))
            {
                 var playerHealth = FindFirstObjectByType<PlayerHealth>();
                 if (playerHealth != null)
                 {
                     playerHealth.Heal(relic.effectValue);
                     Debug.Log($"Applied immediate healing from {relic.relicName}");
                 }
            }
        }
    }

    private void HandleCombatStart()
    {
        foreach (var relic in collectedRelics)
        {
            if (relic == null) continue;
            if (relic.effectType == RelicEffectType.OnCombatStart)
            {
                ApplyEffect(relic);
            }
        }
    }

    private void HandleEnemyKilled()
    {
        foreach (var relic in collectedRelics)
        {
            if (relic == null) continue;
            if (relic.effectType == RelicEffectType.OnKill)
            {
                ApplyEffect(relic);
            }
        }
    }

    private void ApplyEffect(RelicData relic)
    {
        if (relic == null) return;
        Debug.Log($"⚡ Relic Activated: {relic.relicName}");
        
        switch (relic.effectType)
        {
            case RelicEffectType.OnCombatStart:
                // Example: Lucky Coin (+Gold)
                if (relic.relicName.Contains("Coin"))
                {
                    if (PlayerCurrency.Instance != null)
                        PlayerCurrency.Instance.AddGold((int)relic.effectValue);
                }
                break;

            case RelicEffectType.OnKill:
                // Example: Vampiric Dagger (Heal on Kill)
                if (relic.relicName.Contains("Dagger") || relic.relicName.Contains("Vampiric"))
                {
                    var playerHealth = FindFirstObjectByType<PlayerHealth>();
                    if (playerHealth != null)
                        playerHealth.Heal(relic.effectValue);
                }
                break;
                
            case RelicEffectType.StatBoost:
                // Applied on acquisition or checked dynamically
                // For now, let's assume it's applied when added
                break;
        }
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;
        foreach (var relic in collectedRelics)
        {
            if (relic == null) continue;
            if (relic.effectType == RelicEffectType.StatBoost && relic.relicName.Contains("Ring"))
            {
                multiplier += relic.effectValue;
            }
        }
        return multiplier;
    }

    private void HandleDiceMerged(Dice owner, Dice mergedInto)
    {
        foreach (var relic in collectedRelics)
        {
            if (relic == null) continue;
            if (relic.effectType == RelicEffectType.OnDiceMerge)
            {
                ApplyEffect(relic);
            }
        }
    }

    // Duplicator-specific tracking
    private int duplicatorMergeCount = 0;
    private const int DUPLICATOR_MERGE_THRESHOLD = 6;

    /// <summary>
    /// Increment merge counter for Duplicator relic
    /// </summary>
    public void IncrementDuplicatorCounter()
    {
        if (!HasDuplicator()) return;
        
        duplicatorMergeCount++;
        UpdateDuplicatorUI();
        
        Debug.Log($"🪞 Duplicator progress: {duplicatorMergeCount}/{DUPLICATOR_MERGE_THRESHOLD}");
    }

    /// <summary>
    /// Check if Duplicator is ready to trigger
    /// </summary>
    public bool IsDuplicatorReady()
    {
        return HasDuplicator() && duplicatorMergeCount >= DUPLICATOR_MERGE_THRESHOLD;
    }

    /// <summary>
    /// Reset Duplicator counter (after use or when board+inventory full)
    /// </summary>
    public void ResetDuplicatorCounter()
    {
        duplicatorMergeCount = 0;
        UpdateDuplicatorUI();
    }

    /// <summary>
    /// Check if player has Duplicator relic (not checking if ready)
    /// </summary>
    public bool HasDuplicator()
    {
        foreach (var relic in collectedRelics)
        {
            if (relic == null) continue;
            if (relic.relicName.Contains("Duplicator"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get current progress string for Duplicator
    /// </summary>
    public string GetDuplicatorProgress()
    {
        if (!HasDuplicator()) return "";
        return $"{duplicatorMergeCount}/{DUPLICATOR_MERGE_THRESHOLD}";
    }

    /// <summary>
    /// Update the UI counter text for Duplicator relic
    /// </summary>
    private void UpdateDuplicatorUI()
    {
        // Find all RelicIconUI instances and update the one with Duplicator
        RelicIconUI[] relicIcons = FindObjectsByType<RelicIconUI>(FindObjectsSortMode.None);
        foreach (var icon in relicIcons)
        {
            icon.UpdateCounterText();
        }
    }
}
