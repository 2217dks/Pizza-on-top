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

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Start()
        {
            // Auto destroy beam after beamLifetime seconds if it hits nothing
            Destroy(gameObject, beamLifetime);

            if (rb != null)
            {
                rb.linearVelocity = transform.right * flySpeed;
            }
        }

        private void FixedUpdate()
        {
            // Velocity fallback if Rigidbody2D is missing or kinematic
            if (rb == null || rb.bodyType == RigidbodyType2D.Kinematic)
            {
                transform.Translate(Vector3.right * (flySpeed * Time.fixedDeltaTime));
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore laser machine emitter itself
            if (other.GetComponent<LaserEmitter2D>() != null) return;

            // 1. Hit Player -> Trigger Respawn Death and destroy beam!
            if (other.CompareTag("Player"))
            {
                PlayerController2D player = other.GetComponent<PlayerController2D>();
                if (player != null && !player.IsDead)
                {
                    Debug.Log("[LaserBeam2D] Laser beam hit player! Respawning!");
                    player.RespawnPlayer();
                }
                Destroy(gameObject);
                return;
            }

            // 2. Hit Walls, Floors, Boxes, or Obstacles -> ONLY destroy the beam itself (leaves obstacles intact!)
            if (!other.isTrigger)
            {
                Debug.Log($"[LaserBeam2D] Laser beam hit '{other.gameObject.name}' and destroyed itself!");
                Destroy(gameObject);
            }
        }
    }
}
