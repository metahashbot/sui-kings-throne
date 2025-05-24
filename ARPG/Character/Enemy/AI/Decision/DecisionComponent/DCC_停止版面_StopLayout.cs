using System;
using ARPG.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	
	[Serializable]
	[TypeInfoBox("停止版面时会检查版面的caster是否为自己，所以如果已经去世了就停不到了")]
	public class DCC_停止版面_StopLayout : BaseDecisionCommonComponent
	{
		[SerializeField,LabelText("需要停止的版面UID")]
		public string NeedToStop;
		
		[SerializeField,LabelText("需要清理已存在投射物？")]
		public bool NeedToClearExistProjectile = false;


		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			SubGameplayLogicManager_ARPG.Instance.ProjectileLayoutManagerReference.TryStopLayout(NeedToStop,
				relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour,
				NeedToClearExistProjectile);
		}
		
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 停止版面：_{NeedToStop}_";
		}
	}
}