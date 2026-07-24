using UnityEngine;
using PizzaOnTop.Player;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class Hazard2D : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController2D player = other.GetComponent<PlayerController2D>();
                if (player != null && !player.IsDead)
                {
                    Debug.Log("[Hazard2D] Player hit hazard! Respawning at entrance...");
                    player.RespawnPlayer();
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
                if (player != null && !player.IsDead)
                {
                    Debug.Log("[Hazard2D] Player touched hazard! Respawning at entrance...");
                    player.RespawnPlayer();
                }
            }
        }
    }
}
