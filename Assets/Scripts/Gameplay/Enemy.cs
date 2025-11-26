using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 3f;
    private float maxHealth;
    private EnemyHealthUI healthUI;

    public GameObject projectilePrefab;
    public float fireInterval = 1f;
    public float projectileDamage = 1f;

    private bool isDead = false;
    public bool IsDead => isDead;

    private float timer;

    void Start()
    {
        maxHealth = health;
        healthUI = gameObject.AddComponent<EnemyHealthUI>();
        healthUI.Setup(maxHealth);

        if (ObjectPooler.Instance != null && projectilePrefab != null)
        {
            // Use the prefab name as the unique pool tag
            string poolTag = projectilePrefab.name;
            
            if (!ObjectPooler.Instance.poolDictionary.ContainsKey(poolTag))
            {
                ObjectPooler.Instance.CreatePool(poolTag, projectilePrefab, 10);
            }
        }
        else if (projectilePrefab == null)
        {
             Debug.LogError($"Enemy {gameObject.name}: Projectile Prefab is null!");
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"Enemy Took Damage: {amount}. Current Health: {health}");
        if (healthUI != null) 
        {
            healthUI.UpdateHealth(health);
        }
        else
        {
            Debug.LogWarning("Enemy: HealthUI is null!");
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"💀 {gameObject.name} died!");
        isDead = true;
        
        // Notify spawner for wave progress
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }
        
        EnemySpawner.UnregisterEnemy(this);
        Destroy(gameObject);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireInterval)
        {
            ShootAtPlayer();
            timer = 0f;
        }
    }

    void ShootAtPlayer()
    {
        if (isDead || projectilePrefab == null) 
        {
            if (projectilePrefab == null) Debug.LogWarning($"Enemy {gameObject.name}: Projectile Prefab is null!");
            return;
        }

        if (ObjectPooler.Instance == null)
        {
            Debug.LogWarning("ObjectPooler instance is null! Cannot spawn projectile.");
            return;
        }

        Debug.Log($"Enemy {gameObject.name} shooting at player.");
        // Spawn slightly below the enemy to avoid immediate collision
        Vector3 spawnPos = transform.position + Vector3.down * 1.0f; 
        
        // Use the prefab name as the tag
        string poolTag = projectilePrefab.name;
        GameObject projectile = ObjectPooler.Instance.SpawnFromPool(poolTag, spawnPos, Quaternion.identity);

        if (projectile != null)
        {
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj == null)
            {
                Debug.LogWarning($"Spawned object {projectile.name} missing Projectile component. Adding it dynamically.");
                proj = projectile.AddComponent<Projectile>();
            }

            if (proj != null)
            {
                proj.owner = ProjectileOwner.Enemy; // Set owner
                proj.damage = projectileDamage;
                proj.direction = Vector3.down;
                proj.validToDamage = true;
                Debug.Log($"Projectile spawned and configured. Damage: {proj.damage}");
            }
            else
            {
                Debug.LogError("Failed to add Projectile component!");
            }
        }
        else
        {
             Debug.LogError("Failed to spawn projectile from pool 'EnemyProjectile'.");
        }
    }
}
