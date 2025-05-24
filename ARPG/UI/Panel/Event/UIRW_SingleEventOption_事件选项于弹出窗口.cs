using System;
using System.Collections.Generic;
using GameplayEvent;
using GameplayEvent.Handler.StructureUtility;
using Global;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace ARPG.UI.Panel.Event
{
	public class UIRW_SingleEventOption_事件选项于弹出窗口 : UI_UISingleRuntimeWidget
	{

		[SerializeField, LabelText("button_本体按钮"), TitleGroup("===组件===")]
		public UnityEngine.UI.Button _button_OptionButton;

		[SerializeField, LabelText("_text_选项文本"), TitleGroup("===组件===")]
		public TMPro.TextMeshProUGUI _text_OptionText;

		[SerializeField, LabelText("layout_选项图标布局"), TitleGroup("===组件===")]
		public UnityEngine.UI.LayoutGroup _layout_OptionIconLayout;

		[SerializeField, LabelText("prefab_选项图标prefab"), TitleGroup("===prefab===")]
		public GameObject _prefab_OptionIconPrefab;


		[ShowInInspector]
		public List<UIRW_DescOptionIcon_EventOption_事件选项上的额外图标> DescIconUIRWList =
			new List<UIRW_DescOptionIcon_EventOption_事件选项上的额外图标>();

		public UIRW_DescOptionIcon_EventOption_事件选项上的额外图标 ParentEventPopupWindowRef { get; private set; }


		public UIRW_EventPopupWindow_事件弹出窗口UIRW.PerOptionContentPair RelatedOptionInfoRef { get; private set; }

		private UnityAction<UIRW_SingleEventOption_事件选项于弹出窗口, BaseEventData> _c_OnPointerEnterOption;

		private UnityAction _c_OnPointerExitOption;

		private UnityAction<UIRW_SingleEventOption_事件选项于弹出窗口> _c_click;


		public void InitializeOnInstantiate(
			UIRW_EventPopupWindow_事件弹出窗口UIRW.PerOptionContentPair infoRef,
			UnityAction<UIRW_SingleEventOption_事件选项于弹出窗口, BaseEventData> callback_OnHover,
			UnityAction _callback_onExit,
			UnityAction<UIRW_SingleEventOption_事件选项于弹出窗口> callback_onClick)
		{
			RelatedOptionInfoRef = infoRef;
			_c_OnPointerEnterOption = callback_OnHover;
			_c_OnPointerExitOption = _callback_onExit;
			_c_click = callback_onClick;

			//处理文本
			_text_OptionText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EventOption,
					infoRef.OptionInfo.OptionContent_LKey.Trim());


			//处理选项条件
			if (RelatedOptionInfoRef.OptionInfo.GetOptionEWFByT(out EWF_选项有条件可用_OptionHasRequirement requirement))
			{
				if (requirement.GetIfOptionValid())
				{
					//显示自己
					_button_OptionButton.gameObject.SetActive(true);
					_button_OptionButton.interactable = true;
				}
				else
				{
					if (requirement.HideWhenNotValid)
					{
						//隐藏自己
						_button_OptionButton.gameObject.SetActive(false);
					}
					else
					{
						//显示自己
						_button_OptionButton.gameObject.SetActive(true);
						_button_OptionButton.interactable = false;
					}
				}
			}



			//处理点击效果
			_button_OptionButton.onClick.RemoveAllListeners();
			_button_OptionButton.onClick.AddListener(_Button_OnClickOptionSelfButton);

			//检查有没有额外图标
		}


		private void _Button_OnClickOptionSelfButton()
		{
			_c_click?.Invoke(this);
		
		}


		public void ClearBeforeDestroy()
		{
			_button_OptionButton.onClick.RemoveAllListeners();
			foreach (UIRW_DescOptionIcon_EventOption_事件选项上的额外图标 perUIRW in DescIconUIRWList)
			{
				Destroy(perUIRW.gameObject);
			}
			DescIconUIRWList.Clear();
		}

		public void _ET_OnPointerEnter(BaseEventData bed)
		{
			if (RelatedOptionInfoRef.OptionInfo.HasHover)
			{
				_c_OnPointerEnterOption.Invoke(this, bed);
			}
		}
		public void _ET_OnPointerExit( BaseEventData bed)
		{
			if (RelatedOptionInfoRef.OptionInfo.HasHover)
			{
				_c_OnPointerExitOption.Invoke();
			}
		}

	}
}