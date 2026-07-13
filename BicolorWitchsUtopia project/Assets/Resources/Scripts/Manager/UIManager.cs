using UnityEngine;
using UnityEngine.UI;
using BicolorWitch.Game;
using BicolorWitch.Player;
using System;
using System.Collections;
using BicolorWitch.Core;

namespace BicolorWitch.UI
{
    /// <summary>
    /// UIの表示・非表示、更新を管理するクラス。
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance = null;
        [Header("UIパネル")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject stageSelectPanel;
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject stageClearPanel;
        [SerializeField] private GameObject pausePanel;

        [Header("プレイヤーUI要素 (SisterT)")]
        [SerializeField] private Slider sisterTHPSlider;
        [SerializeField] private Text sisterTHPText;
        [SerializeField] private Slider sisterTMPSlider;
        [SerializeField] private Text sisterTMPText;
        [SerializeField] private Slider sisterTDashCooldownSlider;

        [Header("プレイヤーUI要素 (SisterD)")]
        [SerializeField] private Slider sisterDHPSlider;
        [SerializeField] private Text sisterDHPText;
        [SerializeField] private Slider sisterDMPSlider;
        [SerializeField] private Text sisterDMPText;
        [SerializeField] private Slider sisterDDashCooldownSlider;
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
        public void UpdateMP(CharacterType characterType, float currentMP, float maxMP)
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
        /// プレイヤーのダッシュクールタイムを更新する。
        /// </summary>
        /// <param name="characterType">対象キャラクターのタイプ。</param>
        /// <param name="currentCooldown">現在のクールタイム。</param>
        /// <param name="maxCooldown">最大クールタイム。</param>
        public void UpdateDashCooldown(CharacterType characterType, float currentCooldown, float maxCooldown)
        {
            Slider targetSlider = null;

            if (characterType == CharacterType.SisterT)
            {
                targetSlider = sisterTDashCooldownSlider;
            }
            else if (characterType == CharacterType.SisterD)
            {
                targetSlider = sisterDDashCooldownSlider;
            }

            if (targetSlider != null)
            {
                targetSlider.maxValue = maxCooldown;
                targetSlider.value = currentCooldown;
            }
        }

        // GameManagerからゲームプレイUIの表示を指示される想定
        public void ShowGamePlayUI()
        {
            HideAllGameUIs();
            gameUIPanel?.SetActive(true);
        }
    }
}
