using System;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class Buff_InvincibleAll : BaseRPBuff
	{

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamagePrelude_对接收方伤害流程序言部分,
				_ABC_ModifyDamageResult_OnDamagePreCalculate);
		}

		
		
		

		protected void _ABC_ModifyDamageResult_OnDamagePreCalculate(DS_ActionBusArguGroup ds)
		{
			var damageResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			damageResult.ResultLogicalType = RP_DamageResultLogicalType.InvincibleAllSoNothing;
			
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamagePrelude_对接收方伤害流程序言部分,
				_ABC_ModifyDamageResult_OnDamagePreCalculate);
			return base.OnBuffPreRemove();
		}

	}
}