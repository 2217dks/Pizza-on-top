using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using PizzaOnTop.Environment;
using PizzaOnTop.Managers;

namespace PizzaOnTop.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement & Acceleration")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float acceleration = 70f;
        [SerializeField] private float deceleration = 50f;
        [SerializeField] private float airControlMultiplier = 0.85f;

        [Header("Hollow Knight Responsive Jump Physics")]
        [SerializeField] private float jumpForce = 17.5f;
        [SerializeField] private float gravityScale = 3.2f;
        [SerializeField] private bool enableVariableJumpHeight = true;
        [SerializeField] private float jumpCutMultiplier = 0.4f;
        [SerializeField] private float maxFallSpeed = 25f;       // Hard cap on downward terminal velocity
        [SerializeField] private float maxUpwardSpeed = 20f;     // Hard cap on upward launch velocity!

        [Header("Juice Polish: Coyote, Buffer & Death Delay")]
        [SerializeField] private float coyoteTime = 0.12f;      // Grace period to jump after walking off ledges
        [SerializeField] private float jumpBufferTime = 0.15f;  // Queues jump before landing for instant corner jumps!
        [SerializeField] private float respawnDelayTime = 1.0f; // Delay duration between death and teleporting back to spawn!

        [Header("Debug Controls")]
        [SerializeField] private KeyCode debugDeathKey = KeyCode.K;

        public Rigidbody2D RB { get; private set; }
        public Collider2D PlayerCollider { get; private set; }
        public PlayerAnimator PlayerAnim { get; private set; }

        public bool IsGrounded => isGrounded;
        public bool IsOnRope { get; set; } = false;
        public bool IsDead { get; set; } = false;

        // External Wind Force Integration
        public Vector2 ActiveWindVelocity { get; set; } = Vector2.zero;

        private float moveInput;
        private bool isGrounded;
        private bool isJumping;

        // Polish Timers
        private float coyoteCounter;
        private float jumpBufferCounter;
        private float footstepTimer;
        private float footstepInterval = 0.35f;

        // GC-Free Physics Buffer & Contact Filter
        private readonly Collider2D[] groundOverlapResults = new Collider2D[8];
        private ContactFilter2D groundFilter;

        private void Awake()
        {
            RB = GetComponent<Rigidbody2D>();
            PlayerCollider = GetComponent<Collider2D>();
            PlayerAnim = GetComponent<PlayerAnimator>();

            if (RB != null)
            {
                RB.constraints = RigidbodyConstraints2D.FreezeRotation;
                RB.gravityScale = gravityScale;
                RB.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            // Zero friction physics material prevents wall catching when jumping next to blocks
            if (PlayerCollider != null && PlayerCollider.sharedMaterial == null)
            {
                PhysicsMaterial2D zeroFrictionMat = new PhysicsMaterial2D("ZeroWallFriction")
                {
                    friction = 0f,
                    bounciness = 0f
                };
                PlayerCollider.sharedMaterial = zeroFrictionMat;
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

            // 2. Perform Ground Check & Coyote Time
            CheckIsGrounded();

            if (isGrounded)
            {
                coyoteCounter = coyoteTime;
                if (RB.linearVelocity.y <= 0.1f)
                {
                    isJumping = false;
                }

                if (Mathf.Abs(moveInput) > 0.1f)
                {
                    footstepTimer -= Time.deltaTime;
                    if (footstepTimer <= 0f)
                    {
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayFootstep();
                        footstepTimer = footstepInterval;
                    }
                }
                else
                {
                    footstepTimer = 0f;
                }
            }
            else
            {
                coyoteCounter -= Time.deltaTime;
            }

            // 3. Jump Input & Jump Buffer (Space or Up Arrow)
            bool jumpPressedThisFrame = false;
            bool jumpReleasedThisFrame = false;

            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) jumpPressedThisFrame = true;
                if (keyboard.spaceKey.wasReleasedThisFrame || keyboard.upArrowKey.wasReleasedThisFrame) jumpReleasedThisFrame = true;
            }
            else
            {
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) jumpPressedThisFrame = true;
                    if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow)) jumpReleasedThisFrame = true;
                }
                catch { }
            }

            // Update Jump Buffer Timer
            if (jumpPressedThisFrame)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // Execute Jump (Instant response when landing or during Coyote Time!)
            if (jumpBufferCounter > 0f && (coyoteCounter > 0f || IsOnRope))
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
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

            // Combine player input speed with active horizontal wind force
            float targetSpeed = (moveInput * moveSpeed) + ActiveWindVelocity.x;
            float currentX = RB.linearVelocity.x;

            float accel = isGrounded ? (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration)
                                     : (acceleration * airControlMultiplier);

            float newX = Mathf.MoveTowards(currentX, targetSpeed, accel * Time.fixedDeltaTime);
            float newY = RB.linearVelocity.y + ActiveWindVelocity.y * Time.fixedDeltaTime;

            // Strict Vertical Velocity Ceiling & Terminal Fall Speed Clamp!
            newY = Mathf.Clamp(newY, -maxFallSpeed, maxUpwardSpeed);

            RB.linearVelocity = new Vector2(newX, newY);

            // Clear frame wind velocity
            ActiveWindVelocity = Vector2.zero;
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

                if (AudioManager.Instance != null) AudioManager.Instance.PlayJump();

                if (PlayerAnim != null)
                {
                    PlayerAnim.TriggerJumpVisual();
                }
            }
        }

        public void RespawnPlayer()
        {
            if (IsDead) return;
            StartCoroutine(RoutineRespawn());
        }

        private IEnumerator RoutineRespawn()
        {
            IsDead = true;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDeath();
            if (PlayerAnim != null)
            {
                PlayerAnim.TriggerHurtVisual();
            }

            if (RB != null) RB.linearVelocity = Vector2.zero;

            // Wait for respawnDelayTime so player sees death animation before teleporting back!
            yield return new WaitForSeconds(respawnDelayTime);

            GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");
            if (spawnObj != null)
            {
                transform.position = spawnObj.transform.position;
            }

            // Auto-reset all crumbling tilemaps on respawn!
            CrumblingTilemap2D[] tilemaps = FindObjectsByType<CrumblingTilemap2D>(FindObjectsSortMode.None);
            foreach (var tm in tilemaps)
            {
                tm.ResetAllCrumblingTiles();
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
