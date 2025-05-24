using System;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[TypeInfoBox("会试图转身。本质上是先转面，然后播动画。")]
	[Serializable]
	public class DH_试图转身_TryTurnFace : BaseDecisionHandler
	{
		
		[InfoBox("这个动画应当是从反方向转到正方向。比如这个角色默认朝右，那这个动画的内容应当是从左边转到右边")]
		[SerializeField,LabelText("转身用的动画"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[FoldoutGroup("配置", true)]
		protected string _AN_TurnFaceSpineAnimation;
		
		[SerializeField,LabelText("进入时刷新仇恨")] [FoldoutGroup("配置", true)]
		protected bool _refreshHatredOnEnter = true;




		[NonSerialized]
		private bool _noNeedForTurnAndEnd;



		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds = base.OnDecisionBeforeStartExecution(withSideEffect);

			if (_refreshHatredOnEnter)
			{
				SelfRelatedBrainInstanceRef.BrainHandlerFunction.RefreshHatredTarget();
			}

			var currentSelfFace = SelfRelatedBrainInstanceRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour
				.GetRelatedArtHelper().CurrentFaceLeft;
			var selfPos = SelfRelatedBrainInstanceRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform
				.position;
			selfPos.y = 0f;
			var hatredPos = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget().transform
				.position;
			hatredPos.y = 0f;
			  
			var selfToHatred = hatredPos - selfPos;
			bool currentHatredOnLeft = selfToHatred.x < 0;
			//当前朝左，
			if (currentSelfFace)
			{
				//仇恨在做，无事发生，下帧结束
				if (currentHatredOnLeft)
				{
					_noNeedForTurnAndEnd = true;
					return ds;
				}
				else
				{
					_noNeedForTurnAndEnd = false;
				}
			}
			else
			{
				if (!currentHatredOnLeft)
				{
					_noNeedForTurnAndEnd = true;
					return ds;
				}
				else
				{
					_noNeedForTurnAndEnd = false;
				}
			}

			//仍然需要转身，那就播转身动画
			if (!_noNeedForTurnAndEnd)
			{
				 SelfRelatedBrainInstanceRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetRelatedArtHelper().SetFaceLeft(
					 currentHatredOnLeft);
				_Internal_RequireAnimation(GetAnimationInfoFromBrain(_AN_TurnFaceSpineAnimation));
			}
			
			
			return ds;
			
		}


		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			if (_noNeedForTurnAndEnd)
			{
				OnDecisionNormalComplete();
				return;
			}
		}

		protected override void _ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(DS_ActionBusArguGroup ds)
		{
			base._ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(ds);
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(ds.ObjectArguStr as SOConfig_AIDecision))
			{
				return;
			}
			OnDecisionNormalComplete();
			 
		}



	}
}