using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    public TextMeshProUGUI currencyText;

    void Start()
    {
        UpdateCurrencyDisplay();
        GameEvents.OnCurrencyChanged += UpdateCurrencyDisplay;
    }

    void OnDestroy()
    {
        GameEvents.OnCurrencyChanged -= UpdateCurrencyDisplay;
    }

    void UpdateCurrencyDisplay(int amount)
    {
        if (currencyText != null)
        {
            currencyText.text = $"Gold: {amount}";
        }
    }

    void UpdateCurrencyDisplay()
    {
        if (PlayerCurrency.Instance != null && currencyText != null)
        {
            currencyText.text = $"Gold: {PlayerCurrency.Instance.CurrentGold}";
        }
    }
}
