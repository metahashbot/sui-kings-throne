using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_响应蓄力的版面生成_SpawnLayoutAffectByCharge  : PAEC_生成版面_SpawnLayout
	{


		[SerializeField, LabelText("√:包含对曲线X范围的修改 | 口:使用原本动画最大蓄力时长")]
		public bool IncludeModifyCurveXRange = false;

		[SerializeField, LabelText("曲线X：蓄力时长")]
		[ShowIf("@(this.IncludeModifyCurveXRange)")]
		public float TimeAtFullCharge = 5f;


		[Header("===蓄力===")]
		[SerializeField, LabelText("√:受前摇蓄力影响 | 口:受施法蓄力影响")]
		public bool AffectByPrepare = true;

		[SerializeField, LabelText("√:包含尺寸变动")]
		public bool IncludeSizeMultiply = true;

		[SerializeField, LabelText("曲线Y：蓄满的尺寸")]
		[ShowIf(nameof(IncludeSizeMultiply))]
		public float SizeMultiplyAtFullCharge = 3f;

		[SerializeField, LabelText("曲线：尺寸随蓄力变动")]
		[ShowIf(nameof(IncludeSizeMultiply))]
		public AnimationCurve SizeMultiplyCurve = AnimationCurve.Linear(0f, 0.6f, 1f, 1f);



		
		[InfoBox("曲线的计算结果会乘原本配置的攻击力。如果原本攻击力就是1，曲线采样结果为0.4，曲线y范围为4，那么最终攻击力为 1*4*0.4=1.6")]
		[SerializeField,LabelText("√:包含 攻击力 倍率 乘算")]
		public bool IncludeDamageMultiply = true;
		
		[SerializeField,LabelText("曲线Y：蓄满的乘算的倍率")]
		[ShowIf(nameof(IncludeDamageMultiply))]
		public float DamageMultiplyAtFullCharge = 4f;
		 
		[SerializeField,LabelText("曲线：伤害乘算倍率")]
		[ShowIf(nameof(IncludeDamageMultiply))]
		public AnimationCurve DamageMultiplyCurve = AnimationCurve.Linear(0f, 0.25f, 1f, 1f);
		 
		
		[SerializeField,LabelText("√:包含 生命周期 倍率 乘算")]
		public bool IncludeLifeTimeMultiply = true;
		 
		[SerializeField,LabelText("曲线Y：蓄满的乘算的倍率")]
		[ShowIf(nameof(IncludeLifeTimeMultiply))]
		public float LifeTimeMultiplyAtFullCharge = 2f;
		
		[SerializeField,LabelText("曲线：生命周期乘算倍率")]
		[ShowIf(nameof(IncludeLifeTimeMultiply))]
		public AnimationCurve LifeTimeMultiplyCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);




		public override string GetElementNameInList()
		{
			 
			return $"{GetBaseCustomName()} 响应蓄力的版面生成";
		}





	}
}