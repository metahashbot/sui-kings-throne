using System;
using System.Collections.Generic;
using GameplayEvent;
using GameplayEvent.SO;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_触发游戏性事件_LaunchGameplayEvent : BaseDecisionCommonComponent
	{
		[SerializeField, LabelText("将要触发事件的UID"), GUIColor(255f / 255f, 248f / 255f, 10f / 255f)]
		public string EventUID;

		[SerializeField, LabelText("将要触发事件预制")]
		public List<SOConfig_PrefabEventConfig> PrefabEventConfigs = new List<SOConfig_PrefabEventConfig>();

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var DS = new DS_GameplayEventArguGroup();
			DS.ObjectArgu1 = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			if (!string.IsNullOrEmpty(EventUID))
			{
				GameplayEventManager.Instance.StartGameplayEvent(EventUID, DS);
			}
			foreach (var perConfig in PrefabEventConfigs)
			{
				GameplayEventManager.Instance.StartGameplayEvent(perConfig, DS);
			}
		}
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 触发游戏性事件  UID：_{EventUID}_ && {string.Join("," ,PrefabEventConfigs)}";
		}
	}
}