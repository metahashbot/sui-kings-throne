using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Player.Ally;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff.Element;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.Buff.ConcreteBuff.Common
{

	[Serializable]
	public class Buff_眩晕状态_DizzyState : BaseRPBuff, I_MayResistByToughness
	{
		[SerializeField, LabelText("VFXID")]
		public string _vfxID;

		private string _runtimeVFXID;
		private PerVFXInfo _vfxInfo_Dizzy;

		[TitleGroup("====配置====", Alignment = TitleAlignments.Centered)]
		[SerializeField, LabelText("非玩家-需要执行的决策名，从上到下查找"), GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		public List<string> _did_StiffnessOnHitDecisionName = new List<string>() { "通用循环异常" };

		[SerializeField, LabelText("玩家-动画配置名"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ancn_PlayerCharacterStiffnessOnHitAnimationConfigName = "循环异常";

		private List<string> _weaknessGroupToRestoreList = new List<string>();

		private Float_RPDataEntry _moveSpeedEntry;
		private Float_ModifyEntry_RPDataEntry _moveSpeedModifyEntry;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画,
				_ABC_BlockAnimationRestore);
			_runtimeVFXID = _vfxID;
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_moveSpeedEntry =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			return ds;
		}

		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);

			return ds;
		}

        protected void TurnToDizzy()
		{
			switch (Parent_SelfBelongToObject)
			{
				case EnemyARPGCharacterBehaviour enemyArpgCharacterBehaviour:
					if (!enemyArpgCharacterBehaviour.CharacterDataValid)
					{
						return;
					}

					var brain_enemy = enemyArpgCharacterBehaviour.GetAIBrainRuntimeInstance();
					if (brain_enemy == null)
					{
						return;
					}
					foreach (string perDID in _did_StiffnessOnHitDecisionName)
					{
						var find = brain_enemy.BrainHandlerFunction.FindSelfDecisionByString(perDID, true);
						if (find != null)
						{
							var add = brain_enemy.BrainHandlerFunction.AddDecisionToQueue(find,
								BaseAIBrainHandler.DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首,
								true);
							if (add.DecisionHandler is not DH_通用常规失衡_CommonUnbalance unbalance)
							{
								DBug.LogError($" {enemyArpgCharacterBehaviour.name}的AI决策{perDID}不是通用循环异常，无法执行眩晕。");
								return;
							}
							unbalance.SetDuration_NotLess(BuffRemainingAvailableTime);
							return;
						}
					}
					//走到这说明压根就没找到
					break;
				case AllyARPGCharacterBehaviour allyArpgCharacterBehaviour:
					var brain_ally = allyArpgCharacterBehaviour.GetAIBrainRuntimeInstance();
					foreach (string perDID in _did_StiffnessOnHitDecisionName)
					{
						var find = brain_ally.BrainHandlerFunction.FindSelfDecisionByString(perDID, true);
						if (find != null)
						{
							var add = brain_ally.BrainHandlerFunction.AddDecisionToQueue(find,
								BaseAIBrainHandler.DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首);
							if (add.DecisionHandler is not DH_通用常规失衡_CommonUnbalance unbalance)
							{
								DBug.LogError($" {allyArpgCharacterBehaviour.name}的AI决策{perDID}不是通用循环异常，无法执行眩晕。");
								return;
							}
							unbalance.SetDuration_NotLess(BuffRemainingAvailableTime);
							return;
						}
					}
					break;
				case PlayerARPGConcreteCharacterBehaviour playerArpgConcreteCharacterBehaviour:
					var ds_ani_player = new DS_ActionBusArguGroup(
						_ancn_PlayerCharacterStiffnessOnHitAnimationConfigName,
						AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
						playerArpgConcreteCharacterBehaviour.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
						false,
						FixedOccupiedCancelTypeEnum.StrongBreak_强断);
					playerArpgConcreteCharacterBehaviour.ReleaseSkill_GetActionBus().TriggerActionByType(ds_ani_player);

					break;
			}
			if (_moveSpeedModifyEntry == null)
			{
				_moveSpeedModifyEntry = Float_ModifyEntry_RPDataEntry.GetNewFromPool(-9999f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.RearAdd,
					this);
			}
			else
			{
				if (!_moveSpeedEntry.ModifyListContains(_moveSpeedModifyEntry))
				{
					_moveSpeedEntry.AddDataEntryModifier(_moveSpeedModifyEntry);
				}
			}

			if (_vfxID != null)
			{
				_vfxInfo_Dizzy = _VFX_GetAndSetBeforePlay(_runtimeVFXID)._VFX__10_PlayThis();
			}
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();
		
			_vfxInfo_Dizzy?.VFX_StopThis(true);
			return ds;
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_设置持续和有效时间_SetDurationAndTime blp_SetDurationAndTime:
					if (blp_SetDurationAndTime.ContainDurationSet)
					{
						if (blp_SetDurationAndTime.IsDurationReset)
						{
							ResetExistDurationAs(blp_SetDurationAndTime.IsDurationNotLessThan
								? Math.Max(BuffRemainingExistDuration, blp_SetDurationAndTime.DurationResetValue)
								: blp_SetDurationAndTime.DurationResetValue);
							_buffDuration = blp_SetDurationAndTime.DurationResetValue;
						}
						else
						{
							ResetExistDurationAs(blp_SetDurationAndTime.IsDurationModifyByMultiply
								? BuffRemainingExistDuration * blp_SetDurationAndTime.DurationModifyValue
								: BuffRemainingExistDuration + blp_SetDurationAndTime.DurationModifyValue);
						}
					}
					if (blp_SetDurationAndTime.ContainAvailableTimeSet)
					{
						if (blp_SetDurationAndTime.IsAvailableTimeReset)
						{
							ResetAvailableTimeAs(blp_SetDurationAndTime.IsAvailableTimeNotLessThan
								? Math.Max(BuffRemainingAvailableTime, blp_SetDurationAndTime.AvailableTimeResetValue)
								: blp_SetDurationAndTime.AvailableTimeResetValue);
							_buffAvailableTime = blp_SetDurationAndTime.AvailableTimeResetValue;
						}
						else
						{
							ResetAvailableTimeAs(blp_SetDurationAndTime.IsAvailableTimeModifyByMultiply
								? BuffRemainingAvailableTime * blp_SetDurationAndTime.AvailableTimeModifyValue
								: BuffRemainingAvailableTime + blp_SetDurationAndTime.AvailableTimeModifyValue);
						}
					}
					TurnToDizzy();
					break;


				case BLP_可指定特效的眩晕参数_DizzyUseSpecificVFXID dizzy:


					if (dizzy.ContainNonDefaultVFX)
					{
						_runtimeVFXID = dizzy.NewUID;
					}

					_vfxInfo_Dizzy?.VFX_StopThis(true);
					if (!string.IsNullOrEmpty(_runtimeVFXID))
					{
						_vfxInfo_Dizzy = _VFX_GetAndSetBeforePlay(_runtimeVFXID)._VFX__10_PlayThis();
					}
					ResetDurationAndAvailableTimeAs(dizzy.DizzyDuration, dizzy.DizzyDuration, true);
					TurnToDizzy();

					break;
				case BLP_眩晕后恢复常驻弱点_ReOpenWeaknessAfterDizzy reOpenWeaknessAfterDizzy:
					if (Parent_SelfBelongToObject.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
						.CommonEnemyWeakness_通用敌人弱点) == BuffAvailableType.Available_TimeInAndMeetRequirement)
					{
						var buff = Parent_SelfBelongToObject.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
							.CommonEnemyWeakness_通用敌人弱点) as Buff_通用敌人弱点_CommonEnemyWeakness;
						var main = buff.GetMainWeaknessInfoGroup();
						_weaknessGroupToRestoreList.Add(main.GroupUID);
					}
					break;
				
			}
		}

        protected void _ABC_BlockAnimationRestore(DS_ActionBusArguGroup ds)
        {
            var rr = ds.GetObj1AsT<RP_DS_AnimationRestoreResult>();
            if (rr != null)
            {
                rr.RestoreSuccess = false;
                rr.BlockBy = this;
            }
        }

        [Serializable]
		public class BLP_眩晕后恢复常驻弱点_ReOpenWeaknessAfterDizzy : BaseBuffLogicPassingComponent
		{
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_眩晕后恢复常驻弱点_ReOpenWeaknessAfterDizzy>.Release(this);
			}
		}

		[Serializable]
		public class BLP_可指定特效的眩晕参数_DizzyUseSpecificVFXID : BaseBuffLogicPassingComponent
		{
			[SerializeField, LabelText("包含非默认特效")]
			public bool ContainNonDefaultVFX = false;

			[SerializeField, LabelText("新的特效UID"), ShowIf("ContainNonDefaultVFX")]
			public string NewUID;

			[SerializeField, LabelText("眩晕的不小于设置时长")]
			public float DizzyDuration = 2f;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_可指定特效的眩晕参数_DizzyUseSpecificVFXID>.Release(this);
			}
		}
	}
}