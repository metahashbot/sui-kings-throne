using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common.HitEffect
{
	[Serializable]
	public class PerHitColorInfoPair
	{
		[SerializeReference, LabelText("提供生效前提")]
		public BaseVFXInfoContent Requirement;
		[SerializeField, LabelText("具体配置")]
		public HitColorEffectInfo HitColorEffectInfoContent;
	}

	[Serializable]
	public class HitColorEffectInfo
	{
		[ColorUsage(true, true)]
		[LabelText("目标颜色")]
		public Color TargetColor = Color.white;
		[LabelText("目标透明度")]
		public float TargetAlpha = 0.75f;
		[LabelText("目标持续时间")]
		public float Duration = 0.12f;
		[LabelText("渐变方式")]
		public Ease EaseType = Ease.OutCubic;
	}

}