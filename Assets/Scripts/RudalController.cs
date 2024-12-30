using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RudalController : MonoBehaviour
{
    [Tooltip("Time before the missile is destroyed")]
    public float lifetime = 5f;
    public int damage = 300;

    [Tooltip("Speed of the missile")]
    public float speed = 100f;

    private bool hasCollided = false; // Flag untuk mencegah collision berulang

    private void Start()
    {
        // Hancurkan rudal setelah waktu tertentu
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Gerakkan rudal maju dengan kecepatan yang ditentukan
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            return; // Abaikan tabrakan dengan pemain

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Health enemyHealth = collision.gameObject.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        Destroy(gameObject); // Hancurkan rudal setelah tabrakan
    }
}
