using UnityEngine;
using System;
using System.Collections.Generic;
using BicolorWitch.Player;
using BicolorWitch.Game;
using BicolorWitch.UI;

namespace BicolorWitch.Magic
{
    /// <summary>
    /// 魔法の種類を定義する列挙型。
    /// </summary>
    public enum MagicType
    {
        None,
        Fireball,   // 火の玉
        Heal,       // 回復
        Shield,     // シールド
        // 必要に応じて魔法を追加
    }

    /// <summary>
    /// 魔法のデータ構造。
    /// </summary>
    [Serializable]
    public class MagicData
    {
        [Tooltip("魔法の種類")]
        public MagicType Type;
        [Tooltip("魔法の名前")]
        public string Name;
        [Tooltip("魔法の消費MP")]
        public int CostMP;
        [Tooltip("魔法のクールタイム")]
        public float CooldownTime;
        [Tooltip("魔法のエフェクトPrefab (任意)")]
        public GameObject EffectPrefab;
        [Tooltip("魔法のアイコン (任意)")]
        public Sprite Icon;

        [NonSerialized] private float currentCooldown = 0f;

        /// <summary>
        /// 現在のクールタイムを取得する。
        /// </summary>
        public float CurrentCooldown => currentCooldown;

        /// <summary>
        /// 魔法が使用可能か判定する。
        /// </summary>
        public bool IsReady => currentCooldown <= 0f;

        /// <summary>
        /// クールタイムを開始する。
        /// </summary>
        public void StartCooldown()
        {
            currentCooldown = CooldownTime;
        }

        /// <summary>
        /// クールタイムを更新する。
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間。</param>
        public void UpdateCooldown(float deltaTime)
        {
            if (currentCooldown > 0f)
            {
                currentCooldown -= deltaTime;
                if (currentCooldown < 0f)
                {
                    currentCooldown = 0f;
                }
            }
        }

        /// <summary>
        /// クールタイムをリセットする。
        /// </summary>
        public void ResetCooldown()
        {
            currentCooldown = 0f;
        }
    }

    /// <summary>
    /// 魔法システムを管理するクラス。
    /// PlayerControllerのIMagicSystemインターフェースを実装する。
    /// </summary>
    public class MagicSystem : MonoBehaviour, IMagicSystem
    {
        public static MagicSystem Instance { get; private set; }

        [Header("魔法データ")]
        [SerializeField, Tooltip("ゲーム内で利用可能な全ての魔法データリスト")]
        private List<MagicData> allMagicSpells = new List<MagicData>();

        [Header("キャラクター別魔法スロット設定")]
        [SerializeField, Tooltip("姉の魔法スロット1に設定する魔法のallMagicSpellsリストインデックス (-1で未設定)")]
        private int sisterTMagicSlot1Index = -1;
        [SerializeField, Tooltip("姉の魔法スロット2に設定する魔法のallMagicSpellsリストインデックス (-1で未設定)")]
        private int sisterTMagicSlot2Index = -1;
        [SerializeField, Tooltip("妹の魔法スロット1に設定する魔法のallMagicSpellsリストインデックス (-1で未設定)")]
        private int sisterDMagicSlot1Index = -1;
        [SerializeField, Tooltip("妹の魔法スロット2に設定する魔法のallMagicSpellsリストインデックス (-1で未設定)")]
        private int sisterDMagicSlot2Index = -1;

        // CharacterType, SlotIndex, MagicData
        private Dictionary<CharacterType, Dictionary<int, MagicData>> characterMagicSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeMagicSlots();
        }

        private void Update()
        {
            if (characterMagicSlots == null) return;

            // 全ての魔法のクールタイムを更新
            foreach (var characterEntry in characterMagicSlots)
            {
                if (characterEntry.Value == null) continue;

                foreach (var magicEntry in characterEntry.Value)
                {
                    if (magicEntry.Value != null)
                    {
                        magicEntry.Value.UpdateCooldown(Time.deltaTime);
                    }
                }
            }
        }

        private void InitializeMagicSlots()
        {
            characterMagicSlots = new Dictionary<CharacterType, Dictionary<int, MagicData>>();

            // Helper to add magic to a slot
            Action<CharacterType, int, int> AddMagicToSlot = (charType, slotIdx, magicIdx) =>
            {
                if (magicIdx >= 0 && magicIdx < allMagicSpells.Count)
                {
                    if (!characterMagicSlots.ContainsKey(charType))
                    {
                        characterMagicSlots.Add(charType, new Dictionary<int, MagicData>());
                    }
                    characterMagicSlots[charType].Add(slotIdx, allMagicSpells[magicIdx]);
                }
                else if (magicIdx != -1) // -1は未設定とみなす
                {
                    Debug.LogWarning($"Magic index {magicIdx} for {charType} slot {slotIdx} is out of bounds or invalid. Please check allMagicSpells list size.", this);
                }
            };

            AddMagicToSlot(CharacterType.SisterT, 1, sisterTMagicSlot1Index);
            AddMagicToSlot(CharacterType.SisterT, 2, sisterTMagicSlot2Index);
            AddMagicToSlot(CharacterType.SisterD, 1, sisterDMagicSlot1Index);
            AddMagicToSlot(CharacterType.SisterD, 2, sisterDMagicSlot2Index);
        }

        /// <summary>
        /// 指定されたスロットの魔法の発動を試みる。
        /// IMagicSystemインターフェースの実装。
        /// </summary>
        /// <param name="slotIndex">魔法スロットのインデックス (例: 1または2)。</param>
        /// <returns>魔法の発動が成功した場合はtrue、失敗した場合はfalse。</returns>
        public bool TryCastMagic(int slotIndex)
        {
            CharacterType activeCharacterType = CharacterSwitcher.Instance.GetActiveCharacterType();

            if (!characterMagicSlots.TryGetValue(activeCharacterType, out var magicSlots))
            {
                Debug.LogWarning($"アクティブなキャラクター({activeCharacterType})の魔法スロットが見つかりません。", this);
                return false;
            }

            if (!magicSlots.TryGetValue(slotIndex, out var magicData))
            {
                Debug.LogWarning($"キャラクター({activeCharacterType})の魔法スロット{slotIndex}に魔法が設定されていません。", this);
                return false;
            }

            if (!magicData.IsReady)
            {
                Debug.LogWarning($"{magicData.Name} はクールタイム中です。残り: {magicData.CurrentCooldown:F2}秒", this);
                return false;
            }

            // MP消費
            if (!CharacterSwitcher.Instance.ConsumeMP(activeCharacterType, magicData.CostMP))
            {
                Debug.LogWarning($"{magicData.Name} の発動に必要なMPが足りません。必要MP: {magicData.CostMP}", this);
                return false;
            }

            // 魔法発動処理
            Debug.Log($"{activeCharacterType} が {magicData.Name} (スロット{slotIndex}) を発動しました！", this);
            magicData.StartCooldown();

            // TODO: 魔法の種類に応じた具体的な効果を実装する
            // 例: エフェクトの生成、ダメージ処理、回復処理など
            if (magicData.EffectPrefab != null)
            {
                Instantiate(magicData.EffectPrefab, transform.position, Quaternion.identity);
            }

            // UI更新
            UIManager.Instance.UpdateMP(activeCharacterType, CharacterSwitcher.Instance.GetCurrentMP(activeCharacterType), CharacterSwitcher.Instance.GetMaxMP(activeCharacterType));

            return true;
        }

        /// <summary>
        /// 全ての魔法のクールタイムをリセットする。
        /// ステージ開始時などに呼び出すことを想定。
        /// </summary>
        public void ResetAllMagicCooldowns()
        {
            foreach (var characterEntry in characterMagicSlots)
            {
                foreach (var magicEntry in characterEntry.Value)
                {
                    magicEntry.Value.ResetCooldown();
                }
            }
        }
    }
}
