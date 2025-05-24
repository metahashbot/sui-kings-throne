using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Character.Enemy.AI;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Utility
{
	[Serializable]
	public class Buff_已记录死亡_RegisterToDeath : BaseRPBuff
	{
		[TitleGroup("===具体配置===")] 
		[SerializeField, LabelText("通用硬直决策")]
		public string _did_stoicDecision = "通用不循环异常";

		[SerializeField, LabelText("起始硬直时间")] [TitleGroup("===具体配置===")]
		public float StoicTime = 0.75f;

		[SerializeField, LabelText("每次递减的硬直时间")]
		[TitleGroup("===具体配置===")]
		public float StoicTimeDecreasePerHit = 0.17f;

		[SerializeField, LabelText("最短硬直时间")]
		[TitleGroup("===具体配置===")]
		public float StoicTimeMin = 0.2f;

		
		public float CurrentStoicTimeAdd { get; private set; }

		private EnemyARPGCharacterBehaviour _enemyArpgCharacterBehaviourRef;

		private FloatPresentValue_RPDataEntry _entry_currentHP;



		private UnityAction<bool> _toggleUA;
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_entry_currentHP = parent.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			
			//停止datamodel 的Tick
			_enemyArpgCharacterBehaviourRef = parent as EnemyARPGCharacterBehaviour;
			SOConfig_AIBrain aib = _enemyArpgCharacterBehaviourRef.GetAIBrainRuntimeInstance();
			
			
			if (aib == null || aib.BrainHandlerFunction == null)
			{
				DBug.LogError(
					$"敌人 {_enemyArpgCharacterBehaviourRef.name} 的AIBrain为空，但是它又没有 不记录死亡 的buff，这是不合理的，应当在ARPG初始化表中为其添加不记录死亡的buff" +
					$"这种敌人就不需要记录死亡并更换死亡决策了");
			}




			var blp_ResetDuration = GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Get();
			blp_ResetDuration.SetAllAsNotLess(StoicTime);
			var stoicBuff = Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(
				RolePlay_BuffTypeEnum.UnbalanceMovement_失衡推拉,
				Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
				Parent_SelfBelongToObject,
				blp_ResetDuration);
			GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Release(blp_ResetDuration);
			ResetDurationAndAvailableTimeAs(CurrentStoicTimeAdd, CurrentStoicTimeAdd, true);
			aib.BrainHandlerFunction.CurrentBrainActive = false;
			// var dRef = aib.BrainHandlerFunction.FindSelfDecisionByString(_did_stoicDecision, true, false);
			// aib.BrainHandlerFunction.AddDecisionToQueue(dRef,
			// 	BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首,
			// 	true);
			// CurrentStoicTimeAdd = StoicTime;
			// aib.BrainHandlerFunction.CurrentBrainActive = false;
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			return base.OnBuffInitialized(blps);
		}



		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);
			CurrentStoicTimeAdd -= StoicTimeDecreasePerHit;
			ResetDurationAndAvailableTimeAs(CurrentStoicTimeAdd, CurrentStoicTimeAdd, true);
			return ds;
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_敌人记录死亡_EnemyRegisterToDeath blp_death:
					blp_death._toggleUA = _toggleUA;
					_toggleUA.Invoke(true);
					break;
			}
		}

		public class BLP_敌人记录死亡_EnemyRegisterToDeath : BaseBuffLogicPassingComponent
		{
			public UnityAction<bool> _toggleUA;
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_敌人记录死亡_EnemyRegisterToDeath>.Release(this);
			}
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			_entry_currentHP.ResetDataToValue(0f, true);
			var ds_death = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Damage_OnFinallyDeath_伤害流程最终死亡);
			ds_death.ObjectArgu1 = Parent_SelfBelongToObject as EnemyARPGCharacterBehaviour;
			ds_death.ObjectArgu2 = null;
			ds_death.ObjectArguStr = null;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ds_death);
			
			
			return base.OnBuffPreRemove();
		}
	}
}