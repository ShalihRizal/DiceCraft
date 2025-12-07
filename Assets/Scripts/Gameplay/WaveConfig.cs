using UnityEngine;

[System.Serializable]
public struct WaveConfig
{
    [Tooltip("Legacy: Enemy prefab (deprecated, use enemyData instead)")]
    public GameObject enemyPrefab;
    
    [Tooltip("New: Enemy data asset (recommended)")]
    public EnemyData enemyData;
    
    public int count; // Number of enemies to spawn
}
