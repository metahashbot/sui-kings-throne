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
	public class AIListen_长时间贴墙_CloseToAirWallListen : BaseAIListenComponent, I_AIListenNeedTick
	{



		[SerializeField, LabelText("重置帧数阈值")]
		public int ReleaseFrameThreshold = 60;


		[SerializeField, LabelText("触发累积帧数阈值")]
		public int TriggerThreshold = 40;



		[SerializeField, LabelText("决策更换时重置")]
		public bool ResetWhenDecisionChange = true;

		[SerializeField, LabelText("需要匹配当前决策")]
		public bool NeedMatchCurrentDecision = false;

		[SerializeField, LabelText("仅在以下决策时累积"), ShowIf("NeedMatchCurrentDecision"),
		 GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		public List<string> MatchDecisionList = new List<string>();



		public int AccumulateFrameCount { get; private set; }

		// [SerializeReference, LabelText("直属=触发副作用")]
		// public List<BaseDecisionCommonComponent> Hit_CommonComponents_RuntimeAll;
		
		 [SerializeField,LabelText("【作用】配置")]
		public DCCConfigInfo Hit_DCCConfigInfo;
		
		


		/// <summary>
		/// 上次记录累积的帧数，超过阈值时将会重置
		/// </summary>
		private int _lastAccumulateFrameIndex;


		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);

			var lab = brainRef.BrainHandlerFunction.SelfLocalActionBusRef;

			lab.RegisterAction(ActionBus_ActionTypeEnum.L_AIDecision_DecisionComplete_决策自行结束,
				_ABC_ResetThreshold_OnDecisionChange);


			lab.RegisterAction(ActionBus_ActionTypeEnum.L_Utility_MovementCollideWithAirWall_此次移动碰撞到了空气墙,
				_ABC_AccumulateFrameCount_OnMovementCollideWithAirWall);
			Hit_DCCConfigInfo.ProcessRuntimeBuild();
		}





		private void _ABC_AccumulateFrameCount_OnMovementCollideWithAirWall(DS_ActionBusArguGroup ds)
		{
			if (RelatedAIBrainRuntimeInstance == null)
			{
				return;
			}
			if (NeedMatchCurrentDecision)
			{
				var currentD = RelatedAIBrainRuntimeInstance.BrainHandlerFunction.CurrentRunningDecision.ConfigContent
					.DecisionID;
				if (!MatchDecisionList.Contains(currentD))
				{
					return;
				}
			}
			AccumulateFrameCount++;
			_lastAccumulateFrameIndex = BaseGameReferenceService.CurrentFixedFrame;

			if (AccumulateFrameCount > TriggerThreshold)
			{
				AccumulateFrameCount = 0;
				foreach (var per in Hit_DCCConfigInfo.CommonComponents_RuntimeAll)
				{
					per.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
		}

		private void _ABC_ResetThreshold_OnDecisionChange(DS_ActionBusArguGroup ds)
		{
			if (ResetWhenDecisionChange)
			{
				AccumulateFrameCount = 0;
			}
		}

		public override void UnRegisterListenInActionBus()
		{
			base.UnRegisterListenInActionBus();
			var lab = RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef;
			lab.RemoveAction(ActionBus_ActionTypeEnum.L_AIDecision_DecisionComplete_决策自行结束,
				_ABC_ResetThreshold_OnDecisionChange);
			lab.RemoveAction(ActionBus_ActionTypeEnum.L_Utility_MovementCollideWithAirWall_此次移动碰撞到了空气墙,
				_ABC_AccumulateFrameCount_OnMovementCollideWithAirWall);
			Hit_DCCConfigInfo.ClearOnUnload();
			;
		}


		public void FixedUpdateTick(float ct, int cf, float delta)
		{
			if (cf > (_lastAccumulateFrameIndex + ReleaseFrameThreshold))
			{
				AccumulateFrameCount = 0;
			}
		}
	}
}