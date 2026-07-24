using UnityEngine;

namespace PizzaOnTop.CameraSystem
{
    public class CameraController2D : MonoBehaviour
    {
        public static CameraController2D Instance { get; private set; }

        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.25f;

        [Header("Vertical Lock Options")]
        [SerializeField] private bool lockVertical = true;
        [SerializeField] private float fixedCameraY = 0f;
        [SerializeField] private bool useInitialYAsFixed = true;

        [Header("Horizontal Camera Bounding (Optional)")]
        [SerializeField] private bool useHorizontalBounds = false;
        [SerializeField] private float minXBounds = -20f;
        [SerializeField] private float maxXBounds = 20f;

        private Vector3 currentVelocity = Vector3.zero;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (useInitialYAsFixed)
            {
                fixedCameraY = transform.position.y;
            }

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

            // Lock vertical Y axis if enabled
            if (lockVertical)
            {
                targetPos.y = fixedCameraY;
            }

            // Apply horizontal bounds if enabled
            if (useHorizontalBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minXBounds, maxXBounds);
            }

            // Smooth horizontal camera dampening
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        }

        public void ResetCameraVelocity()
        {
            currentVelocity = Vector3.zero;
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
