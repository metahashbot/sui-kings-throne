using System;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Skill.ConcreteSkill.RedFang
{
	[TypeInfoBox("大技能：鲜血武装，猩月黯影")]
	[Serializable]
	public class Skill_RedFang_ScarletDarkness : BaseRPSkill 
	{
		[SerializeField,LabelText("猩月黯影 buff持续时长") , TitleGroup("===配置====")]
		private float _buffDuration = 20f;

		public override bool _Internal_TryPrepareSkill()
		{
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}
			else
			{
				ApplyStrongStoic();
				return true;

			}
			
		}


		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			RemoveStrongStoic();
			var blp_ResetDuration = GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Get();
			blp_ResetDuration.SetAllAsNotLess(_buffDuration);
			var apply = _characterBehaviourRef.ReceiveBuff_TryApplyBuff(
				RolePlay_BuffTypeEnum.FromSkill_RedFang_ScarletDarkness_猩月黯影,
				_characterBehaviourRef,
				_characterBehaviourRef,
				blp_ResetDuration);
			blp_ResetDuration.ReleaseOnReturnToPool();
			
			base._InternalSkillEffect_SkillDefaultTakeEffect();
		}

		protected override void _InternalSkillEffect_SkillDefaultFinishEffect()
		{
			base._InternalSkillEffect_SkillDefaultFinishEffect();
		}





		//
		//
		// [SerializeField, LabelText("vfx_施法准备"), FoldoutGroup("配置", true), GUIColor(187f / 255f, 1f, 0f)]
		// private string _Vfx_prepare;
		//
		// private PerVFXInfo _vfxInfo_Prepare;
		//
		// [SerializeField, LabelText("vfx_爆发"), FoldoutGroup("配置", true), GUIColor(187f / 255f, 1f, 0f)]
		// private string _vfx_release;
		//
		// [SerializeField, LabelText("事件_准备"), FoldoutGroup("配置", true)]
		// private SOConfig_PrefabEventConfig _event_Prepare;
		//
		// [SerializeField, LabelText("事件_爆发"), FoldoutGroup("配置", true)]
		// private SOConfig_PrefabEventConfig _event_release;
		//
		// private AnimationInfoBase _sai_Release;
		//
		//
		// [SerializeField,InlineEditor(InlineEditorObjectFieldModes.Boxed),
		//  LabelText("Buff模板引用——效果Buff"), FoldoutGroup("配置", true)]
		// public SOConfig_RPBuff BuffConfigRef;
		//
		//
		// [SerializeField,LabelText("调整残影间隔为多少帧")]
		// public int GhostInterval = 4;
		//
		//
		//
		// private enum SkillStateTypeEnum
		// {
		// 	None_无事发生 = 0,
		// 	Releasing_释放中 =1,
		// 	TakingEffect_生效中 = 2,
		// }
		//
		// private SkillStateTypeEnum _skillState = SkillStateTypeEnum.None_无事发生;
		//
		// private Buff_ScarletDarkness _buff_ScarletDarknessRef;
		//
		//
		// protected FloatPresentValue_RPDataEntry _entry_CurrentUP;
		// public override void InitOnObtain(
		// 	RPSkill_SkillHolder skillHolderRef,
		// 	SOConfig_RPSkill configRuntimeInstance,
		// 	I_RP_ObjectCanReleaseSkill parent,
		// 	SkillSlotTypeEnum slot)
		// {
		// 	base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
		//
		// 	_playerControllerRef = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
		// 	_characterBehaviourRef = parent as PlayerARPGConcreteCharacterBehaviour;
		// 	_characterArtHelperRef = _characterBehaviourRef.GetSelfRolePlayArtHelper() as PlayerARPGArtHelper;
		// 	// _sai_Release = GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
		// 	// 	._AN_SkillReleaseSpineAnimationName);
		//
		//
		// 	_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved,
		// 		_ABC_ResetSkillStateOnBuffRemoved);
		// 	_entry_CurrentUP = _characterBehaviourRef.ReleaseSkill_GetPresentDataEntry(RP_DataEntry_EnumType
		// 		.CurrentUP_当前UP);
		// }
		//
		//
		// public override SkillReadyTypeEnum GetSkillReadyType()
		// {
		// 	var type = base.GetSkillReadyType();
		// 	if (_entry_CurrentUP.CurrentValue < 100f)
		// 	{
		// 		return SkillReadyTypeEnum.BlockByCD;
		// 	}
		// 	if (type != SkillReadyTypeEnum.Ready)
		// 	{
		// 		if (_buff_ScarletDarknessRef != null && _buff_ScarletDarknessRef.GetBuffCurrentAvailableType() ==
		// 			BuffAvailableType.TimeInButNotMeetOtherRequirement)
		// 		{
		// 			return SkillReadyTypeEnum.Using_还在使用;
		// 		}
		// 	}
		// 	return type;
		// }
		//
		//
		// protected override void BindingInput()
		// {
		// 	if (SkillSlot == SkillSlotTypeEnum.None_未装备)
		// 	{
		// 		return;
		// 	}
		//
		// 	InputAction ia = GetTargetInputActionRef(SkillSlot);
		// 	ia.performed += _IC_ReceiveSkillInput_NormalPerformed;
		// }
		//
		// protected override void UnbindInput()
		// {
		// 	if (SkillSlot == SkillSlotTypeEnum.None_未装备)
		// 	{
		// 		return;
		// 	}
		//
		// 	InputAction ia = GetTargetInputActionRef(SkillSlot);
		// 	ia.performed -= _IC_ReceiveSkillInput_NormalPerformed;
		// }
		//
		//
		// public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
		// {
		// 	base.UpdateTick(currentTime, currentFrameCount, delta);
		//
		// 	if (_ghostEffectActive)
		// 	{
		// 		_characterArtHelperRef.SetGhostPosition(5, GhostInterval);
		//
		//
		// 	}
		// }
		//
		//
		//
		// private bool _ghostEffectActive = false;
		// private void ActivateGhost()
		// {
		// 	if(!_ghostEffectActive)
		// 	{
		// 		_ghostEffectActive = true;
		// 		_characterArtHelperRef.ActivateGhostEffect();
		// 	}
		// }
		// private void DeactivateGhost()
		// {
		// 	if (_ghostEffectActive)
		// 	{
		// 		_ghostEffectActive = false;
		// 		_characterArtHelperRef.DeactivateGhostEffect();
		// 	}
		// }
		//
		//
		//
		// protected virtual void _IC_ReceiveSkillInput_NormalPerformed(InputAction.CallbackContext context)
		// {
		// 	//不是自己，无事发生
		// 	if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
		// 	{
		// 		return;
		// 	}
		//
		// 	/*
		// 	 * 如果当前技能就是活跃的，则表明快速施法
		// 	 */
		// 	OnSkillSlotReceiveInput();
		//
		// 	switch (_skillState)
		// 	{
		// 		case SkillStateTypeEnum.None_无事发生:
		//
		// 			if (_buff_ScarletDarknessRef != null && _buff_ScarletDarknessRef.GetBuffCurrentAvailableType() !=
		// 				BuffAvailableType.NotExist)
		// 			{
		// 				return;
		// 			}
		//
		// 			if (!IfReactToInput())
		// 			{
		// 				return;
		// 			}
		// 			if (!CheckIfDataEntryEnough())
		// 			{
		// 				return;
		// 			}
		//
		// 			if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_Release.OccupationInfo))
		// 			{
		// 				return;
		// 			}
		//
		// 			//到这里了，开始准备
		// 			OnSkillBeginPrepare();
		// 			break;
		// 		case SkillStateTypeEnum.Releasing_释放中:
		// 			break;
		// 		case SkillStateTypeEnum.TakingEffect_生效中:
		// 			break;
		// 	}
		//
		//
		// }
		//
		//
		// // protected override void _ABC_OnSkillAnimationGeneralTakeEffect(DS_ActionBusArguGroup ds)
		// // {
		// // 	base._ABC_OnSkillAnimationGeneralTakeEffect(ds);
		// //
		// // 	if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
		// // 	{
		// // 		return;
		// // 	}
		// // 	var aniConfig = ds.ObjectArguStr as string;
		// // 	if (aniConfig.Equals(_sai_Release.ConfigName, StringComparison.OrdinalIgnoreCase))
		// // 	{
		// // 		ProcessSkillTakeEffect();
		// // 	}
		// // }
		// // public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupySourceInfo)
		// // {
		// // 	var fixedType = GetFixedOccupiedCancelType(occupySourceInfo);
		// // 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupySourceInfo);
		// // 	switch (_skillState)
		// // 	{
		// // 		case SkillStateTypeEnum.None_无事发生:
		// // 			break;
		// // 		case SkillStateTypeEnum.Releasing_释放中:
		// // 			switch (fixedType)
		// // 			{
		// // 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
		// // 					if (!isSelf)
		// // 					{
		// // 						C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
		// // 					}
		// // 					break;
		// // 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
		// // 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
		// // 					break;
		// // 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
		// // 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
		// // 					break;
		// // 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
		// // 					_skillState = SkillStateTypeEnum.None_无事发生;
		// // 					DeactivateGhost();
		// // 					VFX_GeneralClear();
		// // 					_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum
		// // 						.FromSkill_RedFang_ScarletDarkness_猩月黯影);
		// // 					break;
		// // 				default:
		// // 					throw new ArgumentOutOfRangeException();
		// // 			}
		// // 			break;
		// // 		case SkillStateTypeEnum.TakingEffect_生效中:
		// // 			switch (fixedType)
		// // 			{
		// // 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
		// // 					if(!isSelf)
		// // 					{
		// // 						if (occupySourceInfo.OccupationInfoConfigName.Contains("入场",
		// // 							    StringComparison.OrdinalIgnoreCase) ||
		// // 						    occupySourceInfo.OccupationInfoConfigName.Contains("离场",
		// // 							    StringComparison.OrdinalIgnoreCase))
		// // 						{
		// // 							_skillState = SkillStateTypeEnum.None_无事发生;
		// // 							DeactivateGhost();
		// // 							VFX_GeneralClear();
		// // 							_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum
		// // 								.FromSkill_RedFang_ScarletDarkness_猩月黯影);
		// // 						}
		// // 					}
		// // 					break;
		// // 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
		// // 					break;
		// // 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
		// // 					_skillState = SkillStateTypeEnum.None_无事发生;
		// // 					DeactivateGhost();
		// // 					VFX_GeneralClear();
		// // 					_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum
		// // 						.FromSkill_RedFang_ScarletDarkness_猩月黯影);
		// // 					break;
		// // 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
		// // 					_skillState = SkillStateTypeEnum.None_无事发生;
		// // 					DeactivateGhost();
		// // 					VFX_GeneralClear();
		// // 					_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum
		// // 						.FromSkill_RedFang_ScarletDarkness_猩月黯影);
		// // 					break;
		// // 				default:
		// // 					throw new ArgumentOutOfRangeException();
		// // 			}
		// // 			break;
		// // 		default:
		// // 			throw new ArgumentOutOfRangeException();
		// // 	}
		// // }
		// public override void Clear_FullClearAllSkillContentImmediate()
		// {
		// 	(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
		// 	(this as I_SkillContainResistToAbnormal).RemoveResist_SkillVersion();
		// 	VFX_GeneralClear(true);
		// }
		// public override void Clear_PartialClearNotImmediate()
		// {
		// 	(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
		// 	(this as I_SkillContainResistToAbnormal).RemoveResist_SkillVersion();
		// 	VFX_GeneralClear();
		// }
		//
		// protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
		// {
		// 	var ds = base.OnSkillBeginPrepare(autoLaunch);
		//
		// 	(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
		// 	 (this as I_SkillContainResistToAbnormal).AddResist_SkillVersion();
		// 	_skillState = SkillStateTypeEnum.Releasing_释放中;
		// 	var ds_ani = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
		// 		.L_Skill_RequireAnimationByAnimationInfo_技能要求动画操作);
		// 	ds_ani.ObjectArgu1 = _sai_Release;
		// 	_selfActionBusRef.TriggerActionByType(ds_ani);
		//
		//
		// 	_vfxInfo_Prepare = _VFX_GetOrInstantiateNew(_Vfx_prepare)?._VFX__10_PlayThis();
		//
		// 	if (_event_Prepare != null)
		// 	{
		// 		GameplayEventManager.Instance.StartGameplayEvent(_event_Prepare);
		// 	}
		//
		// 	return ds;
		// }
		//
		// /// <summary>
		// /// <para>技能在此生效，施加Buff，清除UP等</para>
		// /// </summary>
		// protected void ProcessSkillTakeEffect()
		// {
		// 	(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
		// 	 (this as I_SkillContainResistToAbnormal).AddResist_SkillVersion();
		//
		//
		// 	_entry_CurrentUP.ResetDataToValue(0f);
		// 	if (_characterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum
		// 		.FromSkill_RedFang_ScarletDarkness_猩月黯影) != BuffAvailableType.NotExist)
		// 	{
		// 		DBug.LogError($"技能猩月黯影在试图添加buff的时候，发现之前的buff没清掉，这不合理");
		// 		return;
		// 	}
		// 	_vfxInfo_Prepare?.VFX_StopThis();
		// 	_VFX_GetOrInstantiateNew(_vfx_release)?._VFX__10_PlayThis();
		// 	if (_event_release != null)
		// 	{
		// 		GameplayEventManager.Instance.StartGameplayEvent(_event_release);
		// 	}
		// 	
		// 	
		// 	BuffApplyResultEnum buffApply = _characterBehaviourRef.ReceiveBuff_TryApplyBuff(
		// 		RolePlay_BuffTypeEnum.FromSkill_RedFang_ScarletDarkness_猩月黯影,
		// 		_characterBehaviourRef,
		// 		_characterBehaviourRef);
		// 	_buff_ScarletDarknessRef =
		// 		_characterBehaviourRef.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum
		// 			.FromSkill_RedFang_ScarletDarkness_猩月黯影) as Buff_ScarletDarkness;
		// 	
		//
		// }
		//
		//
		// protected override bool IfSkillCanCDTick()
		// {
		// 	if (_skillState != SkillStateTypeEnum.None_无事发生)
		// 	{
		// 		return false;
		// 	}
		// 	if (_buff_ScarletDarknessRef != null && _buff_ScarletDarknessRef.GetBuffCurrentAvailableType() ==
		// 		BuffAvailableType.Available_TimeInAndMeetRequirement)
		// 	{
		// 		return false;
		// 	}
		// 	return true;
		// }
		//
		//
		// private void _ABC_ResetSkillStateOnBuffRemoved(DS_ActionBusArguGroup ds)
		// {
		// 	var buff = ds.ObjectArgu1 as BaseRPBuff;
		// 	if (buff is Buff_ScarletDarkness s)
		// 	{
		// 		_buff_ScarletDarknessRef = null;
		// 		// _Internal_BroadcastSkillReleaseFinish();
		// 		_skillState = SkillStateTypeEnum.None_无事发生;
		// 		DeactivateGhost();
		// 		Clear_PartialClearNotImmediate();
		// 	}
		// }
		//
		// protected override void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
		// {
		// 	// base._ABC_OnGeneralAnimationComplete(ds);
		// 	// var configName = ds.ObjectArguStr as string;
		// 	// if (configName.Equals(_sai_Release.ConfigName, StringComparison.OrdinalIgnoreCase))
		// 	// {
		// 	// 	_skillState = SkillStateTypeEnum.TakingEffect_生效中;
		// 	// 	ActivateGhost();
		// 	// }
		// }
		//
		// protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
		// {
		// 	var str = oInfo.OccupationInfoConfigName;
		// 	if (str.Equals(_sai_Release.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase))
		// 	{
		// 		return true;
		// 	}
		// 	else
		// 	{
		// 		return false;
		// 		
		// 	}
		// }
		//
		// public override DS_ActionBusArguGroup ClearBeforeRemove()
		// {
		// 	_selfActionBusRef.RemoveAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved,
		// 		_ABC_ResetSkillStateOnBuffRemoved);
		// 	return base.ClearBeforeRemove();
		// }
	}
}