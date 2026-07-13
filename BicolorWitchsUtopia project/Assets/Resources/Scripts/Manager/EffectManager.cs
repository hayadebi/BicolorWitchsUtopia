using UnityEngine;
using System.Collections.Generic;

namespace BicolorWitch.Effect
{
    /// <summary>
    /// ゲーム内のエフェクトを管理するシングルトンクラス。
    /// エフェクトの生成、再生、破棄を一元的に行います。
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }

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
        /// 指定されたエフェクトプレハブを再生します。
        /// </summary>
        /// <param name="effectPrefab">再生するエフェクトのプレハブ。</param>
        /// <param name="position">エフェクトを再生する位置。</param>
        /// <param name="rotation">エフェクトの回転。</param>
        /// <param name="duration">エフェクトの自動破棄までの時間。0以下の場合はプレハブのParticleSystemのDurationに依存。</param>
        /// <returns>生成されたエフェクトのGameObject。</returns>
        public GameObject PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, float duration = 0f)
        {
            if (effectPrefab == null)
            {
                Debug.LogWarning("EffectManager: 再生するエフェクトプレハブが指定されていません。", this);
                return null;
            }

            GameObject effectInstance = Instantiate(effectPrefab, position, rotation);

            // durationが指定されている場合は、その時間後に破棄
            if (duration > 0f)
            {
                Destroy(effectInstance, duration);
            }
            else
            {
                // ParticleSystemのDurationに基づいて自動破棄
                ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(effectInstance, ps.main.duration);
                }
                else
                {
                    // ParticleSystemがない場合は、デフォルトで5秒後に破棄
                    Destroy(effectInstance, 5f);
                }
            }

            return effectInstance;
        }

        // TODO: オブジェクトプーリングの実装を検討
    }
}
