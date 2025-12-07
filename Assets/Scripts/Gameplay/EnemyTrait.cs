using UnityEngine;

/// <summary>
/// Defines when an enemy trait should trigger
/// </summary>
public enum EnemyEventTrigger
{
    OnSpawned,
    OnDamaged,
    On50Percent,
    On25Percent,
    On1Percent,
    OnDeath,
    OnBuffed,
    OnShielded,
    OnAttack
}

/// <summary>
/// Base class for all enemy traits. Override ExecuteEffect to define behavior.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Trait", menuName = "Game/Enemy/Trait")]
public class EnemyTrait : ScriptableObject
{
    [Header("Trait Info")]
    public string traitName = "New Trait";
    [TextArea(2, 4)]
    public string description = "Describe what this trait does.";
    public Sprite icon;

    [Header("Trigger")]
    public EnemyEventTrigger triggerEvent;

    /// <summary>
    /// Execute the trait's effect. Override this in derived classes.
    /// </summary>
    public virtual void ExecuteEffect(Enemy owner)
    {
        Debug.Log($"[{traitName}] triggered on {owner.name}");
    }

    /// <summary>
    /// Called when the trait is first applied to an enemy
    /// </summary>
    public virtual void OnApplied(Enemy owner)
    {
        // Override for initialization logic
    }

    public virtual string GetDescription()
    {
        string processedDesc = description;
        
        // Simple Regex to find {FieldName} or {FieldName:Format}
        // Matches {VariableName} or {VariableName:F2}
        var matches = System.Text.RegularExpressions.Regex.Matches(processedDesc, @"\{(\w+)(?::([^\}]+))?\}");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            string fullTag = match.Value;          // {health}
            string fieldName = match.Groups[1].Value; // health
            string format = match.Groups[2].Success ? match.Groups[2].Value : null; // F2 (optional)

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
                // Try Property if field not found
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
        
        // Also support user's previous <tag> style by attempting to match field names case-insensitively if needed, 
        // but {Braces} is cleaner for formatting support. 
        // Let's stick to {Braces} as the "New Standard" and update the existing ones.

        return processedDesc;
    }
}
