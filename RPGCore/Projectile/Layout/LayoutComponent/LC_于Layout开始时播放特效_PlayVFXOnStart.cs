using System;
using ARPG.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Projectile.Layout
{
	public class LC_于Layout开始时播放特效_PlayVFXOnStart : BaseProjectileLayoutComponent
	{
		[SerializeField, LabelText("关联特效VFX Prefab")]
		public GameObject RelatedVFXPrefab;

		[SerializeField,LabelText("覆写尺寸")]
		public bool OverrideSizeEnabled = false;

		[SerializeField, LabelText("尺寸")]
		public float SizeMul = 1f;

		[NonSerialized]
		private Nullable<bool> _vfxIsPSPP;
		
		public override void StartLayoutComponent(float currentTime, int currentFrame)
		{
			base.StartLayoutComponent(currentTime, currentFrame);
			if (RelatedVFXPrefab)
			{
				if (_vfxIsPSPP == null)
				{
					if (RelatedVFXPrefab.GetComponent<VFX_ParticleSystemPlayProxy>() != null)
					{
						_vfxIsPSPP = true;
					}
					else
					{
						_vfxIsPSPP = false;
					}
				}
				if (_vfxIsPSPP.Value)
				{
					var pspp = VFXPoolManager.Instance.GetPSPPRuntimeByPrefab(RelatedVFXPrefab);
					pspp.transform.position =
						_parentProjectileLayoutConfig.LayoutHandlerFunction.OverrideSpawnFromPosition ??
						_parentProjectileLayoutConfig.LayoutHandlerFunction.GetCasterPosition();

					pspp.transform.localScale = Vector3.one * (OverrideSizeEnabled ? SizeMul
						: _parentProjectileLayoutConfig.LayoutContentInSO.RelatedProjectileScale * SizeMul);

					pspp.Play();
				}
				else
				{
					var ps = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(RelatedVFXPrefab);
					ps.transform.position =
						_parentProjectileLayoutConfig.LayoutHandlerFunction.OverrideSpawnFromPosition ??
						_parentProjectileLayoutConfig.LayoutHandlerFunction.GetCasterPosition();
					ps.transform.localScale = Vector3.one * (OverrideSizeEnabled ? SizeMul
						: _parentProjectileLayoutConfig.LayoutContentInSO.RelatedProjectileScale * SizeMul);
					ps.Play();
				}
			}
		}

	}
}