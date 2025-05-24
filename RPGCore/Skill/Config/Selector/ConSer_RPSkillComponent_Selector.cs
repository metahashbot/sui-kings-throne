using System;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.SkillSelector;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Skill.Config.Selector
{
	[Serializable]
	public class ConSer_RPSkillComponent_Selector
	{
		[LabelText("需要外范围指示")]
		public bool NeedOutRangeIndicator = false;

		[ShowIf(nameof(NeedOutRangeIndicator)), LabelText("使用的外范围指示配置")]
		public RPSkillIndicatorConfig_Range OutRangeIndicatorConfig;
		
		[LabelText("选择方式")]
		public SelectorTypeEnum SelectorType = SelectorTypeEnum.None;



		/*
	 * 带有技能预位移的技能 不应当是自身位移了。即这个位移要么是“位移技能”，要么是“技能前跳”，
	 * 这两个是冲突的，它不能既前跳又位移。
	 * 不过可以既不前跳又不位移。
	 */
		[InfoBox("技能包括前跳位移——配置信息。\n" + "这个处理的不是技能自身的位移！技能自身的位移是通过代码实现的，这个配置不包括那些东西！\n" + "并且如果技能本身就是一个位移技能（冲撞、闪现、跳跃等等），这个就不应当勾选")]
		public bool ContainAnimationPreDisplacement = false;
		[ShowIf(nameof(ContainAnimationPreDisplacement))]
		public RPSkill_SelectorDisplacement AnimationPreDisplacementConfig;

		[ShowIf("@SelectorType == SelectorTypeEnum.Ring_有角度的扇形"), Header("扇形选择器，可以选择以自身为圆心、任意角大小的范围")]
		public RPSkill_SectorSelector selfRPSkillSectorSelector;

		[LabelText("对象选择器，根据指针选择一个特定对象，可有距离限制"), ShowIf("@SelectorType == SelectorTypeEnum.RPObject_一个对象")]
		public RPSkill_RPObjectSelector SelfRPSkillRPObjectSelector;
		[LabelText("方向选择器，根据指针位置选择一个方向，可以有宽度限制(矩形)"), ShowIf("@SelectorType == SelectorTypeEnum.Direction")]
		public RPSkill_DirectionSelector SelfRPSkillDirectionSelector;
		[LabelText("圆形选择器，在大圆内选择一个小圆"), ShowIf("@SelectorType == SelectorTypeEnum.Circle_大圆中的小圆")]
		public RPSkill_CircleSelector SelfRPSkillCircleSelector;


		public enum SelectorTypeEnum
		{
			None,
			Ring_有角度的扇形,
			Circle_大圆中的小圆,
			RPObject_一个对象,
			Direction,
		}
	}

}
namespace RPGCore.Skill.Config.Selector
{
}