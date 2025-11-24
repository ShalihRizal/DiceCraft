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

    public virtual System.Collections.Generic.List<Dice> GetAffectedNeighbors(Dice owner)
    {
        // Default: Affects all physical neighbors
        return owner.GetNeighbors();
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
