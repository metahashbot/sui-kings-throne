using System;
using System.Collections.Generic;
using DG.Tweening;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.AssistBusiness
{
	[Serializable]
	[TypeInfoBox("用于跳字业务的配置信息")]
	[CreateAssetMenu(fileName = "跳字配置",menuName = "#SO Assets#/#Global Config#/跳字配置",order = -200)]
	public class SOConfig_DamageHintConfiguration : ScriptableObject
	{

		public virtual void InitBuild()
		{
			SelfDamageTypeAndColorLUTDict = new Dictionary<DamageTypeEnum, DamageColorAndTypeInfoPair>();
			foreach (var pair in DamageColorAndTypeInfoPairList)
			{
				SelfDamageTypeAndColorLUTDict.Add(pair.DamageType, pair);
			}
		}
		
		[Serializable]
		public class DamageColorAndTypeInfoPair
		{
			[LabelText("对应伤害类型")]
			public DamageTypeEnum DamageType;
			[LabelText("字体顶点颜色")]
			public Color FontVertexColor = Color.white;

			[LabelText("左上角的颜色"),HorizontalGroup("Line1")]
			public Color color0 = Color.white;

			[LabelText("右上角的颜色"),HorizontalGroup("Line1")]
			public Color color1 = Color.white;

			[LabelText("左下角的颜色"),HorizontalGroup("Line2")]
			public Color color2 = Color.white;

			[LabelText("右下角的颜色"),HorizontalGroup("Line2")]
			public Color color3 = Color.white;
		}

		[SerializeField, LabelText("一次跳字持续的整体时间"), SuffixLabel("秒")]
		public float AllDuration = 1.25f;


		[Title("=== 生成位置 ===", TitleAlignment = TitleAlignments.Centered)]
		[SerializeField, LabelText("在[世界上]方向上偏移范围曲线")]
		public AnimationCurve PopupPositionOffset_Up = new AnimationCurve(new Keyframe(0f, 0.35f),
			new Keyframe(0.5f, -0.5f),
			new Keyframe(1f, 0.35f));

		[NonSerialized]
		public float CurrentStep_Up;
		
		[SerializeField, LabelText("↑单次步进范围")]
		public Vector2 PopupPositionOffset_Up_Step = new Vector2(0.1f, 0.2f);


		public float GetCurrentStop_Up()
		{
			CurrentStep_Up += UnityEngine.Random.Range(PopupPositionOffset_Up_Step.x, PopupPositionOffset_Up_Step.y);
			if (CurrentStep_Up > 1f)
			{
				CurrentStep_Up -= 1f;
			}
			return PopupPositionOffset_Up.Evaluate(CurrentStep_Up);
		}

		
		

		[SerializeField, LabelText("在[世界右] 方向上偏移范围曲线")]
		public AnimationCurve PopupPositionOffsetRange_Right = new AnimationCurve(new Keyframe(0f, 0.35f),
			new Keyframe(0.5f, -0.5f),
			new Keyframe(1f, 0.35f));

		[NonSerialized]
		public float CurrentStep_Right;

		[SerializeField, LabelText("↑单次步进范围")]
		public Vector2 PopupPositionOffset_Right_Step = new Vector2(0.1f, 0.2f);
		
		
		public float GetCurrentStop_Right()
		{
			CurrentStep_Right += UnityEngine.Random.Range(PopupPositionOffset_Right_Step.x,
				PopupPositionOffset_Right_Step.y);
			if (CurrentStep_Right > 1f)
			{
				CurrentStep_Right -= 1f;
			}
			return PopupPositionOffsetRange_Right.Evaluate(CurrentStep_Right);
		}
		 

		[SerializeField, LabelText("起始高度 —— 高于伤害发生位置这么多")]
		public float PopYInitialOffset = 1f;

		[SerializeField, LabelText("到摄像机距离的修正——负得越多则越靠近屏幕")]
		public float PopZInitialOffset = -0.5f;

		[Title("=== 颜色 ===", TitleAlignment = TitleAlignments.Centered)]
		[SerializeField, LabelText("伤害类型和颜色的配置")]
		public List<DamageColorAndTypeInfoPair> DamageColorAndTypeInfoPairList = new List<DamageColorAndTypeInfoPair>()
		{
			new DamageColorAndTypeInfoPair
			{
				DamageType = DamageTypeEnum.NoType_无属性,
			}
		};
		
		public DamageColorAndTypeInfoPair GetColorAndTypeInfoByType(DamageTypeEnum type)
		{
			if (SelfDamageTypeAndColorLUTDict.ContainsKey(type))
			{
				return SelfDamageTypeAndColorLUTDict[type];
			}
			else
			{
				return SelfDamageTypeAndColorLUTDict[DamageTypeEnum.NoType_无属性];
			}
		}

		[NonSerialized]
		private Dictionary<DamageTypeEnum, DamageColorAndTypeInfoPair> SelfDamageTypeAndColorLUTDict;


		[SerializeField, LabelText("开始淡出的延迟_相对于刚刚生成")]
		public float OutDelay = 1f;


		[SerializeField, LabelText("淡出所用的时间")]
		public float OutDuration = 0.2f;


		[Title("=== 尺寸 ===", TitleAlignment = TitleAlignments.Centered)]
		[SerializeField, LabelText("字体尺寸  【改大小不是改字体尺寸，是整个游戏对象的尺寸】")]
		public float FontSize = 8;

		[SerializeField, LabelText("刚激活时的物件缩放倍率")]
		public float DefaultInitializeSize = 0.3f;

		[SerializeField, LabelText("缩放到1需要的时间"), SuffixLabel("秒")]
		public float DefaultSizeDuration = 0.5f;

		[SerializeField, LabelText("缩放到1的尺寸曲线")]
		public AnimationCurve Scale1Curve = AnimationCurve.Linear(0, 0, 1, 1);

		[Title("=== 高度 ===", TitleAlignment = TitleAlignments.Centered)]
		[SerializeField, LabelText("Y轴上的运行距离")]
		public float PopYDistance = 2.2f;

		[SerializeField, LabelText("默认弹出上升过程需要的时间"), SuffixLabel("秒")]
		public float DefaultPopDuration = 0.45f;

		[SerializeField, LabelText("上升过程使用的曲线")]
		public AnimationCurve PopProgressCurve = AnimationCurve.Linear(0, 0, 1, 1);
	}



}