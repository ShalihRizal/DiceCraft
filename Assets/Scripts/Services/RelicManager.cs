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

    public void AddRelic(RelicData relic)
    {
        if (!collectedRelics.Contains(relic))
        {
            collectedRelics.Add(relic);
            Debug.Log($"💎 Relic Acquired: {relic.relicName}");
            
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
            if (relic.effectType == RelicEffectType.OnKill)
            {
                ApplyEffect(relic);
            }
        }
    }

    private void ApplyEffect(RelicData relic)
    {
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
            if (relic.effectType == RelicEffectType.StatBoost && relic.relicName.Contains("Ring"))
            {
                multiplier += relic.effectValue;
            }
        }
        return multiplier;
    }
}
