using UnityEngine;
using UnityEngine.UI;
using BicolorWitch.Magic;

namespace BicolorWitch.UI
{
    /// <summary>
    /// 魔法選択リストの各アイテムを管理するクラス。
    /// </summary>
    public class MagicListItem : MonoBehaviour
    {
        [SerializeField] private Text magicNameText;
        [SerializeField] private Image magicIconImage;
        [SerializeField] private Button equipSlot1Button;
        [SerializeField] private Button equipSlot2Button;

        private MagicScroll assignedMagicScroll;

        public void Setup(MagicScroll magicScroll)
        {
            assignedMagicScroll = magicScroll;

            if (magicNameText != null) magicNameText.text = magicScroll.MagicName;
            if (magicIconImage != null) magicIconImage.sprite = magicScroll.Icon;

            equipSlot1Button?.onClick.RemoveAllListeners();
            equipSlot1Button?.onClick.AddListener(() => OnEquipButtonClicked(0));

            equipSlot2Button?.onClick.RemoveAllListeners();
            equipSlot2Button?.onClick.AddListener(() => OnEquipButtonClicked(1));
        }

        private void OnEquipButtonClicked(int slotIndex)
        {
            if (assignedMagicScroll != null && MagicSystem.Instance != null)
            {
                MagicSystem.Instance.EquipMagic(assignedMagicScroll.MagicID, slotIndex);
                // 装備後、魔法選択UIを非表示にするか、更新するかはUIManagerで制御
                UIManager.Instance?.HideMagicSelectionUI();
            }
        }
    }
}
