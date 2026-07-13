using UnityEngine;
using System.Collections.Generic; // Dictionaryを使用するために追加

namespace BicolorWitch.Core
{
    /// <summary>
    /// ゲームのサウンドを管理するシングルトンクラス。
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField, Tooltip("BGMを再生するAudioSource")]
        private AudioSource bgmSource;
        [SerializeField, Tooltip("BGMクリップのリスト。IDで管理する")]
        private AudioClip[] bgmClips; // インスペクターから設定
        private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();
        [SerializeField, Tooltip("SEクリップのリスト。IDで管理する")]
        private AudioClip[] seClips; // インスペクターから設定
        private Dictionary<string, AudioClip> seDictionary = new Dictionary<string, AudioClip>();
        [SerializeField, Tooltip("SEを再生するAudioSource")]
        private AudioSource seSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSourceが設定されていない場合は追加
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true; // BGMはループ再生
            }
            if (seSource == null)
            {
                seSource = gameObject.AddComponent<AudioSource>();
            }

            // BGMクリップを辞書に登録
            foreach (AudioClip clip in bgmClips)
            {
                if (clip != null && !bgmDictionary.ContainsKey(clip.name))
                {
                    bgmDictionary.Add(clip.name, clip);
                }
            }

            // SEクリップを辞書に登録
            if (seClips != null)
            {
                foreach (AudioClip clip in seClips)
                {
                    if (clip != null && !seDictionary.ContainsKey(clip.name))
                    {
                        seDictionary.Add(clip.name, clip);
                    }
                }
            }
        }

        /// <summary>
        /// BGMを再生します。
        /// </summary>
        /// <summary>
        /// BGMを再生します。
        /// </summary>
        /// <param name="bgmID">再生するBGMのID。</param>
        public void PlayBGM(string bgmID)
        {
            if (bgmSource != null && bgmDictionary.ContainsKey(bgmID))
            {
                AudioClip clip = bgmDictionary[bgmID];
                if (bgmSource.clip != clip)
                {
                    bgmSource.clip = clip;
                    bgmSource.Play();
                    Debug.Log($"BGMを再生しました: {clip.name}");
                }
            }
            else
            {
                Debug.LogWarning($"BGM ID '{bgmID}' が見つからないか、AudioSourceが設定されていません。", this);
            }
        }

        /// <summary>
        /// BGMを停止します。
        /// </summary>
        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
                Debug.Log("BGMを停止しました。");
            }
        }

        /// <summary>
        /// SEを再生します。
        /// </summary>
        /// <param name="seID">再生するSEのID。</param>
        public void PlaySE(string seID)
        {
            if (seSource == null)
            {
                Debug.LogWarning($"SE AudioSourceが設定されていません。SE ID '{seID}' を再生できません。", this);
                return;
            }

            if (seDictionary.TryGetValue(seID, out AudioClip clip))
            {
                PlaySE(clip); // AudioClipを受け取るオーバーロードを呼び出す
            }
            else
            {
                Debug.LogWarning($"SE ID '{seID}' がseDictionaryに見つかりません。", this);
            }
        }

        /// <summary>
        /// SEを再生します。
        /// </summary>
        /// <param name="clip">再生するオーディオクリップ。</param>
        public void PlaySE(AudioClip clip)
        {
            if (seSource != null && clip != null)
            {
                seSource.PlayOneShot(clip);
                Debug.Log($"SEを再生しました: {clip.name}");
            }
        }

        /// <summary>
        /// SEの音量を設定します。
        /// </summary>
        /// <param name="volume">設定する音量（0.0f～1.0f）。</param>
        public void SetSEVolume(float volume)
        {
            if (seSource != null)
            {
                seSource.volume = Mathf.Clamp01(volume);
                Debug.Log($"SE音量を {seSource.volume} に設定しました。");
            }
        }

        /// <summary>
        /// BGMの音量を設定します。
        /// </summary>
        /// <param name="volume">設定する音量（0.0f～1.0f）。</param>
        public void SetBGMVolume(float volume)
        {
            if (bgmSource != null)
            {
                bgmSource.volume = Mathf.Clamp01(volume);
                Debug.Log($"BGM音量を {bgmSource.volume} に設定しました。");
            }
        }
    }
}