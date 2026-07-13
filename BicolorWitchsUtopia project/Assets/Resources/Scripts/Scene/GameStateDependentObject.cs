using UnityEngine;
using BicolorWitch.Core;

namespace BicolorWitch.Game
{
    /// <summary>
    /// ゲームの状態がPlayingの時のみアクティブになるオブジェクトを制御するスクリプト。
    /// </summary>
    public class GameStateDependentObject : MonoBehaviour
    {
        private void OnEnable()
        {
            if (GManager.Instance != null)
            {
                GManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                // 現在のゲーム状態に基づいて初期状態を設定
                gameObject.SetActive(GManager.Instance.GetCurrentGameState() == GManager.GameState.Playing);
            }
            else
            {
                Debug.LogWarning("GManager.Instanceが見つかりません。GameStateDependentObjectは正しく動作しない可能性があります。", this);
                // GManagerがない場合、とりあえずアクティブにしておくか、非アクティブにするか、要検討
                // ここではデフォルトでアクティブにしておく
                gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (GManager.Instance != null)
            {
                GManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GManager.GameState newGameState)
        {
            gameObject.SetActive(newGameState == GManager.GameState.Playing);
        }
    }
}
