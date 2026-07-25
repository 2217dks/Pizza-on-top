using UnityEngine;

namespace PizzaOnTop.Environment
{
    [ExecuteAlways]
    public class Rope : MonoBehaviour
    {
        [Header("Rope Segment Prefab & Dimensions")]
        [SerializeField] private GameObject ropeSegmentPrefab;
        [SerializeField] private float segmentHeight = 1.0f;       // Total height of 1 segment sprite in world units

        [Header("Physics Joint & Rotation Tuning")]
        [SerializeField] private bool allow360TopCeiling = true;    // Ceiling joint rotates 360° freely
        [SerializeField] private float innerJointLimitAngle = 55f;  // Inner joint free rotation limit (-55° to +55°)
        [SerializeField] private float jointMotorResistance = 3f;  // Minor torque damping for natural rope feel
        [SerializeField] private float topSegmentMass = 0.5f;
        [SerializeField] private float middleSegmentMass = 0.7f;
        [SerializeField] private float bottomHandleMass = 1.5f;   // Heavier bottom weight keeps rope taut

        [Header("Linear & Angular Drag")]
        [SerializeField] private float linearDrag = 0.2f;
        [SerializeField] private float angularDrag = 0.4f;
        [SerializeField] private float gravityScale = 2.5f;

        private void Start()
        {
            if (transform.childCount == 0 && ropeSegmentPrefab != null)
            {
                Generate3SegmentRope();
            }
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!Application.isPlaying && transform.childCount == 0 && ropeSegmentPrefab != null)
            {
                UnityEditor.EditorApplication.delayCall += AutoGenerateInEditor;
            }
        }

        private void AutoGenerateInEditor()
        {
            if (this == null || Application.isPlaying) return;
            if (transform.childCount == 0 && ropeSegmentPrefab != null)
            {
                Generate3SegmentRope();
            }
        }
#endif

        [ContextMenu("Generate 3-Segment Rope")]
        public void Generate3SegmentRope()
        {
            if (ropeSegmentPrefab == null)
            {
                Debug.LogWarning("[Rope] Please assign a ropeSegmentPrefab in Inspector!");
                return;
            }

            // 1. Static Ceiling Anchor on parent
            Rigidbody2D topCeilingRB = GetComponent<Rigidbody2D>();
            if (topCeilingRB == null)
            {
                topCeilingRB = gameObject.AddComponent<Rigidbody2D>();
            }
            topCeilingRB.bodyType = RigidbodyType2D.Static;

            // 2. Clear existing child segments safely
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.DestroyObjectImmediate(child);
#else
                    DestroyImmediate(child);
#endif
                }
            }

            // 3. Auto-detect segment height from SpriteRenderer if possible
            SpriteRenderer sr = ropeSegmentPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                segmentHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
            }

            float halfHeight = segmentHeight * 0.5f;
            Rigidbody2D previousRB = topCeilingRB;
            string[] segmentNames = { "Rope_Top", "Rope_Middle", "Rope_BottomHandle" };

            // 4. Instantiate 3 interconnected physics segments with zero gaps
            for (int i = 0; i < 3; i++)
            {
                GameObject segObj = Instantiate(ropeSegmentPrefab, transform);
                segObj.name = segmentNames[i];

                // Position segment center down from ceiling anchor
                float targetY = -(i * segmentHeight + halfHeight);
                segObj.transform.localPosition = new Vector3(0f, targetY, 0f);
                segObj.transform.localRotation = Quaternion.identity;

                // Configure Rigidbody2D
                Rigidbody2D rb = segObj.GetComponent<Rigidbody2D>();
                if (rb == null) rb = segObj.AddComponent<Rigidbody2D>();

                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.mass = (i == 0) ? topSegmentMass : (i == 1 ? middleSegmentMass : bottomHandleMass);
                rb.gravityScale = gravityScale;
                rb.linearDamping = linearDrag;
                rb.angularDamping = angularDrag;
                rb.constraints = RigidbodyConstraints2D.None;

                // Configure Trigger Collider (non-solid for player)
                Collider2D col = segObj.GetComponent<Collider2D>();
                if (col != null) col.isTrigger = true;

                // Configure HingeJoint2D
                HingeJoint2D hinge = segObj.GetComponent<HingeJoint2D>();
                if (hinge == null) hinge = segObj.AddComponent<HingeJoint2D>();

                hinge.enabled = true;
                hinge.enableCollision = false;
                hinge.connectedBody = previousRB;
                hinge.autoConfigureConnectedAnchor = false;

                // Hinge Joint Sockets:
                // Anchor on THIS segment = top tip (0, +halfHeight)
                hinge.anchor = new Vector2(0f, halfHeight);

                // Connected Anchor on PREVIOUS segment:
                if (i == 0)
                {
                    // Top segment connects directly to parent Ceiling Anchor at origin (0, 0)
                    hinge.connectedAnchor = Vector2.zero;

                    if (allow360TopCeiling)
                    {
                        hinge.useLimits = false;
                        hinge.useMotor = false;
                    }
                }
                else
                {
                    // Middle & Bottom segments connect to the BOTTOM TIP (0, -halfHeight) of the previous segment
                    hinge.connectedAnchor = new Vector2(0f, -halfHeight);

                    JointAngleLimits2D limits = new JointAngleLimits2D
                    {
                        min = -innerJointLimitAngle,
                        max = innerJointLimitAngle
                    };
                    hinge.limits = limits;
                    hinge.useLimits = true;

                    if (jointMotorResistance > 0)
                    {
                        JointMotor2D motor = new JointMotor2D
                        {
                            motorSpeed = 0f,
                            maxMotorTorque = jointMotorResistance
                        };
                        hinge.motor = motor;
                        hinge.useMotor = true;
                    }
                }

                previousRB = rb;
            }

            Debug.Log($"[Rope] 3-Segment HingeJoint Rope generated! Segment height = {segmentHeight} units.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            float halfH = segmentHeight * 0.5f;

            for (int i = 0; i < 3; i++)
            {
                Vector3 center = transform.position + Vector3.down * (i * segmentHeight + halfH);
                Vector3 topSocket = center + Vector3.up * halfH;
                Vector3 bottomSocket = center + Vector3.down * halfH;

                Gizmos.DrawWireSphere(topSocket, 0.08f);
                Gizmos.DrawWireSphere(bottomSocket, 0.08f);
                Gizmos.DrawLine(topSocket, bottomSocket);
            }
        }
    }
}
