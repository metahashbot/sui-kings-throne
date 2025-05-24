using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Global;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable] 
	[TypeInfoBox("动画的监听不能用这个。需要使用“通用动画监听”")]
	public class AIListen_通用监听_SimpleCallback : BaseAIListenComponent
	{
		[LabelText("这是一个全局监听吗？")]
		[FoldoutGroup("配置",true)]
		public bool IsGlobal;
		[LabelText("事件线上的回调类型")]
		[FoldoutGroup("配置", true)]
		public ActionBus_ActionTypeEnum ActionType;


		// [SerializeReference, LabelText("响应的副作用们"), FoldoutGroup("配置", true)]
		// public List<BaseDecisionCommonComponent> CommonComponents = new List<BaseDecisionCommonComponent>();
		//
		// [SerializeField, LabelText("文件=副作用们"), ListDrawerSettings(ShowFoldout = true),
		//  InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		// public List<SOConfig_PresetSideEffects> CommonComponents_File = new List<SOConfig_PresetSideEffects>();

		[SerializeField,]
		public DCCConfigInfo SelfDCCConfigInfo = new DCCConfigInfo();
		
		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);
			
			SelfDCCConfigInfo.CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			SelfDCCConfigInfo.BuildRuntimeDCC(ref SelfDCCConfigInfo.CommonComponents_RuntimeAll);
			
			if (IsGlobal)
			{
				GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionType, _ABC_ProcessSideEffects_OnTriggered);
			}
			else
			{
				brainRef.BrainHandlerFunction.SelfLocalActionBusRef.RegisterAction(ActionType,
					_ABC_ProcessSideEffects_OnTriggered);
			}

		}


		protected virtual void _ABC_ProcessSideEffects_OnTriggered(DS_ActionBusArguGroup ds)
		{
			foreach (var commonComponent in SelfDCCConfigInfo.CommonComponents_RuntimeAll)
			{
				commonComponent.EnterComponent(RelatedAIBrainRuntimeInstance);
			}
			
		}
		
		
		


		public override void UnRegisterListenInActionBus()
		{
			base.UnRegisterListenInActionBus();
			if (IsGlobal)
			{
				GlobalActionBus.GetGlobalActionBus().RemoveAction(ActionType, _ABC_ProcessSideEffects_OnTriggered);
			}
			else
			{
				RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef.RemoveAction(ActionType,
					_ABC_ProcessSideEffects_OnTriggered);
			}
			SelfDCCConfigInfo.ClearOnUnload();
		}


	}
}