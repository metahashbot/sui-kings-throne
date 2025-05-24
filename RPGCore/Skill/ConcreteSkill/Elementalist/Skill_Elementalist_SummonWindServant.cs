using System;
using ARPG.Character;
using ARPG.Manager;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Aura;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
// 	[Serializable] [TypeInfoBox("元素使-【风】元素傀儡")]
// 	public class Skill_Elementalist_SummonWindServant: Skill_SummonServantBase
// 	{
//
//
// 		[SerializeField, LabelText("推人光环-Buff 类型"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// #if UNITY_EDITOR
// 		[InlineButton("@PeekBuffConfig(_healAuraBuffType)", "瞅一眼")]
// #endif
// 		protected RolePlay_BuffTypeEnum _healAuraBuffType = RolePlay_BuffTypeEnum.Aura_SimplePullAura_简单无硬直拉人光环;
//
//
// 		[SerializeField, LabelText("作用参数"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		protected Buff_DisplacementAuraBase.BLP_位移光环信息_DisplacementAura _displacementAuraInfo =
// 			new Buff_DisplacementAuraBase.BLP_位移光环信息_DisplacementAura
// 			{
// 				CurrentAffectRange = 5f,
// 				CurrentAffectInterval = 1f,
// 				CurrentAffectDuration = 0.5f,
// 				CurrentAffectFaction = FactionTypeEnumFlag.Enemy_敌人,
// 				PullDistancePerSecond = -1.8f,
// 				PullDistancePerWeight = 0.20f,
// 				MaxPullForce = 5f
// 			};
//
//
// 		private enum SkillStateTypeEnum
// 		{
// 			None_无事发生 = 0, Releasing_施法中 = 1,
// 		}
//
//
// 		private SkillStateTypeEnum _skillState;
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
//
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
// 			summoned.ReceiveBuff_TryApplyBuff(_healAuraBuffType, summoned, summoned, _displacementAuraInfo);
//
//
//
//
// 			return summonedInfo;
// 		}
//
//
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
//
// 	}
}