using UnityEngine;
using BicolorWitch.Magic;
using BicolorWitch.Core;

namespace BicolorWitch.Game
{
    /// <summary>
    /// プレイヤーが魔法スクロールを獲得するためのアイテム。
    /// </summary>
    public class MagicScrollPickup : MonoBehaviour
    {
        [SerializeField, Tooltip("このアイテムで獲得できる魔法スクロールのデータ")]
        private MagicScroll magicScrollData;

        [SerializeField, Tooltip("獲得時に再生するエフェクトのプレハブ（オプション）")]
        private GameObject pickupEffectPrefab;

        [SerializeField, Tooltip("獲得時に再生するSEのID（オプション）")]
        private string pickupSoundID;

        private bool hasBeenPickedUp = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenPickedUp) return;

            // プレイヤーが触れた場合
            if (other.CompareTag("Player"))
            {
                if (magicScrollData == null)
                {
                    Debug.LogWarning($"MagicScrollPickup: {gameObject.name} に MagicScrollData が設定されていません。", this);
                    return;
                }

                // 魔法システムにスクロール獲得を通知
                if (MagicSystem.Instance != null)
                {
                    MagicSystem.Instance.AcquireMagicScroll(magicScrollData.MagicID);

                    // 獲得エフェクト再生
                    if (pickupEffectPrefab != null)
                    {
                        BicolorWitch.Effect.EffectManager.Instance?.PlayEffect(pickupEffectPrefab, transform.position, Quaternion.identity);
                    }

                    // 獲得SE再生
                    if (!string.IsNullOrEmpty(pickupSoundID))
                    {
                        BicolorWitch.Core.SoundManager.Instance?.PlaySE(pickupSoundID);
                    }

                    hasBeenPickedUp = true;
                    // アイテムを非アクティブ化または破棄
                    gameObject.SetActive(false); // または Destroy(gameObject);

                    // オブジェクト破棄後にResources.UnloadUnusedAssets()を呼び出す
                    BicolorWitch.Core.GManager.Instance?.CallUnloadUnusedAssets();
                }
            }
        }
    }
}
