using UnityEngine;
using UnityEngine.UI;
using PizzaOnTop.Managers;
using PizzaOnTop.Player;

namespace PizzaOnTop.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI Text References (Text or TextMeshProUGUI)")]
        [SerializeField] private Text timerText;
        [SerializeField] private Text floorText;
        [SerializeField] private Text promptText;

        [Header("Overlays")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        private PlayerInteract playerInteract;

        private void Start()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimerUpdated += UpdateTimerUI;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFloorChanged += UpdateFloorUI;
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                UpdateFloorUI(GameManager.Instance.GetCurrentFloor());
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerInteract = player.GetComponent<PlayerInteract>();
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (promptText != null) promptText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimerUpdated -= UpdateTimerUI;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFloorChanged -= UpdateFloorUI;
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void Update()
        {
            UpdateInteractionPrompt();
        }

        private void UpdateTimerUI(float remainingSeconds)
        {
            if (timerText != null && TimerManager.Instance != null)
            {
                timerText.text = $"TIME: {TimerManager.Instance.GetFormattedTime()}";
                timerText.color = remainingSeconds < 30f ? Color.red : Color.white;
            }
        }

        private void UpdateFloorUI(int floorIndex)
        {
            if (floorText != null)
            {
                int total = GameManager.Instance != null ? GameManager.Instance.GetTotalFloors() : 9;
                floorText.text = $"FLOOR {floorIndex} / {total}";
            }
        }

        private void UpdateInteractionPrompt()
        {
            if (promptText == null) return;

            if (playerInteract == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null) playerInteract = player.GetComponent<PlayerInteract>();
            }

            if (playerInteract != null)
            {
                IInteractable interactable = playerInteract.GetCurrentInteractable();
                if (interactable != null)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = interactable.GetInteractionPrompt();
                    return;
                }
            }

            promptText.gameObject.SetActive(false);
        }

        private void HandleGameStateChanged(GameState newState)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(newState == GameState.GameOver);
            if (victoryPanel != null) victoryPanel.SetActive(newState == GameState.Victory);
        }

        public void OnRestartButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
    }
}
