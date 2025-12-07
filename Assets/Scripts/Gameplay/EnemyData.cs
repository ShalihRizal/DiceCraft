using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    Normal,
    Elite,
    Boss
}

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public EnemyType enemyType = EnemyType.Normal;
    public GameObject prefab;
    public Sprite icon;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float damage = 1f;
    public float moveSpeed = 2f;
    public float attackInterval = 2f;

    [Header("Traits")]
    public List<EnemyTrait> traits = new List<EnemyTrait>();

    [Header("Loot")]
    public LootTable lootTable;

    [Header("Visual")]
    public Color healthBarColor = Color.red;

    /// <summary>
    /// Get all traits that trigger on a specific event
    /// </summary>
    public List<EnemyTrait> GetTraitsForEvent(EnemyEventTrigger trigger)
    {
        List<EnemyTrait> result = new List<EnemyTrait>();
        foreach (var trait in traits)
        {
            if (trait != null && trait.triggerEvent == trigger)
            {
                result.Add(trait);
            }
        }
        return result;
    }
}
