using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Player.Ally;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Utility
{

	[Serializable]
	public class Buff_受击硬直_StiffnessOnHit : BaseRPBuff, I_MayResistByToughness
	{
		[TitleGroup("====配置====", Alignment = TitleAlignments.Centered)]
		[SerializeField, LabelText("非玩家-需要执行的决策名，从上到下查找"), GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		public List<string> _did_StiffnessOnHitDecisionName = new List<string>() { "通用受击硬直" };

		[SerializeField, LabelText("玩家-动画配置名"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ancn_PlayerCharacterStiffnessOnHitAnimationConfigName = "受击硬直";

		[LabelText("计算用时长算子")]
		public float ForcedDisplacement_DurationOperator = 3.3f;
		[LabelText("计算用时长倍率")]
		public float ForcedDisplacement_DurationMultiplier = 1f;

		private Float_RPDataEntry _moveSpeedEntry;
		private Float_ModifyEntry_RPDataEntry _moveSpeedModifyEntry;

		[ShowInInspector,LabelText("当前 受击硬直 活跃吗")]
		protected bool _CurrentStiffnessActivate;

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
		}
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds =base.OnBuffInitialized(blps);
			_moveSpeedEntry = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			return ds;
		}

		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);




			return ds;
		}

		protected void TurnToStiffness(float duration)
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
						var find =
							brain_enemy.BrainHandlerFunction.FindSelfDecisionByString(perDID,
								true);
						if (find != null)
						{
							var add = brain_enemy.BrainHandlerFunction.AddDecisionToQueue(find,
								BaseAIBrainHandler.DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首,
								true);
							if (add.DecisionHandler is not DH_通用常规失衡_CommonUnbalance unbalance)
							{
								DBug.LogError($" {enemyArpgCharacterBehaviour.name}的AI决策{perDID}不是通用常规失衡，无法执行硬直。");
								return;
							}
							unbalance.SetDuration_NotLess(duration);
							return;
						}
					}
					//走到这说明压根就没找到
					DBug.LogError( $" {enemyArpgCharacterBehaviour.name}的硬直用AI决策{_did_StiffnessOnHitDecisionName.GetStringListString()}不存在，无法执行硬直。");
					break;
				case AllyARPGCharacterBehaviour allyArpgCharacterBehaviour:
					var brain_ally = allyArpgCharacterBehaviour.GetAIBrainRuntimeInstance();
					foreach (string perDID in _did_StiffnessOnHitDecisionName)
					{
						var find =
							brain_ally.BrainHandlerFunction.FindSelfDecisionByString(perDID,
								true);
						if (find != null)
						{
							var add = brain_ally.BrainHandlerFunction.AddDecisionToQueue(find,
								BaseAIBrainHandler.DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首);
							if (add.DecisionHandler is not DH_通用常规失衡_CommonUnbalance unbalance)
							{
								DBug.LogError($" {allyArpgCharacterBehaviour.name}的AI决策{perDID}不是通用常规失衡，无法执行硬直。");
								return;
							}
							unbalance.SetDuration_NotLess(duration);
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
						FixedOccupiedCancelTypeEnum.StiffnessBreak_硬直断);
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
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画,
				_ABC_BlockAnimationRestore);

			switch (Parent_SelfBelongToObject)
			{
				case EnemyARPGCharacterBehaviour enemyArpgCharacterBehaviour:
					//会试图将对应DH设置时长为0，但是为不小于设置，如果那边时长还是很长，就其实相当于没作用
					if (!enemyArpgCharacterBehaviour.GetAIBrainRuntimeInstance())
					{
						break;
					}
					var dh = enemyArpgCharacterBehaviour.GetAIBrainRuntimeInstance().BrainHandlerFunction
						.CurrentRunningDecision.DecisionHandler;
					if (dh is DH_通用常规失衡_CommonUnbalance un)
					{
						un.SetDuration_NotLess(0.01f);
					}
					break;
				case AllyARPGCharacterBehaviour allyArpgCharacterBehaviour:
					var dh_ally = allyArpgCharacterBehaviour.GetAIBrainRuntimeInstance().BrainHandlerFunction
						.CurrentRunningDecision.DecisionHandler;
					if (dh_ally is DH_通用常规失衡_CommonUnbalance un_ally)
					{
						un_ally.SetDuration_NotLess(0.01f);
					}
					break;
				case PlayerARPGConcreteCharacterBehaviour playerArpgConcreteCharacterBehaviour:
					var ds_tryRestore =
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画);
					ds_tryRestore.ObjectArgu1 = playerArpgConcreteCharacterBehaviour.GetSelfRolePlayArtHelper()
						.GetInitRestoreResult();
					playerArpgConcreteCharacterBehaviour.ReleaseSkill_GetActionBus().TriggerActionByType(ds_tryRestore);
					if (ds_tryRestore.GetObj1AsT<RP_DS_AnimationRestoreResult>().RestoreSuccess)
					{
						var ds_ani_player = new DS_ActionBusArguGroup("战斗待机",
							AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
							playerArpgConcreteCharacterBehaviour.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
							false,
							FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);
						playerArpgConcreteCharacterBehaviour.ReleaseSkill_GetActionBus()
							.TriggerActionByType(ds_ani_player);
					}
					break;
			}
			
			
			
			if (_moveSpeedEntry != null)
			{
				if (_moveSpeedEntry.ModifyListContains(_moveSpeedModifyEntry))
				{
					_moveSpeedEntry.RemoveEntryModifier(_moveSpeedModifyEntry);
					_moveSpeedModifyEntry.ReleaseToPool();
					;

				}
			
			}


			return ds;
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_受击硬直通用_StiffnessOnHitBLP stiffnessOnHitBlp:
					if (!Parent_SelfBelongToObject.CurrentDataValid())
					{
						return;
					}

					float cAttackPower = stiffnessOnHitBlp.AttackPower;
					var currentMass =
						Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MovementMass_重量);
					if (stiffnessOnHitBlp.AttackPower < currentMass.CurrentValue)
					{
						if (!stiffnessOnHitBlp.StiffnessGuaranteed)
						{
							return;
						}
						cAttackPower = currentMass.CurrentValue;
					}

					//此次造成的硬直时长。为 SQRT ( 时长算子 * ( 力度 / 重量) ) / 10 * 时长倍率
					float duration =
						Mathf.Sqrt(ForcedDisplacement_DurationOperator * (cAttackPower / currentMass.CurrentValue)) /
						ForcedDisplacement_DurationMultiplier * stiffnessOnHitBlp.StiffnessDurationMultiplier;
					duration = (this as I_MayResistByToughness).GetFinalValueResistByToughness(duration);

					ResetAvailableTimeAs(duration);
					ResetExistDurationAs(duration);


					TurnToStiffness(duration);

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
		public class BLP_受击硬直通用_StiffnessOnHitBLP : BaseBuffLogicPassingComponent
		{
			[SerializeField, LabelText("攻击力度")]
			public float AttackPower = 1f;
			[SerializeField, LabelText("硬直保底？必定能产生该重量最小的硬直")]
			public bool StiffnessGuaranteed = false;

			[SerializeField, LabelText("硬直时长乘数")]
			public float StiffnessDurationMultiplier = 1f;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_受击硬直通用_StiffnessOnHitBLP>.Release(this);
			}
		}
	}
}