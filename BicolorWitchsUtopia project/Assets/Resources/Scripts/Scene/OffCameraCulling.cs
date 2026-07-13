using UnityEngine;

namespace BicolorWitch.Effect
{
    /// <summary>
    /// カメラの視界外に出たGameObjectを非アクティブ化し、視界内に入ったらアクティブ化するスクリプト。
    /// 主にパフォーマンス最適化のために使用する。
    /// </summary>
    public class OffCameraCulling : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer == null)
            {
                Debug.LogWarning($"Rendererが見つかりません。{gameObject.name} はOffCameraCullingの対象外です。", this);
                enabled = false; // Rendererがない場合はこのスクリプトを無効化
            }
        }

        private void OnBecameVisible()
        {
            // カメラに映り始めたらGameObjectをアクティブにする
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        private void OnBecameInvisible()
        {
            // カメラから見えなくなったらGameObjectを非アクティブにする
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
