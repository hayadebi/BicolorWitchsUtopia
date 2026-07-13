using UnityEngine;
using System;
using BicolorWitch.Player; // IHPManagerとCharacterTypeを参照するために追加

namespace BicolorWitch.Player
{
    /// <summary>
    /// プレイヤーの入力イベントを提供するインターフェース。
    /// InputManagerが実装することを想定。
    /// </summary>
    public interface IInputProvider
    {
        event Action<float> OnMoveInput;
        event Action OnJumpInput;
        event Action OnBasicAttackInput;
        event Action OnSwitchCharacterInput;
        event Action OnMagicInput1;
        event Action OnMagicInput2;
        event Action OnDashInputStart; // ダッシュ開始
        event Action OnDashInputEnd;   // ダッシュ終了
    }

    /// <summary>
    /// 魔法システム機能を提供するインターフェース。
    /// MagicSystemが実装することを想定。
    /// </summary>
    public interface IMagicSystem
    {
        /// <summary>
    /// 指定されたスロットの魔法の発動を試みる。
    /// </summary>
        /// <param name="slotIndex">魔法スロットのインデックス (例: 1または2)。</param>
        /// <returns>魔法の発動が成功した場合はtrue、失敗した場合はfalse。</returns>
        bool TryCastMagic(int slotIndex);
    }

    /// <summary>
    /// ゲームオーバーイベントを通知するインターフェース。
    /// GameManagerが実装することを想定。
    /// </summary>
    public interface IGameOverNotifier
    {
        /// <summary>
    /// ゲームオーバーを通知するイベント。
    /// </summary>
        event Action OnGameOver;
    }

    /// <summary>
    /// プレイヤーキャラクター（姉妹）の移動、ジャンプ、基本攻撃、切り替え処理を管理する。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        // Inspector設定項目
        [Header("移動設定")]
        // Inspector: プレイヤーの通常移動速度を設定
        [SerializeField, Tooltip("プレイヤーの通常移動速度")]
        private float moveSpeed = 5f;
        // Inspector: プレイヤーのジャンプ力を設定
        [SerializeField, Tooltip("プレイヤーのジャンプ力")]
        private float jumpForce = 10f;
        // Inspector: 地面判定用のレイヤーマスクを設定
        [SerializeField, Tooltip("地面として判定するレイヤーマスク")]
        private LayerMask groundLayer;
        // Inspector: 地面判定用のRaycastの原点からのオフセットを設定
        [SerializeField, Tooltip("地面判定用のRaycastの原点からのオフセット")]
        private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
        // Inspector: 地面判定用のRaycastの長さを設定
        [SerializeField, Tooltip("地面判定用のRaycastの長さ")]
        private float groundCheckDistance = 0.1f;

        [Header("ダッシュ設定")]
        // Inspector: プレイヤーのダッシュ移動速度を設定
        [SerializeField, Tooltip("プレイヤーのダッシュ移動速度")]
        private float dashSpeed = 8f;
        // Inspector: ダッシュクールタイムの最大値を設定
        [SerializeField, Tooltip("ダッシュクールタイムの最大値")]
        private float maxDashCooldown = 3f;
        public float MaxDashCooldown => maxDashCooldown; // Public getterを追加
        // Inspector: ダッシュ中のクールタイム増加速度を設定
        [SerializeField, Tooltip("ダッシュ中のクールタイム増加速度")]
        private float dashCooldownIncreaseRate = 1f;
        // Inspector: ダッシュしていない時のクールタイム回復速度を設定
        [SerializeField, Tooltip("ダッシュしていない時のクールタイム回復速度")]
        private float dashCooldownRecoveryRate = 0.5f;

        [Header("攻撃設定")]
        // Inspector: 基本攻撃のクールタイムを設定
        [SerializeField, Tooltip("基本攻撃のクールタイム")]
        private float basicAttackCooldown = 0.5f;

        [Header("ゲームオーバー設定")]
        // Inspector: プレイヤーがこのY座標を下回るとゲームオーバーになる
        [SerializeField, Tooltip("プレイヤーがこのY座標を下回るとゲームオーバーになる閾値")]
        private float fallThresholdY = -10f;

        [Header("依存コンポーネント")]
        // Inspector: InputManagerを実装したオブジェクトを設定
        [SerializeField, Tooltip("入力イベントを提供するInputManagerを実装したオブジェクト")]
        private MonoBehaviour inputProviderMono;
        // Inspector: CharacterSwitcherを実装したオブジェクトを設定
        [SerializeField, Tooltip("キャラクター切り替え機能を提供するCharacterSwitcherを実装したオブジェクト")]
        private MonoBehaviour characterSwitcherMono;
        // Inspector: MagicSystemを実装したオブジェクトを設定
        [SerializeField, Tooltip("魔法システム機能を提供するMagicSystemを実装したオブジェクト")]
        private MonoBehaviour magicSystemMono;
        // Inspector: HPManagerを実装したオブジェクトを設定（現在はCharacterSwitcher経由でHP状態を把握するため直接は使用しないが、将来的な拡張のために残す）
        [SerializeField, Tooltip("HP管理機能を提供するHPManagerを実装したオブジェクト")]
        private MonoBehaviour hpManagerMono; // IHPManagerを実装したMonoBehaviour (将来的な拡張のために残す)
        // Inspector: ゲームオーバーを通知するオブジェクトを設定
        [SerializeField, Tooltip("ゲームオーバーを通知するIGameOverNotifierを実装したオブジェクト")]
        private MonoBehaviour gameOverNotifierMono;

        // プライベート変数
        private Rigidbody2D rb;
        private Animator animator;
        private IInputProvider inputProvider;
        private ICharacterSwitcher characterSwitcher;
        private IMagicSystem magicSystem;
        private IGameOverNotifier gameOverNotifier;

        private float currentMoveInput = 0f;
        private bool isGrounded;
        private bool canBasicAttack = true;
        private float basicAttackTimer = 0f;
        private bool isDead = false; // PlayerController自身の死亡状態（操作不能状態）
        private bool isDashing = false; // ダッシュ中かどうか
        private float currentDashCooldown = 0f;

        // ゲームオーバーイベント
        public event Action OnGameOverEvent;
        // ダッシュクールタイムが変更されたことを通知するイベント (現在のクールタイム, 最大クールタイム)
        public event Action<float, float> OnDashCooldownChanged;

        private const string ANIM_PARAM_SPEED = "Speed";
        private const string ANIM_PARAM_JUMP = "Jump";
        private const string ANIM_PARAM_ATTACK = "Attack";
        private const string ANIM_PARAM_DEAD = "Dead"; // 死亡アニメーション用

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // 依存コンポーネントの取得とNullチェック
            if (inputProviderMono == null || !(inputProviderMono is IInputProvider))
            {
                Debug.LogError("InputProviderMonoが設定されていないか、IInputProviderを実装していません。", this);
                enabled = false; // スクリプトを無効化
                return;
            }
            inputProvider = inputProviderMono as IInputProvider;

            if (characterSwitcherMono == null || !(characterSwitcherMono is ICharacterSwitcher))
            {
                Debug.LogError("CharacterSwitcherMonoが設定されていないか、ICharacterSwitcherを実装していません。", this);
                enabled = false;
                return;
            }
            characterSwitcher = characterSwitcherMono as ICharacterSwitcher;

            if (magicSystemMono == null || !(magicSystemMono is IMagicSystem))
            {
                Debug.LogError("MagicSystemMonoが設定されていないか、IMagicSystemを実装していません。", this);
                enabled = false;
                return;
            }
            magicSystem = magicSystemMono as IMagicSystem;

            // HPManagerMonoは直接使用しないが、設定されているか確認
            // CharacterSwitcherがIHPManagerと連携して死亡状態を管理することを想定する。
            if (hpManagerMono != null && !(hpManagerMono is IHPManager))
            {
                Debug.LogWarning("HPManagerMonoが設定されていますが、IHPManagerを実装していません。", this);
            }

            if (gameOverNotifierMono == null || !(gameOverNotifierMono is IGameOverNotifier))
            {
                Debug.LogError("GameOverNotifierMonoが設定されていないか、IGameOverNotifierを実装していません。", this);
                enabled = false;
                return;
            }
            gameOverNotifier = gameOverNotifierMono as IGameOverNotifier;
        }

        private void OnEnable()
        {
            // イベント購読
            if (inputProvider != null)
            {
                inputProvider.OnMoveInput += HandleMoveInput;
                inputProvider.OnJumpInput += HandleJumpInput;
                inputProvider.OnBasicAttackInput += HandleBasicAttackInput;
                inputProvider.OnSwitchCharacterInput += HandleSwitchCharacterInput;
                inputProvider.OnMagicInput1 += HandleMagicInput1;
                inputProvider.OnMagicInput2 += HandleMagicInput2;
                inputProvider.OnDashInputStart += HandleDashInputStart;
                inputProvider.OnDashInputEnd += HandleDashInputEnd;
            }
        }

        private void OnDisable()
        {
            // イベント購読解除
            if (inputProvider != null)
            {
                inputProvider.OnMoveInput -= HandleMoveInput;
                inputProvider.OnJumpInput -= HandleJumpInput;
                inputProvider.OnBasicAttackInput -= HandleBasicAttackInput;
                inputProvider.OnSwitchCharacterInput -= HandleSwitchCharacterInput;
                inputProvider.OnMagicInput1 -= HandleMagicInput1;
                inputProvider.OnMagicInput2 -= HandleMagicInput2;
                inputProvider.OnDashInputEnd -= HandleDashInputEnd;
                inputProvider.OnDashInputStart -= HandleDashInputStart;
            }
        }

        private void Start()
        {
            // 初期クールタイム状態を通知
            OnDashCooldownChanged?.Invoke(currentDashCooldown, maxDashCooldown);
        }

        private void Update()
        {
            if (isDead) return; // 死亡状態なら操作を受け付けない

            CheckGroundStatus();
            UpdateBasicAttackCooldown();
            UpdateDashCooldown(); // ダッシュクールタイムの更新
            ApplyMovement();
            UpdateAnimation();
            CheckFallForGameOver();

            // 全てのキャラクターが死亡している場合もゲームオーバー
            if (characterSwitcher != null && characterSwitcher.AreAllCharactersDead)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// 移動入力を処理する。
        /// </summary>
        /// <param name="input">移動方向（-1:左, 0:停止, 1:右）</param>
        private void HandleMoveInput(float input)
        {
            if (isDead) return;
            currentMoveInput = input;
        }

        /// <summary>
        /// ジャンプ入力を処理する。
        /// </summary>
        private void HandleJumpInput()
        {
            if (isDead) return;
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger(ANIM_PARAM_JUMP);
            }
        }

        /// <summary>
        /// 基本攻撃入力を処理する。
        /// </summary>
        private void HandleBasicAttackInput()
        {
            if (isDead) return;
            if (canBasicAttack)
            {
                animator.SetTrigger(ANIM_PARAM_ATTACK);
                canBasicAttack = false;
                basicAttackTimer = basicAttackCooldown;
                // TODO: 攻撃判定の生成やダメージ処理は、アニメーションイベントや別のコンポーネントで行うことを想定
            }
        }

        /// <summary>
        /// キャラクター切り替え入力を処理する。
        /// </summary>
        private void HandleSwitchCharacterInput()
        {
            if (isDead) return;
            if (characterSwitcher != null)
            {
                if (!characterSwitcher.TrySwitchCharacter())
                {
                    Debug.LogWarning("キャラクター切り替えに失敗しました。HPが0のキャラクターには切り替えできません。", this);
                }
            }
        }

        /// <summary>
        /// 魔法スロット1の使用入力を処理する。
        /// </summary>
        private void HandleMagicInput1()
        {
            if (isDead) return;
            if (magicSystem != null)
            {
                if (!magicSystem.TryCastMagic(1))
                {
                    Debug.LogWarning("魔法スロット1の発動に失敗しました。", this);
                }
            }
        }

        /// <summary>
        /// 魔法スロット2の使用入力を処理する。
        /// </summary>
        private void HandleMagicInput2()
        {
            if (isDead) return;
            if (magicSystem != null)
            {
                if (!magicSystem.TryCastMagic(2))
                {
                    Debug.LogWarning("魔法スロット2の発動に失敗しました。", this);
                }
            }
        }

        /// <summary>
        /// ダッシュ開始入力を処理する。
        /// </summary>
        private void HandleDashInputStart()
        {
            if (isDead) return;
            if (currentDashCooldown < maxDashCooldown) // クールタイムが満タンでなければダッシュ可能
            {
                isDashing = true;
            }
            else
            {
                Debug.Log("ダッシュクールタイムが最大のため、ダッシュできません。", this);
            }
        }

        /// <summary>
        /// ダッシュ終了入力を処理する。
        /// </summary>
        private void HandleDashInputEnd()
        {
            if (isDead) return;
            isDashing = false;
        }

        /// <summary>
        /// 地面との接触状態をチェックする。
        /// </summary>
        private void CheckGroundStatus()
        {
            Vector2 rayOrigin = (Vector2)transform.position + groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
            isGrounded = hit.collider != null;

            // デバッグ用
            // Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
        }

        /// <summary>
        /// 移動を適用する。
        /// </summary>
        private void ApplyMovement()
        {
            float speed = isDashing && currentDashCooldown < maxDashCooldown ? dashSpeed : moveSpeed;
            rb.linearVelocity = new Vector2(currentMoveInput * speed, rb.linearVelocity.y);

            // プレイヤーの向きを反転
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
            if (isDashing && currentMoveInput != 0) // ダッシュ中で横移動している場合
            {
                currentDashCooldown += Time.deltaTime * dashCooldownIncreaseRate;
                if (currentDashCooldown >= maxDashCooldown)
                {
                    currentDashCooldown = maxDashCooldown;
                    isDashing = false; // クールタイムが満タンになったらダッシュを強制終了
                }
            }
            else // ダッシュしていない、または横移動していない場合
            {
                currentDashCooldown -= Time.deltaTime * dashCooldownRecoveryRate;
                if (currentDashCooldown < 0f)
                {
                    currentDashCooldown = 0f;
                }
            }
            currentDashCooldown = Mathf.Clamp(currentDashCooldown, 0f, maxDashCooldown);
            OnDashCooldownChanged?.Invoke(currentDashCooldown, maxDashCooldown);
        }

        /// <summary>
        /// アニメーションを更新する。
        /// </summary>
        private void UpdateAnimation()
        {
            animator.SetFloat(ANIM_PARAM_SPEED, Mathf.Abs(rb.linearVelocity.x));
            // ジャンプアニメーションはTriggerで制御するため、ここでは速度による制御は行わない。
            // 地面に着地した際にisGroundedがtrueになり、AnimatorのTransitionでIdle/Runに戻ることを想定。
        }

        /// <summary>
        /// 落下によるゲームオーバー判定を行う。
        /// </summary>
        private void CheckFallForGameOver()
        {
            if (transform.position.y < fallThresholdY)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// ゲームオーバーを通知する。
        /// </summary>
        private void TriggerGameOver()
        {
            if (isDead) return; // 既に死亡状態なら重複して通知しない

            isDead = true; // PlayerControllerを死亡状態にする
            animator.SetTrigger(ANIM_PARAM_DEAD); // 死亡アニメーションを再生
            // 物理挙動を停止させるなど、死亡時の処理を追加
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算を停止
            enabled = false; // スクリプトを無効化し、操作を受け付けないようにする

            OnGameOverEvent?.Invoke(); // ゲームオーバーイベントを発火
            Debug.Log("ゲームオーバー！ (落下または全キャラクター死亡)", this);
        }

        /// <summary>
        /// プレイヤーの状態をリセットする。
        /// ステージ開始時などに外部から呼び出されることを想定。
        /// </summary>
        public void ResetPlayerState()
        {
            isDead = false;
            canBasicAttack = true;
            basicAttackTimer = 0f;
            currentMoveInput = 0f;
            isDashing = false;
            currentDashCooldown = 0f;
            OnDashCooldownChanged?.Invoke(currentDashCooldown, maxDashCooldown); // クールタイムUIをリセット
            rb.bodyType = RigidbodyType2D.Dynamic; // 物理演算を再開
            enabled = true; // スクリプトを有効化
            // Animatorの状態もリセットする必要があるが、これはAnimator Controllerの設定に依存するため、
            // 必要に応じて外部からAnimator.Play("Idle")などを呼び出すことを想定。
            // または、AnimatorのResetTriggerなどを利用する。
            animator.ResetTrigger(ANIM_PARAM_JUMP);
            animator.ResetTrigger(ANIM_PARAM_ATTACK);
            animator.ResetTrigger(ANIM_PARAM_DEAD);
            animator.SetFloat(ANIM_PARAM_SPEED, 0f);
        }

        // エラー対策：Destroy済みオブジェクトへのアクセス防止
        private void OnDestroy()
        {
            OnDisable(); // イベント購読解除を確実に行う
            // イベント購読解除はOnDisableで十分だが、念のため
            OnGameOverEvent = null; // イベントリスナーをクリア
            OnDashCooldownChanged = null; // イベントリスナーをクリア
        }
    }
}
