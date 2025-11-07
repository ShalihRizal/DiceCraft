using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public float damage = 1f;

    public Vector3 direction = Vector3.up;

    public bool validToDamage = true;
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
        {
            if (!validToDamage) return; // 👈 Check if projectile should deal damage
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

}
