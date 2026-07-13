using UnityEngine;
using BicolorWitch.Player;

namespace BicolorWitch.Magic
{
    /// <summary>
    /// 魔法のデータを定義するScriptableObject。
    /// 各魔法の属性、威力、消費MP、クールタイム、発射するプレハブなどの情報を保持する。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagicData", menuName = "BicolorWitch/Magic Data")]
    public class MagicData : ScriptableObject
    {
        [Header("基本設定")]
        [Tooltip("魔法の識別名")]
        public string Name = "New Magic";
        [Tooltip("魔法の説明")]
        [TextArea] public string description = "";
        [Tooltip("この魔法を使用できるキャラクターの種類")]
        public CharacterType usableCharacterType;

        [Header("コストとクールタイム")]
        [Tooltip("魔法を使用するために必要なMP")]
        public int CostMP = 10;
        [Tooltip("魔法のクールタイム（秒）")]
        public float CooldownTime = 1.0f;

        [Header("魔法の効果")]
        [Tooltip("魔法の威力（ダメージ量など）")]
        public int power = 10;
        [Tooltip("魔法の射程距離")]
        public float range = 10.0f;
        [Tooltip("魔法の発射速度")]
        public float projectileSpeed = 5.0f;

        [Header("視覚効果とプレハブ")]
        [Tooltip("魔法発動時のエフェクトプレハブ")]
        public GameObject castEffectPrefab;
        [Tooltip("魔法の弾（プロジェクタイル）のプレハブ")]
        public GameObject projectilePrefab;
        [Tooltip("魔法がヒットした際のエフェクトプレハブ")]
        public GameObject hitEffectPrefab;


    }
}
