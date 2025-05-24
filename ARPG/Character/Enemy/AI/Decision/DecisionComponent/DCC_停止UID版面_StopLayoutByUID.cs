using System;
using ARPG.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_停止UID版面_StopLayoutByUID : BaseDecisionCommonComponent
	{

		[LabelText("需要停止的版面UID"), SerializeField]
		public string TargetUID;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			SubGameplayLogicManager_ARPG.Instance.ProjectileLayoutManagerReference.StopLayoutByUIDAndCaster(TargetUID,
				relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour);
		}
	}
}