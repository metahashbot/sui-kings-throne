using System;
using RPGCore.UtilityDataStructure;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class BLP_ApplyFromConfig_由表中直接施加 : BaseBuffLogicPassingComponent
	{
		
		

		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_ApplyFromConfig_由表中直接施加>.Release(this);
		}
	}
}