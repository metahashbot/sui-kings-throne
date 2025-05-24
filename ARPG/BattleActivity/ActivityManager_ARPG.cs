using System;
using System.Collections.Generic;
using ARPG.Character.Config;
using ARPG.Config.BattleLevelConfig;
using ARPG.DropItem;
using ARPG.Manager.Component;
using ARPG.Manager.Config;
using Global;
using Global.ActionBus;
using Global.AreaOnMap;
using Global.AreaOnMap.EditorProxy;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager
{
	[TypeInfoBox("ARPG中，战斗活动管理\n" + "管理：处于|脱离 战斗状态，开不开怪等")]
	public class ActivityManager_ARPG : MonoBehaviour
	{
#if UNITY_EDITOR
		// redraw constantly
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }

#endif
		public static ActivityManager_ARPG Instance;

		[ShowInInspector, NonSerialized, LabelText("当前活跃的战斗活动组件们"), FoldoutGroup("运行时", true)]
		public List<BaseSubActivityService> CurrentBattleActivityComponents;
		[ShowInInspector, NonSerialized, LabelText("缓存_生成信息"), FoldoutGroup("运行时", true)]
		public EnemySpawnService_SubActivityService EnemySpawnServiceSubActivityServiceComponentRef;
		[ShowInInspector, NonSerialized, LabelText("区域信息"), FoldoutGroup("运行时", true)]
		public AreaInfo_SubActivityService AreaInfoSubActivityServiceComponentRef;
		[ShowInInspector, NonSerialized, LabelText("当前掉落控制信息"), FoldoutGroup("运行时", true)]
		public DropItemService_SubActivity DropItemServiceSubActivity;
        [ShowInInspector, NonSerialized, LabelText("房间控制系统"), FoldoutGroup("运行时", true)]
        public RoomController_SubActivity RoomControllerSubActivity;
        [ShowInInspector, NonSerialized, LabelText("战斗结算系统"), FoldoutGroup("运行时", true)]
        public BattleConclusionService_SubActivity BattleConclusionServiceSubActivity;
        [ShowInInspector, NonSerialized, LabelText("场景内所有触发区域"), FoldoutGroup("运行时", true)]
		public Dictionary<string, EP_AreaByTrigger> AllAreaByTriggerDict = new Dictionary<string, EP_AreaByTrigger>();

		/// <summary>
		/// 所有的场景物件UID到场景物件的映射
		/// <para>第一个Key是区域的名字。如果这个Object在加载注册的时候，在Hierarchy就属于某个Area之下，那这个key就是那个Area的名字</para>
		/// <para>		如果它上方没有任何一个Area，那就是"default"区域</para>
		/// 
		/// </summary>
		protected Dictionary<string, Dictionary<string, GameObject>> AllUID2SceneObjectDict = new();

        public GameObject GetSceneObjectByObjectIDAndAreaID(string objectID, string areaID = "default")
		{
			if (!AllUID2SceneObjectDict.ContainsKey(areaID))
			{
				DBug.LogError($"没有这个区域:{areaID}");
				return null;
			}

			var Area = AllUID2SceneObjectDict[areaID];
			if (Area.ContainsKey(objectID.Trim()))
			{
				return Area[objectID.Trim()];
			}
			else
			{
				DBug.LogError($"没有这个物件:{objectID}");
				return null;
			}

			return AllUID2SceneObjectDict[areaID][objectID];
		}		
		
		

		public void AwakeInitialize(SubGameplayLogicManager_ARPG glm)
		{
			Instance = this;
			BaseSubActivityService.StaticInitialize(this);
			CurrentBattleActivityComponents = new List<BaseSubActivityService>();
			EnemySpawnServiceSubActivityServiceComponentRef = new EnemySpawnService_SubActivityService();
			EnemySpawnServiceSubActivityServiceComponentRef.AwakeInitialize();
			CurrentBattleActivityComponents.Add(EnemySpawnServiceSubActivityServiceComponentRef);
			AreaInfoSubActivityServiceComponentRef = new AreaInfo_SubActivityService();
			CurrentBattleActivityComponents.Add(AreaInfoSubActivityServiceComponentRef);
			DropItemServiceSubActivity = new DropItemService_SubActivity();
			DropItemServiceSubActivity.AwakeInitialize();
			CurrentBattleActivityComponents.Add(DropItemServiceSubActivity);
            RoomControllerSubActivity = new RoomController_SubActivity();
            RoomControllerSubActivity.AwakeInitialize();
			CurrentBattleActivityComponents.Add(RoomControllerSubActivity);
            BattleConclusionServiceSubActivity = new BattleConclusionService_SubActivity();
			BattleConclusionServiceSubActivity.AwakeInitialize();
			CurrentBattleActivityComponents.Add(BattleConclusionServiceSubActivity);


            foreach (EP_AreaByTrigger perArea in FindObjectsOfType<EP_AreaByTrigger>(true))
			{
				AllAreaByTriggerDict.Add(perArea.AreaUID, perArea);
				perArea.AwakeInitialize();
            }
            foreach (var perPosition in FindObjectsOfType<EP_SceneObjectRegister>(true))
			{
				var parentArea = GetComponentInParentRecursively<EP_BaseArea>(perPosition.transform);
				var areaKey = "default";
				if (parentArea != null)
				{
					areaKey = parentArea.AreaUID;
				}
				if (!AllUID2SceneObjectDict.ContainsKey(areaKey))
				{
					AllUID2SceneObjectDict.Add(areaKey, new Dictionary<string, GameObject>());
				}
				var d2 = AllUID2SceneObjectDict[areaKey];
				if (d2.ContainsKey(perPosition.SceneObjectUID))
				{
					DBug.LogError($"场景物件UID重复了:{perPosition.SceneObjectUID}");
					continue;
				}
				else
				{
					perPosition.RelatedAreaUID = areaKey;
					d2.Add(perPosition.SceneObjectUID, perPosition.gameObject);
				}
			}
		}

		T GetComponentInParentRecursively<T>(UnityEngine.Component c) where T : MonoBehaviour
		{
			if (c == null)
			{
				return null;
			}
			T comp = c.GetComponent<T>();
			if (comp != null)
			{
				return comp as T;
			}
			return GetComponentInParentRecursively<T>(c.transform.parent);
		}



		public void StartInitialize()
		{
			AreaInfoSubActivityServiceComponentRef.Initialize();
			GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_ARPG_lateLoadDone,
				_ABC_ComplementRegisterObject_OnLateLoadDone);

		}


		public void LateLoadInitialize()
		{
			foreach (var perArea in AllAreaByTriggerDict.Values)
			{
				perArea.LateLoadInitialize(AreaInfoSubActivityServiceComponentRef);
			}
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_Activity_ARPG_RequireNewDropItemInfoConfig_活动ARPG_要求使用新的掉落道具配置,
				_ABC_AddDropItemConfig_OnRequireNewDropItemInfoConfig);
			EnemySpawnServiceSubActivityServiceComponentRef.LateLoadInitialize();
            RoomControllerSubActivity.LateLoadInitialize();
            BattleConclusionServiceSubActivity.LateLoadInitialize();
        }
		
		

		private void _ABC_ComplementRegisterObject_OnLateLoadDone(DS_ActionBusArguGroup ds)
		{
			var allFind =
				FindObjectsByType<EP_SceneObjectRegister>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (var perPosition in allFind)
			{
				var areaKey = "default";
		
				if (!AllUID2SceneObjectDict.ContainsKey(areaKey))
				{
					AllUID2SceneObjectDict.Add(areaKey, new Dictionary<string, GameObject>());
				}
				var d2 = AllUID2SceneObjectDict[areaKey];
				if (d2.ContainsKey(perPosition.SceneObjectUID))
				{
					continue;
				}
				else
				{
					perPosition.RelatedAreaUID = "default";
					d2.Add(perPosition.SceneObjectUID.Trim(), perPosition.gameObject);
				}
			}
			
		}
		public void UpdateTick(float ct, int cf, float delta)
		{
			foreach (BaseSubActivityService perComponent in CurrentBattleActivityComponents)
			{
				perComponent.UpdateTick(ct, cf, delta);
			}
		}

		public void FixedUpdateTick(float ct, int cf, float delta)
		{
		// 	foreach (var perArea in AllAreaByTriggerDict.Values)
		// 	{
		// 		perArea.FixedUpdateTick(ct, cf, delta);
		// 	}
			foreach (BaseSubActivityService perComponent in CurrentBattleActivityComponents)
			{
				perComponent.FixedUpdateTick(ct, cf, delta);
			}
		}


		protected virtual void _ABC_AddDropItemConfig_OnRequireNewDropItemInfoConfig(DS_ActionBusArguGroup ds)
		{
			var config = ds.ObjectArgu1 as List<SOConfig_DropItemControl>;
			if(config != null && config.Count>0)
			{
				foreach (SOConfig_DropItemControl perC in config)
                {
                    DropItemServiceSubActivity.InitializeDropItemControl(perC);
                }
			}
		}
	}
}