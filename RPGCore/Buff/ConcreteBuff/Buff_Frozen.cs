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
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff
{
	[InfoBox("需要监听一个OvertimeTick")]
	[Serializable]
	public class Buff_Frozen : BaseRPBuff
	{



		[TitleGroup("====配置====")]
		[SerializeField, LabelText("非玩家-需要执行的决策名，从上到下查找"), GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		public List<string> _did_UnbalanceMovementDecisionName = new List<string>() { "通用不循环异常" };

		[TitleGroup("====配置====")]
		[SerializeField, LabelText("玩家-动画配置名"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ancn_PlayerCharacterUnbalanceMovementAnimationConfigName = "不循环异常";

		[TitleGroup("====配置====")]
		[SerializeField, LabelText("vfx_冰冻开始特效配置名"), GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfxId_FrozenStart;


		[TitleGroup("====配置====")]
		[SerializeField, LabelText("vfx_冰冻结束特效配置名"), GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfxId_FrozenEnd;

		protected PerVFXInfo _vfxInfo_Frozen;
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
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_vfxInfo_Frozen = _VFX_GetAndSetBeforePlay(_vfxId_FrozenStart)._VFX__10_PlayThis(true, true);
			var _behaviourRef = Parent_SelfBelongToObject as BaseARPGCharacterBehaviour;
			_moveSpeedEntry = _behaviourRef.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画,
				_ABC_BlockAnimationRestore);
			TurnToUnbalance();
			return ds;
		}
		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			_vfxInfo_Frozen = _VFX_GetAndSetBeforePlay(_vfxId_FrozenStart)._VFX__10_PlayThis(true, true);
			TurnToUnbalance();
			return base.OnExistBuffRefreshed(caster, blps);
		}

		protected void TurnToUnbalance()
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
					foreach (string perDID in _did_UnbalanceMovementDecisionName)
					{
						var find =
							brain_enemy.BrainHandlerFunction.FindSelfDecisionByString(perDID,
								true);
						if (find != null)
						{
							var add = brain_enemy.BrainHandlerFunction.AddDecisionToQueue(find,
								BaseAIBrainHandler.DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首);
							if (add.DecisionHandler is not DH_通用常规失衡_CommonUnbalance unbalance)
							{
								DBug.LogError($" {enemyArpgCharacterBehaviour.name}的AI决策{perDID}不是通用常规失衡，无法执行硬直。");
								return;
							}
							unbalance.SetDuration_NotLess(BuffRemainingAvailableTime);
							return;
						}
					}
					break;
				case AllyARPGCharacterBehaviour allyArpgCharacterBehaviour:
					var brain_ally = allyArpgCharacterBehaviour.GetAIBrainRuntimeInstance();
					foreach (string perDID in _did_UnbalanceMovementDecisionName)
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
							unbalance.SetDuration_NotLess(BuffRemainingAvailableTime);
							return;
						}
					}
					break;
				case PlayerARPGConcreteCharacterBehaviour playerArpgConcreteCharacterBehaviour:
					var ds_ani_player = new DS_ActionBusArguGroup(
						_ancn_PlayerCharacterUnbalanceMovementAnimationConfigName,
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

			if (!_moveSpeedEntry.ModifyListContains(_moveSpeedModifyEntry))
			{
				_moveSpeedEntry.AddDataEntryModifier(_moveSpeedModifyEntry);
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




		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			_vfxInfo_Frozen?.VFX_StopThis(true);



			var ds = base.OnBuffPreRemove();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画,
				_ABC_BlockAnimationRestore);

			if (Parent_SelfBelongToObject.CurrentDataValid())
			{
				switch (Parent_SelfBelongToObject)
				{
					case EnemyARPGCharacterBehaviour enemyArpgCharacterBehaviour:
						if (enemyArpgCharacterBehaviour.GetAIBrainRuntimeInstance() == null ||
						    enemyArpgCharacterBehaviour.GetAIBrainRuntimeInstance().BrainHandlerFunction == null)
						{
							break;
						}
						//会试图将对应DH设置时长为0，但是为不小于设置，如果那边时长还是很长，就其实相当于没作用
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
						var ds_tryRestore = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画);
						ds_tryRestore.ObjectArgu1 = playerArpgConcreteCharacterBehaviour.GetSelfRolePlayArtHelper()
							.GetInitRestoreResult();
						playerArpgConcreteCharacterBehaviour.ReleaseSkill_GetActionBus()
							.TriggerActionByType(ds_tryRestore);
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

				if (_moveSpeedModifyEntry != null)
				{
					if (_moveSpeedEntry.ModifyListContains(_moveSpeedModifyEntry))
					{
						_moveSpeedEntry.RemoveEntryModifier(_moveSpeedModifyEntry);
						_moveSpeedModifyEntry.ReleaseToPool();
						;
					}
				}
			}

			_VFX_GetAndSetBeforePlay(_vfxId_FrozenEnd);
			return ds;
		}


	}
}