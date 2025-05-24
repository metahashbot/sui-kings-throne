using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_响应前摇蓄力的特效配置_SpawnVFXConfigAffectByCharge : PAEC_生成特效配置_SpawnVFXFromConfig
	{


		[Header("===蓄力===")]
		[SerializeField, LabelText("√:受前摇蓄力影响 | 口:受施法蓄力影响")]
		public bool AffectByPrepare = true;

		[SerializeField, LabelText("√:包含尺寸增加")]
		public bool IncludeSizeMultiply = true;

		[SerializeField, LabelText("曲线Y：蓄满的增加的尺寸")]
		[ShowIf(nameof(IncludeSizeMultiply))]
		public float SizeMultiplyAtFullCharge = 3f;

		[SerializeField, LabelText("曲线：增加的额外尺寸")]
		[ShowIf(nameof(IncludeSizeMultiply))]
		public AnimationCurve SizeMultiplyCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[SerializeField, LabelText("√:包含对曲线X范围的修改 | 口:使用原本动画最大蓄力时长")]
		[ShowIf(nameof(IncludeSizeMultiply))]
		public bool IncludeModifyCurveXRange = false;

		[SerializeField, LabelText("曲线X：蓄力时长")]
		[ShowIf("@(this.IncludeSizeMultiply && this.IncludeModifyCurveXRange)")]
		public float TimeAtFullCharge = 5f;


		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 响应前摇蓄力的特效配置";
		}



	}
}