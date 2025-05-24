using System;
using Global.ActionBus;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_关闭描边预设任务_StopOutlinePresetTask : BaseDecisionCommonComponent
	{

		[Sirenix.OdinInspector.LabelText("关联描边预设"), SerializeField, Sirenix.OdinInspector.GUIColor(187f / 255f, 1f, 0f)]
		public string _outlinePresetName;
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var ds_stop =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AnimationHelper_OnRemoveOutlineTask_当要求移除描边任务);
			ds_stop.ObjectArgu1 = _outlinePresetName;
			relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetRelatedActionBus()
				.TriggerActionByType(ds_stop);
		}

		public override string GetElementNameInList()
		{ 
			return $"{GetBaseCustomName()} 关闭描边预设任务  描边预设：_{_outlinePresetName}_";
		}
	}
}