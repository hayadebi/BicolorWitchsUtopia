// EnemyAI.cs
using UnityEngine;
using System;
using BicolorWitch.Player;
using BicolorWitch.Game;

namespace BicolorWitch.Enemy
{
    /// <summary>
    /// 敵キャラクターのインターフェース。
    /// </summary>
    public interface IEnemy
    {
        /// <summary>
        /// 敵がダメージを受けたときに呼び出される。
        /// </summary>
        /// <param name="amount">ダメージ量。</param>
        void TakeDamage(int amount);

        /// <summary>
        /// 敵が死亡したときに発火するイベント。
        /// </summary>
        event Action OnEnemyDied;
    }

    /// <summary>
    /// 敵キャラクターのAIと動作を管理するクラス。
    /// </summary>
    public class EnemyAI : MonoBehaviour, IEnemy
    {
        [Header("敵のステータス")]
        [SerializeField, Tooltip("敵の最大HP")]
        private int maxHP = 50;
        [SerializeField, Tooltip("敵の現在のHP (デバッグ用)")]
        private int currentHP;
        [SerializeField, Tooltip("敵の移動速度")]
        private float moveSpeed = 2.0f;
        [SerializeField, Tooltip("プレイヤーへの攻撃力")]
        private int attackDamage = 10;
        [SerializeField, Tooltip("プレイヤーを検知する範囲")]
        private float detectionRange = 5.0f;
        [SerializeField, Tooltip("プレイヤーに攻撃する範囲")]
        private float attackRange = 1.5f;
        [SerializeField, Tooltip("攻撃クールタイム")]
        private float attackCooldown = 2.0f;

        [Header("アニメーション設定")]
        [SerializeField, Tooltip("敵のAnimatorコンポーネント")]
        private Animator animator;
        [SerializeField, Tooltip("攻撃アニメーションのトリガー名リスト")]
        private string[] attackAnimationTriggers = { "Attack1", "Attack2" };
        [SerializeField, Tooltip("移動アニメーションのパラメータ名 (float)")]
        private string moveSpeedParameter = "MoveSpeed";
        [SerializeField, Tooltip("死亡アニメーションのトリガー名")]
        private string dieAnimationTrigger = "Die";

        [Header("依存コンポーネント")]
        [SerializeField, Tooltip("プレイヤーのHP管理機能を提供するHPManagerを実装したオブジェクト")]
        private MonoBehaviour hpManagerMono;
        [SerializeField, Tooltip("キャラクター切り替え機能を提供するCharacterSwitcherを実装したオブジェクト")]
        private MonoBehaviour characterSwitcherMono;

        private BicolorWitch.Game.IHPManager hpManager;
        private ICharacterSwitcher characterSwitcher;
        private Transform playerTransform;
        private float currentAttackCooldown;
        private bool isDead = false;
        private Camera mainCamera;
        private Renderer enemyRenderer;

        public event Action OnEnemyDied;

        private void Awake()
        {
            currentHP = maxHP;
            mainCamera = Camera.main;
            enemyRenderer = GetComponentInChildren<Renderer>();

            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning("Animatorコンポーネントが設定されていません。アニメーションは再生されません。", this);
                }
            }

            if (hpManagerMono == null || !(hpManagerMono is BicolorWitch.Game.IHPManager))
            {
                Debug.LogError("HPManagerMonoが設定されていないか、IHPManagerを実装していません。", this);
                enabled = false;
                return;
            }
            hpManager = hpManagerMono as BicolorWitch.Game.IHPManager;

            if (characterSwitcherMono == null || !(characterSwitcherMono is ICharacterSwitcher))
            {
                Debug.LogError("CharacterSwitcherMonoが設定されていないか、ICharacterSwitcherを実装していません。", this);
                enabled = false;
                return;
            }
            characterSwitcher = characterSwitcherMono as ICharacterSwitcher;
        }

        private void Update()
        {
            if (isDead) return;

            // カメラに映っていない場合は動作を停止
            if (!IsVisibleFromCamera())
            {
                if (animator != null) animator.SetFloat(moveSpeedParameter, 0f);
                return;
            }

            // プレイヤーの位置を取得
            GameObject activePlayer = characterSwitcher.GetActiveCharacterGameObject();
            if (activePlayer == null) return; // プレイヤーが見つからない場合は何もしない

            playerTransform = activePlayer.transform;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange)
            {
                // 攻撃範囲内：攻撃を試みる
                AttackPlayer();
                if (animator != null) animator.SetFloat(moveSpeedParameter, 0f);
            }
            else if (distanceToPlayer <= detectionRange)
            {
                // 検知範囲内：プレイヤーを追いかける
                ChasePlayer();
            }
            else
            {
                // 範囲外：待機
                if (animator != null) animator.SetFloat(moveSpeedParameter, 0f);
            }

            // 攻撃クールタイムの更新
            if (currentAttackCooldown > 0)
            {
                currentAttackCooldown -= Time.deltaTime;
            }
        }

        /// <summary>
        /// 敵がカメラに映っているか判定する。
        /// </summary>
        private bool IsVisibleFromCamera()
        {
            if (enemyRenderer == null || mainCamera == null) return true; // Rendererがない場合は常にアクティブとする

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            return GeometryUtility.TestPlanesAABB(planes, enemyRenderer.bounds);
        }

        /// <summary>
        /// 敵がダメージを受ける処理。
        /// </summary>
        /// <param name="amount">ダメージ量。</param>
        public void TakeDamage(int amount)
        {
            if (isDead) return;

            currentHP = Mathf.Max(0, currentHP - amount);
            Debug.Log($"{gameObject.name} が {amount} ダメージを受けました。残りHP: {currentHP}", this);

            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// プレイヤーを追いかける処理。
        /// </summary>
        private void ChasePlayer()
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            // 敵の向きをプレイヤーの方向へ
            transform.LookAt(playerTransform);

            if (animator != null)
            {
                animator.SetFloat(moveSpeedParameter, moveSpeed);
            }
        }

        /// <summary>
        /// プレイヤーを攻撃する処理。
        /// </summary>
        private void AttackPlayer()
        {
            if (currentAttackCooldown <= 0)
            {
                Debug.Log($"{gameObject.name} がプレイヤー({characterSwitcher.GetActiveCharacterType()})を攻撃！", this);
                hpManager.ApplyDamage(characterSwitcher.GetActiveCharacterType(), attackDamage);
                currentAttackCooldown = attackCooldown;

                // ランダムな攻撃アニメーションを再生
                if (animator != null && attackAnimationTriggers.Length > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, attackAnimationTriggers.Length);
                    animator.SetTrigger(attackAnimationTriggers[randomIndex]);
                }
            }
        }

        /// <summary>
        /// 敵が死亡したときの処理。
        /// </summary>
        private void Die()
        {
            isDead = true;
            Debug.Log($"{gameObject.name} が倒されました！", this);
            OnEnemyDied?.Invoke();

            if (animator != null)
            {
                animator.SetTrigger(dieAnimationTrigger);
            }

            // アニメーション再生後に破棄するため、少し遅延させる (例: 2秒後)
            Destroy(gameObject, 2.0f);
        }
    }
}