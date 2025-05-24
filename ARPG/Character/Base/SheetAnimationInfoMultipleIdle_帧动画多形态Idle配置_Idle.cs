using System;
using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Base
{
	[Serializable]
	public class SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle : SheetAnimationInfo_帧动画配置
	{
		//
		// [SerializeField, LabelText("长Idle动画片")]
		// [PropertyOrder(-1)]
		// public AnimationClip Clip_TransitionClip;

		[SerializeField, LabelText("长Idle动画片")]
		[PropertyOrder(-1)]
		public ClipTransition ClipTransitionRef_TransitionClip;

		[SerializeField, LabelText("多长时间开始转换动画片")]
		[PropertyOrder(-1)]
		public float ConvertIdleAnimationDelay = 3f;

		
		


		public static SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle GetDeepCopyFromSpine(
			SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle spine)
		{
			SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle newInfo =
				new SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle();

			newInfo.ConfigName = spine.ConfigName;
			newInfo._targetHelperIndex = spine._targetHelperIndex;
			newInfo._animationPresetSpeed = spine._animationPresetSpeed;
			newInfo._aniSpeedMultiplier = spine._aniSpeedMultiplier;
			newInfo.OccupationInfo = new DS_OccupationInfo();
			newInfo.OccupationInfo.OccupationLevel = spine.OccupationInfo.OccupationLevel;
			newInfo.OccupationInfo.OccupationInfoConfigName = spine.OccupationInfo.OccupationInfoConfigName;
			newInfo.OccupationInfo.CancelLevel = spine.OccupationInfo.CancelLevel;
			newInfo.OccupationInfo.ModifyOccupationLevelAfterAnimation =
				spine.OccupationInfo.ModifyOccupationLevelAfterAnimation;
			newInfo.OccupationInfo.OccupationLevelAfterAnimation = spine.OccupationInfo.OccupationLevelAfterAnimation;
			return newInfo;
		}



		public static SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle GetDeepCopy(
			SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle copyFrom)
		{
			SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle newInfo =
				new SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle();
			newInfo.ConfigName = copyFrom.ConfigName;
			newInfo._targetHelperIndex = copyFrom._targetHelperIndex;
			newInfo._animationPresetSpeed = copyFrom._animationPresetSpeed;
			newInfo._aniSpeedMultiplier = copyFrom._aniSpeedMultiplier;
			newInfo.OccupationInfo = new DS_OccupationInfo();
			newInfo.OccupationInfo.OccupationLevel = copyFrom.OccupationInfo.OccupationLevel;
			newInfo.OccupationInfo.OccupationInfoConfigName = copyFrom.OccupationInfo.OccupationInfoConfigName;
			newInfo.OccupationInfo.CancelLevel = copyFrom.OccupationInfo.CancelLevel;
			newInfo.OccupationInfo.ModifyOccupationLevelAfterAnimation =
				copyFrom.OccupationInfo.ModifyOccupationLevelAfterAnimation;
			newInfo.OccupationInfo.OccupationLevelAfterAnimation = copyFrom.OccupationInfo.OccupationLevelAfterAnimation;
			newInfo.ClipTransitionRef = copyFrom.ClipTransitionRef;
			newInfo.ClipTransitionRef_DirectionToForward = copyFrom.ClipTransitionRef_DirectionToForward;
			newInfo.ClipTransitionRef_DirectionToBack = copyFrom.ClipTransitionRef_DirectionToBack;
			newInfo.ClipTransitionRef_TransitionClip = copyFrom.ClipTransitionRef_TransitionClip;
			newInfo.ConvertIdleAnimationDelay = copyFrom.ConvertIdleAnimationDelay;
			return newInfo;
		}



	}
}