using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
namespace RPGCore.Projectile.Layout
{
	[Serializable]
	public class LC_SpawnIsProgress : BaseProjectileLayoutComponent
	{

		[LabelText("将本次Shoots的子弹分散到后续这么长时间内|作为曲线的Y轴")]
		public float SpawnTimeLength = 0.5f;
		
		[LabelText("过程曲线，Y表示分摊到的时长，X轴范围[0,1]表示完成度，1为完全完成")]
		public AnimationCurve FinishCurve;
		
		


		[LabelText("包含速度修正乘数曲线"), FoldoutGroup("配置", true)]
		public bool ContainSpeedOffsetProgress;
		[LabelText("速度修正曲线，X轴范围[0,1]，Y：在该Series中的速度会乘算这个曲线，1表示不变"), ShowIf(nameof(ContainSpeedOffsetProgress)),
		 FoldoutGroup("配置", true)]
		public AnimationCurve SpeedOffsetProgressCurve;

		private List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo> _processList;


		public override void StartLayoutComponent(float currentTime, int currentFrame)
		{
			base.StartLayoutComponent(currentTime, currentFrame);
		}



		// public UnityAction<BaseProjectileLayoutComponent> NotifyLayoutNodeEnd
		// {
		// 	get;
		// 	set;
		// }

		// private class SpawnProgressInfoPair
		// {
		// 	public bool Used;
		// 	public ProjectileBehaviour_Runtime RegisteredProjectileBehaviour;
		// 	public int Index;
		// 	public float SpawnAtTime;
		// }
		// private List<SpawnProgressInfoPair> InfoPairList;


		public override void InitializeBeforeStart(SOConfig_ProjectileLayout config, float currentTime, int currentFrame)
		{

			config.LayoutHandlerFunction.SelfActionBusReference.RegisterAction(
				ActionBus_ActionTypeEnum.L_PLC_Spawn_OnOneSpawnOperationAddToList_当一次生成活动已经将数据加入了列表,
				_ABC_ProcessWaitSpawnList_OnOneSpawnOperationAddToList);
			_processList =
				CollectionPool<List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>,
					BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>.Get();
			
			// _currentProgressTimestamp = 0f;
			// _currentSpawnTimeLength = FinishCurve[FinishCurve.length - 1].time;
			// if (InfoPairList == null)
			// {
			// 	InfoPairList = CollectionPool<List<SpawnProgressInfoPair>, SpawnProgressInfoPair>.Get();
			// }
		}

		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			// base.FixedUpdateTick(currentTime, currentFrame, delta);
			//
			// if (!CurrentIsInSpawnProgress)
			// {
			// 	return;
			// }
			//
			// _currentProgressTimestamp += delta;
			// foreach (SpawnProgressInfoPair spawnProgressInfoPair in InfoPairList)
			// {
			// 	if (!spawnProgressInfoPair.Used && _currentProgressTimestamp > spawnProgressInfoPair.SpawnAtTime)
			// 	{
			// 		ProjectileBehaviour_Runtime projectile = spawnProgressInfoPair.RegisteredProjectileBehaviour;
			// 		spawnProgressInfoPair.Used = true;
			// 		
			// 		projectile.StartAllFunctionComponent(_parentLayoutConfigRef.FunctionComponentsOverride);
			// 		_parentLayoutConfigRef.OneNewProjectileSpawned_RegisterThisAndActivate(projectile);
			//
			// 	}
			// }
			// bool hasUnused = true;
			// foreach (SpawnProgressInfoPair spawnProgressInfoPair in InfoPairList)
			// {
			// 	if (!spawnProgressInfoPair.Used)
			// 	{
			// 		hasUnused = false;
			// 	}
			// 	
			// }
			// if (hasUnused)
			// {
			// 	Stop();
			// 	
			// }
			
		}
		// public void Stop()
		// {
		// 	CurrentIsInSpawnProgress = false;
		// 	_currentProgressTimestamp = 0f;
		// 	NotifyLayoutNodeEnd.Invoke(this);
		// 	if (InfoPairList != null)
		// 	{
		// 		foreach (SpawnProgressInfoPair spawnProgressInfoPair in InfoPairList)
		// 		{
		// 			GenericPool<SpawnProgressInfoPair>.Release(spawnProgressInfoPair);
		// 		}
		// 		InfoPairList.Clear();
		// 		CollectionPool<List<SpawnProgressInfoPair>, SpawnProgressInfoPair>.Release(InfoPairList);
		// 		InfoPairList = null;
		// 	}
		// }

		// public void AddNewPISPEntry(ProjectileBehaviour_Runtime projectile, int index,float partial)
		// {
		// 	if (InfoPairList == null)
		// 	{
		// 		InfoPairList = CollectionPool<List<SpawnProgressInfoPair>, SpawnProgressInfoPair>.Get();
		// 	}
		// 	
		// 	CurrentIsInSpawnProgress = true;
		// 	var infoPair = GenericPool<SpawnProgressInfoPair>.Get();
		// 	infoPair.RegisteredProjectileBehaviour = projectile;
		// 	// infoPair.RegisteredProjectileBehaviour.SetOccupyActiveButNotGOActive();
		// 	infoPair.Used = false;
		// 	infoPair.Index = index;
		// 	infoPair.SpawnAtTime = FinishCurve.Evaluate(partial * _currentSpawnTimeLength) * _currentSpawnTimeLength;
		// 	InfoPairList.Add(infoPair);
		// }

		private void _ABC_ProcessWaitSpawnList_OnOneSpawnOperationAddToList(DS_ActionBusArguGroup ds)
		{
			_processList.Clear();
			var currentSeries = ds.IntArgu1.Value;
			var currentShoots = ds.IntArgu2.Value;
			var list = ds.ObjectArgu1 as List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>;
			var handler = ds.ObjectArgu2 as BaseLayoutHandler;


			foreach (BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo  perInfo in list)
			{
				if (currentSeries == perInfo.SeriesIndex && currentShoots == perInfo.ShootsIndex)
				{
					_processList.Add(perInfo);
				}
			}

			int capacity = _processList.Count;
			for (int i = 0; i < _processList.Count; i++)
			{
				var current = _processList[i];
				current.WillSpawnTime += SpawnTimeLength * FinishCurve.Evaluate((float)i / capacity);
			}



		}
		
		public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
		{
			_processList.Clear();
			CollectionPool<List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>,
				BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>.Release(_processList);

			parentLayoutRef.LayoutHandlerFunction.SelfActionBusReference.RemoveAction(
				ActionBus_ActionTypeEnum.L_PLC_Spawn_OnOneSpawnOperationAddToList_当一次生成活动已经将数据加入了列表,
				_ABC_ProcessWaitSpawnList_OnOneSpawnOperationAddToList);

			
			// if (InfoPairList != null)
			// {
			// 	foreach (SpawnProgressInfoPair spawnProgressInfoPair in InfoPairList)
			// 	{
			// 		GenericPool<SpawnProgressInfoPair>.Release(spawnProgressInfoPair);
			// 	}
			// 	InfoPairList.Clear();
			// 	CollectionPool<List<SpawnProgressInfoPair>, SpawnProgressInfoPair>.Release(InfoPairList);
			// 	InfoPairList = null;
			// }
		}
	}
}