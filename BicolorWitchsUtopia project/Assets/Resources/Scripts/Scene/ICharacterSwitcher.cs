using BicolorWitch.Player;
using System;

namespace BicolorWitch.Player
{
    /// <summary>
    /// キャラクター切り替え機能を提供するインターフェース。
    /// CharacterSwitcherが実装することを想定。
    /// </summary>
    public interface ICharacterSwitcher
    {
        /// <summary>
        /// 現在操作中のキャラクターが死亡しているかを取得する。
        /// </summary>
        bool IsCurrentCharacterDead { get; }

        /// <summary>
        /// 全てのキャラクターが死亡しているかを取得する。
        /// </summary>
        bool AreAllCharactersDead { get; }

        /// <summary>
        /// キャラクターの切り替えを試みる。
        /// </summary>
        /// <returns>切り替えが成功した場合はtrue、失敗した場合はfalse。</returns>
        bool TrySwitchCharacter();

        /// <summary>
        /// 現在アクティブなキャラクターの種類を取得する。
        /// </summary>
        /// <returns>アクティブなキャラクターの種類。</returns>
        CharacterType GetActiveCharacterType();

        /// <summary>
        /// 現在アクティブなキャラクターのGameObjectを取得する。
        /// </summary>
        /// <returns>アクティブなキャラクターのGameObject。</returns>
        UnityEngine.GameObject GetActiveCharacterGameObject();

        /// <summary>
        /// 指定されたキャラクターのMPを消費する。
        /// </summary>
        /// <param name="characterType">MPを消費するキャラクターの種類。</param>
        /// <param name="amount">消費量。</param>
        /// <returns>消費に成功した場合はtrue、MPが足りない場合はfalse。</returns>
        bool ConsumeMP(CharacterType characterType, int amount);

        /// <summary>
        /// 指定されたキャラクターの現在のMPを取得する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>現在のMP。</returns>
        int GetCurrentMP(CharacterType characterType);

        /// <summary>
        /// 指定されたキャラクターの最大MPを取得する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>最大MP。</returns>
        int GetMaxMP(CharacterType characterType);

        /// <summary>
        /// キャラクターが切り替わったときに発火するイベント。
        /// </summary>
        event Action<CharacterType> OnCharacterSwitched;

        /// <summary>
        /// 全てのキャラクターのステータス（MP、ダッシュクールタイムなど）をリセットする。
        /// </summary>
        void ResetAllCharacterStats();

        /// <summary>
        /// 指定されたキャラクターの最大ダッシュクールタイムを取得する。
        /// </summary>
        /// <param name="characterType">キャラクターの種類。</param>
        /// <returns>最大ダッシュクールタイム。</returns>
        float GetMaxDashCooldown(CharacterType characterType);
    }
}
