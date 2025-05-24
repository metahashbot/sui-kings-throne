using System;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
using ARPG.Character.Base;

namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_开始特效配置_StartVFX : BaseDecisionCommonComponent
	{

		[LabelText("关联特效配置"), SerializeField, GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_VFXOnTakeEffect;


		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			if (_vfx_VFXOnTakeEffect != null && _vfx_VFXOnTakeEffect.Length > 1)
			{
					relatedBrain.BrainHandlerFunction._VFX_GetAndSetBeforePlay(_vfx_VFXOnTakeEffect,
							true,
							relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetRelatedArtHelper())
						?._VFX__10_PlayThis();
				
				

			}
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 开始特效：_{_vfx_VFXOnTakeEffect}_";
		}
	}
}