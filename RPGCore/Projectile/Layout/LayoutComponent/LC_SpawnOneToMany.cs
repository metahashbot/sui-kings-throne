using System;
using System.Collections.Generic;
using Global.ActionBus;
using Global.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace RPGCore.Projectile.Layout
{
	[Serializable]
	public class LC_SpawnOneToMany : BaseProjectileLayoutComponent,ILayoutComponent_CanAddProjectileToSpawnCollection
	{
		[LabelText("一变几")]
		public int ConversionCount = 2;

		[LabelText("散布宽度，会在正负两个方向上都散布的")]
		public float SpreadWidth = 0.5f;

		[LabelText("√：均匀散布；口：随机散布")]
		public bool SpreadEven = true;

		[LabelText("转换导致的延迟时间，0就是直接换")]
		public float SpawnDelay = 0f;

		[LabelText("关注的生成队列任务等级")]
		public int ReactToSpawnIndex = 0;
		
		[LabelText("加入生成队列时任务等级，通常不用动")]
		public int SpawnIndex = 1;
		
		

		public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime,
			int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
			_parentProjectileLayoutConfig.LayoutHandlerFunction.RegisterBaseSpawnDoneFromIRespondToSpawn(
				_ABC_ProcessOneToMany_OnProjectileBaseSetDoneOnSpawn);
		}

		private void _ABC_ProcessOneToMany_OnProjectileBaseSetDoneOnSpawn(DS_ActionBusArguGroup ds)
		{
			var targetProjectile = ds.ObjectArgu1 as ProjectileBehaviour_Runtime;
			var targetLayout = targetProjectile.FromLayoutComponentRef as BaseProjectileLayoutComponent;
			//如果不是同一个LayoutConfig发起的，则与自己无关，直接return
			if (!CheckIfSameLayoutConfigParent(targetLayout))
			{
				return;
			}
			
			if(ds.IntArgu1.Value != ReactToSpawnIndex)
			{
				return;
			}
			
			
			var collection = ds.ObjectArgu2 as List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>;
			float spawnTime = ds.FloatArgu1.Value;


			ProjectileBehaviour_Runtime originalProjectile = collection[collection.Count - 1].ProjectileBehaviour;
			collection[collection.Count - 1].WillSpawnTime += SpawnDelay;
			
			Vector3 originalPos = originalProjectile.StartPosition;
			Vector2 originalDir = new Vector2(originalProjectile.StartDirection.x, originalProjectile.StartDirection.z);
			Vector3 targetStartPosition = originalPos;
 
			for (int conversionIndex = 0;
				conversionIndex < ConversionCount;
				conversionIndex++)
			{
				ProjectileBehaviour_Runtime currentProcessProjectile = null;
				//最后一个的时候，才改原始的那个Projectile。因为之前的那几个需要原始的projectile做数据参照，改了就没了。
				if (conversionIndex == ConversionCount - 1)
				{
					currentProcessProjectile = originalProjectile;
				}
				//不是最后一个，则从PBM那再拿个新的
				else
				{
					currentProcessProjectile =
						_parentProjectileLayoutConfig.LayoutHandlerFunction.GetLayoutRelatedAvailableProjectile();
					currentProcessProjectile.InitializeOnGetFromSame(originalProjectile);
				}
				float offsetResult = 0f;
				
				
				if (SpreadEven)
				{
					float currentPartial =
						conversionIndex / (float)(ConversionCount - 1); //偏移结果小于0，则顺时针转90度
					offsetResult = currentPartial.Remap(0f,
						1f,
						-SpreadWidth,
						SpreadWidth);
				}
				else
				{
					offsetResult = UnityEngine.Random.Range(-SpreadWidth,
						SpreadWidth);
				}
				Vector2 _newRotateDir;
				if (offsetResult < 0f)
				{
					//根据当前方向，顺时针转90度，然后加一点距离
					_newRotateDir = MathExtend.Vector2Rotate(originalDir, 90f);
					offsetResult = Mathf.Abs(offsetResult);
					Vector2 offsetDistance = offsetResult * _newRotateDir;
					targetStartPosition+= new Vector3(offsetDistance.x, 0f, offsetDistance.y);
				}
				else
				{
					//根据当前方向，顺时针转90度，然后加一点距离
					_newRotateDir = MathExtend.Vector2Rotate(originalDir, -90f);
					Vector2 offsetDistance = offsetResult * _newRotateDir;
					targetStartPosition+= new Vector3(offsetDistance.x, 0f, offsetDistance.y);
				}

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartPosition(
					currentProcessProjectile,
					targetStartPosition,
					_parentProjectileLayoutConfig.LayoutHandlerFunction);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartDirection(
					currentProcessProjectile,
					originalDir,
					_parentProjectileLayoutConfig.LayoutHandlerFunction);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartSpeed(
					currentProcessProjectile,
					currentProcessProjectile.StartSpeed);



				//最后一个，则不需要添加新的
				if (conversionIndex == ConversionCount - 1)
				{
				}
				//不是最后一个，则需要广播有东西被新添加了，
				else
				{
					(this as ILayoutComponent_CanAddProjectileToSpawnCollection)
						.SetOneProjectileBaseInfoDoneAndAddToWaitList(currentProcessProjectile,
							collection,
							spawnTime + SpawnDelay,
							originalProjectile.RelatedSeriesIndex,
							originalProjectile.RelatedShootsIndex,
							SpawnIndex);
				}

			}
		}


		public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
		{
			_parentProjectileLayoutConfig.LayoutHandlerFunction.UnregisterBaseSpawnDownFromIRespondToSpawn(_ABC_ProcessOneToMany_OnProjectileBaseSetDoneOnSpawn);
		}

	}
}