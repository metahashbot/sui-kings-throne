using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.Config;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.BuffComponent
{
	[Serializable]
	public abstract class BaseBuffTriggerSource
	{
		[HideInInspector]
		public BaseRPBuff SelfRelatedBuffRef;
	}

	[Serializable]
	public class BuffTriggerSource_ActionBusCallbackComponent : BaseBuffTriggerSource
	{
		public ActionBus_ActionTypeEnum RelationType;
		[LabelText("优先级，越小越先"),HorizontalGroup("Config")]
		public int Order = 0;
		[LabelText("全局吗？"), HorizontalGroup("Config")]
		public bool IsGlobalListener;
		
	}

}