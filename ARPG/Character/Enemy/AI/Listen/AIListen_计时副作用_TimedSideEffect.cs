using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	public class AIListen_计时副作用_TimedSideEffect : BaseAIListenComponent, I_AIListenNeedTick
	{
		[SerializeField, LabelText("固定时长吗")]
		public bool FixedDelay = true;
		
		
		[SerializeField,LabelText("固定的时长")]
		[ShowIf(nameof(FixedDelay))]
		public float FixedDelayTime = 10f;
		
		
		[SerializeField,LabelText("随机时长范围")]
		[HideIf(nameof(FixedDelay))]
		public Vector2 RandomDelayRange = new Vector2(5f, 10f);

		// [SerializeReference, LabelText("将要进行的副作用们=直属")]
		// public List<BaseDecisionCommonComponent> SideEffects_Direct = new List<BaseDecisionCommonComponent>();
		//
		// [SerializeField,LabelText("将要进行的副作用们=文件")]
		// public List<SOConfig_PresetSideEffects> SideEffects_File = new List<SOConfig_PresetSideEffects>();


		[SerializeField]
		public DCCConfigInfo SelfDCCConfigInfo = new DCCConfigInfo();

		private float _willTriggerTime;
		private bool _triggered = false;





		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);
			_triggered = false;
			if (FixedDelay)
			{
				_willTriggerTime = BaseGameReferenceService.CurrentFixedTime + FixedDelayTime;
			}
			else
			{
				_willTriggerTime = BaseGameReferenceService.CurrentFixedTime + UnityEngine.Random.Range(RandomDelayRange.x, RandomDelayRange.y);
			}
			SelfDCCConfigInfo.CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			SelfDCCConfigInfo.BuildRuntimeDCC(ref SelfDCCConfigInfo.CommonComponents_RuntimeAll);

		}

		public void FixedUpdateTick(float ct, int cf, float delta)
		{
			if (_triggered)
			{
				return;
			}
			if (ct > _willTriggerTime)
			{
				_triggered = true;
				foreach (BaseDecisionCommonComponent perDcc in SelfDCCConfigInfo.CommonComponents_RuntimeAll)
				{
					perDcc.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
			
		}

		public override void UnRegisterListenInActionBus()
		{
			base.UnRegisterListenInActionBus();
			SelfDCCConfigInfo.ClearOnUnload();
		}
	}
}