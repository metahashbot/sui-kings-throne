using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Timeline;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_播放Timeline特效_PlayTimeline : BaseDecisionCommonComponent
	{
		[InfoBox("Timeline特效只能跟随敌人朝【正左-正右】两个方向。\n" + "它通过翻转PlayableDirector所在的Transform来实现。不要试图让特效随意旋转！")]
		[SerializeField, LabelText("timeline-pd")]
		public TimelineAsset RelatedTimelineAsset;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var pd = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetRelatedPlayableDirector();
			if (pd == null)
			{
				Debug.LogError(
					$"角色{relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}没有TimelineDirector，在播什么");
				return;
			}
			pd.Play(RelatedTimelineAsset);
		}
	}
}