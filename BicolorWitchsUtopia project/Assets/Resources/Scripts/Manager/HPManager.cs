using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BicolorWitch.Player;
using BicolorWitch.UI;
using BicolorWitch.Core;

namespace BicolorWitch.Game
{
    /// <summary>
    /// キャラクターごとのHPデータを保持するクラス。
    /// </summary>
    [Serializable]
    public class CharacterHPData
    {
        [Tooltip("このHPデータが対応するキャラクターの種類")]
        public CharacterType Type;
        [Tooltip("初期最大HP")]
        public int MaxHP = 100;
        [Tooltip("現在のHP (デバッグ用、実行時に初期化されます)")]
        public int CurrentHP;

        public bool IsDead => CurrentHP <= 0;

        /// <summary>
        /// HPデータを初期化する。
        /// </summary>
        public void Initialize()
        {
            CurrentHP = MaxHP;
        }

        /// <summary>
        /// ダメージを適用する。
        /// </summary>
        /// <param name="amount">ダメージ量。</param>
        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
        }

        /// <summary>
        /// HPを回復する。
        /// </summary>
        /// <param name="amount">回復量。</param>
        public void RestoreHP(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        }

        /// <summary>
        /// HPを最大値にリセットする。
        /// </summary>
        public void ResetToMaxHP()
        {
            CurrentHP = MaxHP;
        }
    }

    /// <summary>
    /// プレイヤーキャラクターのHPを管理するクラス。
    /// IHPManagerインターフェースを実装する。
    /// </summary>
    public class HPManager : MonoBehaviour, IHPManager
    {
        public static HPManager Instance { get; private set; }
        [Header("HP設定")]
        [SerializeField, Tooltip("T型キャラクターのHPデータ")]
        public CharacterHPData tCharacterHPData;
        [SerializeField, Tooltip("D型キャラクターのHPデータ")]
        public CharacterHPData dCharacterHPData;
        [SerializeField, Tooltip("ゲームオーバーとなる落下Y座標")]
        private float gameOverFallY = -10f;

        // IHPManagerの実装
        public event Action<CharacterType, int, int> OnHPChanged;

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

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => Core.GManager.Instance != null);
            yield return new WaitUntil(() => UIManager.Instance != null);
            // HPデータの初期化
            tCharacterHPData.Initialize();
            dCharacterHPData.Initialize();
            yield return null;
            // UIの初期更新
            UIManager.Instance?.UpdateHP(tCharacterHPData.Type, tCharacterHPData.CurrentHP, tCharacterHPData.MaxHP);
            UIManager.Instance?.UpdateHP(dCharacterHPData.Type, dCharacterHPData.CurrentHP, dCharacterHPData.MaxHP);
        }

        private void Update()
        {
            // 落下によるゲームオーバー判定はStageManagerで行うため、ここでは削除

            // 両方のキャラクターが死亡した場合のゲームオーバー判定
            if (GManager.Instance != null && GManager.Instance.currentGameState == GManager.GameState.Playing && CharacterSwitcher.Instance != null && CharacterSwitcher.Instance.AreAllCharactersDead)
            {
                GManager.Instance.GameOver();
            }
        }

        /// <summary>
        /// 指定されたキャラクターが死亡しているか判定する。
        /// </summary>
        public bool IsCharacterDead(CharacterType characterType)
        {
            return GetCharacterHPData(characterType)?.IsDead ?? true; // データが見つからない場合は死亡とみなす
        }

        /// <summary>
        /// 指定されたキャラクターにダメージを適用する。
        /// </summary>
        public void ApplyDamage(CharacterType characterType, int amount)
        {
            CharacterHPData targetData = GetCharacterHPData(characterType);
            if (targetData != null)
            {
                targetData.TakeDamage(amount);
                OnHPChanged?.Invoke(characterType, targetData.CurrentHP, targetData.MaxHP);
                UIManager.Instance.UpdateHP(characterType, targetData.CurrentHP, targetData.MaxHP);

                if (targetData.IsDead)
                {
                    Debug.Log($"{characterType} が死亡しました。", this);
                    // CharacterSwitcherが死亡判定を処理するため、ここではゲームオーバーを直接呼ばない
                }
            }
        }

        /// <summary>
        /// 指定されたキャラクターのHPを回復する。
        /// </summary>
        public void Heal(CharacterType characterType, int amount)
        {
            CharacterHPData targetData = GetCharacterHPData(characterType);
            if (targetData != null)
            {
                targetData.RestoreHP(amount);
                OnHPChanged?.Invoke(characterType, targetData.CurrentHP, targetData.MaxHP);
                UIManager.Instance.UpdateHP(characterType, targetData.CurrentHP, targetData.MaxHP);
            }
        }

        /// <summary>
        /// 指定されたキャラクターのHPを最大値にリセットする。
        /// </summary>
        public void ResetHP(CharacterType characterType)
        {
            CharacterHPData targetData = GetCharacterHPData(characterType);
            if (targetData != null)
            {
                targetData.ResetToMaxHP();
                OnHPChanged?.Invoke(characterType, targetData.CurrentHP, targetData.MaxHP);
                UIManager.Instance.UpdateHP(characterType, targetData.CurrentHP, targetData.MaxHP);
            }
        }

        /// <summary>
        /// 全てのキャラクターのHPをリセットする。
        /// </summary>
        public void ResetAllCharacterStats()
        {
            ResetHP(CharacterType.SisterT);
            ResetHP(CharacterType.SisterD);
        }

        /// <summary>
        /// 指定されたキャラクターの現在のHPを取得する。
        /// </summary>
        public int GetCurrentHP(CharacterType characterType)
        {
            return GetCharacterHPData(characterType)?.CurrentHP ?? 0;
        }

        /// <summary>
        /// 指定されたキャラクターの最大HPを取得する。
        /// </summary>
        public int GetMaxHP(CharacterType characterType)
        {
            return GetCharacterHPData(characterType)?.MaxHP ?? 0;
        }

        /// <summary>
        /// 指定されたキャラクターのHPデータを取得するヘルパーメソッド。
        /// </summary>
        private CharacterHPData GetCharacterHPData(CharacterType characterType)
        {
            if (tCharacterHPData.Type == characterType) return tCharacterHPData;
            if (dCharacterHPData.Type == characterType) return dCharacterHPData;
            Debug.LogWarning($"指定されたキャラクタータイプ {characterType} のHPデータが見つかりません。", this);
            return null;
        }
    }
}
