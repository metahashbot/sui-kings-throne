using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	[TypeInfoBox("最终调速时长是按照幂计算的，e.g.对1单位时长0.4，每单位削减40%,则3单位时长为 (0.4) * (1-0.4)^(3-1) = 0.144")]
	public class PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver : ProjectileBaseFunctionComponent
	{

		[SerializeField, LabelText("最大可用重量")]
		public float MaxAvailableMass = 5f;

		[SerializeField, LabelText("每额外单位重量造成的时长减少幂")]
		public float PerExtraMassPower = 0.4f;

		[SerializeField, LabelText("调速倍率")]
		public float SpeedMultiplier = 0.2f;

		[SerializeField, LabelText("调速时长-相对于重量1")]
		public float SpeedModifyDuration = 0.4f;
		
		
		public static PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver GetFromPool(PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver copy = null)
		{
			PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver tmpNew = GenericPool<PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver>.Get();
			if (copy != null)
			{
				tmpNew.MaxAvailableMass = copy.MaxAvailableMass;
				tmpNew.PerExtraMassPower = copy.PerExtraMassPower;
				tmpNew.SpeedMultiplier = copy.SpeedMultiplier;
				tmpNew.SpeedModifyDuration = copy.SpeedModifyDuration;
			}
			return tmpNew;
		}
		
		
		public override void ResetOnReturn()
		{
			GenericPool<PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver));
		}
	}
}