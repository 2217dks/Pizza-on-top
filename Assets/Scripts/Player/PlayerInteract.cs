using UnityEngine;
using UnityEngine.InputSystem;

namespace PizzaOnTop.Player
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact();
    }

    public class PlayerInteract : MonoBehaviour
    {
        [Header("Door Interaction Circle Tuning")]
        [SerializeField] private float interactRadius = 1.2f; // Control circle size in Inspector!
        [SerializeField] private Vector2 centerOffset = new Vector2(0f, 0f); // Control position offset!
        [SerializeField] private LayerMask interactableLayer;

        private IInteractable currentInteractable;
        private Collider2D playerCollider;

        private void Awake()
        {
            playerCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            CheckInteractable();

            Keyboard keyboard = Keyboard.current;
            bool interactPressed = false;

            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame))
            {
                interactPressed = true;
            }

            if (currentInteractable != null && interactPressed)
            {
                currentInteractable.Interact();
            }
        }

        private Vector2 GetCenterPosition()
        {
            if (playerCollider != null)
            {
                return (Vector2)playerCollider.bounds.center + centerOffset;
            }
            return (Vector2)transform.position + centerOffset;
        }

        private void CheckInteractable()
        {
            Vector2 checkPos = GetCenterPosition();
            Collider2D col = Physics2D.OverlapCircle(checkPos, interactRadius, interactableLayer);
            if (col != null)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    currentInteractable = interactable;
                    return;
                }
            }

            currentInteractable = null;
        }

        public IInteractable GetCurrentInteractable() => currentInteractable;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetCenterPosition(), interactRadius);
        }
    }
}
