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
        var data = dice.diceData;
        var runtime = dice.runtimeStats;
        SetInfo(data, runtime.upgradeLevel);
    }

    public void SetInfo(DiceData data, int level = 1)
    {
        if (data == null)
        {
            Debug.LogWarning("DiceTooltip: Data is null!");
            return;
        }

        if (nameText != null) nameText.text = $"{data.diceName} (Lv. {level})";
        if (fireRateText != null) fireRateText.text = $"Fire Rate: {(data.baseFireInterval):0.00}s";
        if (damageText != null) damageText.text = $"Damage: {data.baseDamage}";
        if (sidesText != null) sidesText.text = $"Sides: {data.sides}";

        if (data.passive != null)
        {
            if (passiveNameText != null)
            {
                passiveNameText.text = data.passive.passiveName;
                passiveNameText.gameObject.SetActive(true);
            }
            if (passiveDescText != null)
            {
                passiveDescText.text = data.passive.description;
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
