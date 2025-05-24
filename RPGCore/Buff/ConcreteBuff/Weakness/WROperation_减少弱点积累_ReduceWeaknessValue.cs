using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	/// <summary>
	/// <para>一次性操作</para>
	/// </summary>
	[Serializable]
	public class WROperation_减少弱点积累_ReduceWeaknessValue : BaseWeaknessAffectRule , I_WeaknessRuleAsOnetime
	{
		[SerializeField, LabelText("减少的值，填负的就是加弱点积累"), TitleGroup("===配置===")]
		public float ReduceValue;


		public void ProcessOnetimeEffect(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup groupRef)
		{
			if (groupRef.CurrentGroupActive)
			{
				groupRef.ModifyCounter(-ReduceValue);

			}
		}
		 


		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
			
		}
	}
}