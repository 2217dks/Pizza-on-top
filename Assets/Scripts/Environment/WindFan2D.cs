using UnityEngine;
using PizzaOnTop.Player;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
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

        [Header("Wind Direction & Physics Settings")]
        [SerializeField] private WindDirection direction = WindDirection.Up;
        [SerializeField] private Vector2 customDirection = Vector2.up;
        [SerializeField] private float windForce = 6f;                 // Default Base wind force = 6
        [SerializeField] private float verticalWindMultiplier = 8f;     // Default Vertical Gravity Multiplier = 8
        [SerializeField] private float maxSpeedInWind = 10f;           // Default Max Speed = 10

        [Header("Wind Area Size Control (Inspector Driven)")]
        [SerializeField] private Vector2 windAreaSize = new Vector2(2f, 8f);      // Width x Height of the wind force field
        [SerializeField] private Vector2 windAreaOffset = new Vector2(0f, 4f);    // Position offset of the wind field
        [SerializeField] private bool autoUpdateCollider = true;                   // Automatically sets BoxCollider2D size

        private BoxCollider2D windBoxCollider;

        private void Awake()
        {
            windBoxCollider = GetComponent<BoxCollider2D>();
            if (windBoxCollider != null)
            {
                windBoxCollider.isTrigger = true;
                UpdateColliderDimensions();
            }
        }

        private void OnValidate()
        {
            if (autoUpdateCollider)
            {
                if (windBoxCollider == null) windBoxCollider = GetComponent<BoxCollider2D>();
                UpdateColliderDimensions();
            }
        }

        public void UpdateColliderDimensions()
        {
            if (windBoxCollider != null)
            {
                windBoxCollider.isTrigger = true;
                windBoxCollider.size = windAreaSize;
                windBoxCollider.offset = windAreaOffset;
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
            Vector2 windVector = GetWindVector();

            // Calculate effective force compensating for gravity on vertical updrafts
            float effectiveWindForce = windForce;
            if (direction == WindDirection.Up || direction == WindDirection.Down || windVector.y > 0.5f)
            {
                effectiveWindForce *= verticalWindMultiplier;
            }

            // 1. Direct Player Wind Integration
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null && !player.IsDead)
            {
                // Pass balanced wind velocity directly into player controller
                player.ActiveWindVelocity += windVector * effectiveWindForce;

                // Also dampen player vertical velocity in wind to prevent super-launching
                if (player.RB != null && player.RB.linearVelocity.magnitude > maxSpeedInWind * 1.2f)
                {
                    player.RB.linearVelocity = Vector2.ClampMagnitude(player.RB.linearVelocity, maxSpeedInWind * 1.2f);
                }
                return;
            }

            // 2. Rope Segment Handling (Clamp rope speed so wind doesn't explode rope joints!)
            RopeSegment ropeSeg = other.GetComponent<RopeSegment>();
            if (ropeSeg != null)
            {
                Rigidbody2D segRB = ropeSeg.GetComponent<Rigidbody2D>();
                if (segRB != null)
                {
                    float speedInWindDirection = Vector2.Dot(segRB.linearVelocity, windVector);
                    if (speedInWindDirection < maxSpeedInWind * 0.75f)
                    {
                        segRB.AddForce(windVector * (effectiveWindForce * 0.4f * Time.fixedDeltaTime * 60f), ForceMode2D.Force);
                    }
                    segRB.linearVelocity = Vector2.ClampMagnitude(segRB.linearVelocity, maxSpeedInWind * 0.85f);
                }
                return;
            }

            // 3. Generic Rigidbody2D Objects (PushBoxes, etc.)
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                float speedInWindDirection = Vector2.Dot(rb.linearVelocity, windVector);
                if (speedInWindDirection < maxSpeedInWind)
                {
                    rb.AddForce(windVector * (effectiveWindForce * Time.fixedDeltaTime * 60f), ForceMode2D.Force);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector2 dir = GetWindVector();

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;

            Gizmos.DrawWireCube(windAreaOffset, windAreaSize);
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.DrawRay(transform.position, dir * 2.5f);
        }
    }
}
