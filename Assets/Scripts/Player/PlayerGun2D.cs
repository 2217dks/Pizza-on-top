using UnityEngine;
using UnityEngine.InputSystem;
using PizzaOnTop.Managers;

namespace PizzaOnTop.Player
{
    public class PlayerGun2D : MonoBehaviour
    {
        [Header("Weapon References")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.25f;

        private SpriteRenderer playerSprite;
        private float nextFireTime;

        private void Awake()
        {
            playerSprite = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // Gun is unlocked when key is collected or explicitly enabled in GameManager
            bool isUnlocked = (GameManager.Instance != null && (GameManager.Instance.IsGunUnlocked || GameManager.Instance.HasSpecialKey));
            if (!isUnlocked) return;

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool shootPressed = (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                                (keyboard != null && keyboard.fKey.wasPressedThisFrame);

            if (!shootPressed)
            {
                try { if (Input.GetKeyDown(KeyCode.F)) shootPressed = true; } catch { }
            }

            if (shootPressed && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        private void Shoot()
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("[PlayerGun2D] bulletPrefab is missing!");
                return;
            }

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            float facingDir = (playerSprite != null && playerSprite.flipX) ? -1f : 1f;
            Vector2 shootDir = new Vector2(facingDir, 0f);

            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet2D bullet = bulletObj.GetComponent<Bullet2D>();
            if (bullet != null)
            {
                bullet.Initialize(shootDir);
            }

            Debug.Log("[PlayerGun2D] Fired bullet!");
        }
    }
}
