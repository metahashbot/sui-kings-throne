using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	public class PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster : ProjectileBaseFunctionComponent
	{
		[SerializeField, LabelText("调速倍率")]
		public float SpeedMultiplier = 0.2f;
		
		[SerializeField,LabelText("调速时长-相对于重量1")]
		public float SpeedModifyDuration = 0.4f;
		
		public static PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster GetFromPool(PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster copy = null)
		{
			PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster tmpNew = GenericPool<PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster>.Get();
			if (copy != null)
			{
				tmpNew.SpeedMultiplier = copy.SpeedMultiplier;
				tmpNew.SpeedModifyDuration = copy.SpeedModifyDuration;
			}
			return tmpNew;
		}

		public override void ResetOnReturn()
		{
			GenericPool<PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{ 
			targetList.Add(GetFromPool(copyFrom as PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster));
		}
	}
}