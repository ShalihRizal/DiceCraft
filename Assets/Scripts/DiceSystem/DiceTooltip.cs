using UnityEngine;
using TMPro;

public class DiceTooltip : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI sidesText;

    public RectTransform rectTransform;

    public TextMeshProUGUI passiveNameText;
    public TextMeshProUGUI passiveDescText;

    public void SetInfo(Dice dice)
    {
        // ✅ Use Runtime Stats if available to reflect passive buffs
        if (dice.runtimeStats != null)
        {
            SetInfo(dice.runtimeStats);
        }
        else
        {
            SetInfo(dice.diceData);
        }
    }

    public void SetInfo(RuntimeDiceData runtime)
    {
        if (runtime == null) return;
        SetInfo(runtime.baseData, runtime.upgradeLevel, runtime);
    }

    public void SetInfo(DiceData data, int level = 1, RuntimeDiceData runtime = null)
    {
        if (data == null)
        {
            return;
        }

        if (nameText != null) nameText.text = $"{data.diceName} (Lv. {level})";
        
        if (fireRateText != null)
        {
            float baseInterval = data.baseFireInterval;
            float currentInterval = (runtime != null && runtime.fireInterval > 0) ? runtime.fireInterval : baseInterval;
            
            // Apply Global Multiplier for display consistency
            if (GameManager.Instance != null)
                currentInterval /= (1f + GameManager.Instance.globalFireRateMultiplier);

            currentInterval = Mathf.Round(currentInterval * 100f) / 100f;
            fireRateText.text = $"Fire Rate: {currentInterval}s";
            fireRateText.gameObject.SetActive(true);
        }

        if (damageText != null)
        {
            if (data.passive is HealPassive healPassive)
            {
                damageText.text = $"Heal: {healPassive.healAmount}";
                damageText.color = Color.green;
                damageText.gameObject.SetActive(true);
            }
            else if (data.passive is IcePassive icePassive)
            {
                damageText.text = $"Shield: {icePassive.shieldAmount}";
                damageText.color = new Color(0.4f, 0.6f, 1f);
                damageText.gameObject.SetActive(true);
            }
            else if (data.canAttack)
            {
                // Use runtime value if available
                float baseDmg = data.baseDamage;
                float currentDmg = (runtime != null) ? runtime.baseDamage : baseDmg;
                
                float effectiveDmg = currentDmg;

                // Apply Global Multiplier
                if (GameManager.Instance != null)
                    effectiveDmg *= (1f + GameManager.Instance.globalDamageMultiplier);

                // Apply Relic Multiplier
                if (RelicManager.Instance != null)
                    effectiveDmg *= RelicManager.Instance.GetDamageMultiplier();

                // Apply Passive Damage Modifiers (Generic - works for any passive)
                if (data.passive != null && runtime != null)
                {
                    // Find the dice instance to pass to the passive
                    Dice[] allDice = FindObjectsByType<Dice>(FindObjectsSortMode.None);
                    Dice ownerDice = null;
                    foreach (var dice in allDice)
                    {
                        if (dice.runtimeStats == runtime)
                        {
                            ownerDice = dice;
                            break;
                        }
                    }

                    if (ownerDice != null)
                    {
                        float passiveMultiplier = data.passive.GetProjectedDamageMultiplier(ownerDice);
                        effectiveDmg *= passiveMultiplier;
                    }
                }

                effectiveDmg = Mathf.Round(effectiveDmg * 10f) / 10f;

                damageText.text = $"Damage: {effectiveDmg}";
                
                if (effectiveDmg > baseDmg) damageText.color = Color.cyan;
                else if (effectiveDmg < baseDmg) damageText.color = new Color(1f, 0.4f, 0.4f);
                else damageText.color = Color.white;

                damageText.gameObject.SetActive(true);
            }
            else
            {
                damageText.gameObject.SetActive(false);
            }
        }

        if (sidesText != null)
        {
            sidesText.text = $"Sides: {data.sides}";
            sidesText.gameObject.SetActive(true);
        }

        if (data.passive != null)
        {
            if (passiveNameText != null)
            {
                passiveNameText.text = data.passive.passiveName;
                passiveNameText.gameObject.SetActive(true);
            }
            if (passiveDescText != null)
            {
                // Use formatted description if we have a dice instance
                Dice[] allDice = FindObjectsByType<Dice>(FindObjectsSortMode.None);
                Dice ownerDice = null;
                foreach (var dice in allDice)
                {
                    if (dice.runtimeStats == runtime || dice.diceData == data)
                    {
                        ownerDice = dice;
                        break;
                    }
                }

                passiveDescText.text = ownerDice != null 
                    ? data.passive.GetFormattedDescription(ownerDice)
                    : data.passive.description;
                passiveDescText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (passiveNameText != null) passiveNameText.gameObject.SetActive(false);
            if (passiveDescText != null) passiveDescText.gameObject.SetActive(false);
        }
    }

    public void SetInfo(RelicData relic)
    {
        if (relic == null) return;

        if (nameText != null) nameText.text = $"{relic.relicName} ({relic.rarity})";
        
        // Hide Dice specific stats
        if (fireRateText != null) fireRateText.gameObject.SetActive(false);
        if (damageText != null) damageText.gameObject.SetActive(false);
        if (sidesText != null) sidesText.gameObject.SetActive(false);

        // Use passive fields for description
        if (passiveNameText != null)
        {
            passiveNameText.text = "Effect";
            passiveNameText.gameObject.SetActive(true);
        }
        if (passiveDescText != null)
        {
            passiveDescText.text = relic.description;
            passiveDescText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Show merge preview with current level and next level stats
    /// </summary>
    public void SetMergePreview(Dice currentDice, int nextLevel)
    {
        if (currentDice == null || currentDice.diceData == null) return;

        DiceData data = currentDice.diceData;
        int currentLevel = currentDice.runtimeStats != null ? currentDice.runtimeStats.upgradeLevel : 1;

        if (nameText != null)
        {
            nameText.text = $"{data.diceName} - Merge Preview";
        }

        // Show current and next level damage
        if (damageText != null && data.canAttack)
        {
            float currentDmg = currentDice.runtimeStats != null ? currentDice.runtimeStats.baseDamage : data.baseDamage;
            float nextDmg = currentDmg; // Same base damage (passives might change)
            
            damageText.text = $"Damage: {currentDmg} → {nextDmg}";
            damageText.color = Color.cyan;
            damageText.gameObject.SetActive(true);
        }

        // Show current and next level fire rate
        if (fireRateText != null)
        {
            float currentInterval = currentDice.runtimeStats != null ? currentDice.runtimeStats.fireInterval : data.baseFireInterval;
            float nextInterval = currentInterval; // Same (unless passive changes it)
            
            fireRateText.text = $"Fire Rate: {currentInterval:F2}s → {nextInterval:F2}s";
            fireRateText.gameObject.SetActive(true);
        }

        if (sidesText != null)
        {
            sidesText.text = $"Sides: {data.sides}";
            sidesText.gameObject.SetActive(true);
        }

        // Show passive upgrade preview
        if (data.passive != null)
        {
            if (passiveNameText != null)
            {
                passiveNameText.text = $"{data.passive.passiveName} (Lv.{currentLevel} → Lv.{nextLevel})";
                passiveNameText.gameObject.SetActive(true);
            }

            if (passiveDescText != null)
            {
                // Create a temporary runtime data for next level preview
                RuntimeDiceData tempRuntime = new RuntimeDiceData(data);
                tempRuntime.upgradeLevel = nextLevel;
                
                // Create a temporary dice for preview
                GameObject tempObj = new GameObject("TempDicePreview");
                Dice tempDice = tempObj.AddComponent<Dice>();
                tempDice.diceData = data;
                tempDice.runtimeStats = tempRuntime;

                string currentDesc = data.passive.GetFormattedDescription(currentDice);
                string nextDesc = data.passive.GetFormattedDescription(tempDice);

                Destroy(tempObj);

                passiveDescText.text = $"Current: {currentDesc}\n<color=#00FF00>Next: {nextDesc}</color>";
                passiveDescText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (passiveNameText != null) passiveNameText.gameObject.SetActive(false);
            if (passiveDescText != null) passiveDescText.gameObject.SetActive(false);
        }
    }
}
