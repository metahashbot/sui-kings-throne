using System;
using System.Collections.Generic;
using UnityEngine;
namespace RPGCore.Skill
{
	[Serializable]
	public class DisplacementSkillInfo
	{

		public List<DisplacementSkillInfo_Segment> SelfSegmentList;

		public class DisplacementSkillInfo_Segment
		{
			/// <summary>
			/// <para>此次位移的起始点</para>
			/// </summary>
			public Vector3 DisplacementSegmentFromPosition;
			/// <summary>
			/// <para>此次位移的目标点</para>
			/// </summary>
			public Vector3 DisplacementSegmentTargetPosition;
			/// <summary>
			/// <para>完成度曲线，X轴表示持续时间的绝对值，Y为[0,1]的完成度</para>
			/// </summary>
			public AnimationCurve DisplacementSegmentCurve;

			/// <summary>
			/// <para>该片段持续的时间</para>
			/// </summary>
			public float DisplacementSegmentDuration;

			/// <summary>
			/// <para>该片段开始的时间点</para>
			/// </summary>
			public float SegmentStartTime;


		}

	}

	/// <summary>
	/// <para>表明这是一个位移技能（释放的时候总是会对自身造成位移），包含若干段位移。</para>
	/// </summary>
	public interface I_SkillIsDisplacementSkill
	{
		public DisplacementSkillInfo RelatedDisplacementSkillInfo { get; protected set; }
	}
}