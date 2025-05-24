using System;
using System.Collections.Generic;
using RPGCore.Buff;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	public sealed class BLP_关联血量的触发事件_LaunchEventRelateToHP : BaseBuffLogicPassingComponent
	{
		[LabelText("关联的事件和血量信息们")]
		public List<LaunchEventInfo_WithHP> InfoList;
		
		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_关联血量的触发事件_LaunchEventRelateToHP>.Release(this);
		}
	}
}