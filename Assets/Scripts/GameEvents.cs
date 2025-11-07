using System;
using UnityEngine;

public static class GameEvents
{
    // Combat
    public static event Action OnCombatStarted;
    public static event Action OnCombatEnded;

    // Shop
    public static event Action OnShopOpened;
    public static event Action OnShopClosed;

    // Currency
    public static event Action<int> OnCurrencyChanged;

    public static event Action OnPlayerDied;

    // Dice
    public static event Action<DiceData> OnDiceMerged;

    // Healing
    public static event Action<float> OnOverHeal;

    // Cycle
    public static Action<int, int> OnTimeChanged;

    public static event Action<float> OnGainShield;
public static event Action<float> OnLostShield;

public static void RaiseGainShield(float amount) => OnGainShield?.Invoke(amount);
public static void RaiseLostShield(float amount) => OnLostShield?.Invoke(amount);


    // Methods to call
    public static void RaiseCombatStarted() => OnCombatStarted?.Invoke();
    public static void RaiseCombatEnded() => OnCombatEnded?.Invoke();

    public static void RaiseShopOpened() => OnShopOpened?.Invoke();
    public static void RaiseShopClosed() => OnShopClosed?.Invoke();

    public static void RaiseCurrencyChanged(int amount) => OnCurrencyChanged?.Invoke(amount);

    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();

    public static void RaiseDiceMerged(DiceData diceType) => OnDiceMerged?.Invoke(diceType);

    public static void RaiseOverHeal(float amount) => OnOverHeal?.Invoke(amount);
}
