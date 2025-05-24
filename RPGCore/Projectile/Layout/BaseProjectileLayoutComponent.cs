using System;
using System.Collections.Generic;
using ARPG.Manager;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.Projectile.Layout.LayoutComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
namespace RPGCore.Projectile.Layout
{
	
	/// <summary>
	/// <para>一个基本的投射物布局组件。</para>
	/// </summary>
	[Serializable]
	public abstract class BaseProjectileLayoutComponent
	{
		protected static ProjectileBehaviourManager ProjectileBehaviourManagerReference;
		protected static SubGameplayLogicManager_ARPG GLMRef;
		public static void StaticInitializeByPBM(ProjectileBehaviourManager pbm ,SubGameplayLogicManager_ARPG glm)
		{
			ProjectileBehaviourManagerReference = pbm;
			GLMRef = glm;
		}
		
		[LabelText("组件有效？")]
		public bool ComponentValid = true;

		[LabelText("组件总是有效？总是有效则不计时，否则持续时间结束后将不再有效")]
		public bool AlwaysValid = true;
		[LabelText("持续时间"), HideIf(nameof(AlwaysValid))]
		public float RemainingValidDuration;

		protected SOConfig_ProjectileLayout _parentProjectileLayoutConfig;

		public LocalActionBus GetSelfLocalActionBusRef()
		{
			return _parentProjectileLayoutConfig.LayoutHandlerFunction.SelfActionBusReference;
		}
		
		
		
		//【edit】
		// 23.8.19 具体的版面Component不再直接拿CasterRef，而是向Handler本身去拿(因为Caster无了这件事情是版面Handler层面才有的信息)，如果此时Data不再Valid，则返回之前的数据缓存。
		// protected I_RP_Projectile_ObjectCanReleaseProjectile GetSelfCasterRef()
		// {
		// 	return _parentProjectileLayoutConfig.LayoutHandlerFunction.CasterRef;
		// }

		public virtual void InitializeBeforeStart(SOConfig_ProjectileLayout config,float currentTime, int currentFrame){
			_parentProjectileLayoutConfig = config;
		}
		public virtual void StartLayoutComponent(float currentTime, int currentFrame){}
		public virtual void FixedUpdateTick(float currentTime, int currentFrame, float delta){}
		public virtual void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef) { }

		/// <summary>
		/// <para>检查目标LayoutComponent是不是和自己同属于一个LayoutConfig实例，如果不是就其实与自己无关</para>
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckIfSameLayoutConfigParent(BaseProjectileLayoutComponent layoutComponent)
		{
			if (layoutComponent == null)
			{
				return false;
			}
			return System.Object.ReferenceEquals(layoutComponent._parentProjectileLayoutConfig,
				_parentProjectileLayoutConfig);
		}
	}

	/// <summary>
	/// <para>Layout生成时的中间辅助类。会被事件线传来传去</para>
	/// <para>注意这是一次Spawn生成操作的的信息，并不是生成中的每个Projectile的信息。</para>
	/// <para>（当然如果一次生成只有一个Projectile，那就是这个；但通常不是，比如NWay则会以这个数据来作为生成后面Projectile的基础数据）</para>
	/// </summary>
	public class LayoutSpawnInfo_OneSpawn
	{
		public Vector3 SpawnFromPosition;
		public Vector3 SpawnFromDirection;
		public float SpawnSize = 1f;
		public string SpawnLayoutUID;
		public int CurrentSeriesIndex;
		public int CurrentShootsIndex;
	}
	
	
	
	
	public interface ILayoutComponent_RespondToSpawn
	{
		public BaseProjectileLayoutComponent GetLayoutComponent()
		{
			return this as BaseProjectileLayoutComponent;
		}
		public BaseLayoutHandler GetHandlerRef();
		public void RespondToSpawnOperation(LayoutSpawnInfo_OneSpawn oneSpawnInfo,BaseLayoutHandler handler,
			List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo> collection,float currentTime,int currentFrame,int seriesIndex, int shootsIndex);


		public void OneSpawnOperation_SetFromSize(float size, LayoutSpawnInfo_OneSpawn oneSpawn,BaseLayoutHandler handlerRef)
		{
			oneSpawn.SpawnSize = size;

			DS_ActionBusArguGroup ds_spawnOperationSetFromPosition = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PLC_Spawn_OneProjectileSetScale_一个投射物生成设置了尺寸);
			ds_spawnOperationSetFromPosition.ObjectArgu1 = this;
			ds_spawnOperationSetFromPosition.ObjectArgu2 = oneSpawn;
			ds_spawnOperationSetFromPosition.ObjectArguStr = handlerRef;
			GetLayoutComponent().GetSelfLocalActionBusRef().TriggerActionByType(
				ds_spawnOperationSetFromPosition);
		}
		public void OneSpawnOperation_SetFromPosition(Vector3 fromPos, LayoutSpawnInfo_OneSpawn oneSpawn ,BaseLayoutHandler handlerRef)
		{
			oneSpawn.SpawnFromPosition = fromPos;

			DS_ActionBusArguGroup ds_spawnOperationSetFromPosition = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PLayout_Spawn_OneSpawnOperationSetFromPosition_一次生成设置了起始位置);
			ds_spawnOperationSetFromPosition.ObjectArgu1 = this;
			ds_spawnOperationSetFromPosition.ObjectArgu2 = oneSpawn;
			ds_spawnOperationSetFromPosition.ObjectArguStr = handlerRef;
			GetLayoutComponent().GetSelfLocalActionBusRef().TriggerActionByType(
				ds_spawnOperationSetFromPosition);
		}


		public void OneSpawnOperation_SetFromDirection(Vector3 spawnDirection,LayoutSpawnInfo_OneSpawn oneSpawn, BaseLayoutHandler
			handler)
		{
			oneSpawn.SpawnFromDirection = spawnDirection;
			DS_ActionBusArguGroup ds_spawnOperationSetFromRotation = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PLayout_Spawn_OneSpawnOperationSetFromDirection_一次生成设置了起始朝向);
			ds_spawnOperationSetFromRotation.ObjectArgu1 = this;
			ds_spawnOperationSetFromRotation.ObjectArgu2 = oneSpawn;
			ds_spawnOperationSetFromRotation.ObjectArguStr = handler;
			GetLayoutComponent().GetSelfLocalActionBusRef().TriggerActionByType(
				ds_spawnOperationSetFromRotation);
		}


		/// <summary>
		/// <para>根据目标的生命周期和期望距离，设置飞行的速度</para>
		/// <para>常用于技能等问题的判定，只有一个期望距离和生命周期，此时希望设置合适的速度来达到这个目的</para>
		/// </summary>
		public void SetSpeedByLifetimeAndDistance(float lifetime, float distance)
		{
			float speedRaw = distance / lifetime;
			if (this is LC_JustSpawn justSpawn)
			{
				justSpawn.SetFlySpeed = speedRaw;
			}
			else if (this is LC_NWay nWay)
			{
				nWay.FlySpeed = speedRaw;
			}


		}
	}

	public interface ILayoutComponent_CanAddProjectileToSpawnCollection
	{
		public BaseProjectileLayoutComponent GetLayoutComponent()
		{
			return this as BaseProjectileLayoutComponent;
		}
		


		public virtual DS_ActionBusArguGroup SetOneProjectilePreStartPosition(ProjectileBehaviour_Runtime projectile,Vector3 position, BaseLayoutHandler handlerRef)
		{
			//设置位置
			projectile.StartPosition = position;
			DS_ActionBusArguGroup _preSetPositionDS =
				new DS_ActionBusArguGroup(
					ActionBus_ActionTypeEnum.L_PLC_Spawn_OneProjectileSetPrePosition_一个投射物生成设置了预位置);
			_preSetPositionDS.ObjectArgu1 = projectile;
			_preSetPositionDS.ObjectArgu2 = this;
			_preSetPositionDS.ObjectArguStr = handlerRef;
			GetLayoutComponent().GetSelfLocalActionBusRef()
				.TriggerActionByType(
					_preSetPositionDS);
			return _preSetPositionDS;
		}

		/// <summary>
		/// <para>设置起始方向，这是只在平面上计算的，所以传入的是一个Vector2的旋转方向</para>
		/// </summary>
		public DS_ActionBusArguGroup SetOneProjectilePreStartDirection(ProjectileBehaviour_Runtime projectile,
			Vector2 rotateDir,BaseLayoutHandler handlerRef)
		{

			//设置朝向
			projectile.SetProjectileStartDirectionV2(rotateDir);
			DS_ActionBusArguGroup _preSetDirectionDS =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_PLC_Spawn_OneProjectileSetPreDirection_一个投射物生成设置了预朝向);
			_preSetDirectionDS.ObjectArgu1 = projectile;
			_preSetDirectionDS.ObjectArgu2 = this;
			_preSetDirectionDS.ObjectArguStr = handlerRef;
			GetLayoutComponent().GetSelfLocalActionBusRef()
				.TriggerActionByType(ActionBus_ActionTypeEnum.L_PLC_Spawn_OneProjectileSetPreDirection_一个投射物生成设置了预朝向,
					_preSetDirectionDS);
			return _preSetDirectionDS;

		}

		public DS_ActionBusArguGroup SetOneProjectilePreStartDirection(ProjectileBehaviour_Runtime projectile,
			Vector3 rotateDirInV3,BaseLayoutHandler handlerRef)
		{

			//设置朝向
			projectile.SetProjectileStartDirection(rotateDirInV3);
			DS_ActionBusArguGroup _preSetDirectionDS =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_PLC_Spawn_OneProjectileSetPreDirection_一个投射物生成设置了预朝向);
			_preSetDirectionDS.ObjectArgu1 = projectile;
			_preSetDirectionDS.ObjectArgu2 = this;
			_preSetDirectionDS.ObjectArguStr = handlerRef;
			GetLayoutComponent().GetSelfLocalActionBusRef().TriggerActionByType(
				_preSetDirectionDS);
			return _preSetDirectionDS;

		}
		public DS_ActionBusArguGroup SetOneProjectilePreStartSpeed(ProjectileBehaviour_Runtime projectile, float speed)
		{


			//设置速度
			projectile.StartSpeed = speed;
			DS_ActionBusArguGroup _preSetFlySpeed =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PLC_Spawn_OnProjectileSetPreSpeed_一个投射物生成设置了预速度);
			_preSetFlySpeed.ObjectArgu1 = projectile;
			_preSetFlySpeed.ObjectArgu2 = this;
			GetLayoutComponent().GetSelfLocalActionBusRef()
				.TriggerActionByType(_preSetFlySpeed);
			return _preSetFlySpeed;
		}


		public DS_ActionBusArguGroup SetOneProjectilePreLifetime(ProjectileBehaviour_Runtime projectile, float lifetime)
		{

			projectile.StartLifetime = lifetime;
			DS_ActionBusArguGroup ds_preLifetime =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PLC_Spawn_OnProjectileSetPreLifetime_一个投射物生成设置了生命周期);
			ds_preLifetime.ObjectArgu1 = projectile;
			ds_preLifetime.ObjectArgu2 = this;
			GetLayoutComponent().GetSelfLocalActionBusRef().TriggerActionByType(ds_preLifetime);
			return ds_preLifetime;
		}

		public DS_ActionBusArguGroup SetOneProjectileSize(ProjectileBehaviour_Runtime projectile, float size)
		{
			projectile.StartLocalSize = size;
			DS_ActionBusArguGroup ds_size =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PLC_Spawn_OneProjectileSetScale_一个投射物生成设置了尺寸);
			ds_size.ObjectArgu1 = projectile;
			ds_size.ObjectArgu2 = this;
			GetLayoutComponent().GetSelfLocalActionBusRef().TriggerActionByType(ds_size);
			return ds_size;

		}

		public DS_ActionBusArguGroup SetOneProjectileBaseInfoDoneAndAddToWaitList(ProjectileBehaviour_Runtime projectile,
			List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo> collection, float spawnTime,int seriesIndex,int shootsIndex,int setIndex)
		{
			BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo newWait = GenericPool<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>.Get();
			newWait.SeriesIndex = seriesIndex;
			newWait.ShootsIndex = shootsIndex;
			newWait.ProjectileBehaviour = projectile;
			newWait.WillSpawnTime = spawnTime;
			collection.Add(newWait);
		

			DS_ActionBusArguGroup ds_projectileBaseSetDone = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_PLC_Spawn_OnProjectileRespondToSpawnBaseSetDone_一个在响应生成时的原始投射物基础设置完成);
			ds_projectileBaseSetDone.IntArgu1 = setIndex;
			ds_projectileBaseSetDone.FloatArgu1 = spawnTime;
			ds_projectileBaseSetDone.ObjectArgu1 = projectile;
			ds_projectileBaseSetDone.ObjectArgu2 = collection;

			GetLayoutComponent().GetSelfLocalActionBusRef()
				.TriggerActionByType(
					ActionBus_ActionTypeEnum.L_PLC_Spawn_OnProjectileRespondToSpawnBaseSetDone_一个在响应生成时的原始投射物基础设置完成,
					ds_projectileBaseSetDone);
			return ds_projectileBaseSetDone;
		}
		
	}
	
	
	
	
}