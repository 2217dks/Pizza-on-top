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

        [Header("Floor Camera Snapping (16 Units Height Per Floor)")]
        [SerializeField] private bool snapToFloorHeight = true;  // Automatically centers camera on current floor!
        [SerializeField] private float floorHeight = 16f;        // Exact world height per floor (Camera Ortho Size 8)
        [SerializeField] private float baseFloorCenterY = 8f;     // Center Y position of Floor 1

        [Header("Manual Vertical Lock Fallback")]
        [SerializeField] private bool lockVertical = true;
        [SerializeField] private float fixedCameraY = 8f;

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
            FindPlayerTarget();
            if (target != null && snapToFloorHeight)
            {
                UpdateTargetFloorY();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return;
            }

            Vector3 targetPos = target.position + offset;

            if (snapToFloorHeight)
            {
                // Calculate current floor index from player Y position and snap camera center!
                int currentFloorIndex = Mathf.FloorToInt(target.position.y / floorHeight);
                targetPos.y = (currentFloorIndex * floorHeight) + baseFloorCenterY;
            }
            else if (lockVertical)
            {
                targetPos.y = fixedCameraY;
            }

            if (useHorizontalBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minXBounds, maxXBounds);
            }

            // Smooth horizontal and vertical camera dampening
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        }

        public void UpdateTargetFloorY()
        {
            if (target != null && snapToFloorHeight)
            {
                int currentFloorIndex = Mathf.FloorToInt(target.position.y / floorHeight);
                fixedCameraY = (currentFloorIndex * floorHeight) + baseFloorCenterY;
            }
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
