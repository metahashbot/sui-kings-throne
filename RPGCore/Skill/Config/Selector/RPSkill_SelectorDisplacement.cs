using System;
using RPGCore.Skill.SkillSelector;
using UnityEngine;

namespace RPGCore.Skill.Config.Selector
{
    [Serializable]
    public class RPSkill_SelectorDisplacement
    {
        [Header("√：闪现，口：冲刺")] public bool BlinkOrDash;

        [Header("位移造成的距离")]
        public float DisplacementDistance;

        [Header("自身位移持续的时间")] 
        public float DisplacementDuration;

        [Header("完成曲线，Blink的0为原地，1为目的地，其余为虚空")]
        public AnimationCurve DisplacementCurve;

        [Header("用于标记位移范围的指示器配置")]
        public RPSkillIndicatorConfig_Range selfDisplacementIndicatorConfig;
    }
}
namespace RPGCore.Skill.Config.Selector
{
}