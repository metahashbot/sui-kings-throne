using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Buff.ConcreteBuff.Skill.RedFang
{
	
	[Serializable]
	public class BloodPoison_血毒 : BaseRPBuff
	{
		
		[SerializeField, LabelText("当前伤害增加百分比"), SuffixLabel("%")]
		public float CurrentDamageBonusPartial;
		
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
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ProcessDamageBonus_OnDamagePreTakenHP);
			return base.OnBuffInitialized(blps);
		}

		private void _ABC_ProcessDamageBonus_OnDamagePreTakenHP(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			dar.CP_DamageAmount_DPart.MultiplyPart += (CurrentDamageBonusPartial / 100f);
		}
        
		protected override void ClearAndUnload()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ProcessDamageBonus_OnDamagePreTakenHP);
			base.ClearAndUnload();
		}
			
		}

	}
