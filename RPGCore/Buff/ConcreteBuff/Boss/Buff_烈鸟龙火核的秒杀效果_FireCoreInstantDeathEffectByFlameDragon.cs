using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff.Boss
{
	[Serializable]
	public class Buff_烈鸟龙火核的秒杀效果_FireCoreInstantDeathEffectByFlameDragon : BaseRPBuff
	{
		
		[LabelText("成功施加后造成的伤害"), TitleGroup("===具体配置===")]
		public ConSer_DamageApplyInfo DamageApplyInfoOnTakeSuccess;

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);

			RP_DS_DamageApplyInfo dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromFromConSer(
				DamageApplyInfoOnTakeSuccess,
				Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
				Source_SelfReceiveFromObjectRef as I_RP_Damage_ObjectCanApplyDamage,
				null);
			RP_DS_DamageApplyResult dar = _damageAssistServiceRef.ApplyDamage(dai);

			dar.ReleaseToPool();


			return ds;
		}

	}
}