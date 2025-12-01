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

        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (healthUI != null) 
        {
            healthUI.UpdateHealth(health);
        }
        else
        {

        }

        if (health <= 0f)
        {
            Die();
        }
    }

    public int coinDropAmount = 10;

    public void Die()
    {

        isDead = true;
        GameEvents.RaiseEnemyKilled();
        
        // Drop Coins
        if (PlayerCurrency.Instance != null)
        {
            PlayerCurrency.Instance.AddGold(coinDropAmount);

        }
        
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

            return;
        }

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

                proj = projectile.AddComponent<Projectile>();
            }

            if (proj != null)
            {
                proj.owner = ProjectileOwner.Enemy; // Set owner
                proj.damage = projectileDamage;
                proj.direction = Vector3.down;
                proj.validToDamage = true;

            }
            else
            {

            }
        }
        else
        {

        }
    }
}
