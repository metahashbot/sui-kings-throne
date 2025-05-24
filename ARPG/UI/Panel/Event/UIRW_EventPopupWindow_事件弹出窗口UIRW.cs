using System;
using System.Collections.Generic;
using GameplayEvent;
using GameplayEvent.Handler.StructureUtility;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using Global.UIBase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WorldMapScene.Character;
namespace ARPG.UI.Panel.Event
{
	public class UIRW_EventPopupWindow_事件弹出窗口UIRW : UI_UISingleRuntimeWidget 
	{

		[SerializeField, Required, LabelText("button_关闭按钮"), TitleGroup("===组件===")]
		public Button _button_CloseWindow;

		[SerializeField, LabelText("text_标题"), TitleGroup("===组件===")]
		public TextMeshProUGUI _text_TitleText;
		
		[SerializeField,LabelText("holder_插图Holder") ,TitleGroup("===组件===")] 
		public RectTransform _holder_IllustrationHolder;

		[SerializeField, LabelText("image_插图"), TitleGroup("===组件===")]
		public Image _image_IllustrationImage;
		
		[SerializeField,LabelText("text_内容"),TitleGroup("===组件===")]
		public TextMeshProUGUI _text_ContentText;
		

		[SerializeField, LabelText("layout_选项布局"), TitleGroup("===组件===")]
		public LayoutGroup _layout_OptionLayout;

		[SerializeField, LabelText("prefab_选项prefab"), TitleGroup("===prefab===")]
		public GameObject _prefab_OptionPrefab;

		public SOConfig_PrefabEventConfig RelatedEventConfigInstance { get; private set; }
		
		public GEH_弹出事件窗口_基本窗口_PopupEventWindowBase RelatedEventWindowGEHRef { get; private set; }
		
		public UIP_EventPanel.EventWindowToInstancePair RelatedEventWindowPairInstance { get; private set; }

		public class PerOptionContentPair
		{
			public UIRW_EventPopupWindow_事件弹出窗口UIRW ParentWindowRef;
			[ShowInInspector]
			public UIRW_SingleEventOption_事件选项于弹出窗口 OptionUIRW;
			[ShowInInspector]
			public OptionInPopupWindow OptionInfo;
		
			 
		}
		
		[ShowInInspector,NonSerialized,LabelText( "当前的选项们"),TitleGroup("===运行时===")]
		public List<PerOptionContentPair> CurrentOptionList = new List<PerOptionContentPair>();
		
		private UnityAction<UIRW_EventPopupWindow_事件弹出窗口UIRW> _closeWindowCallback;


		private UnityAction<UIRW_SingleEventOption_事件选项于弹出窗口, BaseEventData> _c_OnPointerEnterOption;
		private UnityAction _c_OnPointerExitOption;

		public void InitializeOnInstantiate(UIP_EventPanel.EventWindowToInstancePair infoRef ,
			UnityAction<UIRW_EventPopupWindow_事件弹出窗口UIRW> closeCallback ,
			UnityAction<UIRW_SingleEventOption_事件选项于弹出窗口, BaseEventData> callback_OnHover,
			 UnityAction callback_onExit)
		{

			RelatedEventWindowPairInstance = infoRef;
			RelatedEventConfigInstance = infoRef.EventContainPopup;
			RelatedEventWindowGEHRef = infoRef.PopupWindowGEHRef;
			_closeWindowCallback = closeCallback;
			_c_OnPointerEnterOption = callback_OnHover;
			_c_OnPointerExitOption = callback_onExit;


			if (RelatedEventWindowGEHRef.HideCloseButton)
			{
				_button_CloseWindow.gameObject.SetActive(false);
			}
			else
			{
				_button_CloseWindow.gameObject.SetActive(true);
			}
			
			//加载图片
			if (RelatedEventWindowGEHRef.HasIllustration)
			{
				if (!RelatedEventWindowGEHRef.OP.IsValid())
				{
					if (RelatedEventWindowGEHRef.Address_配图地址 != null &&
					    RelatedEventWindowGEHRef.Address_配图地址.RuntimeKeyIsValid())
					{
						RelatedEventWindowGEHRef.OP =
							Addressables.LoadAssetAsync<Sprite>(RelatedEventWindowGEHRef.Address_配图地址);
						_image_IllustrationImage.sprite = RelatedEventWindowGEHRef.OP.WaitForCompletion();
						_holder_IllustrationHolder.gameObject.SetActive(true);
					}
				}
				else
				{
					_holder_IllustrationHolder.gameObject.SetActive(true);
					_image_IllustrationImage.sprite = RelatedEventWindowGEHRef.OP.Result;
				}
			}
			else
			{
				_holder_IllustrationHolder.gameObject.SetActive(false);
			}
			//设置文本
			_text_TitleText.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EventTitle,
					RelatedEventWindowGEHRef.EventTitle_LKey.Trim());
			
			var baseContentText = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_EventContent,
					RelatedEventWindowGEHRef.EventContent_LKey.Trim());
			//进行文本内容的处理
			ProcessEventContentText(ref baseContentText);
			_text_ContentText.text = baseContentText;
			
			
			
			//读读选项
			foreach (OptionInPopupWindow perOption in RelatedEventWindowGEHRef.OptionList)
			{
				PerOptionContentPair newContentPair = new PerOptionContentPair();
				CurrentOptionList.Add(newContentPair);
				newContentPair.ParentWindowRef = this;
				newContentPair.OptionInfo = perOption;
				GameObject newOptionGO = Instantiate(_prefab_OptionPrefab, _layout_OptionLayout.transform);
				UIRW_SingleEventOption_事件选项于弹出窗口 newUIRW =
					newContentPair.OptionUIRW = newOptionGO.GetComponent<UIRW_SingleEventOption_事件选项于弹出窗口>();
				//选项点了啥效果，由各个选项自己处理
				newUIRW.InitializeOnInstantiate(newContentPair,
					_c_OnPointerEnterOption,
					_c_OnPointerExitOption,
					_callback_ProcessOptionFunction); 
			}
			
			
		}


		/// <summary>
		/// 关闭整个事件窗口前，进行清理
		/// </summary>
		private void ClearEventPopupWindow()
		{
			
		
		}


		private void ProcessEventContentText(ref string text)
		{
			
		}




		private void _callback_ProcessOptionFunction(UIRW_SingleEventOption_事件选项于弹出窗口 option)
		{
			foreach (BaseEventWindowOptionFunction perOption in option.RelatedOptionInfoRef.OptionInfo
				.GetOptionFunctionList_Runtime())
			{
				_ProcessEWF(perOption);
			}
		}


		private void _ProcessEWF(BaseEventWindowOptionFunction ewf)
		{
			switch (ewf)
			{
				case EWF_触发事件_TriggerEvent ewf触发事件TriggerEvent:
					foreach (var prefabEvent in ewf触发事件TriggerEvent.TriggerEventList)
					{
						GameplayEventManager.Instance.StartGameplayEvent(prefabEvent);
					}
					break;
				case EWF_基本功能_关闭窗口_CloseWindow ewf基本功能关闭窗口CloseWindow:
					//关闭窗口不能再次关闭窗口！
					//处理关闭前业务
					
					foreach (BaseEventWindowOptionFunction dd in RelatedEventWindowGEHRef
						.OptionFunctionListOnCloseWindow)
					{
						if (dd is EWF_基本功能_关闭窗口_CloseWindow)
						{
							continue;
						}
						_ProcessEWF(dd);
						
					}


					ClearEventPopupWindow();
					CurrentOptionList.Clear();


					//Destroy自己是在这个回调里面进行的，这个回调来自整个事件大面板
					_closeWindowCallback.Invoke(this);


					break;
				case EWF_释放交互阻塞_ReleaseInteractionBlock :
					 GlobalConfigurationAssetHolderHelper.GetCurrentBaseGLM().CurrentPlayerCharacterInteractionBlockProcessorRef.ReleaseCurrentTriggerAreaRefBlock();
					 break;

				case EWF_恢复战斗_ResumeeBattle:
                    var ds_pause = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequireResumeGame_要求解除暂停);
                    ds_pause.ObjectArgu1 = this;
                    GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);
                    break;
			}
		}

	}
}