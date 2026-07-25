using UnityEngine;
using PizzaOnTop.Player;
using PizzaOnTop.Managers;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Target Floor Teleport Settings")]
        [SerializeField] private string targetSpawnPointID = "Spawn_Floor_02";   // ID matching target SpawnPoint
        [SerializeField] private int targetFloorIndex = 2;                        // Level/Floor index (1 to 9)

        [Header("Goal Setting")]
        [SerializeField] private bool isRoofDeliveryGoal = false;                 // Final roof customer pizza delivery goal!
        [SerializeField] private string promptText = "Press E to Enter Floor";

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public string GetInteractionPrompt()
        {
            if (isRoofDeliveryGoal) return "Press E to Deliver Pizza!";
            return $"{promptText} {targetFloorIndex}";
        }

        public void Interact()
        {
            if (isRoofDeliveryGoal)
            {
                Debug.Log("[Door] Reached roof! Delivering pizza to customer...");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerVictory();
                }
            }
            else
            {
                Debug.Log($"[Door] Entering door to floor {targetFloorIndex} (Spawn: '{targetSpawnPointID}')");
                if (LevelTransitionManager.Instance != null)
                {
                    LevelTransitionManager.Instance.TeleportToSpawnPoint(targetSpawnPointID, targetFloorIndex);
                }
            }
        }
    }
}
