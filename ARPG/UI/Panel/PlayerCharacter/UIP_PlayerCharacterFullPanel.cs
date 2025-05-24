using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Config;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using Global.AssetLoad;
using Global.Character;
using Global.GlobalConfig;
using Global.RuntimeRecord;
using Global.UI;
using Global.UIBase;
using RPGCore.DataEntry;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;
using WorldMapScene;
using WorldMapScene.Manager;
namespace ARPG.UI.Panel.PlayerCharacter
{
	[Serializable]
	[TypeInfoBox("UIP：玩家详情角色面板")]
	public class UIP_PlayerCharacterFullPanel : UI_UIBasePanel
	{


		[SerializeField, LabelText("button_返回"), TitleGroup("配置/按钮")]
		private Button _button_backButton;




		[SerializeField, LabelText("Image_角色立绘"), TitleGroup("配置/左侧基本信息")]
		public Image _image_CharacterPortrait;

		[SerializeField, LabelText("Text_称号名称"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_CharacterTitleName;

		[SerializeField, LabelText("Text_称号等级"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_CharacterTitleLevel;


		[SerializeField, LabelText("Text_角色名字"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_CharacterName;
		[SerializeField, LabelText("Text_职业名称"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_CharacterJobName;

		[SerializeField, LabelText("Slider_HP滑条"), TitleGroup("配置/左侧基本信息")]
		public Slider _slider_HPBar;


		[SerializeField, LabelText("Text_HP当前值"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_HPValue;

		[SerializeField, LabelText("Text_HP最大值"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_HPMaxValue;
		[SerializeField, LabelText("Slider_SP滑条"), TitleGroup("配置/左侧基本信息")]
		public Slider _slider_SPBar;

		[SerializeField, LabelText("Text_SP当前值"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_SPValue;

		[SerializeField, LabelText("Text_SP最大值"), TitleGroup("配置/左侧基本信息")]
		public TextMeshProUGUI _text_SPMaxValue;




#region 右上角toggle组

		[SerializeField, LabelText("Toggle_天赋"), TitleGroup("配置/右上角toggle组")]
		private Toggle _toggle_RightPerkPanel;

		[SerializeField, LabelText("Toggle_装备"), TitleGroup("配置/右上角toggle组")]
		private Toggle _toggle_InventoryPanel;


		[SerializeField, LabelText("Toggle_角色信息"), TitleGroup("配置/右上角toggle组")]
		private Toggle _toggle_characterInfo;

#endregion






#region sp 角色详情

		[SerializeField, Required, LabelText("SP-角色信息详情"), TitleGroup("配置/SP 角色详情")]
		private UISP_PCF_CharacterDetailInfoSubPanel _SP_CharacterDetailInfoSubPanel;

#endregion
#region sp 当前角色装备技能

		[SerializeField, Required, LabelText("SP-中间面板，包括装备主属性技能"), TitleGroup("配置/SP 角色详情")]
		private UISP_PCF_CurrentCharacterStatePanel _SP_CurrentCharacterStateSubPanel;

#endregion

#region sp 右侧阅历天赋

		[SerializeField, Required, LabelText("sp-右侧面板 阅历天赋"), TitleGroup("配置/SP 右侧面板 阅历天赋")]
		private UISP_PCF_PlayerExpAndPerkPanel _SP_PlayerExpAndPerkPanel;

#endregion


		[SerializeField, Required, LabelText("SP-装备仓库"), TitleGroup("配置/SP 装备仓库")]
		private UISP_PCF_EquipmentInventorySubPanel _SP_EquipmentInventorySubPanel;

#region 悬停

		[LabelText("holder_悬停详情的holder"), SerializeField, Required, TitleGroup("===Widget===")]
		private RectTransform _holder_HoverDetail;

		[LabelText("text_详情文本 "), SerializeField, Required, TitleGroup("===Widget===")]
		private TMPro.TextMeshProUGUI _text_HoverDetail;
		
		[LabelText("text_详情附加文本 "), SerializeField, Required, TitleGroup("===Widget===")]
		private TMPro.TextMeshProUGUI _text_HoverDetailExtra;

		private void _callback_ShowDetail_OnHoverOnPerk(string perkID, BaseEventData baseEventData)
		{
			_holder_HoverDetail.gameObject.SetActive(true);
			var pos = GetLocalPositionOfPointerAligned(baseEventData);
			_holder_HoverDetail.transform.localPosition = pos;
			
			var perkName = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterPerkName, perkID);
			var perkContent = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterPerkDesc, perkID);

			_text_HoverDetail.text = perkName;
			_text_HoverDetailExtra.text = perkContent;
		}

		private void _callback_ShowDetail_OnHoverOnExtTag(string tag, BaseEventData baseEventData)
		{
			_holder_HoverDetail.gameObject.SetActive(true);
			var pos = GetLocalPositionOfPointerAligned(baseEventData);
			_holder_HoverDetail.transform.localPosition = pos;

			var perkContent = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterEXPDesc, tag);
			_text_HoverDetail.text = perkContent;
		}


		private void _callback_ShowEquipmentPerkDetail_OnHoverOnEquipmentPerkEntry(
			UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel entry,
			BaseEventData eventData)
		{
			_holder_HoverDetail.gameObject.SetActive(true);
			var pos = GetLocalPositionOfPointerAligned(eventData);
			_holder_HoverDetail.transform.localPosition = pos;

			//装备perk的组织是：名字 + 名字X
			//比如  {逐梦者}{逐梦者1} 就==
			string s1 = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LC_PerkText, entry.PerkUID);
			string s2 = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
				.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LC_PerkText, entry.PerkUID + entry.PerkLevel);
			_text_HoverDetail.text = s1 + entry.PerkLevel + s2;
		}



		private void _callback_HideDetail()
		{
			_holder_HoverDetail.gameObject.SetActive(false);
		}

#endregion

#region Hover _悬停

		[SerializeField, Required, LabelText("SP-装备悬停"), TitleGroup("配置/悬停")]
		private UISP_HoverEquipmentSubPanel_Common _sf_spHoverEquipmentSubPanelCommon;

		[SerializeField, Required, LabelText("SP-技能悬停"), TitleGroup("配置/悬停")]
		private UISP_PCF_HoverSkillSubPanel _SP_HoverSkillSubPanel;

#endregion






		public override void StartInitializeByUIM()
		{
			base.StartInitializeByUIM();
			_SP_CharacterDetailInfoSubPanel.StartInitializeBySP(this);




			_SP_CurrentCharacterStateSubPanel.InjectCallback_Skill(
				_callback_ShowSkillDetailHover_OnPointerEnterSkillSlot,
				_callback_HideSkillDetailHover_OnPointerExitSkillSlot);
			_SP_CurrentCharacterStateSubPanel.InjectCallback_CurrentWeapon(_callback_TryDisarmEquipment_ClickEquipped,
				_callback_ShowEquippedEquipmentDetail_OnPointerEnterEquippedEquipment,
				_callback_OnPointerExitNewEquipment_InsideEquipped);
			_SP_CurrentCharacterStateSubPanel.StartInitializeBySP(this);


			_SP_EquipmentInventorySubPanel.InjectConcreteItemCallbackAction(
				_callback_EquipmentDetail_OnPointerEnterNewEquipment,
				_callback_HideEquipmentHover_OnPointerExitEquipmentInInventoryUIRW,
				_callback_TryChangeToThisEquipment_OnClickEquipmentInInventoryUIRW);

			_SP_EquipmentInventorySubPanel.StartInitializeBySP(this);



			_sf_spHoverEquipmentSubPanelCommon.StartInitializeBySP(this);

			_SP_PlayerExpAndPerkPanel.InjectHoverCallback(_callback_ShowDetail_OnHoverOnPerk,
				_callback_ShowDetail_OnHoverOnExtTag,
				_callback_ShowEquipmentPerkDetail_OnHoverOnEquipmentPerkEntry,
				_callback_HideDetail);
			_SP_PlayerExpAndPerkPanel.StartInitializeBySP(this);
			_holder_HoverDetail.gameObject.SetActive(false);

			_SP_HoverSkillSubPanel.StartInitializeBySP(this);
            GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_UI_RequireOpenPlayerCharacterFullPanel_要求开启玩家角色信息面板,
                        _ABC_ShowPlayerCharacterFullPanel_OnRequireOpenPlayerCharacterFullPanel);
        }


		protected override void BindingEvent()
		{
			base.BindingEvent();


			_button_backButton.onClick.AddListener(() => HideThisPanel());
			_toggle_characterInfo.onValueChanged.AddListener(_Toggle_ToggleToCharacterDetailInfo);
			_toggle_InventoryPanel.onValueChanged.AddListener(_Toggle_ToggleToEquipmentInventory);
			_toggle_RightPerkPanel.onValueChanged.AddListener(_Toggle_ToggleToPerkAndEXP);

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_CharacterEquippedEquipmentChanged_角色装备发生变化,
				_ABC_RefreshCurrentCharacterEquipmentOnEquipmentChanged,
				300);

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_EquipmentInventoryChanged_装备背包发生变化,
				_ABC_ProcessEquipmentInventoryChanged_OnEquipmentInventoryChanged,
				20);
		}


		public override void LateInitializeByUIM()
		{
			base.LateInitializeByUIM();
		}
		public override void ShowThisPanel(bool clearShow = true, bool containIngoreEvent = false)
		{
			var ds_pause = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequirePauseGame_要求暂停游戏);
			ds_pause.ObjectArgu1 = this;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_pause);


			if (GameReferenceService_ARPG.Instance != null)
			{
				PlayerARPGConcreteCharacterBehaviour info = GameReferenceService_ARPG.Instance
					.SubGameplayLogicManagerRef.PlayerCharacterBehaviourControllerReference.CurrentControllingBehaviour;
				TopTeam_SelectNewCharacterInTeam(info.SelfCharacterID);
			}




			base.ShowThisPanel(clearShow);
			GenerateTopCharacterTeam();

			RefreshContentOnSelectNewCharacter();




			//左侧角色基本信息部分
			RefreshContent_LeftSideCharacterBaseInfo();
			_SP_CharacterDetailInfoSubPanel.ShowThisSubPanel();
			_SP_CurrentCharacterStateSubPanel.ShowThisSubPanel();
			_SP_EquipmentInventorySubPanel.HideThisSubPanel();
			_SP_HoverSkillSubPanel.HideThisSubPanel();
			_toggle_characterInfo.isOn = true;
		}


		/// <summary>
		/// <para>刷新内容：左侧角色基本信息部分，名字血条Sp条职业</para>
		/// </summary>
		public void RefreshContent_LeftSideCharacterBaseInfo()
		{
			GCSO_PerCharacterInfo characterInfo =
				GlobalConfigSO.RuntimeContent().AllCharacterInfoCollection
					.Find((info => info.CharacterID == CurrentSelectCharacterID));



			var fullConfig =
				((CharacterNamedTypeEnum)characterInfo.CharacterID).GetFullThreeInfoByType(
					characterInfo.GetComponent<CIC_PlayerLevelInfo>().CurrentLevel);

			//设置立绘
			_image_CharacterPortrait.sprite = fullConfig.Item3._OP_Portrait_HalfHandle.Result;

            //设置名称
            _text_CharacterName.text = new LocalizedString(LocalizationTableNameC._LCT_CharacterName,
				characterInfo.CharacterID.ToString()).GetLocalizedString();

			//设置职业
			_text_CharacterJobName.text = new LocalizedString(LocalizationTableNameC._LCT_CharacterBattleJobName,
                characterInfo.GetComponent<CIC_PlayerJobInfo>().CurrentJob.ToString()).GetLocalizedString();

			switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
			{
				case GeneralGameAssetLoadPhaseEnum.WorldMap:
				case GeneralGameAssetLoadPhaseEnum.RegionMap:
					var r = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelperCharacter
						.GetCharacterRecord(CurrentSelectCharacterID);
					float hpP =1f;
					_slider_HPBar.value = hpP;
					//tostring as int
					_text_HPValue.text = r.MaxHP.ToString("F0");
					_text_HPMaxValue.text = r.MaxHP.ToString("F0");

					float spP = 1f;
					_slider_SPBar.value = spP;
					_text_SPValue.text = r.MaxSP.ToString("F0");
					_text_SPMaxValue.text = r.MaxSP.ToString("F0");
					break;
				case GeneralGameAssetLoadPhaseEnum.ARPG:
					var behaviour =
						GameReferenceService_ARPG.Instance.SubGameplayLogicManagerRef
							.PlayerCharacterBehaviourControllerReference.GetBehaviourRef(CurrentSelectCharacterID);
					float hpP2 =
						behaviour.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP).CurrentValue /
						behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).CurrentValue;
					_slider_HPBar.value = hpP2;
					//tostring as int
					_text_HPValue.text = behaviour.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP)
						.CurrentValue.ToString("F0");
					_text_HPMaxValue.text = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP)
						.CurrentValue.ToString("F0");

					float spP2 =
						behaviour.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP).CurrentValue /
						behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP).CurrentValue;
					_slider_SPBar.value = spP2;
					_text_SPValue.text = behaviour.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP)
						.CurrentValue.ToString("F0");
					_text_SPMaxValue.text = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP)
						.CurrentValue.ToString("F0");


					break;
			}
		}


		public override void HideThisPanel(bool notBroadcast = false)
		{
			//清理所有内容
			foreach (UIRW_CharacterInTeamInsidePlayerCharacterFullPanel perUIRW in _list_AllTopCurrentTeamMemberUIRWs)
			{
				perUIRW.ClearBeforeDestroy();
				GameObject.Destroy(perUIRW.gameObject);
			}
			_list_AllTopCurrentTeamMemberUIRWs.Clear();
			base.HideThisPanel();
			var ds_unpause = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Global_RequireResumeGame_要求解除暂停);
			ds_unpause.ObjectArgu1 = this;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_unpause);
		}

        private void _ABC_ShowPlayerCharacterFullPanel_OnRequireOpenPlayerCharacterFullPanel(DS_ActionBusArguGroup ds)
        {
            ShowThisPanel();
        }


        #region 顶部队伍  部分的各种方法

        [SerializeField, LabelText("Holder_顶部小队"), TitleGroup("配置/顶部小队")]
		private GameObject _holder_TopCurrentTeam;

		[SerializeField, LabelText("Layout_小队成员Layout"), TitleGroup("配置/顶部小队")]
		private HorizontalLayoutGroup _layout_TopCurrentTeamLayout;

		[SerializeField, LabelText("uirw_顶部小队成员UIRW"), TitleGroup("配置/顶部小队")]
		private GameObject _prefab_TopCurrentTeamMemberUIRW;

		[SerializeField, LabelText("参数_顶部小队成员UIRW 未选中尺寸"), TitleGroup("配置/顶部小队")]
		private float _param_TopCurrentTeamMemberUIRW_UnSelectedSize = 90f;

		[SerializeField, LabelText("参数_顶部小队成员UIRW 选中尺寸"), TitleGroup("配置/顶部小队")]
		private float _param_TopCurrentTeamMemberUIRW_SelectedSize = 110f;


		[ShowInInspector, LabelText("当前角色ID"), TitleGroup("运行时/顶部小队")]
		public int CurrentSelectCharacterID { get; protected set; }

		[ShowInInspector, LabelText("当前所有小队成员UIRW"), TitleGroup("运行时/顶部小队")]
		public List<UIRW_CharacterInTeamInsidePlayerCharacterFullPanel> _list_AllTopCurrentTeamMemberUIRWs
		{
			get;
			protected set;
		} = new List<UIRW_CharacterInTeamInsidePlayerCharacterFullPanel>();




		/// <summary>
		/// 需要刷新一下，比如左侧的、中间的、右侧的
		/// </summary>
		private void RefreshContentOnSelectNewCharacter()
		{
			RefreshContent_LeftSideCharacterBaseInfo();

			//刷新右侧的，并显示为详情而不是仓库或阅历
			_SP_CharacterDetailInfoSubPanel.RefreshContent();
			_SP_CharacterDetailInfoSubPanel.ShowThisSubPanel();
			//刷新中间的
			_SP_CurrentCharacterStateSubPanel.RefreshContent();
			//如果显示的是仓库，就先关了
			_SP_EquipmentInventorySubPanel.HideThisSubPanel();
			_SP_PlayerExpAndPerkPanel.HideThisSubPanel();
		}


		/// <summary>
		/// <para>生成顶部小队。如果当前是ARPG场景，那就只生成出战小队。在其他场景会生成所有成员</para>
		/// </summary>
		private void GenerateTopCharacterTeam()
		{
			foreach (UIRW_CharacterInTeamInsidePlayerCharacterFullPanel perUIRW in _list_AllTopCurrentTeamMemberUIRWs)
			{
				Destroy(perUIRW.gameObject);
			}
			_list_AllTopCurrentTeamMemberUIRWs.Clear();
			List<CharacterNamedTypeEnum> willGenerate = new List<CharacterNamedTypeEnum>();
			var gcso = GlobalConfigSO.RuntimeContent();

			if (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase() == GeneralGameAssetLoadPhaseEnum.ARPG)
			{
				//选出当前队伍
				var teamCurrent = gcso.CurrentPlayerTeamList[gcso.CurrentActiveTeamIndex];
				foreach (CharacterNamedTypeEnum perC in teamCurrent.CharacterList)
				{
					willGenerate.Add(perC);
				}
			}
			else
			{
				foreach (var perTeam in gcso.CurrentPlayerTeamList)
				{
					foreach (CharacterNamedTypeEnum perC in perTeam.CharacterList)
					{
						willGenerate.Add(perC);
					}
				}
			}




			switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
			{
				case GeneralGameAssetLoadPhaseEnum.WorldMap:
				case GeneralGameAssetLoadPhaseEnum.RegionMap:
					CurrentSelectCharacterID = (int)willGenerate[0];
					break;
				case GeneralGameAssetLoadPhaseEnum.ARPG:
					CurrentSelectCharacterID = SubGameplayLogicManager_ARPG.Instance
						.PlayerCharacterBehaviourControllerReference.CurrentControllingBehaviour.SelfCharacterID;
					break;
			}

			//生成顶部队伍栏
			foreach (CharacterNamedTypeEnum perCharacter in willGenerate)
			{
				var baseInfo = GlobalConfigSO.RuntimeContent().AllCharacterInfoCollection.
					Find((info => info.CharacterID == (int)perCharacter));
				var fullConfig = perCharacter.GetFullThreeInfoByType();


				GameObject newUIRWGO = Instantiate(_prefab_TopCurrentTeamMemberUIRW,
					_layout_TopCurrentTeamLayout.transform);
				UIRW_CharacterInTeamInsidePlayerCharacterFullPanel newUIRW =
					newUIRWGO.GetComponent<UIRW_CharacterInTeamInsidePlayerCharacterFullPanel>();
				_list_AllTopCurrentTeamMemberUIRWs.Add(newUIRW);
				newUIRW._le_selfLayoutElement.preferredWidth = _param_TopCurrentTeamMemberUIRW_UnSelectedSize;
				newUIRW._le_selfLayoutElement.preferredHeight = _param_TopCurrentTeamMemberUIRW_UnSelectedSize;
				newUIRW._image_characterSelfImage.sprite = fullConfig.Item3.GetPortrait_Pixel();
				newUIRW.InitializeOnInstantiate((int)perCharacter,
					_callback_RefreshToCurrentSelectCharacter_OnCurrentCharacterInTeamClicked);
			}
			//直接选中当前
			_callback_RefreshToCurrentSelectCharacter_OnCurrentCharacterInTeamClicked(CurrentSelectCharacterID, true);
		}

		private void _callback_RefreshToCurrentSelectCharacter_OnCurrentCharacterInTeamClicked(int cid)
		{
			_callback_RefreshToCurrentSelectCharacter_OnCurrentCharacterInTeamClicked(cid, false);
		}


		/// <summary>
		/// <para>当点选了一个顶部的小队成员。如果就是刚才选的那个，无事发生，不是则刷新</para>
		/// <para>这是一个回调，在顶部小队成员生成时传入</para>
		/// </summary>
		private void _callback_RefreshToCurrentSelectCharacter_OnCurrentCharacterInTeamClicked(int cid, bool allowSame)
		{
			if (CurrentSelectCharacterID == cid && !allowSame)
			{
				return;
			}

			TopTeam_SelectNewCharacterInTeam(
				_list_AllTopCurrentTeamMemberUIRWs.Find((uirw => uirw.RelatedCharacterID == cid)));
			CurrentSelectCharacterID = cid;

			RefreshContentOnSelectNewCharacter();
		}



		/// <summary>
		/// <para>选中了上方的新角色，需要处理一下那个缩放动画</para>
		/// </summary>
		private void TopTeam_SelectNewCharacterInTeam(UIRW_CharacterInTeamInsidePlayerCharacterFullPanel selectUIRW)
		{
			foreach (UIRW_CharacterInTeamInsidePlayerCharacterFullPanel perUIRW in _list_AllTopCurrentTeamMemberUIRWs)
			{
				if (perUIRW == selectUIRW)
				{
					perUIRW.SelectThisUIRW();
					perUIRW._le_selfLayoutElement.DOPreferredSize(
						Vector2.one * _param_TopCurrentTeamMemberUIRW_SelectedSize,
						0.5f).SetUpdate(true);
				}
				else
				{
					perUIRW.DeselectThisUIRW();

					perUIRW._le_selfLayoutElement.DOPreferredSize(
						Vector2.one * _param_TopCurrentTeamMemberUIRW_UnSelectedSize,
						0.5f).SetUpdate(true);
				}
			}
		}
		private void TopTeam_SelectNewCharacterInTeam(int characterID)
		{
			TopTeam_SelectNewCharacterInTeam(
				_list_AllTopCurrentTeamMemberUIRWs.Find((x) => x.RelatedCharacterID == characterID));
		}

#endregion


#region 当前角色的装备

		/// <summary>
		/// 试图卸下装备。武器是不能卸下的，只能直接更换
		/// </summary>
		private void _callback_TryDisarmEquipment_ClickEquipped(GlobalConfigSO.PlayerEquipmentInfo click)
		{
			if (click == null)
			{
				return;
			}

			switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
			{
				case GeneralGameAssetLoadPhaseEnum.WorldMap:
				case GeneralGameAssetLoadPhaseEnum.RegionMap:
					if (click.IfIsEquipmentIsWeapon())
					{
						//武器不能卸下
						var ds_not =
							new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_RequireNotification_要求广播一个通知);
						ds_not.ObjectArguStr = "~武器不能卸下，只可更换~";
						GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_not);
						return;
					}
					else
					{
						click.DisarmEquipment();
					}
					break;
				case GeneralGameAssetLoadPhaseEnum.ARPG:
					var ds_noti = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_RequireNotification_要求广播一个通知);
					ds_noti.ObjectArguStr = "~战斗中不可更换装备~";
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_noti);
					
					return;
			}
		}



		private void _callback_OnPointerExitNewEquipment_InsideEquipped()
		{
			_sf_spHoverEquipmentSubPanelCommon.HideThisSubPanel();
			_holder_HoverDetail.gameObject.SetActive(false);
		}


		private void _ABC_RefreshCurrentCharacterEquipmentOnEquipmentChanged(DS_ActionBusArguGroup ds)
		{

			RefreshContent_LeftSideCharacterBaseInfo();
			_SP_CurrentCharacterStateSubPanel.RefreshContent();
		}

#endregion


#region 装备

#region 查看装备

		/// <summary>
		/// <para>当指针进入装备面板中的一个装备时</para>
		/// </summary>
		public void _callback_EquipmentDetail_OnPointerEnterNewEquipment(
			UIRW_PerEquipmentEntryInEquipmentInventorySubPanel enter,
			BaseEventData baseEventData)
		{

			_sf_spHoverEquipmentSubPanelCommon.HideThisSubPanel();
			_holder_HoverDetail.gameObject.SetActive(false);
			var pos = GetLocalPositionOfPointerAligned(baseEventData);

			
			
			GlobalConfigSO.PlayerEquipmentInfo content = enter._rawEquipmentInfo;
			if (content != null)
			{
				_sf_spHoverEquipmentSubPanelCommon.ShowThisSubPanel();
				pos.y = 0f;
				_sf_spHoverEquipmentSubPanelCommon.transform.localPosition = pos;
				if (content.IfIsEquipmentIsWeapon())
				{
					_sf_spHoverEquipmentSubPanelCommon.ShowOnType_WeaponInInventory(content);
					return;
				}
				if (content.IfIsEquipmentIsNonWeapon())
				{
					_sf_spHoverEquipmentSubPanelCommon.ShowOnType_EquipmentInInventory(content);
					return;
				}
			} //是个材料
			else
			{
				_holder_HoverDetail.gameObject.SetActive(true);
				_holder_HoverDetail.transform.localPosition = pos;
				var itemName = GCAHHExtend.GetProperty(enter._rawIngredientInfo.UID).Name;
				_text_HoverDetail.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
					.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_IngredientName,
                        itemName);
				_text_HoverDetailExtra.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
					.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_IngredientDesc,
                        itemName);
			}
		}



		private void _callback_ShowEquippedEquipmentDetail_OnPointerEnterEquippedEquipment(
			GlobalConfigSO.PlayerEquipmentInfo enter,
			BaseEventData baseEventData)
		{
			GlobalConfigSO.PlayerEquipmentInfo content = enter;
			if (content == null)
			{
				return;
			}

			_sf_spHoverEquipmentSubPanelCommon.ShowThisSubPanel();

			var pos = GetLocalPositionOfPointerAligned(baseEventData);
			pos.y = 0f;
			_sf_spHoverEquipmentSubPanelCommon.transform.localPosition = pos;


		
			if (content.IfIsEquipmentIsWeapon())
			{
				_sf_spHoverEquipmentSubPanelCommon.ShowOnType_EquippedWeapon(content);
			}
			if( content.IfIsEquipmentIsNonWeapon())
			{
				_sf_spHoverEquipmentSubPanelCommon.ShowOnType_EquippedEquipment(content);
			}
		}

#endregion



#region 更换装备

		/// <summary>
		/// <para> 试图更换这件装备——当点击了仓库中的装备UIRW时</para>
		/// <para>作为callback</para>
		/// </summary>
		public void _callback_TryChangeToThisEquipment_OnClickEquipmentInInventoryUIRW(
			UIRW_PerEquipmentEntryInEquipmentInventorySubPanel changeTo)
		{
			//点击了这个武器，要换掉它。
			//要刷新所有

			switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
			{
				case GeneralGameAssetLoadPhaseEnum.WorldMap:
				case GeneralGameAssetLoadPhaseEnum.RegionMap:

					//先检查这个部位是不是已经有装备了，如果有，就先卸掉
					var record = GlobalConfigurationAssetHolderHelper.Instance.RuntimeRecordHelperCharacter;

					record.Equipment_EquipThisEquipment(CurrentSelectCharacterID, changeTo._rawEquipmentInfo);

					_callback_HideEquipmentHover_OnPointerExitEquipmentInInventoryUIRW();
					break;
				case GeneralGameAssetLoadPhaseEnum.ARPG:
					var ds_noti = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_RequireNotification_要求广播一个通知);
					ds_noti.ObjectArguStr = "~战斗中不可更换装备~";
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_noti);
					break;
			}
			
		}




		public void _callback_HideEquipmentHover_OnPointerExitEquipmentInInventoryUIRW()
		{
			_sf_spHoverEquipmentSubPanelCommon.HideThisSubPanel();
			_holder_HoverDetail.gameObject.SetActive(false);
		}


		private void _ABC_ProcessEquipmentInventoryChanged_OnEquipmentInventoryChanged(DS_ActionBusArguGroup ds)
		{
			_SP_EquipmentInventorySubPanel.ShowContents(UISP_PCF_EquipmentInventorySubPanel.ShowTypes
				.AllEquipment_所有装备);
		}

#endregion

#endregion


#region 右侧选项

		private void _Toggle_ToggleToCharacterDetailInfo(bool isOn)
		{
			if (isOn)
			{
				var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
				ds.ObjectArgu1 = _toggle_characterInfo.transform;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds);

				_SP_CharacterDetailInfoSubPanel.ShowThisSubPanel();
			}
			else
			{
				_SP_CharacterDetailInfoSubPanel.HideThisSubPanel();
			}
		}

		private void _Toggle_ToggleToEquipmentInventory(bool isOn)
		{
			if (isOn)
			{
				var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
				ds.ObjectArgu1 = _toggle_InventoryPanel.transform;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds);

				_SP_EquipmentInventorySubPanel.ShowThisSubPanel();
				_SP_EquipmentInventorySubPanel.ShowContents(UISP_PCF_EquipmentInventorySubPanel.ShowTypes
					.All);
			}
			else
			{
				_SP_EquipmentInventorySubPanel.HideThisSubPanel();
			}
		}


		private void _Toggle_ToggleToPerkAndEXP(bool isOn)
		{
			if (isOn)
			{
				var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_UI_General_RequireGeneralInteractVFX_UI要求通用交互VFX);
				ds.ObjectArgu1 = _toggle_RightPerkPanel.transform;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds);

				_SP_PlayerExpAndPerkPanel.ShowThisSubPanel();
			}
			else
			{
				_SP_PlayerExpAndPerkPanel.HideThisSubPanel();
			}
		}

#endregion


#region 技能悬停

		
		private void _callback_ShowSkillDetailHover_OnPointerEnterSkillSlot(
			int cid,SkillSlotTypeEnum slotType, RPSkill_SkillTypeEnum skillEnum,
			BaseEventData baseEventData)
		{
			var pos = GetLocalPositionOfPointerAligned(baseEventData);
			// pos.y = 0f;
			_SP_HoverSkillSubPanel.transform.localPosition = pos;
			SOConfig_RPSkill skillConfigRef = null;
			if (slotType != SkillSlotTypeEnum.NormalAttack_是普攻)
			{
				skillConfigRef = GlobalConfigurationAssetHolderHelper.GetGCAHH().Collection_SkillConfig
					.GetRPSkillConfigByTypeAndLevel(skillEnum, 1);
			}
			_SP_HoverSkillSubPanel.ShowThisSubPanel();
			_SP_HoverSkillSubPanel.RefreshDisplayContent(slotType, skillEnum, skillConfigRef);
		}

		private void _callback_HideSkillDetailHover_OnPointerExitSkillSlot()
		{
			_SP_HoverSkillSubPanel.HideThisSubPanel();
		}

#endregion

#region 整体监听

#endregion

	}
}