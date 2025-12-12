using UnityEngine;

/// <summary>
/// Utility class for formatting numbers in a human-readable way
/// </summary>
public static class NumberFormatter
{
    /// <summary>
    /// Format a number with K, M, B, T suffixes
    /// Examples: 1000 -> 1K, 1500000 -> 1.5M, 1000000000 -> 1B
    /// </summary>
    public static string FormatNumber(float number)
    {
        if (number < 1000)
            return Mathf.RoundToInt(number).ToString();

        if (number < 1000000) // Thousands (K)
            return (number / 1000f).ToString("0.#") + "K";

        if (number < 1000000000) // Millions (M)
            return (number / 1000000f).ToString("0.#") + "M";

        if (number < 1000000000000) // Billions (B)
            return (number / 1000000000f).ToString("0.#") + "B";

        // Trillions (T)
        return (number / 1000000000000f).ToString("0.#") + "T";
    }

    /// <summary>
    /// Format HP specifically (always shows at least 1 decimal for clarity)
    /// </summary>
    public static string FormatHP(float hp)
    {
        if (hp < 1000)
            return Mathf.RoundToInt(hp).ToString();

        if (hp < 1000000) // Thousands (K)
            return (hp / 1000f).ToString("0.0") + "K";

        if (hp < 1000000000) // Millions (M)
            return (hp / 1000000f).ToString("0.0") + "M";

        if (hp < 1000000000000) // Billions (B)
            return (hp / 1000000000f).ToString("0.0") + "B";

        // Trillions (T)
        return (hp / 1000000000000f).ToString("0.0") + "T";
    }
}
