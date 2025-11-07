
using TMPro;
using UnityEngine;

public class DayHourUI : MonoBehaviour
{
    public TextMeshProUGUI dayHourText;

    void Update()
    {
        if (GameManager.Instance != null)
        {
            int day = GameManager.Instance.currentDay;
            int hour = GameManager.Instance.currentHour;
            dayHourText.text = $"Day {day}, Hour {hour}";
        }
    }
}
