using System.Collections.Generic;
using RPGCore.UtilityDataStructure;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	public class DCC_施加Buff_ApplyBuffWithArguments : BaseDecisionCommonComponent
	{
		public List<ConSer_BuffApplyInfo> TryApplyBuffList = new List<ConSer_BuffApplyInfo>();
		

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var behaviourRef = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			foreach (ConSer_BuffApplyInfo perApplyInfo in TryApplyBuffList)
			{
				behaviourRef.ReceiveBuff_TryApplyBuff(perApplyInfo.BuffType,
					behaviourRef,
					behaviourRef,
					perApplyInfo.GetFullBLPList() );
			}
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 施加Buff {string.Join("," ,TryApplyBuffList)}";
		}
	}
}