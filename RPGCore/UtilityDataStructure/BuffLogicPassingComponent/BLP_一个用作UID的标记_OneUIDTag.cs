using System;
using System.Collections.Generic;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	public sealed class BLP_一个用作UID的标记_OneUIDTag : BaseBuffLogicPassingComponent
	{
		public string UID;
		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_一个用作UID的标记_OneUIDTag>.Release(this);
		}
	}
}