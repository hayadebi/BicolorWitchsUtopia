using UnityEngine;
using System;
using BicolorWitch.Player;
using BicolorWitch.Game;
using BicolorWitch.UI;

namespace BicolorWitch.Core
{
    /// <summary>
    /// ゲーム全体の進行を管理するクラス。
    /// シングルトンパターンを適用し、ゲーム全体で唯一のインスタンスであることを保証する。
    /// </summary>
    public class GManager : MonoBehaviour, IGManager
    {
        public static GManager Instance { get; private set; }

        public GameState currentGameState = GameState.Title;

        public event Action<GameState> OnGameStateChanged;

        public enum GameState
        {
            Title,
            StageSelect,
            Playing,
            GameOver,
            StageClear,
            Pause,
            Dialogue
        }
        public void CallUnloadUnusedAssets(){
            Resources.UnloadUnusedAssets();
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
        }

        private void Start()
        {
            // ゲーム開始時の初期状態を設定
            SetGameState(GameState.Title);
        }

        /// <summary>
        /// ゲームの状態を設定する。
        /// </summary>
        /// <param name="newState">設定する新しいゲームの状態。</param>
        public void SetGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            Debug.Log($"ゲーム状態が {currentGameState} から {newState} に変更されました。", this);
            currentGameState = newState;
            OnGameStateChanged?.Invoke(newState);
            switch (currentGameState)
            {
                case GameState.Title:
                    // タイトル画面のUI表示、ステージアンロードなど
                    StageManager.Instance?.UnloadCurrentStage();
                    UIManager.Instance?.ShowTitleUI();
                    Time.timeScale = 1f;
                    break;
                case GameState.StageSelect:
                    // ステージ選択画面のUI表示
                    UIManager.Instance?.ShowStageSelectUI();
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    // プレイ中のUI表示、ゲーム再開など
                    UIManager.Instance?.HideAllGameUIs(); // タイトルやステージ選択UIを非表示にする想定
                    Time.timeScale = 1f;
                    break;
                case GameState.GameOver:
                    // ゲームオーバー処理はStageManagerから呼ばれる
                    Time.timeScale = 0f;
                    break;
                case GameState.StageClear:
                    // ステージクリア処理はStageManagerから呼ばれる
                    Time.timeScale = 0f;
                    break;
                case GameState.Pause:
                    // ポーズUI表示、ゲーム一時停止
                    UIManager.Instance?.ShowPauseUI();
                    Time.timeScale = 0f;
                    break;
            }
        }

        /// <summary>
        /// 現在のゲーム状態を取得する。
        /// </summary>
        /// <returns>現在のゲーム状態。</returns>
        public GameState GetCurrentGameState()
        {
            return currentGameState;
        }

        /// <summary>
        /// IGManagerインターフェースの実装。ゲームオーバーを通知する。
        /// </summary>
        public void GameOver()
        {
            SetGameState(GameState.GameOver);
            UIManager.Instance?.ShowGameOverUI();
            // 必要に応じてリトライボタン表示など
        }

        /// <summary>
        /// IGManagerインターフェースの実装。ステージリセット処理を開始する。
        /// </summary>
        public void ResetStage()
        {
            StageManager.Instance?.ResetStageState();
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// IGManagerインターフェースの実装。ステージクリアを通知する。
        /// </summary>
        public void StageClear()
        {
            SetGameState(GameState.StageClear);
            UIManager.Instance?.ShowStageClearUI();
            // 必要に応じて次のステージへ進むボタン表示など
        }

        /// <summary>
        /// 指定されたステージIDでゲームを開始する。
        /// </summary>
        /// <param name="stageID">開始するステージのID。</param>
        public void StartGame(string stageID)
        {
            StageManager.Instance?.LoadStage(stageID);
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// ゲームをポーズする。
        /// </summary>
        public void PauseGame()
        {
            SetGameState(GameState.Pause);
        }

        /// <summary>
        /// ポーズを解除し、ゲームを再開する。
        /// </summary>
        public void ResumeGame()
        {
            if (currentGameState == GameState.Pause)
            {
                SetGameState(GameState.Playing);
            }
        }

        /// <summary>
        /// タイトル画面に戻る。
        /// </summary>
        public void ReturnToTitle()
        {
            SetGameState(GameState.Title);
        }

        /// <summary>
        /// ゲームを終了する。
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
