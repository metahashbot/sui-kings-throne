using System;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WAR_禁用UID弱点_RemoveWeaknessByUID : BaseWeaknessAffectRule , I_WeaknessRuleAsOnetime
	{

		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
			
		} 
		
		public void ProcessOnetimeEffect(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup groupRef)
		{
			groupRef.CurrentGroupActive = false;
		}
	}
}