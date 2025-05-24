using System;
using Global.ActionBus;
using Sirenix.OdinInspector;
namespace RPGCore.Skill.Config
{
	[Serializable]
	public class SkillActionBusCallbackTriggerSource
	{
		public ActionBus_ActionTypeEnum RelationType;
		[LabelText("优先级，越小越优先"), HorizontalGroup("Config")]
		public int Order = 0;
		[LabelText("全局吗？"), HorizontalGroup("Config")]
		public bool IsGlobalListener;

	}
}