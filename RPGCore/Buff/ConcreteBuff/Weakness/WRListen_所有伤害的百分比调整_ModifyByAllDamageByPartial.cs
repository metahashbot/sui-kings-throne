using System;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WRListen_所有伤害的百分比调整_ModifyByAllDamageByPartial : BaseWeaknessAffectRule, I_WeaknessRuleAsListener
	{
		[InfoBox("弱点值固定为100。如果这里填500，也就是说一管血打空会完整地触发五次弱点\n")]
		[LabelText("每造成等同最大生命值的伤害时，增加多少弱点值")]
		public float DamageModifyPercent = 500f;

		[LabelText("是否匹配伤害类型")]
		public bool ContainDamageTypeSpecific = false;

		[LabelText("匹配的伤害类型")]
		public DamageTypeEnum DamageTypeTarget = DamageTypeEnum.None;

		public static WRListen_所有伤害的百分比调整_ModifyByAllDamageByPartial GetDeepCopy(
			WRListen_所有伤害的百分比调整_ModifyByAllDamageByPartial copy)
		{
			WRListen_所有伤害的百分比调整_ModifyByAllDamageByPartial ret = new WRListen_所有伤害的百分比调整_ModifyByAllDamageByPartial();
			ret.DamageModifyPercent = copy.DamageModifyPercent;
			ret.ContainDamageTypeSpecific = copy.ContainDamageTypeSpecific;
			ret.DamageTypeTarget = copy.DamageTypeTarget;
			ret.RelatedBuffRef = copy.RelatedBuffRef;
			ret.RelatedGroupRef = copy.RelatedGroupRef;
			return ret;
		}

	public void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var copy = GetDeepCopy(this);
			group.AddListenerRule(copy);
			RelatedBuffRef = group.RelatedBuff;
			RelatedGroupRef = group;
		}
		public void ProcessOnRegisterToRuntimeGroup(
			Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var lab = group.RelatedBuff.Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
				_ABC_ProcessDamageTaken_OnDamageOnHP);
			RelatedBuffRef = group.RelatedBuff;
			RelatedGroupRef = group;
			
		}

		public float Cache_CurrentAccumulate;
		protected virtual void _ABC_ProcessDamageTaken_OnDamageOnHP(DS_ActionBusArguGroup ds)
		{
			if (!RelatedGroupRef.CurrentGroupActive)
			{
				return;
			}

			RP_DS_DamageApplyResult dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar.DamageResult_TakenOnHP < 0f || dar.ResultLogicalType != RP_DamageResultLogicalType.NormalResult)
			{
				return;
			}
			// //小于0，实际上会增加弱点。然后如果同组出现了 同组击晕不增加，则不增加
			// if (RelatedBuffRef.Parent_SelfBelongToObject.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Dizzy_眩晕) ==
			//     BuffAvailableType.Available_TimeInAndMeetRequirement)
			// {
			// 	if (RelatedBuffRef.AllRules.Exists((rule =>
			// 		rule is WARResult_效果之施加眩晕_WeaknessResultOnDizzy dizzy && dizzy.NotAddWeaknessWhenDizzy &&
			// 		dizzy.RelatedWeaknessUID.Equals(RelatedWeaknessUID, StringComparison.OrdinalIgnoreCase))))
			// 	{
			// 		return;
			// 	}
			// }
			if (!ContainDamageTypeSpecific || (ContainDamageTypeSpecific && dar.DamageType == DamageTypeTarget))
			{
				float maxHP = RelatedGroupRef.RelatedBuff.Parent_SelfBelongToObject
					.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP).CurrentValue;
				Cache_CurrentAccumulate = (dar.DamageResult_TakenOnHP / maxHP)  * DamageModifyPercent;


				var ds_accu =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_Damage_WeaknessEffectOnTakenAccumulate_弱点将要积累弱点槽时);
				ds_accu.ObjectArgu1 = this;
				ds_accu.ObjectArgu2 = dar;
				RelatedBuffRef.Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ds_accu);

				RelatedGroupRef.ModifyCounter(Cache_CurrentAccumulate);
			}
		}



		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
			var lab = relatedBuff.Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			lab.RemoveAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
				_ABC_ProcessDamageTaken_OnDamageOnHP);
		}


	}
}