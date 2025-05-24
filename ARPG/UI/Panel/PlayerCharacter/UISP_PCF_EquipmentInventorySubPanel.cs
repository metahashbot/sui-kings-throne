using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Equipment;
using Global;
using Global.GlobalConfig;
using Global.UI;
using Global.UIBase;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ARPG.UI.Panel.PlayerCharacter
{
	[TypeInfoBox("通用装备仓库面板。用于 玩家完整背包、强化选取、商店出售")]
	public class UISP_PCF_EquipmentInventorySubPanel : UI_UIBaseSubPanel
	{

#region 排列
		[LabelText("Dropdown_排列的DropDown") ,SerializeField,Required,TitleGroup("===Widget===")]
		private TMP_Dropdown _dropdown_SortType;


		public enum SortTypeEnum
		{
			None_无排列 = 0, ByType_EquipmentType_按照装备类型排列 = 1, ByQuality_按照品质排列 = 2,
		}


		public SortTypeEnum CurrentSortType { get; private set; } = SortTypeEnum.None_无排列;

		public void SetSortType(SortTypeEnum sortType)
		{
			CurrentSortType = sortType;
			ShowContents(CurrentShowTypes);
		}



		public void OVC_Toggle_SortType_None(int tt)
		{
			switch (tt)
			{
				case 0:
					SetSortType(SortTypeEnum.None_无排列);
					break;
				case 1:
					SetSortType(SortTypeEnum.ByType_EquipmentType_按照装备类型排列);
					break;
				case 2:
					SetSortType(SortTypeEnum.ByQuality_按照品质排列);
					break;
			}
		}

#endregion
		[Flags]
		public enum ShowTypes
		{
			None_无 = 0,
			Weapon_武器 = 1,
			NonWeaponEquipment_非武器装备 = 2,
			AllEquipment_所有装备 = Weapon_武器 | NonWeaponEquipment_非武器装备,
			Ingredient_材料 = 4,
			All = AllEquipment_所有装备 | Ingredient_材料
		}



		/// <summary>
		/// 是否已经包含了搜索选项
		/// <para>通常在已经包含了搜索选项时，会无视重刷需求</para>
		/// </summary>
		public ShowTypes CurrentShowTypes { get; private set; } = ShowTypes.None_无;


		[SerializeField, Required, LabelText("layout——装备条目Layout"), TitleGroup("配置")]
		private GridLayoutGroup _layout_EquipmentInInventory;


		[SerializeField, Required, LabelText("prefab——UIRW装备条目"), TitleGroup("配置")]
		private GameObject _prefab_PerEquipmentEntry;


		private List<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel> _currentAllEquipmentUIRWList =
			new List<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel>();

		
		
		



		public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
		{
			base.StartInitializeBySP(parentUIP);
			_button_AdjustShowEquipment.onClick.RemoveAllListeners();
			_button_AdjustShowEquipment.onClick.AddListener(_Button_ToggleToShowEquipment);
			_button_AdjustShowCharacterCardContent.onClick.RemoveAllListeners();
			_button_AdjustShowCharacterCardContent.onClick.AddListener(_Button_ToggleToShowCard);
			_button_AdjustShowIngredientContent.onClick.RemoveAllListeners();
			_button_AdjustShowIngredientContent.onClick.AddListener(_Button_ToggleToShowIngredient)
			;
		}

		UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel, BaseEventData> _callback_PointerEnter;
		UnityAction _callback_PointerExit;
		UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel> _callback_click;

		public void InjectConcreteItemCallbackAction(
			UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel, BaseEventData> pointerEnter,
			UnityAction _PointerExit,
			UnityAction<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel> _click)
		{
			_callback_PointerEnter = pointerEnter;
			_callback_PointerExit = _PointerExit;
			_callback_click = _click;
		}

		public override void ShowThisSubPanel()
		{
			base.ShowThisSubPanel();
			CurrentShowTypes = ShowTypes.None_无;
			CurrentSortType = SortTypeEnum.None_无排列;
		}





		public void ShowContents(ShowTypes showTypes)
		{
			if (!PanelActive)
			{
				ShowThisSubPanel();
			}
			ClearContent();

			// set each CurrentShowTypes to showTypes 
			if (showTypes.HasFlag(ShowTypes.AllEquipment_所有装备))
			{
				ShowInventoryContent_Equipment(GCSOExtend.GetCurrentEquipmentList());
            }
			if (showTypes.HasFlag(ShowTypes.Ingredient_材料))
			{
				ShowInventoryContent_Ingredient(GCSOExtend.GetCurrentItemList());
            }
            if (showTypes.HasFlag(ShowTypes.AllEquipment_所有装备))
            {
                ShowInventoryContent_Equipment(GCSOExtend.GetTmpEquipmentList(), true);
            }
            if (showTypes.HasFlag(ShowTypes.Ingredient_材料))
            {
                ShowInventoryContent_Ingredient(GCSOExtend.GetTmpItemList(), true);
            }

            AppendEmptySlot();
		}



		public void ClearContent()
		{
			foreach (UIRW_PerEquipmentEntryInEquipmentInventorySubPanel perUIRW in _currentAllEquipmentUIRWList)
			{
				Destroy(perUIRW.gameObject);
			}
			_currentAllEquipmentUIRWList.Clear();
		}



		private void AppendEmptySlot()
		{
			var currentCount = _currentAllEquipmentUIRWList.Count;
			if (currentCount <= 56)
			{
				for (int i = currentCount; i < 56; i++)
				{
					GameObject newGO = Instantiate(_prefab_PerEquipmentEntry, _layout_EquipmentInInventory.transform);
					UIRW_PerEquipmentEntryInEquipmentInventorySubPanel newUIRW =
						newGO.GetComponent<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel>();
					_currentAllEquipmentUIRWList.Add(newUIRW);
					newUIRW.InitializeEmpty();
				}
			}
		}

		/// <summary>
		/// 显示所有的装备
		/// </summary>
		public void ShowInventoryContent_Equipment(List<GlobalConfigSO.PlayerEquipmentInfo> sourceList,
			bool isTmpList = false)
		{
			if (sourceList.IsNullOrEmpty()) return;
			var tmpList = sourceList.Where(info => info.EquippedWithCharacter == 0).ToList();
			switch (CurrentSortType)
			{
				case SortTypeEnum.None_无排列:
					break;
				case SortTypeEnum.ByType_EquipmentType_按照装备类型排列:
					tmpList.Sort((x, y) =>
					{
						var xType = x.EquipmentUID;
						var yType = y.EquipmentUID;
						return xType.CompareTo(yType);
					});
					break;
				case SortTypeEnum.ByQuality_按照品质排列:
					tmpList.Sort((x, y) =>
					{
						var xType = GCAHHExtend.GetEquipmentRawInfo(x.EquipmentUID).QualityType;
						var yType = GCAHHExtend.GetEquipmentRawInfo(y.EquipmentUID).QualityType;
						return xType.CompareTo(yType);
					});
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			foreach (var info in tmpList)
			{
				GameObject newGO = Instantiate(_prefab_PerEquipmentEntry, _layout_EquipmentInInventory.transform);
				var newUIRW = newGO.GetComponent<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel>();
				_currentAllEquipmentUIRWList.Add(newUIRW);
				newUIRW.InitializeOnInstantiate(info, _callback_PointerEnter, _callback_PointerExit, _callback_click);
				newUIRW.SetTmpIcon(isTmpList);
			}
		}

		public void ShowInventoryContent_Ingredient(List<GlobalConfigSO.ItemInfo> sourceList,
            bool isTmpList = false)
        {
            if (sourceList.IsNullOrEmpty()) return;
            var tmpList = new List<GlobalConfigSO.ItemInfo>(sourceList);
			switch (CurrentSortType)
			{
				case SortTypeEnum.ByType_EquipmentType_按照装备类型排列:
					tmpList.Sort((x, y) =>
					{
						var xType = x.UID;
						var yType = y.UID;
						return xType.CompareTo(yType);
					});
					break;
				case SortTypeEnum.ByQuality_按照品质排列:
					tmpList.Sort((x, y) =>
					{
						var xType = GCAHHExtend.GetProperty(x.UID).Quality;
						var yType = GCAHHExtend.GetProperty(y.UID).Quality;
						return xType.CompareTo(yType);
					});
					break;
			}
			foreach (var temInfo in tmpList)
			{
				GameObject newGO = Instantiate(_prefab_PerEquipmentEntry, _layout_EquipmentInInventory.transform);
				var newUIRW = newGO.GetComponent<UIRW_PerEquipmentEntryInEquipmentInventorySubPanel>();
				_currentAllEquipmentUIRWList.Add(newUIRW);
				newUIRW.InitializeOnInstantiate(temInfo, _callback_PointerEnter, _callback_PointerExit, _callback_click);
                newUIRW.SetTmpIcon(isTmpList);
            }
		}

#region 调整显示内容

		 [SerializeField,LabelText("button_调整装备显示") , Required, TitleGroup("===Widget===")]
		 private Button _button_AdjustShowEquipment;
		 
		  
		 
		 [SerializeField,LabelText("button_调整卡牌显示") , Required, TitleGroup("===Widget===")]
 		 private Button _button_AdjustShowCharacterCardContent;
		   
		 [SerializeField,LabelText("button_调整材料显示") , Required, TitleGroup("===Widget===")]
		   		 private Button _button_AdjustShowIngredientContent;


		 private void _Button_ToggleToShowEquipment()
		 {
			  if(CurrentShowTypes.HasFlag(ShowTypes.AllEquipment_所有装备))
			  {
				  CurrentShowTypes &= ~ShowTypes.AllEquipment_所有装备;
			  }
			  else
			  {
				  CurrentShowTypes |= ShowTypes.AllEquipment_所有装备;
			  }
			  ShowContents(CurrentShowTypes);
		 }

		 private void _Button_ToggleToShowCard()
		 {
			 //if(CurrentShowTypes.HasFlag(ShowTypes.CharacterCard_角色卡))
			 //{
				// CurrentShowTypes &= ~ShowTypes.CharacterCard_角色卡;
			 //}
			 //else
			 //{
				// CurrentShowTypes |= ShowTypes.CharacterCard_角色卡;
			 //}
			 //ShowContents();
		 }


		 private void _Button_ToggleToShowIngredient()
		 {
			 if (CurrentShowTypes.HasFlag(ShowTypes.Ingredient_材料))
			 {
				 CurrentShowTypes &= ~ShowTypes.Ingredient_材料;
			 }
			 else
			 {
				 CurrentShowTypes |= ShowTypes.Ingredient_材料;
			 }
			 ShowContents(CurrentShowTypes);
		 }

#endregion
	}
}