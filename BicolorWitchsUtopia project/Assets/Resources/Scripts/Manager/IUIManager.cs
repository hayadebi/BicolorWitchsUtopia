using BicolorWitch.Player;

namespace BicolorWitch.Game
{
    /// <summary>
    /// UI管理機能を提供するインターフェース。
    /// </summary>
    public interface IUIManager
    {
        void ShowTitleUI();
        void ShowStageSelectUI();
        void ShowGameOverUI();
        void ShowStageClearUI();
        void ShowPauseUI();
        void HideAllGameUIs();
        void UpdateHP(CharacterType characterType, float currentHP, float maxHP);
        void UpdateMP(CharacterType characterType, float currentMP, float maxMP);
        void UpdateDashCooldown(CharacterType characterType, float currentCooldown, float maxCooldown);
        void ShowGamePlayUI();

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
}
