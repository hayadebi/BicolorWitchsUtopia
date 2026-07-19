using UnityEngine;
using System;
using System.Collections.Generic;
using BicolorWitch.Player;
using BicolorWitch.Game;
using BicolorWitch.UI; // UIManagerを参照するために追加
using BicolorWitch.Magic; // IMagicSystemを参照するために追加
using BicolorWitch.Effect; // EffectManagerを参照するために追加

namespace BicolorWitch.Magic
{


    /// <summary>
    /// 魔法システムを管理するクラス。
    /// PlayerControllerのIMagicSystemインターフェースを実装する。
    /// </summary>
    public class MagicSystem : MonoBehaviour, IMagicSystem
    {
        public static MagicSystem Instance { get; private set; }

        // 装備中の魔法IDを保持する配列 (スロット0とスロット1)
        private int[] equippedMagicIDs = { -1, -1 }; // -1は未装備を示す

        [Header("魔法データ")]
        [SerializeField, Tooltip("ゲーム内で利用可能な全ての魔法スクロールデータリスト")]
        private List<MagicScroll> allMagicScrolls = new List<MagicScroll>();

        // プレイヤーが所持している魔法スクロールのIDリスト
        private List<int> ownedMagicScrollIDs = new List<int>();


        /// <summary>
        /// 魔法スロットの実行時データを保持する内部クラス。
        /// </summary>
        private class MagicSlotData
        {
            public Magic.MagicData MagicDataAsset { get; private set; }
            public MagicScroll MagicScrollAsset { get; private set; }
            public float CurrentCooldown { get; private set; }

            public MagicSlotData(Magic.MagicData magicDataAsset, MagicScroll magicScrollAsset = null)
            {
                MagicDataAsset = magicDataAsset;
                MagicScrollAsset = magicScrollAsset;
                CurrentCooldown = 0f;
            }

            public bool IsReady => CurrentCooldown <= 0f;

            public void StartCooldown()
            {
                CurrentCooldown = MagicDataAsset.CooldownTime;
            }

            public void UpdateCooldown(float deltaTime)
            {
                if (CurrentCooldown > 0)
                {
                    CurrentCooldown -= deltaTime;
                    if (CurrentCooldown < 0) CurrentCooldown = 0;
                }
            }

            public void ResetCooldown()
            {
                CurrentCooldown = 0f;
            }
        }

        // CharacterType, SlotIndex, MagicSlotData
        private Dictionary<CharacterType, Dictionary<int, MagicSlotData>> characterMagicSlots;

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
            characterMagicSlots = new Dictionary<CharacterType, Dictionary<int, MagicSlotData>>();

            // Helper to add magic to a slot
            Action<CharacterType, int, int> AddMagicToSlot = (charType, slotIdx, magicID) =>
            {
                MagicScroll magicScroll = allMagicScrolls.Find(m => m.MagicID == magicID);
                if (magicScroll != null)
                {
                    if (!characterMagicSlots.ContainsKey(charType))
                    {
                        characterMagicSlots.Add(charType, new Dictionary<int, MagicSlotData>());
                    }
                   
                    characterMagicSlots[charType].Add(slotIdx, new MagicSlotData(magicScroll.MagicData, magicScroll));
                }
                else if (magicID != -1) // -1は未設定とみなす
                {
                    Debug.LogWarning($"Magic ID {magicID} for {charType} slot {slotIdx} not found in allMagicScrolls list.", this);
                }
            };

            // ロードされた装備魔法IDに基づいてスロットを初期化
            AddMagicToSlot(CharacterType.SisterT, 1, equippedMagicIDs[0]);
            AddMagicToSlot(CharacterType.SisterT, 2, equippedMagicIDs[1]);
            AddMagicToSlot(CharacterType.SisterD, 1, equippedMagicIDs[0]); // SisterDも同じ魔法を装備
            AddMagicToSlot(CharacterType.SisterD, 2, equippedMagicIDs[1]); // SisterDも同じ魔法を装備
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

            if (!magicSlots.TryGetValue(slotIndex, out var magicSlotData))
            {
                Debug.LogWarning($"キャラクター({activeCharacterType})の魔法スロット{slotIndex}に魔法が設定されていません。", this);
                return false;
            }

            if (!magicSlotData.IsReady)
            {
                Debug.LogWarning($"{magicSlotData.MagicDataAsset.Name} はクールタイム中です。残り: {magicSlotData.CurrentCooldown:F2}秒", this);
                return false;
            }

            // MP消費
            if (!CharacterSwitcher.Instance.ConsumeMP(activeCharacterType, magicSlotData.MagicDataAsset.CostMP))
            {
                Debug.LogWarning($"{magicSlotData.MagicDataAsset.Name} の発動に必要なMPが足りません。必要MP: {magicSlotData.MagicDataAsset.CostMP}", this);
                return false;
            }

            // 魔法発動処理
            Debug.Log($"{activeCharacterType} が {magicSlotData.MagicDataAsset.Name} (スロット{slotIndex}) を発動しました！", this);
            magicSlotData.StartCooldown();

            // TODO: 魔法の種類に応じた具体的な効果を実装する
            // 例: エフェクトの生成、ダメージ処理、回復処理など
            if (magicSlotData.MagicDataAsset.castEffectPrefab != null)
            {
                // EffectManager経由でエフェクトを再生
                Effect.EffectManager.Instance?.PlayEffect(magicSlotData.MagicDataAsset.castEffectPrefab, transform.position, Quaternion.identity);
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








        /// <summary>
        /// 魔法スクロールを獲得する。
        /// </summary>
        /// <param name="magicID">獲得する魔法のID。</param>
        public void AcquireMagicScroll(int magicID)
        {
            if (!ownedMagicScrollIDs.Contains(magicID))
            {
                ownedMagicScrollIDs.Add(magicID);
                Debug.Log($"魔法スクロール (ID: {magicID}) を獲得しました。", this);
                // 必要に応じてUIの更新などを呼び出す
                UIManager.Instance?.UpdateMagicSelectionList();
            }
            else
            {
                Debug.Log($"魔法スクロール (ID: {magicID}) は既に所持しています。", this);
            }
        }

        /// <summary>
        /// 魔法をスロットに装備する。
        /// </summary>
        /// <param name="magicID">装備する魔法のID。</param>
        /// <param name="slotIndex">装備するスロットのインデックス (0または1)。</param>
        public void EquipMagic(int magicID, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= equippedMagicIDs.Length)
            {
                Debug.LogError($"無効な魔法スロットインデックス: {slotIndex}", this);
                return;
            }

            if (!ownedMagicScrollIDs.Contains(magicID))
            {
                Debug.LogWarning($"所持していない魔法 (ID: {magicID}) を装備しようとしました。", this);
                return;
            }

            MagicScroll magicScroll = allMagicScrolls.Find(m => m.MagicID == magicID);
            if (magicScroll == null)
            {
                Debug.LogError($"魔法スクロールデータ (ID: {magicID}) が見つかりません。", this);
                return;
            }

            equippedMagicIDs[slotIndex] = magicID;
            Debug.Log($"魔法 {magicScroll.MagicName} をスロット {slotIndex + 1} に装備しました。", this);

            // 装備変更を反映するために魔法スロットを再初期化
            InitializeMagicSlots();

            // UIの更新
            UIManager.Instance?.UpdateEquippedMagicUI(slotIndex, magicScroll.MagicName, magicScroll.Icon);
        }

        /// <summary>
        /// 指定されたスロットに装備されている魔法のIDを取得する。
        /// </summary>
        /// <param name="slotIndex">スロットのインデックス (0または1)。</param>
        /// <returns>装備されている魔法のID。未装備の場合は-1。</returns>
        public int GetEquippedMagicID(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= equippedMagicIDs.Length)
            {
                Debug.LogError($"無効な魔法スロットインデックス: {slotIndex}", this);
                return -1;
            }
            return equippedMagicIDs[slotIndex];
        }

        /// <summary>
        /// 所持している魔法スクロールのIDリストを取得する。
        /// </summary>
        /// <returns>所持している魔法スクロールのIDリスト。</returns>
        public List<int> GetOwnedMagicScrolls()
        {
            return new List<int>(ownedMagicScrollIDs);
        }

        /// <summary>
        /// 所持している魔法スクロールのIDリストを設定する。（ロード用）
        /// </summary>
        /// <param name="scrollIDs">設定する魔法スクロールのIDリスト。</param>
        public void SetOwnedMagicScrolls(List<int> scrollIDs)
        {
            ownedMagicScrollIDs = new List<int>(scrollIDs);
            Debug.Log($"所持魔法スクロールをロードしました: {string.Join(", ", ownedMagicScrollIDs)}", this);
        }

        /// <summary>
        /// 魔法IDから魔法スクロールのデータを取得する。
        /// </summary>
        /// <param name="magicID">魔法ID。</param>
        /// <returns>魔法スクロールのデータ。見つからない場合はnull。</returns>
        public MagicScroll GetMagicScrollData(int magicID)
        {
            return allMagicScrolls.Find(m => m.MagicID == magicID);
        }
    }
}
