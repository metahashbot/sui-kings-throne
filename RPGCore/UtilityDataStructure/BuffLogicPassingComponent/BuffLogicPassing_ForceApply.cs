using System;
using System.Collections.Generic;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	public sealed class BuffLogicPassing_ForceApply : BaseBuffLogicPassingComponent
	{
		public bool ForceApply = true;
		public override void ReleaseOnReturnToPool()
		{ 
			GenericPool<BuffLogicPassing_ForceApply>.Release(this);
		}
	}
}