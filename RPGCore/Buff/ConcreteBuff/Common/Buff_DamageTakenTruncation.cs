using System;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Enemy;
using ARPG.Manager;
using Global.ActionBus;
using RPGCore.AssistBusiness;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	[TypeInfoBox("伤害截断。\n" +
	             "监听受到致命伤/与伤害达到HP上")]
	public class Buff_DamageTakenTruncation : BaseRPBuff
	{

		[LabelText("高于此值的伤害会被截断到该值"), ShowInInspector, FoldoutGroup("配置", true)]
		public float DamageTruncation = 1f;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			var lab = parent.ReceiveBuff_GetRelatedActionBus();
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
				_ABC_TruncateDamageTaken_OnDamagePreTakenOnHP , 100);
		}


		private void _ABC_TruncateDamageTaken_OnDamagePreTakenOnHP(DS_ActionBusArguGroup ds)
		{
			var daResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			daResult.DamageAmount_AfterShield = Mathf.Min(daResult.DamageAmount_AfterShield, DamageTruncation);
			daResult.PopupDamageNumber = daResult.DamageAmount_AfterShield;
		}
		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var lab = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			lab.RemoveAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
				_ABC_TruncateDamageTaken_OnDamagePreTakenOnHP);
			return base.OnBuffPreRemove();
		}
		
		
	}
}