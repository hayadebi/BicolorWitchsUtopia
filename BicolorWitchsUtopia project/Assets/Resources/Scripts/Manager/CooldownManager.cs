using UnityEngine;
using System;

namespace BicolorWitch.Core
{
    /// <summary>
    /// キャラクターの切り替え、魔法使用、ダッシュに共通のクールタイムを管理するシングルトンクラス。
    /// </summary>
    public class CooldownManager : MonoBehaviour
    {
        public static CooldownManager Instance { get; private set; }

        [SerializeField, Tooltip("統一クールタイムの最大値")]
        private float maxUnifiedCooldown = 3.0f;
        public float MaxUnifiedCooldown => maxUnifiedCooldown;

        private float currentUnifiedCooldown = 0f;
        private bool isCooldownActive = false;

        // クールタイムが変更されたことを通知するイベント (現在のクールタイム, 最大クールタイム)
        public event Action<float, float> OnUnifiedCooldownChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (isCooldownActive)
            {
                currentUnifiedCooldown -= Time.deltaTime;
                if (currentUnifiedCooldown <= 0f)
                {
                    currentUnifiedCooldown = 0f;
                    isCooldownActive = false;
                }
                OnUnifiedCooldownChanged?.Invoke(currentUnifiedCooldown, maxUnifiedCooldown);
            }
        }

        /// <summary>
        /// クールタイムが利用可能かどうかをチェックします。
        /// </summary>
        /// <returns>クールタイムがアクティブでなければtrue。</returns>
        public bool IsCooldownReady()
        {
            return !isCooldownActive;
        }

        /// <summary>
        /// クールタイムを開始します。
        /// </summary>
        /// <param name="cooldownDuration">クールタイムの期間。</param>
        public void StartCooldown(float cooldownDuration)
        {
            currentUnifiedCooldown = cooldownDuration;
            isCooldownActive = true;
            OnUnifiedCooldownChanged?.Invoke(currentUnifiedCooldown, maxUnifiedCooldown);
        }

        /// <summary>
        /// 現在のクールタイムの進捗を取得します。
        /// </summary>
        /// <returns>現在のクールタイム（0.0f～maxUnifiedCooldown）。</returns>
        public float GetCurrentCooldown()
        {
            return currentUnifiedCooldown;
        }

        /// <summary>
        /// クールタイムをリセットします。
        /// </summary>
        public void ResetCooldown()
        {
            currentUnifiedCooldown = 0f;
            isCooldownActive = false;
            OnUnifiedCooldownChanged?.Invoke(currentUnifiedCooldown, maxUnifiedCooldown);
        }
    }
}
