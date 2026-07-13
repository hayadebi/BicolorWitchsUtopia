using UnityEngine;
using System;
using BicolorWitch.Core;
using BicolorWitch.Magic;
using BicolorWitch.InputManagement;

namespace BicolorWitch.Player
{
    /// <summary>
    /// プレイヤーキャラクター（姉妹）の移動、ジャンプ、基本攻撃、切り替え処理を管理する。//
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        // Inspector設定項目
        [Header("移動設定")]
        [SerializeField, Tooltip("プレイヤーの通常移動速度")]
        private float moveSpeed = 5f;
        [SerializeField, Tooltip("プレイヤーのジャンプ力")]
        private float jumpForce = 10f;
        [SerializeField, Tooltip("地面として判定するレイヤーマスク")]
        private LayerMask groundLayer;
        [SerializeField, Tooltip("地面判定用のRaycastの原点からのオフセット")]
        private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
        [SerializeField, Tooltip("地面判定用のRaycastの長さ")]
        private float groundCheckDistance = 0.1f;

        [Header("ダッシュ設定")]
        [SerializeField, Tooltip("プレイヤーのダッシュ移動速度")]
        private float dashSpeed = 8f;
        [SerializeField, Tooltip("ダッシュクールタイムの最大値")]
        private float maxDashCooldown = 3f;
        public float MaxDashCooldown => maxDashCooldown;
        [SerializeField, Tooltip("ダッシュ中のクールタイム増加速度")]
        private float dashCooldownIncreaseRate = 1f;
        [SerializeField, Tooltip("ダッシュしていない時のクールタイム回復速度")]
        private float dashCooldownRecoveryRate = 0.5f;

        [Header("攻撃設定")]
        [SerializeField, Tooltip("基本攻撃のクールタイム")]
        private float basicAttackCooldown = 0.5f;

        [Header("ゲームオーバー設定")]
        [SerializeField, Tooltip("プレイヤーがこのY座標を下回るとゲームオーバーになる閾値")]
        private float fallThresholdY = -10f;

        // プライベート変数
        private Rigidbody2D rb;
        private Animator animator;

        private float currentMoveInput = 0f;
        private bool isGrounded;
        private bool canBasicAttack = true;
        private float basicAttackTimer = 0f;
        private bool isDead = false;
        private bool isDashing = false;
        private float currentDashCooldown = 0f;

        // ダッシュクールタイムが変更されたことを通知するイベント (現在のクールタイム, 最大クールタイム)
        public event Action<float, float> OnDashCooldownChanged;

        private const string ANIM_PARAM_SPEED = "Speed";
        private const string ANIM_PARAM_JUMP = "Jump";
        private const string ANIM_PARAM_ATTACK = "Attack";
        private const string ANIM_PARAM_DEAD = "Dead";

        private void HandleMoveInput(float input)
        {
            currentMoveInput = input;
        }

        private void HandleJumpInput()
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger(ANIM_PARAM_JUMP);
            }
        }

        private void HandleBasicAttackInput()
        {
            if (canBasicAttack)
            {
                animator.SetTrigger(ANIM_PARAM_ATTACK);
                canBasicAttack = false;
                basicAttackTimer = basicAttackCooldown;
                // 実際の攻撃処理（当たり判定など）はアニメーションイベント等で呼び出す
            }
        }

        private void HandleSwitchCharacterInput()
        {
            // CharacterSwitcher.Instance は BicolorWitch.Player 名前空間にあると仮定
            if (Player.CharacterSwitcher.Instance != null)
            {
                if (!Player.CharacterSwitcher.Instance.TrySwitchCharacter())
                {
                    Debug.LogWarning("キャラクター切り替えに失敗しました。HPが0のキャラクターには切り替えできません。", this);
                }
            }
        }

        private void HandleMagic1Input()
        {
            if (MagicSystem.Instance != null)
            {
                MagicSystem.Instance.TryCastMagic(1);
            }
        }

        private void HandleMagic2Input()
        {
            if (MagicSystem.Instance != null)
            {
                MagicSystem.Instance.TryCastMagic(2);
            }
        }

        private void HandleDashInputStart()
        {
            if (currentDashCooldown < maxDashCooldown)
            {
                isDashing = true;
            }
        }

        private void HandleDashInputEnd()
        {
            isDashing = false;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            if (GManager.Instance == null)
            {
                Debug.LogError("GManager.Instanceが見つかりません。", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (InputManagement.InputManager.Instance != null)
            {
                InputManager.Instance.OnMoveInput += HandleMoveInput;
                InputManager.Instance.OnJumpInput += HandleJumpInput;
                InputManager.Instance.OnBasicAttackInput += HandleBasicAttackInput;
                InputManager.Instance.OnSwitchCharacterInput += HandleSwitchCharacterInput;
                InputManager.Instance.OnMagicInput1 += HandleMagic1Input;
                InputManager.Instance.OnMagicInput2 += HandleMagic2Input;
                InputManager.Instance.OnDashInputStart += HandleDashInputStart;
                InputManager.Instance.OnDashInputEnd += HandleDashInputEnd;
            }
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMoveInput -= HandleMoveInput;
                InputManager.Instance.OnJumpInput -= HandleJumpInput;
                InputManager.Instance.OnBasicAttackInput -= HandleBasicAttackInput;
                InputManager.Instance.OnSwitchCharacterInput -= HandleSwitchCharacterInput;
                InputManager.Instance.OnMagicInput1 -= HandleMagic1Input;
                InputManager.Instance.OnMagicInput2 -= HandleMagic2Input;
                InputManager.Instance.OnDashInputStart -= HandleDashInputStart;
                InputManager.Instance.OnDashInputEnd -= HandleDashInputEnd;
            }
        }

        private void Start()
        {
            OnDashCooldownChanged?.Invoke(currentDashCooldown, maxDashCooldown);
        }

        private void Update()
        {
            if (isDead || GManager.Instance.currentGameState != GManager.GameState.Playing) return;

            CheckGroundStatus();
            UpdateBasicAttackCooldown();
            UpdateDashCooldown();
            ApplyMovement();
            UpdateAnimation();
            CheckFallForGameOver();

            if (Player.CharacterSwitcher.Instance != null && Player.CharacterSwitcher.Instance.AreAllCharactersDead)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// 地面との接触状態をチェックする。
        /// </summary>
        private void CheckGroundStatus()
        {
            Vector2 rayOrigin = (Vector2)transform.position + groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
            isGrounded = hit.collider != null;
        }

        /// <summary>
        /// 移動を適用する。
        /// </summary>
        private void ApplyMovement()
        {
            float speed = isDashing && currentDashCooldown < maxDashCooldown ? dashSpeed : moveSpeed;
            rb.linearVelocity = new Vector2(currentMoveInput * speed, rb.linearVelocity.y);

            if (currentMoveInput != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(currentMoveInput), 1, 1);
            }
        }

        /// <summary>
        /// 基本攻撃のクールタイムを更新する。
        /// </summary>
        private void UpdateBasicAttackCooldown()
        {
            if (!canBasicAttack)
            {
                basicAttackTimer -= Time.deltaTime;
                if (basicAttackTimer <= 0f)
                {
                    canBasicAttack = true;
                }
            }
        }

        /// <summary>
        /// ダッシュクールタイムを更新する。
        /// </summary>
        private void UpdateDashCooldown()
        {
            if (isDashing && currentDashCooldown < maxDashCooldown)
            {
                currentDashCooldown += Time.deltaTime * dashCooldownIncreaseRate;
                currentDashCooldown = Mathf.Min(currentDashCooldown, maxDashCooldown);
            }
            else if (!isDashing && currentDashCooldown > 0)
            {
                currentDashCooldown -= Time.deltaTime * dashCooldownRecoveryRate;
                currentDashCooldown = Mathf.Max(currentDashCooldown, 0f);
            }
            // UI.UIManager.Instance は BicolorWitch.UI 名前空間にあると仮定
            if (UI.UIManager.Instance != null && Player.CharacterSwitcher.Instance != null)
            {
                UI.UIManager.Instance.UpdateDashCooldown(Player.CharacterSwitcher.Instance.GetActiveCharacterType(), currentDashCooldown, maxDashCooldown);
            }
            OnDashCooldownChanged?.Invoke(currentDashCooldown, maxDashCooldown);
        }

        /// <summary>
        /// アニメーションを更新する。
        /// </summary>
        private void UpdateAnimation()
        {
            animator.SetFloat(ANIM_PARAM_SPEED, Mathf.Abs(currentMoveInput));
        }

        /// <summary>
        /// プレイヤーが落下しすぎた場合にゲームオーバーをトリガーする。
        /// </summary>
        private void CheckFallForGameOver()
        {
            if (transform.position.y < fallThresholdY)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// ゲームオーバー処理をトリガーする。
        /// </summary>
        private void TriggerGameOver()
        {
            if (!isDead)
            {
                isDead = true;
                animator.SetTrigger(ANIM_PARAM_DEAD);
                Debug.Log("PlayerController: ゲームオーバー！", this);
                GManager.Instance?.GameOver();
                enabled = false;
            }
        }

        /// <summary>
        /// プレイヤーをリセットする。
        /// </summary>
        public void ResetPlayer()
        {
            isDead = false;
            currentDashCooldown = 0f;
            canBasicAttack = true;
            basicAttackTimer = 0f;
            isDashing = false;
            enabled = true;
            animator.SetTrigger("Reset");
            OnDashCooldownChanged?.Invoke(currentDashCooldown, maxDashCooldown);
        }
    }
}
