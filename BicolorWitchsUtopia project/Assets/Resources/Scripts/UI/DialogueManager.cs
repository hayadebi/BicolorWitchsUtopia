using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BicolorWitch.Core;
using BicolorWitch.UI;
using BicolorWitch.Manager;
using BicolorWitch.Localization;
using UnityEngine.UI;

namespace BicolorWitch.Dialogue
{
    /// <summary>
    /// 会話イベントの進行を管理するシングルトンクラス。
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI設定")]
        [SerializeField, Tooltip("会話テキストを表示するUIテキストコンポーネント")]
        private Text dialogueText;
        [SerializeField, Tooltip("話者名を表示するUIテキストコンポーネント")]
        private Text speakerNameText;
        [SerializeField, Tooltip("会話UIのルートGameObject")]
        private GameObject dialoguePanel;

        private Queue<DialogueMessage> currentDialogueQueue;
        private DialogueEvent currentDialogueEvent;
        private Action onDialogueFinishedCallback;
        private bool isTyping = false;
        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            dialoguePanel.SetActive(false);
        }

        private void OnEnable()
        {
            BicolorWitch.InputManagement.InputManager.Instance.OnBasicAttackInput += HandleAdvanceDialogue;
            BicolorWitch.InputManagement.InputManager.Instance.OnPauseInput += HandleSkipDialogue;
        }

        private void OnDisable()
        {
            BicolorWitch.InputManagement.InputManager.Instance.OnBasicAttackInput -= HandleAdvanceDialogue;
            BicolorWitch.InputManagement.InputManager.Instance.OnPauseInput -= HandleSkipDialogue;
        }

        /// <summary>
        /// 会話イベントを開始する。
        /// </summary>
        /// <param name="dialogueEvent">開始する会話イベントデータ。</param>
        /// <param name="onFinished">会話終了時に呼び出すコールバック。</param>
        public void StartDialogue(DialogueEvent dialogueEvent, Action onFinished = null)
        {
            if (dialogueEvent == null || dialogueEvent.Messages == null || dialogueEvent.Messages.Count == 0)
            {
                Debug.LogWarning("開始する会話イベントが空か無効です。", this);
                onFinished?.Invoke();
                return;
            }

            currentDialogueEvent = dialogueEvent;
            onDialogueFinishedCallback = onFinished;
            currentDialogueQueue = new Queue<DialogueMessage>(dialogueEvent.Messages);

            // ゲームの状態を会話中に変更
            BicolorWitch.Core.GManager.Instance?.SetGameState(BicolorWitch.Core.GManager.GameState.Dialogue);

            dialoguePanel.SetActive(true);
            DisplayNextMessage();
        }

        /// <summary>
        /// 次のメッセージを表示する。
        /// </summary>
        private void DisplayNextMessage()
        {
            if (isTyping)
            {
                // タイピング中の場合は、テキストを最後まで表示
                StopCoroutine(typingCoroutine);
                dialogueText.text = GetLocalizedMessage(currentDialogueQueue.Peek().LocalizedMessages);
                isTyping = false;
                return;
            }

            if (currentDialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            DialogueMessage message = currentDialogueQueue.Dequeue();
            speakerNameText.text = message.SpeakerName;

            // BGM/SEの再生
            if (!string.IsNullOrEmpty(message.BGM_ID))
            {
                BicolorWitch.Core.SoundManager.Instance?.PlayBGM(message.BGM_ID);
            }
            if (!string.IsNullOrEmpty(message.SE_ID))
            {
                BicolorWitch.Core.SoundManager.Instance?.PlaySE(message.SE_ID);
            }

            // 表情の更新（CharacterSwitcherやEffectManagerと連携）
            // if (!string.IsNullOrEmpty(message.Expression_ID))
            // {
            //     // TODO: キャラクターの表情を更新するロジック
            // }

            string localizedText = GetLocalizedMessage(message.LocalizedMessages);
            typingCoroutine = StartCoroutine(TypeSentence(localizedText));
        }

        /// <summary>
        /// テキストをタイピングエフェクトで表示するコルーチン。
        /// </summary>
        /// <param name="sentence">表示する文章。</param>
        private IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;
            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return null; // 1フレーム待機
            }
            isTyping = false;
        }

        /// <summary>
        /// 会話を進める入力ハンドラ。
        /// </summary>
        private void HandleAdvanceDialogue()
        {
            if (BicolorWitch.Core.GManager.Instance?.currentGameState == BicolorWitch.Core.GManager.GameState.Dialogue)
            {
                DisplayNextMessage();
            }
        }

        /// <summary>
        /// 会話をスキップする入力ハンドラ。
        /// </summary>
        private void HandleSkipDialogue()
        {
            if (BicolorWitch.Core.GManager.Instance?.currentGameState == BicolorWitch.Core.GManager.GameState.Dialogue)
            {
                EndDialogue();
            }
        }

        /// <summary>
        /// 現在の言語設定に基づいて、LocalizedMessagesリストから適切なメッセージを取得します。
        /// </summary>
        /// <param name="localizedMessages">多言語対応メッセージのリスト。</param>
        /// <returns>現在の言語に対応するメッセージ。見つからない場合は日本語、それも見つからない場合はエラーメッセージ。</returns>
        private string GetLocalizedMessage(List<BicolorWitch.Localization.Localizer.LocalizedString> localizedMessages)
        {
            if (SettingManager.Instance == null) return "SettingManager not found!";

            SettingManager.Language currentLanguage = SettingManager.Instance.CurrentLanguage;

            foreach (var entry in localizedMessages)
            {
                if (entry.language == currentLanguage)
                {
                    return entry.text;
                }
            }

            // 現在の言語が見つからない場合、日本語を試す
            foreach (var entry in localizedMessages)
            {
                if (entry.language == SettingManager.Language.Japanese)
                {
                    return entry.text;
                }
            }

            return "Localization Error!"; // どの言語も見つからない場合
        }

        /// <summary>
        /// 会話イベントを終了する。
        /// </summary>
        private void EndDialogue()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            dialoguePanel.SetActive(false);
            isTyping = false;
            currentDialogueQueue.Clear();

            // ゲームの状態をPlayingに戻すか、前の状態に戻す
            BicolorWitch.Core.GManager.Instance?.SetGameState(BicolorWitch.Core.GManager.GameState.Playing); // 仮でPlayingに戻す

            onDialogueFinishedCallback?.Invoke();
            onDialogueFinishedCallback = null;
            currentDialogueEvent = null;
        }
    }
}