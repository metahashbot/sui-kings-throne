using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace RPGCore.Projectile.Layout.LayoutComponent
{
	/// <summary>
	/// <para>常用的XY轴曲线，并额外记录了XY是相对值还是绝对值</para>
	/// </summary>
	[Serializable]
	public class CommonDataCurvePair
	{

		[LabelText("基准Y轴值")]
		public float BaseValue_OrYValue;
		
		

		[LabelText("使用曲线吗？")]
		public bool UseCurve;

		[LabelText("X的最大值参照"), ShowIf(nameof(UseCurve))]
		public float XMaxValue;
		
		
		[LabelText("√：Y作为原始Y值的乘数；口：Y作为直接覆盖的值"), ShowIf(nameof(UseCurve))]
		public bool CurveYAsRelative = false;


		[SerializeField, LabelText("曲线"), ShowIf(nameof(UseCurve))]
		public AnimationCurve SelfCurve;
		public static implicit operator CommonDataCurvePair(float value)
		{
			return new CommonDataCurvePair
			{
				BaseValue_OrYValue = value,
			};
		}

		/// <summary>
		/// <para>传入参数是X的绝对值，需要使用曲线记录的X最大值来得到比率</para>
		/// </summary>
		public float GetCurrentValueAtXAbs(float xAbs)
		{
			return GetCurrentValueAtXPartial(xAbs / XMaxValue);
		}


		/// <summary>
		/// <para>！！X值始终是0~1的范围，调用发起者需要自行处理X在[0,1]的映射</para>
		/// </summary>
		public float GetCurrentValueAtXPartial(float x)
		{
			if (UseCurve)
			{
				if (CurveYAsRelative)
				{
					//作为[0,1]范围，乘算
					return SelfCurve.Evaluate(x) * BaseValue_OrYValue;
				}
				else
				{
					//作为直接值，直接返回
					return SelfCurve.Evaluate(x);
				}
			}
			else
			{
				return BaseValue_OrYValue;
			}
		}

#if UNITY_EDITOR

		[Button, HorizontalGroup("A")]
		public AnimationCurve 按当前设置重设动画_仅两点()
		{
			if (SelfCurve == null)
			{
				SelfCurve = new AnimationCurve();
			}
			else if (SelfCurve.keys.Length > 1)
			{
				if (!UnityEditor.EditorUtility.DisplayDialog("警告", "当前动画曲线已有内容，重设会清空他们，确定吗？", "确定", "取消"))
				{
					return SelfCurve;
				}
			}


			SelfCurve.ClearKeys();

			if (CurveYAsRelative)
			{
				//作为直接值，重设两点
				SelfCurve.AddKey(0f, 1f);
				SelfCurve.AddKey(1f, 1f);
			}
			else
			{
				SelfCurve.AddKey(0f, BaseValue_OrYValue);
				SelfCurve.AddKey(1f, BaseValue_OrYValue);
			}

			return SelfCurve;
		}
	



#endif


	}
}