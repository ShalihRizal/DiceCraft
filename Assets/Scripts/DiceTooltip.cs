using TMPro;
using UnityEngine;

public class DiceTooltip : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI sidesText;

    public RectTransform rectTransform;

    public void SetInfo(Dice dice)
    {
        var data = dice.diceData;
        var runtime = dice.runtimeStats;

        nameText.text = data.diceName;
        fireRateText.text = $"Fire Rate: {(data.baseFireInterval):0.00}s";
        damageText.text = $"Damage per side: {data.baseDamage}";
        sidesText.text = $"Sides: {data.sides}";
    }

    void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        rectTransform.position = screenPos + new Vector3(0, 50f); // offset above
    }
}
