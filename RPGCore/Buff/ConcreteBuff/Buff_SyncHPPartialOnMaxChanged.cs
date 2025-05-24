using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff
{
	[Serializable]
	[TypeInfoBox("同步HP最大值时变动到当前HP，并且为当前值设置上界为最大值\n")]
	public class Buff_SyncHPPartialOnMaxChanged : BaseRPBuff
	{

		private Float_RPDataEntry _entry_MaxHP;
		private Float_RPDataEntry _entry_CurrentHP;


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_SyncData);
			_entry_MaxHP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
			_entry_CurrentHP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			_entry_CurrentHP.SetUpperBound(_entry_MaxHP.CurrentValue);
			return ds;
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变, _ABC_SyncData);
			return base.OnBuffPreRemove();
		}


		private void _ABC_SyncData(DS_ActionBusArguGroup ds)
		{
			if (ds.ActionType != ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变)
			{
				return;
			}
			var entry = ds.ObjectArgu1 as Float_RPDataEntry;
			if (entry.RP_DataEntryType == RP_DataEntry_EnumType.HPMax_最大HP)
			{
				float beforeChange = ds.FloatArgu2.Value;
				float beforePartial = _entry_CurrentHP.CurrentValue / beforeChange;
				beforePartial = UnityEngine.Mathf.Clamp01(beforePartial);
				float afterChange = ds.FloatArgu1.Value;
				float afterValue = beforePartial * afterChange;
				_entry_CurrentHP.SetUpperBound(_entry_MaxHP.CurrentValue);
				if (float.IsNaN(afterValue))
				{
					_entry_CurrentHP.ResetDataToValue(_entry_MaxHP.CurrentValue);
				}
				else
				{
					_entry_CurrentHP.ResetDataToValue(afterValue);
				}
			}
		}
	}
}