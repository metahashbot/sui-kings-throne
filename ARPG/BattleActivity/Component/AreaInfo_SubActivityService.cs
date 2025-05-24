using System;
using System.Collections.Generic;
using ARPG.Config.BattleLevelConfig;
using DefaultNamespace;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.EditorProxy;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager.Component
{


	[TypeInfoBox("区域信息。记录了，当前正在激活的区域们、历史激活的区域们。")]
	[Serializable]
	public class AreaInfo_SubActivityService : BaseSubActivityService , I_ProcessorOfPlayerCharacterInteraction
	{
		[Serializable]
		public class AreaHistoryInfo
		{
			[LabelText("区域UID")]
			public string AreaUID;
			[LabelText("√ 是进入  || 口 是离开")]
			public bool EnterArea;
			[LabelText("进入时间")]
			public float RecordTime;
		}


		[LabelText("区域历史记录信息")]
		public List<AreaHistoryInfo> AllAreaHistoryInfoList;


		[LabelText("当前玩家处于的区域们")]
		private List<AreaHistoryInfo> _currentActiveAreaList = new List<AreaHistoryInfo>();

		private List<EP_BaseArea> _currentAllBaseAreaList = new List<EP_BaseArea>();


		public T GetAreaRefByUIDAndType<T>(string areaUID) where T : EP_BaseArea
		{
			foreach (var perArea in _currentAllBaseAreaList)
			{
				if (perArea.AreaUID.Equals(areaUID, StringComparison.OrdinalIgnoreCase))
				{
					if (perArea is T)
					{
						return perArea as T;
					}
					else
					{
						DBug.LogError($"区域UID为{areaUID}的区域，类型不匹配，期望是{typeof(T).Name}，实际是{perArea.GetType().Name}");
					}
				}
			}

			DBug.LogError($"区域UID为{areaUID}的区域，没有找到");
		
			
			return null;
		}

		public bool? CheckIfPlayerInsideArea(string areaUID)
		{
			var findI = _currentAllBaseAreaList.FindIndex(area =>
					area.AreaUID.Equals(areaUID, StringComparison.OrdinalIgnoreCase))
				;
			if (findI == -1)
			{
				return null;
			}

			var area = _currentAllBaseAreaList[findI];
			return area.IsPlayerInArea();

		}
		
		public void Initialize()
		{
			var logicHelper = UnityEngine.Object.FindObjectOfType<EditorProxy_AreaLogicHolder>();
			foreach (var perArea in logicHelper.GetComponentsInChildren<EP_BaseArea>(true))
			{
				_currentAllBaseAreaList.Add(perArea);
			}
			
			AllAreaHistoryInfoList = new List<AreaHistoryInfo>();

			var gab = GlobalActionBus.GetGlobalActionBus();

			gab.RegisterAction(ActionBus_ActionTypeEnum.G_Area_OnEnterTriggerArea_当进入触发区域, _ABC_ProcessEnterArea, -999);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_Area_OnExitTriggerArea_当离开触发区域, _ABC_ProcessExitArea, -999);
			GlobalActionBus.GetGlobalActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.G_ARPG_lateLoadDone, _ABC_ProcessLateLoadDone_OnLateLoadDone);
			SubGameplayLogicManager_ARPG.Instance.SetPlayerCharacterInteractionProcessor(this);
		}


		private void _ABC_ProcessLateLoadDone_OnLateLoadDone(DS_ActionBusArguGroup ds)
		{




			foreach (EP_BaseArea perArea in _currentAllBaseAreaList)
			{
				perArea.ProcessFixedFunctionAndRegister_OnLateLoadDone();
			}

			
		}

		private void _ABC_ProcessEnterArea(DS_ActionBusArguGroup ds)
		{
			var triggerAreaUID = ds.ObjectArguStr as string;
			var triggerAreaProxy = ds.ObjectArgu1 as EP_AreaByTrigger;
			AreaHistoryInfo newHistoryEntry = new AreaHistoryInfo();

			newHistoryEntry.EnterArea = true;
			newHistoryEntry.RecordTime = BaseGameReferenceService.CurrentFixedTime;
			newHistoryEntry.AreaUID = triggerAreaUID;




			var ds_changed =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Area_OnTriggerAreaHistoryChanged_当触发区域的历史记录发生变化);
			ds_changed.ObjectArguStr = triggerAreaUID;
			ds_changed.ObjectArgu1 = newHistoryEntry;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_changed);

			//检查是否首次进入
			if (!AllAreaHistoryInfoList.Exists((info =>
				info.AreaUID.Equals(triggerAreaUID, StringComparison.OrdinalIgnoreCase))))
			{
				AllAreaHistoryInfoList.Add(newHistoryEntry);
				_currentActiveAreaList.Add(newHistoryEntry);

				var ds_first =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Area_OnTriggerAreaFirstEnter_当某个区域被首次进入);
				ds_first.ObjectArguStr = triggerAreaUID;
				ds_first.ObjectArgu1 = newHistoryEntry;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_first);
			}
			else
			{
				AllAreaHistoryInfoList.Add(newHistoryEntry);
				_currentActiveAreaList.Add(newHistoryEntry);
			}
		}


		private void _ABC_ProcessExitArea(DS_ActionBusArguGroup ds)
		{
			var triggerAreaUID = ds.ObjectArguStr as string;
			var triggerAreaProxy = ds.ObjectArgu1 as EP_AreaByTrigger;
			AreaHistoryInfo newHistoryEntry = new AreaHistoryInfo();

			newHistoryEntry.EnterArea = false;
			newHistoryEntry.RecordTime = BaseGameReferenceService.CurrentFixedTime;
			newHistoryEntry.AreaUID = triggerAreaUID;



			AllAreaHistoryInfoList.Add(newHistoryEntry);
			var findIndex = _currentActiveAreaList.FindIndex((info =>
				info.AreaUID.Equals(triggerAreaUID, StringComparison.OrdinalIgnoreCase)));
			if (findIndex == -1)
			{
				DBug.LogError($"玩家离开了一个区域，但是这个区域并不在当前活动区域列表中，这是不可能的");
			}
			else
			{
				_currentActiveAreaList.RemoveAt(findIndex);
			}


			var ds_changed =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_Area_OnTriggerAreaHistoryChanged_当触发区域的历史记录发生变化);
			ds_changed.ObjectArguStr = triggerAreaUID;
			ds_changed.ObjectArgu1 = newHistoryEntry;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_changed);
		}


		/// <summary>
		/// <para>获取当前玩家处于的区域列表们</para>
		/// </summary>
		/// <returns></returns>
		public List<AreaHistoryInfo> GetCurrentActiveAreaList()
		{
			return _currentActiveAreaList;
		}
		
		
		
		private EP_BaseArea _currentTriggerAreaRefBlock;
		private List<EP_SelfTriggerArea> _currentAllInsideAreaRefList = new List<EP_SelfTriggerArea>();

		EP_BaseArea I_ProcessorOfPlayerCharacterInteraction.CurrentTriggerAreaRefBlock
		{
			get => _currentTriggerAreaRefBlock;
			set => _currentTriggerAreaRefBlock = value;
		}
		List<EP_SelfTriggerArea> I_ProcessorOfPlayerCharacterInteraction.CurrentAllInsideAreaRefList
		{
			get => _currentAllInsideAreaRefList;
			set => _currentAllInsideAreaRefList = value;
		}
	}
}