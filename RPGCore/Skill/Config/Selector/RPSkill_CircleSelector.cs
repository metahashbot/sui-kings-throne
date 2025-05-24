using System;
using RPGCore.Skill.SkillSelector;
using UnityEngine;

namespace RPGCore.Skill.Config.Selector
{
    [Serializable]
    public class RPSkill_CircleSelector : RPSkill_SelectorBase
    {
        [Header("外圈的最大半径")]
        public float SelectorOuterRange = 5f;

        [Header("用于选择的圆圈的半径")] 
        public float SelectorInnerRage = 2f;

        public RPSkillIndicatorConfig_Position selfCircleIndicatorConfig;
    }
}
namespace RPGCore.Skill.Config.Selector
{
}