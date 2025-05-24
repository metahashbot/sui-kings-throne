using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Common;
using ARPG.Config.BattleLevelConfig;
using ARPG.Manager.Config;
using GameplayEvent.Handler;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.AreaFunctionHandler;
using Global.AreaOnMap.EditorProxy;
using Global.Utility;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using static ARPG.Manager.Config.SOFE_MissionConfigInfo;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;
// using UnityEngine.Pool;
namespace ARPG.Manager.Component
{
	/// <summary>
	/// <para>一个负责敌人生成的组件。</para>
	/// </summary>
	[Serializable]
	public class EnemySpawnService_SubActivityService : BaseSubActivityService
	{
		/// <summary>
		/// 单条生成信息。
		/// <para>必定是运行时生成的，每次有要求生成的时候就都是使用这个数据结构</para>
		/// </summary>
		public class SingleSpawnInfo
		{
			public SOConfig_SpawnEnemyConfig RelatedConfigInstance;
			public SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo RawSpawnInfo;
			[LabelText("将于这个时间点生成"), ShowInInspector]
			public float WillSpawnTime;

			public EnemySpawnPoint_PresetTypeEnum SpawnPresetPositionType;

			public string SpawnAreaID;

			public string SpecificSpawnPositionUID;

			/// <summary>
			/// <para>生成数量覆写。如果没有覆写，就使用RawSpawnInfo里面的那个数据</para>
			/// <para>覆写常见于使用相同|相似配置进行生成，但不需要生成那么多数量，比如分身的时候，计时持续生成的时候</para>
			/// </summary>
			public Nullable<int> SpawnCountOverride = null;
			public bool WithAddon = true;

			public List<SOConfig_PrefabEventConfig> EventConfigListRef_Single;
			public List<SOConfig_PrefabEventConfig> EventConfigListRef_Once;
			public float CharacterSelfVariantScaleMul = 1f;

		}


		[LabelText("等待生成的信息们"), FoldoutGroup("运行时", true), ShowInInspector,
		 InlineEditor(InlineEditorModes.LargePreview)]
		private Dictionary<SOConfig_SpawnEnemyConfig, List<SingleSpawnInfo>> _waitToSpawnInfoDict =
			new Dictionary<SOConfig_SpawnEnemyConfig, List<SingleSpawnInfo>>();

		/// <summary>
		/// 生成完成的，正在等待销毁
		/// </summary>
		private List<SOConfig_SpawnEnemyConfig> _waitToDestroyRuntimeInstanceList =
			new List<SOConfig_SpawnEnemyConfig>();


		/// <summary>
		/// key是AreaUID，
		/// </summary>
		[LabelText("目前所有的生成点们"), FoldoutGroup("运行时", true), ShowInInspector]
		private Dictionary<string, List<EnemySpawnPositionRuntimeInfo>> AllSpawnPointInfoRuntimeDict =
			new Dictionary<string, List<EnemySpawnPositionRuntimeInfo>>();
		





		public void AwakeInitialize()
        {
            int missionUID = EditorProxy_ARPGEditor.Instance.AreaLogicHolderPrefab
                .GetComponent<EditorProxy_AreaLogicHolder>().MissionUID;
            if (GCAHHExtend.IfMissionConfigContain(missionUID))
            {
                GlobalActionBus gab = GlobalActionBus.GetGlobalActionBus();
                gab.RegisterAction(
                    ActionBus_ActionTypeEnum.G_NB_TriggerAreaManager_OnTransmissionToNewArea_当传送到新区域,
                    SpawenEnemyByMissionConfig);
                gab.RegisterAction(
                    ActionBus_ActionTypeEnum.G_CharacterOnMap_OnEnemyCorpseVanish_敌人尸体消失,
                    OnEnemyDie, 100);
            }
			else
            {
                GlobalActionBus gab = GlobalActionBus.GetGlobalActionBus();
                gab.RegisterAction(
                    ActionBus_ActionTypeEnum.G_Activity_ARPG_RequireNewEnemySpawnConfig_活动ARPG_要求生成新的敌人配置,
                    _ABC_ConfigureEnemySpawnConfig_OnRequireNewEnemySpawnConfig);
                gab.RegisterAction(
                    ActionBus_ActionTypeEnum.G_CharacterOnMap_OnEnemyCorpseVanish_敌人尸体消失,
                    _ABC_CheckEnemyGroup_OnCorpseVanish, 100);
                gab.RegisterAction(
                    ActionBus_ActionTypeEnum.G_Activity_ARPG_RequireStopEnemySpawnConfig_活动ARPG_要求停止生成敌人配置,
                    _ABC_TryStopConfig_OnRequireStopEnemySpawnConfig);
            }
        }

		public void LateLoadInitialize()
		{
			var areaLogic = UnityEngine.Object.FindObjectOfType<EditorProxy_AreaLogicHolder>(true);
			if (areaLogic == null)
			{
				DBug.LogError($"在敌人生成服务的初始化时，没有找到AreaLogicHolder。这不应当出现，检查一下");
				return;
			}
			var allArea = areaLogic.GetComponentsInChildren<EP_AreaByTrigger>(true);

			foreach (var perArea in allArea)
			{
				if (AllSpawnPointInfoRuntimeDict.ContainsKey(perArea.AreaUID))
				{
					DBug.LogError($"在初始化生成点信息时，发现了重复的区域：AreaUID：{perArea.AreaUID}");
					continue;
				}
				List<EnemySpawnPositionRuntimeInfo> pointList = new List<EnemySpawnPositionRuntimeInfo>();
				AllSpawnPointInfoRuntimeDict.Add(perArea.AreaUID, pointList);
				var allPresetInArea = perArea.GetComponentsInChildren<PresetSpawnRegister>(true);
				for (int i = 0; i < allPresetInArea.Length; i++)
				{
					PresetSpawnRegister perPreset = allPresetInArea[i];
					var newRuntimeInfo = new EnemySpawnPositionRuntimeInfo();
					newRuntimeInfo.AreaByTriggerRef = perArea;
					newRuntimeInfo.PresetType = perPreset.SpawnPointPresetType;
					newRuntimeInfo.MatchingGameObjectList = new List<GameObject>();
					switch (perPreset.SpawnPointPresetType)
					{
						case EnemySpawnPoint_PresetTypeEnum.Default_默认通用:
						case EnemySpawnPoint_PresetTypeEnum.Specific_指定专用:
							newRuntimeInfo.MatchingGameObjectList.Add(perPreset.gameObject);
							break;
						case EnemySpawnPoint_PresetTypeEnum.FromAndTo_从这里到那里:
							newRuntimeInfo.MatchingGameObjectList.Add(perPreset.gameObject);
							if (perPreset.TargetPointPositionGO == null)
							{
								DBug.LogError($"在生成点的配置中，发现了一个从这里到那里的配置，但是没有指定目标点。这不应当出现，检查一下");
								continue;
							}
							newRuntimeInfo.MatchingGameObjectList.Add(perPreset.TargetPointPositionGO);
							break;
						case EnemySpawnPoint_PresetTypeEnum.OrderName_按顺序的:
							if (perPreset.SomePositionsGOList == null || perPreset.SomePositionsGOList.Count == 0)
							{
								DBug.LogError($"在生成点的配置中，发现了一个按顺序的配置，但是没有指定目标点。这不应当出现，检查一下");
								continue;
							}
							for (int j = 0; j < perPreset.SomePositionsGOList.Count; j++)
							{
								newRuntimeInfo.MatchingGameObjectList.Add(perPreset.SomePositionsGOList[j]);
							}
							break;
						case EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合:
							if (perPreset.SomePositionsGOList == null || perPreset.SomePositionsGOList.Count == 0)
							{
								DBug.LogError($"在生成点的配置中，发现了一个不重复的集合，但是没有指定目标点。这不应当出现，检查一下");
								continue;
							}
							for (int j = 0; j < perPreset.SomePositionsGOList.Count; j++)
							{
								newRuntimeInfo.MatchingGameObjectList.Add(perPreset.SomePositionsGOList[j]);
							}
							break;
					}
					newRuntimeInfo.SpawnPointNameID = string.IsNullOrEmpty(perPreset.SelfRegisterName)
						? perPreset.gameObject.name : perPreset.SelfRegisterName;
					pointList.Add(newRuntimeInfo);
				}
			}
        }

		/// <summary>
		/// <para>响应生成要求。进行具体的生成配置</para>
		/// </summary>
		protected void _ABC_ConfigureEnemySpawnConfig_OnRequireNewEnemySpawnConfig(DS_ActionBusArguGroup ds)
		{
			SOConfig_SpawnEnemyConfig configRaw = ds.ObjectArgu1 as SOConfig_SpawnEnemyConfig;
			GEH_生成敌人_SpawnEnemy spawnFrom = ds.ObjectArgu2 as GEH_生成敌人_SpawnEnemy;
			if (spawnFrom == null)
			{
				DBug.LogError($"【敌人生成服务】在接收新的生成配置时，并没有接收到应当被一并传入的刷怪事件Handler引用。这并不合理");
				return;
			}


			SOConfig_SpawnEnemyConfig newRuntimeConfig = UnityEngine.Object.Instantiate(configRaw);
			newRuntimeConfig.InitializeOnInstantiate(configRaw, spawnFrom.AreaUID);



			//实例化运行时的配置对象。会将发起这个请求的事件的相关信息注入到新对象内

			newRuntimeConfig.RelatedSpawnEnemyGameplayEventHandlerRef = spawnFrom;
			newRuntimeConfig.EnemySpawnConfigTypeHandler.ProcessConcreteSpawnInfoWithWaitSpawnInfoDict(
				_waitToSpawnInfoDict);





			DS_ActionBusArguGroup ds_configDone = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.G_Activity_ARPG_OnNewSpawnConfigRegistered_活动ARPG_敌人生成配置已注册);
			ds_configDone.ObjectArgu1 = newRuntimeConfig;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_configDone);
		}

		/// <summary>
		/// <para>当任一敌人尸体消失时，对所有敌人进行检查，来判断是否需要触发“区域”内敌人清空的情况</para>
		/// <para></para>
		/// </summary>
		private void _ABC_CheckEnemyGroup_OnCorpseVanish(DS_ActionBusArguGroup ds)
		{
			//edit 230811
			/*
			 * 我们并不检查[全局]的敌人是否都死了。因为会因为场景机制的存在，场上始终会存在一些敌人
			 * 所以检查的是，来自某一个[区域]的敌人是否都死了。区域的划分是根据触发刷怪的事件所归属的区域来划分的
			 * 也就是说，每个大的场景，都会有一个用来刷预制内容的整体区域。然后再在此之上，刷每个具体的房间的敌人。
			 */

			/*
			 * 对于某个具体[区域]的检查，还要判断是否还有正在[排队]的情况，因为可能有些情况下，敌人的生成过程是一段时间，
			 * 这时候虽然场上没有敌人了，但是并不算做"区域清怪"
			 */


			if (ds.ObjectArgu1 is not EnemyARPGCharacterBehaviour enemyBehaviour)
			{
				return;
			}

			SOConfig_SpawnEnemyConfig relatedSpawnConfig = enemyBehaviour.RelatedSpawnConfigInstance;


			//如果当前所有排着的生成信息里面，有和这个区域Key相同的，则表明还没有生成完
			foreach (var kvp in _waitToSpawnInfoDict)
			{
				if (kvp.Key.EnemySpawnConfigTypeHandler.IfNotCountInLevelClear)
				{
					continue;
				}
				if (!kvp.Key.RelatedSpawnEnemyGameplayEventHandlerRef.HandlerFinished)
				{
					Debug.Log($"[清理判定]{enemyBehaviour.name}之后，仍有敌人需要生成");
					return;
				}
				if (kvp.Key.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID.Equals(
					relatedSpawnConfig.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID,
					StringComparison.OrdinalIgnoreCase) && kvp.Value.Count > 0)
                {
                    Debug.Log($"[清理判定]{enemyBehaviour.name}之后，仍有敌人需要生成");
                    return;
				}
			}
			//没有排队，那就检查还有没有或者的

			string relatedAreaID = enemyBehaviour.RelatedSpawnConfigInstance.RelatedSpawnEnemyGameplayEventHandlerRef
				.AreaUID;




			List<BaseARPGCharacterBehaviour> com = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
				.CurrentAllActiveARPGCharacterBehaviourCollection;
			//场上活着的里面还有，则也不是
			//(此处判定：COM中，有敌人，且该敌人数据仍有效，且该敌人关联的生成信息中的区域UID和当前的区域UID相同）
			if (com.Exists((behaviour => behaviour is EnemyARPGCharacterBehaviour enemyArpgCharacterBehaviour &&
			                             enemyArpgCharacterBehaviour.CharacterDataValid &&
			                             string.Equals(relatedAreaID,
                                             enemyArpgCharacterBehaviour.RelatedAreaID,
				                             StringComparison.OrdinalIgnoreCase) && !enemyArpgCharacterBehaviour
				                             .RelatedSpawnConfigInstance.EnemySpawnConfigTypeHandler
				                             .IfNotCountInLevelClear)))
            {
                Debug.Log($"[清理判定]{enemyBehaviour.name}之后，场上仍有敌人");
                return;
			}



			if (!relatedSpawnConfig.ClearBroadcast)
			{
				//此处可以部分清除了。
				//部分清除：暂时有可能还并不能完全清除，会存在类似于“这波敌人消灭后刷援军”的业务，而且这类业务需要[部分清除]去触发。
				//所以会先把部分清除的事件打出去，如果打出去之后没有这样的业务使得仍然在排队|占用，那才是真的完全清除
				var ds_clearPartial = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.G_Activity_ARPG_PartiallyClearAreaEnemy_活动ARPG_区域内的敌人部分清除了);
				ds_clearPartial.ObjectArguStr = relatedSpawnConfig.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_clearPartial);
				relatedSpawnConfig.ClearBroadcast = true;
			}
			else
			{
				return;
			}


			//由于 部分清除 事件已触发，所以这里又排上了，则表明确实是有类似于 消灭后刷援军 的业务响应了，所以return掉，不会触发“全部清除”
			//如果当前所有排着的生成信息里面，有和这个区域Key相同的，则表明还没有生成完
			foreach (var kvp in _waitToSpawnInfoDict)
			{
				if (kvp.Key.EnemySpawnConfigTypeHandler.IfNotCountInLevelClear)
				{
					continue;
				}
				if (kvp.Key.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID.Equals(
					relatedSpawnConfig.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID,
					StringComparison.OrdinalIgnoreCase) && kvp.Value.Count > 0)
                {
                    Debug.Log($"[清理判定]{enemyBehaviour.name}之后，广播后有援军生成");
                    return;
				}
			}



			DS_ActionBusArguGroup ds_clear =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Activity_ARPG_ClearAreaEnemy_活动ARPG_清除区域内的敌人);
			ds_clear.ObjectArguStr = relatedSpawnConfig.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_clear);
		}

		protected void _ABC_TryStopConfig_OnRequireStopEnemySpawnConfig(DS_ActionBusArguGroup ds)
		{
			var id = ds.ObjectArguStr as string;
			foreach (var kvp in _waitToSpawnInfoDict)
			{
				if (string.IsNullOrEmpty(kvp.Key.ConfigID))
				{
					continue;
				}
				if (kvp.Key.ConfigID.Equals(id, StringComparison.OrdinalIgnoreCase))
				{
					kvp.Key.ConfigFinished = true;
				}
			}
		}

		private List<WaveConfigInfo>.Enumerator waveEnumerator;
		private List<EnemyARPGCharacterBehaviour> activeEnemyList = new();

        private void SpawenEnemyByMissionConfig(DS_ActionBusArguGroup ds)
        {
            if (ds.ObjectArgu1 is string areaUID)
			{
				var templateID = ds.IntArgu1.Value;
				var templateConfig = GCAHHExtend.GetRoomTemplateConfigInfo(templateID);
				if (templateConfig == null)
				{
					return;
				}
				waveEnumerator = templateConfig.WaveConfigList.GetEnumerator();

                if (waveEnumerator.MoveNext())
                {
                    ActivityManagerRef.StartCoroutine(
						SpawnWave(templateConfig.AreaID, waveEnumerator.Current));
                }
            }
        }

        private IEnumerator SpawnWave(string areaID, WaveConfigInfo waveConfig)
        {
            yield return new WaitForSeconds(1.0f);

            foreach (var enemyInfo in waveConfig.EnemyConfigList)
            {
                CharacterNamedTypeEnum enemyType = (CharacterNamedTypeEnum)enemyInfo.UID;
				for(int i = 0; i< enemyInfo.EnemyNumber; i++)
                {
                    Vector3 enemyPosition = GetEnemyPosition(areaID, enemyInfo.SpawnPoint);
                    var enemy = SubGameplayLogicManager_ARPG.Instance
                        .CharacterOnMapManagerReference.SpawnNewEnemy(enemyType, enemyPosition);
                    enemy.GetAIBrainRuntimeInstance()?.BrainHandlerFunction.PickDefaultBehaviourPattern();
                    enemy.RelatedAreaID = areaID;
                    activeEnemyList.Add(enemy);
                    yield return null;
                }
            }
        }

		public Vector3 GetEnemyPosition(string areaID, string pointName)
		{
			var pointList = AllSpawnPointInfoRuntimeDict[areaID];
			EnemySpawnPositionRuntimeInfo point = null;
			if (!string.IsNullOrEmpty(pointName))
			{
				point = pointList.Find(p => p.SpawnPointNameID == pointName);
			}
			if (point == null)
			{
				var tmpList = pointList.Where(
					p => p.PresetType == EnemySpawnPoint_PresetTypeEnum.Default_默认通用).ToList();
				int index = UnityEngine.Random.Range(0, tmpList.Count);
				point = tmpList[index];
			}

            return point.GetCurrentSpawnPosition();
		}

		public void OnEnemyDie(DS_ActionBusArguGroup ds)
        {
            if (ds.ObjectArgu1 is not EnemyARPGCharacterBehaviour enemyBehaviour)
            {
                return;
            }
			string areaID = enemyBehaviour.RelatedAreaID;

			activeEnemyList.Remove(enemyBehaviour);
			if (activeEnemyList.Count == 0)
            {
                if (waveEnumerator.MoveNext())
                {
                    ActivityManagerRef.StartCoroutine(SpawnWave(areaID, waveEnumerator.Current));
                }
				else
                {
                    DS_ActionBusArguGroup ds_clear = new DS_ActionBusArguGroup(
						ActionBus_ActionTypeEnum.G_Activity_ARPG_ClearAreaEnemy_活动ARPG_清除区域内的敌人);
                    ds_clear.ObjectArguStr = areaID;
                    GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_clear);
                }
            }
        }

        private List<SOConfig_SpawnEnemyConfig> _waitToRemoveFromDict = new List<SOConfig_SpawnEnemyConfig>();
		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			_waitToRemoveFromDict.Clear();
			//遍历当前已记录的容器，key是包含SpawnHandler的那个Config的运行时实例。value是它对应的单条生成项目容器(单条不等于一个，单条里面也可能有很多个）
			foreach (KeyValuePair<SOConfig_SpawnEnemyConfig, List<SingleSpawnInfo>> kvp in _waitToSpawnInfoDict)
			{
				SOConfig_SpawnEnemyConfig configRef = kvp.Key;
				List<SingleSpawnInfo> perList = kvp.Value;

				configRef.EnemySpawnConfigTypeHandler.FixedUpdateTick(ct, cf, delta);

				if (configRef.ConfigFinished)
				{
					_waitToRemoveFromDict.Add(kvp.Key);
				}
			}

			//把已经生成完的配置 标记为待移除，从Dict中移除。在之后，它们将会在“已广播”后被销毁
			foreach (SOConfig_SpawnEnemyConfig removeKey in _waitToRemoveFromDict)
			{
				_waitToSpawnInfoDict.Remove(removeKey);
				_waitToDestroyRuntimeInstanceList.Add(removeKey);
			}

			for (int i = _waitToDestroyRuntimeInstanceList.Count - 1; i >= 0; i--)
			{
				if (_waitToDestroyRuntimeInstanceList[i].ClearBroadcast)
				{
					_waitToDestroyRuntimeInstanceList[i].ClearBeforeDestroy();
					UnityEngine.Object.Destroy(_waitToDestroyRuntimeInstanceList[i]);
					_waitToDestroyRuntimeInstanceList.RemoveAt(i);
				}
			}
		}

		//内部用来一次查找多个生成位置时的缓存
		private List<EnemySpawnPositionRuntimeInfo> _internalPositionQueryList =
			new List<EnemySpawnPositionRuntimeInfo>();

		/// <summary>
		/// 不限制类型，之查找名字的目标点
		/// areaID可以是空的，如果是空的则表示不限制区域，只要是点位本身的名字匹配就可以。
		/// 返回结果会按照距离排序
		/// </summary>
		public List<EnemySpawnPositionRuntimeInfo> GetTargetSpawnPoint(string areaID, string targetID)
		{
			_internalPositionQueryList.Clear();

			
			if (string.IsNullOrEmpty(areaID))
			{
				foreach (var everyList in AllSpawnPointInfoRuntimeDict.Values)
				{
					foreach (EnemySpawnPositionRuntimeInfo perInfo in everyList)
					{
						if (perInfo.SpawnPointNameID.Equals(targetID, StringComparison.OrdinalIgnoreCase))
						{
							_internalPositionQueryList.Add(perInfo);
						}
					}
				}
			}
			else
			{
				if (!AllSpawnPointInfoRuntimeDict.ContainsKey(areaID))
				{
					DBug.LogError($"在查找生成点时，传入的区域名：{areaID}没有记录过。通常来说如果没有传名字，也会传发起者关联的区域名。这不应当出现，检查一下");
					return null;
				}
				var listByArea = AllSpawnPointInfoRuntimeDict[areaID];
				foreach (EnemySpawnPositionRuntimeInfo perInfo in listByArea)
				{
					if (perInfo.SpawnPointNameID.Equals(targetID, StringComparison.OrdinalIgnoreCase))
					{
						_internalPositionQueryList.Add(perInfo);
					}
				}
			}
			
			

			if (_internalPositionQueryList.Count == 0)
			{
				DBug.LogError($"生成点查找时结果数量为0，这可能不合理。：：区域名为{areaID}，点位名为{targetID}");
				return null;
			}
			return _internalPositionQueryList;
		}


		/// <summary>
		/// <para>获取一个生成点信息。需要一个区域名字。如果没有指定特定的点，那就随机刷了</para>
		/// </summary>
		public List<EnemySpawnPositionRuntimeInfo> GetTargetSpawnPoint(
			string areaID,
			EnemySpawnPoint_PresetTypeEnum targetPointType,
			string pointID)
		{
			_internalPositionQueryList.Clear();

			if (string.IsNullOrEmpty(areaID))
			{
				foreach (var everyList in AllSpawnPointInfoRuntimeDict.Values)
				{
					foreach (EnemySpawnPositionRuntimeInfo perInfo in everyList)
					{
						switch (targetPointType)
						{
							case EnemySpawnPoint_PresetTypeEnum.Default_默认通用:
								if (string.IsNullOrEmpty(pointID))
								{
									if (perInfo.PresetType == EnemySpawnPoint_PresetTypeEnum.Default_默认通用)
									{
										_internalPositionQueryList.Add(perInfo);
									}
								}
								else
								{
									if (perInfo.PresetType == EnemySpawnPoint_PresetTypeEnum.Default_默认通用 &&
									    perInfo.SpawnPointNameID.Equals(pointID, StringComparison.OrdinalIgnoreCase))
									{
										_internalPositionQueryList.Add(perInfo);
									}
								}
								break;
							case EnemySpawnPoint_PresetTypeEnum.FromAndTo_从这里到那里:
							case EnemySpawnPoint_PresetTypeEnum.Specific_指定专用:
							case EnemySpawnPoint_PresetTypeEnum.OrderName_按顺序的:
							case EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合:
								if (string.IsNullOrEmpty(pointID))
								{
									DBug.LogError($"在查找生成点时，【指定专用】却没名字。来自区域{areaID}。这不应当出现，检查一下");
								}
								else
								{
									if (perInfo.PresetType == targetPointType &&
									    perInfo.SpawnPointNameID.Equals(pointID, StringComparison.OrdinalIgnoreCase))
									{
										_internalPositionQueryList.Add(perInfo);
									}
								}
								break;
						}
					}
				}
			}
			else
			{
				if (!AllSpawnPointInfoRuntimeDict.ContainsKey(areaID))
				{
					DBug.LogError($"在查找生成点时，传入的区域名：{areaID}没有记录过。通常来说如果没有传名字，也会传发起者关联的区域名。这不应当出现，检查一下");
					return null;
				}
				var listByArea = AllSpawnPointInfoRuntimeDict[areaID];
				foreach (EnemySpawnPositionRuntimeInfo perInfo in listByArea)
				{
					switch (targetPointType)
					{
						case EnemySpawnPoint_PresetTypeEnum.Default_默认通用:
							if (string.IsNullOrEmpty(pointID))
							{
								if (perInfo.PresetType == EnemySpawnPoint_PresetTypeEnum.Default_默认通用)
								{
									_internalPositionQueryList.Add(perInfo);
								}
							}
							else
							{
								if (perInfo.PresetType == EnemySpawnPoint_PresetTypeEnum.Default_默认通用 &&
								    perInfo.SpawnPointNameID.Equals(pointID, StringComparison.OrdinalIgnoreCase))
								{
									_internalPositionQueryList.Add(perInfo);
								}
							}
							break;
						case EnemySpawnPoint_PresetTypeEnum.FromAndTo_从这里到那里:
						case EnemySpawnPoint_PresetTypeEnum.Specific_指定专用:
						case EnemySpawnPoint_PresetTypeEnum.OrderName_按顺序的:
						case EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合:
							if (string.IsNullOrEmpty(pointID))
							{
								DBug.LogError($"在查找生成点时，【指定专用】却没名字。来自区域{areaID}。这不应当出现，检查一下");
							}
							else
							{
								if (perInfo.PresetType == targetPointType &&
								    perInfo.SpawnPointNameID.Equals(pointID, StringComparison.OrdinalIgnoreCase))
								{
									_internalPositionQueryList.Add(perInfo);
								}
							}
							break;
					}
				}
			}
			if (_internalPositionQueryList.Count == 0)
			{
				DBug.LogError($"生成点查找时结果数量为0，这可能不合理。：：区域名为{areaID}，类型为{targetPointType}，点位名为{pointID}");
				return null;
			}
			//如果没有找到符合条件的生成点，返回null
			return _internalPositionQueryList;
		}

	}

}