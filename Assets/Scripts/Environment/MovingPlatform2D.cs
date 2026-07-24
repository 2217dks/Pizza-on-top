using UnityEngine;

namespace PizzaOnTop.Environment
{
    public class MovingPlatform2D : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float waitTimeAtWaypoint = 0.5f;

        private int currentTargetIndex = 0;
        private float waitTimer;

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            Transform target = waypoints[currentTargetIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.05f)
            {
                currentTargetIndex = (currentTargetIndex + 1) % waypoints.Length;
                waitTimer = waitTimeAtWaypoint;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.transform.SetParent(null);
            }
        }
    }
}
