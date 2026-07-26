using System.Collections.Generic;
using UnityEngine;

namespace PizzaOnTop.Environment
{
    public class SpawnPoint : MonoBehaviour
    {
        public static readonly Dictionary<int, SpawnPoint> All = new();

        [SerializeField] private int floorIndex;

        [Header("Initial Spawn")]
        [SerializeField] private GameObject playerPrefab;

        public int FloorIndex => floorIndex;

        private void Awake()
        {
            All[floorIndex] = this;
        }

        private void Start()
        {
            // Spawn the player at Floor 0 if one doesn't already exist.
            if (floorIndex != 0)
                return;

            if (GameObject.FindWithTag("Player") != null)
                return;

            if (playerPrefab == null)
            {
                Debug.LogWarning("No Player Prefab assigned to the Floor 0 SpawnPoint.");
                return;
            }

            Instantiate(playerPrefab, transform.position, Quaternion.identity).name = "Player";
        }

        private void OnDestroy()
        {
            if (All.TryGetValue(floorIndex, out var spawn) && spawn == this)
                All.Remove(floorIndex);
        }
    }
}