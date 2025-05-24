using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
namespace RPGCore.Projectile.Layout
{
	// [TypeInfoBox("圆形的预警")]
	// [Serializable]
	// public class LC_WarningArea : BaseProjectileLayoutComponent
	// {
	// 	[SerializeField, LabelText("使用的预警圈Prefab"), TabGroup("Config")]
	// 	private WarnCircleDecalHelper _prefab_WarnCircleDecalPrefab;
	//
	// 	[SerializeField, LabelText("预警圈基准底色"), TabGroup("Config")]
	// 	public Color WarnCircleBaseColor;
	// 	[SerializeField, LabelText("预警圈覆盖颜色"), TabGroup("Config")]
	// 	public Color WarnCircleOverlayColor;
	// 	[SerializeField, LabelText("预警圈扰动颜色滚动速度"), TabGroup("Config")]
	// 	public float WarnOverlayOffsetSpeed = 0.1f;
	//
	// 	[SerializeField,LabelText("预警圈半径")]
	// 	public float WarnCircleRadius;
	//
	// 	[SerializeField,LabelText("预警圈持续时间")]
	// 	public float WarnDuration;
	//
	// 	private float _warnDisableTime;
	// 	private bool _warnCurrentActive;
	//
	// 	private WarnCircleDecalHelper _runtimeDecalHelper;
	//
	// 	public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime, int currentFrame)
	// 	{
	// 		base.InitializeBeforeStart(config,currentTime, currentFrame);
	// 		_runtimeDecalHelper = Object.Instantiate(_prefab_WarnCircleDecalPrefab);
	// 		// Vector3 casterPos = _parentLayoutConfigRef.CasterRef.GetCasterPosition();
	// 		// casterPos.y += _prefab_WarnCircleDecalPrefab.BaseYOffset;
	// 		// _runtimeDecalHelper.OriginalDecalProjector.material.SetColor("_MainColor", WarnCircleBaseColor);
	// 		// _runtimeDecalHelper.OverlayDecalProjector.material.SetColor("_MainColor", WarnCircleBaseColor);
	// 		// _runtimeDecalHelper.OverlayDecalProjector.material.SetColor("_Secondary_Color", WarnCircleOverlayColor);
	// 		//
	// 		// _runtimeDecalHelper.gameObject.SetActive(true);
	// 		// _warnDisableTime = currentTime + WarnDuration;
	// 		// _warnCurrentActive = true;
	// 		//
	// 		// _runtimeDecalHelper.SetPositionAndScale(casterPos, WarnCircleRadius);
	// 		
	//
	//
	// 	}
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