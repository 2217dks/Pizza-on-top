using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PizzaOnTop.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement & Acceleration")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float acceleration = 70f;
        [SerializeField] private float deceleration = 50f;
        [SerializeField] private float airControlMultiplier = 0.75f;

        [Header("Jump Physics (Snappy & Fast)")]
        [SerializeField] private float jumpForce = 17.5f;
        [SerializeField] private float gravityScale = 3.2f;
        [SerializeField] private bool enableVariableJumpHeight = true;
        [SerializeField] private float jumpCutMultiplier = 0.4f;

        [Header("Debug Controls")]
        [SerializeField] private KeyCode debugDeathKey = KeyCode.K;

        public Rigidbody2D RB { get; private set; }
        public Collider2D PlayerCollider { get; private set; }
        public PlayerAnimator PlayerAnim { get; private set; }

        public bool IsGrounded => isGrounded;
        public bool IsOnRope { get; set; } = false;
        public bool IsDead { get; set; } = false;

        private float moveInput;
        private bool isGrounded;
        private bool isJumping;

        // GC-Free Physics Buffer & Contact Filter
        private readonly Collider2D[] groundOverlapResults = new Collider2D[8];
        private ContactFilter2D groundFilter;
        private readonly WaitForSeconds respawnDelay = new WaitForSeconds(0.3f);

        private void Awake()
        {
            RB = GetComponent<Rigidbody2D>();
            PlayerCollider = GetComponent<Collider2D>();
            PlayerAnim = GetComponent<PlayerAnimator>();

            if (RB != null)
            {
                RB.constraints = RigidbodyConstraints2D.FreezeRotation;
                RB.gravityScale = gravityScale;
            }

            // Setup contact filter
            groundFilter = new ContactFilter2D();
            groundFilter.useTriggers = false;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            // Debug Death Key Test (Press K)
            bool debugDeathPressed = false;
            if (keyboard != null && keyboard.kKey.wasPressedThisFrame) debugDeathPressed = true;
            else
            {
                try { if (Input.GetKeyDown(debugDeathKey)) debugDeathPressed = true; } catch { }
            }

            if (debugDeathPressed)
            {
                Debug.Log("[PlayerController2D] Debug Death Key (K) pressed!");
                RespawnPlayer();
                return;
            }

            if (IsDead) return;

            // 1. Read Movement Input
            moveInput = 0f;
            if (keyboard != null)
            {
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput -= 1f;
            }

            if (moveInput == 0f)
            {
                try
                {
                    float axis = Input.GetAxisRaw("Horizontal");
                    if (Mathf.Abs(axis) > 0.01f) moveInput = axis;
                }
                catch { }
            }

            // 2. Perform Ground Check
            CheckIsGrounded();
            if (isGrounded && RB.linearVelocity.y <= 0.1f)
            {
                isJumping = false;
            }

            // 3. Jump Input
            bool jumpPressedThisFrame = false;
            bool jumpReleasedThisFrame = false;

            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
                {
                    jumpPressedThisFrame = true;
                }
                if (keyboard.spaceKey.wasReleasedThisFrame || keyboard.wKey.wasReleasedThisFrame || keyboard.upArrowKey.wasReleasedThisFrame)
                {
                    jumpReleasedThisFrame = true;
                }
            }
            else
            {
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) jumpPressedThisFrame = true;
                    if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) jumpReleasedThisFrame = true;
                }
                catch { }
            }

            // Execute Jump
            if (jumpPressedThisFrame && (isGrounded || IsOnRope))
            {
                ExecuteJump();
            }

            // Variable Jump Cut
            if (enableVariableJumpHeight && jumpReleasedThisFrame && isJumping && RB.linearVelocity.y > 0)
            {
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, RB.linearVelocity.y * jumpCutMultiplier);
                isJumping = false;
            }
        }

        private void FixedUpdate()
        {
            if (IsDead || IsOnRope || RB == null) return;

            float targetSpeed = moveInput * moveSpeed;
            float currentX = RB.linearVelocity.x;

            float accel = isGrounded ? (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration)
                                     : (acceleration * airControlMultiplier);

            float newX = Mathf.MoveTowards(currentX, targetSpeed, accel * Time.fixedDeltaTime);
            RB.linearVelocity = new Vector2(newX, RB.linearVelocity.y);
        }

        private void CheckIsGrounded()
        {
            if (PlayerCollider == null)
            {
                isGrounded = true;
                return;
            }

            Bounds bounds = PlayerCollider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
            Vector2 boxSize = new Vector2(bounds.size.x * 0.85f, 0.08f);

            LayerMask mask = ~LayerMask.GetMask("Player");
            groundFilter.SetLayerMask(mask);

            int count = Physics2D.OverlapBox(origin + Vector2.down * 0.05f, boxSize, 0f, groundFilter, groundOverlapResults);

            isGrounded = false;
            for (int i = 0; i < count; i++)
            {
                Collider2D col = groundOverlapResults[i];
                if (col != null && col.gameObject != gameObject)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        private void ExecuteJump()
        {
            if (RB != null)
            {
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, jumpForce);
                isJumping = true;

                if (PlayerAnim != null)
                {
                    PlayerAnim.TriggerJumpVisual();
                }
            }
        }

        public void RespawnPlayer()
        {
            StartCoroutine(RoutineRespawn());
        }

        private IEnumerator RoutineRespawn()
        {
            if (PlayerAnim != null)
            {
                PlayerAnim.TriggerHurtVisual();
            }

            IsDead = true;
            if (RB != null) RB.linearVelocity = Vector2.zero;

            yield return respawnDelay;

            GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");
            if (spawnObj != null)
            {
                transform.position = spawnObj.transform.position;
            }

            if (RB != null) RB.linearVelocity = Vector2.zero;
            isJumping = false;
            IsDead = false;
        }

        public Vector2 GetVelocity() => RB != null ? RB.linearVelocity : Vector2.zero;
        public void SetVelocity(Vector2 vel) { if (RB != null) RB.linearVelocity = vel; }

        private void OnDrawGizmosSelected()
        {
            if (PlayerCollider != null)
            {
                Gizmos.color = Color.green;
                Bounds bounds = PlayerCollider.bounds;
                Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
                Vector2 boxSize = new Vector2(bounds.size.x * 0.85f, 0.08f);
                Gizmos.DrawWireCube(origin + Vector2.down * 0.05f, boxSize);
            }
        }
    }
}
