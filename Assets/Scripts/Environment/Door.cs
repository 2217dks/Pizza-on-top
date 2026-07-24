using UnityEngine;
using PizzaOnTop.Player;
using PizzaOnTop.Managers;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Target Floor Destination")]
        [SerializeField] private string targetSceneName = "Floor_02";
        [SerializeField] private string targetSpawnPointID = "default";
        [SerializeField] private int targetFloorIndex = 2;

        [Header("Goal Setting")]
        [SerializeField] private bool isRoofDeliveryGoal = false;
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
                Debug.Log("[Door] Reached roof! Delivering pizza...");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerVictory();
                }
            }
            else
            {
                Debug.Log($"[Door] Entering door to '{targetSceneName}' at spawn '{targetSpawnPointID}'");
                if (LevelTransitionManager.Instance != null)
                {
                    LevelTransitionManager.Instance.LoadFloorScene(targetSceneName, targetSpawnPointID, targetFloorIndex);
                }
            }
        }
    }
}
