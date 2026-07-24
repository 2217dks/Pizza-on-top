using UnityEngine;
using PizzaOnTop.Managers;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class KeyCollectible2D : MonoBehaviour
    {
        [Header("Animation & Polish")]
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float rotateSpeed = 90f;

        private Vector3 startPos;
        private bool isCollected = false;

        private void Awake()
        {
            startPos = transform.position;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Start()
        {
            // Check if key was already collected in game run
            if (GameManager.Instance != null && GameManager.Instance.HasSpecialKey)
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (isCollected) return;

            // Gentle floating bob & rotate animation
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player"))
            {
                isCollected = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CollectKey();
                }

                Debug.Log("[KeyCollectible2D] Key collected by player!");
                gameObject.SetActive(false);
            }
        }
    }
}
