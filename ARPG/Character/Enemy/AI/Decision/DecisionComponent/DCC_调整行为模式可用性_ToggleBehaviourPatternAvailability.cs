using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_调整行为模式可用性_ToggleBehaviourPatternAvailability : BaseDecisionCommonComponent
	{
		[SerializeField,LabelText("将要启用的行为模式ID们"),ListDrawerSettings(ShowFoldout = true),
		 GUIColor(250f / 255f, 113f / 255f, 15f / 255f)]
		public string[] ToggleOnArray;



		[SerializeField, LabelText("将要禁用的行为模式ID们"), ListDrawerSettings(ShowFoldout = true),
		 GUIColor(250f / 255f, 113f / 255f, 15f / 255f)]
		public string[] ToggleOffArray;

		
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			if (ToggleOnArray != null && ToggleOnArray.Length > 0)
			{
				foreach (string perS in ToggleOnArray)
				{
					relatedBrain.BrainHandlerFunction.FindBehaviourPatternByUID(perS).IsAvailable = true;

				}
			}


			if (ToggleOffArray != null && ToggleOffArray.Length > 0)
			{
				foreach (string perS in ToggleOffArray)
				{
					relatedBrain.BrainHandlerFunction.FindBehaviourPatternByUID(perS).IsAvailable = false;

				}
			}

			
			
		}
		
		
		
	}
}