using System;
using Global;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[Serializable]
	[InfoBox("一个空动画决策，不播动画，只使用它结构上的功能。决策会在【占用的帧数】之后结束。")]
	public class DH_空动画决策_EmptyAnimationDecision : BaseDecisionHandler
	{

		[SerializeField,LabelText("占用的帧数"),FoldoutGroup("配置")]
		private int _holdFrameCount =1;
		private int _willEndFrameIndex;

		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds = base.OnDecisionBeforeStartExecution(withSideEffect);
			_willEndFrameIndex = BaseGameReferenceService.CurrentFixedFrame + _holdFrameCount;
			return ds;
		}


		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			if (cf > _willEndFrameIndex)
			{
				OnDecisionNormalComplete();
				return;
			}
		}




	}
}