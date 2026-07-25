using UnityEngine;
using UnityEngine.InputSystem;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PushBox2D : MonoBehaviour
    {
        [Header("Pick Up & Throw Settings")]
        [SerializeField] private float grabRadius = 1.6f;
        [SerializeField] private Vector3 overheadOffset = new Vector3(0f, 1.25f, 0f);
        [SerializeField] private float throwForceX = 8f;
        [SerializeField] private float throwForceY = 4f;

        public Rigidbody2D RB { get; private set; }
        public Collider2D BoxCollider { get; private set; }
        public bool IsCarried { get; private set; } = false;

        private Transform carrierTransform;
        private SpriteRenderer carrierSpriteRenderer;

        private void Awake()
        {
            RB = GetComponent<Rigidbody2D>();
            BoxCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            // Update position above carrier's head when being carried overhead
            if (IsCarried && carrierTransform != null)
            {
                transform.position = carrierTransform.position + overheadOffset;

                // Handle drop / throw input (E key)
                Keyboard keyboard = Keyboard.current;
                bool interactPressed = (keyboard != null && keyboard.eKey.wasPressedThisFrame);
                if (!interactPressed)
                {
                    try { if (Input.GetKeyDown(KeyCode.E)) interactPressed = true; } catch { }
                }

                if (interactPressed)
                {
                    DropOrThrow();
                }
            }
            else
            {
                // Check if nearby player presses E to pick up box overhead
                CheckForPickupInput();
            }
        }

        private void CheckForPickupInput()
        {
            Keyboard keyboard = Keyboard.current;
            bool interactPressed = (keyboard != null && keyboard.eKey.wasPressedThisFrame);
            if (!interactPressed)
            {
                try { if (Input.GetKeyDown(KeyCode.E)) interactPressed = true; } catch { }
            }

            if (!interactPressed) return;

            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                float dist = Vector2.Distance(transform.position, playerObj.transform.position);
                if (dist <= grabRadius)
                {
                    PickupOverhead(playerObj.transform);
                }
            }
        }

        public void PickupOverhead(Transform player)
        {
            carrierTransform = player;
            carrierSpriteRenderer = player.GetComponent<SpriteRenderer>();

            IsCarried = true;
            if (RB != null)
            {
                RB.bodyType = RigidbodyType2D.Kinematic;
                RB.linearVelocity = Vector2.zero;
            }
            if (BoxCollider != null)
            {
                BoxCollider.enabled = false;
            }

            Debug.Log("[PushBox2D] Box picked up overhead!");
        }

        public void DropOrThrow()
        {
            if (!IsCarried) return;

            IsCarried = false;
            if (BoxCollider != null)
            {
                BoxCollider.enabled = true;
            }

            if (RB != null)
            {
                RB.bodyType = RigidbodyType2D.Dynamic;
                float facingDir = (carrierSpriteRenderer != null && carrierSpriteRenderer.flipX) ? -1f : 1f;
                RB.linearVelocity = new Vector2(facingDir * throwForceX, throwForceY);
            }

            carrierTransform = null;
            carrierSpriteRenderer = null;
            Debug.Log("[PushBox2D] Box dropped / tossed!");
        }
    }
}
