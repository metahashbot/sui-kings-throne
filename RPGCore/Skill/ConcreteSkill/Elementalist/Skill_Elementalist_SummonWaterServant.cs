using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Player.Ally;
using ARPG.Manager;
using Global;
using Global.ActionBus;

using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff.Aura;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
// 	[Serializable] [TypeInfoBox("元素使-【水】元素傀儡")]
// 	public class Skill_Elementalist_SummonWaterServant : Skill_SummonServantBase
// 	{
// 		
// 		[SerializeField, LabelText("治疗光环-Buff 类型"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// #if UNITY_EDITOR
// 		[InlineButton("PeekBuffConfig(_healAuraBuffType)","瞅一眼")]
// #endif
// 		protected RolePlay_BuffTypeEnum _healAuraBuffType = RolePlay_BuffTypeEnum.Aura_CommonHealAura_通用治疗光环;
//
// 		[SerializeField, LabelText("光环信息BLP"), FoldoutGroup("配置", true)]
// 		protected Buff_CommonAuraBase.BLP_基本光环构建信息_BaseAuraBuild _blp_AuraBuild =
// 			new Buff_CommonAuraBase.BLP_基本光环构建信息_BaseAuraBuild
// 			{
// 				DamageApplyInfo = new ConSer_DamageApplyInfo
// 				{
// 					DamageType = DamageTypeEnum.Heal_治疗,
// 					DamageOriginalGenre = DamageOriginalGenreTypeEnum.Normal,
// 					DamageTryTakenBase = 50,
// 					DamageTakenRelatedDataEntry = true,
// 					RelatedDataEntryInfos = new List<ConSer_DataEntryRelationConfig>
// 					{
// 						new ConSer_DataEntryRelationConfig
// 						{
// 							RelatedToReceiver = false,
// 							RelatedDataEntryType = RP_DataEntry_EnumType.AttackPower_攻击力,
// 							Partial = 0.50f,
// 							CalculatePosition = (ModifyEntry_CalculatePosition)ModifyEntry_CalculatePosition.FrontAdd,
// 						}
// 					},
// 					DamageTypeEnhanced = false,
// 					ContainDodgeCalculation = false,
// 					ContainCriticalCalculation = false,
// 					ContainMustCritical = false,
// 					ContainForceMovement = false,
// 					ContainForceStiffness = false,
// 					ContainBuffEffect = false
// 				},
// 				CurrentAffectRange = 5f,
// 				CurrentAffectInterval = 1f,
// 				CurrentAffctFaction = FactionTypeEnumFlag.PlayerAlly_玩家的友军 | FactionTypeEnumFlag.Player_玩家角色
// 			};
// 		
// 		
//
//
// 		private enum SkillStateTypeEnum
// 		{
// 			None_无事发生 = 0,
// 			Releasing_施法中 = 1,
// 		}
//
//
// 		private SkillStateTypeEnum _skillState;
//
//
// 		public override void InitOnObtain(
// 			RPSkill_SkillHolder skillHolderRef,
// 			SOConfig_RPSkill configRuntimeInstance,
// 			I_RP_ObjectCanReleaseSkill parent,
// 			SkillSlotTypeEnum slot)
// 		{
// 			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
// 		}
//
//
// 		protected override void BindingInput()
// 		{
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
// 			var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
// 			var ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed += _IC_ReceiveSkillNormalPerformed;
// 		}
// 		protected override void UnbindInput()
// 		{
// 			var ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed -= _IC_ReceiveSkillNormalPerformed;
// 		}
// 		public override SkillReadyTypeEnum GetSkillReadyType()
// 		{
// 			if (_skillState != SkillStateTypeEnum.None_无事发生)
// 			{
// 				return SkillReadyTypeEnum.Using_还在使用;
// 			}
// 			return base.GetSkillReadyType();
// 		}
//
// 		protected virtual void _IC_ReceiveSkillNormalPerformed(InputAction.CallbackContext context)
// 		{
// 			//不是自己，无事发生
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
// 			OnSkillSlotReceiveInput();
// 			switch (_skillState)
// 			{
// 				case SkillStateTypeEnum.None_无事发生:
// 					if (!IfReactToInput())
// 					{
// 						return;
// 					}
// 					if (!CheckIfDataEntryEnough())
// 					{
// 						return;
// 					}
// 					// if (!_characterBehaviourRef.TryOccupyByOccupationInfo(
// 					// 	GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 					// 		._AN_SkillReleaseSpineAnimationName).OccupationInfo))
// 					// {
// 					// 	return;
// 					// }
//
// 					
// 					//如果达到召唤上限了，就无事发生
// 					if (SummonedAllyInfoList.Count >= InitialSummonLimit)
// 					{
// 						return;
// 					}
// 					
// 					
// 					//到这里了，开始准备
// 					OnSkillBeginPrepare();
// 					
// 					break;
// 				case SkillStateTypeEnum.Releasing_施法中:
// 					
// 					break;
// 			}
// 		}
//
//
// 		protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
// 		{
// 			var ds = base.OnSkillBeginPrepare(autoLaunch);
//
// 			_skillState = SkillStateTypeEnum.Releasing_施法中;
//
//
// 			return ds;
// 		}
//
// 		protected override SummonedBehaviourInfo SummonedBehaviourAppear()
// 		{
// 			var summonedInfo = base.SummonedBehaviourAppear();
// 			var summoned = summonedInfo.BehaviourRef;
// 			
// 			SyncWithSelfData(summoned);
// 			summoned.ReceiveBuff_TryApplyBuff(_healAuraBuffType, summoned, summoned, _blp_AuraBuild);
// 			var auraBuff =
// 				summoned.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum.Aura_CommonHealAura_通用治疗光环) as
// 					Buff_CommonAuraBase;
// 			auraBuff.ResetAvailableTimeAs(-1f);
// 			auraBuff.ResetExistDurationAs(-1f);
// 			auraBuff.CurrentAffctFaction = FactionTypeEnumFlag.Player_玩家角色 | FactionTypeEnumFlag.PlayerAlly_玩家的友军;
//
// 			return summonedInfo;
// 		}
//
//
//
//
// 		// protected override void _ABC_OnSkillAnimationGeneralTakeEffect(DS_ActionBusArguGroup ds)
// 		// {
// 		// 	base._ABC_OnSkillAnimationGeneralTakeEffect(ds);
// 		// 	var aniConfigName = ds.ObjectArguStr as string;
// 		// 	if (aniConfigName.Equals(_sai_Release.ConfigName, StringComparison.OrdinalIgnoreCase))
// 		// 	{
// 		// 		RegisterSummonPositionAndTask();
// 		// 		
// 		// 	}
// 		// }
// 		// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupySourceInfo)
// 		// {
// 		// 	var fixedType = GetFixedOccupiedCancelType(occupySourceInfo);
// 		// 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupySourceInfo);
// 		// 	switch (_skillState)
// 		// 	{
// 		// 		case SkillStateTypeEnum.None_无事发生:
// 		// 			
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Releasing_施法中:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					if (!isSelf)
// 		// 					{
// 		// 						_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 						// Clear_PartialClearNotImmediate();
// 		// 					}
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		default:
// 		// 			throw new ArgumentOutOfRangeException();
// 		// 	}
// 		// 	
// 		// }
// 		public override void Clear_FullClearAllSkillContentImmediate()
// 		{
// 			VFX_GeneralClear(true);
// 		}
// 		public override void Clear_PartialClearNotImmediate()
// 		{
// 			VFX_GeneralClear();
// 		}
//
//
// 		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
// 		{
// 			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
// 			if (_skillState != SkillStateTypeEnum.None_无事发生)
// 			{
// 				(this as I_SkillNeedShowProgress).ShowProgressTick(currentTime, currentFrameCount, delta);
// 			}
// 		}
//
//
// 		protected override bool IfSkillCanCDTick()
// 		{
// 			return true;
// 		}
// 		protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
// 		{
// 			if (oInfo.OccupationInfoConfigName.Equals(_sai_Release.OccupationInfo.OccupationInfoConfigName,
// 				StringComparison.OrdinalIgnoreCase))
// 			{
// 				return true;
// 			}
// 			return false;
// 		}
// 		
// 		
// 	}
}