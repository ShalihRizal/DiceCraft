using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;

    void Start()
{
    GameEvents.OnCurrencyChanged += UpdateUI;
    UpdateUI(PlayerCurrency.Instance.CurrentGold);
}

void OnDestroy()
{
    GameEvents.OnCurrencyChanged -= UpdateUI;
}

void UpdateUI(int amount)
{
    goldText.text = $"Gold: {amount}";
}

}