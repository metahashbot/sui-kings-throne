using System;
using System.Collections.Generic;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
namespace RPGCore.Projectile.Layout
{
	[Serializable]
	[TypeInfoBox("NWay。，发射时可以扰动")]
	public class LC_NWay : BaseProjectileLayoutComponent, ILayoutComponent_RespondToSpawn,
		ILayoutComponent_CanAddProjectileToSpawnCollection
	{
		[LabelText("路数。偶数way瞄玩家就是完全打不到，奇数way瞄玩家就是自机狙"), FoldoutGroup("配置", true)]
		public int WayCount;


		[LabelText("散布方式"), FoldoutGroup("配置", true)]
		public LayoutArrangeTypeEnum ArrangeType = LayoutArrangeTypeEnum.EqualSpread_圆周等距;


		[LabelText("整体默认生命周期"), FoldoutGroup("配置", true)]
		public float DefaultLifetime = 10f;

		[LabelText("散布角度"),
		 ShowIf(
			 "@this.ArrangeType == LayoutArrangeTypeEnum.Random_随机 || this.ArrangeType == LayoutArrangeTypeEnum.ArrangeSpread_弧上等距"),
		 FoldoutGroup("配置", true)]
		public float SpreadAngle = 30f;

		[LabelText("扰动角度，每颗都会扰动 | 正右为0"), FoldoutGroup("配置", true)]
		public Vector2 OffsetAngleRange = Vector2.zero;

		[LabelText("需要根据方向进行角度偏移"), SerializeField, FoldoutGroup("配置", true)]
		[PropertyOrder(20)]
		private bool NeedInwardByWorldRight = false;
		
		[ShowIf(nameof(NeedInwardByWorldRight)),InfoBox("在怪物右方则逆时针偏移，左方则顺时针偏移")]
		public float InwardAngle = 0f;
		
		
		[LabelText("飞行速度"), FoldoutGroup("配置", true)]
		public float FlySpeed;

		[LabelText("加入生成队列时任务等级，通常不用动"), FoldoutGroup("配置", true)]
		public int SpawnIndex = 0;
		/// <summary>
		/// <para>表示生成的时候，需不需要向它本身的朝向再往前一段距离，0就是在原地生成</para>
		/// </summary>
		[LabelText("生成距离修正"), FoldoutGroup("配置", true)]
        [HideIf("@this.SpawnDistanceInRange")]
        public float SpawnDistanceOffset = 0f;

		[SerializeField, LabelText("生成距离是个范围"), FoldoutGroup("配置", true)]
		public bool SpawnDistanceInRange;

		[SerializeField, LabelText("   生成距离范围"), FoldoutGroup("配置", true), ShowIf("@this.SpawnDistanceInRange")]
		public Vector2 SpawnDistanceRange;

		[SerializeField, LabelText("   正负都有可能？"), FoldoutGroup("配置", true), ShowIf("@this.SpawnDistanceInRange")]
		public bool RandomInOppisite;

        [LabelText("包含预配置的位置修正"), FoldoutGroup("配置", true)] [SerializeField]
		protected bool ContainPresetPositionOffset;

		[LabelText("    在两个轴各修正多少：X右 | Y前 "),
		 ShowIf("@this.ContainPresetPositionOffset"), FoldoutGroup("配置", true)] [SerializeField]
		protected Vector3 OffsetLength;
		
		private bool _needOffset = false;

		private bool _needInward = false;

		public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime,
			int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
		}

		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
		}

		public BaseLayoutHandler GetHandlerRef()
		{
			return _parentProjectileLayoutConfig.LayoutHandlerFunction;
		}
		public void RespondToSpawnOperation(LayoutSpawnInfo_OneSpawn oneSpawnInfo, BaseLayoutHandler handler,
			List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo> collection, float currentTime, int currentFrame,
			int seriesIndex, int shootsIndex)
		{
			
			if (OffsetAngleRange.sqrMagnitude > 0.01f)
			{
				_needOffset = true;
			}
			
			if (InwardAngle > 0.01f)
			{
				_needInward = true;
				
			}
			

			Vector3 spawnOperationFromPosition, spawnOperationFromDirection;


			spawnOperationFromPosition = handler.OverrideSpawnFromPosition ?? _parentProjectileLayoutConfig.LayoutHandlerFunction.GetCasterPosition();
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromPosition(spawnOperationFromPosition,
				oneSpawnInfo,handler);
			spawnOperationFromPosition = oneSpawnInfo.SpawnFromPosition;
			
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromSize(
				handler.RelatedProjectileLayoutConfigRef.LayoutContentInSO.RelatedProjectileScale,
				oneSpawnInfo,handler);


			spawnOperationFromDirection = handler.OverrideSpawnFromDirection ?? Vector3.right;
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromDirection(spawnOperationFromDirection,
				oneSpawnInfo,
				handler);
			spawnOperationFromDirection = oneSpawnInfo.SpawnFromDirection;
			//基本瞄准角度。在XZ平面上计算。0度表示正右方(1,|,0)，顺时针为负，逆时针为正

			float baseAimAngleFrom = Mathf.Atan2(oneSpawnInfo.SpawnFromDirection.z, oneSpawnInfo.SpawnFromDirection.x) *
			                         Mathf.Rad2Deg;

			//Nway自己的for循环，
			for (int currentShootIndex = 0; currentShootIndex < WayCount; currentShootIndex++)
			{
				Vector2 rotateDir = Vector2.zero;
				ProjectileBehaviour_Runtime projectile = _parentProjectileLayoutConfig.LayoutHandlerFunction
					.GetLayoutRelatedAvailableProjectile();
				float spawnSize = oneSpawnInfo.SpawnSize;
				projectile.RelatedSeriesIndex = seriesIndex;
				projectile.RelatedShootsIndex = shootsIndex;
				projectile.FromLayoutComponentRef = this;
				//偶数way
				if (WayCount % 2 == 0)
				{
					switch (ArrangeType)
					{
						case LayoutArrangeTypeEnum.Random_随机:
							float rangeRaw = SpreadAngle / 2f;
							float angle = baseAimAngleFrom + Random.Range(-rangeRaw, rangeRaw) + InwardAngle;
							if (_needOffset)
							{
								angle += Random.Range(OffsetAngleRange.x, OffsetAngleRange.y);
							}
							
							if (_needInward && spawnOperationFromDirection.x < 0)
							{
								baseAimAngleFrom += InwardAngle;
							}
							else if (_needInward && spawnOperationFromDirection.x > 0)
							{
								baseAimAngleFrom -= InwardAngle;
							}
							
							rotateDir = MathExtend.Vector2Rotate(Vector2.right, angle);

							break;
						case LayoutArrangeTypeEnum.EqualSpread_圆周等距:
							float es_perWaySplit = 360f / WayCount;
							float es_baseAngle = baseAimAngleFrom - 360f / 2f;
							float currentShootAngleOffset = es_baseAngle + es_perWaySplit * currentShootIndex;
							if (_needOffset)
							{
								currentShootAngleOffset += Random.Range(OffsetAngleRange.x, OffsetAngleRange.y);
							}

							if (_needInward && spawnOperationFromDirection.x < 0)
							{
								baseAimAngleFrom += InwardAngle;
							}
							else if (_needInward && spawnOperationFromDirection.x > 0)
							{
								baseAimAngleFrom -= InwardAngle;
							}
							
							rotateDir = MathExtend.Vector2Rotate(Vector2.right, currentShootAngleOffset);
							break;
						case LayoutArrangeTypeEnum.ArrangeSpread_弧上等距:
							float perWaySplit_Spread = (SpreadAngle + InwardAngle) / (WayCount - 1);
							float baseAngle = baseAimAngleFrom - (SpreadAngle + InwardAngle) / 2f;
							float cs_currentShootAngleOffset = baseAngle + currentShootIndex * perWaySplit_Spread;
							if (_needOffset)
							{
								cs_currentShootAngleOffset += Random.Range(OffsetAngleRange.x, OffsetAngleRange.y);
							}
							
							if (_needInward && spawnOperationFromDirection.x < 0)
							{
								baseAimAngleFrom += InwardAngle;
							}
							else if (_needInward && spawnOperationFromDirection.x > 0)
							{
								baseAimAngleFrom -= InwardAngle;
							}
							
							rotateDir = MathExtend.Vector2Rotate(Vector2.right, cs_currentShootAngleOffset);
							break;
					}
				}
				//奇数way
				else
				{
					//只有1way，按照fromDirection即可
					if (WayCount == 1)
					{
						if (_needOffset)
						{
							baseAimAngleFrom += Random.Range(OffsetAngleRange.x, OffsetAngleRange.y);
						}
						
						if (_needInward && spawnOperationFromDirection.x < 0)
						{
							baseAimAngleFrom += InwardAngle;
						}
						else if (_needInward && spawnOperationFromDirection.x > 0)
						{
							baseAimAngleFrom -= InwardAngle;
						}
						
						rotateDir = MathExtend.Vector2Rotate(Vector2.right, baseAimAngleFrom);
					}
					//奇数way，且瞄着玩家，则第一颗的基本修正就是基准玩家方向修正baseAimPlayerOffsetAngle，随后每个增加way之间的角度
					else
					{
						switch (ArrangeType)
						{
							case LayoutArrangeTypeEnum.Random_随机:
								float rangeRaw = (SpreadAngle + InwardAngle) / 2f;
								float angle = baseAimAngleFrom + Random.Range(-rangeRaw, rangeRaw);
								if (_needOffset)
								{
									angle += Random.Range(OffsetAngleRange.x, OffsetAngleRange.y);
								}
								if (_needInward && spawnOperationFromDirection.x < 0)
								{
									baseAimAngleFrom += InwardAngle;
								}
								else if (_needInward && spawnOperationFromDirection.x > 0)
								{
									baseAimAngleFrom -= InwardAngle;
								}
								rotateDir = MathExtend.Vector2Rotate(Vector2.right, angle);
								break;
							case LayoutArrangeTypeEnum.EqualSpread_圆周等距:
								float perWaySplitAngle = 360f / WayCount;
								float currentShootAngleOffset = baseAimAngleFrom + perWaySplitAngle * currentShootIndex;
								if (_needOffset)
								{
									currentShootAngleOffset += Random.Range(OffsetAngleRange.x, OffsetAngleRange.y);
								}
								if (_needInward && spawnOperationFromDirection.x < 0)
								{
									baseAimAngleFrom += InwardAngle;
								}
								else if (_needInward && spawnOperationFromDirection.x > 0)
								{
									baseAimAngleFrom -= InwardAngle;
								}
								rotateDir = MathExtend.Vector2Rotate(Vector2.right, currentShootAngleOffset);
								break;
							case LayoutArrangeTypeEnum.ArrangeSpread_弧上等距:
								float perWaySplit_Spread = (SpreadAngle + InwardAngle) / (WayCount - 1);
								float baseAngle = baseAimAngleFrom - perWaySplit_Spread * (WayCount - 1) / 2;
								float cs_currentShootAngleOffset = baseAngle + currentShootIndex * perWaySplit_Spread;
								if (_needOffset)
								{
									cs_currentShootAngleOffset += Random.Range(OffsetAngleRange.x, OffsetAngleRange.y);
								}
								if (_needInward && spawnOperationFromDirection.x < 0)
								{
									baseAimAngleFrom += InwardAngle;
								}
								else if (_needInward && spawnOperationFromDirection.x > 0)
								{
									baseAimAngleFrom -= InwardAngle;
								}
								rotateDir = MathExtend.Vector2Rotate(Vector2.right, cs_currentShootAngleOffset);
								break;
						}
					}
				}
				
				Vector3 offsetPos = Vector3.zero;
				//如果包含距离修正，则向其起始位置加上这个修正，是 修正距离 * 方向
				if (!SpawnDistanceInRange)
				{
                    if (SpawnDistanceOffset > Mathf.Epsilon)
                    {
                        Vector3 dirNor = new Vector3(rotateDir.x, 0f, rotateDir.y).normalized;
                        offsetPos += dirNor * SpawnDistanceOffset;
                    }
                }
				else
                {
                    Vector3 dirNor = new Vector3(rotateDir.x, 0f, rotateDir.y).normalized;
                    float rInX = UnityEngine.Random.Range(SpawnDistanceRange.x, SpawnDistanceRange.y);
					if (RandomInOppisite)
					{
						rInX *= (Random.Range(0, 2) == 0 ? 1 : -1f );

                    }
					offsetPos += dirNor * rInX; ;
                }
			

				if (ContainPresetPositionOffset)
				{
					
					offsetPos += 
						Vector3.right * OffsetLength.x +
						Vector3.forward * OffsetLength.y;
				}

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartPosition(
					projectile,
					spawnOperationFromPosition + offsetPos,handler);


				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartDirection(
					projectile,
					rotateDir,handler);

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartSpeed(projectile,
					FlySpeed);

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection)
					.SetOneProjectileSize(projectile, spawnSize);

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreLifetime(projectile,
					DefaultLifetime);
				

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection)
					.SetOneProjectileBaseInfoDoneAndAddToWaitList(projectile,
						collection,
						currentTime,
						seriesIndex,
						shootsIndex,
						SpawnIndex);
			}
		}
		public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
		{
		}
	}
}