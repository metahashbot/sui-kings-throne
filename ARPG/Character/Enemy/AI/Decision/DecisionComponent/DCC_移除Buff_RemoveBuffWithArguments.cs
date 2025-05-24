using System;
using System.Collections.Generic;
using RPGCore.Buff;
using Sirenix.OdinInspector;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_移除Buff_RemoveBuffWithArguments : BaseDecisionCommonComponent
	{

		[LabelText("将要试图移除的Buff列表")]
		public List<RolePlay_BuffTypeEnum> RequiredToRemoveList = new List<RolePlay_BuffTypeEnum>();
		
		
		
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{

			if (RequiredToRemoveList != null && RequiredToRemoveList.Count > 0)
			{
				var behaviourRef = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
				foreach (RolePlay_BuffTypeEnum perType in RequiredToRemoveList)
				{
					if (behaviourRef.ReceiveBuff_CheckTargetBuff(perType) != BuffAvailableType.NotExist)
					{
						behaviourRef.ReceiveBuff_RemoveTargetBuff(perType);
					}
				}
			}
			
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 移除Buff {string.Join("," ,RequiredToRemoveList)}";
		}
	}
}