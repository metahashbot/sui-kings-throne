using System;
using System.Collections.Generic;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Projectile.Layout.LayoutComponent
{
	[Serializable]
	public class LC_JustSpawn : BaseProjectileLayoutComponent, ILayoutComponent_RespondToSpawn,
		ILayoutComponent_CanAddProjectileToSpawnCollection
	{

		[LabelText("总共生成几个"),FoldoutGroup("配置",true)]
		public int TotalSpawnCount = 1;



		[LabelText("飞行速度"), FoldoutGroup("配置", true)]
		public float SetFlySpeed = 0f;
		[LabelText("生命周期"), FoldoutGroup("配置", true)]
		public float SetLifetime = 1f;



		[LabelText("在一个区域内生成"), SerializeField, FoldoutGroup("配置", true)]
		public bool SpawnInArea = false;
		
		[LabelText("区域于X/Z方向上的随机范围")]
		[ShowIf("SpawnInArea")]
		[FoldoutGroup("配置", true)]
		public Vector2 AreaRange = new Vector2(3, 3);

		[LabelText("包含 预警——生成 过程？"), FoldoutGroup("配置", true)]
		public bool IncludeWarning = false;
		
		[NonSerialized]
		public bool WarningProgress_HasStarted = false;

		
		
		
		private class PerInfo
		{
			public float WillSpawnTime;
			public Vector3 SpawnPosition;
			public Vector3 SpawnDirection;
			public float SpawnSize;
			public GameObject VFXInstance;
		}
		
		public BaseLayoutHandler GetHandlerRef()
		{
			return _parentProjectileLayoutConfig.LayoutHandlerFunction;
		}
		public void RespondToSpawnOperation(LayoutSpawnInfo_OneSpawn oneSpawnInfo, BaseLayoutHandler handler, List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo> collection,
			float currentTime, int currentFrame, int seriesIndex, int shootsIndex)
		{

			Vector3 fromPos, fromDirection;
			fromPos = handler.OverrideSpawnFromPosition.HasValue ? handler.OverrideSpawnFromPosition.Value
				: _parentProjectileLayoutConfig.LayoutHandlerFunction.GetCasterPosition();
			if (SpawnInArea)
			{
				fromPos = fromPos + new Vector3(UnityEngine.Random.Range(-AreaRange.x, AreaRange.x),
					0,
					UnityEngine.Random.Range(-AreaRange.y, AreaRange.y));
				(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromPosition(fromPos,
					oneSpawnInfo,
					handler);
			}
			else
			{
				(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromPosition(fromPos,
					oneSpawnInfo,
					handler);
			}


			fromDirection = handler.OverrideSpawnFromDirection.HasValue ? handler.OverrideSpawnFromDirection.Value
				: _parentProjectileLayoutConfig.LayoutHandlerFunction.GetCasterForwardDirection();
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromSize(
				handler.RelatedProjectileLayoutConfigRef.LayoutContentInSO.RelatedProjectileScale,
				oneSpawnInfo,
				handler);
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromDirection(fromDirection, oneSpawnInfo,
				handler);

			
			WarningProgress_HasStarted = true;

			for (int i = 0; i < TotalSpawnCount; i++)
			{
				ProjectileBehaviour_Runtime projectile = _parentProjectileLayoutConfig.LayoutHandlerFunction
					.GetLayoutRelatedAvailableProjectile();
				float spawnSize = oneSpawnInfo.SpawnSize;
				projectile.RelatedSeriesIndex = seriesIndex;
				projectile.RelatedShootsIndex = shootsIndex;
				projectile.FromLayoutComponentRef = this;
				Vector3 finalSpawnPos = oneSpawnInfo.SpawnFromPosition;
				Vector3 direction = oneSpawnInfo.SpawnFromDirection;
				projectile.StartPosition = finalSpawnPos;
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartPosition(
					projectile,
					finalSpawnPos,
					handler);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartDirection(
					projectile,
					direction,
					handler);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartSpeed(projectile,
					SetFlySpeed);

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection)
					.SetOneProjectileSize(projectile, spawnSize);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreLifetime(projectile,
					SetLifetime);


				(this as ILayoutComponent_CanAddProjectileToSpawnCollection)
					.SetOneProjectileBaseInfoDoneAndAddToWaitList(projectile,
						collection,
						currentTime,
						seriesIndex,
						shootsIndex,
						0);	
			}
		}


		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);

			if (WarningProgress_HasStarted && IncludeWarning)
			{
				
				
				
			}
			
			
			
		}
	}
}