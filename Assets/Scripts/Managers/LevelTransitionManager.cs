using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using PizzaOnTop.Environment;

namespace PizzaOnTop.Managers
{
    public class LevelTransitionManager : MonoBehaviour
    {
        public static LevelTransitionManager Instance { get; private set; }

        public string TargetSpawnPointID { get; private set; } = "default";
        public bool IsTransitioning { get; private set; }

        public event Action OnTransitionStarted;
        public event Action OnTransitionCompleted;

        private readonly WaitForSeconds transitionDelay = new WaitForSeconds(0.2f);
        private readonly WaitForEndOfFrame waitForFrameEnd = new WaitForEndOfFrame();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void LoadFloorScene(string sceneName, string spawnPointID, int floorIndex = -1)
        {
            if (IsTransitioning) return;
            StartCoroutine(RoutineLoadScene(sceneName, spawnPointID, floorIndex));
        }

        private IEnumerator RoutineLoadScene(string sceneName, string spawnPointID, int floorIndex)
        {
            IsTransitioning = true;
            TargetSpawnPointID = spawnPointID;
            OnTransitionStarted?.Invoke();

            yield return transitionDelay;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            if (floorIndex > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentFloor(floorIndex);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(RoutinePositionPlayerAtSpawn());
        }

        private IEnumerator RoutinePositionPlayerAtSpawn()
        {
            yield return waitForFrameEnd;

            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                Transform chosenPoint = null;

                foreach (SpawnPoint sp in spawnPoints)
                {
                    if (sp.SpawnID == TargetSpawnPointID)
                    {
                        chosenPoint = sp.transform;
                        break;
                    }
                }

                if (chosenPoint != null)
                {
                    playerObj.transform.position = chosenPoint.position;
                    Debug.Log($"[LevelTransitionManager] Player positioned at SpawnPoint '{chosenPoint.name}' ({chosenPoint.position})");
                }
            }

            IsTransitioning = false;
            OnTransitionCompleted?.Invoke();
        }
    }
}
