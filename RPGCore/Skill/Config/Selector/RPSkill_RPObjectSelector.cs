using System;
using RPGCore.Skill.SkillSelector;
using UnityEngine;

namespace RPGCore.Skill.Config.Selector
{
    /// <summary>
    /// <para>技能会选择一个RolePlay体系下的对象作为目标</para>
    /// <para>默认选择自己，如果不是自己，则会选离选择器最近的（指针或者指定方向）</para>
    /// </summary>
    [Serializable]
    public class RPSkill_RPObjectSelector : RPSkill_SelectorBase
    {
        [Header("选择距离限制")] public float SelectDistanceLimit = 1f;


        public RPSkillIndicatorConfig_Position selfObjectIndicatorConfig;
    }
}
namespace RPGCore.Skill.Config.Selector
{
}