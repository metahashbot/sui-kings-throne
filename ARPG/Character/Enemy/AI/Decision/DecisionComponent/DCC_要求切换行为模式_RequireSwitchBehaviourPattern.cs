using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_要求切换行为模式_RequireSwitchBehaviourPattern : BaseDecisionCommonComponent
	{
		[SerializeField, LabelText("目标行为模式ID"), GUIColor(250f / 255f, 113f / 255f, 15f / 255f)]
		public string TargetBehaviourPatternID;
		
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			relatedBrain.BrainHandlerFunction.SwitchBehaviourPattern(TargetBehaviourPatternID);
			
			
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 要求切换行为模式  ID：_{TargetBehaviourPatternID}_";
		}
	}
}