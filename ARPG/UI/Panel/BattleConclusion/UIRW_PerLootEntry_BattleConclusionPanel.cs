using Global.Loot;
using Global.UIBase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
namespace ARPG.UI.Panel.BattleConclusion
{
	public class UIRW_PerLootEntry_BattleConclusionPanel : UI_UISingleRuntimeWidget
	{
		[LabelText("image_本体图片底框"), SerializeField, TitleGroup("===Widget==="), Required]	
		private UnityEngine.UI.Image image_selfBorderImage;
		[LabelText("image_图标Icon"), SerializeField, TitleGroup("===Widget==="), Required]
		private UnityEngine.UI.Image image_iconImage;
		[LabelText("text_数量文本 - 数量"), SerializeField, TitleGroup("===Widget==="), Required]
		private TextMeshProUGUI text_amountText;

		public void InstantiateInitialize(Sprite iconSprite, Sprite borderSprite, int count)
		{
			image_selfBorderImage.sprite = borderSprite;
			image_iconImage.sprite = iconSprite;
			text_amountText.text = count.ToString();
		}

        public void InstantiateInitialize(Sprite iconSprite, Sprite borderSprite)
        {
            image_selfBorderImage.sprite = borderSprite;
            image_iconImage.sprite = iconSprite;
            text_amountText.gameObject.SetActive(false);
        }
    }
}