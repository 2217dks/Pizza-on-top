using System.Collections;
using UnityEngine;

namespace PizzaOnTop.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class CrumblingPlatform2D : MonoBehaviour
    {
        [Header("Crumble Settings")]
        [SerializeField] private float crumbleDelay = 0.4f;
        [SerializeField] private float respawnDelay = 3.0f;
        [SerializeField] private float shakeIntensity = 0.05f;

        private Collider2D platformCollider;
        private SpriteRenderer spriteRenderer;
        private Vector3 initialPosition;
        private bool isCrumbling = false;

        private void Awake()
        {
            platformCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            initialPosition = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isCrumbling) return;

            if (collision.gameObject.CompareTag("Player"))
            {
                // Check if player landed on top
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f)
                    {
                        StartCoroutine(RoutineCrumble());
                        break;
                    }
                }
            }
        }

        private IEnumerator RoutineCrumble()
        {
            isCrumbling = true;

            // Shaking effect before collapse
            float timer = 0;
            while (timer < crumbleDelay)
            {
                timer += Time.deltaTime;
                transform.position = initialPosition + (Vector3)Random.insideUnitCircle * shakeIntensity;
                yield return null;
            }

            // Disable platform -> Player falls to lower floor/redo section!
            transform.position = initialPosition;
            if (platformCollider != null) platformCollider.enabled = false;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            Debug.Log("[CrumblingPlatform2D] Platform broke! Player falls!");

            yield return new WaitForSeconds(respawnDelay);

            // Re-enable platform
            if (platformCollider != null) platformCollider.enabled = true;
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            isCrumbling = false;
        }
    }
}
