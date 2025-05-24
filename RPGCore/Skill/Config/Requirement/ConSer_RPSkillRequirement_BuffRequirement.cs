using System;
using RPGCore.Buff;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.Config.Requirement
{
    [Serializable]
    public class ConSer_RPSkillRequirement_BuffRequirement : ConSer_RPSkillRequirement_Base
    {
        [LabelText("检查Buff类型")]
        public RolePlay_BuffTypeEnum BuffType;

        [LabelText("√：必须是下面这个状态；口：不能是下面这个状态")]
        public bool NeedThisState = true;

        [LabelText("有效 | 存在但无效 | 超时 | 不存在")]
        public BuffAvailableType RequireType = BuffAvailableType.Available_TimeInAndMeetRequirement;


    }
}
namespace RPGCore.Skill.Config.Requirement
{
}