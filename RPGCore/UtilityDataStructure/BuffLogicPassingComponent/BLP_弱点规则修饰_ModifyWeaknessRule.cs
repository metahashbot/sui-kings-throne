using System;
using System.Collections.Generic;
using RPGCore.Buff.ConcreteBuff.Common;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure.BuffLogicPassingComponent
{
	[Serializable]
	public class BLP_弱点规则修饰_ModifyWeaknessRule : BaseBuffLogicPassingComponent
	{

		[SerializeReference]
		public BaseWeaknessAffectRule RelatedRule;

		
		[SerializeReference]
		public List<BaseWeaknessAffectRule> RelatedRuleList;

		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_弱点规则修饰_ModifyWeaknessRule>.Release(this);
		}
	}
}