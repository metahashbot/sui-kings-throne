namespace RPGCore.Buff.ConcreteBuff.Common
{
	/// <summary>
	/// 作为一次性的弱点规则。通常用于某些施加Buff的情况
	/// </summary>
	public interface I_WeaknessRuleAsOnetime
	{
		public abstract void ProcessOnetimeEffect(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup groupRef);

	}
}