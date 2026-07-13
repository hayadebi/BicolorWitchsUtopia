using UnityEngine;
using BicolorWitch.Player;
using BicolorWitch.Effect; // EffectManagerを参照するために追加

namespace BicolorWitch.Magic
{
    /// <summary>
    /// 魔法の弾（プロジェクタイル）の基本動作を定義するベースクラス。
    /// </summary>
    public class MagicProjectile : MonoBehaviour
    {
        [Tooltip("この魔法の弾のデータ")]
        public MagicData magicData;

        private GameObject caster;

        /// <summary>
        /// 魔法の弾を初期化する。
        /// </summary>
        /// <param name="data">使用する魔法データ。</param>
        /// <param name="owner">魔法を発動したGameObject。</param>
        public void Initialize(MagicData data, GameObject owner)
        {
            magicData = data;
            caster = owner;

            // 速度を設定
            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = transform.forward * magicData.projectileSpeed;
            }
            else
            {
                Debug.LogWarning("MagicProjectile: Rigidbodyが見つかりません。手動で速度を設定してください。", this);
            }

            // 一定時間後に自動消滅
            Destroy(gameObject, magicData.range / magicData.projectileSpeed); // 射程距離と速度から時間を計算
        }

        private void OnTriggerEnter(Collider other)
        {
            // 自身が発射したキャラクターとは衝突しない
            if (other.gameObject == caster) return;

            // TODO: 衝突したオブジェクトに対する処理を実装
            // 例: 敵にダメージを与える、壁に当たったら消滅など
            Debug.Log($"MagicProjectile: {magicData.Name} が {other.name} に衝突しました。", this);

            // ヒットエフェクトの生成
            if (magicData.hitEffectPrefab != null)
            {
                // EffectManager経由でエフェクトを再生
                Effect.EffectManager.Instance?.PlayEffect(magicData.hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // 衝突後、魔法の弾を消滅させる
            Destroy(gameObject);
        }

        // Rigidbodyがない場合の移動処理（例）
        private void Update()
        {
            if (magicData != null && !TryGetComponent<Rigidbody>(out _))
            {
                transform.Translate(Vector3.forward * magicData.projectileSpeed * Time.deltaTime);
            }
        }
    }
}
