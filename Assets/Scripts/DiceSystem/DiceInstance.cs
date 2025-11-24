using System.Collections;
using UnityEngine;

[System.Serializable]
public class DiceInstance
{
    public DiceData data;

    public float fireInterval;
    public float damage;
    public int sides;

    public DiceInstance(DiceData source)
    {
        data = source;
        fireInterval = source.baseFireInterval;
        damage = source.baseDamage;
        sides = source.sides;
    }
}
