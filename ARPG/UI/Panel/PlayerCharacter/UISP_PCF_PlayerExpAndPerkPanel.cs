using System.Collections.Generic;
using ARPG.Character.Config;
using Global;
using Global.ActionBus;
using Global.GlobalConfig;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ARPG.UI.Panel.PlayerCharacter
{
	[TypeInfoBox("在玩家完整详情面板上，右侧的天赋和阅历信息")]
	public class UISP_PCF_PlayerExpAndPerkPanel : UI_UIBaseSubPanel
	{


		private UIP_PlayerCharacterFullPanel _PCF_parentUIP;
		
		
		
		

#region 天赋


		[LabelText("grid_具体天赋排版布局"), SerializeField, Required, TitleGroup("===Widget===")]
		private GridLayoutGroup _grid_CharacterPerkLayout;


		[LabelText("prefab_单条天赋信息"), SerializeField, Required, TitleGroup("===prefab===")]
		private GameObject _prefab_CharacterPerkEntry;

		private List<UIRW_CharacterPerk_PlayerCharacterFullPanel> _list_CharacterPerk =
			new List<UIRW_CharacterPerk_PlayerCharacterFullPanel>();



		private void RefreshPerkContent()
		{

			var playerCID = _PCF_parentUIP.CurrentSelectCharacterID;
			// var getFull = ((CharacterNamedTypeEnum)playerCID).GetFullThreeInfoByType();
			var perkContent = GCSOExtend.GetPlayerCharacterExtraCharacterPerkContentComponent(playerCID);
		
			if (perkContent != null)
			{
				foreach (string perPerk in perkContent.PerkList)
				{
					var newObj = Instantiate(_prefab_CharacterPerkEntry, _grid_CharacterPerkLayout.transform);
					var perUIRW = newObj.GetComponent<UIRW_CharacterPerk_PlayerCharacterFullPanel>();
					perUIRW.Init(perPerk, _callback_ShowPerkDetail_OnHover, _callback_HideDetail);
					_list_CharacterPerk.Add(perUIRW);
				}
			}
			

		}


#endregion


#region 阅历
		
		[LabelText("grid-阅历排版布局"), SerializeField, Required, TitleGroup("===Widget===")]
		private GridLayoutGroup _grid_CharacterExpLayout;
		
		[LabelText("prefab_单条阅历信息"), SerializeField, Required, TitleGroup("===prefab===")]
		private GameObject _prefab_CharacterExpEntry;
		
		private List<UIRW_CharacterExpTagEntry_PlayerCharacterFullPanel> _list_CharacterExp =
			new List<UIRW_CharacterExpTagEntry_PlayerCharacterFullPanel>();


		private void RefreshExpContent()
		{
			var playerCID = _PCF_parentUIP.CurrentSelectCharacterID;
			// var getFull = ((CharacterNamedTypeEnum)playerCID).GetFullThreeInfoByType();
			var getCIC = GCSOExtend.GetPlayerCharacterExtraEXPContentComponent( playerCID);
			;
			if (getCIC != null)
			{
				foreach (string perExp in getCIC.EXPList)
				{
					var newObj = Instantiate(_prefab_CharacterExpEntry, _grid_CharacterExpLayout.transform);
					var perUIRW = newObj.GetComponent<UIRW_CharacterExpTagEntry_PlayerCharacterFullPanel>();
					perUIRW.InstantiateInitialize(perExp, _callback_ShowExpDetail_OnHover, _callback_HideDetail);
					_list_CharacterExp.Add(perUIRW);
				}
			}
		}
		 
		
		

#endregion




		public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
		{
			base.StartInitializeBySP(parentUIP);
			_PCF_parentUIP = parentUIP as UIP_PlayerCharacterFullPanel;

			GlobalActionBus.GetGlobalActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.G_CharacterEquippedEquipmentChanged_角色装备发生变化,
					_ABC_RefreshContent);
		}




		private void _ABC_RefreshContent(DS_ActionBusArguGroup ds)
		{
			ResetContent();
			RefreshPerkContent();
			RefreshExpContent();

			
		}




		public override void ShowThisSubPanel()
		{
			base.ShowThisSubPanel();
			
			ResetContent();
			RefreshPerkContent();
			RefreshExpContent();


			var playerCID = ((CharacterNamedTypeEnum)_PCF_parentUIP.CurrentSelectCharacterID).GetCharacterNameStringByType();
			
			
		}


		private void ResetContent()
		{
			foreach (UIRW_CharacterExpTagEntry_PlayerCharacterFullPanel perUIRW in _list_CharacterExp)
			{
				Destroy(perUIRW.gameObject);
			}
			_list_CharacterExp.Clear();

			 
			foreach (UIRW_CharacterPerk_PlayerCharacterFullPanel perUIRW in _list_CharacterPerk)
			{
				Destroy(perUIRW.gameObject);
			}
			_list_CharacterPerk.Clear();
			
			
			RefreshCurrentActiveEquipmentPerk();
			;



		}

#region 装备perk|刻印
		
		
		[LabelText("sprite-激活的perk等级") , SerializeField, Required, TitleGroup("===asset===")]
		public Sprite _sprite_ActivePerkLevel;
		
		[LabelText("sprite-未激活的perk等级") , SerializeField, Required, TitleGroup("===asset===")]
		public Sprite _sprite_InactivePerkLevel;
		
		[LabelText("prefab-单条perk项目") , SerializeField, Required, TitleGroup("===asset===")]
		 private GameObject _prefab_SinglePerkEntry;
		
		List<UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel> _list_SinglePerkEntry = new List<UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel>();
		
		[LabelText("grid_perk项目们的grid布局") , SerializeField, Required, TitleGroup("===Widget===")]
		private GridLayoutGroup _grid_PerksLayout;
		
		 private UnityAction<UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel , BaseEventData> _callback_ShowEquipmentPerkDetail_OnHover;




		 public void RefreshCurrentActiveEquipmentPerk()
		 {
			 foreach (UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel perUIRW in _list_SinglePerkEntry)
			 {
				 Destroy(perUIRW.gameObject);
			 }
			 _list_SinglePerkEntry.Clear();
			 
			 //拿一下当前角色的perk信息，在CharacterRecord里面
			 var perkInfo = GlobalConfigurationAssetHolderHelper.GetGCAHH().RuntimeRecordHelperCharacter
				 .GetCharacterRecord(_PCF_parentUIP.CurrentSelectCharacterID);

			 foreach (KeyValuePair<string,int> per in perkInfo.GetActivePerkInfo_ExceptPrefix())
			 {
				 var newObj = Instantiate(_prefab_SinglePerkEntry, _grid_PerksLayout.transform);
				 var perUIRW = newObj.GetComponent<UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel>();
				 perUIRW.InstantiateInitialize(per.Key,
					 per.Value,
					 this,
					 _callback_ShowEquipmentPerkDetail_OnHover,
					 _callback_HideDetail);
				 _list_SinglePerkEntry.Add(perUIRW);
				 
			 }



		 }
		 

#endregion




#region 悬停
		


		private UnityAction<string, BaseEventData> _callback_ShowPerkDetail_OnHover;
		private UnityAction<string, BaseEventData> _callback_ShowExpDetail_OnHover;


		private UnityAction _callback_HideDetail;
		
		
		public void InjectHoverCallback(UnityAction<string, BaseEventData> callback_ShowPerkDetail_OnHover, 
			UnityAction<string, BaseEventData> callback_ShowExpDetail_OnHover,
			UnityAction<UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel, BaseEventData> callback_ShowEquipmentPerkDetail_OnHover,
			UnityAction callback_HideDetail)
		{
			_callback_ShowPerkDetail_OnHover = callback_ShowPerkDetail_OnHover;
			_callback_ShowExpDetail_OnHover = callback_ShowExpDetail_OnHover;
			_callback_ShowEquipmentPerkDetail_OnHover = callback_ShowEquipmentPerkDetail_OnHover;
			_callback_HideDetail = callback_HideDetail;
		}
#endregion




	}
}