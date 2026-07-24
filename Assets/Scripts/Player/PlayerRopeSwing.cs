using UnityEngine;
using UnityEngine.InputSystem;
using PizzaOnTop.Environment;
using PizzaOnTop.CameraSystem;

namespace PizzaOnTop.Player
{
    [RequireComponent(typeof(PlayerController2D), typeof(Rigidbody2D))]
    public class PlayerRopeSwing : MonoBehaviour
    {
        [Header("Rope Grab Circle Tuning")]
        [SerializeField] private float maxGrabDistance = 1.6f;
        [SerializeField] private Vector2 centerOffset = new Vector2(0f, 0f);

        [Header("Inertia & Physics Swing")]
        [SerializeField] private float momentumTransferFactor = 0.85f;
        [SerializeField] private float swingForce = 180f;
        [SerializeField] private float launchInertiaMultiplier = 1.1f;
        [SerializeField] private float maxLaunchSpeed = 18f;
        [SerializeField] private float ropeDetachCooldown = 0.3f;

        private PlayerController2D playerController;
        private Rigidbody2D playerRB;
        private HingeJoint2D playerHingeJoint;
        private Collider2D playerCollider;

        private RopeSegment nearbyRopeSegment;
        private Rigidbody2D ropeSegmentRB;
        private float detachTimer;
        private float currentSwingInput;

        // GC-Free Overlap Buffer & Contact Filter
        private readonly Collider2D[] ropeOverlapResults = new Collider2D[8];
        private ContactFilter2D ropeFilter;

        private void Awake()
        {
            playerController = GetComponent<PlayerController2D>();
            playerRB = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();

            ropeFilter = new ContactFilter2D();
            ropeFilter.useTriggers = true;
        }

        private void Start()
        {
            if (playerController != null)
            {
                playerController.IsOnRope = false;
            }
        }

        public Vector2 GetCenterPosition()
        {
            if (playerCollider != null)
            {
                return (Vector2)playerCollider.bounds.center + centerOffset;
            }
            return (Vector2)transform.position + centerOffset;
        }

        private void Update()
        {
            if (detachTimer > 0) detachTimer -= Time.deltaTime;

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool isHoldingGrab = (mouse != null && mouse.leftButton.isPressed) ||
                                 (keyboard != null && (keyboard.eKey.isPressed || keyboard.leftShiftKey.isPressed));

            // Fast grab check using body center
            if (!playerController.IsOnRope && detachTimer <= 0 && isHoldingGrab)
            {
                RopeSegment targetSegment = GetBestRopeSegment();
                if (targetSegment != null)
                {
                    AttachToRope(targetSegment);
                }
            }

            currentSwingInput = 0f;
            if (playerController.IsOnRope)
            {
                bool releasePressed = !isHoldingGrab || (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame));
                if (releasePressed)
                {
                    DetachFromRope();
                    return;
                }

                if (keyboard != null)
                {
                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) currentSwingInput += 1f;
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) currentSwingInput -= 1f;
                }
            }
        }

        private void FixedUpdate()
        {
            if (playerController.IsOnRope && ropeSegmentRB != null && Mathf.Abs(currentSwingInput) > 0.01f)
            {
                ropeSegmentRB.AddForce(Vector2.right * (currentSwingInput * swingForce), ForceMode2D.Force);
            }
        }

        private RopeSegment GetBestRopeSegment()
        {
            Vector2 center = GetCenterPosition();

            if (nearbyRopeSegment != null && Vector2.Distance(center, nearbyRopeSegment.transform.position) <= maxGrabDistance)
            {
                return nearbyRopeSegment;
            }

            int count = Physics2D.OverlapCircle(center, maxGrabDistance, ropeFilter, ropeOverlapResults);
            for (int i = 0; i < count; i++)
            {
                Collider2D col = ropeOverlapResults[i];
                if (col != null)
                {
                    RopeSegment seg = col.GetComponent<RopeSegment>();
                    if (seg != null)
                    {
                        return seg;
                    }
                }
            }

            return null;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            RopeSegment segment = other.GetComponent<RopeSegment>();
            if (segment != null)
            {
                nearbyRopeSegment = segment;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            RopeSegment segment = other.GetComponent<RopeSegment>();
            if (segment != null && segment == nearbyRopeSegment)
            {
                nearbyRopeSegment = null;
            }
        }

        public void AttachToRope(RopeSegment segment)
        {
            playerController.IsOnRope = true;
            nearbyRopeSegment = segment;
            ropeSegmentRB = segment.GetComponent<Rigidbody2D>();

            Vector2 incomingVelocity = playerController.GetVelocity();

            Vector2 bodyOffsetFromFeet = GetCenterPosition() - (Vector2)transform.position;
            transform.position = (Vector2)segment.transform.position - bodyOffsetFromFeet;

            // Reset camera velocity on position snap to eliminate camera jitter!
            if (CameraController2D.Instance != null)
            {
                CameraController2D.Instance.ResetCameraVelocity();
            }

            if (ropeSegmentRB != null)
            {
                ropeSegmentRB.linearVelocity += incomingVelocity * momentumTransferFactor;
            }

            if (playerHingeJoint == null)
            {
                playerHingeJoint = gameObject.AddComponent<HingeJoint2D>();
            }

            playerHingeJoint.enabled = true;
            playerHingeJoint.enableCollision = false;
            playerHingeJoint.connectedBody = ropeSegmentRB;
            playerHingeJoint.autoConfigureConnectedAnchor = false;
            playerHingeJoint.anchor = bodyOffsetFromFeet;
            playerHingeJoint.connectedAnchor = Vector2.zero;

            Debug.Log($"[PlayerRopeSwing] Body center snapped cleanly onto rope handle!");
        }

        public void DetachFromRope()
        {
            if (!playerController.IsOnRope) return;

            Vector2 launchVelocity = Vector2.zero;
            if (ropeSegmentRB != null)
            {
                launchVelocity = ropeSegmentRB.linearVelocity * launchInertiaMultiplier;
            }
            else
            {
                launchVelocity = playerController.GetVelocity();
            }

            if (playerHingeJoint != null)
            {
                playerHingeJoint.enabled = false;
                Destroy(playerHingeJoint);
            }

            playerController.IsOnRope = false;
            detachTimer = ropeDetachCooldown;

            launchVelocity = Vector2.ClampMagnitude(launchVelocity, maxLaunchSpeed);
            playerController.SetVelocity(launchVelocity);
            Debug.Log($"[PlayerRopeSwing] Released rope with launch velocity: {launchVelocity}");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(GetCenterPosition(), maxGrabDistance);
        }
    }
}
