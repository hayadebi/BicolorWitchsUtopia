using System;

namespace BicolorWitch.Player
{
    /// <summary>
    /// キャラクターごとのHP管理機能を提供するインターフェース。
    /// HPManagerが実装することを想定。
    /// </summary>
    public interface IHPManager
    {
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

        /// <summary>
        /// 指定されたキャラクターが死亡しているか（HPが0か）を判定する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>死亡している場合はtrue、それ以外はfalse。</returns>
        bool IsCharacterDead(CharacterType characterType);

        /// <summary>
        /// 指定されたキャラクターのHPが変更されたときに発火するイベント。
        /// (キャラクターの種類, 現在のHP, 最大HP)を引数に持つ。
        /// </summary>
        event Action<CharacterType, int, int> OnHPChanged;

        /// <summary>
        /// 指定されたキャラクターのHPをリセットする。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        void ResetHP(CharacterType characterType);

        /// <summary>
        /// 全てのキャラクターのHPをリセットする。
        /// </summary>
        void ResetAllCharacterStats();

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
    }
}
