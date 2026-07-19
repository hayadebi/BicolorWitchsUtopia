using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BicolorWitch.Player;
using BicolorWitch.Enemy;
using BicolorWitch.UI;
using BicolorWitch.Core;

namespace BicolorWitch.Game
{
    /// <summary>
    /// ステージのデータを保持するクラス。
    /// </summary>
    [Serializable]
    public class StageData
    {
        [Tooltip("ステージの識別ID")]
        public string stageID;
        [Tooltip("ステージの表示名")]
        public string stageName;
        [Tooltip("ステージのプレハブ")]
        public GameObject stagePrefab;
    }

    /// <summary>
    /// ステージの進行とゲームの状態を管理するクラス。
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("ステージ管理")]
        [SerializeField, Tooltip("ゲーム内に存在する全ステージのデータリスト")]
        private List<StageData> stageDataList = new List<StageData>();
        [SerializeField, Tooltip("生成されたステージオブジェクトの親となるTransform")]
        private Transform stageParentTransform;

        [Header("ゲームオーバー設定")]
        [SerializeField, Tooltip("プレイヤーが落下したと判定されるY座標のしきい値")]
        private float fallThresholdY = -10f;

        private bool isGameOver = false;
        private GameObject currentStageInstance;
        [HideInInspector]
        public StageData currentStageData;

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

        private void Start()
        {
            // 初期状態ではステージはロードされていない状態とする
            // GameManagerなどから LoadStage が呼ばれることを想定
        }

        /// <summary>
        /// 指定されたIDのステージをロードし、生成する。
        /// </summary>
        /// <param name="stageID">ロードするステージのID。</param>
        public void LoadStage(string stageID)
        {
            StageData dataToLoad = stageDataList.Find(s => s.stageID == stageID);

            if (dataToLoad == null)
            {
                Debug.LogError($"ステージID \'{stageID}\' が見つかりません。", this);
                return;
            }

            // 現在のステージがあれば破棄
            if (currentStageInstance != null)
            {
                Destroy(currentStageInstance);
            }

            // 新しいステージを生成
            if (dataToLoad.stagePrefab != null)
            {
                currentStageInstance = Instantiate(dataToLoad.stagePrefab, stageParentTransform);
                currentStageData = dataToLoad;
                Debug.Log($"ステージ \'{dataToLoad.stageName}\' をロードしました。", this);

                // ステージロード後に状態をリセット
                ResetStageState();
            }
            else
            {
                Debug.LogError($"ステージ \'{dataToLoad.stageName}\' のプレハブが設定されていません。", this);
            }
        }

        /// <summary>
        /// 現在のステージをアンロード（破棄）する。タイトル画面などに戻る際に使用。
        /// </summary>
        public void UnloadCurrentStage()
        {
            if (currentStageInstance != null)
            {
                Destroy(currentStageInstance);
                currentStageInstance = null;
                currentStageData = null;
                Debug.Log("現在のステージをアンロードしました。", this);
            }
        }

        private void Update()
        {
            if (isGameOver || currentStageInstance == null) return;

            CheckGameOverConditions();
        }

        /// <summary>
        /// ステージの状態をリセットする。
        /// 主にキャラクターのHPやMPなどを初期状態に戻す。
        /// </summary>
        public void ResetStageState()
        {
            isGameOver = false;
            // キャラクターのHPとMPをリセット
            HPManager.Instance?.ResetAllCharacterStats();
            CharacterSwitcher.Instance?.ResetAllCharacterStats();

            // UIの更新
            UIManager.Instance?.UpdateHP(CharacterType.SisterT, HPManager.Instance.GetCurrentHP(CharacterType.SisterT), HPManager.Instance.GetMaxHP(CharacterType.SisterT));
            UIManager.Instance?.UpdateMP(CharacterType.SisterT, CharacterSwitcher.Instance.GetCurrentMP(CharacterType.SisterT), CharacterSwitcher.Instance.GetMaxMP(CharacterType.SisterT));
            UIManager.Instance?.UpdateDashCooldown(CharacterType.SisterT, 0f, CharacterSwitcher.Instance.GetMaxDashCooldown(CharacterType.SisterT));

            UIManager.Instance?.UpdateHP(CharacterType.SisterD, HPManager.Instance.GetCurrentHP(CharacterType.SisterD), HPManager.Instance.GetMaxHP(CharacterType.SisterD));
            UIManager.Instance?.UpdateMP(CharacterType.SisterD, CharacterSwitcher.Instance.GetCurrentMP(CharacterType.SisterD), CharacterSwitcher.Instance.GetMaxMP(CharacterType.SisterD));
            UIManager.Instance?.UpdateDashCooldown(CharacterType.SisterD, 0f, CharacterSwitcher.Instance.GetMaxDashCooldown(CharacterType.SisterD));

            Debug.Log("ステージ状態がリセットされました。", this);
        }

        /// <summary>
        /// ゲームオーバー条件をチェックする。
        /// </summary>
        private void CheckGameOverConditions()
        {
            // 両キャラクターのHPが0かチェック
            bool isSisterTDead = HPManager.Instance != null && HPManager.Instance.GetCurrentHP(CharacterType.SisterT) <= 0;
            bool isSisterDDead = HPManager.Instance != null && HPManager.Instance.GetCurrentHP(CharacterType.SisterD) <= 0;

            if (isSisterTDead && isSisterDDead)
            {
                TriggerGameOver("両キャラクターが死亡しました。");
                return;
            }

            // アクティブなキャラクターの落下チェック
            GameObject activePlayer = CharacterSwitcher.Instance?.GetActiveCharacterGameObject();
            if (CharacterSwitcher.Instance != null && activePlayer != null && activePlayer.transform.position.y < fallThresholdY)
            {
                TriggerGameOver($"{CharacterSwitcher.Instance.GetActiveCharacterType()}キャラクターが落下しました。");
                return;
            }
        }

        /// <summary>
        /// ゲームオーバー処理をトリガーする。
        /// </summary>
        /// <param name="reason">ゲームオーバーの理由。</param>
        private void TriggerGameOver(string reason)
        {
            if (isGameOver) return;

            isGameOver = true;
            Debug.Log($"ゲームオーバー: {reason}", this);
            GManager.Instance?.GameOver();
            UIManager.Instance?.ShowGameOverUI();

            // 必要に応じてゲームを一時停止するなどの処理
            Time.timeScale = 0f; // ゲームを一時停止
        }

        /// <summary>
        /// ステージクリア処理をトリガーする。
        /// </summary>
        public void TriggerStageClear()
        {
            if (isGameOver) return; // ゲームオーバー状態ではステージクリアしない

            Debug.Log("ステージクリア！", this);
            GManager.Instance?.StageClear();
            UIManager.Instance?.ShowStageClearUI();

            // 必要に応じてゲームを一時停止するなどの処理
            Time.timeScale = 0f; // ゲームを一時停止
        }
    }
}
