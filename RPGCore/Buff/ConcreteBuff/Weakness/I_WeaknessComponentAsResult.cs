namespace RPGCore.Buff.ConcreteBuff.Common
{
	public interface I_WeaknessComponentAsResult
	{



		/// <summary>
		///<para>深拷贝并注册进去。由于是作为“结果”的，所以不需要像 监听 那样还需要额外一步对运行时实例的处理</para>
		/// </summary>
		/// <param name="group"></param>
		public abstract void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group);
		
		
		public abstract void TriggerWeaknessResult();
	}
}