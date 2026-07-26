using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using PizzaOnTop.Player;
using System.Globalization;

namespace PizzaOnTop.Managers
{
    public class TimerManager : MonoBehaviour
    {
        public static TimerManager Instance { get; private set; }

        [Header("Timer Settings")]
        [SerializeField] private float totalTimeInSeconds = 300f; // 5 minutes default
        [SerializeField] private bool autoStartTimer = true;

        [SerializeField] private TextMeshProUGUI timertext;

        [SerializeField] private GameObject gameOverScreen;

        [SerializeField] private InputActionReference restartAction;

        [Header("Time Penalty Settings")]
        [SerializeField] private float trapTimePenalty = 10f; // Deduct 10s on trap hit

        public float RemainingTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsExpired { get; private set; }

        public event Action<float> OnTimerUpdated;
        public event Action OnTimerExpired;
        public event Action<float> OnTimeDeducted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            RemainingTime = totalTimeInSeconds;
        }

        private void OnEnable()
        {
            restartAction.action.Enable();
            restartAction.action.performed += OnRestartAction;
        }

        private void OnDisable()
        {
            restartAction.action.performed -= OnRestartAction;
            restartAction.action.Disable();
        }

        private void Start()
        {
            if (autoStartTimer)
            {
                StartTimer();
            }
        }

        private void OnRestartAction(InputAction.CallbackContext context)
        {
            Restart();
        }

        private void Update()
        {
            if (!IsRunning || IsExpired) return;

            RemainingTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(Mathf.Max(0f, RemainingTime));

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                IsRunning = false;
                IsExpired = true;
                OnTimerExpired?.Invoke();
                OnDeliveryFail();
            }
            timertext.text = RemainingTime.ToString("f0");
        }

        private void OnDeliveryFail()
        {
            gameOverScreen.SetActive(true);
            
            PlayerController2D player = FindAnyObjectByType<PlayerController2D>();
            if (player != null)
            {
                player.SetVelocity(Vector2.zero);
                player.enabled = false;
                if (player.gameObject.GetComponent<PlayerInteract>() != null)
                {
                    player.gameObject.GetComponent<PlayerInteract>().enabled = false;
                }
            }
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Home()
        {
            SceneManager.LoadScene(0);
        }

        public void Credits()
        {
            SceneManager.LoadScene(2);
        }

        public void StartTimer()
        {
            IsRunning = true;
            IsExpired = false;
        }

        public void PauseTimer()
        {
            IsRunning = false;
        }

        public void ResumeTimer()
        {
            if (!IsExpired)
            {
                IsRunning = true;
            }
        }

        public void ResetTimer(float newTimeInSeconds = -1f)
        {
            if (newTimeInSeconds > 0) totalTimeInSeconds = newTimeInSeconds;
            RemainingTime = totalTimeInSeconds;
            IsExpired = false;
            IsRunning = false;
            OnTimerUpdated?.Invoke(RemainingTime);
        }

        /// <summary>
        /// Deducts time penalizing player for hitting a trap or dying.
        /// </summary>
        public void ApplyTrapPenalty(float customPenalty = -1f)
        {
            float penalty = customPenalty > 0 ? customPenalty : trapTimePenalty;
            RemainingTime = Mathf.Max(0f, RemainingTime - penalty);
            OnTimeDeducted?.Invoke(penalty);
            OnTimerUpdated?.Invoke(RemainingTime);
            Debug.Log($"[TimerManager] Penalty applied! Lost {penalty} seconds. Remaining: {RemainingTime:F1}s");

            if (RemainingTime <= 0f && !IsExpired)
            {
                RemainingTime = 0f;
                IsRunning = false;
                IsExpired = true;
                OnTimerExpired?.Invoke();
            }
        }

        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(RemainingTime / 60f);
            int seconds = Mathf.FloorToInt(RemainingTime % 60f);
            int milliseconds = Mathf.FloorToInt((RemainingTime * 100f) % 100f);
            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
    }
}
