using UnityEngine;
using System;
using System.Collections.Generic;
using BicolorWitch.Player; // IHPManagerとCharacterTypeを参照するために追加

namespace BicolorWitch.Player
{


    /// <summary>
    /// UI更新機能を提供するインターフェース。
    /// UIManagerが実装することを想定。
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 現在アクティブなキャラクターのUIを更新する。
        /// </summary>
        /// <param name="activeCharacter">アクティブなキャラクターの種類。</param>
        void UpdateActiveCharacterUI(CharacterType activeCharacter);

        /// <summary>
        /// キャラクター切り替えクールタイムのUIを更新する。
        /// </summary>
        /// <param name="currentCooldown">現在のクールタイム。</param>
        /// <param name="maxCooldown">最大クールタイム。</param>
        void UpdateSwitchCooldownUI(float currentCooldown, float maxCooldown);

        /// <summary>
        /// 指定されたキャラクターのMPをUIに表示する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <param name="currentMP">現在のMP。</param>
        /// <param name="maxMP">最大MP。</param>
        void UpdateMP(CharacterType characterType, int currentMP, int maxMP);
    }

    /// <summary>
    /// シェーダー管理機能を提供するインターフェース。
    /// ShaderManagerが実装することを想定。
    /// </summary>
    public interface IShaderManager
    {
        /// <summary>
        /// 指定されたキャラクターの色覚特性をシーンに適用する。
        /// </summary>
        /// <param name="activeCharacter">アクティブなキャラクターの種類。</param>
        void ApplyColorPerception(CharacterType activeCharacter);
    }

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
        // Inspector: このキャラクターのHPManager
        [Tooltip("このキャラクターのHPManager")]
        public MonoBehaviour HPManagerMono; // IHPManagerを実装したMonoBehaviour

        // プライベート変数
        private IHPManager hpManager;
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
            if (HPManagerMono == null || !(HPManagerMono is IHPManager))
            {
                Debug.LogError($"CharacterType {Type} のHPManagerMonoが設定されていないか、IHPManagerを実装していません。", CharacterGameObject);
                return;
            }
            hpManager = HPManagerMono as IHPManager;

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
        public bool IsDead => hpManager != null && hpManager.IsCharacterDead(Type);
    }

    /// <summary>
    /// 姉妹の切り替えロジック、MP/CT管理、視界・実体化の切り替えを管理する。
    /// PlayerControllerのICharacterSwitcherインターフェースを実装する。
    /// </summary>
    public class CharacterSwitcher : MonoBehaviour, ICharacterSwitcher
    {
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
                uiManager.UpdateMP(characterType, targetData.CurrentMP, targetData.MaxMP);
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
        // Inspector: UIManagerを実装したオブジェクトを設定
        [SerializeField, Tooltip("UI更新機能を提供するUIManagerを実装したオブジェクト")]
        private MonoBehaviour uiManagerMono;
        // Inspector: ShaderManagerを実装したオブジェクトを設定
        [SerializeField, Tooltip("シェーダー管理機能を提供するShaderManagerを実装したオブジェクト")]
        private MonoBehaviour shaderManagerMono;

        // プライベート変数
        private CharacterData activeCharacterData;
        private CharacterData inactiveCharacterData;
        private IUIManager uiManager;
        private IShaderManager shaderManager;
        private float currentSwitchCooldown = 0f;
        private bool canSwitch = true;

        // ICharacterSwitcherの実装
        public bool IsCurrentCharacterDead => activeCharacterData != null && activeCharacterData.IsDead;
        public bool AreAllCharactersDead => (sisterTData == null || sisterTData.IsDead) && (sisterDData == null || sisterDData.IsDead);

        // イベント
        public event Action<CharacterType> OnCharacterSwitched; // キャラクターが切り替わったときに発火

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
            uiManager.UpdateMP(sisterTData.Type, sisterTData.CurrentMP, sisterTData.MaxMP);
            uiManager.UpdateMP(sisterDData.Type, sisterDData.CurrentMP, sisterDData.MaxMP);
            uiManager.UpdateSwitchCooldownUI(currentSwitchCooldown, switchCooldownTime);

            Debug.Log("CharacterSwitcher: 全キャラクターのステータスがリセットされました。", this);
        }

        private void Awake()
        {
            // キャラクターデータの初期化
            sisterTData.Initialize(defaultMaxMP);
            sisterDData.Initialize(defaultMaxMP);

            // 依存コンポーネントの取得とNullチェック
            if (uiManagerMono == null || !(uiManagerMono is IUIManager))
            {
                Debug.LogError("UIManagerMonoが設定されていないか、IUIManagerを実装していません。", this);
                enabled = false;
                return;
            }
            uiManager = uiManagerMono as IUIManager;

            if (shaderManagerMono == null || !(shaderManagerMono is IShaderManager))
            {
                Debug.LogError("ShaderManagerMonoが設定されていないか、IShaderManagerを実装していません。", this);
                enabled = false;
                return;
            }
            shaderManager = shaderManagerMono as IShaderManager;
        }

        private void Start()
        {
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

            // HPManagerのイベントを購読
            if (sisterTData.HPManagerMono is IHPManager hpManagerT)
            {
                hpManagerT.OnHPChanged += HandleHPChanged;
            }
            if (sisterDData.HPManagerMono is IHPManager hpManagerD)
            {
                hpManagerD.OnHPChanged += HandleHPChanged;
            }

            // UIの初期更新
            uiManager.UpdateActiveCharacterUI(activeCharacterData.Type);
            uiManager.UpdateSwitchCooldownUI(currentSwitchCooldown, switchCooldownTime);
            uiManager.UpdateMP(sisterTData.Type, sisterTData.CurrentMP, sisterTData.MaxMP);
            uiManager.UpdateMP(sisterDData.Type, sisterDData.CurrentMP, sisterDData.MaxMP);
        }

        private void OnDestroy()
        {
            // HPManagerのイベント購読解除
            if (sisterTData.HPManagerMono is IHPManager hpManagerT)
            {
                hpManagerT.OnHPChanged -= HandleHPChanged;
            }
            if (sisterDData.HPManagerMono is IHPManager hpManagerD)
            {
                hpManagerD.OnHPChanged -= HandleHPChanged;
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
            uiManager.UpdateActiveCharacterUI(activeCharacterData.Type);
            uiManager.UpdateMP(temp.Type, temp.CurrentMP, temp.MaxMP); // 切り替わったキャラクターのMPも更新
            uiManager.UpdateMP(activeCharacterData.Type, activeCharacterData.CurrentMP, activeCharacterData.MaxMP);
            shaderManager.ApplyColorPerception(activeCharacterData.Type);

            OnCharacterSwitched?.Invoke(activeCharacterData.Type);

            Debug.Log($"キャラクターを {activeCharacterData.Type} に切り替えました。", this);
            return true;
        }

        /// <summary>
        /// アクティブキャラクターを設定し、GameObjectの有効/無効を切り替える。
        /// </summary>
        /// <param name="characterToActivate">アクティブにするキャラクターデータ。</param>
        private void SetActiveCharacter(CharacterData characterToActivate)
        {
            if (characterToActivate == null) return;

            // 現在のアクティブキャラクターを非アクティブにする
            if (activeCharacterData != null && activeCharacterData != characterToActivate)
            {
                activeCharacterData.CharacterGameObject.SetActive(false);
                activeCharacterData.Controller.enabled = false; // PlayerControllerも無効化
            }

            activeCharacterData = characterToActivate;
            inactiveCharacterData = (activeCharacterData == sisterTData) ? sisterDData : sisterTData;

            activeCharacterData.CharacterGameObject.SetActive(true);
            activeCharacterData.Controller.enabled = true;

            shaderManager.ApplyColorPerception(activeCharacterData.Type);
            uiManager.UpdateActiveCharacterUI(activeCharacterData.Type);
        }

        /// <summary>
        /// キャラクターのMPを回復する。
        /// </summary>
        /// <param name="characterType">回復するキャラクターの種類。</param>
        /// <param name="amount">回復量。</param>
        public void RecoverMP(CharacterType characterType, int amount)
        {
            if (sisterTData.Type == characterType) sisterTData.RecoverMP(amount);
            else if (sisterDData.Type == characterType) sisterDData.RecoverMP(amount);
            else return;

            // UI更新
            uiManager.UpdateMP(characterType, GetCharacterData(characterType).CurrentMP, GetCharacterData(characterType).MaxMP);
        }

        /// <summary>
        /// 全てのキャラクターのHPとMPをリセットする。
        /// ステージ開始時などに呼び出すことを想定。
        /// </summary>
        public void ResetAllCharacterStates()
        {
            sisterTData.ResetMP();
            sisterDData.ResetMP();

            // HPはIHPManager経由でリセット
            if (sisterTData.HPManagerMono is IHPManager hpManagerT) hpManagerT.ResetHP(sisterTData.Type);
            if (sisterDData.HPManagerMono is IHPManager hpManagerD) hpManagerD.ResetHP(sisterTData.Type);

            // UI更新
            uiManager.UpdateMP(sisterTData.Type, sisterTData.CurrentMP, sisterTData.MaxMP);
            uiManager.UpdateMP(sisterDData.Type, sisterDData.CurrentMP, sisterDData.MaxMP);

            // 初期アクティブキャラクターを再設定
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
                Debug.LogError("ResetAllCharacterStates後、両方のキャラクターが死亡しています。", this);
                enabled = false;
                return;
            }

            currentSwitchCooldown = 0f;
            canSwitch = true;
            uiManager.UpdateSwitchCooldownUI(currentSwitchCooldown, switchCooldownTime);
        }

        /// <summary>
        /// 指定されたキャラクターのPlayerControllerを取得する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>PlayerControllerインスタンス。</returns>
        public PlayerController GetPlayerController(CharacterType characterType)
        {
            if (sisterTData.Type == characterType) return sisterTData.Controller;
            if (sisterDData.Type == characterType) return sisterDData.Controller;
            return null;
        }

        /// <summary>
        /// 指定されたキャラクターのHPManagerを取得する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>IHPManagerインスタンス。</returns>
        public IHPManager GetHPManager(CharacterType characterType)
        {
            if (sisterTData.Type == characterType && sisterTData.HPManagerMono is IHPManager hpManagerT) return hpManagerT;
            if (sisterDData.Type == characterType && sisterDData.HPManagerMono is IHPManager hpManagerD) return hpManagerD;
            return null;
        }

        /// <summary>
        /// キャラクター切り替えクールタイムを更新する。
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
                }
                uiManager.UpdateSwitchCooldownUI(currentSwitchCooldown, switchCooldownTime);
            }
        }

        /// <summary>
        /// HP変更イベントを処理する。
        /// </summary>
        /// <param name="characterType">HPが変更されたキャラクターの種類。</param>
        /// <param name="currentHP">現在のHP。</param>
        /// <param name="maxHP">最大HP。</param>
        private void HandleHPChanged(CharacterType characterType, int currentHP, int maxHP)
        {
            // UI更新はUIManagerに任せる
            // ここではキャラクターの死亡状態をチェックし、必要であれば切り替えを促す
            if (currentHP <= 0)
            {
                Debug.Log($"{characterType} のHPが0になりました。", this);
                // アクティブキャラクターが死亡した場合の切り替えはUpdateで処理される
            }
        }
    }
}
