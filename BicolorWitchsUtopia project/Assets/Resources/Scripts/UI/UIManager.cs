using UnityEngine;
using UnityEngine.UI;
using BicolorWitch.Game;
using BicolorWitch.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using BicolorWitch.Core;

namespace BicolorWitch.UI
{
    /// <summary>
    /// UIの表示・非表示、更新を管理するクラス。
    /// </summary>
    public class UIManager : MonoBehaviour, IUIManager
    {
        public static UIManager Instance { get; private set; }
        [Header("UIパネル")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject stageSelectPanel;
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject stageClearPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject magicSelectionPanel; // 魔法選択UIパネル
        [SerializeField] private GameObject initialSettingsPanel; // 初期設定UIパネル
        [SerializeField] private GameObject magicEquipSlot1UI; // 魔法装備スロット1のUI
        [SerializeField] private GameObject magicEquipSlot2UI; // 魔法装備スロット2のUI

        [Header("設定UI要素")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider seVolumeSlider;
        [SerializeField] private Dropdown languageDropdown; // 言語選択用Dropdown
        [SerializeField] private Transform magicListContentParent; // 魔法リストの親オブジェクト
        [SerializeField] private GameObject magicListItemPrefab; // 魔法リストのアイテムプレハブ
        [SerializeField] private Text magicEquipSlot1Text; // 魔法装備スロット1のテキスト
        [SerializeField] private Text magicEquipSlot2Text; // 魔法装備スロット2のテキスト
        [SerializeField] private Image magicEquipSlot1Icon; // 魔法装備スロット1のアイコン
        [SerializeField] private Image magicEquipSlot2Icon; // 魔法装備スロット2のアイコン

        [Header("色覚異常UI要素")]
        [SerializeField] private Text colorBlindnessTypeText; // 色覚異常タイプ表示用テキスト

        [Header("プレイヤーUI要素 (SisterT)")]
        [SerializeField] private Slider sisterTHPSlider;
        [SerializeField] private Text sisterTHPText;
        [SerializeField] private Slider sisterTMPSlider;
        [SerializeField] private Text sisterTMPText;


        [Header("プレイヤーUI要素 (SisterD)")]
        [SerializeField] private Slider sisterDHPSlider;
        [SerializeField] private Text sisterDHPText;
        [SerializeField] private Slider sisterDMPSlider;
        [SerializeField] private Text sisterDMPText;

        [Header("統合クールタイムUI要素")]
        [SerializeField] private Slider integratedCooldownSlider; // 統合クールタイムゲージ用のSlider

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => GManager.Instance != null);
            // 全てのUIパネルを非表示にする
            HideAllGameUIs();
        }

        /// <summary>
        /// 全てのゲーム関連UIパネルを非表示にする。
        /// </summary>
        public void HideAllGameUIs()
        {
            titlePanel?.SetActive(false);
            stageSelectPanel?.SetActive(false);
            gameUIPanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
            stageClearPanel?.SetActive(false);
            pausePanel?.SetActive(false);
            magicSelectionPanel?.SetActive(false);
            initialSettingsPanel?.SetActive(false);
        
        }

        /// <summary>
        /// タイトルUIを表示する。
        /// </summary>
        public void ShowTitleUI()
        {
            HideAllGameUIs();
            titlePanel?.SetActive(true);
        }

        /// <summary>
        /// ステージ選択UIを表示する。
        /// </summary>
        public void ShowStageSelectUI()
        {
            HideAllGameUIs();
            stageSelectPanel?.SetActive(true);
            // ステージ選択時に装備中の魔法UIも更新
            if (BicolorWitch.Magic.MagicSystem.Instance != null)
            {
                int magicID1 = BicolorWitch.Magic.MagicSystem.Instance.GetEquippedMagicID(0);
                int magicID2 = BicolorWitch.Magic.MagicSystem.Instance.GetEquippedMagicID(1);

                if (magicID1 != -1)
                {
                    Magic.MagicScroll magicScroll1 = BicolorWitch.Magic.MagicSystem.Instance.GetMagicScrollData(magicID1);
                    if (magicScroll1 != null) UpdateEquippedMagicUI(0, magicScroll1.MagicName, magicScroll1.Icon);
                }
                else
                {
                    magicEquipSlot1UI.SetActive(false);
                }

                if (magicID2 != -1)
                {
                    Magic.MagicScroll magicScroll2 = BicolorWitch.Magic.MagicSystem.Instance.GetMagicScrollData(magicID2);
                    if (magicScroll2 != null) UpdateEquippedMagicUI(1, magicScroll2.MagicName, magicScroll2.Icon);
                }
                else
                {
                    magicEquipSlot2UI.SetActive(false);
                }
            }
        }

        /// <summary>
        /// ゲームオーバーUIを表示する。
        /// </summary>
        public void ShowGameOverUI()
        {
            HideAllGameUIs();
            gameOverPanel?.SetActive(true);
        }

        /// <summary>
        /// ステージクリアUIを表示する。
        /// </summary>
        public void ShowStageClearUI()
        {
            HideAllGameUIs();
            stageClearPanel?.SetActive(true);
        }

        /// <summary>
        /// ポーズUIを表示する。
        /// </summary>
        public void ShowPauseUI()
        {
            HideAllGameUIs();
            pausePanel?.SetActive(true);
        }

        /// <summary>
        /// 魔法選択UIを表示する。
        /// </summary>
        public void ShowMagicSelectionUI()
        {
            HideAllGameUIs();
            magicSelectionPanel?.SetActive(true);
            UpdateMagicSelectionList();
        }

        /// <summary>
        /// 初期設定UIを表示する。
        /// </summary>
        public void ShowInitialSettingsUI()
        {
            HideAllGameUIs();
            initialSettingsPanel?.SetActive(true);
            // 設定をロードしてUIに反映
            SettingManager.Instance?.LoadSettings();
            if (SettingManager.Instance != null)
            {
                if (masterVolumeSlider != null) masterVolumeSlider.value = SettingManager.Instance.MasterVolume;
                if (bgmVolumeSlider != null) bgmVolumeSlider.value = SettingManager.Instance.BGMVolume;
                if (seVolumeSlider != null) seVolumeSlider.value = SettingManager.Instance.SEVolume;

                // 言語設定Dropdownの初期化
                if (languageDropdown != null)
                {
                    languageDropdown.ClearOptions();
                    List<string> options = new List<string>();
                    foreach (SettingManager.Language lang in Enum.GetValues(typeof(SettingManager.Language)))
                    {
                        options.Add(lang.ToString());
                    }
                    languageDropdown.AddOptions(options);
                    languageDropdown.value = (int)SettingManager.Instance.CurrentLanguage;
                }
            }

            // スライダーのイベントリスナーを設定
            masterVolumeSlider?.onValueChanged.RemoveAllListeners();
            masterVolumeSlider?.onValueChanged.AddListener(SettingManager.Instance.SetMasterVolume);

            bgmVolumeSlider?.onValueChanged.RemoveAllListeners();
            bgmVolumeSlider?.onValueChanged.AddListener(SettingManager.Instance.SetBGMVolume);

            seVolumeSlider?.onValueChanged.RemoveAllListeners();
            seVolumeSlider?.onValueChanged.AddListener(SettingManager.Instance.SetSEVolume);

            // 言語設定Dropdownのイベントリスナーを設定
            languageDropdown?.onValueChanged.RemoveAllListeners();
            languageDropdown?.onValueChanged.AddListener((index) =>
            {
                SettingManager.Instance?.SetLanguage((SettingManager.Language)index);
            });
        }

        /// <summary>
        /// 初期設定UIを非表示にする。
        /// </summary>
        public void HideInitialSettingsUI()
        {
            initialSettingsPanel?.SetActive(false);
            // 設定をセーブ
            SettingManager.Instance?.SaveSettings();
        }

        /// <summary>
        /// 魔法選択リストを更新する。
        /// </summary>
        public void UpdateMagicSelectionList()
        {
            if (magicListContentParent == null || magicListItemPrefab == null) return;

            // 既存のリストアイテムをクリア
            foreach (Transform child in magicListContentParent)
            {
                Destroy(child.gameObject);
            }

            // 所持している魔法スクロールのリストを取得し、表示
            if (BicolorWitch.Magic.MagicSystem.Instance != null)
            {
                List<int> ownedMagicIDs = BicolorWitch.Magic.MagicSystem.Instance.GetOwnedMagicScrolls();
                foreach (int magicID in ownedMagicIDs)
                {
                    BicolorWitch.Magic.MagicScroll magicScroll = BicolorWitch.Magic.MagicSystem.Instance.GetMagicScrollData(magicID);
                    if (magicScroll != null)
                    {
                        GameObject listItem = Instantiate(magicListItemPrefab, magicListContentParent);
                        MagicListItem item = listItem.GetComponent<MagicListItem>();
                        if (item != null)
                        {
                            item.Setup(magicScroll);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 魔法選択UIを非表示にする。
        /// </summary>
        public void HideMagicSelectionUI()
        {
            magicSelectionPanel?.SetActive(false);
            initialSettingsPanel?.SetActive(false);
            // 魔法リストも非表示にする
            if (magicListContentParent != null)
            {
                foreach (Transform child in magicListContentParent)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 装備中の魔法UIを更新する。
        /// </summary>
        /// <param name="slotIndex">スロットのインデックス (0または1)。</param>
        /// <param name="magicName">魔法の名前。</param>
        /// <param name="magicIcon">魔法のアイコン。</param>
        public void UpdateEquippedMagicUI(int slotIndex, string magicName, Sprite magicIcon)
        {
            if (slotIndex == 0)
            {
                magicEquipSlot1Text.text = magicName;
                magicEquipSlot1Icon.sprite = magicIcon;
                magicEquipSlot1UI.SetActive(true);
            }
            else if (slotIndex == 1)
            {
                magicEquipSlot2Text.text = magicName;
                magicEquipSlot2Icon.sprite = magicIcon;
                magicEquipSlot2UI.SetActive(true);
            }
        }

        /// <summary>
        /// 色覚異常タイプ表示UIを更新する。
        /// </summary>
        /// <param name="type">表示する色覚異常タイプ。</param>
        public void UpdateColorBlindnessUI(BicolorWitch.Manager.ShaderManager.ColorBlindnessType type)
        {
            if (colorBlindnessTypeText != null)
            {
                colorBlindnessTypeText.text = $"色覚: {type}";
            }
        }

        /// <summary>
        /// プレイヤーのHPを更新する。
        /// </summary>
        /// <param name="characterType">対象キャラクターのタイプ。</param>
        /// <param name="currentHP">現在のHP。</param>
        /// <param name="maxHP">最大HP。</param>
        public void UpdateHP(CharacterType characterType, float currentHP, float maxHP)
        {
            Slider targetSlider = null;
            Text targetText = null;

            if (characterType == CharacterType.SisterT)
            {
                targetSlider = sisterTHPSlider;
                targetText = sisterTHPText;
            }
            else if (characterType == CharacterType.SisterD)
            {
                targetSlider = sisterDHPSlider;
                targetText = sisterDHPText;
            }

            if (targetSlider != null)
            {
                targetSlider.maxValue = maxHP;
                targetSlider.value = currentHP;
            }
            if (targetText != null)
            {
                targetText.text = $"HP: {currentHP}/{maxHP}";
            }
        }

        /// <summary>
        /// プレイヤーのMPを更新する。
        /// </summary>
        /// <param name="characterType">対象キャラクターのタイプ。</param>
        /// <param name="currentMP">現在のMP。</param>
        /// <param name="maxMP">最大MP。</param>
        public void UpdateMP(CharacterType characterType, int currentMP, int maxMP)
        {
            Slider targetSlider = null;
            Text targetText = null;

            if (characterType == CharacterType.SisterT)
            {
                targetSlider = sisterTMPSlider;
                targetText = sisterTMPText;
            }
            else if (characterType == CharacterType.SisterD)
            {
                targetSlider = sisterDMPSlider;
                targetText = sisterDMPText;
            }

            if (targetSlider != null)
            {
                targetSlider.maxValue = maxMP;
                targetSlider.value = currentMP;
            }
            if (targetText != null)
            {
                targetText.text = $"MP: {currentMP}/{maxMP}";
            }
        }

        /// <summary>
        /// 統合クールタイムゲージのUIを更新する。
        /// </summary>
        /// <param name="currentCooldown">現在のクールタイム。</param>
        /// <param name="maxCooldown">最大クールタイム。</param>
        public void UpdateCooldownUI(float currentCooldown, float maxCooldown)
        {
            if (integratedCooldownSlider != null)
            {
                integratedCooldownSlider.maxValue = maxCooldown;
                integratedCooldownSlider.value = currentCooldown;
            }
        }

        // IUIManagerの古いメソッドの互換性のための実装
        public void UpdateDashCooldown(CharacterType characterType, float currentCooldown, float maxCooldown) => UpdateCooldownUI(currentCooldown, maxCooldown);
        public void UpdateSwitchCooldownUI(float currentCooldown, float maxCooldown) => UpdateCooldownUI(currentCooldown, maxCooldown);

        // GameManagerからゲームプレイUIの表示を指示される想定
        public void ShowGamePlayUI()
        {
            HideAllGameUIs();
            gameUIPanel?.SetActive(true);
        }

        /// <summary>
        /// 現在アクティブなキャラクターのUIを更新する。
        /// </summary>
        /// <param name="activeCharacter">アクティブなキャラクターの種類。</param>
        public void UpdateActiveCharacterUI(CharacterType activeCharacter)
        {
            // アクティブなキャラクターに応じてUIを切り替えるロジックを実装
            Debug.Log($"アクティブキャラクターが {activeCharacter} に切り替わりました。UIを更新します。", this);

            // 装備中の魔法UIも更新
            if (BicolorWitch.Magic.MagicSystem.Instance != null)
            {
                int magicID1 = BicolorWitch.Magic.MagicSystem.Instance.GetEquippedMagicID(0);
                int magicID2 = BicolorWitch.Magic.MagicSystem.Instance.GetEquippedMagicID(1);

                if (magicID1 != -1)
                {
                    Magic.MagicScroll magicScroll1 = BicolorWitch.Magic.MagicSystem.Instance.GetMagicScrollData(magicID1);
                    if (magicScroll1 != null) UpdateEquippedMagicUI(0, magicScroll1.MagicName, magicScroll1.Icon);
                }
                else
                {
                    magicEquipSlot1UI.SetActive(false);
                }

                if (magicID2 != -1)
                {
                    Magic.MagicScroll magicScroll2 = BicolorWitch.Magic.MagicSystem.Instance.GetMagicScrollData(magicID2);
                    if (magicScroll2 != null) UpdateEquippedMagicUI(1, magicScroll2.MagicName, magicScroll2.Icon);
                }
                else
                {
                    magicEquipSlot2UI.SetActive(false);
                }
            }
        }


    }
}
