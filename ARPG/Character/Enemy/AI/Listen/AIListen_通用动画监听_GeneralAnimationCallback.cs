using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	public class AIListen_通用动画监听_GeneralAnimationCallback : BaseAIListenComponent
	{


		[SerializeField, LabelText("【作用】")]
		public DCCConfigInfo SelfDCCConfigInfo = new DCCConfigInfo();

		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);
			SelfDCCConfigInfo.ProcessRuntimeBuild();
			var lab = brainRef.BrainHandlerFunction.SelfLocalActionBusRef;
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationStart_动画通用开始,
				_ABC_Process_OnAnimationStart);
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_Process_OnAnimationComplete);
			lab.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件,
				_ABC_Process_OnAnimationCustomEvent);
			


		}

		private void _ABC_Process_OnAnimationStart(DS_ActionBusArguGroup ds)
		{
					foreach (BaseDecisionCommonComponent perDCC in SelfDCCConfigInfo
						.CommonComponents_RuntimeAll)
					{
						if (perDCC.RequireAnimationMatching &&
						    perDCC.AnimationEventPreset == AnimationEventPresetEnumType.Start_开始 &&
						    perDCC._AN_RelatedAnimationConfigName.Equals(
							    (ds.ObjectArgu2 as AnimationInfoBase).ConfigName,
							    StringComparison.OrdinalIgnoreCase))
						{
							perDCC.EnterComponent(RelatedAIBrainRuntimeInstance);
						}
					}
				
			
			
			
		}
		private void _ABC_Process_OnAnimationComplete(DS_ActionBusArguGroup ds)
		{
			foreach (BaseDecisionCommonComponent perDCC in SelfDCCConfigInfo.CommonComponents_RuntimeAll)
			{
				if (perDCC.RequireAnimationMatching &&
				    perDCC.AnimationEventPreset == AnimationEventPresetEnumType.Complete_结束 &&
				    perDCC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArgu2 as AnimationInfoBase).ConfigName,
					    StringComparison.OrdinalIgnoreCase))
				{
					perDCC.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
		}
		private void _ABC_Process_OnAnimationCustomEvent(DS_ActionBusArguGroup ds)
		{
			foreach (BaseDecisionCommonComponent perDCC in SelfDCCConfigInfo.CommonComponents_RuntimeAll)
			{
				if (perDCC.RequireAnimationMatching &&
				    perDCC.AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义 &&
				    perDCC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArguStr as AnimationInfoBase).ConfigName,
					    StringComparison.OrdinalIgnoreCase) && perDCC.CustomEventString.Trim()
					    .Equals(ds.ObjectArgu2 as string, StringComparison.OrdinalIgnoreCase))
				{
					perDCC.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
		}



	}
	
	
	
}