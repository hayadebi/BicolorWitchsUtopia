using UnityEngine;
using System;
using System.Collections.Generic;
using BicolorWitch.Player;
using BicolorWitch.Game; // IHPManagerとCharacterTypeを参照するために追加
using BicolorWitch.Manager;

namespace BicolorWitch.Player
{


    /// <summary>
    /// キャラクターごとのデータを保持するクラス。
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        // Inspector: このキャラクターの種類
        [Tooltip("このキャラクターの種類")]
        public CharacterType Type;
        // Inspector: このキャラクターのGameObject
        [Tooltip("このキャラクターのGameObject")]
        public GameObject CharacterGameObject;
        // Inspector: このキャラクターのPlayerController
        [Tooltip("このキャラクターのPlayerController")]
        public PlayerController Controller;

        private int currentMP; // キャラクターごとのMP
        private int maxMP;     // キャラクターごとの最大MP

        /// <summary>
        /// 現在のMPを取得する。
        /// </summary>
        public int CurrentMP => currentMP;

        /// <summary>
        /// 最大MPを取得する。
        /// </summary>
        public int MaxMP => maxMP;

        /// <summary>
        /// キャラクターデータを初期化する。
        /// </summary>
        /// <param name="defaultMaxMP">初期の最大MP。</param>
        public void Initialize(int defaultMaxMP)
        {
            maxMP = defaultMaxMP;
            currentMP = maxMP;
            CharacterGameObject.SetActive(false); // 初期状態では非アクティブ
        }

        /// <summary>
        /// MPを消費する。
        /// </summary>
        /// <param name="amount">消費量。</param>
        /// <returns>消費に成功した場合はtrue、MPが足りない場合はfalse。</returns>
        public bool ConsumeMP(int amount)
        {
            if (currentMP >= amount)
            {
                currentMP -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// MPを回復する。
        /// </summary>
        /// <param name="amount">回復量。</param>
        public void RecoverMP(int amount)
        {
            currentMP = Mathf.Min(currentMP + amount, maxMP);
        }

        /// <summary>
        /// MPをリセットする（最大値まで回復）。
        /// </summary>
        public void ResetMP()
        {
            currentMP = maxMP;
        }

        /// <summary>
        /// このキャラクターが死亡しているか（HPが0か）を判定する。
        /// </summary>
        public bool IsDead => HPManager.Instance!= null && HPManager.Instance.IsCharacterDead(Type);
    }

    /// <summary>
    /// 姉妹の切り替えロジック、MP/CT管理、視界・実体化の切り替えを管理する。
    /// PlayerControllerのICharacterSwitcherインターフェースを実装する。
    /// </summary>
    public class CharacterSwitcher : MonoBehaviour, ICharacterSwitcher
    {
        public static CharacterSwitcher Instance { get; private set; }
        // ICharacterSwitcherインターフェースの追加メソッドの実装
        public CharacterType GetActiveCharacterType()
        {
            if (activeCharacterData == null)
            {
                Debug.LogError("アクティブなキャラクターデータが設定されていません。", this);
                return CharacterType.SisterT; // デフォルト値を返すか、適切なエラーハンドリング
            }
            return activeCharacterData.Type;
        }

        public GameObject GetActiveCharacterGameObject()
        {
            if (activeCharacterData == null)
            {
                Debug.LogError("アクティブなキャラクターデータが設定されていません。", this);
                return null;
            }
            return activeCharacterData.CharacterGameObject;
        }

        public bool ConsumeMP(CharacterType characterType, int amount)
        {
            CharacterData targetData = GetCharacterData(characterType);
            if (targetData != null && targetData.ConsumeMP(amount))
            {
                UI.UIManager.Instance.UpdateMP(characterType, targetData.CurrentMP, targetData.MaxMP);
                return true;
            }
            return false;
        }

        public int GetCurrentMP(CharacterType characterType)
        {
            CharacterData targetData = GetCharacterData(characterType);
            return targetData != null ? targetData.CurrentMP : 0;
        }

        public int GetMaxMP(CharacterType characterType)
        {
            CharacterData targetData = GetCharacterData(characterType);
            return targetData != null ? targetData.MaxMP : 0;
        }

        private CharacterData GetCharacterData(CharacterType characterType)
        {
            if (sisterTData.Type == characterType) return sisterTData;
            if (sisterDData.Type == characterType) return sisterDData;
            Debug.LogWarning($"指定されたキャラクタータイプ {characterType} のデータが見つかりません。", this);
            return null;
        }

        // Inspector設定項目
        [Header("キャラクター設定")]
        // Inspector: 姉のキャラクターデータを設定
        [SerializeField, Tooltip("姉のキャラクターデータ")]
        private CharacterData sisterTData;
        // Inspector: 妹のキャラクターデータを設定
        [SerializeField, Tooltip("妹のキャラクターデータ")]
        private CharacterData sisterDData;
        // Inspector: キャラクター切り替えに必要なMPを設定
        [SerializeField, Tooltip("キャラクター切り替えに必要なMP")]
        private int switchCostMP = 10;
        // Inspector: キャラクター切り替えのクールタイムを設定
        [SerializeField, Tooltip("キャラクター切り替えのクールタイム")]
        private float switchCooldownTime = 1.0f;
        // Inspector: キャラクターごとの初期最大MPを設定
        [SerializeField, Tooltip("キャラクターごとの初期最大MP")]
        private int defaultMaxMP = 100;

        [Header("依存コンポーネント")]
        // Inspector: ShaderManagerを実装したオブジェクトを設定


        // プライベート変数
        private CharacterData activeCharacterData;
        private CharacterData inactiveCharacterData;
        private ShaderManager shaderManager;
        private float currentSwitchCooldown = 0f;
        private bool canSwitch = true;

        // ICharacterSwitcherの実装
        public bool IsCurrentCharacterDead => activeCharacterData != null && activeCharacterData.IsDead;
        public bool AreAllCharactersDead => (sisterTData == null || sisterTData.IsDead) && (sisterDData == null || sisterDData.IsDead);

        // イベント
        public event Action<CharacterType> OnCharacterSwitched; // キャラクターが切り替わったときに発火
        public bool isStart = false;

        public float GetMaxDashCooldown(CharacterType characterType)
        {
            // PlayerControllerから最大ダッシュクールタイムを取得する
            // ここでは仮に固定値を返すか、PlayerControllerから取得するロジックを実装する
            // PlayerControllerが持つべき情報なので、PlayerControllerから取得する形が望ましい
            // 現状、PlayerControllerにはGetMaxDashCooldownメソッドがないため、一旦固定値を返す
            // 後でPlayerControllerにGetMaxDashCooldownを追加する際に修正する
            CharacterData targetData = GetCharacterData(characterType);
            if (targetData != null && targetData.Controller != null)
            {
                return targetData.Controller.MaxDashCooldown;
            }
            Debug.LogWarning($"CharacterSwitcher: {characterType} のPlayerControllerが見つからないか、MaxDashCooldownが取得できません。", this);
            return 0f; // エラー時は0を返すか、適切なデフォルト値

        }

        /// <summary>
        /// 全てのキャラクターのステータス（MP、ダッシュクールタイムなど）をリセットする。
        /// </summary>
        public void ResetAllCharacterStats()
        {
            sisterTData.ResetMP();
            sisterDData.ResetMP();
            currentSwitchCooldown = 0f;
            canSwitch = true;

            // UIの更新
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.UpdateMP(sisterTData.Type, sisterTData.CurrentMP, sisterTData.MaxMP);
                UI.UIManager.Instance.UpdateMP(sisterDData.Type, sisterDData.CurrentMP, sisterDData.MaxMP);
                UI.UIManager.Instance.UpdateSwitchCooldownUI(currentSwitchCooldown, switchCooldownTime);
            }

            Debug.Log("CharacterSwitcher: 全キャラクターのステータスがリセットされました。", this);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // キャラクターデータの初期化
            sisterTData.Initialize(defaultMaxMP);
            sisterDData.Initialize(defaultMaxMP);
            shaderManager = ShaderManager.Instance;
        }

        private void Start()
        {
            isStart = false;
            // 初期アクティブキャラクターを設定
            // どちらのキャラクターも死んでいない場合、姉を初期アクティブにする
            if (!sisterTData.IsDead)
            {
                SetActiveCharacter(sisterTData);
            }
            else if (!sisterDData.IsDead)
            {
                SetActiveCharacter(sisterDData);
            }
            else
            {
                Debug.LogError("両方のキャラクターが初期状態で死亡しています。ゲームを開始できません。", this);
                enabled = false;
                return;
            }
            if (HPManager.Instance != null)
            {
                HPManager.Instance.OnHPChanged += HandleHPChanged;
            }

            // UIの初期更新
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.UpdateActiveCharacterUI(activeCharacterData.Type);
                UI.UIManager.Instance.UpdateSwitchCooldownUI(currentSwitchCooldown, switchCooldownTime);
            }
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.UpdateMP(sisterTData.Type, sisterTData.CurrentMP, sisterTData.MaxMP);
                UI.UIManager.Instance.UpdateMP(sisterDData.Type, sisterDData.CurrentMP, sisterDData.MaxMP);
            }
        }

        private void OnDestroy()
        {
            if (HPManager.Instance != null)
            {
                HPManager.Instance.OnHPChanged -= HandleHPChanged;
            }
            OnCharacterSwitched = null;
        }

        private void Update()
        {
            UpdateSwitchCooldown();

            // アクティブキャラクターが死亡した場合、自動的に切り替えを試みる
            if (activeCharacterData != null && activeCharacterData.IsDead)
            {
                Debug.Log($"{activeCharacterData.Type} が死亡しました。切り替えを試みます。", this);
                TrySwitchCharacter(); // 死亡していないキャラクターへの切り替えを試みる

                // 切り替え後もアクティブキャラクターが死亡している、つまり両方死亡している場合
                if (activeCharacterData != null && activeCharacterData.IsDead)
                {
                    // PlayerControllerのAreAllCharactersDeadがtrueになるので、ゲームオーバー処理はそちらに任せる
                    Debug.Log("両方のキャラクターが死亡しました。", this);
                }
            }
        }

        /// <summary>
        /// キャラクターの切り替えを試みる。
        /// ICharacterSwitcherインターフェースの実装。
        /// </summary>
        /// <returns>切り替えが成功した場合はtrue、失敗した場合はfalse。</returns>
        public bool TrySwitchCharacter()
        {
            if (!canSwitch || activeCharacterData == null) return false;

            // 切り替え先のキャラクターが死亡している場合は切り替え不可
            if (inactiveCharacterData.IsDead)
            {
                Debug.LogWarning($"切り替え先のキャラクター({inactiveCharacterData.Type})は死亡しているため切り替えできません。", this);
                return false;
            }

            // MPが足りない場合は切り替え不可
            if (!activeCharacterData.ConsumeMP(switchCostMP))
            {
                Debug.LogWarning($"MPが足りないためキャラクター切り替えできません。現在のMP: {activeCharacterData.CurrentMP}", this);
                return false;
            }

            // クールタイム開始
            currentSwitchCooldown = switchCooldownTime;
            canSwitch = false;

            // キャラクターを切り替える
            CharacterData temp = activeCharacterData;
            activeCharacterData = inactiveCharacterData;
            inactiveCharacterData = temp;

            SetActiveCharacter(activeCharacterData);

            // UIとシェーダーを更新
                        UI.UIManager.Instance?.UpdateActiveCharacterUI(activeCharacterData.Type);
            UI.UIManager.Instance.UpdateMP(temp.Type, temp.CurrentMP, temp.MaxMP); // 切り替わったキャラクターのMPも更新
            UI.UIManager.Instance.UpdateMP(activeCharacterData.Type, activeCharacterData.CurrentMP, activeCharacterData.MaxMP);
            shaderManager.ApplyColorPerception(activeCharacterData.Type);

            OnCharacterSwitched?.Invoke(activeCharacterData.Type);
            Debug.Log($"キャラクターを {activeCharacterData.Type} に切り替えました。", this);
            return true;
        }

        /// <summary>
        /// アクティブなキャラクターを設定し、GameObjectの表示/非表示を切り替える。
        /// </summary>
        /// <param name="character">アクティブにするキャラクターデータ。</param>
        private void SetActiveCharacter(CharacterData character)
        {
            if (activeCharacterData != null && activeCharacterData.CharacterGameObject != null)
            {
                activeCharacterData.CharacterGameObject.SetActive(false);
                activeCharacterData.Controller.enabled = false; // PlayerControllerも無効化
            }

            activeCharacterData = character;
            inactiveCharacterData = (character.Type == sisterTData.Type) ? sisterDData : sisterTData;

            if (activeCharacterData != null && activeCharacterData.CharacterGameObject != null)
            {
                activeCharacterData.CharacterGameObject.SetActive(true);
                activeCharacterData.Controller.enabled = true; // PlayerControllerを有効化
            }
        }

        /// <summary>
        /// 切り替えクールタイムを更新する。
        /// </summary>
        private void UpdateSwitchCooldown()
        {
            if (!canSwitch)
            {
                currentSwitchCooldown -= Time.deltaTime;
                if (currentSwitchCooldown <= 0f)
                {
                    currentSwitchCooldown = 0f;
                    canSwitch = true;
                    Debug.Log("キャラクター切り替えクールタイム終了。", this);
                }
                if (UI.UIManager.Instance != null)
                {
                    UI.UIManager.Instance.UpdateSwitchCooldownUI(currentSwitchCooldown, switchCooldownTime);
                }
            }
        }

        /// <summary>
        /// HP変更イベントのハンドラ。
        /// キャラクターが死亡した場合の処理。
        /// </summary>
        private void HandleHPChanged(CharacterType characterType, int currentHP, int maxHP)
        {
            // 死亡したキャラクターがアクティブな場合、自動切り替えを試みる
            if (characterType == activeCharacterData.Type && currentHP <= 0)
            {
                Debug.Log($"{characterType} のHPが0になりました。自動切り替えを試みます。", this);
                TrySwitchCharacter();
            }
        }
    }
}
