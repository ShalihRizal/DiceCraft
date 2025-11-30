using System;
using UnityEngine;

[Serializable]
public class AchievementReward
{
    public enum RewardType
    {
        Coin,
        DicePip
    }

    public RewardType type;
    public int amount;
    public Sprite icon; // Optional custom icon, otherwise use default based on type
}
