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
        
        // Simple bounds check to return to pool (disable) instead of destroy
        if (Mathf.Abs(transform.position.y) > 10f || Mathf.Abs(transform.position.x) > 10f)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!validToDamage) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                gameObject.SetActive(false); // Return to pool
            }
        }
    }
}
