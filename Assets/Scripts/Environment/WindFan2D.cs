using UnityEngine;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class WindFan2D : MonoBehaviour
    {
        public enum WindDirection
        {
            Up,
            Down,
            Left,
            Right,
            Custom
        }

        [Header("Wind Physics Settings")]
        [SerializeField] private WindDirection direction = WindDirection.Up;
        [SerializeField] private Vector2 customDirection = Vector2.up;
        [SerializeField] private float windForce = 28f;
        [SerializeField] private float maxSpeedInWind = 14f;

        [Header("Particle & Audio (Optional)")]
        [SerializeField] private ParticleSystem windParticles;

        private Collider2D windAreaCollider;

        private void Awake()
        {
            windAreaCollider = GetComponent<Collider2D>();
            if (windAreaCollider != null)
            {
                windAreaCollider.isTrigger = true;
            }
        }

        public Vector2 GetWindVector()
        {
            switch (direction)
            {
                case WindDirection.Up: return transform.up;
                case WindDirection.Down: return -transform.up;
                case WindDirection.Left: return -transform.right;
                case WindDirection.Right: return transform.right;
                case WindDirection.Custom: return customDirection.normalized;
                default: return transform.up;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector2 windVector = GetWindVector();

                // Apply continuous wind force
                if (rb.linearVelocity.magnitude < maxSpeedInWind)
                {
                    rb.AddForce(windVector * (windForce * Time.fixedDeltaTime * 60f), ForceMode2D.Force);
                }

                // Updraft flotation dampening so player floats smoothly
                if (direction == WindDirection.Up && rb.linearVelocity.y < maxSpeedInWind * 0.8f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.MoveTowards(rb.linearVelocity.y, maxSpeedInWind, windForce * Time.fixedDeltaTime));
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector2 dir = GetWindVector();
            Gizmos.DrawRay(transform.position, dir * 2.5f);
            Gizmos.DrawWireCube(transform.position + (Vector3)dir * 1.25f, new Vector3(1f, 2.5f, 1f));
        }
    }
}
