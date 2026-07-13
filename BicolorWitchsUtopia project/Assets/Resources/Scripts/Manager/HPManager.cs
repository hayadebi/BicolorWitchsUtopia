using UnityEngine;
using System;
using System.Collections.Generic;
using BicolorWitch.Player;

namespace BicolorWitch.Game
{
    /// <summary>
    /// HP管理機能を提供するインターフェース。
    /// HPManagerが実装することを想定。
    /// </summary>
    public interface IHPManager
    {
        /// <summary>
        /// 指定されたキャラクターのHPが変更されたときに発火するイベント。
        /// (CharacterType characterType, int currentHP, int maxHP) を引数に持つ。
        /// </summary>
        event Action<CharacterType, int, int> OnHPChanged;

        /// <summary>
        /// 指定されたキャラクターが死亡しているか判定する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>死亡している場合はtrue、それ以外はfalse。</returns>
        bool IsCharacterDead(CharacterType characterType);

        /// <summary>
        /// 指定されたキャラクターにダメージを適用する。
        /// </summary>
        /// <param name="characterType">ダメージを受けるキャラクターの種類。</param>
        /// <param name="amount">ダメージ量。</param>
        void ApplyDamage(CharacterType characterType, int amount);

        /// <summary>
        /// 指定されたキャラクターのHPを回復する。
        /// </summary>
        /// <param name="characterType">HPを回復するキャラクターの種類。</param>
        /// <param name="amount">回復量。</param>
        void Heal(CharacterType characterType, int amount);

        /// <summary>
        /// 指定されたキャラクターのHPを最大値にリセットする。
        /// </summary>
        /// <param name="characterType">HPをリセットするキャラクターの種類。</param>
        void ResetHP(CharacterType characterType);

        /// <summary>
        /// 全てのキャラクターのHPをリセットする。
        /// </summary>
        void ResetAllCharacterStats();

        /// <summary>
        /// 指定されたキャラクターの現在のHPを取得する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>現在のHP。</returns>
        int GetCurrentHP(CharacterType characterType);

        /// <summary>
        /// 指定されたキャラクターの最大HPを取得する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>最大HP。</returns>
        int GetMaxHP(CharacterType characterType);
    }

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
        [Header("HP設定")]
        [SerializeField, Tooltip("T型キャラクターのHPデータ")]
        private CharacterHPData tCharacterHPData;
        [SerializeField, Tooltip("D型キャラクターのHPデータ")]
        private CharacterHPData dCharacterHPData;
        [SerializeField, Tooltip("ゲームオーバーとなる落下Y座標")]
        private float gameOverFallY = -10f;

        [Header("依存コンポーネント")]
        [SerializeField, Tooltip("キャラクター切り替え機能を提供するCharacterSwitcherを実装したオブジェクト")]
        private MonoBehaviour characterSwitcherMono;
        [SerializeField, Tooltip("UI更新機能を提供するUIManagerを実装したオブジェクト")]
        private MonoBehaviour uiManagerMono;
        [SerializeField, Tooltip("ゲーム管理機能を提供するGameManagerを実装したオブジェクト")]
        private MonoBehaviour gameManagerMono;

        private ICharacterSwitcher characterSwitcher;
        private IUIManager uiManager;
        private IGManager gameManager;

        // IHPManagerの実装
        public event Action<CharacterType, int, int> OnHPChanged;

        private void Awake()
        {
            // 依存コンポーネントの取得とNullチェック
            if (characterSwitcherMono == null || !(characterSwitcherMono is ICharacterSwitcher))
            {
                Debug.LogError("CharacterSwitcherMonoが設定されていないか、ICharacterSwitcherを実装していません。", this);
                enabled = false;
                return;
            }
            characterSwitcher = characterSwitcherMono as ICharacterSwitcher;

            if (uiManagerMono == null || !(uiManagerMono is IUIManager))
            {
                Debug.LogError("UIManagerMonoが設定されていないか、IUIManagerを実装していません。", this);
                enabled = false;
                return;
            }
            uiManager = uiManagerMono as IUIManager;

            if (gameManagerMono == null || !(gameManagerMono is IGManager))
            {
                Debug.LogError("GameManagerMonoが設定されていないか、IGManagerを実装していません。", this);
                enabled = false;
                return;
            }
            gameManager = gameManagerMono as IGManager;

            // HPデータの初期化
            tCharacterHPData.Initialize();
            dCharacterHPData.Initialize();
        }

        private void Start()
        {
            // UIの初期更新
            uiManager.UpdateHP(tCharacterHPData.Type, tCharacterHPData.CurrentHP, tCharacterHPData.MaxHP);
            uiManager.UpdateHP(dCharacterHPData.Type, dCharacterHPData.CurrentHP, dCharacterHPData.MaxHP);
        }

        private void Update()
        {
            // 落下によるゲームオーバー判定はStageManagerで行うため、ここでは削除

            // 両方のキャラクターが死亡した場合のゲームオーバー判定
            if (characterSwitcher.AreAllCharactersDead)
            {
                gameManager.GameOver();
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
                uiManager.UpdateHP(characterType, targetData.CurrentHP, targetData.MaxHP);

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
                uiManager.UpdateHP(characterType, targetData.CurrentHP, targetData.MaxHP);
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
                uiManager.UpdateHP(characterType, targetData.CurrentHP, targetData.MaxHP);
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
