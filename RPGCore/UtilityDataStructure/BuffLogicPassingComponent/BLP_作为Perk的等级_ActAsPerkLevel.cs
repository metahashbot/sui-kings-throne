using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure.BuffLogicPassingComponent
{
	[Serializable]
	public class BLP_作为Perk的等级_ActAsPerkLevel : BaseBuffLogicPassingComponent
	{
		[SerializeField]
		public int Level;
		

		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_作为Perk的等级_ActAsPerkLevel>.Release(this);
		}
	}
}