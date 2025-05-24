using System;
using ARPG.Manager;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Projectile.Layout.LayoutComponent
{
	[Serializable]
	public class LC_于Series增加时播放特效_PlayVFXOnSeriesStart : BaseProjectileLayoutComponent	
	{
		[SerializeField, LabelText("关联特效VFX Prefab")]
		public GameObject RelatedVFXPrefab;

		[SerializeField, LabelText("√:覆写尺寸  | 口:乘算尺寸")]
		public bool OverrideSizeEnabled = false;

		[SerializeField, LabelText("尺寸")]
		public float SizeMul = 1f;

		[NonSerialized]
		private Nullable<bool> _vfxIsPSPP;

		public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime, int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
			config.LayoutHandlerFunction.SelfActionBusReference.RegisterAction(
				ActionBus_ActionTypeEnum.L_PLC_Spawn_LayoutSeriesIncrease_LayoutSeries增加,
				_ABC_PlayVFX_OnSeriesStart);
		}

		private void _ABC_PlayVFX_OnSeriesStart(DS_ActionBusArguGroup ds)
		{
			if (!ReferenceEquals(ds.GetObj1AsT<BaseLayoutHandler>(),
				    _parentProjectileLayoutConfig.LayoutHandlerFunction))
			{
				return;
			}
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


		public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
		{
			base.ClearAndUnload(parentLayoutRef);
			parentLayoutRef.LayoutHandlerFunction.SelfActionBusReference.RegisterAction(
				ActionBus_ActionTypeEnum.L_PLC_Spawn_LayoutSeriesIncrease_LayoutSeries增加,
				_ABC_PlayVFX_OnSeriesStart);
		}

	}
}