using UnityEngine;
using PizzaOnTop.Environment;

namespace PizzaOnTop.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Bullet2D : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 22f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 3.5f;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 direction)
        {
            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * speed;
            }
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Damage destructible laser emitters
            LaserEmitter2D laser = other.GetComponent<LaserEmitter2D>();
            if (laser != null)
            {
                laser.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            // Destroy bullet on wall / obstacle hit
            if (!other.CompareTag("Player") && !other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
