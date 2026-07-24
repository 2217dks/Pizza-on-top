using UnityEngine;
using UnityEngine.Events;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class PressurePlate2D : MonoBehaviour
    {
        [Header("Plate Settings")]
        [SerializeField] private bool requireBoxOnly = false; // If true, only PushBox2D triggers it!
        [SerializeField] private Vector3 pressedOffset = new Vector3(0f, -0.1f, 0f);

        [Header("Events")]
        public UnityEvent OnPlatePressed;
        public UnityEvent OnPlateReleased;

        public bool IsPressed { get; private set; } = false;

        private Vector3 unpressedPos;
        private int objectsOnPlateCount = 0;

        private void Awake()
        {
            unpressedPos = transform.position;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsValidTriggerObject(other))
            {
                objectsOnPlateCount++;
                if (!IsPressed)
                {
                    PressPlate();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsValidTriggerObject(other))
            {
                objectsOnPlateCount = Mathf.Max(0, objectsOnPlateCount - 1);
                if (objectsOnPlateCount == 0 && IsPressed)
                {
                    ReleasePlate();
                }
            }
        }

        private bool IsValidTriggerObject(Collider2D other)
        {
            if (other.GetComponent<PushBox2D>() != null) return true;
            if (!requireBoxOnly && other.CompareTag("Player")) return true;
            return false;
        }

        private void PressPlate()
        {
            IsPressed = true;
            transform.position = unpressedPos + pressedOffset;
            Debug.Log($"[PressurePlate2D] Plate '{gameObject.name}' Pressed!");
            OnPlatePressed?.Invoke();
        }

        private void ReleasePlate()
        {
            IsPressed = false;
            transform.position = unpressedPos;
            Debug.Log($"[PressurePlate2D] Plate '{gameObject.name}' Released!");
            OnPlateReleased?.Invoke();
        }
    }
}
