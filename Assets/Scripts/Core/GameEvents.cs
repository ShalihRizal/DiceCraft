using System;
using UnityEngine;

public static class GameEvents
{
    // Combat
    public static event Action OnCombatStarted;
    public static event Action OnCombatEnded;

    // Wave Events
    public static event Action<int, int> OnWaveStarted; // waveNumber, totalEnemies
    public static event Action<int, int> OnWaveProgressChanged; // enemiesKilled, totalEnemies

    // Shop
    public static event Action OnShopOpened;
    public static event Action OnShopClosed;

    // Currency
    public static event Action<int> OnCurrencyChanged;

    // Player
    public static event Action OnPlayerDied;
    public static event Action OnGameOver;

    // Dice
    public static event Action<Dice, Dice> OnDiceMerged;

    // Healing
    public static event Action<float> OnOverHeal;

    // Cycle
    public static Action<int, int> OnTimeChanged;

    // Shields
    public static event Action<float> OnGainShield;
    public static event Action<float> OnLostShield;

    public static void RaiseGainShield(float amount) => OnGainShield?.Invoke(amount);
    public static void RaiseLostShield(float amount) => OnLostShield?.Invoke(amount);

    // Methods to call
    public static void RaiseCombatStarted() => OnCombatStarted?.Invoke();
    public static void RaiseCombatEnded() => OnCombatEnded?.Invoke();
    public static event Action OnPreparationPhaseStarted;
    public static void RaisePreparationPhaseStarted() => OnPreparationPhaseStarted?.Invoke();

    public static void RaiseWaveStarted(int waveNumber, int totalEnemies) => OnWaveStarted?.Invoke(waveNumber, totalEnemies);
    public static void RaiseWaveProgressChanged(int enemiesKilled, int totalEnemies) => OnWaveProgressChanged?.Invoke(enemiesKilled, totalEnemies);

    public static void RaiseShopOpened() => OnShopOpened?.Invoke();
    public static void RaiseShopClosed() => OnShopClosed?.Invoke();

    public static void RaiseCurrencyChanged(int amount) => OnCurrencyChanged?.Invoke(amount);

    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
    public static void RaiseGameOver() => OnGameOver?.Invoke();

    public static void RaiseDiceMerged(Dice owner, Dice mergedInto) => OnDiceMerged?.Invoke(owner, mergedInto);

    public static void RaiseOverHeal(float amount) => OnOverHeal?.Invoke(amount);

    // Map
    public static event Action OnMapGenerated;
    public static void RaiseMapGenerated() => OnMapGenerated?.Invoke();
}
