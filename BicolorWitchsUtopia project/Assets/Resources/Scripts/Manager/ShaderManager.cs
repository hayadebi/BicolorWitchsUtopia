using UnityEngine;
using System.Collections.Generic;
using BicolorWitch.Player;
using BicolorWitch.Core; // GManagerとCooldownManagerを参照するために追加

namespace BicolorWitch.Manager
{
    /// <summary>
    /// シェーダー関連の処理を管理するクラス。
    /// シングルトンパターンを適用し、ゲーム全体で唯一のインスタンスであることを保証する。
    /// </summary>
    public class ShaderManager : MonoBehaviour
    {
        public static ShaderManager Instance { get; private set; }

        [Header("シェーダー設定")]
        [SerializeField, Tooltip("キャラクターのRendererコンポーネント")]
        private Renderer sisterTRenderer;
        [SerializeField, Tooltip("キャラクターのRendererコンポーネント")]
        private Renderer sisterDRenderer;

        [SerializeField, Tooltip("ダメージを受けた際に適用するシェーダー")]
        private Shader damageShader;
        [SerializeField, Tooltip("通常のシェーダー")]
        private Shader normalShader;

        [Header("色覚異常シミュレーション設定")]
        [SerializeField, Tooltip("色覚異常シミュレーション用のシェーダー")]
        private Shader colorBlindnessShader;
        [SerializeField, Tooltip("色覚異常タイプ")]
        private ColorBlindnessType colorBlindnessType = ColorBlindnessType.Protanopia;

        public enum ColorBlindnessType
        {
            Normal,
            Protanopia, // P型
            Deuteranopia, // D型
            Tritanopia // T型
        }

        private Dictionary<CharacterType, Renderer> characterRenderers;
        private Dictionary<CharacterType, Material> originalMaterials;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            characterRenderers = new Dictionary<CharacterType, Renderer>
            {
                { CharacterType.SisterT, sisterTRenderer },
                { CharacterType.SisterD, sisterDRenderer }
            };

            originalMaterials = new Dictionary<CharacterType, Material>();
            foreach (var entry in characterRenderers)
            {
                if (entry.Value != null)
                {
                    originalMaterials[entry.Key] = entry.Value.material;
                }
            }

            // GManagerのGameState変更イベントを購読
            if (GManager.Instance != null)
            {
                GManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        /// <summary>
        /// 指定されたキャラクターにダメージシェーダーを適用する。
        /// </summary>
        /// <param name="characterType">対象キャラクターの種類。</param>
        public void ApplyDamageShader(CharacterType characterType)
        {
            if (characterRenderers.TryGetValue(characterType, out Renderer renderer) && renderer != null && damageShader != null)
            {
                renderer.material.shader = damageShader;
            }
        }

        /// <summary>
        /// 指定されたキャラクターに通常のシェーダーを適用する。
        /// </summary>
        /// <param name="characterType">対象キャラクターの種類。</param>
        public void ApplyNormalShader(CharacterType characterType)
        {
            if (characterRenderers.TryGetValue(characterType, out Renderer renderer) && renderer != null && originalMaterials.TryGetValue(characterType, out Material originalMaterial))
            {
                renderer.material = originalMaterial;
            }
        }

        /// <summary>
        /// 全てのキャラクターに通常のシェーダーを適用する。
        /// </summary>
        public void ApplyNormalShaderToAllCharacters()
        {
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                ApplyNormalShader(type);
            }
        }

        /// <summary>
        /// 指定されたキャラクターの色覚特性をシーンに適用する。
        /// </summary>
        /// <param name="activeCharacter">アクティブなキャラクターの種類。</param>
        public void ApplyColorPerception(CharacterType activeCharacter)
        {
            // 全てのRendererを持つオブジェクトに色覚異常シミュレーションを適用
            ApplyColorBlindnessSimulation(activeCharacter == CharacterType.SisterD);

            // アクティブなキャラクター（SisterTまたはSisterD）には通常のシェーダーを適用
            // SisterDがアクティブな場合でも、SisterD自身は通常色で表示されるようにする
            if (characterRenderers.TryGetValue(activeCharacter, out Renderer activeRenderer) && activeRenderer != null)
            {
                if (originalMaterials.TryGetValue(activeCharacter, out Material originalMaterial))
                {
                    activeRenderer.material = originalMaterial;
                }
            }

            // UIの更新（色覚異常タイプ表示）
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.UpdateColorBlindnessUI(activeCharacter == CharacterType.SisterD ? colorBlindnessType : ColorBlindnessType.Normal);
            }
        }

        /// <summary>
        /// シーン内の全てのRendererを持つオブジェクトに色覚異常シミュレーションを適用する。
        /// UI要素は対象外。
        /// </summary>
        /// <param name="apply">適用する場合はtrue、解除する場合はfalse。</param>
        private void ApplyColorBlindnessSimulation(bool apply)
        {
            // シーン内の全てのRendererを持つオブジェクトを取得
            Renderer[] allRenderers = FindObjectsOfType<Renderer>();

            foreach (Renderer renderer in allRenderers)
            {
                // UI要素はスキップ
                if (renderer.gameObject.layer == LayerMask.NameToLayer("UI")) continue;

                // キャラクター自身のRendererはスキップ（ApplyColorPerceptionで個別に処理されるため）
                if (characterRenderers.ContainsValue(renderer)) continue;

                if (apply && colorBlindnessShader != null)
                {
                    renderer.material.shader = colorBlindnessShader;
                    // シェーダーに色覚異常タイプを渡す
                    renderer.material.SetInt("_ColorBlindnessType", (int)colorBlindnessType);
                }
                else
                {
                    // 元のマテリアルに戻す
                    // ここでは元のマテリアルをDictionaryで管理していないため、単純にnormalShaderに戻す
                    // より厳密には、各オブジェクトの元のマテリアルを保持する必要がある
                    renderer.material.shader = normalShader;
                }
            }
        }

        private void OnDestroy()
        {
            if (GManager.Instance != null)
            {
                GManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GManager.GameState newGameState)
        {
            // GameStateがPlaying以外になったら、色覚異常シミュレーションを解除
            if (newGameState != GManager.GameState.Playing)
            {
                ApplyColorBlindnessSimulation(false);
                if (UI.UIManager.Instance != null)
                {
                    UI.UIManager.Instance.UpdateColorBlindnessUI(ColorBlindnessType.Normal);
                }
            }
        }
    }
}
