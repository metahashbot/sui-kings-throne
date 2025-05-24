using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Enemy;
using ARPG.Character.Player;
using ARPG.Manager;
using ExcelData.Function;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
namespace RPGCore.Projectile.Layout
{
	/// <summary>
	/// <para>管理当前所有ProjectileLayout</para>
	/// <para>同时包括对于所有Layout的子功能：碰撞检测</para>
	/// </summary>
	public class ProjectileLayoutManager : MonoBehaviour
	{
#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif
#region 外部引用

		private SubGameplayLogicManager_ARPG _glmRef;
		private ProjectileBehaviourManager _pbmRef;
		private CharacterOnMapManager _characterOnMapManagerRef;
		private PlayerCharacterBehaviourController _playerControllerRef;

#endregion


		private float _currentTime;
		private int _currentFrame;



		[LabelText("当前正在运转的所有投射物布局配置"), FoldoutGroup("运行时", true)]
		[ShowInInspector, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public List<SOConfig_ProjectileLayout> CurrentAllLayoutConfigList;

		[LabelText("通用消弹的Layout"), FoldoutGroup("配置", true), SerializeField]
		private SOConfig_ProjectileLayout _selfCommonNeutralizeLayoutConfig;


		public void StopLayoutByUIDAndCaster(
			string uid,
			I_RP_Projectile_ObjectCanReleaseProjectile caster)
		{
			foreach (SOConfig_ProjectileLayout perConfig in CurrentAllLayoutConfigList)
			{
				if (perConfig.LayoutContentInSO.LayoutUID.Equals(uid, StringComparison.OrdinalIgnoreCase) &&
				    perConfig.LayoutHandlerFunction.CheckIfSameCaster(caster))
				{
					perConfig.LayoutHandlerFunction.StopLayout();
				}
			}
			return;
		}



		public void AwakeInitialize(SubGameplayLogicManager_ARPG glm, ProjectileBehaviourManager pbm)
		{
			_glmRef = glm;
			_pbmRef = pbm;
			_characterOnMapManagerRef = _glmRef.CharacterOnMapManagerReference;
			_playerControllerRef = glm.PlayerCharacterBehaviourControllerReference;
		}





		public void LateInitialize()
		{
			BaseLayoutHandler.InitializeStatic(this, _pbmRef);
		}



		public void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			_currentTime = currentTime;
			_currentFrame = currentFrame;

			cc_ProjectileInfoInMonoList.Clear();
			cc_RPBehaviourInfoInMonoList.Clear();

			foreach (var perConfig in CurrentAllLayoutConfigList)
			{
				perConfig.LayoutHandlerFunction.FixedUpdateTick(currentTime,
					currentFrame,
					delta,
					cc_ProjectileInfoInMonoList);
			}	

			//每隔一定帧数进行配置实例的清理
			if (currentFrame % 5 == 0)
			{
				for (int i = CurrentAllLayoutConfigList.Count - 1; i >= 0; i--)
				{
					var currentConfig = CurrentAllLayoutConfigList[i];
					if (currentConfig.LayoutHandlerFunction.CheckIfNeedClear())
					{
						currentConfig.LayoutHandlerFunction.ClearAndUnload();
						UnityEngine.Object.Destroy(currentConfig);
						CurrentAllLayoutConfigList.RemoveAt(i);
					}
				}
			}



			CollisionCheck_FixedUpdateTick(currentTime, currentFrame, delta);
		}
#region 响应
		
		
		
		
		

		// private void _ABC_ProcessEnemyDeathNeutralize_SpawnNeutralizeProjectileLayout(DS_ActionBusArguGroup ds)
		// {
		// 	EnemyARPGCharacterBehaviour behaviour = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;
		// 	var neutralizeEntry = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.DeathNeutralizeRange_死亡消弹范围);
		// 	if (neutralizeEntry == null)
		// 	{
		// 		return;
		// 	}
		//
		// 	SOConfig_ProjectileLayout layout = _selfCommonNeutralizeLayoutConfig.SpawnLayout_NoAutoStart(behaviour);
		// 	layout.LayoutContentInSO.CollisionInfo.Radius = neutralizeEntry.CurrentValue;
		// 	layout.LayoutHandlerFunction.StartLayout();
		// }

#endregion

#region 生成

		// /// <summary>
		// /// <para>根据一个Layout来生成内容。会直接生成</para>
		// /// <para>返回的是这个LayoutConfig的运行时实例，是一个全新的</para>
		// /// </summary>
		// public SOConfig_ProjectileLayout SpawnProjectileLayoutBySOConfig(SOConfig_ProjectileLayout configRaw,
		// 	I_RP_Projectile_ObjectCanReleaseProjectile caster,bool autoSetLayerMask = true)
		// {
		// 	return SpawnProjectileLayoutBySOConfig(configRaw, caster, autoSetLayerMask, null);
		// }


		/// <summary>
		/// 从测试环境下生成一个Layout，会直接从场景中的一个测试Caster身上生成
		/// <para>调用过来的时候一定是正在运行的</para>
		/// </summary>
		/// <param name="configRaw"></param>
		public void Debug_SpawnProjectileLayout(SOConfig_ProjectileLayout configRaw)
		{
			RefreshPlayerRef();


			SpawnProjectileLayoutBySOConfig(configRaw, _playerControllerRef.CurrentControllingBehaviour);
		}



		/// <summary>
		/// <para>根据一个Layout来生成内容。会直接生成</para>
		/// <para>返回的是这个LayoutConfig的运行时实例，是一个全新的</para>
		/// <para>额外接受若干忽略项</para>
		/// <para>默认操作总是会判定根据是Player还是Enemy来修正为默认的碰撞层数</para>
		/// </summary>
		public SOConfig_ProjectileLayout SpawnProjectileLayoutBySOConfig(SOConfig_ProjectileLayout configRaw,
			I_RP_Projectile_ObjectCanReleaseProjectile caster,bool autoSetLayerMask = true, bool casterIgnore = true,params RolePlay_BaseBehaviour[] ignores)
		{
			SOConfig_ProjectileLayout runtimeConfig =
				SpawnProjectileLayoutBySOConfig_NotAutoStart(configRaw, caster, autoSetLayerMask, casterIgnore, ignores);

			runtimeConfig.LayoutHandlerFunction.StartLayout();
			return runtimeConfig;
		}

		public SOConfig_ProjectileLayout SpawnProjectileLayoutBySOConfig_NotAutoStart(
			SOConfig_ProjectileLayout configRaw, I_RP_Projectile_ObjectCanReleaseProjectile caster,bool autoSetLayerMask = true, bool casterIgnore = true,
			params RolePlay_BaseBehaviour[] ignores)
		{
			SOConfig_ProjectileLayout runtimeConfig = Instantiate(configRaw);
			if(autoSetLayerMask)
			{
				if (caster is PlayerARPGConcreteCharacterBehaviour)
				{
					//如果是玩家的子弹，那需要对敌人和敌人子弹判定
					runtimeConfig.LayoutContentInSO.CollisionInfo.CollisionLayerMask = (1) | (1 << 2);
				}
				else if (caster is EnemyARPGCharacterBehaviour)
				{
					runtimeConfig.LayoutContentInSO.CollisionInfo.CollisionLayerMask = (1 << 1) | (1 << 3);
				}
			}
			caster.GetRelatedActionBus().TriggerActionByType(
				ActionBus_ActionTypeEnum.L_PLayout_OnLayoutConfigInstantiateRuntime_当布局配置被运行时实例化,
				new DS_ActionBusArguGroup(
					ActionBus_ActionTypeEnum.L_PLayout_OnLayoutConfigInstantiateRuntime_当布局配置被运行时实例化,
					runtimeConfig));
			runtimeConfig.LayoutHandlerFunction.InitializeOnInstantiate(runtimeConfig,
				caster,
				configRaw,
				_currentTime,
				_currentFrame, casterIgnore);

			if (ignores != null)
			{
				runtimeConfig.LayoutHandlerFunction.AddTargetToIgnoreList(ignores);
			}
			CurrentAllLayoutConfigList.Add(runtimeConfig);
			return runtimeConfig;
		}

#endregion
#region 停止

		/// <summary>
		/// <para>试图停止指定UID和相同caster的Layout，可选一并清理已存在投射物</para>
		/// </summary>
		public void TryStopLayout(string uid, I_RP_Projectile_ObjectCanReleaseProjectile caster, bool alsoClear = false)
		{
			foreach (SOConfig_ProjectileLayout perLayout in CurrentAllLayoutConfigList)
			{
				if (perLayout.LayoutContentInSO.LayoutUID.Equals(uid, StringComparison.OrdinalIgnoreCase) &&
				    perLayout.LayoutHandlerFunction.CheckIfSameCaster(caster))
				{
					perLayout.LayoutHandlerFunction.StopLayout();
					if (alsoClear)
					{
						perLayout.LayoutHandlerFunction.ClearLayout(true);
					}
				}
			}
		}

#endregion
#region 查找

		private static List<SOConfig_ProjectileLayout> _internalFindLayoutList = new List<SOConfig_ProjectileLayout>();
		 /// <summary>
		 ///  <para>根据Layout的UID查询所有当前还活跃着的Layout。如果没有传入接受查找结果的容器，会使用内部static的公共容器，不要cache它</para>
		 /// </summary>
		 /// <param name="uid"></param>
		 /// <param name="findList"></param>
		 /// <returns></returns>
		 public List<SOConfig_ProjectileLayout> FindLayoutByUID(
			 string uid,
			 List<SOConfig_ProjectileLayout> findList = null)
		 {
			 List<SOConfig_ProjectileLayout> returnList = findList == null ? _internalFindLayoutList : findList;
			 returnList.Clear();
			 foreach (SOConfig_ProjectileLayout perLayout in CurrentAllLayoutConfigList)
			 {
				 if (perLayout.LayoutContentInSO.LayoutUID.Equals(uid, StringComparison.OrdinalIgnoreCase))
				 {
					 returnList.Add(perLayout);
				 }
			 }
			 return returnList;
		
		}

#endregion
#region 测试场景

		[LabelText("测试——玩家角色")]
		public PlayerARPGConcreteCharacterBehaviour DebugPlayerCharacterRef;
		//
		// [LabelText("测试——木桩子们")]
		// public enemyb


		public void RefreshPlayerRef()
		{
			DebugPlayerCharacterRef = _playerControllerRef.CurrentControllingBehaviour;
		}

#endregion

		public void UpdateTick(float ct, int cf, float delta)
		{
			foreach (var perConfig in CurrentAllLayoutConfigList)
			{
				perConfig.LayoutHandlerFunction.UpdateTick(ct, cf, delta);
			}
		}
#region 碰撞检测

		private List<CollisionCheckInfo_ProjectileFull> cc_ProjectileInfoInMonoList =
			new List<CollisionCheckInfo_ProjectileFull>();
		private List<CollisionCheckInfo_RPBehaviourFull> cc_RPBehaviourInfoInMonoList =
			new List<CollisionCheckInfo_RPBehaviourFull>();
		
		


		public void CollisionCheck_FixedUpdateTick(float ct, int cf, float delta)
		{
#if UNITY_EDITOR
			Profiler.BeginSample("PBM-FixedUpdate");
			Profiler.BeginSample("PBM_PoolTick");

#endif

			//构建场上所有需要计算碰撞的东西
			// 实际构建的时候需要不止一步，比如将一个引用类型的数据传递给能够构建这些数据的地方来构建（e.g.将投射物信息容器 在layout Tick时传过去，以获得真的需要参与运算的投射物们）
			// 1.所有Projectile
			// 2.所有RolePlayBehaviour
			// TODO:所有Dispatcher
			// TODO:所有Explosion

			JobHandle _finalHandle;






#if UNITY_EDITOR
			Profiler.EndSample();
			Profiler.BeginSample("PBM_Phase1");
#endif
			// 1.1 Projectile  ——  通过Mono的投射物信息容器，来构建job版的信息容器	

			NativeArray<CC_ProjectileInfoInJob> CC_ProjectileInfoArray =
				new NativeArray<CC_ProjectileInfoInJob>(cc_ProjectileInfoInMonoList.Count, Allocator.TempJob);

			for (int i = 0; i < cc_ProjectileInfoInMonoList.Count; i++)
			{
				CollisionCheckInfo_ProjectileFull projectileInfo_Mono = cc_ProjectileInfoInMonoList[i];

				var currentTransform = projectileInfo_Mono.ProjectileBehaviourRef.RelatedGORef.transform;
				CC_ProjectileInfoInJob currentInfoInJob = new CC_ProjectileInfoInJob();
				currentInfoInJob.Index = i;

				currentInfoInJob.CollisionMaskLayer = projectileInfo_Mono.ProjectileBehaviourRef.CollisionMaskLayer;

				currentInfoInJob.Radius = projectileInfo_Mono.ColliderCircleRadius *
				                          projectileInfo_Mono.ProjectileBehaviourRef.CurrentLocalSize;


				Vector3 position = currentTransform.position;
				currentInfoInJob.FromPosition = new float2(position.x, position.z);

				CC_ProjectileInfoArray[i] = currentInfoInJob;
			}


			// 1.2  RPBehaviour  ——  通过Mono的RPBehaviour信息容器，来构建job版的信息容器



			//1.2拿一下所有的RPBehaviour碰撞信息

			cc_RPBehaviourInfoInMonoList.Clear();
			_characterOnMapManagerRef.GetRPBehaviourCollisionInfo(cc_RPBehaviourInfoInMonoList);

			NativeArray<CC_RPBehaviourInfoInJob> CC_RPBehaviourInfoArray =
				new NativeArray<CC_RPBehaviourInfoInJob>(cc_RPBehaviourInfoInMonoList.Count, Allocator.TempJob);
			for (int i = 0; i < cc_RPBehaviourInfoInMonoList.Count; i++)
			{
				CollisionCheckInfo_RPBehaviourFull rpBehaviourInfo_Mono = cc_RPBehaviourInfoInMonoList[i];

				CC_RPBehaviourInfoInJob currentInfoInJob = new CC_RPBehaviourInfoInJob();
				currentInfoInJob.Index = i;
				currentInfoInJob.CollisionMaskLayer = rpBehaviourInfo_Mono.LayerMask;
				currentInfoInJob.ColliderInfo = rpBehaviourInfo_Mono.ColliderInfo;
				currentInfoInJob.ColliderOffsetPos = rpBehaviourInfo_Mono.ColliderOffsetPos;
				currentInfoInJob.FromPos = rpBehaviourInfo_Mono.FromPos;
				CC_RPBehaviourInfoArray[i] = currentInfoInJob;
			}


#if UNITY_EDITOR
			Profiler.EndSample();
			Profiler.BeginSample("PBM_Phase2");
#endif


			//2进行各种碰撞计算，拿回结果

			//2.1 Projectile - RPBehaviour 的常规碰撞
			NativeQueue<CCResult_ProjectileToRPBehaviour> result_PtoRPB =
				new NativeQueue<CCResult_ProjectileToRPBehaviour>(Allocator.TempJob);
			Job_CollisionCheckPhase_1_ProjectileWithRPBehaviour checkJob_2_1 =
				new Job_CollisionCheckPhase_1_ProjectileWithRPBehaviour
				{
					FromProjectileArray = CC_ProjectileInfoArray,
					RPBehaviourArray = CC_RPBehaviourInfoArray,
					ResultQueue = result_PtoRPB.AsParallelWriter()
				};

			checkJob_2_1.Run(CC_ProjectileInfoArray.Length);

			// _finalHandle = checkJob_2_1.Schedule(CC_ProjectileInfoArray.Length, 32);
			//





#if UNITY_EDITOR
			Profiler.EndSample();
			Profiler.BeginSample("PBM_Phase3");
#endif


			//3等一个同步

			// _finalHandle.Complete();

#if UNITY_EDITOR
			Profiler.EndSample();
			Profiler.BeginSample("PBM_Phase4——处理碰撞结果");
#endif



			//结算job结果
			/*4.1，结算2.1的结果
			/	常规碰撞，如果projectile没有穿透的组件、没有    
			/	则发生碰撞
			/		结算projectile上面的伤害、数据项效果、buff效果、击退效果
			/		如果projectile上面没有穿透，则销毁它
			*/
			int ee = result_PtoRPB.Count;
			for (int i = 0; i < ee; i++)
			{
				CCResult_ProjectileToRPBehaviour currentResultProjectileTo = result_PtoRPB.Dequeue();
				CollisionCheckInfo_ProjectileFull resultProjectile =
					cc_ProjectileInfoInMonoList[currentResultProjectileTo.ProjectileIndex];
				CollisionCheckInfo_RPBehaviourFull resultRPBehaviour =
					cc_RPBehaviourInfoInMonoList[currentResultProjectileTo.RPBehaviourIndex];
				if (resultProjectile.ProjectileBehaviourRef.CheckTargetIgnored(resultRPBehaviour.RPBehaviourRef))
				{
					continue;
				}
				resultProjectile.ProjectileBehaviourRef.SelfLayoutConfigReference.LayoutHandlerFunction
					.ReceiveCollisionInfo(resultProjectile.ProjectileBehaviourRef,
						resultRPBehaviour.RPBehaviourRef,
						currentResultProjectileTo.CollisionPositionXZ);


				//销毁的操作不是这里进行的。这里只是广播给Projectile本体，比如说发生了一次碰撞。
				//销毁在下面的集体销毁那里。因为会出现单帧中 单个Projectile碰到了多个RPBehaviour这样的情况，
				//碰撞的结果，包括但不限于是否需要return掉，是projectile自身计算得到的，也是由Projectile自行发起这个调用
				//没有穿透，则销毁
				// if(resultProjectile.BehaviourRef.GetRelatedConfig().RelatedFunctionComponentList.Exists((component => component is //Penetrate )))
				// ReturnProjectile(resultProjectile.BehaviourRef);
			}



#region 销毁结算

#endregion






#if UNITY_EDITOR
			Profiler.EndSample();
			Profiler.BeginSample("PBM_Phase5");
#endif

			//  进行清理
			CC_ProjectileInfoArray.Dispose();
			CC_RPBehaviourInfoArray.Dispose();
			result_PtoRPB.Dispose();

#if UNITY_EDITOR
			Profiler.EndSample();
			Profiler.EndSample();
#endif
		}

#endregion

#region Job们

		/// <summary>
		/// <para>第一部分的job计算。以Projectile数量为基准，</para>
		/// </summary>
		[BurstCompile]
		private struct Job_CollisionCheckPhase_1_ProjectileWithRPBehaviour : IJobParallelFor
		{
			[Unity.Collections.ReadOnly]
			public NativeArray<CC_ProjectileInfoInJob> FromProjectileArray;
			[Unity.Collections.ReadOnly]
			public NativeArray<CC_RPBehaviourInfoInJob> RPBehaviourArray;
			[WriteOnly]
			public NativeQueue<CCResult_ProjectileToRPBehaviour>.ParallelWriter ResultQueue;


			public void Execute(int index)
			{
				CC_ProjectileInfoInJob currentProjectile = FromProjectileArray[index];
				for (int i = 0; i < RPBehaviourArray.Length; i++)
				{
					CC_RPBehaviourInfoInJob currentRPBehaviour = RPBehaviourArray[i];
					int r = (currentProjectile.CollisionMaskLayer & currentRPBehaviour.CollisionMaskLayer);
					if (r == 0)
					{
						continue;
					}

					//CollisionInfo的第一位表明碰撞形状
					// Projectile 是圆的

					// Behaviour 是圆的
					if (currentRPBehaviour.ColliderInfo.x < 1f)
					{
						//圆心距离小于和即算作碰撞
						float2 center_Projectile = currentProjectile.FromPosition;
						float2 center_RPBehaviour = currentRPBehaviour.FromPos + currentRPBehaviour.ColliderOffsetPos;
						float distanceCenterSQ = math.distancesq(center_Projectile, center_RPBehaviour);
						float radiusSum = currentProjectile.Radius + currentRPBehaviour.ColliderInfo.y;
						//圆心距离 小于 半径之和， 则算作碰撞
						if (distanceCenterSQ < (radiusSum * radiusSum))
						{
							var dirFromBehaviourToProjectile = (center_Projectile - center_RPBehaviour);
							dirFromBehaviourToProjectile = math.normalize(dirFromBehaviourToProjectile);
							var distanceOfCollisionPosition = currentRPBehaviour.ColliderInfo.y;
							float2 collidePos = center_RPBehaviour +
							                    dirFromBehaviourToProjectile * distanceOfCollisionPosition;
							ResultQueue.Enqueue(new CCResult_ProjectileToRPBehaviour
							{
								ProjectileIndex = currentProjectile.Index,
								RPBehaviourIndex = currentRPBehaviour.Index,
								CollisionPositionXZ = collidePos,
							});
						}
					}
					//Behaviour是 等腰梯形
					else
					{
						//对于 圆- 一般四边 的碰撞检测，先检测是否出现 circle overlap rect 的情形，
						//如果出现了，则直接认为有碰撞；
						//如果没有出现，则检测  circle 对于 四条 line segment 的情况，如果都没有，则无碰撞；有任意一个，则有碰撞

						float2 center_Projectile = currentProjectile.FromPosition;
						float2 center_RPBehaviour = currentRPBehaviour.FromPos + currentRPBehaviour.ColliderOffsetPos;
						float2 size_rect = currentRPBehaviour.ColliderInfo.yz;
						float inclineRatio = currentRPBehaviour.ColliderInfo.w;
						float projectileRadius = currentProjectile.Radius;
						
						var dirFromBehaviourToProjectile = (center_Projectile - center_RPBehaviour);
						dirFromBehaviourToProjectile = math.normalize(dirFromBehaviourToProjectile);
						var distanceOfCollisionPosition = currentRPBehaviour.ColliderInfo.y / 2f;
						float2 collidePos = center_RPBehaviour +
						                    dirFromBehaviourToProjectile * distanceOfCollisionPosition;
						//1.检测 圆心 是否在梯形内。如果在，则直接判断有碰撞
						//							不在，则判断是否对四条边有相交，有则有碰撞，都无则无碰撞
						float2 point0 = GetInclinedPoint(0, center_RPBehaviour, size_rect, inclineRatio);
						float2 point1 = GetInclinedPoint(1, center_RPBehaviour, size_rect, inclineRatio);
						float2 point2 = GetInclinedPoint(2, center_RPBehaviour, size_rect, inclineRatio);
						float2 point3 = GetInclinedPoint(3, center_RPBehaviour, size_rect, inclineRatio);
						if (IsPointInPolygon(center_Projectile, point0, point1, point2, point3))
						{
							// float2 collidePos = new float2(center_Projectile + center_RPBehaviour) / 2f;
							
							//算算位置。
							ResultQueue.Enqueue(new CCResult_ProjectileToRPBehaviour
							{
								ProjectileIndex = currentProjectile.Index,
								RPBehaviourIndex = currentRPBehaviour.Index,
								CollisionPositionXZ = collidePos,
							});
						}
						//2.梯形四个点是否在圆内，在则直接碰撞，且碰撞位置就是RPB的位置
						else if ( math.distancesq(point0, center_Projectile) < (projectileRadius * projectileRadius) ||
						          math.distancesq(point1, center_Projectile) < (projectileRadius * projectileRadius) ||
						          math.distancesq(point2, center_Projectile) < (projectileRadius * projectileRadius) ||
						          math.distancesq(point3, center_Projectile) < (projectileRadius * projectileRadius))
						{
							ResultQueue.Enqueue(new CCResult_ProjectileToRPBehaviour
							{
								ProjectileIndex = currentProjectile.Index,
								RPBehaviourIndex = currentRPBehaviour.Index,
								CollisionPositionXZ = collidePos,
							});
						}
						//3.圆心不在梯形内，则判断四边交叉
						else
						{
							if (IfIntersection(point0, point1, center_Projectile, projectileRadius) ||
							    IfIntersection(point1, point2, center_Projectile, projectileRadius) ||
							    IfIntersection(point2, point3, center_Projectile, projectileRadius) ||
							    IfIntersection(point3, point0, center_Projectile, projectileRadius))
							{
								ResultQueue.Enqueue(new CCResult_ProjectileToRPBehaviour
								{
									ProjectileIndex = currentProjectile.Index,
									RPBehaviourIndex = currentRPBehaviour.Index,
									CollisionPositionXZ = collidePos,
								});
							}
						}
					}


					// //Projectile 圆，RPBehaviour 方（等腰梯形）
					// else if (currentRPBehaviour.Type == 1)
					// {

					// 	}
				}

				void EnqueueOne()
				{
					
				}
			}



			[BurstCompile]
			public bool IfIntersection(float2 lineEnd, float2 lineStart, float2 circle, float radius)
			{
				float2 d = lineStart - lineEnd;
				float2 f = lineEnd - circle;
				float a = math.dot(d, d);
				float b = 2f * math.dot(f, d);
				float c = math.dot(f, f) - radius * radius;
				float discriminant = b * b - 4f * a * c;
				if (discriminant < 0)
				{
					return false;
				}
				else
				{
					discriminant = math.sqrt(discriminant);
					float t1 = (-b - discriminant) / (2f * a);
					float t2 = (-b + discriminant) / (2f * a);
					if (t1 >= 0f && t1 <= 1f)
					{
						return true;
					}
					if (t2 >= 0f && t2 <= 1f)
					{
						return true;
					}
					return false;
				}
			}

			[BurstCompile]
			public float DistancePointLine(float2 point, float2 lineStart, float2 lineEnd) =>
				math.length(ProjectPointLine(point, lineStart, lineEnd) - point);

			[BurstCompile]
			public float2 ProjectPointLine(float2 point, float2 lineStart, float2 lineEnd)
			{
				float2 rhs = point - lineStart;
				float2 vector2 = lineEnd - lineStart;
				float magnitude = math.length(vector2);
				float2 lhs = vector2;
				if ((double)magnitude > 9.999999974752427E-07)
					lhs /= magnitude;
				float num = math.clamp(math.dot(lhs, rhs), 0.0f, magnitude);
				return lineStart + lhs * num;
			}

			/// <summary>
			/// <para>获取倾斜后的梯形的四个角，0123顺序是 左上-右上-右下-左下</para>
			/// </summary>
			[BurstCompile]
			public float2 GetInclinedPoint(int index, float2 center, float2 size, float ratio)
			{
				switch (index)
				{
					//左上
					case 0:
						return new float2(center.x - size.x / 2f + size.y * ratio, center.y + size.y / 2f);
					//右上
					case 1:
						return new float2(center.x + size.x / 2f - size.y * ratio, center.y + size.y / 2f);
					//右下
					case 2:
						return new float2(center.x + size.x / 2f, center.y - size.y / 2f);
					//左下
					case 3:
						return new float2(center.x - size.x / 2f, center.y - size.y / 2f);
				}
				return float2.zero;
			}

			[BurstCompile]
			public bool IsPointInPolygon(float2 point, float2 point0, float2 point1, float2 point2, float2 point3)
			{
				int polygonLength = 4, checkIndex = 0;
				bool inside = false;
				// x, y for tested point.
				float pointX = point.x, pointY = point.y;
				// start / end point for the current polygon segment.
				float startX, startY, endX, endY;
				float2 endPoint = point3;
				endX = endPoint.x;
				endY = endPoint.y;
				while (checkIndex < polygonLength)
				{
					startX = endX;
					startY = endY;

					switch (checkIndex)
					{
						case 0:
							endPoint = point0;
							break;
						case 1:
							endPoint = point1;
							break;
						case 2:
							endPoint = point2;
							break;
						case 3:
							endPoint = point3;
							break;
					}

					endX = endPoint.x;
					endY = endPoint.y;
					//
					inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
					          && /* if so, test if it is under the segment */
					          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
					checkIndex += 1;
				}
				return inside;
			}

		}

	}

#endregion

	public static class SOConfig_ProjectileLayoutExtend
	{
		public static SOConfig_ProjectileLayout SpawnLayout(
			this SOConfig_ProjectileLayout config,
			I_RP_Projectile_ObjectCanReleaseProjectile caster,
			bool autoSetLayerMask = true,
			bool casterIgnore = true,
			params RolePlay_BaseBehaviour[] ignores)
		{
			return SubGameplayLogicManager_ARPG.Instance.ProjectileLayoutManagerReference
				.SpawnProjectileLayoutBySOConfig(config, caster, autoSetLayerMask, casterIgnore, ignores);
		}

		public static void StopLayout(this SOConfig_ProjectileLayout config)
		{
			config?.LayoutHandlerFunction?.StopLayout();
		}

		public static SOConfig_ProjectileLayout SpawnLayout_NoAutoStart(this SOConfig_ProjectileLayout config,
			I_RP_Projectile_ObjectCanReleaseProjectile caster,bool autoSetLayerMask = true, bool casterIgnore = true,params RolePlay_BaseBehaviour[] ignores)
		{
			return SubGameplayLogicManager_ARPG.Instance.ProjectileLayoutManagerReference
				.SpawnProjectileLayoutBySOConfig_NotAutoStart(config, caster, autoSetLayerMask, casterIgnore, ignores);
		}

	}
}