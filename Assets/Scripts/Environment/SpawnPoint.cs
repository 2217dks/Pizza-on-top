using UnityEngine;

namespace PizzaOnTop.Environment
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnID = "default";
        public string SpawnID => spawnID;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, Vector3.up * 1f);
        }
    }
}
