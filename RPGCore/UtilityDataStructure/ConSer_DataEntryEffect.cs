using System;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.UtilityDataStructure
{
    [Serializable]
    public class ConSer_DataEntryEffect
    {
    
        public RP_DataEntry_EnumType DataEntryTypeEnum;
        public float ArgValue1 = 1f;
        public float ArgValue2 = 1f;
        public string RemarkString;

        [Header("记得分正负！比如打伤害就是负的，恢复就是正的|若为百分比，1%就是0.01")]
        public float ModifyValue = 1;



        [Header("√:使用关联数据项| 口:使用纯数值")] 
        public bool InitializeValueAsPercent = false;

        [Header("数据项的计算位置——改变原始值/前加/前乘/后加/后乘：乘法写百分比而不是小数")]
        public ModifyEntry_CalculatePosition ModifyCalculatePosition = ModifyEntry_CalculatePosition.FrontAdd;

        [Header("使用什么数据项的百分比"),ShowIf(nameof(InitializeValueAsPercent))]
        public RP_DataEntry_EnumType InitializeValueFromEntry = RP_DataEntry_EnumType.None;

        [Header("√：上界的百分比 |  口：当前值的百分比"),ShowIf(nameof(InitializeValueAsPercent))] 
        public bool UpperBoundOrCurrentValue;

        [ShowIf(nameof(InitializeValueAsPercent))]
        [Header("√:为buff发起者的数据 |  口:使用来自自身的数据")]
        public bool InitializeValueFromOrigin = true;

        [Header("如果没有找到目标数据项，则退化为该值"),ShowIf(nameof(InitializeValueAsPercent))]
        public float FallbackValue = 1f;





    }
}