using System;
using System.Collections.Generic;
using ARPG.BattleActivity.Config.EnemySpawnAddon;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Config.BattleLevelConfig;
using ARPG.Manager.Component;
using GameplayEvent;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.AreaFunctionHandler;
using Global.Utility;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[Serializable]
	public abstract class BaseSpawnEnemyConfigTypeHandler
	{
		[SerializeField, LabelText("首次延迟")]
		public float InitDelayDuration = 0f;
		[SerializeField, LabelText("不计入[关卡清理]吗")]
		public bool IfNotCountInLevelClear = false;
		[SerializeField,LabelText("该生成配置使用的敌人等级")]
		public int EnemyLevelInSpawnConfigHandler = 1;

		[SerializeField, LabelText("直属配置==敌人生成配置"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true)]
		[FoldoutGroup("配置", true)]
		[FormerlySerializedAs("Collection")]
		[PropertyOrder(100)]
#if UNITY_EDITOR
		[ValidateInput(nameof(_Editor_CollectionInputValidCheck),
			DefaultMessage = "输入校验有误，参照一下上方提示检查一下数据",
			MessageType = InfoMessageType.Warning)]
#endif
		public List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo> Collection_SerializeConfig =
			new List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>();

		[SerializeField, LabelText("文件配置==敌人生成配置"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true)]
		[FoldoutGroup("配置", true), PropertyOrder(100)]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public List<SOConfig_PerEnemyTypeSpawnConfig> Collection_FileConfig;

		
		
		[NonSerialized, LabelText("运行时实际配置数据项们")]
		public List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo> EnemySpawnCollection_RuntimeFinal;



		protected static EnemySpawnService_SubActivityService EnemySpawnServiceSubActivityServiceRef;






		[LabelText("运行时-关联的生成配置SO实例"), FoldoutGroup("运行时"), NonSerialized] [PropertyOrder(200)]
		public SOConfig_SpawnEnemyConfig RelatedSpawnConfigRuntimeRef;


		[ShowInInspector, ReadOnly, LabelText("运行时产生的具体单条生成信息的容器"), NonSerialized, FoldoutGroup("运行时")]
		[PropertyOrder(200)]
		protected List<EnemySpawnService_SubActivityService.SingleSpawnInfo> _relatedSingleSpawnInfoListRef;


		/// <summary>
		/// 该次生成关联的区域ID
		/// </summary>
		public string RelatedAreaID { get; protected set; }
		
		

		public virtual void InitializeOnInstantiate(
			SOConfig_SpawnEnemyConfig configRef,
			SOConfig_SpawnEnemyConfig rawTemplate,string areaID)
		{
			EnemySpawnServiceSubActivityServiceRef = SubGameplayLogicManager_ARPG.Instance.ActivityManagerArpgInstance
				.EnemySpawnServiceSubActivityServiceComponentRef;
			
			RelatedSpawnConfigRuntimeRef = configRef;
			RelatedAreaID = areaID;

			//FESC才是真的读取的数据，另外两个都是构成这个数据容器的内容，需要一些深拷贝
			EnemySpawnCollection_RuntimeFinal = new List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>();

			// Collection =
			// 	CollectionPool<List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>,
			// 		SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>.Get();


			//进行内容的深拷贝
			foreach (SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo perEnemyTypeInfo in rawTemplate
				.EnemySpawnConfigTypeHandler.Collection_SerializeConfig)
			{
				EnemySpawnCollection_RuntimeFinal.Add(perEnemyTypeInfo);
			}
			foreach (SOConfig_PerEnemyTypeSpawnConfig perConfig in rawTemplate.EnemySpawnConfigTypeHandler
				.Collection_FileConfig)
			{
				foreach (var perTT in perConfig.EnemyTypeInfoList)
				{
					EnemySpawnCollection_RuntimeFinal.Add(perTT);
				}
			}


			if (ContainReinforcement)
			{
				if (ReinforcementCheckOnClearOrSingleDie)
				{
					GlobalActionBus.GetGlobalActionBus().RegisterAction(
						ActionBus_ActionTypeEnum.G_Activity_ARPG_PartiallyClearAreaEnemy_活动ARPG_区域内的敌人部分清除了,
						_ABC_CheckIfNeedInstantiateReinforcement_OnAreaPartiallyClear);
				}
				else
				{
					GlobalActionBus.GetGlobalActionBus().RegisterAction(
						ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
						_ABC_CheckIfNeedInstantiateReinforcement_OnSingleEnemyDie,250);
					GlobalActionBus.GetGlobalActionBus().RegisterAction(
						ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
						_ABC_CheckIfNeedInstantiateReinforcement_OnSingleEnemyDie,250);
				}
			}
		}



		/// <summary>
		/// <para>具体的生成过程，</para>
		/// </summary>
		protected virtual void ConcreteSpawnProgress(
			EnemySpawnService_SubActivityService.SingleSpawnInfo perSpawnEntry)
		{
			EnemySpawnPoint_PresetTypeEnum targetPointType = perSpawnEntry.SpawnPresetPositionType;



			// perSpawnEntry.WillSpawnTime = BaseGameReferenceService.CurrentFixedTime + perSpawnEntry.RawSpawnInfo.DelayTime;
			//如果生成时有关联事件，那就触发它们
			if (perSpawnEntry.EventConfigListRef_Once != null && perSpawnEntry.EventConfigListRef_Once.Count > 0)
			{
				foreach (var perEvent in perSpawnEntry.EventConfigListRef_Once)
				{
					var ds_ge = new DS_GameplayEventArguGroup();
					GameplayEventManager.Instance.StartGameplayEvent(perEvent, ds_ge);
				}
			}

			//
			int count = perSpawnEntry.SpawnCountOverride ?? 1;
			var list = EnemySpawnServiceSubActivityServiceRef.GetTargetSpawnPoint(perSpawnEntry.SpawnAreaID,
				targetPointType,
				perSpawnEntry.SpecificSpawnPositionUID);
			list.Shuffle();

			int _positionIndex = 0;
			for (int i = 0; i < count; i++)
			{
				SpawnOnThisPositionInfo(list[_positionIndex]);
				_positionIndex += 1;
				if (_positionIndex >= list.Count)
				{
					_positionIndex = 0;
				}
			}

			void SpawnOnThisPositionInfo(EnemySpawnPositionRuntimeInfo spawnPointInfo)
			{
				EnemyARPGCharacterBehaviour behaviour =
					SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
						.SpawnNewEnemyBySingleConfigEntry(perSpawnEntry,spawnPointInfo.GetCurrentSpawnPosition(),
							spawnPointInfo);
				if (perSpawnEntry.EventConfigListRef_Single != null &&
				    perSpawnEntry.EventConfigListRef_Single.Count > 0)
				{
					foreach (var perEvent in perSpawnEntry.EventConfigListRef_Single)
					{

						var ds_ge = new DS_GameplayEventArguGroup();
						var pos = behaviour.transform.position;
						ds_ge.FloatArgu1 = pos.x;
						ds_ge.FloatArgu2 = pos.y;
						ds_ge.FloatArgu3 = pos.z;
						GameplayEventManager.Instance.StartGameplayEvent(perEvent, ds_ge);
					}
				}
				if (perSpawnEntry.RawSpawnInfo.ContainAddons)
				{
					behaviour.ApplyAddonAfterSpawn(perSpawnEntry.RawSpawnInfo.GetRuntimeFinalAddonList(), spawnPointInfo.GetCurrentSpawnPosition());
				}

				//如果ESA没有包含选行为模式的，那就选个默认，否则就跳过
				if (perSpawnEntry.WithAddon && perSpawnEntry.RawSpawnInfo.ContainAddons)
				{
					var esa_switchBP = perSpawnEntry.RawSpawnInfo.GetRuntimeFinalAddonList().FindIndex((addon =>
						addon is ESA_调整AI行为模式_SwitchAIBehaviourPattern));
					if (esa_switchBP != -1)
					{
						var addon =
							perSpawnEntry.RawSpawnInfo.GetRuntimeFinalAddonList()[esa_switchBP] as
								ESA_调整AI行为模式_SwitchAIBehaviourPattern;
						behaviour.GetAIBrainRuntimeInstance().BrainHandlerFunction
							.SwitchBehaviourPattern(addon.TargetBehaviourPatternID);
					}
					else
					{
						var esa_patrol = perSpawnEntry.RawSpawnInfo.GetRuntimeFinalAddonList().FindIndex((addon =>
							addon is ESA_进行巡逻_EnterAsPatrol));
						if (esa_patrol != -1)
						{
							 var addon = perSpawnEntry.RawSpawnInfo.GetRuntimeFinalAddonList()[esa_patrol] as ESA_进行巡逻_EnterAsPatrol;
							 behaviour.GetAIBrainRuntimeInstance().BrainHandlerFunction
								 .SwitchBehaviourPattern(addon.TargetBehaviourPatternID);
						}
						else
						{
							behaviour.GetAIBrainRuntimeInstance()?.BrainHandlerFunction.PickDefaultBehaviourPattern();
						}
					}
				}
				else
				{
					behaviour.GetAIBrainRuntimeInstance()?.BrainHandlerFunction.PickDefaultBehaviourPattern();
				}
				// GenericPool<EnemySpawnService_SubActivityService.SingleSpawnInfo>.Release(perSpawnEntry);

				
			}


		}

		public virtual void FixedUpdateTick(
			float ct,
			int cf,
			float delta)
		{
			for (int i = _relatedSingleSpawnInfoListRef.Count - 1; i >= 0; i--)
			{
				EnemySpawnService_SubActivityService.SingleSpawnInfo perSpawnEntry = _relatedSingleSpawnInfoListRef[i];
				//23.10.19:生成点位本身不再有CD这个业务，只要记录的生成点时间到了，那就生成。不会试图避免重叠。想要避免重叠那需要在配置上自行处理
				if (ct > (perSpawnEntry.WillSpawnTime + InitDelayDuration))
				{
					ConcreteSpawnProgress(perSpawnEntry);

					_relatedSingleSpawnInfoListRef.RemoveAt(i);
				} //没有指定位置或者指定位置正忙碌
				else
				{
					continue;
				}
			}
		}





#if UNITY_EDITOR
		protected virtual bool _Editor_CollectionInputValidCheck()
		{
			return true;
		}
#endif
		/// <summary>
		/// <para>当SpawnHandler标记已完成的时候，会被丢到待清理容器</para>
		/// </summary>
		/// <returns></returns>
		protected abstract void CheckIfSpawnHandlerFinished();

		/// <summary>
		/// <para>接收从 EnemySpawnService发起的调用，为ESS整体的容器填充内容，相当于实例化后的初始化业务</para>
		/// </summary>
		public virtual void ProcessConcreteSpawnInfoWithWaitSpawnInfoDict(
			Dictionary<SOConfig_SpawnEnemyConfig, List<EnemySpawnService_SubActivityService.SingleSpawnInfo>>
				spawnInfoDict)
		{
			_relatedSingleSpawnInfoListRef = new List<EnemySpawnService_SubActivityService.SingleSpawnInfo>();
			_relatedSingleSpawnInfoListRef.Clear();
			spawnInfoDict.Add(RelatedSpawnConfigRuntimeRef, _relatedSingleSpawnInfoListRef);
			foreach (SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo perEnemyTypeInfo in EnemySpawnCollection_RuntimeFinal)
			{
				for (int i = 0; i < perEnemyTypeInfo.SpawnCount; i++)
				{
					AddNewSingleSpawnInfoToListByPerEnemyTypeInfo(perEnemyTypeInfo,1);
				}
			}
		}
		/// <summary>
		/// <para>将单条信息传入等待生成。这个是带生成数量的！，</para>
		/// </summary>
		public virtual void AddNewSingleSpawnInfoToListByPerEnemyTypeInfo(
			SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo perEnemyTypeInfo,
			Nullable<int> count = null, SimpleSpawnPositionInfo spawnPositionInfo = null)
		{
			var spawnPositionID = string.Empty;
			var newPresetType = perEnemyTypeInfo.SpawnPresetType;
			if (perEnemyTypeInfo.SpawnPresetType == EnemySpawnPoint_PresetTypeEnum.NonRepeatCollection_不重复的集合)
			{
				//挑选，有可能并没有成功

				//对于[不重复的集合]，有可能生成失败
				//扫位置
			
				List<string> validPositionList = CollectionPool<List<string>, string>.Get();
				validPositionList.Clear();
				var allCharacter = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
					.CurrentAllActiveARPGCharacterBehaviourCollection;

				foreach (string perID in perEnemyTypeInfo.SpecificPointIDList)
				{
					if (allCharacter.Exists((behaviour =>
						behaviour is EnemyARPGCharacterBehaviour enemy && enemy.CharacterDataValid &&
						enemy.RelatedSpawnConfigInstance_SingleSpawnInfo.SpecificSpawnPositionUID.Equals(perID))))
					{
						continue;
					}

					if (_relatedSingleSpawnInfoListRef.Exists((info => info.SpecificSpawnPositionUID.Equals(perID))))
					{
						continue;
					}
					
					
					
					validPositionList.Add(perID);
					
				}
				//都没有
				
				if (validPositionList.Count == 0)
				{
					//如果是[已满时跳过]，那就跳过
					if (perEnemyTypeInfo.SkipWhenFull)
					{
						validPositionList.Clear();
						CollectionPool<List<string>, string>.Release(validPositionList);
						return;
					}
					//如果是[已满时随机]，那就随机
				
							List<string> ppList = CollectionPool<List<string>, string>.Get();
							ppList.Clear();
							ppList.AddRange(perEnemyTypeInfo.SpecificPointIDList);
							ppList.Shuffle();
							spawnPositionID = ppList[0];
							CollectionPool<List<string>, string>.Release(ppList);
					
					
				}
				//有那么一个，看看是顺序选还是随机选
				else
				{
					if (perEnemyTypeInfo.RandomOrOrder)
					{
						List<string> ppList2 = CollectionPool<List<string>, string>.Get();
						ppList2.Clear();
						ppList2.AddRange(validPositionList);
						ppList2.Shuffle();
						spawnPositionID = ppList2[0];
						CollectionPool<List<string>, string>.Release(ppList2);
					}
					else
					{
						spawnPositionID = validPositionList[0];
					}
				}
				newPresetType = EnemySpawnPoint_PresetTypeEnum.Specific_指定专用;
				validPositionList.Clear();
				CollectionPool<List<string>, string>.Release(validPositionList);
			}
			else
			{
				spawnPositionID = perEnemyTypeInfo.SpecificPointID;
			}
			
			var newInfo = new EnemySpawnService_SubActivityService.SingleSpawnInfo();
			newInfo.RelatedConfigInstance = RelatedSpawnConfigRuntimeRef;
			newInfo.RawSpawnInfo = perEnemyTypeInfo;
			newInfo.EventConfigListRef_Single = perEnemyTypeInfo.EventConfigList_Single;
			newInfo.EventConfigListRef_Once = perEnemyTypeInfo.EventConfigList_Once;
			newInfo.CharacterSelfVariantScaleMul = perEnemyTypeInfo.CharacterSelfVariantScaleMultiplier;

			newInfo.SpecificSpawnPositionUID = spawnPositionID;
			newInfo.SpawnPresetPositionType = newPresetType;


			if (spawnPositionInfo != null)
			{
				spawnPositionInfo.SpawnPresetType = perEnemyTypeInfo.SpawnPresetType;
				newInfo.SpecificSpawnPositionUID = spawnPositionInfo.SpecificPointID;
			}

			newInfo.WillSpawnTime = perEnemyTypeInfo.DelayTime + BaseGameReferenceService.CurrentFixedTime;
			newInfo.SpawnCountOverride = count;
			_relatedSingleSpawnInfoListRef.Add(newInfo);

		}



		public virtual void ClearBeforeDestroy()
		{
			foreach (SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo perEI in EnemySpawnCollection_RuntimeFinal)
			{
				perEI.GetRuntimeFinalAddonList().Clear();
			}
			EnemySpawnCollection_RuntimeFinal.Clear();

			// foreach (var d in EnemySpawnCollection_RuntimeFinal)
			// {
			// 	//release 
			//
			// 	if (d.ContainAddons && d.AddonList != null && d.AddonList.Count > 0)
			// 	{
			// 		foreach (BaseEnemySpawnAddon perAddon in d.AddonList)
			// 		{
			// 			perAddon.ResetOnReturn();
			// 		}
			// 		// CollectionPool<List<BaseEnemySpawnAddon>, BaseEnemySpawnAddon>.Release(d.AddonList);
			// 	}
			// 	// GenericPool<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>.Release(d);
			// 	
			// }

			//release Collection
			// CollectionPool<List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>, SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>
			// 	.Release(Collection);

			//同时包含业务：把自己产生的那个List也退了清了
			// CollectionPool<List<EnemySpawnService_SubActivityService.SingleSpawnInfo>,
			// 	EnemySpawnService_SubActivityService.SingleSpawnInfo>.Release(_relatedSingleSpawnInfoListRef);


			if (ContainReinforcement)
			{
				if(ReinforcementCheckOnClearOrSingleDie)
				{

					GlobalActionBus.GetGlobalActionBus().RemoveAction(
						ActionBus_ActionTypeEnum.G_Activity_ARPG_PartiallyClearAreaEnemy_活动ARPG_区域内的敌人部分清除了,
						_ABC_CheckIfNeedInstantiateReinforcement_OnAreaPartiallyClear);
				}
				else
				{
					GlobalActionBus.GetGlobalActionBus().RemoveAction(
						ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
						_ABC_CheckIfNeedInstantiateReinforcement_OnSingleEnemyDie);
					GlobalActionBus.GetGlobalActionBus().RemoveAction(
						ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
						_ABC_CheckIfNeedInstantiateReinforcement_OnSingleEnemyDie);
				}
				
			}
			
		}








#region 增援


		[LabelText("包含[增援]吗"), FoldoutGroup("配置", true)]
		public bool ContainReinforcement;

		[LabelText("√:增援于清怪时触发 | 口:增援于单个敌人被清除时触发"), FoldoutGroup("配置", true)]
		[ShowIf(nameof(ContainReinforcement))]
		public bool ReinforcementCheckOnClearOrSingleDie = true;
		
		[LabelText("    剩余几个敌人时触发增援")] [HideIf(nameof(ReinforcementCheckOnClearOrSingleDie)), FoldoutGroup("配置", true)]
		public int ReinforcementSpawnCount = 4;

		[NonSerialized]
		public bool ReinforcementSpawned;
		

		[SerializeField, LabelText("    增援的生成配置"), ShowIf(nameof(ContainReinforcement)), FoldoutGroup("配置", true)]
		public SOConfig_SpawnEnemyConfig ReinforcementSpawnConfig;



		private void _ABC_CheckIfNeedInstantiateReinforcement_OnAreaPartiallyClear(DS_ActionBusArguGroup ds)
		{
			var areaID = ds.ObjectArguStr as string;
			var ds_requireSpawnAsReinforcement = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.G_Activity_ARPG_RequireNewEnemySpawnConfig_活动ARPG_要求生成新的敌人配置);
			ds_requireSpawnAsReinforcement.ObjectArgu1 = ReinforcementSpawnConfig;
			ds_requireSpawnAsReinforcement.ObjectArgu2 =
				RelatedSpawnConfigRuntimeRef.RelatedSpawnEnemyGameplayEventHandlerRef;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_requireSpawnAsReinforcement);
		}

		private void _ABC_CheckIfNeedInstantiateReinforcement_OnSingleEnemyDie(DS_ActionBusArguGroup ds)
		{
			var enemyRef = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;
			var areaRef = enemyRef.RelatedSpawnConfigInstance_SpawnPositionInfo.AreaByTriggerRef.AreaUID;
			if (!ReinforcementSpawned)
			{
				List<BaseARPGCharacterBehaviour> com = SubGameplayLogicManager_ARPG.Instance
					.CharacterOnMapManagerReference.CurrentAllActiveARPGCharacterBehaviourCollection;
				enemyRef.RelatedSpawnConfigInstance.ClearBroadcast = false;
				
				
				
				int dataValidAndSameArea = 0;
				foreach (BaseARPGCharacterBehaviour behaviour in com)
				{
					if (behaviour is not EnemyARPGCharacterBehaviour enemy)
					{
						continue;
					}
					if (enemy.CharacterDataValid &&
					    enemy.RelatedSpawnConfigInstance_SpawnPositionInfo.AreaByTriggerRef.AreaUID.Equals(areaRef))
					{
						dataValidAndSameArea += 1;
					}
				}
				if (dataValidAndSameArea >= ReinforcementSpawnCount)
				{
					return;
				}
				ReinforcementSpawned = true;

				GameReferenceService_ARPG.TimedTaskManagerInstance.AddTimeTask(((arg0, f) =>
					{
						var ds_requireSpawnAsReinforcement = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.G_Activity_ARPG_RequireNewEnemySpawnConfig_活动ARPG_要求生成新的敌人配置);
						ds_requireSpawnAsReinforcement.ObjectArgu1 = ReinforcementSpawnConfig;
						ds_requireSpawnAsReinforcement.ObjectArgu2 =
							RelatedSpawnConfigRuntimeRef.RelatedSpawnEnemyGameplayEventHandlerRef;
						GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_requireSpawnAsReinforcement);

					}),
					1);



			}
			
			
		}



#endregion
	}
}