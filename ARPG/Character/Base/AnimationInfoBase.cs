using System;
using System.Collections.Generic;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Character.Base
{
	/// <summary>
	/// <para>播放动画时的选项们，作为params传入。根据不同的选项可以额外追加不同的播放业务</para>
	/// </summary>
	[Flags]
	public enum AnimationPlayOptionsFlagTypeEnum
	{
		Default_缺省状态,
		DirectionToForward_方向向前 = 1,
		DirectionToBack_方向向后 = 1 << 2,
		MainTransparentVersion_主体透明版本 = 1 << 3,
		PlayIdleReplace_播放长Idle动画 = 1 << 4,
		DisableExceptExplicitIndex_除了指定的索引外全部禁用 = 1 << 5,
		MovingForwardVariant_移动中朝正向的动画片段变体 = 1 << 6,
		StableVariant_停止中的动画片段变体 = 1 << 7,
		HoldAnimationProcess_维持动画进度 = 1 << 8,
		MovingBackVariant_移动中朝背向的动画片段变体 = 1 << 9, 
		ForceReplay_必定重播 = 1<<10,
	}

	public class RP_DS_AnimationPlayResult
	{
		public AnimationPlayOptionsFlagTypeEnum PlayOptionsFlag;
		public AnimationInfoBase RelatedAnimationInfoRef;
		public bool PlayBlockedByOccupation = false;


		public void Reset()
		{
			PlayOptionsFlag = AnimationPlayOptionsFlagTypeEnum.Default_缺省状态;
			RelatedAnimationInfoRef = null;
			PlayBlockedByOccupation = false;
		}
	}


	/// <summary>
	/// <para>当角色试图恢复动画的时候，可能并不会恢复成功。这个数据结构用来实现【阻塞恢复】，并且记录是谁阻塞了恢复</para>
	/// </summary>
	public class RP_DS_AnimationRestoreResult
	{
		public bool RestoreSuccess = true;
		public object BlockBy = null;
	}

	[Serializable]
	public abstract class AnimationInfoBase
	{

		[LabelText("已持续时间"), FoldoutGroup("运行时")]
		public float AccumulateDuration { get; protected set; }

		public enum AnimationDirectionTypeEnum
		{
			None_未指定 = 0,

		}



		[SerializeField, LabelText("配置名"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[PropertyOrder(-2)]
#if UNITY_EDITOR
		[OnValueChanged(nameof(_SyncName))]
#endif
		public string ConfigName = "待配置";

		[FormerlySerializedAs("_spineIndex")]
		[SerializeField, LabelText("将于第几个Helper播放")]
		public int _targetHelperIndex;




		[FormerlySerializedAs("_spineSpeed")]
		[SerializeField, LabelText("动画播放速度|gameplay修正会乘算这个值")]
		public float _animationPresetSpeed = 1f;

		/// <summary>
		/// <para>gameplay逻辑中用于计算动画播放速度的乘值</para>
		/// </summary>
		[NonSerialized]
		public float _aniSpeedMultiplier = 1f;

		public float AnimationPlaySpeed => _animationPresetSpeed * _aniSpeedMultiplier;



		[LabelText("包含方向变体?")]
		public bool ContainDirectionVariation = false;



		[SerializeField, LabelText("占用信息组内容")]
		public DS_OccupationInfo OccupationInfo = new DS_OccupationInfo();


		public void ResetAccumulateDuration()
		{
			AccumulateDuration = 0f;
		}


		public void FixedUpdateTick(float ct, int cf, float delta)
		{
			AccumulateDuration += delta;
		}


#if UNITY_EDITOR
		private void _SyncName()
		{
			OccupationInfo.OccupationInfoConfigName = ConfigName;
		}
#endif
	}
}