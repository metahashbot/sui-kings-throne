using System;
using ARPG.Equipment;
using ARPG.UI.Panel.Test;
using Global;
using Global.ActionBus;
using Global.AssetLoad;
using Global.GlobalConfig;
using Global.UI;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WorldMapScene.UI.TeamBuild;
using static Global.GlobalConfig.GlobalConfigSO;
namespace ARPG.UI.Panel.PlayerCharacter
{
	public class UIRW_PerEquipmentEntryInEquipmentInventorySubPanel : UI_UISingleRuntimeWidget
	{

		[SerializeField, Required, LabelText("Button_本体按钮")]
		private Button _button_selfButton;
		[SerializeField, Required, LabelText("Image_本体图标 边框")]
		private Image _image_selfBorderImage;
		[SerializeField, Required, LabelText("Image_内容图标 ")]
		private Image _image_ContentIcon;
		[SerializeField, Required, LabelText("sprite-空槽 ")]
		private Sprite _sprite_EmptySlot;
		[SerializeField, Required, LabelText("text  数量文本 "), TitleGroup("===Widget===")]
		private TMPro.TextMeshProUGUI _text_SelfButtonIcon_Count;
        [SerializeField, Required, LabelText("Image_临时图标 ")]
        private Image _image_TmpIcon;
        public GlobalConfigSO.PlayerEquipmentInfo _rawEquipmentInfo { get; private set; }
		public GlobalConfigSO.ItemInfo _rawIngredientInfo { get; private set; }

		private UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel, BaseEventData> _callback_PointerEnter;
		private UnityAction _callback_PointerExit;
		private UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel> _callback_click;


		public void InitializeOnInstantiate(
			GlobalConfigSO.ItemInfo itemInfo,
			UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel, BaseEventData> pointerEnter,
			UnityAction pointExit,
			UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel> click)
		{
			_rawIngredientInfo = itemInfo;
			_callback_PointerEnter = pointerEnter;
			_callback_PointerExit = pointExit;
			_callback_click = click;

			if (itemInfo == null)
			{
				_image_ContentIcon.sprite = _sprite_EmptySlot;
                _image_ContentIcon.gameObject.SetActive(false);
                _text_SelfButtonIcon_Count.gameObject.SetActive(false);
                return;
			}
			// 图标
            var itemRawInfo = GCAHHExtend.GetProperty(itemInfo.UID);
			_image_selfBorderImage.sprite = BaseUIManager.QuickGetIconBorder(itemRawInfo.Quality, true);
			_image_ContentIcon.sprite = itemRawInfo.IconSprite;
            _image_ContentIcon.gameObject.SetActive(true);
			// 数量
            _text_SelfButtonIcon_Count.text = itemInfo.Count.ToString();
            _text_SelfButtonIcon_Count.gameObject.SetActive(true);
        }

        public void InitializeOnInstantiate(
			PlayerEquipmentInfo equipInfo,
            UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel, BaseEventData> pointerEnter,
            UnityAction pointExit,
            UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel> click)
        {
            _rawEquipmentInfo = equipInfo;
            _callback_PointerEnter = pointerEnter;
            _callback_PointerExit = pointExit;
            _callback_click = click;

			if (equipInfo == null)
			{
				_image_ContentIcon.gameObject.SetActive(false);
				_image_ContentIcon.sprite = _sprite_EmptySlot;
                _text_SelfButtonIcon_Count.gameObject.SetActive(false);
                return;
            }
            // 图标
            var equipmentRawInfo = GCAHHExtend.GetEquipmentRawInfo(equipInfo.EquipmentUID);
            _image_selfBorderImage.sprite = BaseUIManager.QuickGetIconBorder(equipmentRawInfo.QualityType, true);
            _image_ContentIcon.sprite = equipmentRawInfo.IconSprite;
            _image_ContentIcon.gameObject.SetActive(true);
            // 数量
            _text_SelfButtonIcon_Count.gameObject.SetActive(false);
			// 事件
            _button_selfButton.onClick.AddListener(_Button_ClickToChangeThisToThisEquipment);
		}

		public void  SetTmpIcon(bool value)
		{
			_image_TmpIcon.gameObject.SetActive(value);
		}

        public void InitializeEmpty()
        {
            _rawIngredientInfo = null;
            _rawEquipmentInfo = null;
            _callback_PointerEnter = null;
            _callback_PointerExit = null;
            _callback_click = null;

            _image_ContentIcon.sprite = _sprite_EmptySlot;
            _image_ContentIcon.gameObject.SetActive(false);
            _text_SelfButtonIcon_Count.gameObject.SetActive(false);
        }
        public void _ET_OnPointerEnter(BaseEventData ed)
		{
			_callback_PointerEnter?.Invoke(this, ed);
			// _playerCharacterFullPanelRef.OnPointerEnterNewEquipment_InsideEquipmentInventory(this, ed);
		}
		public void _ET_OnPointerExit(BaseEventData ed)
		{
			_callback_PointerExit?.Invoke();

			// _playerCharacterFullPanelRef.OnPointerExitNewEquipment_InsideEquipmentInventory(this);
		}


		public void _Button_ClickToChangeThisToThisEquipment()
		{
		
		}


		public void _ET_RightClick(BaseEventData baseEventData)
		{
			if ((baseEventData as PointerEventData).button == PointerEventData.InputButton.Right)
			{
				//不能在正常ARPG场景中更换装备
				if (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase() == GeneralGameAssetLoadPhaseEnum.ARPG)
				{
					return;
				}

				//如果显示的不是装备，就无事发生
				if(_rawEquipmentInfo == null)
				{
					return;
				}






				var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
				ds.ObjectArgu1 = transform;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds);

				_callback_click?.Invoke(this);


				// _playerCharacterFullPanelRef.ChangeToThisEquipment(this);
			}
		}
	}
}