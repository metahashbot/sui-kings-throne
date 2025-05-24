using System.Collections.Generic;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	/// <summary>
	/// <para>一个作为规则监听的 弱点规则。它需要 【深拷贝】到这个规则组的整体容器中去</para>
	/// </summary>
	public interface I_WeaknessRuleAsListener
	{


		/// <summary>
		///<para>仅仅是注册到组中去。而且加进去的是深拷贝的那个。所以还要调用下面的 ProcessOnRegisterToRuntimeGroup 来对运行时的实例来执行真正的监听业务</para>
		/// </summary>
		/// <param name="group"></param>
		public abstract void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group);
		/// <summary>
		/// <para>当一个深拷贝的规则 确定被加入运行时的规则组容器中的时候，执行它监听该有的业余，比如注册事件线之类的</para>
		/// </summary>
		/// <param name="group"></param>
		public abstract void ProcessOnRegisterToRuntimeGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group);
	}
}