using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
namespace RPGCore.Projectile.Layout
{
	// [TypeInfoBox("线状的预警")]
	// [Serializable]
	// public class LC_WarningLine : BaseProjectileLayoutComponent
	// {
	// 	[InfoBox("预警线的中心点是实际中心点，Height的一半是长度，方向是forward控制")]
	// 	[SerializeField, Required, LabelText("使用的预警线Decal"), TabGroup("Config"), AssetsOnly]
	// 	private WarnLineDecalHelper _prefab_WarnLineDecalPrefab;
	//
	// 	[SerializeField, LabelText("预警线颜色"), TabGroup("Config")]
	// 	public Color WarnLineColor;
	//
	// 	[SerializeField, LabelText("√：预警线将于目标处停止；口：一直延长")]
	// 	public bool AimEndAtTarget = true;
	//
	// 	[SerializeField,HideIf(nameof(AimEndAtTarget)),LabelText("预警线长度")]
	// 	public float WarnLineLength;
	//
	//
	// 	[SerializeField,LabelText("预警持续时间")]
	// 	public float WarnDuration;
	//
	//
	// 	private WarnLineDecalHelper _runtimeDecalHelper;
	//
	// 	private float _warnDisableTime;
	// 	private bool _warnCurrentActive;
	//
	// 	public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime, int currentFrame)
	// 	{
	// 		base.InitializeBeforeStart(config,currentTime, currentFrame);
	// 		_runtimeDecalHelper = Object.Instantiate(_prefab_WarnLineDecalPrefab);
	// 		_runtimeDecalHelper.transform.localPosition = new Vector3(0f, _prefab_WarnLineDecalPrefab.BaseYOffset, 0f);
	// 		// Vector3 casterPos = _parentLayoutConfigRef.CasterRef.GetCasterPosition();
	// 		// casterPos.y += _prefab_WarnLineDecalPrefab.BaseYOffset;
	// 		// _runtimeDecalHelper.transform.position = casterPos;
	// 		_runtimeDecalHelper.OriginalDecalProjector.material.SetColor("_MainColor", WarnLineColor);
	// 		_runtimeDecalHelper.gameObject.SetActive(true);
	// 		_warnDisableTime = currentTime + WarnDuration;
	// 		_warnCurrentActive = true;
	// 		//如果直到目标就结束，那找到AimPlayer的Component
	// 		if (AimEndAtTarget)
	// 		{
	// 			// var p = ProjectileBehaviourManagerReference.GetAimPlayerPositionAndDirectionOfPBM(_parentLayoutConfigRef.CasterRef);
	// 			// _runtimeDecalHelper.SetEndPosition(p.Item1);
	// 			// Vector3 casterCalPos = _parentLayoutConfigRef.CasterRef.GetCasterY0Position();
	// 			// Vector3 playerPos = p.Item1;
	// 			// playerPos.y = 0f;
	// 			// _parentLayoutConfigRef.OverrideSpawnFromDirection = (playerPos - casterCalPos).normalized;
	// 		}
	// 		else
	// 		{
	// 		}
	// 	}
	//
	// 	public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
	// 	{
	// 		base.FixedUpdateTick(currentTime, currentFrame, delta);
	// 		if (_warnCurrentActive)
	// 		{
	// 			if (currentTime > _warnDisableTime)
	// 			{
	// 				_runtimeDecalHelper.gameObject.SetActive(false);
	// 			}
	// 		}
	// 	}
	//
	// 	public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
	// 	{
	// 		base.ClearAndUnload(parentLayoutRef);
	// 		Object.Destroy(_runtimeDecalHelper.gameObject);
	// 	}
	//
	// }
}