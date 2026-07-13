using UnityEngine;

namespace BicolorWitch.Core
{
    /// <summary>
    /// ゲームのセーブとロードを管理するシングルトンクラス。
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

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

        /// <summary>
        /// ゲームデータをセーブします。
        /// </summary>
        public void SaveGame()
        {
            Debug.Log("ゲームをセーブしました。");
            // ここにセーブ処理の実装を追加
        }

        /// <summary>
        /// ゲームデータをロードします。
        /// </summary>
        public void LoadGame()
        {
            Debug.Log("ゲームをロードしました。");
            // ここにロード処理の実装を追加
        }
    }
}
