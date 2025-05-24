using System;
using ARPG.Character.Base;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.PlayerAnimationMotion
{
	/// <summary>
	/// 大阶段。大阶段下还会有小阶段。具体由PAM来解释
	/// </summary>
	public enum PlayerAnimationMotionProgressTypeEnum
	{
		NoneSelf_不是自己 = 0,
		
		
		
		Prepare_前摇 = 10,
		PrepareCharging_前摇蓄力中 = 11,
		PrepareWaiting_前摇等待中 = 12,
		
		
		
		MiddleCasting_中段持续施法 = 20,
		MiddleCastingCharging_中段持续施法蓄力中 = 21,
		MiddleEffecting_中段作用中 = 22,
		
		
		Post_后摇 = 30,
		MultiPostOffhandWaiting_后摇脱手等待中 = 31,
		
		MultiPrepare_多段前摇 = 40,
		MultiMiddle_多段中段 = 50,
		MultiAfter_多段后摇 = 60,
		
		

	}

	public enum PlayerAnimationMotionTurningResult
	{
		
	}

	/// <summary>
	/// <para>玩家动画动作。</para>
	/// <para>玩家动画动作 将作为 武器与技能 的基类。</para>
	/// <para>包含【三段式动作组】的前摇  和  施法中或作用中  部分。</para>
	/// </summary>
	[Serializable]
	public abstract class BasePlayerAnimationMotion
	{
		[FoldoutGroup("运行时", true, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		[LabelText("！！！当前动画状态！！！")]
		[NonSerialized, ShowInInspector]
		public PlayerAnimationMotionProgressTypeEnum PlayerAnimationMotionProgressType =
			PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;

		
#region 前摇部分

		[FoldoutGroup("===前摇===")]
		[SerializeField, LabelText("动画_基本前摇"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ancn_PrepareAnimationName;

		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("  播放设置")]
		public AnimationPlayOptionsFlagTypeEnum _prepareAnimation_PlayOptionsFlagTypeEnum =
			AnimationPlayOptionsFlagTypeEnum.Default_缺省状态;

		[FoldoutGroup("===前摇===")]
		[SerializeField, LabelText("【蓄力的】？")]
		public bool _Prepare_IsCharging;

		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("    蓄力最短时间")][ShowIf(nameof(_Prepare_IsCharging))]
		public float _Prepare_MinChargeTime = 0.5f;
		
		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("    蓄力最长时间")][ShowIf(nameof(_Prepare_IsCharging))]
		public float _Prepare_MaxChargeTime = 1.5f;


		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("    蓄力-在基本动画后换新的动画？")]
		[ShowIf(nameof(_Prepare_IsCharging))]
		public bool _prepareAnimation_ChangeAnimationAfterBasePrepare = false;


		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("    动画_前摇蓄力动画"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf("@(this._Prepare_IsCharging && this._prepareAnimation_ChangeAnimationAfterBasePrepare)")]
		public string _prepareAnimation_ChangeAnimationAfterChargeAnimationName;
		

		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("可选的: 等候选点")]
		[HideIf(nameof(_Prepare_IsCharging))]
		public bool _Prepare_CanWaitForTargetPoint = false;
		
		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("    动画_前摇等候动画"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf(nameof(_Prepare_CanWaitForTargetPoint))]
		public string _prepareAnimation_WaitingForTargetPointAnimationName;


		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("    等候:包含最长等候限制？")]
		[ShowIf(nameof(_Prepare_CanWaitForTargetPoint))]
		public bool _prepareAnimation_WaitingForTargetPoint_IncludeMaxWaitingTime = true;
		
		
		[NonSerialized]
		[ShowInInspector,LabelText("    已等候时间"),ReadOnly]
		[ShowIf("@(this._Prepare_CanWaitForTargetPoint)")]
		public float _prepareAnimation_WaitingForTargetPoint_InWaitingTime = 0f;
		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("      最长等候时间")]
		[ShowIf("@(this._Prepare_CanWaitForTargetPoint && this._prepareAnimation_WaitingForTargetPoint_IncludeMaxWaitingTime)")]
		public float _prepareAnimation_WaitingForTargetPoint_MaxWaitingTime = 3f;
		
		
		
		
		
		[NonSerialized]
		[LabelText("前摇 - 蓄力已持续时间")]
		public float _prepare_InChargeTime = 0f;
		
		/// <summary>
		/// <para>用于那种要蓄力的但是并没有蓄力的，会在达到最短时间后释放</para>
		/// </summary>
		[NonSerialized]
		[LabelText("前摇 - 蓄力已标记为释放")]
		public bool _prepareAnimation_ChargeMarkedAsRelease = false;

		[FoldoutGroup("===前摇===")]
		[SerializeField, LabelText("前摇动画受【施法效率】加速吗？")]
		public bool _Prepare_AccelerateByCastingAccelerate = false;

		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("  施法效率 影响倍率,小数")]
		[ShowIf(nameof(_Prepare_AccelerateByCastingAccelerate))]
		public float _prepareAnimation_CastingAccelerateMultiplier = 0.5f;
		
		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("前摇动画受【攻击速度】加速吗？")]
		public bool _Prepare_AccelerateByAttackSpeed = false;

		[FoldoutGroup("===前摇===")]
		[SerializeField,LabelText("  攻击速度 影响倍率,小数")]
		[ShowIf(nameof(_Prepare_AccelerateByAttackSpeed))]
		public float _prepareAnimation_AttackSpeedAccelerateMultiplier = 0.5f;
		

		[SerializeField, LabelText("需要读条吗")]
		[FoldoutGroup("===前摇===")]
		public bool _prepareAnimation_NeedProgressHint = false;




		#endregion


#region 中段




		[SerializeField, LabelText("动画_中段基本"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[FoldoutGroup("===中段===")]
		public string _ancn_MiddlePartAnimationName;


		[FoldoutGroup("===中段===")]
		[SerializeField, LabelText("√:施法中 | 口:作用中")]
		public bool ContinuousCasting = true;


		[FoldoutGroup("===中段===")]
		[SerializeField, LabelText("中段动画受【施法效率】加速吗？")]
		public bool  _Middle_AccelerateByCastingAccelerate = false;

		[FoldoutGroup("===中段===")]
		[SerializeField, LabelText("  施法效率 影响倍率")]
		[ShowIf(nameof(_Middle_AccelerateByCastingAccelerate))]
		public float _middleAnimation_CastingAccelerateMultiplier = 0.5f;

		[FoldoutGroup("===中段===")]
		[SerializeField, LabelText("中段动画受【攻击速度】加速吗？")]
		public bool _Middle_AccelerateByAttackSpeed = false;

		[FoldoutGroup("===中段===")]
		[SerializeField, LabelText("  攻击速度 影响倍率")]
		[ShowIf(nameof(_Middle_AccelerateByAttackSpeed))]
		public float _middleAnimation_AttackSpeedAccelerateMultiplier = 0.5f;


		[FoldoutGroup("===中段===")]
		[SerializeField, LabelText("需要读条吗")]
		public bool _middleAnimation_NeedProgressHint = false;
		
		



#region 中段-施法中
		
		[TitleGroup("===中段===/施法中")]
		[SerializeField,LabelText("  可以取消吗")]
		[ShowIf(nameof(ContinuousCasting))]
		public bool _MiddleContinuous_CanCancel = true;

		[TitleGroup("===中段===/施法中")]
		[SerializeField,LabelText("  【蓄力的】？")]
		[ShowIf("@(this._Middle_IsCharging && this.ContinuousCasting)")]
		public bool _Middle_IsCharging = false;


		[TitleGroup("===中段===/施法中")]
		[SerializeField,LabelText("  最短蓄力时长")]
		[ShowIf("@(this._Middle_IsCharging && this.ContinuousCasting)")]
		public float _Middle_MinChargeTime = 0.5f;
		
		[TitleGroup("===中段===/施法中")]
		[SerializeField,LabelText("  最长蓄力时长")]
		[ShowIf("@(this._Middle_IsCharging && this.ContinuousCasting)")]
		public float _Middle_MaxChargeTime = 1.5f;

		[NonSerialized]
		[LabelText("中段 - 蓄力已持续时间")]
		public float _Middle_InChargeTime = 0f;

		/// <summary>
		/// <para>用于那种要蓄力的但是并没有蓄力的，会在达到最短时间后释放</para>
		/// </summary>
		[NonSerialized]
		[LabelText("中段 - 蓄力已标记为释放")]
		public bool _middleAnimation_ChargeMarkedAsRelease = false;
#endregion

#region 中段-作用中

		[TitleGroup("===中段===/作用中")]
		[SerializeField,LabelText("  可以主动取消吗")]
		[HideIf(nameof(ContinuousCasting))]
		public bool _MiddleEffecting_CanCancel = false;


		[NonSerialized]
		[LabelText("中段 - 作用已持续时间")]
		public float _Middle_EffectingTime = 0f;


		[TitleGroup("===中段===/作用中")]
		[SerializeField, LabelText("  作用最长持续时间")]
		[HideIf(nameof( ContinuousCasting))]
		public float _MiddleEffecting_MaxDuration = 3f;
		
		[TitleGroup("===中段===/作用中")]
		[SerializeField, LabelText("  作用最短持续时间")]
		[HideIf(nameof( ContinuousCasting))]
		public float _MiddleEffecting_MinDuration = 0.5f;
#endregion


#endregion


		/// <summary>
		/// <para>是否包含对应动画名的动画配置信息？这常常用来给技能和武器检查 触发动画响应的动画是否与自己有关</para>
		/// </summary>
		/// <returns></returns>
		public virtual bool ContainsAnimationConfig(string configName)
		{
			if (string.Equals(configName, _ancn_PrepareAnimationName))
			{
				return true;
			}
			if(string.Equals(configName, _ancn_MiddlePartAnimationName))
			{
				return true;
			}
			if (string.Equals(configName, _prepareAnimation_ChangeAnimationAfterChargeAnimationName))
			{
				return true;
			}
			if(string.Equals(configName, _prepareAnimation_WaitingForTargetPointAnimationName))
			{
				return true;
			}
			return false;
		}

	}
}