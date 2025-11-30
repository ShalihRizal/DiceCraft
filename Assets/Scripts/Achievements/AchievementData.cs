using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "DiceCraft/Achievement")]
public class AchievementData : ScriptableObject
{
    [Header("General Info")]
    public string id; // Unique ID (e.g., "first_death")
    public string title;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Progress Settings")]
    public bool isSingleTrigger; // If true, no progress bar (0 or 1)
    public int targetValue; // e.g., 10 kills

    [Header("Rewards")]
    public List<AchievementReward> rewards;
}
