using System;
using RPGCore.Buff.Requirement;
using Sirenix.OdinInspector;
using RPGCore.DataEntry;

namespace RPGCore.Skill.Config.Requirement
{
    [Serializable,InfoBox("比较方式。当 DataEntry 比较方式 CompareValue  为true时，通过需求")]
    public class ConSer_RPSkillRequirement_DataEntryRequirement : ConSer_RPSkillRequirement_Base
    {
        [LabelText("比对源数据类型")]
        public RP_DataEntry_EnumType DataEntryFrom;
        [LabelText("√：按百分比比对(需要有上界)；口：按数值比对")]
        public bool CompareByPartial = false;
        [LabelText("比对目标数据"),ShowIf(nameof(CompareByPartial))]
        public RP_DataEntry_EnumType DataEntryTarget;
        [LabelText("比对方式")]
        public CompareMethodEnum CompareMethod = CompareMethodEnum.None;
        [LabelText("比对目标值，如果是百分比则这是百分比，50表示50%")]
        public float CompareValue;

    }
}