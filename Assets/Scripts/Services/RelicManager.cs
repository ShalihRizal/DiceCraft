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
        // Subscribe to events
        GameEvents.OnCombatStarted += HandleCombatStart;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDestroy()
    {
        GameEvents.OnCombatStarted -= HandleCombatStart;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    public static event System.Action<RelicData> OnRelicAdded;

    public void AddRelic(RelicData relic)
    {
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
                     playerHealth.Heal(relic.GetCurrentValue());
                     Debug.Log($"Applied immediate healing from {relic.relicName}");
                 }
            }
        }
    }

    private void HandleCombatStart()
    {
        ApplyRelicEffects(RelicTrigger.OnCombatStart);
    }

    private void HandleEnemyKilled()
    {
        ApplyRelicEffects(RelicTrigger.OnKill);
    }

    private void ApplyRelicEffects(RelicTrigger trigger)
    {
        foreach (var relic in collectedRelics)
        {
            if (relic.trigger == trigger)
            {
                ApplyEffect(relic);
            }
        }
    }

    private void ApplyEffect(RelicData relic)
    {
        Debug.Log($"⚡ Relic Activated: {relic.relicName} (Level {relic.currentLevel})");
        float value = relic.GetCurrentValue();

        switch (relic.effect)
        {
            case RelicEffect.Heal:
                var playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(value);
                    Debug.Log($"[Relic] Healed {value} HP");
                }
                break;

            case RelicEffect.GainGold:
                if (PlayerCurrency.Instance != null)
                {
                    PlayerCurrency.Instance.AddGold((int)value);
                    Debug.Log($"[Relic] Gained {value} Gold");
                }
                break;
                
            case RelicEffect.MaxHealth:
                 var ph = FindFirstObjectByType<PlayerHealth>();
                 if (ph != null)
                 {
                     // Assuming PlayerHealth has a way to increase max health
                     // ph.IncreaseMaxHealth((int)value); 
                     Debug.Log($"[Relic] Max Health increased by {value} (Not fully implemented in PlayerHealth)");
                 }
                 break;
        }
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;
        foreach (var relic in collectedRelics)
        {
            if (relic.trigger == RelicTrigger.Passive && relic.effect == RelicEffect.DamageMultiplier)
            {
                multiplier += relic.GetCurrentValue();
            }
        }
        return multiplier;
    }
    
    public float GetCritChanceBonus()
    {
        float bonus = 0f;
        foreach (var relic in collectedRelics)
        {
            if (relic.trigger == RelicTrigger.Passive && relic.effect == RelicEffect.CritChance)
            {
                bonus += relic.GetCurrentValue();
            }
        }
        return bonus;
    }

    public void UpgradeRelic(RelicData relic)
    {
        if (collectedRelics.Contains(relic))
        {
            relic.Upgrade();
            Debug.Log($"⬆️ Upgraded {relic.relicName} to Level {relic.currentLevel}");
        }
    }

    /// <summary>
    /// Get the passive boost multiplier for a specific dice type
    /// </summary>
    public float GetDicePassiveBoost(DiceData diceData)
    {
        if (diceData == null) return 0f;
        
        float boost = 0f;
        foreach (var relic in collectedRelics)
        {
            if (relic.trigger == RelicTrigger.Passive && 
                relic.effect == RelicEffect.DicePassiveBoost && 
                relic.targetDiceData == diceData)
            {
                boost += relic.GetCurrentValue();
            }
        }
        return boost;
    }
}
