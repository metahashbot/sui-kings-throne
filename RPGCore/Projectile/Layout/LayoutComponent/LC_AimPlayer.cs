using System;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
namespace RPGCore.Projectile
{
	[Serializable]
	public class LC_AimPlayer : BaseProjectileLayoutComponent
	{

		[LabelText("当[投射版面有方向覆写]时依然修改生成方向吗")]
		public bool StillModifySpawnDirectionWhenOverrideByLayout = false;

		[LabelText("√:在具体生成业务前设置 | 口:在具体生成业务后设置")]
		public bool SetBeforeSpawn = false;

		[LabelText("√: 使用当前位置 || 口: 使用延迟位置")]
		public bool AimInstant = true;

		[LabelText("     延迟帧数")] [HideIf(nameof(AimInstant))]
		public int AimDelayFrame = 0;

		[LabelText("√：每Shoots时校准  ||   口：在系列时瞄准")]
		public bool AlwaysAim_OnShoots = true;

		[LabelText("√:需要偏差狙  ||  口:准狙，不偏差")]
		public bool ContainBeginOffset;

		[LabelText("    √:按玩家位置角度进行偏差吗？  ||  口：按玩家位置弧长进行偏差(还没做)")]
		[ShowIf(nameof(ContainBeginOffset))]
		public bool OffsetAsAngle = true;


		[LabelText("    偏差的角度或者弧长")]
		[ShowIf(nameof(ContainBeginOffset))]
		public float OffsetValue;

		// [TabGroup("Debug"), ShowInInspector, ReadOnly]
		// public Vector3 CachedPlayerPosition;

		// [TabGroup("Debug"), ShowInInspector, ReadOnly]
		// public Vector3 CachedPlayerDirection;
		[ShowInInspector, ReadOnly, LabelText("玩家位置和方向缓存")]
		private (Vector3, Vector3) _playerInfoCache;


		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
		}

		public override void InitializeBeforeStart(
			SOConfig_ProjectileLayout config,
			float currentTime,
			int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
			if (SetBeforeSpawn)
			{
				GetSelfLocalActionBusRef()
					.RegisterAction(
						ActionBus_ActionTypeEnum.L_PLayout_Spawn_OneSpawnOperationSetFromDirection_一次生成设置了起始朝向,
						_ABC_ProcessSpawnFromDirection_OnOneSpawnOperationSetFromRotation);
			}
			else
			{
				GetSelfLocalActionBusRef()
					.RegisterAction(ActionBus_ActionTypeEnum.L_PLC_Spawn_OneProjectileSetPreDirection_一个投射物生成设置了预朝向,
						_ABC_ProcessSpawnFromDirection_OnOneProjectileSetPreDirection);
			}
			if (!AimInstant)
			{
				AimDelayFrame = 0;
			}
		}

		private void _ABC_ProcessSpawnFromDirection_OnOneProjectileSetPreDirection(DS_ActionBusArguGroup ds)
		{
			var projectileRef = ds.GetObj1AsT<ProjectileBehaviour_Runtime>();
			var lc = ds.ObjectArgu2 as BaseProjectileLayoutComponent;
			if (!CheckIfSameLayoutConfigParent(lc))
			{
				return;
			}

			//如果不是每Shoots校准，也return无事发生
			if (AlwaysAim_OnShoots)
			{
				RefreshPlayerInfo( projectileRef.StartPosition);
			}
			
			projectileRef.StartDirection = GetFinalDirection(projectileRef.StartPosition);
		}
		

		private void _ABC_ProcessSpawnFromDirection_OnOneSpawnOperationSetFromRotation(DS_ActionBusArguGroup ds)
		{
			if (!CheckIfSameLayoutConfigParent(ds.ObjectArgu1 as BaseProjectileLayoutComponent))
			{
				return;
			}

			BaseLayoutHandler handlerRef = ds.ObjectArguStr as BaseLayoutHandler;
			LayoutSpawnInfo_OneSpawn oneSpawnInfo = ds.ObjectArgu2 as LayoutSpawnInfo_OneSpawn;
			Vector3 fromPos = _parentProjectileLayoutConfig.LayoutHandlerFunction._cache_CasterPosition;

			//如果该Handler已经存在了覆写方向，但是这个LC组件没有要求在已经覆写时继续修改，那就直接返回，无事发生
			if (handlerRef.OverrideSpawnFromDirection != null && !StillModifySpawnDirectionWhenOverrideByLayout)
			{
				return;
			}
			//如果不是每Shoots校准，也return无事发生
			if (AlwaysAim_OnShoots)
			{
				RefreshPlayerInfo(fromPos);
			}


			LayoutSpawnInfo_OneSpawn thisSpawn = ds.ObjectArgu2 as LayoutSpawnInfo_OneSpawn;

			
			oneSpawnInfo.SpawnFromDirection = GetFinalDirection( thisSpawn.SpawnFromPosition);
			
		}


		private Vector3 GetFinalDirection(Vector3 spawnFromPosition)
		{


			float SpawnFromDirectionOffsetAngle = (Mathf.Atan2(_playerInfoCache.Item1.z - spawnFromPosition.z,
				_playerInfoCache.Item1.x - spawnFromPosition.x) * Mathf.Rad2Deg);

			if (ContainBeginOffset)
			{
				if (OffsetAsAngle)
				{
					SpawnFromDirectionOffsetAngle += Random.Range(-OffsetValue, OffsetValue);
				}
				//按距离偏移，那就是要在目的地位置额外旋转
				else
				{
				}

				var finalPos = spawnFromPosition;
				float length = Vector3.Distance(_playerInfoCache.Item1, spawnFromPosition);
				Vector3 rotateUnit = Vector3.right * length;
				Vector3 rotateResult = MathExtend.Vector3RotateOnXOZ(rotateUnit, SpawnFromDirectionOffsetAngle);
				rotateResult += spawnFromPosition;
				return  (rotateResult - spawnFromPosition).normalized;
			}
			else
			{
				return (_playerInfoCache.Item1 - spawnFromPosition).normalized;
			}
			
			

		}

		private void RefreshPlayerInfo(Vector3 fromPos)
		{
			_playerInfoCache =
				ProjectileBehaviourManagerReference.GetAimPlayerPositionAndDirectionOfPBM(fromPos, AimDelayFrame);
		}


		// public (Vector3, Vector3) GetAimPlayerPositionAndDirectionOfPBM(I_RP_Projectile_ObjectCanReleaseProjectile caster)
		// {
		// 	var t = ProjectileBehaviourManagerReference.GetAimPlayerPositionAndDirectionOfPBM(caster);
		//
		// 	if (AlwaysAim)
		// 	{
		// 		return t;
		// 	}
		// 	else
		// 	{
		// 		if (_isFirstAim)
		// 		{
		// 			CachedPlayerPosition = t.Item1;
		// 			CachedPlayerDirection = t.Item2;
		// 			_isFirstAim = false;
		// 			return t;
		// 		}
		// 		else
		// 		{
		// 			return (CachedPlayerPosition, CachedPlayerDirection);
		// 		}
		// 	}
		// }
	}
}