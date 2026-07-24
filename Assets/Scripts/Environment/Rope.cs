using UnityEngine;

namespace PizzaOnTop.Environment
{
    public class Rope : MonoBehaviour
    {
        [Header("3-Segment Rope Settings")]
        [SerializeField] private GameObject ropeSegmentPrefab;
        [SerializeField] private float segmentLength = 0.8f;

        [Header("120° Free Rotation & Soft Resistance Physics")]
        [SerializeField] private bool allowFull360TopAnchor = true;  // Unconstrained 360° top ceiling anchor!
        [SerializeField] private float freeRotationAngle = 60f;      // 120° total free arc (-60° to +60°) with ZERO resistance!
        [SerializeField] private float maxOverBendAngle = 135f;      // Allows bending further up to +/-135° under load!
        [SerializeField] private float minorResistanceTorque = 4f;    // Light resistance beyond 60 degrees
        [SerializeField] private float segmentMass = 0.6f;
        [SerializeField] private float handleMass = 1.6f;            // Taut bottom handle weight
        [SerializeField] private float segmentGravityScale = 2.5f;
        [SerializeField] private float angularDamping = 0.1f;
        [SerializeField] private float linearDamping = 0.02f;

        private void Start()
        {
            if (transform.childCount == 0 && ropeSegmentPrefab != null)
            {
                Generate3SegmentRope();
            }
        }

        [ContextMenu("Generate 3-Segment Rope")]
        public void Generate3SegmentRope()
        {
            if (ropeSegmentPrefab == null)
            {
                Debug.LogWarning("[Rope] Please assign a ropeSegmentPrefab in Inspector!");
                return;
            }

            // 1. Ensure Anchor Rigidbody exists on parent
            Rigidbody2D topAnchor = GetComponent<Rigidbody2D>();
            if (topAnchor == null)
            {
                topAnchor = gameObject.AddComponent<Rigidbody2D>();
            }
            topAnchor.bodyType = RigidbodyType2D.Static;

            // 2. Clear existing children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // 3. Instantiate 3 fluid physics rope segments
            Rigidbody2D previousRB = topAnchor;
            string[] names = { "Rope_Top", "Rope_Middle", "Rope_BottomHandle" };

            for (int i = 0; i < 3; i++)
            {
                GameObject segObj = Instantiate(ropeSegmentPrefab, transform);
                segObj.name = names[i];
                segObj.transform.position = transform.position + Vector3.down * (i * segmentLength + segmentLength * 0.5f);

                Rigidbody2D rb = segObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.mass = (i == 2) ? handleMass : segmentMass;
                    rb.gravityScale = segmentGravityScale;
                    rb.angularDamping = angularDamping;
                    rb.linearDamping = linearDamping;
                }

                HingeJoint2D hinge = segObj.GetComponent<HingeJoint2D>();
                if (hinge != null)
                {
                    hinge.enableCollision = false;
                    hinge.connectedBody = previousRB;
                    hinge.autoConfigureConnectedAnchor = false;
                    hinge.anchor = new Vector2(0, segmentLength * 0.5f);
                    hinge.connectedAnchor = (i == 0) ? Vector2.zero : new Vector2(0, -segmentLength * 0.5f);

                    if (i == 0 && allowFull360TopAnchor)
                    {
                        // Top ceiling anchor: FREE 360-degree rotation for full loops!
                        hinge.useLimits = false;
                        hinge.useMotor = false;
                    }
                    else
                    {
                        // Inner joints: Free -60° to +60° rotation (120° arc), extending to +/-135° with minor resistance!
                        JointAngleLimits2D limits = new JointAngleLimits2D();
                        limits.min = -maxOverBendAngle;
                        limits.max = maxOverBendAngle;
                        hinge.limits = limits;
                        hinge.useLimits = true;

                        JointMotor2D motor = new JointMotor2D();
                        motor.motorSpeed = 0f;
                        motor.maxMotorTorque = minorResistanceTorque;
                        hinge.motor = motor;
                        hinge.useMotor = true;
                    }
                }

                previousRB = rb;
            }

            Debug.Log("[Rope] 120° free rotation rope generated with minor over-bend resistance!");
        }
    }
}
