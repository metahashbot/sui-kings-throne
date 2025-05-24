using System;
using Animancer;
using ARPG.Character.Base.CustomSpineData;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Base
{
	[Serializable]
	public class SheetAnimationInfo_帧动画配置 : AnimationInfoBase
	{
		// [SerializeField, LabelText("【序列帧】动画片")]
		// [PropertyOrder(-1)]
		// public AnimationClip ClipRef;

		[SerializeField, LabelText("Animancer动画配置")]
		[PropertyOrder(-1)]
		public ClipTransition ClipTransitionRef;
		
		// [SerializeField, LabelText("方向变体_Forward朝里"), ShowIf(nameof(ContainDirectionVariation))]
		// public AnimationClip ClipRef_DirectionToForward;
		//

		[SerializeField, LabelText("方向变体_Forward朝里"), ShowIf(nameof(ContainDirectionVariation))]
		[PropertyOrder(-1)]
		public ClipTransition ClipTransitionRef_DirectionToForward;

		// [SerializeField, LabelText("方向变体_Back朝外"), ShowIf(nameof(ContainDirectionVariation))]
		// public AnimationClip ClipRef_DirectionToBack;
		[SerializeField, LabelText("方向变体_Back朝外"), ShowIf(nameof(ContainDirectionVariation))]
		[PropertyOrder(-1)]
		public ClipTransition ClipTransitionRef_DirectionToBack;
		
		[NonSerialized]
		public BaseCharacterSheetAnimationHelper _targetHelperRef;


		public static SheetAnimationInfo_帧动画配置 GetDeepCopy(SheetAnimationInfo_帧动画配置 copyFrom)
		{
			SheetAnimationInfo_帧动画配置 newInfo = new SheetAnimationInfo_帧动画配置();
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
			return newInfo;
		}
		

		// public static SheetAnimationInfo_帧动画配置 GetDeepCopyFromSpine(SpineAnimationInfo_Spine动画配置 spine)
		// {
		// 	SheetAnimationInfo_帧动画配置 newInfo = new SheetAnimationInfo_帧动画配置();
		// 	
		// 	newInfo.ConfigName = spine.ConfigName;
		// 	newInfo._targetHelperIndex  = spine._targetHelperIndex;
		// 	newInfo._animationPresetSpeed = spine._animationPresetSpeed;
		// 	newInfo._aniSpeedMultiplier = spine._aniSpeedMultiplier;
		// 	newInfo.OccupationInfo = new DS_OccupationInfo();
		// 	newInfo.OccupationInfo.OccupationLevel = spine.OccupationInfo.OccupationLevel;
		// 	newInfo.OccupationInfo.OccupationInfoConfigName = spine.OccupationInfo.OccupationInfoConfigName;
		// 	newInfo.OccupationInfo.CancelLevel = spine.OccupationInfo.CancelLevel;
		// 	newInfo.OccupationInfo.ModifyOccupationLevelAfterAnimation = spine.OccupationInfo.ModifyOccupationLevelAfterAnimation;
		// 	newInfo.OccupationInfo.OccupationLevelAfterAnimation = spine.OccupationInfo.OccupationLevelAfterAnimation;
		// 	return newInfo;
		//
		// }



	}
}