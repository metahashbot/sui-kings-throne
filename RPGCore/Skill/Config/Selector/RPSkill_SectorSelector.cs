using System;
using RPGCore.Skill.SkillSelector;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.Config.Selector
{
    /// <summary>
    /// <para>环形选择器：最通用的，</para>
    /// <para>内径为0的时候，即为 圆形</para>
    /// <para>角度不为360的时候，即为 披萨块型 </para>
    /// <para>内径不为0，则表明带初始位移</para>
    /// </summary>
    [Serializable]
    public class RPSkill_SectorSelector : RPSkill_SelectorBase
    {
        [LabelText("使用的技能指示器的类型，None表示不需要指示器")]
        public RPSkillIndicatorConfig_Sector SectorIndicatorConfig;
    

        [LabelText("外半径")]
        public float OuterRadius = 1f;
        [LabelText("角度")]
        public float Degree = 360f;



    }
}

namespace RPGCore.Skill.Config.Selector
{
}
