using BicolorWitch.Player;

namespace BicolorWitch.Magic
{
    /// <summary>
    /// 魔法システム機能を提供するインターフェース。
    /// MagicSystemが実装することを想定。
    /// </summary>
    public interface IMagicSystem
    {
        /// <summary>
        /// 指定されたスロットの魔法の発動を試みる。
        /// </summary>
        /// <param name="slotIndex">魔法スロットのインデックス (例: 1または2)。</param>
        /// <returns>魔法の発動が成功した場合はtrue、失敗した場合はfalse。</returns>
        bool TryCastMagic(int slotIndex);
    }
}
