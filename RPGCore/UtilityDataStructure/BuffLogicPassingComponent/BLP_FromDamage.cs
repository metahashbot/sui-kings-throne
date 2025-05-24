using System;
using System.Collections.Generic;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure
{
	/// <summary>
	/// <para>Buff体系内部用来传递的，表明这是由于伤害流程而产生的一次buff施加业务。</para>
	/// </summary>
	[Serializable]
	public sealed class BLP_FromDamage : BaseBuffLogicPassingComponent
	{
		public RP_DS_DamageApplyResult RelatedDamageResultInfoRef;
		
		public override void ReleaseOnReturnToPool()
		{
			RelatedDamageResultInfoRef = null;
			GenericPool<BLP_FromDamage>.Release(this);
		}
	}
}