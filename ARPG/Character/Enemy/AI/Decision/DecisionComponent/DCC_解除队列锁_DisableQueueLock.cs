using System;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_解除队列锁_DisableQueueLock : BaseDecisionCommonComponent
	{

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			relatedBrain.BrainHandlerFunction.DisableQueueClearLock();
		}
	}
}