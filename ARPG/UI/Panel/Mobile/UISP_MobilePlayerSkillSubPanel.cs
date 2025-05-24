using System.Collections.Generic;
using ARPG.Character;
using Global;
using Global.ActionBus;
using Global.UIBase;
using RPGCore.Skill;
using RPGCore.Skill.ConcreteSkill;
using RPGCore.Skill.Config;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARPG.UI.Panel
{
	/// <summary>
	/// 这个是手游版的一个特定的技能输入面板，用于显示4技能玩家的技能槽位，以及技能的CD状态
	/// 考虑到手机技能面板基本不会出现非特定情况，出现了都需要重新设计，比如5技能，衍生技能等等
	/// 因此这个面板不会做成通用的，角色会选择自己的手游技能面板，更换角色时会自己切换自己的技能面板
	/// </summary>
	public class UISP_MobilePlayerSkillSubPanel : UI_UIBaseSubPanel
	{

		
		//
		//
		//
		//
		//
		// [SerializeField, Required, LabelText("Holder_突进槽位"), FoldoutGroup("配置", true), TitleGroup("配置/位移技能")]
		// private GameObject _holder_DisplacementSkill;
		//
		// //button
		// [SerializeField, Required, LabelText("突进槽位按钮"), FoldoutGroup("配置", true), TitleGroup("配置/位移技能")]
		// private Button Button_DisplacementSkillButton;
		//
		// [SerializeField, Required, LabelText("突进槽位图片"), FoldoutGroup("配置", true), TitleGroup("配置/位移技能")]
		// private Image DisplacementSkillImage;
		//
		// [SerializeField, Required, LabelText("突进槽位CD覆盖"), FoldoutGroup("配置", true), TitleGroup("配置/位移技能")]
		// private Image DisplacementSkillCDCoverImage;
		//
		//
		// private SOConfig_RPSkill _skillRuntime_Displacement;
		//
		//
		//
		//
		// [SerializeField, Required, LabelText("Holder_技能1 槽位"), FoldoutGroup("配置", true), TitleGroup("配置/技能1")]
		// private GameObject _holder_Skill1Skill;
		//
		// //button
		// [SerializeField, Required, LabelText("小技能1槽位按钮"), FoldoutGroup("配置", true), TitleGroup("配置/技能1")]
		// private Button Button_Normal1SkillButton;
		//
		//
		// [SerializeField, Required, LabelText("小技能1槽位图片"), FoldoutGroup("配置", true), TitleGroup("配置/技能1")]
		// private Image Normal1SkillImage;
		//
		// [SerializeField, Required, LabelText("小技能1槽位CD覆盖"), FoldoutGroup("配置", true), TitleGroup("配置/技能1")]
		// private Image Normal1SkillCDCoverImage;
		//
		// private SOConfig_RPSkill _skillRuntime_Skill1;
		//
		//
		// [SerializeField, Required, LabelText("Holder_技能2 槽位"), FoldoutGroup("配置", true), TitleGroup("配置/技能2")]
		// private GameObject _holder_Skill2Skill;
		//
		// //button
		// [SerializeField, Required, LabelText("小技能2槽位按钮"), FoldoutGroup("配置", true), TitleGroup("配置/技能2")]
		// private Button Button_Normal2SkillButton;
		//
		//
		// [SerializeField, Required, LabelText("小技能2槽位图片"), FoldoutGroup("配置", true), TitleGroup("配置/技能2")]
		// private Image Normal2SkillImage;
		//
		// [SerializeField, Required, LabelText("小技能2槽位CD覆盖"), FoldoutGroup("配置", true), TitleGroup("配置/技能2")]
		// private Image Normal2SkillCDCoverImage;
		//
		// private SOConfig_RPSkill _skillRuntime_Skill2;
		//
		//
		//
		// [SerializeField, Required, LabelText("Holder_技能UP 槽位"), FoldoutGroup("配置", true), TitleGroup("配置/UP")]
		// private GameObject _holder_SkillUPSkill;
		//
		// //button
		// [SerializeField, Required, LabelText("技能UP槽位按钮"), FoldoutGroup("配置", true), TitleGroup("配置/UP")]
		// private Button Button_UniqueSkillButton;
		//
		//
		// [SerializeField, Required, LabelText("绝招槽位图片"), FoldoutGroup("配置", true), TitleGroup("配置/UP")]
		// private Image UniqueSkillImage;
		//
		// [SerializeField, Required, LabelText("绝招槽位CD覆盖"), FoldoutGroup("配置", true), TitleGroup("配置/UP")]
		// private Image UniqueSkillCDCoverImage;
		// [SerializeField, Required, LabelText("大招冷却好了外发光"), FoldoutGroup("配置", true), TitleGroup("配置/UP")]
		// private GameObject _selfUniqueSkillReadyImage;
		//
		// private SOConfig_RPSkill _skillRuntime_UniqueSkill;
		//
		//
		//
		//
		// [SerializeField, Required, LabelText("按钮_普通攻击"), FoldoutGroup("配置", true), TitleGroup("配置/普攻")]
		// private Button Button_NormalAttackButton;
		//
		//
		// [SerializeField, Required, LabelText("普通攻击槽位图片"), FoldoutGroup("配置", true), TitleGroup("配置/普攻")]
		// private Image NormalAttackImage;
		//
		//
		// private List<SOConfig_RPSkill> _currentCharacterSkillList = new List<SOConfig_RPSkill>();
		// private PlayerARPGConcreteCharacterBehaviour _currentActivePlayerBehaviourRef;
		// private RPSkill_SkillHolder _currentActivePlayerSkillHolderRef;
		//
		// public override void StartInitializeBySP(UI_UIBasePanel parentUIP)
		// {
		// 	base.StartInitializeBySP(parentUIP);
		//
		// 	// Button_DisplacementSkillButton.image.alphaHitTestMinimumThreshold = 0.3f;
		// 	// Button_Normal1SkillButton.image.alphaHitTestMinimumThreshold = 0.3f;
		// 	// Button_Normal2SkillButton.image.alphaHitTestMinimumThreshold = 0.3f;
		// 	// Button_UniqueSkillButton.image.alphaHitTestMinimumThreshold = 0.3f;
		// 	// Button_NormalAttackButton.image.alphaHitTestMinimumThreshold = 0.3f;
		// 	
		//
		// }
		//
		// protected override void BindingEvents()
		// {
		// 	GlobalActionBus.GetGlobalActionBus().RegisterAction(
		// 		ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
		// 		_ABC_RefreshSkillButton_OnCurrentActiveCharacterChanged);
		// }
		//
		//
		// /// <summary>
		// /// 清理当前已经存在的技能们，如果有的话
		// /// </summary>
		// private void ClearCurrentSkillSlots()
		// {
		// 	_currentCharacterSkillList.Clear();
		// 	_holder_DisplacementSkill.gameObject.SetActive(false);
		// 	_holder_Skill1Skill.gameObject.SetActive(false);
		// 	_holder_Skill2Skill.gameObject.SetActive(false);
		// 	_holder_SkillUPSkill.gameObject.SetActive(false);
		// }
		//
		// private void BuildSkillSlotsByList()
		// {
		// 	//这里是因为现在位置排列不同，会导致技能不同，这样技能会有不符合设计的版面，所以需要找一下对应的技能
		// 	var index1 = _currentCharacterSkillList.FindIndex(x => x.ConcreteSkillFunction.SkillSlot ==
		// 	                                                       SkillSlotTypeEnum.DisplacementSkill_位移槽位);
		// 	if (index1 != -1)
		// 	{
		// 		_holder_DisplacementSkill.gameObject.SetActive(true);
		// 		_skillRuntime_Displacement = _currentCharacterSkillList[index1];
		// 		if (_skillRuntime_Displacement.ConcreteSkillFunction.GetCurrentSprite() != null)
		// 		{
		// 			DisplacementSkillImage.sprite = _skillRuntime_Displacement.ConcreteSkillFunction.GetCurrentSprite();
		// 		}
		// 		
		// 	}
		//
		// 	var index2 = _currentCharacterSkillList.FindIndex(x => x.ConcreteSkillFunction.SkillSlot ==
		// 	                                                       SkillSlotTypeEnum.SlotNormal1_常规槽位1);
		// 	if (index2 != -1)
		// 	{
		// 		_holder_Skill1Skill.gameObject.SetActive(true);
		// 		_skillRuntime_Skill1 = _currentCharacterSkillList[index2];
		// 		if (_skillRuntime_Skill1.ConcreteSkillFunction.GetCurrentSprite() != null)
		// 		{
		// 			Normal1SkillImage.sprite = _skillRuntime_Skill1.ConcreteSkillFunction.GetCurrentSprite();
		// 		}
		// 	}
		//
		// 	var index3 = _currentCharacterSkillList.FindIndex(x => x.ConcreteSkillFunction.SkillSlot ==
		// 	                                                       SkillSlotTypeEnum.SlotNormal2_常规槽位2);
		// 	if (index3 != -1)
		// 	{
		// 		_holder_Skill2Skill.gameObject.SetActive(true);
		// 		_skillRuntime_Skill2 = _currentCharacterSkillList[index3];
		// 		if (_skillRuntime_Skill2.ConcreteSkillFunction.GetCurrentSprite() != null)
		// 		{
		// 			Normal2SkillImage.sprite = _skillRuntime_Skill2.ConcreteSkillFunction.GetCurrentSprite();
		// 		}
		// 	}
		// 	
		// 	
		//
		// 	var index4 = _currentCharacterSkillList.FindIndex(x => x.ConcreteSkillFunction.SkillSlot ==
		// 	                                                       SkillSlotTypeEnum.UltraSkill_超杀槽位);
		// 	if (index4 != -1)
		// 	{
		// 		_holder_SkillUPSkill.gameObject.SetActive(true);
		// 		_skillRuntime_UniqueSkill = _currentCharacterSkillList[index4];
		// 		if (_skillRuntime_UniqueSkill.ConcreteSkillFunction.GetCurrentSprite() != null)
		// 		{
		// 			UniqueSkillImage.sprite = _skillRuntime_UniqueSkill.ConcreteSkillFunction.GetCurrentSprite();
		// 		}
		// 	}
		//
		// }
		// private void _ABC_RefreshSkillButton_OnCurrentActiveCharacterChanged(DS_ActionBusArguGroup ds)
		// {
		// 	ClearCurrentSkillSlots();
		// 	
		// 	_currentActivePlayerBehaviourRef = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;
		// 	_currentActivePlayerSkillHolderRef = _currentActivePlayerBehaviourRef.GetRelatedSkillHolder();
		// 	_currentActivePlayerSkillHolderRef.GetCurrentSkillList(_currentCharacterSkillList);
		// 	BuildSkillSlotsByList();
		//
		//
		//
		// 	NormalAttackImage.sprite =
		// 		_currentActivePlayerBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
		// 			.WeaponNormalAttackIcon ? _currentActivePlayerBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance
		// 			.WeaponFunction.WeaponNormalAttackIcon : null;
		//
		// }
		//
		// public override void UpdateTickByParentPanel(float currentTime, int currentFrame, float delta)
		// {
		// 	// DisplacementSkillCDCoverImage.fillAmount = fillCDCover(_skillRuntime_Displacement);
		// 	// Normal1SkillCDCoverImage.fillAmount = fillCDCover(_skillRuntime_Skill1);
		// 	// Normal2SkillCDCoverImage.fillAmount = fillCDCover(_skillRuntime_Skill2);
		// 	//
		// 	// // 这里目前是读CD，之后要改成读玩家的UP值
		// 	// var upPartial = (_skillRuntime_UniqueSkill.ConcreteSkillFunction as I_SkillAsUltraSkill).GetUltraPartial();
		// 	// if (Mathf.Approximately(upPartial,1f))
		// 	// {
		// 	// 	_selfUniqueSkillReadyImage.SetActive(true);
		// 	// 	Button_UniqueSkillButton.interactable = true;
		// 	// }
		// 	// else
		// 	// {
		// 	// 	_selfUniqueSkillReadyImage.SetActive(false);
		// 	// 	Button_UniqueSkillButton.interactable = false;
		// 	// }
		//
		// 	
		// }
		//
		// private float fillCDCover(SOConfig_RPSkill _selfRelatedSkillConfigRuntime)
		// {
		// 	if (_selfRelatedSkillConfigRuntime == null)
		// 	{
		// 		return 1f;
		// 	}
		// 	float remainingCDPartial =
		// 		_selfRelatedSkillConfigRuntime.ConcreteSkillFunction.RemainingCoolDownDuration /
		// 		_selfRelatedSkillConfigRuntime.ConcreteSkillFunction.CurrentCoolDownDuration;
		// 	if (Mathf.Approximately(remainingCDPartial, 0f))
		// 	{
		// 		SkillReadyTypeEnum state = _selfRelatedSkillConfigRuntime.ConcreteSkillFunction.GetSkillReadyType();
		// 		if (state != SkillReadyTypeEnum.Ready)
		// 		{
		// 			remainingCDPartial = 1f;
		// 		}
		// 	}
		// 	return remainingCDPartial;
		// }
	}
}