// using System;
// using ARPG.Character;
// using ARPG.Character.Base;
// using ARPG.Character.Base.CustomSpineData;
// using ARPG.Manager;
// using Global.ActionBus;
// using RPGCore.Buff.BuffHolder;
// using RPGCore.Buff.ConcreteBuff.Skill.Swordman;
// using RPGCore.Interface;
// using RPGCore.Skill.Config;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using RPGCore.Buff;
// using RPGCore.Buff.ConcreteBuff;
// using RPGCore.UtilityDataStructure;
//
// namespace RPGCore.Skill.ConcreteSkill
// {
// 	/// <summary>
// 	/// <para>剑士剑舞A1</para>
// 	/// <para></para>
// 	/// </summary>
// 	[Serializable]
// 	public class Skill_Swordman_BladeDanceA1 : BaseRPSkill  
// 	{
// 		[SerializeField, LabelText("每秒消耗的SP"), FoldoutGroup("配置", true)]
// 		public float SPConsumePerSecond = 1f;
//
// 		[SerializeField, LabelText("技能持续时长"), FoldoutGroup("配置", true)]
// 		public float SkillDuration = 10f;
//
// 		[SerializeField, LabelText("减免伤害的攻击力倍率"), FoldoutGroup("配置", true)]
// 		public float DamageReduce_FromAttackRatio = 0.5f;
//
//
// 		[SerializeField, LabelText("减免伤害的基础值"), FoldoutGroup("配置", true)]
// 		public float DamageReduce_BaseValue = 50f;
//
// 		[SerializeField, LabelText("弹射的范围半径"), FoldoutGroup("配置", true)]
// 		public float ReboundRange = 5f;
//
//
// 		private AnimationInfoBase _sai_release;
//
// 		private enum SkillStateTypeEnum
// 		{
// 			None_无事发生  = 0,
// 			Releasing_正在释放 = 1,
// 			Active_生效中 = 2,
// 		}
// 		private SkillStateTypeEnum _skillState;
//
// 		public override void InitOnObtain(RPSkill_SkillHolder skillHolderRef, SOConfig_RPSkill configRuntimeInstance,
// 			I_RP_ObjectCanReleaseSkill parent, SkillSlotTypeEnum slot)
// 		{
// 			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
// 			// _sai_release =
// 			// 	GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 			// 		._AN_SkillReleaseSpineAnimationName);
// 			_sai_release.OccupationInfo.RelatedInterface = this;
//
// 		}
//
// 		protected override void BindingInput()
// 		{
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
//
// 			InputAction_ARPG inputActionRef = GameReferenceService_ARPG.Instance.InputActionInstance;
//
// 			InputAction ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed += _IC_ReceiveSkillInput_NormalPerformed;
// 		}
//
// 		protected override void UnbindInput()
// 		{
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
//
// 			InputAction ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed -= _IC_ReceiveSkillInput_NormalPerformed;
// 		}
//
// 		
// 		
// 		protected virtual void _IC_ReceiveSkillInput_NormalPerformed(InputAction.CallbackContext context)
// 		{
// 			//不是自己，无事发生
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
// 			/*
// 			 * 如果当前技能就是活跃的，则表明快速施法
// 			 */
// 			OnSkillSlotReceiveInput();
//
// 			//
// 			if (!IfReactToInput())
// 			{
// 				return;
// 			}
// 			if (!CheckIfDataEntryEnough())
// 			{
// 				return;
// 			}
// 			// if (!_characterBehaviourRef.TryOccupyByOccupationInfo(
// 			// 	GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 			// 		._AN_SkillReleaseSpineAnimationName).OccupationInfo))
// 			// {
// 			// 	return;
// 			// }
//
// 			//到这里了，开始准备
// 			OnSkillBeginPrepare();
// 		}
//
//
// 		protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
// 		{
// 			var ds = base.OnSkillBeginPrepare(autoLaunch);
// 			_skillState = SkillStateTypeEnum.Releasing_正在释放;
// 			OnSkillResetCoolDown();
// 			OnSkillConsumeSP();
//
// 			
// 			//发出要求动画机响应的事件
// 			OnSkillFirstNewRelease();
//
// 			DS_ActionBusArguGroup animationDS =
// 				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_RequireAnimationByAnimationInfo_技能要求动画操作);
// 			// animationDS.ObjectArgu1 = GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 			// 	._AN_SkillReleaseSpineAnimationName);
// 			animationDS.ObjectArgu2 = this;
// 			_selfActionBusRef.TriggerActionByType(animationDS);
//
//
// 			return ds;
// 		}
//
// 		
// 		
// 		protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
// 		{
// 			var str = oInfo.OccupationInfoConfigName;
// 			if (str.Equals(_sai_release.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				return true;
// 			}
// 			else
// 			{
// 				return false;
// 			}
// 		}
//
// 		// protected override void _ABC_OnSkillAnimationGeneralTakeEffect(DS_ActionBusArguGroup ds)
// 		// {
// 		// 	if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 		// 	{
// 		// 		return;
// 		// 	}
// 		//
// 		// 	var configName = (ds.ObjectArguStr as string);
// 		// 	if (!string.Equals(configName,
// 		// 		_sai_release.ConfigName,StringComparison.OrdinalIgnoreCase))
// 		// 	{
// 		// 		return;
// 		// 	}
// 		// 	_skillState = SkillStateTypeEnum.Active_生效中;
// 		//
// 		// 	//在此施加buff
// 		// 	var applyResult = _characterBehaviourRef.ReceiveBuff_TryApplyBuff(
// 		// 		RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1,
// 		// 		_characterBehaviourRef,
// 		// 		_characterBehaviourRef);
// 		// 	if (applyResult == BuffApplyResultEnum.Success ||
// 		// 	    applyResult == BuffApplyResultEnum.AlreadyExistsAndRefresh)
// 		// 	{
// 		// 		Buff_BladeDanceA1 targetBuff =
// 		// 			_characterBehaviourRef.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1)
// 		// 				as Buff_BladeDanceA1;
// 		// 		targetBuff.ResetExistDurationAs(SkillDuration);
// 		// 		targetBuff.ResetAvailableTimeAs(SkillDuration);
// 		// 		targetBuff.DamageReduce_BaseValue = DamageReduce_BaseValue;
// 		// 		targetBuff.DamageReduce_FromAttackRatio = DamageReduce_FromAttackRatio;
// 		// 		targetBuff.ReboundRange = ReboundRange;
// 		// 		targetBuff.SPConsumePerSecond = SPConsumePerSecond;
// 		// 	}
// 		// }
//
// 		protected override void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
// 		{
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
// 			var configName = (ds.ObjectArguStr as string);
//
// 			if (string.Equals(configName, _sai_release.ConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				ReturnToIdleAfterSkill();
// 			}
// 		}
//
// 		// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupyFromInfo)
// 		// {
// 		// 	var fixedType = GetFixedOccupiedCancelType(occupyFromInfo);
// 		// 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupyFromInfo);
// 		// 	switch (_skillState)
// 		// 	{
// 		// 		case SkillStateTypeEnum.None_无事发生:
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Releasing_正在释放:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					if (!isSelf)
// 		// 					{
// 		// 						_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 						Clear_PartialClearNotImmediate();
// 		// 					}
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum
// 		// 						.FromSkill_BladeDanceA1_剑舞A1);
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Active_生效中:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		default:
// 		// 			throw new ArgumentOutOfRangeException();
// 		// 	}
// 		// }
// 		public override void Clear_FullClearAllSkillContentImmediate()
// 		{
// 			_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1);
// 			VFX_GeneralClear(true);
// 		}
// 		public override void Clear_PartialClearNotImmediate()
// 		{
// 			_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1);
// 			VFX_GeneralClear();
// 		}
//
// 		public override SkillReadyTypeEnum GetSkillReadyType()
// 		{
// 			if (_skillState != SkillStateTypeEnum.None_无事发生)
// 			{
// 				return SkillReadyTypeEnum.Using_还在使用;
// 			}
// 			return base.GetSkillReadyType();
// 		}
//
//
// 		private void _ABC_ResetSkillState_OnRelatedBuffRemove(DS_ActionBusArguGroup ds)
// 		{
// 			var targetBuff = ds.ObjectArgu1 as BaseRPBuff;
// 			if (targetBuff is Buff_BladeDanceA1)
// 			{
// 				// _Internal_BroadcastSkillReleaseFinish();
// 				_skillState = SkillStateTypeEnum.None_无事发生;
// 			}
// 		}
// 		protected override bool IfSkillCanCDTick()
// 		{
// 			if (_skillState == SkillStateTypeEnum.None_无事发生)
// 			{
// 				return true;
// 			}
// 			else
// 			{
// 				return false;
// 			}
// 		}
// 		public override Sprite GetCurrentSprite(DamageTypeEnum @override = DamageTypeEnum.None)
// 		{
// 			DamageTypeEnum fd = DamageTypeEnum.None;
//
// 			if (@override != DamageTypeEnum.None)
// 			{
// 				fd = @override;
// 			}
// 			else
// 			{
// 				fd = (RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
// 					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType).CurrentDamageType;
// 			}
// 			switch (fd)
// 			{
// 				case DamageTypeEnum.YuanNengGuang_源能光:
// 					return SpritePairs.Find((pair => pair.Desc.Contains("光"))).SpriteAsset;
// 				case DamageTypeEnum.YuanNengDian_源能电:
// 					return SpritePairs.Find((pair => pair.Desc.Contains("电"))).SpriteAsset;
// 			}
// 			return null;
// 		}
// 	}
// }