using UnityEngine;

namespace BicolorWitch.Magic
{
    /// <summary>
    /// 魔法スクロールのデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagicScroll", menuName = "BicolorWitch/Magic Scroll")]
    public class MagicScroll : ScriptableObject
    {
        [Tooltip("魔法スクロールのID（ユニークであること）")]
        public string MagicScrollID;
        [Tooltip("魔法の名前")]
        public string MagicName;
        [Tooltip("魔法の説明")]
        [TextArea(3, 5)]
        public string Description;
        [Tooltip("このスクロールで習得できる魔法のID")]
        public int MagicID;
        public MagicData MagicData;
        [Tooltip("魔法のアイコン")]
        public Sprite Icon;
    }
}
