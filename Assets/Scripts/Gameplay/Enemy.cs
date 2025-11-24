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
        if (isDead || projectilePrefab == null) return;

        if (ObjectPooler.Instance == null)
        {
            Debug.LogWarning("ObjectPooler instance is null! Cannot spawn projectile.");
            return;
        }

        GameObject projectile = ObjectPooler.Instance.SpawnFromPool("EnemyProjectile", transform.position, Quaternion.identity);

        if (projectile != null)
        {
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.damage = projectileDamage;
                proj.direction = Vector3.down;
                proj.validToDamage = true;
            }
        }
    }
}
