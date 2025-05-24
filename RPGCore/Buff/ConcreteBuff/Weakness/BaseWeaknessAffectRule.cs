using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	/// <summary>
	/// <para>基本弱点生效规则</para>
	/// <para>由于它实际上，总是被用在BLP传递。然后又因为弱点Buff本身使用的也是这个数据类型。所以实际上真的被塞到Buff里面的时候，【必须】要深拷贝</para>
	/// </summary>
	[Serializable]
	public abstract class BaseWeaknessAffectRule
	{
		[SerializeField, LabelText("归属的弱点规则组UID"), TitleGroup("===配置===")] [PropertyOrder(-1)]
		public string RelatedWeaknessUID;

		[NonSerialized, ReadOnly, LabelText("关联的规则组Ref,运行时"), TitleGroup("===运行时===")]
		public Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup RelatedGroupRef;
		
		[NonSerialized,ReadOnly,LabelText("关联的Buff，运行时"),TitleGroup("===运行时===")]
		public Buff_通用敌人弱点_CommonEnemyWeakness RelatedBuffRef;

		public abstract void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff);
		public virtual void FixedUpdateTick(float ct, int cf, float delta){}
	
	}
}