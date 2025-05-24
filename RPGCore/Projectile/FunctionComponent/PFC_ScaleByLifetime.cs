using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[TypeInfoBox("")]
	[Serializable]
	public class PFC_ScaleByLifetime : ProjectileBaseFunctionComponent
	{

		[LabelText("√ 绝对尺寸 ； 口 原始尺寸乘数")]
		public bool SetAsAbsolute = false;

		[LabelText("尺寸变化时间|曲线的X轴值")]
		public float ChangeDuration = 3f;

		[LabelText("尺寸变化的值 | 曲线的Y轴值")]
		public float SizeValue = 3f;

		[LabelText("曲线本体")]
		public AnimationCurve SelfCurve = AnimationCurve.Linear(0, 0, 1, 1);
		

		private float _projectileSizeFrom;


		private float _startTime; 
		

		public static PFC_ScaleByLifetime GetFromPool(PFC_ScaleByLifetime copy = null)
		{
			var tmpNew = GenericPool<PFC_ScaleByLifetime>.Get();
			if (copy != null)
			{
				tmpNew.SetAsAbsolute = copy.SetAsAbsolute;
				tmpNew.ChangeDuration = copy.ChangeDuration;
				tmpNew.SizeValue = copy.SizeValue;
				tmpNew.SelfCurve = copy.SelfCurve;

			}
			return tmpNew;
		}

		public override void OnFunctionStart(float currentTime)
		{
			_startTime = currentTime;
			_projectileSizeFrom = _selfRelatedProjectileBehaviourRef.StartLocalSize;
		}
		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			float eva = (currentTime - _startTime) / ChangeDuration;

			if (SetAsAbsolute)
			{
				_selfRelatedProjectileBehaviourRef.SetProjectileFullScale( SelfCurve.Evaluate(eva) * SizeValue);
				// _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.localScale =
				// 	Vector3.one * (SelfCurve.Evaluate(eva) * SizeValue);
			}
			else
			{
				 _selfRelatedProjectileBehaviourRef.SetProjectileFullScale( SelfCurve.Evaluate(eva) * SizeValue * _projectileSizeFrom);
				// _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.localScale =
				// 	Vector3.one * (SelfCurve.Evaluate(eva) * SizeValue * _projectileSizeFrom);
			}
		}

		public override void ResetOnReturn()
		{
			GenericPool<PFC_ScaleByLifetime>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_ScaleByLifetime));
		}
	}
}