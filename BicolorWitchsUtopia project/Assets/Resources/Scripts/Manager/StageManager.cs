using UnityEngine;
using System;
using System.Collections.Generic;
using BicolorWitch.Player;
using BicolorWitch.Enemy;

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
        [Header("ステージ管理")]
        [SerializeField, Tooltip("ゲーム内に存在する全ステージのデータリスト")]
        private List<StageData> stageDataList = new List<StageData>();
        [SerializeField, Tooltip("生成されたステージオブジェクトの親となるTransform")]
        private Transform stageParentTransform;

        [Header("ゲームオーバー設定")]
        [SerializeField, Tooltip("プレイヤーが落下したと判定されるY座標のしきい値")]
        private float fallThresholdY = -10f;

        [Header("依存コンポーネント")]
        [SerializeField, Tooltip("HP管理機能を提供するHPManagerを実装したオブジェクト")]
        private MonoBehaviour hpManagerMono;
        [SerializeField, Tooltip("キャラクター切り替え機能を提供するCharacterSwitcherを実装したオブジェクト")]
        private MonoBehaviour characterSwitcherMono;
        [SerializeField, Tooltip("ゲーム全体の管理機能を提供するGameManagerを実装したオブジェクト")]
        private MonoBehaviour gameManagerMono;
        [SerializeField, Tooltip("UI管理機能を提供するUIManagerを実装したオブジェクト")]
        private MonoBehaviour uiManagerMono;

        private IHPManager hpManager;
        private ICharacterSwitcher characterSwitcher;
        private IGameManager gameManager;
        private IUIManager uiManager;

        private bool isGameOver = false;
        private GameObject currentStageInstance;
        private StageData currentStageData;

        private void Awake()
        {
            // 依存コンポーネントの取得とNullチェック
            if (hpManagerMono == null || !(hpManagerMono is IHPManager))
            {
                Debug.LogError("HPManagerMonoが設定されていないか、IHPManagerを実装していません。", this);
                enabled = false;
                return;
            }
            hpManager = hpManagerMono as IHPManager;

            if (characterSwitcherMono == null || !(characterSwitcherMono is ICharacterSwitcher))
            {
                Debug.LogError("CharacterSwitcherMonoが設定されていないか、ICharacterSwitcherを実装していません。", this);
                enabled = false;
                return;
            }
            characterSwitcher = characterSwitcherMono as ICharacterSwitcher;

            if (gameManagerMono == null || !(gameManagerMono is IGameManager))
            {
                Debug.LogError("GameManagerMonoが設定されていないか、IGameManagerを実装していません。", this);
                // enabled = false; // GameManagerがないとゲームオーバー処理ができないため、エラーは出すがコンポーネントは無効化しない
                // return;
            }
            gameManager = gameManagerMono as IGameManager;

            if (uiManagerMono == null || !(uiManagerMono is IUIManager))
            {
                Debug.LogWarning("UIManagerMonoが設定されていないか、IUIManagerを実装していません。UI表示は行われません。", this);
                // UIは必須ではないため、警告に留める
            }
            uiManager = uiManagerMono as IUIManager;
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
            hpManager.ResetAllCharacterStats();
            characterSwitcher.ResetAllCharacterStats();

            // UIの更新
            uiManager?.UpdateHP(CharacterType.SisterT, hpManager.GetCurrentHP(CharacterType.SisterT), hpManager.GetMaxHP(CharacterType.SisterT));
            uiManager?.UpdateMP(CharacterType.SisterT, characterSwitcher.GetCurrentMP(CharacterType.SisterT), characterSwitcher.GetMaxMP(CharacterType.SisterT));
            uiManager?.UpdateDashCooldown(CharacterType.SisterT, 0f, characterSwitcher.GetMaxDashCooldown(CharacterType.SisterT));

            uiManager?.UpdateHP(CharacterType.SisterD, hpManager.GetCurrentHP(CharacterType.SisterD), hpManager.GetMaxHP(CharacterType.SisterD));
            uiManager?.UpdateMP(CharacterType.SisterD, characterSwitcher.GetCurrentMP(CharacterType.SisterD), characterSwitcher.GetMaxMP(CharacterType.SisterD));
            uiManager?.UpdateDashCooldown(CharacterType.SisterD, 0f, characterSwitcher.GetMaxDashCooldown(CharacterType.SisterD));

            Debug.Log("ステージ状態がリセットされました。", this);
        }

        /// <summary>
        /// ゲームオーバー条件をチェックする。
        /// </summary>
        private void CheckGameOverConditions()
        {
            // 両キャラクターのHPが0かチェック
            bool isSisterTDead = hpManager.GetCurrentHP(CharacterType.SisterT) <= 0;
            bool isSisterDDead = hpManager.GetCurrentHP(CharacterType.SisterD) <= 0;

            if (isSisterTDead && isSisterDDead)
            {
                TriggerGameOver("両キャラクターが死亡しました。");
                return;
            }

            // アクティブなキャラクターの落下チェック
            GameObject activePlayer = characterSwitcher.GetActiveCharacterGameObject();
            if (activePlayer != null && activePlayer.transform.position.y < fallThresholdY)
            {
                TriggerGameOver($"{characterSwitcher.GetActiveCharacterType()}キャラクターが落下しました。");
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
            gameManager?.GameOver();
            uiManager?.ShowGameOverUI();

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
            gameManager?.StageClear();
            uiManager?.ShowStageClearUI();

            // 必要に応じてゲームを一時停止するなどの処理
            Time.timeScale = 0f; // ゲームを一時停止
        }
    }
}
