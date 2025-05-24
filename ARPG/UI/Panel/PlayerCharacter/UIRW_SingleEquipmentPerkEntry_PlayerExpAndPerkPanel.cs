using System.Collections;
using System.Collections.Generic;
using ARPG.UI.Panel.PlayerCharacter;
using Global;
using Global.UIBase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel : UI_UISingleRuntimeWidget
{
	[LabelText("Image_刻印本体Icon"), SerializeField, Required, TitleGroup("===Widget===")]
	private UnityEngine.UI.Image _image_Icon;
	
	[LabelText("Text_刻印本体名称"), SerializeField, Required, TitleGroup("===Widget===")]
	private TextMeshProUGUI _text_Name;

	[LabelText("刻印等级的Image容器"),SerializeField, Required, TitleGroup("===Widget===")]
	private List<Image> _list_IconLevelMarkerList = new List<Image>();

	[LabelText("text_溢出等级文本"), SerializeField, Required, TitleGroup("===Widget===")]
	private TextMeshProUGUI _text_OverflowLevel;
	
	
	public string PerkUID { get; private set; }
	public int PerkLevel { get; private set; }

	private static UnityAction<UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel, BaseEventData>
		_callback_ShowPerkDetail_OnHover;
	private static UnityAction _callback_HideDetail;

	private static UISP_PCF_PlayerExpAndPerkPanel _parentPanelRef;


	public void InstantiateInitialize(
		string uid,
		int level,
		UISP_PCF_PlayerExpAndPerkPanel parent,
		UnityAction<UIRW_SingleEquipmentPerkEntry_PlayerExpAndPerkPanel, BaseEventData> callback_ShowPerkDetail_OnHover,
		UnityAction callback_HideDetail)
	{
		_callback_ShowPerkDetail_OnHover = callback_ShowPerkDetail_OnHover;
		_callback_HideDetail = callback_HideDetail;
		_parentPanelRef = parent;
		PerkUID = uid;
		PerkLevel = level;
		_text_OverflowLevel.gameObject.SetActive(false);


		_image_Icon.sprite = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_BuffAndPerkResource
			.GetSpriteByUID(PerkUID);
		int finalLevel = PerkLevel > 5 ? 5 : PerkLevel;
		string nameText1=  GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS().GetLocalizedStringByTableAndKey(LocalizationTableNameC._LC_PerkText, PerkUID);
		_text_Name.text = $"{nameText1}{finalLevel}";
		if (PerkLevel > 5)
		{
			_text_OverflowLevel.gameObject.SetActive(true);
			_text_OverflowLevel.text = $"+{PerkLevel - 5}";
		}

		for (int i = 0 ; i < _list_IconLevelMarkerList.Count; i++)
		{
			if (i < finalLevel)
			{
				_list_IconLevelMarkerList[i].sprite = parent._sprite_ActivePerkLevel;

			}
			else
			{
				_list_IconLevelMarkerList[i].sprite = parent._sprite_InactivePerkLevel;
			}
		}

	}
	


	  
	  
	public void _ET_OnPointerEnter(BaseEventData baseEventData)
	{
		_callback_ShowPerkDetail_OnHover?.Invoke(this, baseEventData);

	}

	public void _ET_OnPointerExit(BaseEventData baseEventData)
	{
		_callback_HideDetail.Invoke();
	}



}
