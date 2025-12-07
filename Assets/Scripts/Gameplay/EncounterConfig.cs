using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines an encounter configuration for a combat node
/// </summary>
[CreateAssetMenu(fileName = "New Encounter", menuName = "Game/Encounter Config")]
public class EncounterConfig : ScriptableObject
{
    [Header("Encounter Info")]
    public string encounterName = "Basic Encounter";
    
    [Header("Enemy Composition")]
    [Tooltip("List of enemies to spawn in this encounter")]
    public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();
    
    [Header("Spawn Settings")]
    [Tooltip("Delay between spawning each enemy (0 = instant)")]
    public float spawnDelay = 0.2f;
    
    [Tooltip("Spawn area bounds")]
    public Vector2 spawnAreaMin = new Vector2(-2.5f, 3.5f);
    public Vector2 spawnAreaMax = new Vector2(2.5f, 4.5f);
    
    [Tooltip("Minimum distance between spawned enemies")]
    public float minSpawnDistance = 1.2f;
}

[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Enemy data to spawn")]
    public EnemyData enemyData;
    
    [Tooltip("Number of this enemy type to spawn")]
    public int count = 1;
    
    [Tooltip("Optional: Specific spawn position (leave at 0,0 for random)")]
    public Vector2 specificPosition;
    
    [Tooltip("Use specific position instead of random")]
    public bool useSpecificPosition = false;
}
