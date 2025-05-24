using System;
using Global;
using Global.ActionBus;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WRListen_一段时间不受伤后逐渐减少弱点积累_ReduceWeaknessOnTimeOnNotDamage : BaseWeaknessAffectRule,
		I_WeaknessRuleAsListener
	{

		[SerializeField,LabelText(" 空闲时间")]
		public float IdleTime = 5f;
		
		[SerializeField,LabelText(" 每秒减少量")]
		public float ReduceValuePerSecond = 15f;

		[NonSerialized,ShowInInspector,LabelText("将要开始减少的时间点")]
		public float BeginReduceTime;
		public static WRListen_一段时间不受伤后逐渐减少弱点积累_ReduceWeaknessOnTimeOnNotDamage GetDeepCopy(
			WRListen_一段时间不受伤后逐渐减少弱点积累_ReduceWeaknessOnTimeOnNotDamage copyFrom)
		{
			var result = new WRListen_一段时间不受伤后逐渐减少弱点积累_ReduceWeaknessOnTimeOnNotDamage();
			result.RelatedWeaknessUID = copyFrom.RelatedWeaknessUID;
			result.IdleTime = copyFrom.IdleTime;
			result.ReduceValuePerSecond = copyFrom.ReduceValuePerSecond;
			  
			return result;
		}


		public void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var copy = GetDeepCopy(this);
			copy.RelatedBuffRef = RelatedBuffRef;
			copy.RelatedGroupRef = group;
			group.AddListenerRule(copy);
		}

		public void ProcessOnRegisterToRuntimeGroup(
			Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var lab = group.RelatedBuff.Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
				_ABC_ProcessDamageTaken_OnDamageOnHP);
		}
		
		

		protected virtual void _ABC_ProcessDamageTaken_OnDamageOnHP(DS_ActionBusArguGroup ds)
		{
			if (!RelatedGroupRef.CurrentGroupActive)
			{
				return;
			}
			RP_DS_DamageApplyResult dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar.DamageResult_TakenOnHP > float.Epsilon)
			{
				BeginReduceTime = BaseGameReferenceService.CurrentFixedTime + IdleTime;
			}
		}



		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
			
			var lab = relatedBuff.Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			lab.RemoveAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
				_ABC_ProcessDamageTaken_OnDamageOnHP);
		}




		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			if (!RelatedGroupRef.CurrentGroupActive)
			{
				return;
			}
			if (BaseGameReferenceService.CurrentFixedTime > BeginReduceTime)
			{
				RelatedGroupRef.ModifyCounter(-ReduceValuePerSecond * delta);
			}
		}
	}
}