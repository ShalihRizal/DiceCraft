using UnityEngine;

[System.Serializable]
public class RuntimeDiceData
{
    public DiceData baseData;

    public float fireInterval;
    public float baseDamage;
    public int diceSides;
    public int cost;
    public string diceName;
    public int upgradeLevel = 0;

    public float luck;

    public float critChance;
    public float multicastChance;

    public RuntimeDiceData(DiceData data)
    {
        baseData = data;
        fireInterval = data.baseFireInterval;
        baseDamage = data.baseDamage;
        diceSides = data.sides;
        cost = data.cost;
        diceName = data.diceName;

        // assign to the instance field (previous code shadowed 'luck')
        luck = data.luck / 100f;
        critChance = data.diceCritChance;
        multicastChance = Mathf.Clamp01(luck * 0.25f);
        
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        if (baseData == null) return;

        // Base Stats + (Growth * Level)
        baseDamage = baseData.baseDamage + (baseData.growthDamage * upgradeLevel);
        
        // Fire Interval decreases with level (faster), clamped to 0.1s minimum
        fireInterval = Mathf.Max(0.1f, baseData.baseFireInterval - (baseData.growthFireRate * upgradeLevel));
    }
}
