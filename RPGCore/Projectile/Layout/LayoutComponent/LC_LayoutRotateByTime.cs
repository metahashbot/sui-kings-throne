using System;
using UnityEngine;
namespace RPGCore.Projectile.Layout
{
	/// <summary>
	/// <para>旋转Layout，对于 [ SpawnAngleOffset ] 生效</para>
	/// </summary>
	[Serializable]
	public class LC_LayoutRotateByTime : BaseProjectileLayoutComponent
	{
		public bool NeedCurve;
		public float RotationAnglePerSecond;

		public AnimationCurve RotationCurve;

		// private 

		public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime, int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
		}





	}
}