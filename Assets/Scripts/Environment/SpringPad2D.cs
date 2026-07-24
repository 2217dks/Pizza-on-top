using UnityEngine;
using PizzaOnTop.Player;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class SpringPad2D : MonoBehaviour
    {
        [SerializeField] private float bounceForce = 22f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            Bounce(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Bounce(collision.gameObject);
        }

        private void Bounce(GameObject obj)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerController2D player = obj.GetComponent<PlayerController2D>();
                if (player != null && player.RB != null)
                {
                    player.RB.linearVelocity = new Vector2(player.RB.linearVelocity.x, bounceForce);
                    Debug.Log("[SpringPad2D] Player bounced!");
                }
            }
        }
    }
}
