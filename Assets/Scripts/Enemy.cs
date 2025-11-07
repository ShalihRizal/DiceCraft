using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 3f;

    public GameObject projectilePrefab;
    public float fireInterval = 1f;
    public float projectileDamage = 1f;

    private bool isDead = false;
    public bool IsDead => isDead; // optional public getter if needed elsewhere


    private float timer;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        if (EnemySpawner.activeEnemies.Contains(this))
            EnemySpawner.activeEnemies.Remove(this);

        FindObjectOfType<EnemySpawner>()?.NotifyEnemyDefeated();
        Destroy(gameObject);
    }


    void Update()
    {
        // Debug.Log("Enemy Update: " + gameObject.name);

        timer += Time.deltaTime;

        if (timer >= fireInterval)
        {
            ShootAtPlayer();
            timer = 0f;
        }
    }

    void ShootAtPlayer()
    {
        if (isDead || projectilePrefab == null) return; // ✅ Prevent firing if dead

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.damage = projectileDamage;
            proj.direction = Vector3.down;
            proj.validToDamage = true;
        }
    }





}
