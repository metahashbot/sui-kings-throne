using System;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_要求刷新仇恨_RequireRefreshHatredTarget : BaseDecisionCommonComponent
	{

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			relatedBrain.BrainHandlerFunction.RefreshHatredTarget();
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 要求刷新仇恨";
		}
	}
}