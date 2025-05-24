using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using GameplayEvent;
using GameplayEvent.SO;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Buff.Requirement;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff
{
	[Serializable]
	public class Buff_血量触发事件_TriggerGameplayEventByCurrentHP : BaseRPBuff
	{



		[InfoBox("同时接受【BLP_关联血量的触发事件_LaunchEventRelateToHP】的传递")]
		[SerializeField, LabelText("预设的根据HP触发事件参数")]
		[ShowInInspector, FoldoutGroup("配置",true)]
		public List<LaunchEventInfo_WithHP> _launchEventList;


		protected FloatPresentValue_RPDataEntry _hpPresentValueEntryRef;
		protected Float_RPDataEntry _maxHPDataEntryRef;
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_launchEventList = CollectionPool<List<LaunchEventInfo_WithHP>, LaunchEventInfo_WithHP>.Get();
			_launchEventList.Clear();
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_ProcessHPChanged_OnPresentValueChanged,
				-1);
			_hpPresentValueEntryRef = parent.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			_maxHPDataEntryRef = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);

			switch (blp)
			{
				case BLP_关联血量的触发事件_LaunchEventRelateToHP le:
					foreach (var perEvent in le.InfoList)
					{
						_launchEventList.Add( LaunchEventInfo_WithHP.GetRuntimeCopy(perEvent));
					}
					break;
			}
		}

		// public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		// {
		// 	if (blps != null)
		// 	{
		// 		foreach (BaseBuffLogicPassingComponent perBLP in blps)
		// 		{
		// 			if (perBLP is BLP_关联血量的触发事件_LaunchEventRelateToHP le)
		// 			{
		// 				foreach (LaunchEventInfo_WithHP perInfo in le.InfoList)
		// 				{
		// 					_launchEventList.Add(LaunchEventInfo_WithHP.DeepCopy(perInfo));
		// 				}
		// 			}
		// 		}
		// 	}
		//
		// 	return base.OnBuffInitialized(blps);
		// }


		protected void _ABC_ProcessHPChanged_OnPresentValueChanged(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is not FloatPresentValue_RPDataEntry floatPresentValueRPDataEntry)
			{
				return;
			}
			if (floatPresentValueRPDataEntry.RP_DataEntryType != RP_DataEntry_EnumType.CurrentHP_当前HP)
			{
				return;
			}

			foreach (LaunchEventInfo_WithHP perEventInfo in _launchEventList)
			{
				if (perEventInfo.CanLaunchCount != -1 && perEventInfo.CanLaunchCount <= 0)
				{
					continue;
				}
				//按当前生命值比对
				if (perEventInfo.CompareWithCurrentValue)
				{
					float currentValue = _hpPresentValueEntryRef.CurrentValue;
					switch (perEventInfo.CompareMethod)
					{
						case CompareMethodEnum.Less_小于:
							if (!(currentValue < perEventInfo.CompareAbsoluteValue))
							{
								continue;
							}
							break;
						case CompareMethodEnum.LessOrEqual_小于等于:
							if (!(currentValue <= perEventInfo.CompareAbsoluteValue))
							{
								continue;
							}
							break;
						case CompareMethodEnum.Equal_等于:
							if (!(Mathf.Approximately(currentValue, perEventInfo.CompareAbsoluteValue)))
							{
								continue;
							}
							break;
						case CompareMethodEnum.LargerOrEqual_大于等于:
							if (!(currentValue >= perEventInfo.CompareAbsoluteValue))
							{
								continue;
							}
							break;
						case CompareMethodEnum.Larger_大于:
							if (!(currentValue > perEventInfo.CompareAbsoluteValue))
							{
								continue;
							}
							break;
					}
					LaunchEvent(perEventInfo);
				}
				//按比例比对
				else
				{
					float currentValue =
						(_hpPresentValueEntryRef.CurrentValue / _maxHPDataEntryRef.CurrentValue) * 100f;
					switch (perEventInfo.CompareMethod)
					{
						case CompareMethodEnum.Less_小于:
							if (!(currentValue < perEventInfo.ComparePartial))
							{
								continue;
							}
							break;
						case CompareMethodEnum.LessOrEqual_小于等于:
							if (!(currentValue <= perEventInfo.ComparePartial))
							{
								continue;
							}
							break;
						case CompareMethodEnum.Equal_等于:
							if (!(Mathf.Approximately(currentValue, perEventInfo.ComparePartial)))
							{
								continue;
							}
							break;
						case CompareMethodEnum.LargerOrEqual_大于等于:
							if (!(currentValue >= perEventInfo.ComparePartial))
							{
								continue;
							}
							break;
						case CompareMethodEnum.Larger_大于:
							if (!(currentValue > perEventInfo.ComparePartial))
							{
								continue;
							}
							break;
					}
					LaunchEvent(perEventInfo);
				}
			}

			void LaunchEvent(LaunchEventInfo_WithHP eventInfo)
			{
				eventInfo.CanLaunchCount -= 1;

				var ds_spawn = new DS_GameplayEventArguGroup();

				if (Parent_SelfBelongToObject is BaseARPGCharacterBehaviour character)
				{
					ds_spawn.ObjectArgu1 = character;
				}
				foreach (SOConfig_PrefabEventConfig perEvent in eventInfo.EventList)
				{
					GameplayEventManager.Instance.StartGameplayEvent(perEvent, ds_spawn);
				}
			}
		}



		protected override void ClearAndUnload()
		{
			base.ClearAndUnload();
		
			_launchEventList.Clear();

			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_ProcessHPChanged_OnPresentValueChanged);
		}
	}
}