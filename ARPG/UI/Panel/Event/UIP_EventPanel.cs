using System;
using System.Collections.Generic;
using GameplayEvent.Handler.StructureUtility;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace ARPG.UI.Panel.Event
{
	/// <summary>
	/// 通用的事件面板。
	/// </summary>
	public class UIP_EventPanel : UI_UIBasePanel
	{


		public class EventWindowToInstancePair
		{

			[NonSerialized]
			[ShowInInspector, LabelText("事件"), InlineEditor(InlineEditorObjectFieldModes.Boxed)]
			public SOConfig_PrefabEventConfig EventContainPopup;

			[NonSerialized]
			public GEH_弹出事件窗口_基本窗口_PopupEventWindowBase PopupWindowGEHRef;

			[NonSerialized]
			[ShowInInspector, LabelText("UIRW")]
			public UIRW_EventPopupWindow_事件弹出窗口UIRW PopupWindowUIRWInstance;
		}






		[ShowInInspector, LabelText("当前弹出的事件们"), NonSerialized]
		public List<EventWindowToInstancePair> CurrentActiveEvents = new List<EventWindowToInstancePair>();



		[SerializeField, Required, LabelText("prefab-整个窗口prefab"), TitleGroup("===prefab===")]
		public GameObject Prefab_EventPopupWindow;



		public override void ShowThisPanel(bool clearShow = true, bool notBroadcast = false)
		{
			base.ShowThisPanel(clearShow, notBroadcast);
			_holder_HoverPanel.gameObject.SetActive(false);
		}
		protected override void BindingEvent()
		{
			base.BindingEvent();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_GE_OnGameplayEventLaunched_当游戏性事件发起执行,
				_ABC_ProcessGameplayEventLaunch);
		}


		/// <summary>
		/// <para>当有事件触发时，检查一下这个事件是不是需要事件窗口。</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ProcessGameplayEventLaunch(DS_ActionBusArguGroup ds)
		{
			_holder_HoverPanel.gameObject.SetActive(false);


			//然后检查GEH中有没有弹出信息
			var eventRef = ds.GetObj1AsT<SOConfig_PrefabEventConfig>();
			if (eventRef.IfContainPopupWindow)
			{
				var popup = eventRef.GetEventHandler<GEH_弹出事件窗口_基本窗口_PopupEventWindowBase>();
				if (popup == null)
				{
					return;
				}

				var newObj = Instantiate(Prefab_EventPopupWindow, _rootGO.transform);
				var newUIRW = newObj.GetComponent<UIRW_EventPopupWindow_事件弹出窗口UIRW>();
				var newInfoPair = new EventWindowToInstancePair()
				{
					EventContainPopup = eventRef,
					PopupWindowGEHRef = popup,
					PopupWindowUIRWInstance = newUIRW
				};

				newUIRW.InitializeOnInstantiate(newInfoPair,
					_callback_ClearCertainWindow,
					_callback_ShowHoverDetailContent,
					_callback_HideOptionHoverDetail);

				newUIRW.transform.SetAsFirstSibling();


				CurrentActiveEvents.Add(newInfoPair);
				ShowThisPanel();

				var newPopupWindow =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.G_UI_EventPopup_OnNewEventPopupWindowShowed_事件弹出窗口显示);
				newPopupWindow.ObjectArgu1 = newUIRW;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(newPopupWindow);
			}
		}



		private void _callback_ClearCertainWindow(UIRW_EventPopupWindow_事件弹出窗口UIRW popup)
		{
			var ds_close =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_UI_EventPopup_OnNewEventPopupWindowClosed_事件弹出窗口关闭);
			ds_close.ObjectArgu1 = popup;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_close);
			var fi = CurrentActiveEvents.FindIndex(x => x.PopupWindowUIRWInstance == popup);
			if (fi != -1)
			{
				var per = CurrentActiveEvents[fi];
				Destroy(per.PopupWindowUIRWInstance.gameObject);
				CurrentActiveEvents.RemoveAt(fi);
			}
			if (CurrentActiveEvents.Count == 0)
			{
				HideThisPanel();
			}
		}





#region 悬停

		[SerializeField, LabelText("holder_悬停面板"), Required, TitleGroup("===组件===")]
		public RectTransform _holder_HoverPanel;

		[SerializeField, LabelText("text_悬停详情文本"), Required, TitleGroup("===组件===")]
		public TextMeshProUGUI _text_HoverDetailText;




		private void _callback_ShowHoverDetailContent(UIRW_SingleEventOption_事件选项于弹出窗口 optionUIRWRef, BaseEventData bed)
		{
			_text_HoverDetailText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EventOptionDesc,
					optionUIRWRef.RelatedOptionInfoRef.OptionInfo.OptionContent_LKey.Trim());
			if (_text_HoverDetailText.text.Length > 0)
			{
				_holder_HoverPanel.localPosition = GetLocalPositionOfPointerAligned(bed);
				_holder_HoverPanel.gameObject.SetActive(true);
			}
			else
			{
				_holder_HoverPanel.gameObject.SetActive(false);
			}
		}


		private void _callback_HideOptionHoverDetail()
		{
			_holder_HoverPanel.gameObject.SetActive(false);
		}

#endregion





	}
}