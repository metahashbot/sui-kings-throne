using System;
using ARPG.Manager;
using Global.ActionBus;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_开启描边预设任务_StartOutlinePresetTask : BaseDecisionCommonComponent
	{
		[Sirenix.OdinInspector.LabelText("关联描边预设"), SerializeField, Sirenix.OdinInspector.GUIColor(187f / 255f, 1f, 0f)]
		public string _outlinePresetName;
		 [ Sirenix.OdinInspector.LabelText("持续时间"), SerializeField, Sirenix.OdinInspector.GUIColor(187f / 255f, 1f, 0f)]
		public float _duration;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var ds_outline =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AnimationHelper_OnOutlineTaskRequired_当要求新的描边任务);
			ds_outline.ObjectArgu1 =
				SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference.GetOutlinePresetTaskByPresetID(
					_outlinePresetName);
			ds_outline.ObjectArgu2 = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			ds_outline.ObjectArguStr = _outlinePresetName;
			ds_outline.FloatArgu1 = _duration;
			relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetRelatedActionBus()
				.TriggerActionByType(ds_outline);
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 开启描边预设任务  描边预设：_{_outlinePresetName}_ 持续时间：_{_duration}_";
		}
	}
}