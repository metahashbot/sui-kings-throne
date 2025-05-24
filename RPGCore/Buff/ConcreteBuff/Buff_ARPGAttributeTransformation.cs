using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
namespace RPGCore.Buff.ConcreteBuff
{
	[Serializable]
	[TypeInfoBox("配置监听一个L_DataEntry_OnDataEntryValueChanged_数据项的值被改变")]
	public class Buff_ARPGAttributeTransformation : BaseRPBuff
	{
        [Serializable]
        public class InfluencedAttribute
        {
            [LabelText("影响的属性")]
            public RP_DataEntry_EnumType AttributeType;
            [LabelText("影响系数")]
            public float InfluenceCoefficient;
        }

        [FoldoutGroup("计算参数", true)]
        [LabelText("自身属性")]
        public RP_DataEntry_EnumType SelfType;

        [FoldoutGroup("计算参数", true)]
        [LabelText("受影响的一级属性列表")]
		public List<InfluencedAttribute> Attributes;

		[FoldoutGroup("计算参数",true)]
		[LabelText("计算位置")]
		public ModifyEntry_CalculatePosition CalculatePosition = ModifyEntry_CalculatePosition.RearAdd;

        private Float_RPDataEntry selfEntry;
		private Dictionary<RP_DataEntry_EnumType, Float_RPDataEntry> dataEntrys;
        private Dictionary<RP_DataEntry_EnumType, Float_ModifyEntry_RPDataEntry> modifyEntrys;

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);

            selfEntry = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(SelfType); 
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction( 
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_ProcessSTRChanged);

			dataEntrys = new Dictionary<RP_DataEntry_EnumType, Float_RPDataEntry>();
			modifyEntrys = new Dictionary<RP_DataEntry_EnumType, Float_ModifyEntry_RPDataEntry>();
            foreach (var attribute in Attributes)
			{
				var dataEntry = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(attribute.AttributeType);
                var modifyEntry = Float_ModifyEntry_RPDataEntry.GetNewFromPool(
					0, RPDM_DataEntry_ModifyFrom.FromBuff_Buff, CalculatePosition);
				dataEntry.AddDataEntryModifier(modifyEntry);
				dataEntrys.Add(attribute.AttributeType, dataEntry);
				modifyEntrys.Add(attribute.AttributeType, modifyEntry);
            }
            RefreshRelatedDataEntry();

            return ds;
		}

		private void _ABC_ProcessSTRChanged(DS_ActionBusArguGroup ds)
		{
			if (ds.ActionType != ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变)
			{
				return;
			}
			if ((RP_DataEntry_EnumType)ds.IntArgu1.Value != SelfType)
			{
				return;
			}
			RefreshRelatedDataEntry();
		}

		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);
			RefreshRelatedDataEntry();
			return ds;
		}

		private void RefreshRelatedDataEntry()
		{
            foreach (var attribute in Attributes)
            {
				var value = attribute.InfluenceCoefficient * selfEntry.CurrentValue;
                modifyEntrys[attribute.AttributeType].ModifyValue = value;
                dataEntrys[attribute.AttributeType].Recalculate();
            }
        }
	}
}