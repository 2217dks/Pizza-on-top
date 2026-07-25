using UnityEngine;

namespace PizzaOnTop.Environment
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnID = "default";
        [SerializeField] private GameObject playerPrefab;          // Drag Player.prefab here for auto-spawning!
        [SerializeField] private bool autoSpawnIfMissing = true;

        public string SpawnID => spawnID;

        private void Start()
        {
            if (autoSpawnIfMissing)
            {
                EnsurePlayerExists();
            }
        }

        public void EnsurePlayerExists()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null && playerPrefab != null)
            {
                playerObj = Instantiate(playerPrefab, transform.position, Quaternion.identity);
                playerObj.name = "Player";
                Debug.Log($"[SpawnPoint] Automatically spawned Player prefab at '{spawnID}' ({transform.position})!");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, Vector3.up * 1f);
        }
    }
}
