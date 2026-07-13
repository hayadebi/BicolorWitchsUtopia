using UnityEngine;
using System;
using System.Collections.Generic;
using BicolorWitch.Core;

namespace BicolorWitch.Dialogue
{
    /// <summary>
    /// 会話イベントの各メッセージデータを保持する構造体。
    /// </summary>
    [Serializable]
    public struct DialogueMessage
    {
        [Tooltip("話者の名前")]
        public string SpeakerName;
        [Tooltip("表示するメッセージ内容（多言語対応）")]
        public List<BicolorWitch.Localization.Localizer.LocalizedString> LocalizedMessages;
        [Tooltip("メッセージ表示中に再生するBGMのID（オプション）")]
        public string BGM_ID; // SoundManagerと連携
        [Tooltip("メッセージ表示中に再生するSEのID（オプション）")]
        public string SE_ID; // SoundManagerと連携
        [Tooltip("メッセージ表示中に表示するキャラクターの表情ID（オプション）")]
        public string Expression_ID; // キャラクターの表情管理と連携
    }

    /// <summary>
    /// 一連の会話イベントデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueEvent", menuName = "BicolorWitch/Dialogue Event")]
    public class DialogueEvent : ScriptableObject
    {
        [Tooltip("会話イベントのID（ユニークであること）")]
        public string EventID;
        [Tooltip("この会話イベントを構成するメッセージのリスト")]
        public List<DialogueMessage> Messages;
    }
}
