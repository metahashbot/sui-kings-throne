using System;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_生成特效配置_SpawnVFXFromConfig : BaseDecisionCommonComponent
	{

		[LabelText("关联特效配置"), SerializeField, GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_VFXOnTakeEffect;


		[LabelText("播放设置——强制重播？")]
		public bool _playSetting_ForceReplay = true;


		[LabelText("   播放设置——重新定位？")]
		public bool _playSetting_Reposition = true;



		[InfoBox("此选项仅用于 基于 DH_通用常规攻击 的决策")]
		[LabelText("        攻击方向使用 √：进入决策时获得方向  || 口：攻击时的获得方向"), SerializeField]
		public bool UseDirectionOnEnter = false;


		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var decisionHandler = relatedBrain.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler;
			var behaviour = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			if (!string.IsNullOrEmpty(_vfx_VFXOnTakeEffect))
			{
				switch (decisionHandler)
				{
					case DH_通用常规攻击_CommonAttackWithAnimation attack:
						if (!UseDirectionOnEnter)
						{
							decisionHandler.SelfRelatedBrainInstanceRef.BrainHandlerFunction
								._VFX_GetAndSetBeforePlay(_vfx_VFXOnTakeEffect,
									true,
									behaviour.GetRelatedVFXContainer())
								?._VFX__10_PlayThis(_playSetting_ForceReplay, _playSetting_Reposition);
						}
						else
						{

							decisionHandler.SelfRelatedBrainInstanceRef.BrainHandlerFunction
								._VFX_GetAndSetBeforePlay(_vfx_VFXOnTakeEffect,
									true,
									behaviour.GetRelatedVFXContainer())
								?._VFX__10_PlayThis(_playSetting_ForceReplay, _playSetting_Reposition)
								._VFX__3_SetDirectionOnForwardOnGlobalY0(attack.ToHatredTargetDirectionOnEnterDecision);
						}
						break;
					default:
						decisionHandler.SelfRelatedBrainInstanceRef.BrainHandlerFunction
							._VFX_GetAndSetBeforePlay(_vfx_VFXOnTakeEffect,
								true,
								behaviour.GetRelatedVFXContainer())
							?._VFX__10_PlayThis(_playSetting_ForceReplay, _playSetting_Reposition);
						break;
				}

			}
		}
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 生成特效：[{_vfx_VFXOnTakeEffect}]";
		}
	}
}