using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_更换碰撞信息组_SwitchCollisionInfoGroup : BaseDecisionCommonComponent
	{
		[SerializeField,LabelText("更换为——0是默认")]
		private int _collisionInfoGroupIndex = 0;
		
		

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{

			relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.SetCurrentCollisionInfoIndex(
				_collisionInfoGroupIndex);
		}
	}
}