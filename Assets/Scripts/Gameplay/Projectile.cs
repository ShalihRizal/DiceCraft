using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileOwner
{
    Enemy,
    Player
}

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public ProjectileOwner owner = ProjectileOwner.Enemy;
    public float speed = 5f;
    public float damage = 1f;
    
    [Header("Homing (Player Only)")]
    public bool isHoming = false;
    public float turnSpeed = 360f;
    public Enemy target;

    [Header("Visuals")]
    public GameObject hitEffectPrefab;

    public Vector3 direction = Vector3.up;
    public bool validToDamage = true;

    void Update()
    {
        if (owner == ProjectileOwner.Player && isHoming)
        {
            HandleHomingMovement();
        }
        else
        {
            HandleStraightMovement();
        }

        // Simple bounds check to return to pool (disable) instead of destroy
        // Increased to 20f to ensure it goes off-screen before disappearing
        if (Mathf.Abs(transform.position.y) > 20f || Mathf.Abs(transform.position.x) > 20f)
        {
            gameObject.SetActive(false);
        }
    }

    void HandleStraightMovement()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void HandleHomingMovement()
    {
        if (target == null)
        {
            // If target dies, continue straight or disable? 
            // Let's continue straight for now or destroy.
            // Destroy(gameObject); // Or return to pool
            gameObject.SetActive(false);
            return;
        }

        // Rotate towards target
        Vector3 dir = (target.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Move forward
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!validToDamage) return;

        // Ignore collision with other Projectiles (Player's attacks)
        if (other.GetComponent<Projectile>() != null) return;

        if (owner == ProjectileOwner.Enemy)
        {
            HandleEnemyProjectileCollision(other);
        }
        else if (owner == ProjectileOwner.Player)
        {
            HandlePlayerProjectileCollision(other);
        }
    }

    void HandleEnemyProjectileCollision(Collider2D other)
    {
        // Ignore collision with Enemies (check parent too in case collider is on child)
        if (other.GetComponentInParent<Enemy>() != null) return;

        Debug.Log($"Enemy Projectile hit: {other.name} (Tag: {other.tag})");

        // Try to find PlayerHealth on the object or its parent
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player == null) player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            Debug.Log($"Projectile dealing {damage} damage to Player (via {other.name}).");
            player.TakeDamage(damage);
            gameObject.SetActive(false); // Return to pool
        }
    }

    void HandlePlayerProjectileCollision(Collider2D other)
    {
        // Ignore collision with Player
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerHealth>() != null) return;

        Enemy hitEnemy = other.GetComponent<Enemy>();
        if (hitEnemy == null) hitEnemy = other.GetComponentInParent<Enemy>();

        if (hitEnemy != null)
        {
            hitEnemy.TakeDamage(damage);

            // 💥 Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            gameObject.SetActive(false); // Return to pool (or destroy if instantiated)
             // Note: If instantiated, SetActive(false) won't destroy it. 
             // But ObjectPooler handles SetActive(false). 
             // If Dice instantiates directly, we might need to Destroy.
             // However, Dice.cs currently Instantiates. We should probably use ObjectPooler for Dice too eventually,
             // but for now, let's check if it came from pool? 
             // Actually, if we just SetActive(false), it will leak if not pooled.
             // But let's assume we want to Destroy if not pooled? 
             // For now, let's just Destroy if it's a Player projectile instantiated by Dice (which doesn't use pool yet).
             // Wait, Dice.cs DOES NOT use pool. So we must Destroy.
             Destroy(gameObject); 
        }
    }
}
