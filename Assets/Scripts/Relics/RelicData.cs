using UnityEngine;

public enum RelicRarity
{
    Common,
    Rare,
    Legendary
}

public enum RelicEffectType
{
    None,
    StatBoost,
    OnCombatStart,
    OnTurnStart,
    OnKill,
    OnDiceMerge
}

[CreateAssetMenu(fileName = "New Relic", menuName = "DiceCraft/Relic Data")]
public class RelicData : ScriptableObject
{
    public string relicName;
    [TextArea] public string description;
    public Sprite icon;
    public RelicRarity rarity;
    public int cost = 100;
    
    [Header("Effect Logic")]
    public RelicEffectType effectType;
    public float effectValue; // Generic value for the effect (e.g., +10% damage, +5 gold)

    public string GetDescription()
    {
        string processedDesc = description;
        
        // Matches {VariableName} or {VariableName:F2}
        var matches = System.Text.RegularExpressions.Regex.Matches(processedDesc, @"\{(\w+)(?::([^\}]+))?\}");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            string fullTag = match.Value;
            string fieldName = match.Groups[1].Value;
            string format = match.Groups[2].Success ? match.Groups[2].Value : null;

            System.Reflection.FieldInfo field = this.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.IgnoreCase);

            if (field != null)
            {
                object val = field.GetValue(this);
                string replacement = val != null ? val.ToString() : "null";

                if (val is System.Enum)
                {
                    // Prettify Enum: "OnDamaged" -> "On Damaged"
                    replacement = System.Text.RegularExpressions.Regex.Replace(val.ToString(), "(\\B[A-Z])", " $1");
                }
                else if (!string.IsNullOrEmpty(format) && val is System.IFormattable formattable)
                {
                    replacement = formattable.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
                }

                processedDesc = processedDesc.Replace(fullTag, replacement);
            }
            else
            {
                // Try Property
                System.Reflection.PropertyInfo prop = this.GetType().GetProperty(fieldName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.IgnoreCase);

                if (prop != null)
                {
                    object val = prop.GetValue(this);
                    string replacement = val != null ? val.ToString() : "null";
                    
                    if (!string.IsNullOrEmpty(format) && val is System.IFormattable formattable)
                    {
                        replacement = formattable.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    processedDesc = processedDesc.Replace(fullTag, replacement);
                }
            }
        }

        return processedDesc;
    }
}
