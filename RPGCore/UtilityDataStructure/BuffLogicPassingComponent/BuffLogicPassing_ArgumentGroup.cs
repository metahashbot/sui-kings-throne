using System;
using System.Collections.Generic;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	public sealed class BuffLogicPassing_ArgumentGroup : BaseBuffLogicPassingComponent
	{
		public int GroupIndex = 0;
		public int IntArgu1;
		public int IntArgu2;
		public float FloatArgu1;
		public float FloatArgu2;
		public float FloatArgu3;
		public float FloatArgu4;
		public float FloatArgu5;
		public float FloatArgu6;
        
		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BuffLogicPassing_ArgumentGroup>.Release(this);
		}
	}
}