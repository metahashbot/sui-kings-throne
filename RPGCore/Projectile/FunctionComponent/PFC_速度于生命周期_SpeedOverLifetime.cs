using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	public class PFC_速度于生命周期_SpeedOverLifetime : ProjectileBaseFunctionComponent
	{

		[InfoBox("对于LC_Drop(掉落)来说，如果那边不是显式的生命周期，那其实它的生命周期会很大，这时候就不应该用生命周期完成量了\n" +
		         "对于 LC_NWay和LC_JustSpawn来说，就是正常的生命周期。\b" +
		         "如果是【按照生命周期数值】，那下面的\"生命周期范围_作为曲线X轴\"就是曲线的X范围。处理这个的时候不用管它原来的生命周期是多少")]
		[SerializeField, LabelText("√：按照生命周期完成量  || 口：按照生命周期数值")]
		public bool CalculateAsLifetimePartial;

		[SerializeField, LabelText("生命周期范围_作为曲线X轴"),HideIf(nameof(CalculateAsLifetimePartial))]
		public float LifetimeDuration = 3f;


		[SerializeField, LabelText("√：覆盖原始速度 || 口：为原始速度乘算")]
		public bool OverrideOriginalSpeed;

		[SerializeField, LabelText("覆盖速度数值_作为曲线的Y轴"),ShowIf(nameof(OverrideOriginalSpeed))]
		public float SpeedOverrideValue = 10f;

		[SerializeField, LabelText("速度乘数数值_作为曲线的Y轴"), HideIf(nameof(OverrideOriginalSpeed))]
		public float SpeedMultiplyValue = 1.5f;
		
		[SerializeField,LabelText("相对值曲线")]
		public AnimationCurve SelfCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);




		
		public static PFC_速度于生命周期_SpeedOverLifetime GetFromPool(PFC_速度于生命周期_SpeedOverLifetime copy = null)
		{
			var tmpNew = GenericPool<PFC_速度于生命周期_SpeedOverLifetime>.Get();
			if (copy != null)
			{
				
				tmpNew.CalculateAsLifetimePartial = copy.CalculateAsLifetimePartial;
				tmpNew.LifetimeDuration = copy.LifetimeDuration;
				tmpNew.OverrideOriginalSpeed = copy.OverrideOriginalSpeed;
				tmpNew.SpeedOverrideValue = copy.SpeedOverrideValue;
				tmpNew.SpeedMultiplyValue = copy.SpeedMultiplyValue;
				tmpNew.SelfCurve = copy.SelfCurve;

			}
			return tmpNew;
		}


		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			if (CalculateAsLifetimePartial)
			{
				var currentLifetimePartial = _selfRelatedProjectileBehaviourRef.ActiveElapsedTime /
				                             _selfRelatedProjectileBehaviourRef.StartLifetime;
				float evaFromCurve = SelfCurve.Evaluate(currentLifetimePartial);
				if (OverrideOriginalSpeed)
				{
					_selfRelatedProjectileBehaviourRef.DesiredMoveSpeed = SpeedOverrideValue * evaFromCurve;
				}
				else
				{
					_selfRelatedProjectileBehaviourRef.DesiredMoveSpeed = _selfRelatedProjectileBehaviourRef.StartSpeed *
					                                                      (SpeedMultiplyValue * evaFromCurve);
				}
			}
			else
			{
				float evaFromCurve = SelfCurve.Evaluate(_selfRelatedProjectileBehaviourRef.ActiveElapsedTime / LifetimeDuration);
				if (OverrideOriginalSpeed)
				{
					_selfRelatedProjectileBehaviourRef.DesiredMoveSpeed = SpeedOverrideValue * evaFromCurve;
				}
				else
				{
					_selfRelatedProjectileBehaviourRef.DesiredMoveSpeed = _selfRelatedProjectileBehaviourRef.StartSpeed *
					                                                      (SpeedMultiplyValue * evaFromCurve);
				}
			}
			
			
		}


		public override void ResetOnReturn()
		{
			GenericPool<PFC_速度于生命周期_SpeedOverLifetime>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_速度于生命周期_SpeedOverLifetime));
		}
	}
}