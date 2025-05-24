using System;
using ARPG.Character.Base;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_转身_TurnFace : BaseDecisionCommonComponent
	{

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			BaseARPGCharacterBehaviour behaviour = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			if (behaviour.GetRelatedArtHelper().CurrentFaceLeft)
			{
				behaviour.GetRelatedArtHelper().SetFaceLeft(false);
			}
			else
			{
				behaviour.GetRelatedArtHelper().SetFaceLeft(true);
			}
		}

		public override string GetElementNameInList()
		{ 
			return $"{GetBaseCustomName()} 转身";
		}
	}
}