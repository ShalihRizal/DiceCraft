using UnityEngine;
using System;
using UnityEngine.Events;

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance;

    [SerializeField] private int currentGold = 0;
    public int CurrentGold => currentGold;

    // Optional: for UI to subscribe to changes
    public UnityEvent OnCurrencyChanged = new UnityEvent();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddGold(int amount)
{
    currentGold += amount;
    GameEvents.RaiseCurrencyChanged(currentGold);
}

public bool SpendGold(int amount)
{
    if (currentGold >= amount)
    {
        currentGold -= amount;
        GameEvents.RaiseCurrencyChanged(currentGold);
        return true;
    }
    return false;
}


    public int GetGold()
    {
        return currentGold;
    }
}