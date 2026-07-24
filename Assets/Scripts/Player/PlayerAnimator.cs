using System.Collections.Generic;
using UnityEngine;

namespace PizzaOnTop.Player
{
    [RequireComponent(typeof(PlayerController2D), typeof(SpriteRenderer))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Animator Reference")]
        [SerializeField] private Animator animator;

        [Header("Direct Clip Names")]
        [SerializeField] private string idleClipName = "Player_Idle";
        [SerializeField] private string runClipName = "Player_Run";
        [SerializeField] private string jumpClipName = "Player_Jump";
        [SerializeField] private string ropeClipName = "Player_Rope";
        [SerializeField] private string hurtClipName = "Player_Hurt";
        [SerializeField] private string deadClipName = "Player_Dead";
        [SerializeField] private string attackClipName = "Player_Attack";

        [Header("Squash & Stretch Polish")]
        [SerializeField] private bool useSquashAndStretch = true;
        [SerializeField] private Vector3 jumpSquashScale = new Vector3(0.75f, 1.25f, 1f);
        [SerializeField] private Vector3 landSquashScale = new Vector3(1.25f, 0.75f, 1f);
        [SerializeField] private float squashDampSpeed = 12f;

        private PlayerController2D playerController;
        private SpriteRenderer spriteRenderer;
        private Vector3 originalScale;
        private bool wasGroundedLastFrame;
        private string currentClip = "";
        private HashSet<int> existingParamHashes = new HashSet<int>();

        // Parameter Names
        private const string IsGroundedParam = "IsGrounded";
        private const string SpeedParam = "Speed";
        private const string VerticalVelocityParam = "VerticalVelocity";
        private const string IsOnRopeParam = "IsOnRope";
        private const string IsDeadParam = "IsDead";
        private const string JumpTriggerParam = "Jump";
        private const string HurtTriggerParam = "Hurt";
        private const string AttackTriggerParam = "Attack";

        private void Awake()
        {
            playerController = GetComponent<PlayerController2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();
            originalScale = transform.localScale;

            CacheAnimatorParameters();
        }

        private void CacheAnimatorParameters()
        {
            existingParamHashes.Clear();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    existingParamHashes.Add(param.nameHash);
                }
            }
        }

        private void Update()
        {
            if (playerController == null) return;

            Vector2 vel = playerController.GetVelocity();
            bool isGrounded = playerController.IsGrounded;
            bool isOnRope = playerController.IsOnRope;
            bool isDead = playerController.IsDead;

            // 1. Flip Sprite Facing
            if (!isOnRope && Mathf.Abs(vel.x) > 0.1f)
            {
                spriteRenderer.flipX = vel.x < 0;
            }

            // 2. Safe Parameter Setting & Direct State Playback
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                SetSafeBool(IsGroundedParam, isGrounded);
                SetSafeFloat(SpeedParam, Mathf.Abs(vel.x));
                SetSafeFloat(VerticalVelocityParam, vel.y);
                SetSafeBool(IsOnRopeParam, isOnRope);
                SetSafeBool(IsDeadParam, isDead);

                if (isDead)
                {
                    ChangeAnimationState(deadClipName);
                }
                else if (isOnRope)
                {
                    if (!ChangeAnimationState(ropeClipName))
                    {
                        ChangeAnimationState(idleClipName);
                    }
                }
                else if (!isGrounded)
                {
                    if (!ChangeAnimationState(jumpClipName))
                    {
                        ChangeAnimationState(idleClipName);
                    }
                }
                else
                {
                    if (Mathf.Abs(vel.x) > 0.1f)
                    {
                        ChangeAnimationState(runClipName);
                    }
                    else
                    {
                        ChangeAnimationState(idleClipName);
                    }
                }
            }

            // 3. Procedural Jump & Land Squash-and-Stretch
            if (useSquashAndStretch)
            {
                if (isGrounded && !wasGroundedLastFrame && vel.y <= 0.1f)
                {
                    transform.localScale = Vector3.Scale(originalScale, landSquashScale);
                }

                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * squashDampSpeed);
            }

            wasGroundedLastFrame = isGrounded;
        }

        private void SetSafeBool(string paramName, bool value)
        {
            int hash = Animator.StringToHash(paramName);
            if (existingParamHashes.Contains(hash))
            {
                animator.SetBool(hash, value);
            }
        }

        private void SetSafeFloat(string paramName, float value)
        {
            int hash = Animator.StringToHash(paramName);
            if (existingParamHashes.Contains(hash))
            {
                animator.SetFloat(hash, value);
            }
        }

        private void SetSafeTrigger(string paramName)
        {
            int hash = Animator.StringToHash(paramName);
            if (existingParamHashes.Contains(hash))
            {
                animator.SetTrigger(hash);
            }
        }

        private bool ChangeAnimationState(string newClip)
        {
            if (string.IsNullOrEmpty(newClip)) return false;
            if (currentClip == newClip) return true;

            int stateHash = Animator.StringToHash(newClip);
            if (animator != null && animator.HasState(0, stateHash))
            {
                animator.Play(stateHash);
                currentClip = newClip;
                return true;
            }

            return false;
        }

        public void TriggerJumpVisual()
        {
            SetSafeTrigger(JumpTriggerParam);

            if (useSquashAndStretch)
            {
                transform.localScale = Vector3.Scale(originalScale, jumpSquashScale);
            }
        }

        public void TriggerHurtVisual()
        {
            SetSafeTrigger(HurtTriggerParam);
            ChangeAnimationState(hurtClipName);
        }

        public void TriggerAttackVisual()
        {
            SetSafeTrigger(AttackTriggerParam);
            ChangeAnimationState(attackClipName);
        }
    }
}
