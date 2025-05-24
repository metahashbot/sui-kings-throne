using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Manager;
using GameplayEvent;
using Global;
using Global.ActionBus;
using RPGCore.AssistBusiness;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout.LayoutComponent;
using RPGCore.UtilityDataStructure;

using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
namespace RPGCore.Projectile.Layout
{
	/// <summary>
	/// <para>LayoutHandler的基类。需要对投射物的发射与运行过程有详细操作，则继承并实现</para>
	/// <para>如果不需要什么特殊操作，常规的内容直接使用基础版本也能实现</para>
	/// </summary>
	[Serializable]
	[TypeInfoBox("【配置】组内的数据是要在SO Asset时期就处理好的\n" + "【运行时】组内的数据在运行时会自动配好，编辑时它们就会是空的或者乱的")]
	public class BaseLayoutHandler
	
	{
		public SOConfig_ProjectileLayout RelatedProjectileLayoutConfigRef { get; private set; }

#region 静态部分

		protected static float _currentTime;
		protected static int _currentFrame;
		protected static ProjectileLayoutManager _projectileLayoutManagerRef;
		protected static ProjectileBehaviourManager _projectileBehaviourManagerRef;
		protected static GlobalActionBus _globalActionBusRef;
		protected static DamageAssistService _damageAssistServiceRef;
		public static void InitializeStatic(ProjectileLayoutManager plm, ProjectileBehaviourManager pbm)
		{
			_projectileLayoutManagerRef = plm;
			_projectileBehaviourManagerRef = pbm;
			_damageAssistServiceRef = SubGameplayLogicManager_ARPG.Instance.DamageAssistServiceInstance;
			_globalActionBusRef = GlobalActionBus.GetGlobalActionBus();
		}

#endregion




#region 内部字段

		[ShowInInspector, ReadOnly, LabelText("当前活跃吗？"), FoldoutGroup("运行时", true), NonSerialized]
		public bool SelfHandlerActive = false;
		
		[ShowInInspector,ReadOnly,LabelText("版面的ID，用于同一版面的多个运行时实例区分"),FoldoutGroup("运行时",true)]
		public int SelfHandlerID { get; private set; }
		
		
		/// <summary>
		/// <para>清理完成了吗？由于发起清理的来源可能有多个，所以避免重复清理</para>
		/// </summary>
		[ShowInInspector, LabelText("已清理？"), FoldoutGroup("运行时", true), NonSerialized]
		public bool SelfCleared = false;

		[ShowInInspector, ReadOnly, LabelText("从激活到现在的时间？"), FoldoutGroup("运行时", true), NonSerialized]
		protected float _elapsedTime;
		[ShowInInspector, ReadOnly, LabelText("当前已经开始运行了？"), FoldoutGroup("运行时", true), NonSerialized]
		public bool CurrentStarted = false;


		[ShowInInspector, ReadOnly, LabelText("内部生成过程结束了吗？"), FoldoutGroup("运行时", true), NonSerialized]
		public bool InternalSpawnProgressFinished = false;
		
		/// <summary>
		/// <para>事件线就是caster的事件线。直接使用更多的参数来区分不同的layout / projectileBehaviour</para>
		/// </summary>
		[ShowInInspector, ReadOnly]
		public LocalActionBus SelfActionBusReference { get; protected set; }

		[ShowInInspector, ReadOnly, LabelText("关联发射者"), FoldoutGroup("运行时", true),NonSerialized]
		protected I_RP_Projectile_ObjectCanReleaseProjectile CasterRef;


		public bool CheckIfSameCaster(I_RP_Projectile_ObjectCanReleaseProjectile caster)
		{
			if (caster == null || CasterRef == null)
			{
				return false;
			}
			return System.Object.ReferenceEquals(caster, CasterRef);
		}


		[ShowInInspector, ReadOnly, LabelText("判定忽略目标容器(I_RP_Projectile_ObjectCanReceiveProjectileCollision)"), FoldoutGroup("运行时/判定", true),NonSerialized]
		public List<I_RP_Projectile_ObjectCanReceiveProjectileCollision> IgnoreTargetList;

		[SerializeField, LabelText("开启Caster的数据缓存吗？"), FoldoutGroup("配置")]
		public bool EnableCasterDataEntryCache = true;
		
		[ShowInInspector,ReadOnly,LabelText("Caster的数据缓存"),FoldoutGroup("运行时"),NonSerialized]
		public Dictionary<RP_DataEntry_EnumType,float> CasterDataEntryCache;
		
		[ShowInInspector,ReadOnly,LabelText("Caster的缓存位置"),FoldoutGroup("运行时"),NonSerialized]
		public Vector3 _cache_CasterPosition;

		[ShowInInspector, ReadOnly, LabelText("Caster的缓存前朝向"), FoldoutGroup("运行时"), NonSerialized]
		public Vector3 _cache_CasterForwardDirection;
		
		
		
		/// <summary>
		/// <para>int1是Series索引，int2是Shoot索引</para>
		/// </summary>
		[ShowInInspector, ReadOnly, LabelText("SeriesDict"), FoldoutGroup("运行时/投射物", true)]
		protected Dictionary<(int, int), List<ProjectileBehaviour_Runtime>> selfRelatedProjectileBehaviourSeriesShootDict;


		public bool CheckIfProjectileIsFromThisLayoutHandler(ProjectileBehaviour_Runtime projectile)
		{
			foreach (var perSeries in selfRelatedProjectileBehaviourSeriesShootDict)
			{
				if (perSeries.Value.Contains(projectile))
				{
					return true;
				}
			}

			return false;
		}
		public Dictionary<(int, int), List<ProjectileBehaviour_Runtime>> GetAllSeriesDict()
		{
			return selfRelatedProjectileBehaviourSeriesShootDict;
		}

		protected struct WaitToRemoveInfo
		{
			public int Series;
			public int Shoots;
			public ProjectileBehaviour_Runtime BehaviourRef;
		}

		[ShowInInspector, LabelText("等待移除的投射物信息"), FoldoutGroup("运行时/投射物", true)]
		protected List<WaitToRemoveInfo> _waitToRemoveList;



		[ShowInInspector, ReadOnly, LabelText("下次Series的时间点"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public float NextSeriesTime;

		[ShowInInspector, ReadOnly, LabelText("下次Shoot的时间点"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public float NextShootsTime;


		[ShowInInspector, ReadOnly, LabelText("下次Spawn的时间点"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public float NextSpawnTime;

		[ShowInInspector, ReadOnly, LabelText("当前Series的Index"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public int CurrentSeriesIndex;


		[ShowInInspector, ReadOnly, LabelText("当前Shoots的Index"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public int CurrentShootsIndex;

		/// <summary>
		/// <para>用于处理  版面发射者 已经去世，但是滞留的投射物依然试图读取版面发射者身上的数据时的问题</para>
		/// <para>会在 激活时、每次发射时，都把计算得到的值放到这里，如果没有了就用缓存，还没去世就重算然后再放到这</para>
		/// </summary>
		[ShowInInspector, LabelText("来自上次计算的伤害值"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public float _cache_DamageCalculationValue;



		[ShowInInspector, LabelText("被显式覆写的起始生成位置"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public Nullable<Vector3> OverrideSpawnFromPosition;

		[ShowInInspector, LabelText("被显式覆写的起始生成方向"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public Nullable<Vector3> OverrideSpawnFromDirection;


		[ShowInInspector, LabelText("显式的伤害戳"), FoldoutGroup("运行时", true)]
		[NonSerialized]
		public int DamageTimeStamp;

		public class WaitToSpawnProjectileBehaviourInfo
		{
			public int SeriesIndex;
			public int ShootsIndex;
			public ProjectileBehaviour_Runtime ProjectileBehaviour;
			public float WillSpawnTime;
		}

		/// <summary>
		/// <para>等待生成的ProjectileBehaviour队列。所有的生成都在这里面。瞬发生成也在这里，判定生成的时间是大于等于，所以同帧也会生成</para>
		/// </summary>
		public List<WaitToSpawnProjectileBehaviourInfo> WaitToSpawnProjectileList { get; protected set; }
		
		[ShowInInspector,LabelText("该Handler被要求统一时间戳吗?"),FoldoutGroup("运行时")]
		[NonSerialized]
		public bool PresetNeedUniformTimeStamp = false;
		
		[ShowInInspector,LabelText("被统一的时间戳"),FoldoutGroup("运行时")]
		[NonSerialized]
		public int UniformTimeStamp = 0;

		[ShowInInspector, ReadOnly, LabelText("版面是否已经首次命中"), FoldoutGroup("运行时"), NonSerialized]
		public bool _layoutHasHitFirstTime;
#region cache

		private ILayoutComponent_RespondToSpawn _cacheLcRespondToSpawn;
		

#endregion

#endregion


		public virtual void AddTargetToIgnoreList(params I_RP_Projectile_ObjectCanReceiveProjectileCollision[] rolePlayBaseBehaviour)
		{
			if (IgnoreTargetList == null)
			{
				IgnoreTargetList = CollectionPool<List<I_RP_Projectile_ObjectCanReceiveProjectileCollision>,
					I_RP_Projectile_ObjectCanReceiveProjectileCollision>.Get();
			}
			foreach (I_RP_Projectile_ObjectCanReceiveProjectileCollision baseBehaviour in rolePlayBaseBehaviour)
			{
				IgnoreTargetList.Add(baseBehaviour);
			}
		}
		public virtual bool CheckTargetIgnored(I_RP_Projectile_ObjectCanReceiveProjectileCollision b)
		{
			if (IgnoreTargetList == null)
			{
				return false;
			}
			return IgnoreTargetList.Contains(b);
		}


		/// <summary>
		/// <para>初始化构建。在一个Layout的Start之前会进行。</para>
		/// <para>设置caster的引用。通常会带一个把它加到忽视列表的步骤</para>
		/// </summary>
		public virtual void InitializeOnInstantiate(SOConfig_ProjectileLayout layoutRef,
			I_RP_Projectile_ObjectCanReleaseProjectile caster, SOConfig_ProjectileLayout rawTemplate, float currentTime,
			int currentFrame, bool casterIgnore = true)
		{
			SelfHandlerID = this.GetHashCode() + Time.captureFramerate + caster.GetHashCode();
			PresetNeedUniformTimeStamp = false;
			UniformTimeStamp = 0;
			
			RelatedProjectileLayoutConfigRef = layoutRef;
			RelatedProjectileLayoutConfigRef.OriginalSOAssetTemplate = rawTemplate;
			DamageTimeStamp = 0;

			_elapsedTime = 0f;
			SelfCleared = false;
			InternalSpawnProgressFinished = false;
			CasterRef = caster;
			SelfActionBusReference = caster.GetRelatedActionBus();
			WaitToSpawnProjectileList =
				CollectionPool<List<WaitToSpawnProjectileBehaviourInfo>, WaitToSpawnProjectileBehaviourInfo>.Get();
			_waitToRemoveList = CollectionPool<List<WaitToRemoveInfo>, WaitToRemoveInfo>.Get();

			IgnoreTargetList = CollectionPool<List<I_RP_Projectile_ObjectCanReceiveProjectileCollision>,
				I_RP_Projectile_ObjectCanReceiveProjectileCollision>.Get();
			
			
			if (casterIgnore && CasterRef is I_RP_Projectile_ObjectCanReceiveProjectileCollision casterRP)
			{
				AddTargetToIgnoreList(casterRP);
			}


			selfRelatedProjectileBehaviourSeriesShootDict =
				CollectionPool<Dictionary<(int, int), List<ProjectileBehaviour_Runtime>>,
					KeyValuePair<(int, int), List<ProjectileBehaviour_Runtime>>>.Get();

			foreach (BaseProjectileLayoutComponent perComponent in RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList)
			{
				
				perComponent.InitializeBeforeStart(RelatedProjectileLayoutConfigRef,currentTime,currentFrame);
			}
			
			_GetInternalCache();
			

			SelfActionBusReference?.TriggerActionByType(
				ActionBus_ActionTypeEnum.L_PHandler_OnLayoutHandlerInitialized_当布局Handler被初始化,
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PHandler_OnLayoutHandlerInitialized_当布局Handler被初始化,
					this));

			SelfActionBusReference?.RegisterAction(ActionBus_ActionTypeEnum.L_PBR_ProjectileLifetimeEnd_一个投射物生命周期结束,
				_ABC_RecycleDestroyedProjectile_OnProjectileLifetimeEnd);


			SelfActionBusReference?.RegisterAction(
				ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
				_ABC_CheckIfStopSpawnOperationAndCacheCasterData_OnCasterDataInvalid);

			_cache_DamageCalculationValue = 0f;

			if (RelatedProjectileLayoutConfigRef.LayoutContentInSO.DamageApplyInfo != null)
			{
				var dai = RelatedProjectileLayoutConfigRef.LayoutContentInSO.DamageApplyInfo;
				if (!dai.DamageTakenRelatedDataEntry)
				{
					_cache_DamageCalculationValue = dai.DamageTryTakenBase;
				}
				else
				{
					foreach (ConSer_DataEntryRelationConfig perEntry in dai.RelatedDataEntryInfos)
					{
						//关联释放者
						if (!perEntry.RelatedToReceiver)
						{
							Float_RPDataEntry targetEntry = (caster as I_RP_Damage_ObjectCanApplyDamage)
								.ApplyDamage_GetRelatedDataEntry(perEntry.RelatedDataEntryType);

							perEntry.CacheDataEntryValue = targetEntry.GetCurrentValue() * perEntry.Partial;
						}
					}
				}
			}

			if (EnableCasterDataEntryCache)
			{
				_cache_CasterPosition = caster.GetCasterPosition();
				_cache_CasterForwardDirection = CasterRef.GetCasterForwardDirection();
				CasterDataEntryCache =
					CollectionPool<Dictionary<RP_DataEntry_EnumType, float>, KeyValuePair<RP_DataEntry_EnumType, float>>
						.Get();
				CasterDataEntryCache.Clear();
				caster.FillDataCache(CasterDataEntryCache);
				caster.GetRelatedActionBus()
					.RegisterAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
						_ABC_RefreshDataEntryCache,
						9999);

			}
		}


		private void _ABC_RefreshDataEntryCache(DS_ActionBusArguGroup ds)
		{
			var entry = ds.ObjectArgu1 as Float_RPDataEntry;
			if (entry != null)
			{
				CasterDataEntryCache[entry.RP_DataEntryType] = ds.FloatArgu1.Value;
			}
		}

		protected virtual void _GetInternalCache()
		{
			foreach (BaseProjectileLayoutComponent perComponent in RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList)
			{
				if (perComponent is ILayoutComponent_RespondToSpawn respondToSpawn)
				{
					_cacheLcRespondToSpawn = respondToSpawn;
					return;
				}
			}
		}

#region Spawn——生成相关


		/// <summary>
		/// <para>标记生成过程已结束，可以进行是否清理的判定了。</para>
		/// <para>这是自行清理的那种，前提条件时生成过程已经完成了</para>
		/// </summary>
		protected virtual  void SpawnOperationEnd()
		{
			InternalSpawnProgressFinished = true;
		}
		
		

		


		/// <summary>
		/// <para>一个来自能够Spawn的Component发起的 Spawn调用</para>
		/// <para>是每个Shoots发起的</para>
		/// <para>需要传入发起者自身</para>
		/// <para>然后会遍历内部所有具有 Spawner 接口实现的对象，然后将生成信息传递过去，等待处理后再传回</para>
		/// </summary>
		public virtual void RequireSpawnOperation(int seriesIndex,
			int shootsIndex)
		{
			LayoutSpawnInfo_OneSpawn oneSpawnInfo = GenericPool<LayoutSpawnInfo_OneSpawn>.Get();
			oneSpawnInfo.CurrentSeriesIndex = seriesIndex;
			oneSpawnInfo.CurrentShootsIndex = shootsIndex;
			HandlerRespondToSpawnOperation(oneSpawnInfo, WaitToSpawnProjectileList, seriesIndex, shootsIndex);
			
			//在此向LocalActionBus发出生成事件
			var dsSpawn = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PLC_Spawn_OnOneSpawnOperationAddToList_当一次生成活动已经将数据加入了列表);
			dsSpawn.IntArgu1 = seriesIndex;
			dsSpawn.IntArgu2 = shootsIndex;
			dsSpawn.ObjectArgu1 = WaitToSpawnProjectileList;
			dsSpawn.ObjectArgu2 = this;
			SelfActionBusReference.TriggerActionByType(
				dsSpawn);


			GenericPool<LayoutSpawnInfo_OneSpawn>.Release(oneSpawnInfo);
		}


		/// <summary>
		/// <para>获取指定Series和Shoots索引下的那个ProjectileBehaviour的容器</para>
		/// </summary>
		public virtual List<ProjectileBehaviour_Runtime> GetProjectileBehaviourCollectionAtSeriesAndShoots(
			int seriesIndex, int shootsIndex)
		{
			(int, int) pair = (seriesIndex, shootsIndex);
			if (selfRelatedProjectileBehaviourSeriesShootDict.TryGetValue(pair,
				out List<ProjectileBehaviour_Runtime> shoots))
			{
				return shoots;
			}
			else
			{
				List<ProjectileBehaviour_Runtime> newList =
					CollectionPool<List<ProjectileBehaviour_Runtime>, ProjectileBehaviour_Runtime>.Get();
				selfRelatedProjectileBehaviourSeriesShootDict.Add(pair, newList);
				newList.Clear();
				return newList;
			}
		}


		/// <summary>
		/// <para>Handler默认实现的对于生成操作的相应，默认即找到 LC_RespondToSpawn调用</para>
		/// </summary>
		/// <returns></returns>
		public virtual void HandlerRespondToSpawnOperation(LayoutSpawnInfo_OneSpawn oneSpawnInfo,
			List<WaitToSpawnProjectileBehaviourInfo> collection, int seriesIndex, int shootsIndex)
		{
			_cacheLcRespondToSpawn.RespondToSpawnOperation(oneSpawnInfo,
				this,
				collection,
				_currentTime,
				_currentFrame,
				seriesIndex,
				shootsIndex);
			if(PresetNeedUniformTimeStamp)
			{
				foreach (WaitToSpawnProjectileBehaviourInfo perWait in collection)
				{
					perWait.ProjectileBehaviour.OverrideDamageStamp = UniformTimeStamp;
				}
			}
		}


#endregion


		public virtual void StartLayout()
		{
//			DBug.Log($"版面{RelatedProjectileLayoutConfigRef.name}启动了");
//			if (CurrentStarted)
//			{
//#if UNITY_EDITOR
//				DBug.LogWarning(this + "已经启动，但又要求启动了，这不合理");
//#endif
//				return;
//			}
			CurrentStarted = true;
			SelfHandlerActive = true;
			CurrentShootsIndex = 0;
			CurrentSeriesIndex = 0;
			//Layout开始时，需要计算第一次生成的时间点
			//如果有Series，则加上Series的StartDelay
			//如果还有Shoots，则加上Shoots的StartDelay

			float startDelay = RelatedProjectileLayoutConfigRef.LayoutContentInSO.OverallStartDelay;
			NextSpawnTime = BaseGameReferenceService.CurrentFixedTime + startDelay;
			foreach (var perComponent in RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList)
			{
				perComponent.StartLayoutComponent(BaseGameReferenceService.CurrentFixedTime,
					BaseGameReferenceService.CurrentFixedFrame);

			}
			
		}

#region TICK

		public void FixedUpdateTick(float currentTime, int currentFrame, float delta,
			List<CollisionCheckInfo_ProjectileFull> collisionCheck_BaseProjectileInfo)
		{
			//当casterRef不为空时，则表示Caster的DataValid有效，则此时缓存那些数据
			if (EnableCasterDataEntryCache && CasterRef != null)
			{
				_cache_CasterPosition = CasterRef.GetCasterPosition();
				_cache_CasterForwardDirection = CasterRef.GetCasterForwardDirection();



			}
			_currentTime = currentTime;
			_currentFrame = currentFrame;
			_elapsedTime += delta;

			if (NextSpawnTime < 0f)
			{
				return;
			}
			if (!InternalSpawnProgressFinished && currentTime > NextSpawnTime && SelfHandlerActive)
			{
				//进行一个Spawn

				RequireSpawnOperation(
					CurrentSeriesIndex,
					CurrentShootsIndex);

				var ds_shootsIncrease =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PLC_Spawn_LayoutShootsIncrease_LayoutShoots增加);
				ds_shootsIncrease.ObjectArgu1 = this;
				SelfActionBusReference.TriggerActionByType(ds_shootsIncrease);
				
				CurrentShootsIndex += 1;
				//如果当前Shoots的Index已经和Shoots数量相同了，则进入下一个Series
				if (CurrentShootsIndex == Mathf.RoundToInt(RelatedProjectileLayoutConfigRef.LayoutContentInSO.ShootsCountPerSeries))
				{
					CurrentSeriesIndex += 1;
					var ds_series =
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.L_PLC_Spawn_LayoutSeriesIncrease_LayoutSeries增加);
					ds_series.ObjectArgu1 = this;
					SelfActionBusReference.TriggerActionByType(ds_series);

					var ds_shootsReturn =
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PLC_SpawnLayoutShootsReset_LayoutShoots归0);
					ds_shootsReturn.ObjectArgu1 = this;
					 SelfActionBusReference.TriggerActionByType(ds_shootsReturn);
					CurrentShootsIndex = 0;
				}

				//当前Series索引已经和Series数量相同了，则不再进行下一次生成，并通知这次生成已经完成了，将这个Layout标记为等待清理
				if (CurrentSeriesIndex == Mathf.RoundToInt(RelatedProjectileLayoutConfigRef.LayoutContentInSO.SeriesCount))
				{
					SpawnOperationEnd();
					return;

				}

				//会计算下一次生成时间，根据当前的Series Index和 Shoots Index
				float nextInterval = 0f;

				//如果当前的ShootsIndex是0，则表明这次是进入了一个新的Series，则此时的间隔是Series的间隔
				if (CurrentShootsIndex == 0)
				{
					nextInterval += RelatedProjectileLayoutConfigRef.LayoutContentInSO.SeriesInterval + 
					                CurrentSeriesIndex * RelatedProjectileLayoutConfigRef.LayoutContentInSO.SeriesIntervalOffset;
				}
				//如果不是，则是Shoots的间隔
				else
				{
					nextInterval += RelatedProjectileLayoutConfigRef.LayoutContentInSO.BaseShootIntervalInsideSeries +
					                CurrentShootsIndex * RelatedProjectileLayoutConfigRef.LayoutContentInSO.ShootsIntervalOffset;
				}



				NextSpawnTime = currentTime + nextInterval;

			}
			foreach (var component in RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList)
			{
				component.FixedUpdateTick(currentTime, currentFrame, delta);
			}

			//
			WaitToSpawnListTick(currentTime, currentFrame, delta);
			
			
			
			
			//然后是对于自己关联的所有ProjectileBehaviour的Tick
			foreach (KeyValuePair<(int, int),List<ProjectileBehaviour_Runtime>> kvp in selfRelatedProjectileBehaviourSeriesShootDict)
			{
				foreach (ProjectileBehaviour_Runtime perBehaviour in kvp.Value)
				{
					perBehaviour.FixedUpdateTick(currentTime, currentFrame, delta);
				}
			}
			
			
			
			
			//从NeedToRemove容器中逐个移除
			foreach (WaitToRemoveInfo remove in _waitToRemoveList)
			{
				//如果是自己的投射物，那么就要回收
				selfRelatedProjectileBehaviourSeriesShootDict[(remove.Series,
					remove.Shoots)].Remove(remove.BehaviourRef);
				//通知PBM进行进一步的回收处理
				_projectileBehaviourManagerRef.ReturnProjectileBehaviourRuntime(remove.BehaviourRef);	
			}
			_waitToRemoveList.Clear();

			
			
			//将参与计算的投射物的碰撞信息添加到列表中
			foreach (List<ProjectileBehaviour_Runtime> perList in selfRelatedProjectileBehaviourSeriesShootDict.Values)
			{
				foreach (ProjectileBehaviour_Runtime perBehaviour in perList)
				{
					if (perBehaviour.SelfActive && perBehaviour.CurrentCollisionActive)
					{

						collisionCheck_BaseProjectileInfo.Add(new CollisionCheckInfo_ProjectileFull
						{
							ProjectileBehaviourRef = perBehaviour,
							ColliderCircleRadius = perBehaviour.ProjectileColliderRadius
						});
					}
				}
			}



		}

		public void UpdateTick(float ct, int cf, float delta)
		{

			//然后是对于自己关联的所有ProjectileBehaviour的Tick
			foreach (KeyValuePair<(int, int), List<ProjectileBehaviour_Runtime>> kvp in
				selfRelatedProjectileBehaviourSeriesShootDict)
			{
				foreach (ProjectileBehaviour_Runtime perBehaviour in kvp.Value)
				{
					perBehaviour.UpdateTick(ct, cf, delta);
				}
			}
		}


		/// <summary>
		/// <para>Tick等待生成的列表</para>
		/// </summary>
		protected virtual void WaitToSpawnListTick(float currentTime, int cf, float delta)
		{
			if (!SelfHandlerActive)
			{
				return;
			}
			for (int i = WaitToSpawnProjectileList.Count - 1; i >= 0; i--)
			{
				if (currentTime >= WaitToSpawnProjectileList[i].WillSpawnTime)
				{
					WaitToSpawnProjectileBehaviourInfo tmpInfo = WaitToSpawnProjectileList[i];
					
					
					List<ProjectileBehaviour_Runtime> targetList =
						GetProjectileBehaviourCollectionAtSeriesAndShoots(tmpInfo.SeriesIndex, tmpInfo.ShootsIndex);
					targetList.Add(tmpInfo.ProjectileBehaviour);
					ActivateProjectileBehaviour(WaitToSpawnProjectileList[i].ProjectileBehaviour);
					GenericPool<WaitToSpawnProjectileBehaviourInfo>.Release(tmpInfo);
					WaitToSpawnProjectileList.RemoveAt(i);
				}
			}
		}


		protected List<ProjectileBehaviour_Runtime> GetRelatedIndexBehaviourList(int series, int shoots)
		{
			(int, int) key = (series, shoots);
			if (selfRelatedProjectileBehaviourSeriesShootDict.ContainsKey(key))
			{
				return
				selfRelatedProjectileBehaviourSeriesShootDict[key];
			}
			else
			{
				var newList =
					CollectionPool<List<ProjectileBehaviour_Runtime>, ProjectileBehaviour_Runtime>.Get();
				newList.Clear();
				selfRelatedProjectileBehaviourSeriesShootDict.Add(key, newList);
				return newList;
			}
		}

		/// <summary>
		/// <para>激活指定的投射物行为，</para>
		/// </summary>
		protected virtual void ActivateProjectileBehaviour(ProjectileBehaviour_Runtime behaviour)
		{
			behaviour.ActivateThisProjectile(_currentTime);
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(
					ActionBus_ActionTypeEnum.L_PHandler_OnProjectileBehaviourActivated_当某个投射物行为被激活);
			ds.ObjectArgu1 = behaviour;
			ds.ObjectArgu2 = this;
			SelfActionBusReference?.TriggerActionByType(ActionBus_ActionTypeEnum
					.L_PHandler_OnProjectileBehaviourActivated_当某个投射物行为被激活,
				ds);
		}

#endregion




		public ProjectileBehaviour_Runtime GetLayoutRelatedAvailableProjectile(string specific = null)
		{
			ProjectileBehaviour_Runtime pb = null;
			if (string.IsNullOrEmpty(specific))
			{
				pb = _projectileBehaviourManagerRef.GetAvailableProjectileByUID(GetSelfProjectileUID());
			}
			else
			{
				pb = _projectileBehaviourManagerRef.GetAvailableProjectileByUID(specific);
			}
			pb.InitializeOnGet(RelatedProjectileLayoutConfigRef, CasterRef, SelfActionBusReference);
			DS_ActionBusArguGroup ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PHandler_Spawn_GetLayoutAvailableProjectile_获取了一个可用的投射物);
			ds.ObjectArgu1 = pb;
			ds.ObjectArgu2 = this;
			SelfActionBusReference?.TriggerActionByType(ActionBus_ActionTypeEnum
					.L_PHandler_Spawn_GetLayoutAvailableProjectile_获取了一个可用的投射物,
				ds);
			return pb;
		}

		/// <summary>
		/// <para>从SOConfig的ContentInSO配置部分取出可以使用的具体投射物类型。可能会有随机要素</para>
		/// </summary>
		public string GetSelfProjectileUID()
		{
			string displayName = "";
			if (RelatedProjectileLayoutConfigRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Count == 1)
			{
				displayName =  RelatedProjectileLayoutConfigRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList[0];
			}
			else
			{
				displayName = RelatedProjectileLayoutConfigRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList[
					UnityEngine.Random.Range(0,
						RelatedProjectileLayoutConfigRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Count - 1)];
			}
			return GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_ProjectileLoad
				.GetLoadContentByDisplayName(displayName).ProjectileNameUID;
		}

#region 碰撞相关

		

		/// <summary>
		/// <para>由PBM在统一计算流程中， 向各个发生了碰撞信息的ProjectileBehaviour的Layout发起这个调用</para>
		/// <para>包含了：向本地事件广播 L_PHandler_OnProjectileCollideWithRPBehaviour_一个投射物与RPBehaviour碰撞</para>
		/// <para>      GetProjectileApplyDamageInfo</para>
		/// <para>		GetTargetDamageApplyInfo</para>
		/// <para>		ApplyDamageViaDamageAssist</para>
		/// </summary>
		public virtual void ReceiveCollisionInfo(ProjectileBehaviour_Runtime projectile, BaseARPGCharacterBehaviour rpBehaviour,float2 collisionPos)
		{

			DS_ActionBusArguGroup ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PHandler_OnProjectileCollideWithRPBehaviour_一个投射物与RPBehaviour碰撞);
			ds.ObjectArgu1 = projectile;
			ds.ObjectArgu2 = rpBehaviour;
			ds.FloatArgu1 = collisionPos.x;
			ds.FloatArgu2 = collisionPos.y;
			SelfActionBusReference?.TriggerActionByType(ActionBus_ActionTypeEnum
					.L_PHandler_OnProjectileCollideWithRPBehaviour_一个投射物与RPBehaviour碰撞,
				ds);
			RP_DS_DamageApplyResult damageApplyResult = null;
			ConSer_DamageApplyInfo damageInfoInConfig = GetProjectileApplyDamageInfo(projectile);
			if (damageInfoInConfig != null)
			{
				RP_DS_DamageApplyInfo damageApplyInfoRuntime = GetTargetDamageApplyInfo(damageInfoInConfig,
					projectile.SelfCaster as I_RP_Damage_ObjectCanApplyDamage,
					rpBehaviour,projectile);
				
				
				// damageApplyInfoRuntime.RelatedLayoutUID = projectile.RelatedLayoutUID;
				Vector3 collidePos = new Vector3(collisionPos.x, 0f, collisionPos.y);
				Vector3 receiverPos = rpBehaviour.GetCollisionCenter();
				damageApplyInfoRuntime.DamageWorldPosition = new Vector3(collidePos.x, receiverPos.y, collidePos.z);
				receiverPos.y = 0f;
				damageApplyInfoRuntime.DamageDirectionV3 = (receiverPos - collidePos).normalized;
				damageApplyInfoRuntime.RelatedProjectileBehaviourRuntime = projectile;
				
				var projectileDirection = projectile.DesiredMoveDirection;
				projectileDirection.y = 0f;
				float angle = Vector3.SignedAngle(damageApplyInfoRuntime.DamageDirectionV3.Value,
					projectileDirection,
					Vector3.up);
				// Use any axis that is not parallel to a or b
				Vector3 bisectorDirection = Quaternion.AngleAxis(angle / 2f, Vector3.up) *
				                            damageApplyInfoRuntime.DamageDirectionV3
					                            .Value; // Rotate a by half of the angle
				damageApplyInfoRuntime.ForceDirectionV3 = bisectorDirection.normalized;

				
				/*
				 * 检查时间戳
				 */
				if (projectile.OverrideDamageStamp != null)
				{
					damageApplyInfoRuntime.DamageTimestamp = projectile.OverrideDamageStamp.Value;
				}
				else
				{
					if (DamageTimeStamp != 0)
					{
						damageApplyInfoRuntime.DamageTimestamp = DamageTimeStamp;
					}
					else
					{
						damageApplyInfoRuntime.DamageTimestamp = 0;
					}
				}
				
				damageApplyResult = ApplyDamageViaDamageAssist(damageApplyInfoRuntime);
				
				
			}

			//然后检查是否需要销毁，有的只是掉血而不销毁，
			ProcessProjectileCollided(projectile , rpBehaviour, damageApplyResult);



		}


		protected virtual void ProcessProjectileCollided(ProjectileBehaviour_Runtime projectileBehaviour,
			I_RP_Projectile_ObjectCanReceiveProjectileCollision behaviour, RP_DS_DamageApplyResult result)
		{
			//TODO：如果有穿透之类的就不销毁
			
			
			/*
			 * 检查是否有碰撞时触发的游戏性事件
			 */

			if (projectileBehaviour.SelfSpawnGameplayEventOnHitComponent != null && projectileBehaviour.SelfSpawnGameplayEventOnHitComponent.GameplayEventNameList!=null)
			{
				var ds_ge = new DS_GameplayEventArguGroup();
				ds_ge.ObjectArgu1 = this;
				foreach (string perEventUID in projectileBehaviour.SelfSpawnGameplayEventOnHitComponent
					.GameplayEventNameList)
				{
					GameplayEventManager.Instance.StartGameplayEvent(perEventUID, ds_ge);
				}
			}

			if (!_layoutHasHitFirstTime && result.ResultLogicalType == RP_DamageResultLogicalType.NormalResult)
			{
				_layoutHasHitFirstTime = true;
				var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_PLayout_LayoutFirstValidHit_当投射物Layout首次有效命中);
				ds.ObjectArgu1 = this;
				ds.ObjectArgu2 = result;
				SelfActionBusReference.TriggerActionByType(ds);

			}


			//检查是否有穿透
			if (projectileBehaviour.SelfPenetrationComponent != null)
			{
				projectileBehaviour.SelfPenetrationComponent.AddPenetrateInfo(behaviour,
					BaseGameReferenceService.CurrentFixedFrame, projectileBehaviour.OverrideDamageStamp ?? 0);
				projectileBehaviour.SelfPenetrationComponent.CurrentRemainingPenetrationAmount -=
					projectileBehaviour.SelfPenetrationComponent.PerPenetrationReduce;
				
				if (projectileBehaviour.SelfPenetrationComponent.CurrentRemainingPenetrationAmount < 0f)
				{
					AddToRemoveAndRelease();
				}
			}
			else
			{
				//如果是闪避了，就把命中到的东西加入到忽略列表中
				if (result.ResultLogicalType == RP_DamageResultLogicalType.DodgedSoNothing)
				{
					projectileBehaviour.AddTargetToIgnoreList(behaviour);
				}
				else
				{
					AddToRemoveAndRelease();

				}
			}


			void AddToRemoveAndRelease()
			{

				AddWaitToRemove(projectileBehaviour);
				result.ReleaseToPool();
			}
		}


		protected virtual ConSer_DamageApplyInfo GetProjectileApplyDamageInfo(ProjectileBehaviour_Runtime projectile)
		{
			if (projectile.RelatedSeriesIndex == -1)
			{
				var t = RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList.FindIndex((component =>
					component is LC_追加投射物于响应_AppendProjectileOnAction));
				if (t == -1)
				{
					DBug.LogError(
						$"当投射物Series索引为-1时，通常表明这是来自AppendProjectileOnAction_追加投射物于响应的生成，但是此次查找并没有在关联的投射物配置{projectile.SelfLayoutConfigReference.name}中查找到这个LC，这不合理");
					return null;
				}
				else
				{
					return (RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList[t] as
						LC_追加投射物于响应_AppendProjectileOnAction).DAI;
				}
			}
			else if (RelatedProjectileLayoutConfigRef.LayoutContentInSO.DamageApplyInfo != null)
			{
				return RelatedProjectileLayoutConfigRef.LayoutContentInSO.DamageApplyInfo;
			}
			return null;
			
		}

		protected virtual RP_DS_DamageApplyInfo GetTargetDamageApplyInfo(
			ConSer_DamageApplyInfo damageApplyInfo, I_RP_Damage_ObjectCanApplyDamage caster,
			I_RP_Damage_ObjectCanReceiveDamage receiver , ProjectileBehaviour_Runtime pr)
		{


			RP_DS_DamageApplyInfo newDAI = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromFromConSer(damageApplyInfo,
				receiver,
				caster,pr);

			if (CasterRef != null)
			{
				var ds_onDamageApplyInfoBuild = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_PLayout_Damage_OnBuildRuntimeDamageApplyInfo_当来自投射物Layout的运行时伤害信息被构建);
				ds_onDamageApplyInfoBuild.ObjectArgu1 = this;
				ds_onDamageApplyInfoBuild.ObjectArgu2 = newDAI;
				CasterRef.GetRelatedActionBus().TriggerActionByType(ds_onDamageApplyInfoBuild);
			}
			return newDAI;
		}


		protected virtual RP_DS_DamageApplyResult ApplyDamageViaDamageAssist(RP_DS_DamageApplyInfo applyInfo)
		{
			return _damageAssistServiceRef.ApplyDamage(applyInfo);
		}

		
		
		
#endregion

		/// <summary>
		/// <para>显式地要求停止这个layout的Spawn活动，将Active置false</para>
		/// <para>但与CheckIfNeedClear不关联</para>
		/// </summary>
		public void StopLayout()
		{
			SelfHandlerActive = false;
			WaitToSpawnProjectileList.Clear();
			InternalSpawnProgressFinished = true;
		}


		/// <summary>
		/// <para>显式地要求清理已经存在的所有的投射物</para>
		/// </summary>
		public void ClearLayout(bool stop = false)
		{
			if (stop)
			{
				StopLayout();
			}
			foreach (var kvp in selfRelatedProjectileBehaviourSeriesShootDict)
			{
				for (int i = kvp.Value.Count - 1; i >= 0; i--)

				{
					_waitToRemoveList.Add(new WaitToRemoveInfo
					{
						Series = kvp.Key.Item1,
						Shoots = kvp.Key.Item2,
						BehaviourRef = kvp.Value[i]
					});
					kvp.Value.RemoveAt(i);
				}
			}	
			
		}


		/// <summary>
		/// <para>检查是否需要清理这个Layout了</para>
		/// <para>显式地活跃（通常表明Spawn过程还没有完全结束）</para>
		/// <para>遍历自身关联的ProjectileRuntime数量不为0，表示还有活跃着的关联Projectile</para>
		/// </summary>
		public bool CheckIfNeedClear()
		{
			if (!InternalSpawnProgressFinished)
			{
				return false;
			}
			else
			{
				if (WaitToSpawnProjectileList.Count > 0)
				{
					return false;
				}
				foreach (var perList in selfRelatedProjectileBehaviourSeriesShootDict.Values)
				{
					if (perList.Count > 0)
					{
						return false;
					}
				}
			}

			return true;
		}

#region 全局监听


		/// <summary>
		/// <para>当caster的数据不再有效的时候，(通常是因为死了或者由场景机制要求强制退场了)此时角色还会短暂地存在于场景中</para>
		/// <para>这时候(默认)不再相应版面的生成，并且(默认)将现存投射物清理</para>
		/// <para>并且缓存caster数据，用于还需要继续生成的投射物的操作</para>
		/// </summary>
		protected virtual void _ABC_CheckIfStopSpawnOperationAndCacheCasterData_OnCasterDataInvalid(
			DS_ActionBusArguGroup ds)
		{
			//如果没有 要求 [停止后继续响应生成]的组件，则不停止生成的响应
			if (RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList.Exists((component =>
				component is LC_数据无效后依然响应生成操作_HoldSpawnOperationAfterDataInvalid)))
			{
				
			}
			else
			{
				StopLayout();
			}

			if (RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutComponentList.Exists((component =>
				component is LC_数据无效后不清除已存在投射物_HoldProjectileAfterDataInvalid)))
			{
				
			}
			else
			{
				ClearLayout();
			}

			
			
			//进行caster数据缓存
			CasterRef = null;

			
			
			
			
		}

		

#endregion


#region 来自子 Layout Component

		public Vector3 GetCasterPosition()
		{
			if (CasterRef != null)
			{
				return CasterRef.GetCasterPosition();
			}
			else
			{
				return _cache_CasterPosition;
			}
		}

		public Vector3 GetCasterForwardDirection()
		{

			if (CasterRef != null)
			{
				return CasterRef.GetCasterForwardDirection();
			}
			else
			{
				return _cache_CasterForwardDirection;
			}
		}
		

		public void RegisterBaseSpawnDoneFromIRespondToSpawn(UnityAction<DS_ActionBusArguGroup> call)
		{
			CasterRef.GetRelatedActionBus().RegisterAction(ActionBus_ActionTypeEnum
					.L_PLC_Spawn_OnProjectileRespondToSpawnBaseSetDone_一个在响应生成时的原始投射物基础设置完成,
				call);
		}

		public void UnregisterBaseSpawnDownFromIRespondToSpawn(UnityAction<DS_ActionBusArguGroup> call)
		{
			CasterRef.GetRelatedActionBus().RemoveAction(ActionBus_ActionTypeEnum
					.L_PLC_Spawn_OnProjectileRespondToSpawnBaseSetDone_一个在响应生成时的原始投射物基础设置完成,
				call);
		}


#endregion

		
		
		protected virtual void _ABC_RecycleDestroyedProjectile_OnProjectileLifetimeEnd(DS_ActionBusArguGroup ds)
		{
			var targetProjectile = ds.ObjectArgu1 as ProjectileBehaviour_Runtime;
			if(targetProjectile.SelfLayoutConfigReference != RelatedProjectileLayoutConfigRef)
			{
				return;
			}

			AddWaitToRemove(targetProjectile);

		}

		/// <summary>
		/// <para>将投射物添加到 [等待移除]的容器中去，会在FixedUpdateTick的末尾将它们真的移除</para>
		/// </summary>
		public void AddWaitToRemove(ProjectileBehaviour_Runtime projectile)
		{
			WaitToRemoveInfo newInfo = new WaitToRemoveInfo
			{
				Series = projectile.RelatedSeriesIndex,
				Shoots = projectile.RelatedShootsIndex,
				BehaviourRef = projectile
			};
			if(_waitToRemoveList.Exists((info => info.BehaviourRef.Equals(projectile))))
			{
				return;
			}
			else
			{
				_waitToRemoveList.Add(newInfo);
			}
		}
		
		/// <summary>
		/// <para>显式地要求清除内容并卸载，包括把池中的内容都退回，各容器置空</para>
		/// <para>由ProjectileLayoutManager调用至此，下一行就是Destroy这个SO Instance了，</para>
		/// </summary>
		public void ClearAndUnload()
		{
			if (SelfCleared)
			{
				return;
			}
			else
			{
				SelfCleared = true;
			}
#if UNITY_EDITOR
			//DBug.Log($"Layout要求卸载：{RelatedProjectileLayoutConfigRef.name},归属于{CasterRef}");
#endif

			var layoutDestroy =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PLayout_LayoutWillDestroy_当投射物Layout将要销毁);
			layoutDestroy.ObjectArgu1 = this;
			SelfActionBusReference?.TriggerActionByType(layoutDestroy);
			
			foreach (List<ProjectileBehaviour_Runtime> perSeries in
				selfRelatedProjectileBehaviourSeriesShootDict.Values)
			{
				foreach (var perBehaviour in perSeries)
				{
					perBehaviour.UnregisterCallback();
					;
				}
			}


			if (EnableCasterDataEntryCache)
			{
				if (CasterRef != null)
				{
					CasterRef.GetRelatedActionBus()
						.RemoveAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
							_ABC_RefreshDataEntryCache);
				}
				CasterDataEntryCache.Clear();
				CollectionPool<Dictionary<RP_DataEntry_EnumType, float>, KeyValuePair<RP_DataEntry_EnumType, float>>
					.Release(CasterDataEntryCache);
			}

			SelfActionBusReference?.RemoveAction(ActionBus_ActionTypeEnum.L_PBR_ProjectileLifetimeEnd_一个投射物生命周期结束,
				_ABC_RecycleDestroyedProjectile_OnProjectileLifetimeEnd);
		
			SelfActionBusReference?.RemoveAction(
				ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效,
				_ABC_CheckIfStopSpawnOperationAndCacheCasterData_OnCasterDataInvalid);

			WaitToSpawnProjectileList.Clear();
			CollectionPool<List<WaitToSpawnProjectileBehaviourInfo>, WaitToSpawnProjectileBehaviourInfo>.Release(
				WaitToSpawnProjectileList);

			foreach (BaseProjectileLayoutComponent baseProjectileLayoutComponent in RelatedProjectileLayoutConfigRef
				.LayoutContentInSO.LayoutComponentList)
			{
				baseProjectileLayoutComponent.ClearAndUnload(RelatedProjectileLayoutConfigRef);
			}
			foreach (var perList in selfRelatedProjectileBehaviourSeriesShootDict.Values)
			{
				CollectionPool<List<ProjectileBehaviour_Runtime>, ProjectileBehaviour_Runtime>.Release(perList);
			}
			selfRelatedProjectileBehaviourSeriesShootDict.Clear();
			CollectionPool<Dictionary<(int, int), List<ProjectileBehaviour_Runtime>>,
					KeyValuePair<(int, int), List<ProjectileBehaviour_Runtime>>>
				.Release(selfRelatedProjectileBehaviourSeriesShootDict);


			_waitToRemoveList.Clear();
			CollectionPool<List<WaitToRemoveInfo>, WaitToRemoveInfo>.Release(_waitToRemoveList);

			IgnoreTargetList.Clear();
			CollectionPool<List<I_RP_Projectile_ObjectCanReceiveProjectileCollision>, I_RP_Projectile_ObjectCanReceiveProjectileCollision>.Release(IgnoreTargetList);
			
			CurrentStarted = false;
		}
	}
}