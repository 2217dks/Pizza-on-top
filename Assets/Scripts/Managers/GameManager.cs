using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PizzaOnTop.Managers
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Floor Tracking")]
        [SerializeField] private int totalFloors = 9;
        [SerializeField] private int currentFloorIndex = 1;

        [Header("Pizza Settings")]
        [SerializeField] private float maxPizzaHeat = 100f;
        public float CurrentPizzaHeat { get; private set; }

        [Header("Key & Gun Weapon Tracking")]
        public bool HasSpecialKey { get; set; } = false;
        public bool IsGunUnlocked { get; set; } = false;

        public GameState CurrentState { get; private set; } = GameState.Playing;

        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnFloorChanged;
        public event Action<bool> OnKeyCollected;

        // Persistent Level State Tracking across Scene Loads
        private readonly HashSet<string> brokenPlatformIDs = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentPizzaHeat = maxPizzaHeat;
        }

        private void Start()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimerExpired += HandleTimeExpired;
            }
        }

        private void OnDestroy()
        {
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.OnTimerExpired -= HandleTimeExpired;
            }
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            Debug.Log($"[GameManager] Game state changed to: {newState}");
            OnGameStateChanged?.Invoke(newState);

            if (newState == GameState.GameOver)
            {
                if (TimerManager.Instance != null) TimerManager.Instance.PauseTimer();
            }
            else if (newState == GameState.Victory)
            {
                if (TimerManager.Instance != null) TimerManager.Instance.PauseTimer();
            }
        }

        public void SetCurrentFloor(int floorNumber)
        {
            currentFloorIndex = Mathf.Clamp(floorNumber, 1, totalFloors);
            OnFloorChanged?.Invoke(currentFloorIndex);
            Debug.Log($"[GameManager] Now on Floor {currentFloorIndex} / {totalFloors}");
        }

        public int GetCurrentFloor() => currentFloorIndex;
        public int GetTotalFloors() => totalFloors;

        // --- Key & Weapon Collectible System ---
        public void CollectKey()
        {
            HasSpecialKey = true;
            IsGunUnlocked = true; // Unlocks the gun for subsequent floors / boss fight!
            Debug.Log("[GameManager] Special Key Collected! Gun Weapon Unlocked!");
            OnKeyCollected?.Invoke(true);
        }

        // --- Persistent Broken Platform Traps Tracking ---
        public bool IsPlatformBroken(string platformID)
        {
            if (string.IsNullOrEmpty(platformID)) return false;
            return brokenPlatformIDs.Contains(platformID);
        }

        public void MarkPlatformBroken(string platformID)
        {
            if (string.IsNullOrEmpty(platformID)) return;
            if (!brokenPlatformIDs.Contains(platformID))
            {
                brokenPlatformIDs.Add(platformID);
                Debug.Log($"[GameManager] Platform '{platformID}' permanently marked as broken across scenes!");
            }
        }

        private void HandleTimeExpired()
        {
            SetState(GameState.GameOver);
        }

        public void TriggerVictory()
        {
            SetState(GameState.Victory);
            Debug.Log("[GameManager] Pizza Delivered! You WIN!");
        }

        public void RestartGame()
        {
            currentFloorIndex = 1;
            CurrentPizzaHeat = maxPizzaHeat;
            HasSpecialKey = false;
            IsGunUnlocked = false;
            brokenPlatformIDs.Clear(); // Reset broken trap states on full new game restart

            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ResetTimer();
                TimerManager.Instance.StartTimer();
            }

            SetState(GameState.Playing);
            SceneManager.LoadScene("Floor_01");
        }
    }
}
