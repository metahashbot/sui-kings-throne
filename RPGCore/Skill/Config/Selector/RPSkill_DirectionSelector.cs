using System;
using RPGCore.Skill.SkillSelector;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.Config.Selector
{
    /// <summary>
    /// <para>选择一个方向</para>
    /// <para>常用于矩形判定的技能和连续位移</para>
    /// <para>在ForceFullRange的时候使用DirectionIndicator；否则使用LineIndicator</para>
    /// </summary>
    [Serializable]
    public class  RPSkill_DirectionSelector : RPSkill_SelectorBase
    {
    
        [Header("选择距离限制")]
        public float SelectorRange = 4f;


        [Header("判定宽度。矩形宽度或位移时造成效果的宽度")]
        public float EffectWidth = 1.5f;

        [Header("一定要选择完整距离(圆周)上的位置吗？")]
        public bool ForceFullRange = false;

    
        [Header("方向指示器"),ShowIf(nameof(ForceFullRange))]
        public RPSkillIndicatorConfig_Direction DirectionSelectorConfig;


        [Header("线段指示器"), HideIf(nameof(ForceFullRange))]
        public RPSkillIndicatorConfig_Line LineSelectorConfig;



    }
}
namespace RPGCore.Skill.Config.Selector
{
}