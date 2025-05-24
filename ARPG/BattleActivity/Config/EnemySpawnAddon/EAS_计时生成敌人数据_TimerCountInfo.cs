using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class EAS_计时生成敌人数据_TimerCountInfo : BaseEnemySpawnAddon
	{
		[LabelText( "挑选权重")]
		public float DefaultPickWeight = 20f;

		[LabelText("挑选后下次挑选的时间间隔")]
		public float PickInterval = 2f;
        
		[LabelText("该类敌人的上限")]
		public int MaxCount = 10;
		
		[LabelText("共用上限组吗")]
		public bool UseSharedMaxCount = true;
		
		[LabelText("共用上限组的ID")]
		[ShowIf(nameof(UseSharedMaxCount))]
		public string SharedMaxCountID;


		[NonSerialized, ShowInInspector, ReadOnly, LabelText("下次可用时间")]
		public float NextAvailableTime;


		public override void ResetOnReturn()
		{
GenericPool<EAS_计时生成敌人数据_TimerCountInfo>.Release(this);
			return;
			
		}
	}
}