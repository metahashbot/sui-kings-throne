using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class Buff_伤害微调_DamageTweak : BaseRPBuff
	{


		/// <summary>
		/// 受到的受害减少
		/// </summary>
		public float DamageReceiveTweakMin = 0.05f;
		/// <summary>
		///  受到的受害增加
		///  </summary>
		public float DamageReceiveTweakMax = 0.1f;

		public float DamageCastTweakMin = 0.05f;

		public float DamageCastTweakMax = 0.1f;

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
			var lab = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_TweakDamageOnReceive,
				999);
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_TweakDamageOnCast,
				999);
			return base.OnBuffInitialized(blps);
		}



		protected virtual void _ABC_TweakDamageOnReceive(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			var rr = UnityEngine.Random.Range(DamageReceiveTweakMin, DamageReceiveTweakMax);

			dar.CP_DamageAmount_DPart.MultiplyPart -= rr;
		}

		protected virtual void _ABC_TweakDamageOnCast(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			var rr = UnityEngine.Random.Range(DamageCastTweakMin, DamageCastTweakMax);
			dar.CP_DamageAmount_DPart.MultiplyPart += rr;
			
		}




		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			// switch (blp)
			// {
			// 	case BLP_DamageTweakInfo tweakInfo:
			//
			// 		break;
			// }
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var lab = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus();
			lab.RemoveAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_TweakDamageOnReceive);
			lab.RemoveAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_TweakDamageOnCast);
			return base.OnBuffPreRemove();
		}




		// [Serializable]
		// public class BLP_DamageTweakInfo : BaseBuffLogicPassingComponent
		// {
		// 	/// <summary>
		// 	/// 受到的受害减少
		// 	/// </summary>
		// 	public float DamageReceiveTweakMin = 0.05f;
		// 	/// <summary>
		// 	///  受到的受害增加
		// 	///  </summary>
		// 	public float DamageReceiveTweakMax = 0.1f;
		//
		// 	public float DamageCastTweakMin = 0.05f;
		//
		// 	public float DamageCastTweakMax = 0.1f;
		//
		//
		// 	public static BLP_DamageTweakInfo GetFromCopy(BLP_DamageTweakInfo copyFrom = null)
		// 	{
		// 		var ret = new BLP_DamageTweakInfo();
		// 		if (copyFrom != null)
		// 		{
		// 			ret.DamageReceiveTweakMin = copyFrom.DamageReceiveTweakMin;
		// 			ret.DamageReceiveTweakMax = copyFrom.DamageReceiveTweakMax;
		// 			ret.DamageCastTweakMin = copyFrom.DamageCastTweakMin;
		// 			ret.DamageCastTweakMax = copyFrom.DamageCastTweakMax;
		// 		}
		// 		return ret;
		// 	}
		//
		//
		// 	public override void DeepCopyToRuntimeList(
		// 		BaseBuffLogicPassingComponent copyFrom,
		// 		List<BaseBuffLogicPassingComponent> targetList)
		// 	{
		// 		var copyFrom_ = copyFrom as BLP_DamageTweakInfo;
		// 		targetList.Add(GetFromCopy(copyFrom_));
		// 	}
		// 	public override void Release()
		// 	{
		// 		return;
		// 	}
		// }
	}
}