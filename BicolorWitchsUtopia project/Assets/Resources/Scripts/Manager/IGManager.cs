// IGManager.cs (依存関係のため同時生成)
using BicolorWitch.Player;
using System;

namespace BicolorWitch.Game
{
    /// <summary>
    /// ゲーム管理機能を提供するインターフェース。
    /// GameManagerが実装することを想定。
    /// </summary>
    public interface IGManager
    {
        /// <summary>
        /// ゲームオーバー処理を開始する。
        /// </summary>
        void GameOver();

        /// <summary>
        /// ステージリセット処理を開始する。
        /// </summary>
        void ResetStage();

        void StageClear();
    }
}