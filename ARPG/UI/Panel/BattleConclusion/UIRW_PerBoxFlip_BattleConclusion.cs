using Global;
using Global.ActionBus;
using Global.GlobalConfig;
using Global.Loot;
using Global.UI;
using Global.UIBase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace ARPG.UI.Panel.BattleConclusion
{
	public class UIRW_PerBoxFlip_BattleConclusion : UI_UISingleRuntimeWidget
	{
		[LabelText("button - 本体按钮"), SerializeField, Required, TitleGroup("===Widget===")]
		private Button _button_SelfButton;

		[LabelText("image - 本体图片"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_SelfImage;

		[LabelText("image_奖励边框 "), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_RewardBorder;

		[LabelText("image_奖励图标"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_RewardIcon;

		[LabelText("text_奖励数量"), SerializeField, Required, TitleGroup("===Widget===")]
		private TextMeshProUGUI _text_RewardAmount;

		
		[LabelText("sprite-闭合状态"), SerializeField, Required, TitleGroup("===Asset===")]
		private Sprite _sprite_Close;
		 
		 [LabelText("sprite-选择状态"), SerializeField, Required, TitleGroup("===Asset===")]
		 private Sprite _sprite_Select;



		 public bool SelfChestOpened { get; private set; }



		private UnityAction<UIRW_PerBoxFlip_BattleConclusion> _callback_clickSelfOnSelect;

		public int RelatedIngredientID { get; private set; }
		public int RelatedIngredientAmount { get; private set; }

		
		SOFE_PropertyInfo.PerTypeInfo _relatedIngredientInfo;


		public void InstantiateInitialize(int ingredientID, int amount,
			UnityAction<UIRW_PerBoxFlip_BattleConclusion> callbackOfSelect)
		{
			_button_SelfButton.interactable = true;
			_callback_clickSelfOnSelect = callbackOfSelect;
			_image_RewardBorder.gameObject.SetActive(false);
			_image_RewardIcon.gameObject.SetActive(false);
			_text_RewardAmount.gameObject.SetActive(false);

			RelatedIngredientID = ingredientID;
			RelatedIngredientAmount = amount;
			
			
			_image_SelfImage.sprite = _sprite_Close;

			_button_SelfButton.onClick.AddListener(_Button_ClickSelf);


			_relatedIngredientInfo = GCAHHExtend.GetProperty(ingredientID);
			
			
			

		}

		public void DisableButton()
		{
			_button_SelfButton.interactable = false;
		}


		private void _Button_ClickSelf()
		{
			_button_SelfButton.interactable = false;
			
			_callback_clickSelfOnSelect.Invoke(this);
			
			_image_RewardBorder.gameObject.SetActive(true);

			_image_RewardBorder.sprite =
				BaseUIManager.QuickGetIconBorder(_relatedIngredientInfo.Quality, false);

			_image_RewardIcon.gameObject.SetActive(true);
			_image_RewardIcon.sprite = _relatedIngredientInfo.IconSprite;
			
			_text_RewardAmount.gameObject.SetActive(true);
			_text_RewardAmount.text = RelatedIngredientAmount.ToString();
			
			
			
			//向背包中添加物品
			GlobalConfigSO.ContentInGCSO gcsoContent = GlobalConfigSO.RuntimeContent();
			//gcsoContent.AddIngredient(RelatedIngredientID, RelatedIngredientAmount);
			
			
			
			
			var ds_vfx =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
			ds_vfx.ObjectArgu1 = transform;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_vfx);
			
			



		}
		
		

	}
}