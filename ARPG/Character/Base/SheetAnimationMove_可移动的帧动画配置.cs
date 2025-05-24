using System;
using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Base
{
	[Serializable]
	public class SheetAnimationMove_可移动的帧动画配置 : SheetAnimationInfo_帧动画配置
	{

		// [PropertyOrder(-1)]
		// [SerializeField, LabelText("移动变体_向攻击方向移动时")]
		// public AnimationClip ClipRef_MovingTowardsAttack;


		[PropertyOrder(-1)]
		[SerializeField, LabelText("移动变体_向攻击方向移动时")] 
		public ClipTransition ClipTransitionRef_MovingTowardsAttack;

		// [PropertyOrder(-1)]
		// [SerializeField, LabelText("移动变体_背对攻击方向移动时")]
		// public AnimationClip ClipRef_MovingAwayFromAttack;

		[PropertyOrder(-1)]
		[SerializeField, LabelText("移动变体_背对攻击方向移动时")]
		public ClipTransition ClipTransitionRef_MovingAwayFromAttack;

		[PropertyOrder(-1)]
		[SerializeField, LabelText("触发移动预制_帧")]
		public float TriggerMovementThreshold = 0.01f;


		
		public static SheetAnimationMove_可移动的帧动画配置 GetDeepCopy(SheetAnimationMove_可移动的帧动画配置 copyFrom)
		{
			SheetAnimationMove_可移动的帧动画配置 newInfo = new SheetAnimationMove_可移动的帧动画配置();
			newInfo.ConfigName = copyFrom.ConfigName;
			newInfo._targetHelperIndex = copyFrom._targetHelperIndex;
			newInfo._animationPresetSpeed = copyFrom._animationPresetSpeed;
			newInfo._aniSpeedMultiplier = copyFrom._aniSpeedMultiplier;
			newInfo.OccupationInfo = new DS_OccupationInfo();
			newInfo.OccupationInfo.OccupationLevel = copyFrom.OccupationInfo.OccupationLevel;
			newInfo.OccupationInfo.OccupationInfoConfigName = copyFrom.OccupationInfo.OccupationInfoConfigName;
			newInfo.OccupationInfo.CancelLevel = copyFrom.OccupationInfo.CancelLevel;
			newInfo.OccupationInfo.ModifyOccupationLevelAfterAnimation = copyFrom.OccupationInfo.ModifyOccupationLevelAfterAnimation;
			newInfo.OccupationInfo.OccupationLevelAfterAnimation = copyFrom.OccupationInfo.OccupationLevelAfterAnimation;
			newInfo.ClipTransitionRef = copyFrom.ClipTransitionRef;
			newInfo.ClipTransitionRef_DirectionToForward = copyFrom.ClipTransitionRef_DirectionToForward;
			newInfo.ClipTransitionRef_DirectionToBack = copyFrom.ClipTransitionRef_DirectionToBack;
			newInfo.ClipTransitionRef_MovingTowardsAttack = copyFrom.ClipTransitionRef_MovingTowardsAttack;
			newInfo.ClipTransitionRef_MovingAwayFromAttack = copyFrom.ClipTransitionRef_MovingAwayFromAttack;
			newInfo.TriggerMovementThreshold = copyFrom.TriggerMovementThreshold;
			return newInfo;
		}

	}
}