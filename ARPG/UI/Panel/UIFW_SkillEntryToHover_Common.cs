using System.Collections;
using System.Collections.Generic;
using Global;
using Global.Character;
using Global.GlobalConfig;
using Global.Loot;
using Global.UI;
using Global.UIBase;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[TypeInfoBox("一个技能槽位，常常用于悬停显示详情")]
public class UIFW_SkillEntryToHover_Common : UI_UISingleRuntimeWidget
{
	[LabelText("关联的技能槽位")]
	public SkillSlotTypeEnum SelfSkillSlot;



	[LabelText("image_本体边框"), SerializeField, Required, TitleGroup("===Widget===")]
	private UnityEngine.UI.Image _image_selfBorder;

	[LabelText("image_本体Icon"), SerializeField, Required, TitleGroup("===Widget===")]
	private UnityEngine.UI.Image _image_selfIcon;


	private UnityAction<int, SkillSlotTypeEnum, RPSkill_SkillTypeEnum,BaseEventData> _callback_HoverEnter;
	
	private UnityAction _callback_HoverExit;

	private RPSkill_SkillTypeEnum _selfSkillType;


	public CIC_PlayerSkillInfo.PlayerSkillInfoEntry RelatedSkillInfoRef { get; private set; }


	public int RelatedPlayerCharacterID { get; private set; }

	/// <summary>
	/// <para>将于 SP_PCF_CurrentCharacterStatePanel 的 RefreshCurrentPlayerSkill 进行。 会根据自己身上配置的槽位来设置自己的图片和UI交互信息</para>
	/// </summary>
	public void InstantiateInitialize(
		int cid,
		UnityAction<int, SkillSlotTypeEnum, RPSkill_SkillTypeEnum, BaseEventData> callbackHoverEnter,
		UnityAction callbackHoverExit)
	{
		RelatedPlayerCharacterID = cid;
		_callback_HoverExit = callbackHoverExit;
		_callback_HoverEnter = callbackHoverEnter;

		var runtimeCharacter = GlobalConfigSO.RuntimeContent().AllCharacterInfoCollection.
			Find((info => info.CharacterID == cid));
        var runtimeWeapon = GlobalConfigSO.RuntimeContent().CurrentEquipmentInfoList.
			Find((info => info.EquippedWithCharacter == cid &&  info.IfIsEquipmentIsWeapon()));
        var weaponInfo = GCAHHExtend.GetEquipmentRawInfo(runtimeWeapon.EquipmentUID) as SOFE_WeaponInfo.PerTypeInfo;
        if (SelfSkillSlot == SkillSlotTypeEnum.NormalAttack_是普攻)
		{
			_selfSkillType = weaponInfo.EquivalentSkillEnum;
		}
		else
		{
			var k = runtimeCharacter.GetComponent<CIC_PlayerSkillInfo>();
			RelatedSkillInfoRef = k.ObtainedSkillList.Find((entry => entry.SkillSlot == SelfSkillSlot));
			_selfSkillType = RelatedSkillInfoRef.SkillType;
			var findSkillConfig = RelatedSkillInfoRef.GetSkillByConfig();
			_image_selfBorder.sprite = BaseUIManager.QuickGetSkillBorder(cid, RelatedSkillInfoRef);
			_image_selfIcon.sprite = findSkillConfig.ConcreteSkillFunction.GetCurrentSprite(weaponInfo.DamageType);
		}
	}
	
	
	public void _ET_ShowSkillDetail_OnPointerEnter(BaseEventData ed)
	{
		_callback_HoverEnter?.Invoke(RelatedPlayerCharacterID, SelfSkillSlot,_selfSkillType , ed);
	}
	
	
	public void _ET_HideSkillDetail_OnPointerExit(BaseEventData ed)
	{
		_callback_HoverExit?.Invoke();
	}
	
	
	

}