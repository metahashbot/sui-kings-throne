using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.UtilityDataStructure;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class Buff_光等压制_LightSuppress : BaseRPBuff
	{



		/// <summary>
		/// <para>所有对血量的伤害增加这么多</para>
		/// </summary>
		public float CurrentDamageBonusPartial { get; private set; }



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ProcessDamageBonus_OnDamageTakenOnHP,
				95);
			return base.OnBuffInitialized(blps);
		}

		private void _ABC_ProcessDamageBonus_OnDamageTakenOnHP(DS_ActionBusArguGroup ds)
		{
			// var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			// dar.TakenOnHealthPower *= (1f + CurrentDamageBonusPartial / 100f);
		}




		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_压光参数_LightSuppressParam lightSuppressParam:
					CurrentDamageBonusPartial = lightSuppressParam.LightSuppressPercent;



					break;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ProcessDamageBonus_OnDamageTakenOnHP);
			return base.OnBuffPreRemove();
		}

		[Serializable]
		public class BLP_压光参数_LightSuppressParam : BaseBuffLogicPassingComponent
		{
			public float LightSuppressPercent;
			
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_压光参数_LightSuppressParam>.Release(this);
			}
		}

	}
}