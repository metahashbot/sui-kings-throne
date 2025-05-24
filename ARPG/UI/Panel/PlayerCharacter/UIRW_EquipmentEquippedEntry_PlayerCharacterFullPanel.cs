using ARPG.Equipment;
using Global;
using Global.GlobalConfig;
using Global.UI;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ARPG.UI.Panel.PlayerCharacter
{
	[TypeInfoBox(" 用于玩家角色详情面板上的，当前装备栏。并不是运行时生成的而是预设就已配置的。\n" + "")]
	public class UIRW_EquipmentEquippedEntry_PlayerCharacterFullPanel : UI_UISingleRuntimeWidget
	{
		[SerializeField, Required, LabelText("本体按钮"), TitleGroup("===Widget===")]
		private Button _button_selfButton;


		[LabelText("image-本体边框"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_selfBorder;

		[LabelText("image-本体图标"), SerializeField, Required, TitleGroup("===Widget===")]
		private Image _image_selfIcon;



		[LabelText("关联的装备位置"), SerializeField, TitleGroup("===Widget===")]
		public EquipmentCharacterSlotOnTypeEnum SlotOnType;
		private static UIP_PlayerCharacterFullPanel _playerCharacterFullPanelRef;

		public GlobalConfigSO.PlayerEquipmentInfo RelatedEquipmentInfoRef { get; private set; }


		private UnityAction<GlobalConfigSO.PlayerEquipmentInfo> _callback_click;

		private UnityAction<GlobalConfigSO.PlayerEquipmentInfo, BaseEventData> _callback_HoverEnter;

		private UnityAction _callback_hoverExit;




		public void InstantiateInitialize(
			UIP_PlayerCharacterFullPanel pcf,
			UnityAction<GlobalConfigSO.PlayerEquipmentInfo> callbackClick,
			UnityAction<GlobalConfigSO.PlayerEquipmentInfo, BaseEventData> callbackHoverEnter,
			UnityAction callbackHoverExit)
		{
			_playerCharacterFullPanelRef = pcf;
			_callback_click = callbackClick;
			_callback_HoverEnter = callbackHoverEnter;
			_callback_hoverExit = callbackHoverExit;
			_button_selfButton.onClick.AddListener(_button_ClickSelfButton);
		}

		/// <summary>
		/// 自行刷新。其中UIP-PCF会在已装备的装备发生变动时调用这个，然后自己查自己刷
		/// </summary>
		public void RefreshRelatedEquipmentInfo()
		{
			RelatedEquipmentInfoRef = null;
			var currentIndex = _playerCharacterFullPanelRef.CurrentSelectCharacterID;
			var getEquipmentInfoIndex = GlobalConfigSO.RuntimeContent().CurrentEquipmentInfoList
					.FindIndex((info => info.EquippedWithCharacter == currentIndex && 
					info.EquippedOnSlot == SlotOnType));
			if (getEquipmentInfoIndex == -1)
			{
				_image_selfIcon.gameObject.SetActive(false);
				_button_selfButton.interactable = false;
				_image_selfBorder.sprite = BaseUIManager.QuickGetIconBorder("白", false);
				return;
			}
			RelatedEquipmentInfoRef = GlobalConfigSO.RuntimeContent().
				CurrentEquipmentInfoList[getEquipmentInfoIndex];
            var info = GCAHHExtend.GetEquipmentRawInfo(RelatedEquipmentInfoRef.EquipmentUID);
            _image_selfIcon.gameObject.SetActive(true);
            _image_selfIcon.sprite = info.IconSprite;
            _image_selfBorder.sprite = BaseUIManager.QuickGetIconBorder(info.QualityType, false);
            _button_selfButton.interactable = true;
        }


		private void _button_ClickSelfButton()
		{
		
		}

		public void _ET_ShowEquipmentHover_OnPointerEnter(BaseEventData ed)
		{
			if (RelatedEquipmentInfoRef == null)
			{
				return;
			}
			_callback_HoverEnter.Invoke(RelatedEquipmentInfoRef, ed);
		}

		public void _ET_HideEquipmentHover_OnPointerExit(BaseEventData ed)
		{
			_callback_hoverExit.Invoke();
		}


		public void _ET_RightClickSelfButton(BaseEventData ed)
		{
			if ((ed as PointerEventData).button == PointerEventData.InputButton.Right)
			{
				if (RelatedEquipmentInfoRef == null)
				{
					return;
				}
				_callback_click.Invoke(RelatedEquipmentInfoRef);
			}
		}
	}
}