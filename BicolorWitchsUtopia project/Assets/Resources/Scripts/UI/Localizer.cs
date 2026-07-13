using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BicolorWitch.Core;

namespace BicolorWitch.Localization
{
    /// <summary>
    /// テキストコンポーネントを持つオブジェクトにアタッチし、
    /// SettingManagerの言語設定に応じて表示内容を切り替えるスクリプト。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class Localizer : MonoBehaviour
    {
        [System.Serializable]
        public class LocalizedString
        {
            public SettingManager.Language language;
            [TextArea(3, 10)]
            public string text;
        }

        [SerializeField] private List<LocalizedString> localizedStrings = new List<LocalizedString>();
        private Text targetText;

        private void Awake()
        {
            targetText = GetComponent<Text>();
        }

        private void OnEnable()
        {
            // SettingManagerの言語変更イベントを購読
            if (SettingManager.Instance != null)
            {
                SettingManager.Instance.OnLanguageChanged += UpdateText;
            }
            UpdateText(SettingManager.Instance?.CurrentLanguage ?? SettingManager.Language.Japanese); // 初期表示
        }

        private void OnDisable()
        {
            // イベント購読を解除
            if (SettingManager.Instance != null)
            {
                SettingManager.Instance.OnLanguageChanged -= UpdateText;
            }
        }

        /// <summary>
        /// 現在の言語設定に基づいてテキストを更新します。
        /// </summary>
        /// <param name="newLanguage">新しい言語設定。</param>
        private void UpdateText(SettingManager.Language newLanguage)
        {
            if (targetText == null) return;

            foreach (var entry in localizedStrings)
            {
                if (entry.language == newLanguage)
                {
                    targetText.text = entry.text;
                    return;
                }
            }
            // 指定された言語のテキストが見つからない場合、デフォルト（日本語）を表示
            foreach (var entry in localizedStrings)
            {
                if (entry.language == SettingManager.Language.Japanese)
                {
                    targetText.text = entry.text;
                    return;
                }
            }
            targetText.text = "Localization Error!"; // どの言語も見つからない場合
        }

        /// <summary>
        /// LocalizedStringリストに新しいエントリを追加します。
        /// </summary>
        public void AddLocalizedString(SettingManager.Language language, string text)
        {
            localizedStrings.Add(new LocalizedString { language = language, text = text });
        }

        /// <summary>
        /// LocalizedStringリストをクリアします。
        /// </summary>
        public void ClearLocalizedStrings()
        {
            localizedStrings.Clear();
        }
    }
}
