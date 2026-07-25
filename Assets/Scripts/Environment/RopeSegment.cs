using UnityEngine;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class RopeSegment : MonoBehaviour
    {
        public HingeJoint2D Joint { get; private set; }
        public Rigidbody2D RB { get; private set; }
        public Collider2D SegmentCollider { get; private set; }

        private void Awake()
        {
            Joint = GetComponent<HingeJoint2D>();
            RB = GetComponent<Rigidbody2D>();
            SegmentCollider = GetComponent<Collider2D>();

            // Guarantee rope colliders are non-solid triggers so they never block or bump the player physically!
            if (SegmentCollider != null)
            {
                SegmentCollider.isTrigger = true;
            }
        }
    }
}
