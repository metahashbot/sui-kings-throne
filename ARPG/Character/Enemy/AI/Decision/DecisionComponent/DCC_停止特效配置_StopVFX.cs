using System;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_停止特效配置_StopVFX : BaseDecisionCommonComponent
	{

		[SerializeField, LabelText("需要停止的特效配置"), GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_needToStop;

		[SerializeField, LabelText("√:立刻停止 || 口:常规停止")]
		public bool StopImmediate = false;
		

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var vfx = relatedBrain.BrainHandlerFunction._VFX_JustGet(_vfx_needToStop);
			if (vfx != null)
			{
				vfx.VFX_StopThis(StopImmediate);
			}
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} {(StopImmediate ? "立刻": "" )} 停止特效：_{_vfx_needToStop}_";
		}
	}
}