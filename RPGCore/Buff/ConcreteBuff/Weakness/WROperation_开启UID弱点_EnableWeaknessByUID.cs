using System;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WROperation_开启UID弱点_EnableWeaknessByUID : BaseWeaknessAffectRule , I_WeaknessRuleAsOnetime
	{



		public void ProcessOnetimeEffect(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup groupRef)
		{
			groupRef.CurrentGroupActive = true;
		}
		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
			return;
		}
	}
}