using System;
using System.Collections.Generic;
using ARPG.Config.BattleLevelConfig;
using ARPG.Manager.Component;
using Global;
using Global.AreaOnMap.AreaFunctionHandler;
using Sirenix.OdinInspector;
using UnityEngine;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[TypeInfoBox("使用这类型的生成配置，需要每个具体类型的敌人信息都包含【附加项 - EAS_计时敌人生成数据】")]
	[Serializable]
	public class SECT_计时生成_TimerCountSpawnEnemyConfigType : BaseSpawnEnemyConfigTypeHandler
	{

		[SerializeField, LabelText("生成将会持续这么长时间"), FoldoutGroup("配置", true)]
		public float SpawnDuration = 20f;


		//停止生成的时间
		[NonSerialized, ShowInInspector, ReadOnly, LabelText("停止生成的时间"), FoldoutGroup("运行时")]
		public float StopSpawnTime;
		[NonSerialized, FoldoutGroup("运行时"), ShowInInspector, ReadOnly]
		private Dictionary<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo, EAS_计时生成敌人数据_TimerCountInfo>
			_perTypeInfoToTimerCountInfoDict;



		public override void InitializeOnInstantiate(
			SOConfig_SpawnEnemyConfig configRef,
			SOConfig_SpawnEnemyConfig rawTemplate,string areaID)
		{
			base.InitializeOnInstantiate(configRef, rawTemplate, areaID);
			_perTypeInfoToTimerCountInfoDict =
				new Dictionary<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo, EAS_计时生成敌人数据_TimerCountInfo>();
			// _perTypeInfoToTimerCountInfoDict =
			// 	CollectionPool<Dictionary<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo, EAS_计时生成敌人数据_TimerCountInfo>,
			// 		KeyValuePair<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo, EAS_计时生成敌人数据_TimerCountInfo>>.Get();
			foreach (SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo perInfo in RelatedSpawnConfigRuntimeRef
				.EnemySpawnConfigTypeHandler.EnemySpawnCollection_RuntimeFinal)
			{
				EAS_计时生成敌人数据_TimerCountInfo eas =
					perInfo.GetRuntimeFinalAddonList().Find((addon => addon is EAS_计时生成敌人数据_TimerCountInfo)) as
						EAS_计时生成敌人数据_TimerCountInfo;
				_perTypeInfoToTimerCountInfoDict.Add(perInfo, eas);
			}
		}
		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			CheckIfSpawnHandlerFinished();
			if (RelatedSpawnConfigRuntimeRef.ConfigFinished)
			{
				return;
			}
			foreach (var perKVP in _perTypeInfoToTimerCountInfoDict)
			{
				SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo perInfo = perKVP.Key;

				EAS_计时生成敌人数据_TimerCountInfo eas = perKVP.Value;

				if (ct >= eas.NextAvailableTime)
				{
					//如果可以生成
					//先判断是否超过上限
					if (eas.MaxCount > 0)
					{
						if (SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
							.GetCharacterCountBySpecificType(perInfo.Type) >= eas.MaxCount)
						{
							//超过上限，不生成
							continue;
						}
					}
					//可以生成
					//先判断是否有位置
					// AF_刷怪位置_EnemySpawnPositionRegister.EnemySpawnPositionRuntimeInfo positionInfo = getPositionDelegate(
					// 	perInfo.SpawnPresetType,
					// 	perInfo.SpecificPointID);
					// if (positionInfo == null)
					// {
					// 	//没有位置，不生成
					// 	continue;
					// }
					//可以生成
					//生成
					AddNewSingleSpawnInfoToListByPerEnemyTypeInfo(perInfo, perInfo.SpawnCount);
					//更新下次可用时间
					eas.NextAvailableTime = ct + eas.PickInterval;
				}
			}

			base.FixedUpdateTick(ct, cf, delta);
		}


		public override void ProcessConcreteSpawnInfoWithWaitSpawnInfoDict(
			Dictionary<SOConfig_SpawnEnemyConfig, List<EnemySpawnService_SubActivityService.SingleSpawnInfo>>
				spawnInfoDict)
		{
			_relatedSingleSpawnInfoListRef = new List<EnemySpawnService_SubActivityService.SingleSpawnInfo>();
			_relatedSingleSpawnInfoListRef.Clear();
			spawnInfoDict.Add(RelatedSpawnConfigRuntimeRef, _relatedSingleSpawnInfoListRef);
			StopSpawnTime = BaseGameReferenceService.CurrentFixedTime + SpawnDuration + InitDelayDuration;
		}

		protected override void CheckIfSpawnHandlerFinished()
		{
			if (BaseGameReferenceService.CurrentFixedTime >= StopSpawnTime)
			{
				for (int i = _relatedSingleSpawnInfoListRef.Count - 1; i >= 0; i--)
				{
					_relatedSingleSpawnInfoListRef[i].SpawnCountOverride = 0;
					_relatedSingleSpawnInfoListRef.RemoveAt(i);
				}
				_relatedSingleSpawnInfoListRef.Clear();
				RelatedSpawnConfigRuntimeRef.ConfigFinished = true;
			}
		}


		public override void ClearBeforeDestroy()
		{
			// CollectionPool<Dictionary<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo, EAS_计时生成敌人数据_TimerCountInfo>,
			// 		KeyValuePair<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo, EAS_计时生成敌人数据_TimerCountInfo>>
			// 	.Release(_perTypeInfoToTimerCountInfoDict);
			base.ClearBeforeDestroy();
		}

#if UNITY_EDITOR
		protected override bool _Editor_CollectionInputValidCheck()
		{
			foreach (var c in Collection_FileConfig)
			{
				foreach (var perTypeInfo in c.EnemyTypeInfoList)
				{
					if (perTypeInfo.GetRuntimeFinalAddonList() == null ||
					    !perTypeInfo.GetRuntimeFinalAddonList().Exists((addon => addon is EAS_计时生成敌人数据_TimerCountInfo)))
					{
						return false;
					}
				}
			}
			foreach (var perTypeInfo in Collection_SerializeConfig)
			{
				if (perTypeInfo.GetRuntimeFinalAddonList() == null ||
				    !perTypeInfo.GetRuntimeFinalAddonList().Exists((addon => addon is EAS_计时生成敌人数据_TimerCountInfo)))
				{
					return false;
				}
			}
			return base._Editor_CollectionInputValidCheck();
		}
#endif
	}
}