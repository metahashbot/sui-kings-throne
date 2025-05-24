using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Timeline;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_停止Timeline内容_StopTimeline : BaseDecisionCommonComponent
	{
		[SerializeField,LabelText("需要匹配Timeline资产名？")]
		public bool NeedMatchTimelineAssetName = false;
		 [SerializeField,LabelText("将要停止的Timeline资产")]
		[ShowIf("@this.NeedMatchTimelineAssetName")]
		public TimelineAsset TimelineAssetToStop;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var behaviour = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			var directorRef = behaviour.GetRelatedPlayableDirector();
			if (directorRef == null)
			{
				Debug.LogError($"角色{behaviour.name}并没有Timeline，啥都没停");
				return;
			}
			if (NeedMatchTimelineAssetName)
			{
				if (directorRef.playableAsset.name.Equals(TimelineAssetToStop.name))
				{
					directorRef.Stop();
				}
			}
			else
			{
				directorRef .Stop();
			}
			return;
		}
	}
}