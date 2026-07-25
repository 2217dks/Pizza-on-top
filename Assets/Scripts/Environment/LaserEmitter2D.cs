using System.Collections;
using UnityEngine;

namespace PizzaOnTop.Environment
{
    public class LaserEmitter2D : MonoBehaviour
    {
        [Header("Laser Nozzle & Prefab Settings")]
        [SerializeField] private GameObject laserBeamPrefab;                      // Laser beam projectile prefab
        [SerializeField] private Vector2 nozzleOffset = new Vector2(0.5f, 0f);     // Nozzle tip position offset

        [Header("Shooting Interval & Target Tracking")]
        [SerializeField] private float fireRateInterval = 1.2f;                   // Time between laser shots (sec)
        [SerializeField] private bool targetPlayer = true;                         // Rotates nozzle to track player
        [SerializeField] private float targetingRange = 25f;                       // Max range to detect and fire at player
        [SerializeField] private float rotationSpeed = 90f;                        // Aim rotation speed
        [SerializeField] private bool requirePlayerInRangeToFire = true;           // ONLY shoots when player is in range!

        [Header("Destructible Machine Settings")]
        [SerializeField] private bool isDestructible = true;
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private Sprite destroyedSprite;

        public bool IsBeamActive { get; private set; } = true;

        private SpriteRenderer spriteRenderer;
        private Collider2D machineCollider;
        private int currentHealth;
        private bool isDestroyed = false;
        private Transform playerTransform;
        private float nextFireTime;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            machineCollider = GetComponent<Collider2D>();
            currentHealth = maxHealth;
        }

        private void Start()
        {
            GameObject pObj = GameObject.FindWithTag("Player");
            if (pObj != null) playerTransform = pObj.transform;
        }

        private void Update()
        {
            if (isDestroyed) return;

            bool isPlayerInArea = IsPlayerInTargetingRange();

            // 1. Aim Nozzle Towards Player when in range
            if (targetPlayer && isPlayerInArea)
            {
                AimAtPlayer();
            }

            // 2. Fire Laser Beam when player is in targeting range!
            if (Time.time >= nextFireTime)
            {
                if (!requirePlayerInRangeToFire || isPlayerInArea)
                {
                    FireLaserBeam();
                    nextFireTime = Time.time + fireRateInterval;
                }
            }
        }

        private bool IsPlayerInTargetingRange()
        {
            if (playerTransform == null)
            {
                GameObject pObj = GameObject.FindWithTag("Player");
                if (pObj != null) playerTransform = pObj.transform;
                else return false;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            return dist <= targetingRange;
        }

        private void AimAtPlayer()
        {
            if (playerTransform == null) return;

            Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        public void FireLaserBeam()
        {
            if (laserBeamPrefab == null || isDestroyed) return;

            Vector2 spawnPos = (Vector2)transform.position + (Vector2)(transform.rotation * nozzleOffset);
            Instantiate(laserBeamPrefab, spawnPos, transform.rotation);

            Debug.Log("[LaserEmitter2D] Fired targeted laser beam blast!");
        }

        public void TakeDamage(int damage = 1)
        {
            if (!isDestructible || isDestroyed) return;

            currentHealth -= damage;
            Debug.Log($"[LaserEmitter2D] Machine hit! Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                DestroyMachine();
            }
        }

        public void DestroyMachine()
        {
            isDestroyed = true;

            if (destroyedSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = destroyedSprite;
            }

            Debug.Log($"[LaserEmitter2D] Laser machine '{gameObject.name}' DESTROYED!");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector2 nozzlePos = (Vector2)transform.position + (Vector2)(transform.rotation * nozzleOffset);
            Gizmos.DrawWireSphere(nozzlePos, 0.1f);
            Gizmos.DrawRay(nozzlePos, transform.right * 3f);

            if (targetPlayer)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, targetingRange);
            }
        }
    }
}
