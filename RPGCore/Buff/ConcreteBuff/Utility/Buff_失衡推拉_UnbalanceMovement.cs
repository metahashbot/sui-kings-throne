using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Player.Ally;
using Global;
using Global.ActionBus;
using Global.Setting;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Utility
{
	[Serializable]
	public class Buff_失衡推拉_UnbalanceMovement : BaseRPBuff
	{

		[TitleGroup("====配置====")]
		[SerializeField, LabelText("非玩家-需要执行的决策名，从上到下查找"), GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		public List<string> _did_UnbalanceMovementDecisionName = new List<string>() { "通用不循环异常" };

		[TitleGroup("====配置====")]
		[SerializeField, LabelText("玩家-动画配置名"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ancn_PlayerCharacterUnbalanceMovementAnimationConfigName = "不循环异常";



		[TitleGroup("====运行时数据====")]
		[NonSerialized, ShowInInspector, LabelText("当前还活跃的失衡推拉信息列表")]
		public List<PerUnbalanceInfo> CurrentActiveDragInfoList = new List<PerUnbalanceInfo>();



		private static ConSer_MiscSettingInSO _miscSettingRef;

		private BaseARPGCharacterBehaviour _behaviourRef;

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
			_miscSettingRef = GlobalConfigurationAssetHolderHelper.GetGCAHH().MiscSetting_Runtime.SettingContent;
			_behaviourRef = Parent_SelfBelongToObject as BaseARPGCharacterBehaviour;
			_moveSpeedEntry = _behaviourRef.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画,
				_ABC_BlockAnimationRestore);
		}



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			if (!blps.Exists((component => component is BLP_开始失衡推拉_StartUnbalanceMovementBLP)))
			{
				// DBug.LogWarning($"来自角色{Parent_SelfBelongToObject}的失衡推拉施加并没有附带BLP，这可能会引发错误的动画操作，检查一下");
			}
			var ds=  base.OnBuffInitialized(blps);
			;
			return ds;
		}

		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);

			return ds;
		}


		protected void TurnToUnbalance(float duration)
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
								BaseAIBrainHandler.DecisionEnqueueType.BreakAndEnqueueFirst_打断清空并置首,true);
							if (add.DecisionHandler is not DH_通用常规失衡_CommonUnbalance unbalance)
							{
								DBug.LogError($" {enemyArpgCharacterBehaviour.name}的AI决策{perDID}不是通用常规失衡，无法执行硬直。");
								return;
							}
							unbalance.SetDuration_NotLess(duration);
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
							unbalance.SetDuration_NotLess(duration);
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


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			//从所有信息中获得最终的位移向量
			//相当于每个还在活跃着的位移中，都计算得出当前帧的delta位移，然后加起来

			Vector3 finalMove = Vector3.zero;
			for (int i = CurrentActiveDragInfoList.Count - 1; i >= 0; i--)
			{
				if (CurrentActiveDragInfoList[i].RemainingTime < 0f)
				{
					GenericPool<PerUnbalanceInfo>.Release(CurrentActiveDragInfoList[i]);
					CurrentActiveDragInfoList.RemoveAt(i);
					continue;
				}
			}

			foreach (PerUnbalanceInfo perDragInfo in CurrentActiveDragInfoList)
			{
				float lastTime = perDragInfo.ElapsedTime;
				float lastP = perDragInfo.CurrentFinishPartial;
				float lastEva = _miscSettingRef.ForcedDisplacement_CompletionCurve.Evaluate(lastP);


				perDragInfo.ElapsedTime += delta;
				float currentEva =
					_miscSettingRef.ForcedDisplacement_CompletionCurve.Evaluate(perDragInfo.CurrentFinishPartial);

				var deltaMovement = perDragInfo.DisplacementAllVector * (currentEva - lastEva);
				finalMove += deltaMovement;
			}
			_behaviourRef.TryMovePosition_XYZ(finalMove);
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_开始失衡推拉_StartUnbalanceMovementBLP unbalance:
					if (!Parent_SelfBelongToObject.CurrentDataValid())
					{
						return;
					}


					var selfMass = Parent_SelfBelongToObject
						.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MovementMass_重量).CurrentValue;
					if (Mathf.Abs(unbalance.UnbalancePower) < selfMass)
					{
						return;
					}
					PerUnbalanceInfo unbalanceInfo = null;

					unbalanceInfo = GenericPool<PerUnbalanceInfo>.Get();
					CurrentActiveDragInfoList.Add(unbalanceInfo);

					//如果方向太小，就说明需要计算方向而不是采用传入的放系那个
					Vector3 direction = Vector3.zero;

					if (unbalance.UnbalanceDirection.sqrMagnitude < 0.1f)
					{
						direction = Vector3.right;
					}
					else
					{
						Vector3 dir = Vector3.zero;
						if (unbalance.UseXAxis)
						{
							dir.x = unbalance.UnbalanceDirection.x;
						}
						if (unbalance.UseZAxis)
						{
							dir.z = unbalance.UnbalanceDirection.z;
						}
						
						direction = dir;

					}
					
					direction.y = 0f;
					direction = direction.normalized;

					float forceAmount = Mathf.Abs(unbalance.UnbalancePower);
					direction *= unbalance.UnbalancePower > 0f ? 1f : -1f;
					float selfMassWeight = selfMass;
					float thisLength = Mathf.Abs(forceAmount /
					                             (selfMassWeight * _miscSettingRef.ForcedDisplacement_MassMultiplier) *
					                             _miscSettingRef.ForcedDisplacement_DistanceMultiplier);
					Vector3 thisDisplacement = thisLength * direction;
					float thisDuration =
						Mathf.Sqrt(_miscSettingRef.ForcedDisplacement_DurationOperator *
						           (forceAmount / selfMassWeight)) / 10f *
						_miscSettingRef.ForcedDisplacement_DurationMultiplier;

					unbalanceInfo.ElapsedTime = 0f;
					unbalanceInfo.MovementTargetDuration = thisDuration;
					unbalanceInfo.DisplacementAllVector = thisDisplacement;

					TurnToUnbalance(thisDuration);
					ResetTimeToMax();

					break;
			}
		}

		private void ResetTimeToMax()
		{
			float max = 0f;
			foreach (var perInfo in CurrentActiveDragInfoList)
			{
				if (perInfo.RemainingTime > max)
				{
					max = perInfo.RemainingTime;
				}
			}
			ResetExistDurationAs(max);
			ResetAvailableTimeAs(max);
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画,
				_ABC_BlockAnimationRestore);

			if (CurrentActiveDragInfoList != null && CurrentActiveDragInfoList.Count > 0)
			{
				foreach (var perInfo in CurrentActiveDragInfoList)
				{
					GenericPool<PerUnbalanceInfo>.Release(perInfo);
				}
				CurrentActiveDragInfoList.Clear();
			}
			if (Parent_SelfBelongToObject.CurrentDataValid())
			{
				switch (Parent_SelfBelongToObject)
				{
					case EnemyARPGCharacterBehaviour enemyArpgCharacterBehaviour:
						//会试图将对应DH设置时长为0，但是为不小于设置，如果那边时长还是很长，就其实相当于没作用
						if (enemyArpgCharacterBehaviour.GetAIBrainRuntimeInstance() == null ||
						    enemyArpgCharacterBehaviour.GetAIBrainRuntimeInstance().BrainHandlerFunction == null)
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
	
			return ds;
		}


		[Serializable]
		public class BLP_开始失衡推拉_StartUnbalanceMovementBLP : BaseBuffLogicPassingComponent
		{
			[SerializeField, LabelText("失衡推拉力度")]
			public float UnbalancePower = 1;
			[SerializeField, LabelText("失衡推拉方向")]
			public Vector3 UnbalanceDirection = Vector3.zero;
			[SerializeField, LabelText("使用X轴")]
			public bool UseXAxis = true;
			[SerializeField, LabelText("使用Z轴")]
			public bool UseZAxis = true;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_开始失衡推拉_StartUnbalanceMovementBLP>.Release(this);
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



		public class PerUnbalanceInfo
		{

			[ShowInInspector, LabelText("已经过时长")]
			public float ElapsedTime;

			[ShowInInspector, LabelText("剩余时长")]
			public float RemainingTime => MovementTargetDuration - ElapsedTime;


			[ShowInInspector, LabelText("位移整体向量")]
			public Vector3 DisplacementAllVector;

			[ShowInInspector, LabelText("当前完成度")]
			public float CurrentFinishPartial => Mathf.Clamp01(ElapsedTime / MovementTargetDuration);

			[ShowInInspector, LabelText("本次位移将要进行的时长")]
			public float MovementTargetDuration;

		}


	}
}