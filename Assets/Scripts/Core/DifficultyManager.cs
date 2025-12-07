using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages difficulty scaling based on nodes completed (inspired by Genshin Impact's level scaling)
/// Uses level-based multipliers similar to Genshin's system
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    [Header("Level System")]
    [Tooltip("Current enemy level (increases per node)")]
    public int currentLevel = 1;
    
    [Tooltip("Levels gained per node completed")]
    public int levelsPerNode = 2;

    [Header("Multiplier Tables")]
    [Tooltip("HP multiplier curve by level")]
    public AnimationCurve hpMultiplierCurve;
    
    [Tooltip("ATK multiplier curve by level")]
    public AnimationCurve atkMultiplierCurve;

    [Header("Special Multipliers")]
    [Tooltip("Elite enemy HP multiplier")]
    public float eliteHPMultiplier = 2.0f;
    
    [Tooltip("Elite enemy ATK multiplier")]
    public float eliteATKMultiplier = 1.5f;
    
    [Tooltip("Boss enemy HP multiplier")]
    public float bossHPMultiplier = 5.0f;
    
    [Tooltip("Boss enemy ATK multiplier")]
    public float bossATKMultiplier = 2.0f;

    [Header("Current Multipliers (Read-Only)")]
    public float currentHPMultiplier = 1f;
    public float currentATKMultiplier = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultCurves();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDefaultCurves()
    {
        // Create default curves if not set
        if (hpMultiplierCurve == null || hpMultiplierCurve.keys.Length == 0)
        {
            hpMultiplierCurve = new AnimationCurve();
            // Genshin-like HP scaling: exponential growth
            hpMultiplierCurve.AddKey(1, 1.0f);      // Level 1: 1x
            hpMultiplierCurve.AddKey(10, 2.5f);     // Level 10: 2.5x
            hpMultiplierCurve.AddKey(20, 5.0f);     // Level 20: 5x
            hpMultiplierCurve.AddKey(30, 8.0f);     // Level 30: 8x
            hpMultiplierCurve.AddKey(40, 12.0f);    // Level 40: 12x
            hpMultiplierCurve.AddKey(50, 18.0f);    // Level 50: 18x
        }

        if (atkMultiplierCurve == null || atkMultiplierCurve.keys.Length == 0)
        {
            atkMultiplierCurve = new AnimationCurve();
            // ATK scales slower than HP (Genshin-like)
            atkMultiplierCurve.AddKey(1, 1.0f);     // Level 1: 1x
            atkMultiplierCurve.AddKey(10, 1.8f);    // Level 10: 1.8x
            atkMultiplierCurve.AddKey(20, 2.8f);    // Level 20: 2.8x
            atkMultiplierCurve.AddKey(30, 4.0f);    // Level 30: 4x
            atkMultiplierCurve.AddKey(40, 5.5f);    // Level 40: 5.5x
            atkMultiplierCurve.AddKey(50, 7.5f);    // Level 50: 7.5x
        }
    }

    /// <summary>
    /// Call this when a node is completed
    /// </summary>
    public void OnNodeCompleted()
    {
        currentLevel += levelsPerNode;
        RecalculateMultipliers();
        
        Debug.Log($"📈 Enemy Level increased to {currentLevel}!");
        Debug.Log($"   HP Multiplier: x{currentHPMultiplier:F2}, ATK Multiplier: x{currentATKMultiplier:F2}");
    }

    void RecalculateMultipliers()
    {
        // HP Scaling
        if (hpMultiplierCurve != null && hpMultiplierCurve.length > 0)
        {
            float maxTime = hpMultiplierCurve.keys[hpMultiplierCurve.length - 1].time;
            
            if (currentLevel <= maxTime)
            {
                currentHPMultiplier = hpMultiplierCurve.Evaluate(currentLevel);
            }
            else
            {
                // Extrapolate linearly based on the last two keys
                Keyframe lastKey = hpMultiplierCurve.keys[hpMultiplierCurve.length - 1];
                Keyframe secondLastKey = hpMultiplierCurve.keys[hpMultiplierCurve.length - 2];
                
                float slope = (lastKey.value - secondLastKey.value) / (lastKey.time - secondLastKey.time);
                float timeDiff = currentLevel - lastKey.time;
                
                currentHPMultiplier = lastKey.value + (slope * timeDiff);
            }
        }
        else
        {
            currentHPMultiplier = 1f;
        }

        // ATK Scaling
        if (atkMultiplierCurve != null && atkMultiplierCurve.length > 0)
        {
            float maxTime = atkMultiplierCurve.keys[atkMultiplierCurve.length - 1].time;
            
            if (currentLevel <= maxTime)
            {
                currentATKMultiplier = atkMultiplierCurve.Evaluate(currentLevel);
            }
            else
            {
                // Extrapolate linearly
                Keyframe lastKey = atkMultiplierCurve.keys[atkMultiplierCurve.length - 1];
                Keyframe secondLastKey = atkMultiplierCurve.keys[atkMultiplierCurve.length - 2];
                
                float slope = (lastKey.value - secondLastKey.value) / (lastKey.time - secondLastKey.time);
                float timeDiff = currentLevel - lastKey.time;
                
                currentATKMultiplier = lastKey.value + (slope * timeDiff);
            }
        }
        else
        {
            currentATKMultiplier = 1f;
        }
        
        // Debug curve state if multiplier is seemingly stuck
        if (currentLevel > 1 && currentHPMultiplier == 1f)
        {
             Debug.LogWarning($"⚠️ HP Multiplier is 1.0 at Level {currentLevel}. Curve might be flat or missing keys? Keys: {hpMultiplierCurve?.length}");
        }
    }

    [ContextMenu("Reset Scaling Curves")]
    public void ResetCurves()
    {
        hpMultiplierCurve = new AnimationCurve();
        // Genshin-like HP scaling: exponential growth
        hpMultiplierCurve.AddKey(new Keyframe(1, 1.0f));      // Level 1: 1x
        hpMultiplierCurve.AddKey(new Keyframe(10, 2.5f));     // Level 10: 2.5x
        hpMultiplierCurve.AddKey(new Keyframe(20, 5.0f));     // Level 20: 5x
        hpMultiplierCurve.AddKey(new Keyframe(30, 8.0f));     // Level 30: 8x
        hpMultiplierCurve.AddKey(new Keyframe(40, 12.0f));    // Level 40: 12x
        hpMultiplierCurve.AddKey(new Keyframe(50, 18.0f));    // Level 50: 18x
        
        atkMultiplierCurve = new AnimationCurve();
        // ATK scales slower than HP
        atkMultiplierCurve.AddKey(new Keyframe(1, 1.0f));     // Level 1: 1x
        atkMultiplierCurve.AddKey(new Keyframe(10, 1.8f));    // Level 10: 1.8x
        atkMultiplierCurve.AddKey(new Keyframe(20, 2.8f));    // Level 20: 2.8x
        atkMultiplierCurve.AddKey(new Keyframe(30, 4.0f));    // Level 30: 4x
        atkMultiplierCurve.AddKey(new Keyframe(40, 5.5f));    // Level 40: 5.5x
        atkMultiplierCurve.AddKey(new Keyframe(50, 7.5f));    // Level 50: 7.5x
        
        Debug.Log("✅ Scaling Curves have been reset to default values!");
        RecalculateMultipliers();
    }

    /// <summary>
    /// Apply difficulty scaling to enemy (Genshin-like formula)
    /// </summary>
    public void ScaleEnemy(Enemy enemy)
    {
        if (enemy == null || enemy.enemyData == null) return;

        EnemyData data = enemy.enemyData;

        // Get base multipliers from level
        float hpMult = currentHPMultiplier;
        float atkMult = currentATKMultiplier;

        // Apply type-specific multipliers (like Genshin's special multipliers)
        switch (data.enemyType)
        {
            case EnemyType.Elite:
                hpMult *= eliteHPMultiplier;
                atkMult *= eliteATKMultiplier;
                break;
            case EnemyType.Boss:
                hpMult *= bossHPMultiplier;
                atkMult *= bossATKMultiplier;
                break;
        }

        // Apply scaling (Genshin formula: Real Stat = Base Stat × Level Multiplier × Type Multiplier)
        enemy.health *= hpMult;
        enemy.projectileDamage *= atkMult;
        
        // Attack speed doesn't scale in Genshin, but we can keep it for variety
        // enemy.fireInterval /= (1f + (currentLevel * 0.01f)); // Optional: 1% faster per level
    }

    /// <summary>
    /// Get scaled stats for display purposes
    /// </summary>
    public float GetScaledHP(float baseHP, EnemyType type = EnemyType.Normal)
    {
        float mult = currentHPMultiplier;
        
        switch (type)
        {
            case EnemyType.Elite:
                mult *= eliteHPMultiplier;
                break;
            case EnemyType.Boss:
                mult *= bossHPMultiplier;
                break;
        }
        
        return baseHP * mult;
    }

    public float GetScaledDamage(float baseDamage, EnemyType type = EnemyType.Normal)
    {
        float mult = currentATKMultiplier;
        
        switch (type)
        {
            case EnemyType.Elite:
                mult *= eliteATKMultiplier;
                break;
            case EnemyType.Boss:
                mult *= bossATKMultiplier;
                break;
        }
        
        return baseDamage * mult;
    }

    public float GetScaledAttackInterval(float baseInterval)
    {
        // Attack speed doesn't scale (Genshin-like)
        return baseInterval;
    }

    /// <summary>
    /// Reset difficulty (for new runs)
    /// </summary>
    public void ResetDifficulty()
    {
        currentLevel = 1;
        RecalculateMultipliers();
        Debug.Log("🔄 Difficulty reset to Level 1!");
    }

    /// <summary>
    /// Get current level for display
    /// </summary>
    public string GetLevelDisplay()
    {
        return $"Lv.{currentLevel}";
    }
}
