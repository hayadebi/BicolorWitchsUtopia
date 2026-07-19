using UnityEngine;
using System;

namespace BicolorWitch.Core
{
    /// <summary>
    /// ゲームの設定を管理するシングルトンクラス。
    /// </summary>
    public class SettingManager : MonoBehaviour
    {
        public static SettingManager Instance { get; private set; }

        // 例: 音量設定
        public float MasterVolume { get; private set; } = 1.0f;
        public float BGMVolume { get; private set; } = 1.0f;
        public float SEVolume { get; private set; } = 1.0f;

        public enum Language
        {
            Japanese,
            English,
            Chinese,
            Korean
        }

        public Language CurrentLanguage { get; private set; } = Language.Japanese;

        public event Action<Language> OnLanguageChanged; // 言語変更イベント

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初回起動時に言語設定をロードまたは自動検出
            if (!PlayerPrefs.HasKey("Language"))
            {
                DetectSystemLanguage();
                SaveSettings(); // 初期言語設定を保存
            }
            else
            {
                LoadSettings();
            }
        }

        /// <summary>
        /// マスター音量を設定します。
        /// </summary>
        /// <param name="volume">設定する音量（0.0f～1.0f）。</param>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            Debug.Log($"マスター音量を {MasterVolume} に設定しました。");
            // ここに実際の音量設定処理（AudioMixerなど）を追加
            SoundManager.Instance?.SetBGMVolume(BGMVolume);
            SoundManager.Instance?.SetSEVolume(SEVolume);
        }

        /// <summary>
        /// BGM音量を設定します。
        /// </summary>
        /// <param name="volume">設定する音量（0.0f～1.0f）。</param>
        public void SetBGMVolume(float volume)
        {
            BGMVolume = Mathf.Clamp01(volume);
            SoundManager.Instance?.SetBGMVolume(BGMVolume);
            Debug.Log($"BGM音量を {BGMVolume} に設定しました。");
        }

        /// <summary>
        /// SE音量を設定します。
        /// </summary>
        /// <param name="volume">設定する音量（0.0f～1.0f）。</param>
        public void SetSEVolume(float volume)
        {
            SEVolume = Mathf.Clamp01(volume);
            SoundManager.Instance?.SetSEVolume(SEVolume);
            Debug.Log($"SE音量を {SEVolume} に設定しました。");
        }

        /// <summary>
        /// 言語を設定します。
        /// </summary>
        /// <param name="language">設定する言語。</param>
        public void SetLanguage(Language language)
        {
            if (CurrentLanguage == language) return;
            CurrentLanguage = language;
            Debug.Log($"言語を {CurrentLanguage} に設定しました。");
            OnLanguageChanged?.Invoke(CurrentLanguage); // 言語変更イベントを発火
            // ここに言語変更時の処理（UIの再描画など）を追加
        }

        /// <summary>
        /// システムの言語を検出し、設定します。
        /// </summary>
        private void DetectSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Japanese:
                    CurrentLanguage = Language.Japanese;
                    break;
                case SystemLanguage.English:
                    CurrentLanguage = Language.English;
                    break;
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    CurrentLanguage = Language.Chinese;
                    break;
                case SystemLanguage.Korean:
                    CurrentLanguage = Language.Korean;
                    break;
                default:
                    CurrentLanguage = Language.English; // デフォルトは英語
                    break;
            }
            Debug.Log($"システム言語を検出しました: {Application.systemLanguage} -> {CurrentLanguage}");
        }

        /// <summary>
        /// 設定をロードします。
        /// </summary>
        public void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            BGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            SEVolume = PlayerPrefs.GetFloat("SEVolume", 1.0f);
            CurrentLanguage = (Language)PlayerPrefs.GetInt("Language", (int)Language.Japanese);
            Debug.Log("設定をロードしました。");
            // ロードした設定を適用
            SoundManager.Instance?.SetBGMVolume(BGMVolume);
            SoundManager.Instance?.SetSEVolume(SEVolume);
        }

        /// <summary>
        /// 設定をセーブします。
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            PlayerPrefs.SetFloat("BGMVolume", BGMVolume);
            PlayerPrefs.SetFloat("SEVolume", SEVolume);
            PlayerPrefs.SetInt("Language", (int)CurrentLanguage);
            PlayerPrefs.Save(); // 変更をディスクに保存
            Debug.Log("設定をセーブしました。");
        }
    }
}
