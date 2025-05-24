using System;
using System.Collections.Generic;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	public class PFC_生命周期结束后必定结束粒子_MustEndParticleOnLifetimeEnd : ProjectileBaseFunctionComponent
	{


		public static PFC_生命周期结束后必定结束粒子_MustEndParticleOnLifetimeEnd GetFromPool(
			PFC_生命周期结束后必定结束粒子_MustEndParticleOnLifetimeEnd copy = null)
		{
			PFC_生命周期结束后必定结束粒子_MustEndParticleOnLifetimeEnd tmpNew =
				GenericPool<PFC_生命周期结束后必定结束粒子_MustEndParticleOnLifetimeEnd>.Get();
			if (copy != null)
			{
			}
			return tmpNew;
		}


		public override void ResetOnReturn()
		{
			GenericPool<PFC_生命周期结束后必定结束粒子_MustEndParticleOnLifetimeEnd>.Release(this);
		}
		public override void DeepCopyToRuntimeList(
			ProjectileBaseFunctionComponent copyFrom,
			List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_生命周期结束后必定结束粒子_MustEndParticleOnLifetimeEnd));
		}
	}
}