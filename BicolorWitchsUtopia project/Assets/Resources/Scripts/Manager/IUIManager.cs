// IUIManager.cs (依存関係のため同時生成)
using BicolorWitch.Player;
using System;

namespace BicolorWitch.Game
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

        /// <summary>
        /// 指定されたキャラクターのHPをUIに表示する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <param name="currentHP">現在のHP。</param>
        /// <param name="maxHP">最大HP。</param>
        void UpdateHP(CharacterType characterType, int currentHP, int maxHP);

        /// <summary>
        /// ダッシュクールタイムのUIを更新する。
        /// </summary>
        /// <param name="currentCooldown">現在のクールタイム。</param>
        /// <param name="maxCooldown">最大クールタイム。</param>
        void UpdateDashCooldownUI(float currentCooldown, float maxCooldown);

        /// <summary>
        /// ゲームオーバーUIを表示する。
        /// </summary>
        void ShowGameOverUI();

        /// <summary>
        /// ステージクリアUIを表示する。
        /// </summary>
        void ShowStageClearUI();

        void ShowTitleUI();
        void ShowStageSelectUI();
        void HideAllGameUIs();
        void ShowPauseUI();

        /// <summary>
        /// キャラクターのダッシュクールタイムを更新する。
        /// </summary>
        /// <param name="characterType">更新するキャラクターのタイプ。</param>
        /// <param name="currentCooldown">現在のクールタイム。</param>
        /// <param name="maxCooldown">最大クールタイム。</param>
        void UpdateDashCooldown(CharacterType characterType, float currentCooldown, float maxCooldown);
    }
}