using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_调整作用中时长的位移_StartDisplacementAndModifyEffectDuration : PAEC_开始一段位移_StartDisplacement
	{
		[Header("===调整作用中时长===")]
		[SerializeField,LabelText("曲线Y：最大持续时间 额外乘数")]
		public float  MaxAffectDuration = 1.4f;
		
		[SerializeField,LabelText("【持续时间】的曲线")]
		public AnimationCurve ModifyDurationCurve = AnimationCurve.Linear(0f, 0.35f, 1f, 1f);
		
		
		[Header("===")]
		[SerializeField,LabelText("曲线Y：最大位移距离 额外乘数")]
		public float  MaxDisplacementDistance = 1.7f;
		
		 [SerializeField,LabelText("【位移距离】的曲线")]
		public AnimationCurve ModifyDisplacementDistanceCurve = AnimationCurve.Linear(0f, 0.35f, 1f, 1f);
		
		

		[SerializeField, LabelText("√:包含对曲线X范围的修改 | 口:使用原本动画最大蓄力时长")]
		public bool IncludeModifyCurveXRange = false;

		[SerializeField, LabelText("曲线X：蓄力时长")]
		[ShowIf("@(this.IncludeModifyCurveXRange)")]
		public float TimeAtFullCharge = 5f;
		
		
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 调整作用中时长的位移";
		}
		
		

	}
}