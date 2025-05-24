using System;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;

namespace RPGCore.Buff.Requirement
{
    /// <summary>
    /// <para>与数据项有关的触发条件</para>
    /// </summary>
    [Serializable]
    [InfoBox("比较成立时，即 自己的DataEntry  比较方法  CompareValue 为true  的时候\n" +
             "e.g1: 目标数据 '力量'，比较方法'大于'，'√'按值比对，比对数据[23]。 则当  当前  [力量] [大于] [23]的时候，算作比较通过\n" +
             "e.g2：目标数据 '当前血量'，比较方法 '小于'，'口'按百分比，目标比对数据'最大血量'，比对数据[44]。则当 [当前血量] [小于] [最大血量]*[44%] 的时候，算作比较通过\n")]
    public class RPBuffRequirement_DataEntry
    {
        [LabelText("比较目标数据类型")]
        public RP_DataEntry_EnumType DataEntryType = RP_DataEntry_EnumType.None;
        [LabelText("比较方法")]
        public CompareMethodEnum CompareMethod = CompareMethodEnum.None;
        /// <summary>
        /// <para>True为比对Value，False为按百分比比对</para>
        /// </summary>
        [LabelText("√：按值比对；口：按百分比比对其他数值")]
        public bool CompareAsValue = true;

        [HideIf(nameof(CompareAsValue)), LabelText("目标比对数据类型")]
        public RP_DataEntry_EnumType TargetType = RP_DataEntry_EnumType.None;

        [LabelText("比对数据：百分比时也是百分，50表示50%")]
        public float CompareValue = 1f;




    }

    public enum CompareMethodEnum
    {
        None = 0,
        Less_小于 = 1,
        LessOrEqual_小于等于 = 2,
        Equal_等于 = 3,
        LargerOrEqual_大于等于 = 4,
        Larger_大于 = 5,
        NotEqual_不等于 = 6,
    }
}