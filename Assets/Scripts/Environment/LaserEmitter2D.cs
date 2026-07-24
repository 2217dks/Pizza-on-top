using System.Collections;
using UnityEngine;
using PizzaOnTop.Player;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserEmitter2D : MonoBehaviour
    {
        [Header("Laser Physics Settings")]
        [SerializeField] private float maxDistance = 25f;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Pulsing / Rotation Options")]
        [SerializeField] private bool isPulsing = false;
        [SerializeField] private float activeDuration = 2.0f;
        [SerializeField] private float inactiveDuration = 1.5f;
        [SerializeField] private bool isRotating = false;
        [SerializeField] private float rotationSpeed = 35f;

        [Header("Destructible Machine Settings")]
        [SerializeField] private bool isDestructible = true;
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private Sprite destroyedSprite;

        public bool IsBeamActive { get; private set; } = true;

        private LineRenderer lineRenderer;
        private SpriteRenderer spriteRenderer;
        private Collider2D machineCollider;
        private int currentHealth;
        private bool isDestroyed = false;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            machineCollider = GetComponent<Collider2D>();
            currentHealth = maxHealth;

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
            }
        }

        private void Start()
        {
            if (isPulsing)
            {
                StartCoroutine(RoutinePulseLaser());
            }
        }

        private void Update()
        {
            if (isDestroyed) return;

            if (isRotating)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }

            if (IsBeamActive)
            {
                UpdateLaserBeam();
            }
            else
            {
                if (lineRenderer != null) lineRenderer.enabled = false;
            }
        }

        private void UpdateLaserBeam()
        {
            if (lineRenderer == null) return;

            Vector2 origin = transform.position;
            Vector2 direction = transform.right;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, obstacleMask);
            Vector2 endPoint = hit.collider != null ? hit.point : origin + direction * maxDistance;

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, endPoint);

            // Check if laser beam hits player -> Trigger player death!
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                PlayerController2D player = hit.collider.GetComponent<PlayerController2D>();
                if (player != null && !player.IsDead)
                {
                    Debug.Log("[LaserEmitter2D] Player touched laser beam! Respawning!");
                    player.RespawnPlayer();
                }
            }
        }

        private IEnumerator RoutinePulseLaser()
        {
            while (!isDestroyed)
            {
                IsBeamActive = true;
                yield return new WaitForSeconds(activeDuration);

                IsBeamActive = false;
                yield return new WaitForSeconds(inactiveDuration);
            }
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
            IsBeamActive = false;

            if (lineRenderer != null) lineRenderer.enabled = false;

            if (destroyedSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = destroyedSprite;
            }

            Debug.Log($"[LaserEmitter2D] Laser machine '{gameObject.name}' DESTROYED!");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * maxDistance);
        }
    }
}
