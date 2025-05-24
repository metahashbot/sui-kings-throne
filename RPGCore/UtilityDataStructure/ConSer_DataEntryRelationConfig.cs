using System;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.UtilityDataStructure
{
    [Serializable]
    public class ConSer_DataEntryRelationConfig
    {
        [LabelText("关联谁"),ToggleButtons("关联接收方","关联释放方")]
        public bool RelatedToReceiver;

        [LabelText("数据项类型")]
        public RP_DataEntry_EnumType RelatedDataEntryType;

        [LabelText("数值的比率，1表示100%")]
        public float Partial = 1f;
        [LabelText("计算位置")]
        public ModifyEntry_CalculatePosition CalculatePosition = ModifyEntry_CalculatePosition.FrontAdd;

        [NonSerialized, ShowInInspector, ReadOnly, LabelText("缓存值")]
        public float CacheDataEntryValue;
    }
}
