using UnityEngine;

namespace PizzaOnTop.CameraSystem
{
    public class CameraController2D : MonoBehaviour
    {
        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Camera Bounding (Optional)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-20f, 0f);
        [SerializeField] private Vector2 maxBounds = new Vector2(20f, 30f);

        private Vector3 currentVelocity = Vector3.zero;

        private void Start()
        {
            FindPlayerTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return;
            }

            // Target position with offset
            Vector3 targetPos = target.position + offset;

            // Apply level bounds if enabled
            if (useBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
                targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
            }

            // Smooth camera dampening
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        }

        private void FindPlayerTarget()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }
}
