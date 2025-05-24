using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	public class AIListen_仇恨有效性监听_HatredTargetValidListen : BaseAIListenComponent
	{
		
		// [SerializeReference,LabelText("直属=副作用——当获得有效仇恨时")]
		// public List<BaseDecisionCommonComponent> SideEffectOnHatredTargetValid  = new List<BaseDecisionCommonComponent>();
		//
		// [SerializeField, LabelText("文件=副作用——当获得有效仇恨时"), ListDrawerSettings(ShowFoldout = true),
		//  InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		// public List<SOConfig_PresetSideEffects> SideEffectOnHatredTargetValid_File = new List<SOConfig_PresetSideEffects>();

		[SerializeField,]
		public DCCConfigInfo SideEffectOnHatredTargetValid_DCCInfo;

		
		// //
		// [SerializeReference,LabelText("直属=副作用——当仇恨目标变化时")]
		// public List<BaseDecisionCommonComponent> SideEffectOnHatredTargetChange  = new List<BaseDecisionCommonComponent>();
		//
		// [SerializeField,LabelText("文件=副作用——当仇恨目标变化时") ,ListDrawerSettings(ShowFoldout = true),
		//  InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		// public List<SOConfig_PresetSideEffects> SideEffectOnHatredTargetChange_File  = new List<
		// 	SOConfig_PresetSideEffects>();


		[SerializeField,]
		public DCCConfigInfo SideEffectOnHatredTargetChange_DCCInfo;
		
		
        
		 
		
		// [SerializeReference,LabelText("直属-副作用——当无法获取仇恨目标时")]
		// public List<BaseDecisionCommonComponent> SideEffectOnHatredTargetInvalid  = new List<BaseDecisionCommonComponent>();
		//
		// [SerializeField,LabelText("文件-副作用——当无法获取仇恨目标时") ,ListDrawerSettings(ShowFoldout = true),
		//  InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		 
		// public List<SOConfig_PresetSideEffects> SideEffectOnHatredTargetInvalid_File  = new List<
		// 	SOConfig_PresetSideEffects>();


		[SerializeField,]
		public DCCConfigInfo SideEffectOnHatredTargetInvalid_DCCInfo;


		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			SideEffectOnHatredTargetValid_DCCInfo.CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			SideEffectOnHatredTargetValid_DCCInfo.BuildRuntimeDCC(ref SideEffectOnHatredTargetValid_DCCInfo
				.CommonComponents_RuntimeAll);



			SideEffectOnHatredTargetChange_DCCInfo.CommonComponents_RuntimeAll =
				new List<BaseDecisionCommonComponent>();
			SideEffectOnHatredTargetChange_DCCInfo.BuildRuntimeDCC(ref SideEffectOnHatredTargetChange_DCCInfo
				.CommonComponents_RuntimeAll);



			SideEffectOnHatredTargetInvalid_DCCInfo .CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			SideEffectOnHatredTargetInvalid_DCCInfo.BuildRuntimeDCC(ref SideEffectOnHatredTargetInvalid_DCCInfo
				.CommonComponents_RuntimeAll);
		
			
			 
			base.InitializeAndProcessRegister(brainRef);
			brainRef.BrainHandlerFunction.SelfLocalActionBusRef.RegisterAction(ActionBus_ActionTypeEnum
					.L_AIDecision_OnHatredTargetBecomeValid_当仇恨目标变为有效目标,
				_ABC_ProcessSideEffectOnHatredTargetValid);
			brainRef.BrainHandlerFunction.SelfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_AIDecision_OnHatredTargetChanged_当仇恨目标发生了变换,
				_ABC_ProcessSideEffectOnHatredTargetChanged);
			brainRef.BrainHandlerFunction.SelfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_AIDecision_OnHatredTargetBecomeInvalid_当仇恨目标变为无效目标,
				_ABC_ProcessSideEffectOnHatredTargetInvalid);
		}

		public override void UnRegisterListenInActionBus()
		{
			base.UnRegisterListenInActionBus();
			RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef.RemoveAction(
				ActionBus_ActionTypeEnum.L_AIDecision_OnHatredTargetBecomeValid_当仇恨目标变为有效目标,
				_ABC_ProcessSideEffectOnHatredTargetValid);
			RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef.RemoveAction(
				ActionBus_ActionTypeEnum.L_AIDecision_OnHatredTargetChanged_当仇恨目标发生了变换,
				_ABC_ProcessSideEffectOnHatredTargetChanged);
			RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef.RemoveAction(
				ActionBus_ActionTypeEnum.L_AIDecision_OnHatredTargetBecomeInvalid_当仇恨目标变为无效目标,
				_ABC_ProcessSideEffectOnHatredTargetInvalid);
			SideEffectOnHatredTargetValid_DCCInfo.ClearOnUnload();
			SideEffectOnHatredTargetChange_DCCInfo.ClearOnUnload();
			SideEffectOnHatredTargetInvalid_DCCInfo.ClearOnUnload();

		}


		protected void _ABC_ProcessSideEffectOnHatredTargetValid(DS_ActionBusArguGroup ds)
		{
			if (SideEffectOnHatredTargetValid_DCCInfo.CommonComponents_RuntimeAll != null)
			{
				foreach (BaseDecisionCommonComponent perComponent in SideEffectOnHatredTargetValid_DCCInfo.CommonComponents_RuntimeAll)
				{
					if (perComponent == null)
					{
						continue;
					}
					perComponent.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
		}


		protected void _ABC_ProcessSideEffectOnHatredTargetChanged(DS_ActionBusArguGroup ds)
		{
			if (SideEffectOnHatredTargetChange_DCCInfo.CommonComponents_RuntimeAll != null)
			{
				foreach (BaseDecisionCommonComponent perComponent in SideEffectOnHatredTargetChange_DCCInfo
					.CommonComponents_RuntimeAll)
				{
					if (perComponent == null)
					{
						continue;
					}
					perComponent.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
		}

		protected void _ABC_ProcessSideEffectOnHatredTargetInvalid(DS_ActionBusArguGroup ds)
		{
			if (SideEffectOnHatredTargetInvalid_DCCInfo.CommonComponents_RuntimeAll != null)
			{
				foreach (BaseDecisionCommonComponent perComponent in SideEffectOnHatredTargetInvalid_DCCInfo
					.CommonComponents_RuntimeAll)
				{
					if (perComponent == null)
					{
						continue;
					}
					perComponent.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
		}
		
		
		
	}
}