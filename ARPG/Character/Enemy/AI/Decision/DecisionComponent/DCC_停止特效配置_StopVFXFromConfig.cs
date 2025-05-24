using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_停止特效配置_StopVFXFromConfig : BaseDecisionCommonComponent
	{

		[LabelText("关联特效配置"), SerializeField, GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_VFXOnTakeEffect;

		[LabelText("立刻停止吗？"), SerializeField]
		public bool StopImmediate = false;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			if (_vfx_VFXOnTakeEffect != null && _vfx_VFXOnTakeEffect.Length > 1)
			{
				var targetIndex = relatedBrain.BrainHandlerFunction.AllVFXInfoList_RuntimeAll.FindIndex(info =>
					info._VFX_InfoID.Equals(_vfx_VFXOnTakeEffect, StringComparison.OrdinalIgnoreCase));
				if (targetIndex == -1)
				{
					return;
				}
				relatedBrain.BrainHandlerFunction.AllVFXInfoList_RuntimeAll[targetIndex].VFX_StopThis(StopImmediate);
			}
		}
		
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} {(StopImmediate ? "立刻": "" )} 停止特效：_{_vfx_VFXOnTakeEffect}_";
		}
	}
}