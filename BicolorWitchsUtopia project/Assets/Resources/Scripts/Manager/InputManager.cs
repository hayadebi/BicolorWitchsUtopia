using UnityEngine;
using System;

namespace BicolorWitch.InputManagement
{
    /// <summary>
    /// ゲームの入力を一元管理するシングルトンクラス。
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        // イベント定義
        public event Action<float> OnMoveInput;
        public event Action OnJumpInput;
        public event Action OnBasicAttackInput;
        public event Action OnSwitchCharacterInput;
        public event Action OnMagicInput1;
        public event Action OnMagicInput2;
        public event Action OnDashInputStart;
        public event Action OnDashInputEnd;
        public event Action OnPauseInput;

        // プロパティ定義
        public float MoveInput { get; private set; }
        public bool IsJumpPressed { get; private set; }
        public bool IsBasicAttackPressed { get; private set; }
        public bool IsSwitchCharacterPressed { get; private set; }
        public bool IsMagic1Pressed { get; private set; }
        public bool IsMagic2Pressed { get; private set; }
        public bool IsDashPressed { get; private set; }
        public bool IsPausePressed { get; private set; }

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

        private void Update()
        {
            // 移動入力
            float move = Input.GetAxisRaw("Horizontal");
            if (move != MoveInput)
            {
                MoveInput = move;
                OnMoveInput?.Invoke(MoveInput);
            }

            // ジャンプ入力
            if (Input.GetButtonDown("Jump"))
            {
                IsJumpPressed = true;
                OnJumpInput?.Invoke();
            }
            else if (Input.GetButtonUp("Jump"))
            {
                IsJumpPressed = false;
            }

            // 基本攻撃入力
            if (Input.GetButtonDown("Fire1"))
            {
                IsBasicAttackPressed = true;
                OnBasicAttackInput?.Invoke();
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                IsBasicAttackPressed = false;
            }

            // キャラクター切り替え入力
            if (Input.GetButtonDown("Fire2"))
            {
                IsSwitchCharacterPressed = true;
                OnSwitchCharacterInput?.Invoke();
            }
            else if (Input.GetButtonUp("Fire2"))
            {
                IsSwitchCharacterPressed = false;
            }

            // 魔法1入力
            if (Input.GetButtonDown("Fire3"))
            {
                IsMagic1Pressed = true;
                OnMagicInput1?.Invoke();
            }
            else if (Input.GetButtonUp("Fire3"))
            {
                IsMagic1Pressed = false;
            }

            // 魔法2入力
            if (Input.GetButtonDown("Fire4"))
            {
                IsMagic2Pressed = true;
                OnMagicInput2?.Invoke();
            }
            else if (Input.GetButtonUp("Fire4"))
            {
                IsMagic2Pressed = false;
            }

            // ダッシュ入力
            if (Input.GetButtonDown("Dash"))
            {
                IsDashPressed = true;
                OnDashInputStart?.Invoke();
            }
            else if (Input.GetButtonUp("Dash"))
            {
                IsDashPressed = false;
                OnDashInputEnd?.Invoke();
            }

            // ポーズ入力
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                IsPausePressed = true;
                OnPauseInput?.Invoke();
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                IsPausePressed = false;
            }
        }
    }
}
