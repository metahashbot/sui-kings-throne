using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_重新面向当前仇恨_ReFaceToCurrentHatred : BaseDecisionCommonComponent
	{

		[SerializeField,LabelText("√:面向 | 口:背向")]
		public bool _faceHatredTarget = true;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			if (relatedBrain.BrainHandlerFunction.CurrentRunningDecision == null)
			{
				return;
			}
			
			relatedBrain.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler.QuickFaceHateTargetDirection(
				!_faceHatredTarget);
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 重新 {(_faceHatredTarget ? "面向" : "背向")} 当前仇恨 ";
		}
	}
}