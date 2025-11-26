using UnityEngine;
using UnityEditor;

public class DebugDiceSetup : MonoBehaviour
{
    [MenuItem("Debug/Check Dice Setup")]
    public static void CheckDice()
    {
        Debug.Log("--- Starting Dice Setup Check ---");

        Dice[] allDice = FindObjectsByType<Dice>(FindObjectsSortMode.None);
        Debug.Log($"Found {allDice.Length} Dice in the scene.");

        foreach (var dice in allDice)
        {
            if (dice.diceData == null)
            {
                Debug.LogError($"❌ Dice '{dice.name}' has NO DiceData assigned!");
                continue;
            }

            Debug.Log($"🎲 Dice '{dice.name}' (Data: {dice.diceData.name})");
            Debug.Log($"   - Can Attack: {dice.diceData.canAttack}");
            
            if (dice.diceData.passive == null)
            {
                Debug.Log($"   - Passive: NONE");
            }
            else
            {
                Debug.Log($"   - Passive: {dice.diceData.passive.name} (Type: {dice.diceData.passive.GetType().Name})");
                
                if (dice.diceData.passive is IcePassive ice)
                {
                    Debug.Log($"     - ❄ Ice Passive detected. Shield Amount: {ice.shieldAmount}");
                }
            }

            if (dice.projectilePrefab == null)
            {
                Debug.LogWarning($"   - ⚠ Projectile Prefab is NULL (OK if passive skips projectile)");
            }
            else
            {
                Debug.Log($"   - Projectile: {dice.projectilePrefab.name}");
            }
        }

        Debug.Log("--- End Dice Check ---");
    }
}
