using UnityEngine;
using UnityEditor;

public class DebugPlayerSetup : MonoBehaviour
{
    [MenuItem("Debug/Check Player Setup")]
    public static void CheckSetup()
    {
        Debug.Log("--- Starting Player Setup Check ---");

        // 1. Find Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No GameObject found with tag 'Player' in the scene!");
            // Try to find by name
            player = GameObject.Find("Player");
            if (player != null)
            {
                Debug.LogWarning($"⚠ Found GameObject named 'Player' but it has tag '{player.tag}'. Please change tag to 'Player'.");
            }
        }
        else
        {
            Debug.Log($"✅ Found Player object: {player.name}");

            // 2. Check PlayerHealth
            if (player.GetComponent<PlayerHealth>() == null)
                Debug.LogError("❌ Player object is missing 'PlayerHealth' component!");
            else
                Debug.Log("✅ Player has 'PlayerHealth' component.");

            // 3. Check Collider
            Collider2D col = player.GetComponent<Collider2D>();
            if (col == null)
                Debug.LogError("❌ Player object is missing a Collider2D (BoxCollider2D, CircleCollider2D, etc.)!");
            else
                Debug.Log($"✅ Player has Collider2D: {col.GetType().Name} (IsTrigger: {col.isTrigger})");

            // 4. Check Rigidbody
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb == null)
                Debug.LogWarning("⚠ Player object has no Rigidbody2D. Ensure Projectile has one, or Physics won't register collision!");
            else
                Debug.Log($"✅ Player has Rigidbody2D (BodyType: {rb.bodyType})");
        }

        // 5. Check Projectile Prefab via ObjectPooler
        if (ObjectPooler.Instance != null)
        {
            if (ObjectPooler.Instance.poolDictionary.ContainsKey("EnemyProjectile"))
            {
                // We can't easily peek into the Queue without dequeuing, but we can check the pool config list if available
                // Or just check the first active projectile in scene
                Projectile activeProj = FindFirstObjectByType<Projectile>();
                if (activeProj != null)
                {
                    Debug.Log($"--- Checking Active Projectile: {activeProj.name} ---");
                    CheckProjectile(activeProj.gameObject);
                }
                else
                {
                    Debug.Log("ℹ No active projectiles found in scene to check.");
                }
            }
            else
            {
                Debug.LogWarning("⚠ ObjectPooler does not have 'EnemyProjectile' pool ready yet (might need to play game first).");
            }
        }
        else
        {
            Debug.LogWarning("⚠ ObjectPooler Instance is null.");
        }

        Debug.Log("--- End Check ---");
    }

    static void CheckProjectile(GameObject proj)
    {
        if (proj.GetComponent<Collider2D>() == null)
            Debug.LogError("❌ Projectile is missing Collider2D!");
        else
            Debug.Log($"✅ Projectile has Collider2D (IsTrigger: {proj.GetComponent<Collider2D>().isTrigger})");

        if (proj.GetComponent<Rigidbody2D>() == null)
            Debug.LogWarning("⚠ Projectile has no Rigidbody2D. If Player also has no Rigidbody2D, collision will FAIL.");
        else
            Debug.Log("✅ Projectile has Rigidbody2D.");
    }
}
