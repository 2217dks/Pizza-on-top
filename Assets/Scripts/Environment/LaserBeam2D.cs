using UnityEngine;
using PizzaOnTop.Player;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class LaserBeam2D : MonoBehaviour
    {
        [Header("Laser Beam Flight & Lifetime")]
        [SerializeField] private float flySpeed = 22f;          // Flight speed along nozzle direction (transform.right)
        [SerializeField] private float beamLifetime = 2.5f;       // Auto-destroys after max duration if hitting nothing

        private Rigidbody2D rb;
        private Collider2D col;
        private Vector2 lastPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Start()
        {
            lastPosition = transform.position;
            Destroy(gameObject, beamLifetime);

            if (rb != null)
            {
                rb.linearVelocity = transform.right * flySpeed;
            }
        }

        private void FixedUpdate()
        {
            // Continuous collision check between frames to prevent high-speed clipping through boxes or walls
            Vector2 currentPosition = transform.position;
            Vector2 delta = currentPosition - lastPosition;
            float dist = delta.magnitude;

            if (dist > 0.001f)
            {
                RaycastHit2D[] hits = Physics2D.LinecastAll(lastPosition, currentPosition);
                foreach (var hit in hits)
                {
                    if (hit.collider != null && ProcessCollision(hit.collider))
                    {
                        return;
                    }
                }
            }

            lastPosition = currentPosition;

            // Velocity fallback if Rigidbody2D is missing or kinematic
            if (rb == null || rb.bodyType == RigidbodyType2D.Kinematic)
            {
                transform.Translate(Vector3.right * (flySpeed * Time.fixedDeltaTime));
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ProcessCollision(other);
        }

        private bool ProcessCollision(Collider2D other)
        {
            if (other == null || other.gameObject == gameObject) return false;

            // Ignore laser machine emitter and wind fan trigger fields completely
            if (other.GetComponent<LaserEmitter2D>() != null || other.GetComponent<WindFan2D>() != null) return false;

            // 1. Hit PushBox -> Destroy laser beam on impact, protecting player taking cover behind it!
            if (other.GetComponent<PushBox2D>() != null)
            {
                Debug.Log($"[LaserBeam2D] Beam hit PushBox '{other.gameObject.name}' and was blocked!");
                Destroy(gameObject);
                return true;
            }

            // 2. Hit Player -> Trigger Respawn Death and destroy beam!
            if (other.CompareTag("Player"))
            {
                PlayerController2D player = other.GetComponent<PlayerController2D>();
                if (player != null && !player.IsDead)
                {
                    Debug.Log("[LaserBeam2D] Laser beam hit player! Respawning!");
                    player.RespawnPlayer();
                }
                Destroy(gameObject);
                return true;
            }

            // 3. Hit Walls, Floors, or other solid non-trigger obstacles -> destroy beam
            if (!other.isTrigger)
            {
                Debug.Log($"[LaserBeam2D] Laser beam hit '{other.gameObject.name}' and destroyed itself!");
                Destroy(gameObject);
                return true;
            }

            return false;
        }
    }
}
