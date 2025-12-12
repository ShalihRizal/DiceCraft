using UnityEngine;
using System.Collections;

[System.Flags]
public enum PassiveTriggerType
{
    None = 0,
    OnDiceSpawn = 1 << 0,
    OnDiceFire = 1 << 1,
    OnDiceRemoved = 1 << 2,
    OnEnemyHit = 1 << 3,
    OnEnemyKilled = 1 << 4,
    OnDiceMerged = 1 << 5,
    OnCombatStart = 1 << 6,
    OnCombatEnd = 1 << 7
}

[CreateAssetMenu(fileName = "New Dice Passive", menuName = "Dice/Passives/New Passive")]
public class DicePassive : ScriptableObject
{
    [Header("General Info")]
    public string passiveName = "New Passive";
    [TextArea(2, 5)]
    public string description = "Describe what this passive does.";
    public Sprite icon;

    public virtual string GetDescription()
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

    [Header("Trigger Settings")]
    public PassiveTriggerType triggerEvents = PassiveTriggerType.OnDiceFire;

    [Header("Optional Settings")]
    public float duration = 0f;
    public float cooldown = 0f;

    // =========================
    // 🔹 Core Dice Lifecycle
    // =========================
    public virtual void OnDiceSpawn(Dice owner, RuntimeDiceData stats)
    {
        DebugTrigger(owner, "OnDiceSpawn");
    }

    public virtual void OnDiceFire(Dice owner, ref float damage, ref bool skipProjectile)
    {
        DebugTrigger(owner, "OnDiceFire");
    }

    public virtual void OnDiceRemoved(Dice owner)
    {
        DebugTrigger(owner, "OnDiceRemoved");
    }

    // =========================
    // 🔹 Extended Gameplay Events
    // =========================
    public virtual void OnEnemyHit(Dice owner, Enemy enemy, ref float damageDealt)
    {
        DebugTrigger(owner, "OnEnemyHit");
    }

    public virtual void OnEnemyKilled(Dice owner, Enemy enemy)
    {
        DebugTrigger(owner, "OnEnemyKilled");
    }

    public virtual void OnDiceMerged(Dice owner, Dice mergedInto)
    {
        DebugTrigger(owner, "OnDiceMerged");
    }

    public virtual void OnCombatStart(Dice owner)
    {
        DebugTrigger(owner, "OnCombatStart");
    }

    public virtual void OnCombatEnd(Dice owner)
    {
        DebugTrigger(owner, "OnCombatEnd");
    }

    public virtual void OnSwap(Dice thisDice, Dice otherDice)
    {
        // Override in specific passives like Joker
        // Called when this dice is swapped with another dice
    }

    // =========================
    // 🔹 Adjacency Events
    // =========================
    public virtual void OnNeighborFire(Dice owner, Dice neighbor, ref float damage, ref bool skipProjectile)
    {
        // Override to buff neighbor
    }

    public virtual void OnNeighborSpawn(Dice owner, Dice neighbor)
    {
        // Override to react to neighbor spawn
    }

    public virtual void OnNeighborRemoved(Dice owner, Dice neighbor)
    {
        // Override to react to neighbor removal
    }

    public virtual void OnNeighborHit(Dice owner, Enemy enemy, ref float damageDealt)
    {
        // Override to react when a neighbor hits an enemy
        // Useful for passives that trigger on ally attacks
    }

    public virtual System.Collections.Generic.List<Dice> GetAffectedNeighbors(Dice owner)
    {
        // Default: Affects NO neighbors (Override in specific passives like Anemo)
        return new System.Collections.Generic.List<Dice>();
    }

    // =========================
    // 🔹 Tooltip Projection
    // =========================
    /// <summary>
    /// Returns the damage multiplier this passive would apply for tooltip display.
    /// Override this in passives that modify damage (e.g., FirePassive).
    /// </summary>
    public virtual float GetProjectedDamageMultiplier(Dice owner)
    {
        return 1f; // Default: no modification
    }

    // =========================
    // 🔹 Utility Helpers
    // =========================
    public void DebugTrigger(Dice owner, string triggerName)
    {
        if (triggerEvents.HasFlag(ParseTrigger(triggerName)))
        {
            Debug.Log($"🧩 Passive Triggered: {passiveName} on {owner.name} → {triggerName}");
        }
    }

    private PassiveTriggerType ParseTrigger(string triggerName)
    {
        return triggerName switch
        {
            "OnDiceSpawn" => PassiveTriggerType.OnDiceSpawn,
            "OnDiceFire" => PassiveTriggerType.OnDiceFire,
            "OnDiceRemoved" => PassiveTriggerType.OnDiceRemoved,
            "OnEnemyHit" => PassiveTriggerType.OnEnemyHit,
            "OnEnemyKilled" => PassiveTriggerType.OnEnemyKilled,
            "OnDiceMerged" => PassiveTriggerType.OnDiceMerged,
            "OnCombatStart" => PassiveTriggerType.OnCombatStart,
            "OnCombatEnd" => PassiveTriggerType.OnCombatEnd,
            _ => PassiveTriggerType.None
        };
    }

    protected IEnumerator TemporaryModifier(float duration, System.Action apply, System.Action revert)
    {
        apply?.Invoke();
        yield return new WaitForSeconds(duration);
        revert?.Invoke();
    }

    protected void Log(Dice owner, string msg)
    {
        Debug.Log($"[{passiveName}] ({owner.name}): {msg}");
    }
}
