using System;
using System.Collections;
using System.Collections.Generic;
using ARPG.Equipment;
using Global;
using Global.Loot;
using Global.UIBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIRW_CharacterPerk_PlayerCharacterFullPanel : UI_UISingleRuntimeWidget
{
	
	
	
	
	[LabelText("text_天赋名称"), SerializeField, Required, TitleGroup("===Widget===")]
	private TMPro.TextMeshProUGUI _text_CharacterPerkName;
	
	[LabelText("image 天赋底图"), SerializeField, Required, TitleGroup("===Widget===")]
	private UnityEngine.UI.Image _image_CharacterPerkIcon;
	
	
	public string RelatedPerkName { get; private set; }

	private UnityAction<string, BaseEventData> _callback_OnPointerEnter;

	private UnityAction _callback_OnPointerExit;


	public SOFE_CharacterPerkCollection.PerTypeInfo RelatedPerkInfoRef { get; private set; }
	
	public void Init(string perkName, UnityAction<string, BaseEventData> callback_OnPointerEnter, UnityAction callback_OnPointerExit)
	{
		RelatedPerkName = perkName;
		RelatedPerkInfoRef = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_CharacterPerkInfo
			.GetPerkInfoByName(perkName);
		
		
		
		_callback_OnPointerEnter = callback_OnPointerEnter;
		_callback_OnPointerExit = callback_OnPointerExit;
		_text_CharacterPerkName.text = GlobalConfigurationAssetHolderHelper.GetCurrentBaseGRS()
			.GetLocalizedStringByTableAndKey(LocalizationTableNameC._LCT_CharacterPerkDesc, perkName);
		
		_image_CharacterPerkIcon.sprite = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_GeneralSpriteResource
			.GetSpriteByUID(GetPerkSpriteName());


	}



	public void _ET_PointerEnter(BaseEventData baseEventData)
	{

		_callback_OnPointerEnter.Invoke(RelatedPerkName, baseEventData);

	}

	public void _ET_PointerExit(BaseEventData baseEventData)
	{
		_callback_OnPointerExit.Invoke();

	}


	private string GetPerkSpriteName()
	{
		switch (RelatedPerkInfoRef.Quality)
		{
			case EquipmentQualityTypeEnum.None_未指定:
				return "天赋框灰";
			case EquipmentQualityTypeEnum.White_白色:
				return "天赋框白";

			case EquipmentQualityTypeEnum.Green_绿色:
				return "天赋框绿";
				break;
			case EquipmentQualityTypeEnum.Blue_蓝色:
				return "天赋框蓝";
				break;
			case EquipmentQualityTypeEnum.Purple_紫色:
				return "天赋框紫";
				break;
			case EquipmentQualityTypeEnum.Gold_金色:
				return "天赋框金";
				break;
			case EquipmentQualityTypeEnum.Red_红色:
				return "天赋框红";
				break;
			 
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}
