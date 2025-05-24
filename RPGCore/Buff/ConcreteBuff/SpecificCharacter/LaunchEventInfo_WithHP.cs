using System;
using System.Collections.Generic;
using GameplayEvent.SO;
using RPGCore.Buff.Requirement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff
{
	[InfoBox("这些数据在运行时不作修改，相当于配置只读")]
	[Serializable]
	public class LaunchEventInfo_WithHP
	{

		[LabelText("将要触发的事件们"), SerializeField, InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public List<SOConfig_PrefabEventConfig> EventList = new List<SOConfig_PrefabEventConfig>();
		//触发后是否不会再次触发
		//生命值低于、等于、还是高于触发
		[LabelText("能够触发的次数，-1就是无限"), SerializeField]
		public int CanLaunchCount = 1;

		[LabelText("触发条件——比较")]
		public CompareMethodEnum CompareMethod = CompareMethodEnum.Equal_等于;

		[LabelText("    √:按当前生命值数值比对 || 口:按最大生命值比例比对")]
		public bool CompareWithCurrentValue = true;

		[LabelText("        比较值"), ShowIf(nameof(CompareWithCurrentValue))]
		public float CompareAbsoluteValue = 1;

		[LabelText("        触发条件——比例"), SuffixLabel("%"), HideIf(nameof(CompareWithCurrentValue))]
		public float ComparePartial = 50f;

		/// <summary>
		/// <para>获得运行时拷贝。并不会深拷贝EventList，但是会拷贝其他数据</para>
		/// </summary>
		/// <param name="copyFrom"></param>
		/// <returns></returns>
		public static LaunchEventInfo_WithHP GetRuntimeCopy(LaunchEventInfo_WithHP copyFrom)
		{
			var result = new LaunchEventInfo_WithHP();
			result.CanLaunchCount = copyFrom.CanLaunchCount;
			result.CompareAbsoluteValue = copyFrom.CompareAbsoluteValue;
			result.CompareMethod = copyFrom.CompareMethod;
			result.ComparePartial = copyFrom.ComparePartial;
			result.CompareWithCurrentValue = copyFrom.CompareWithCurrentValue;
			result.EventList = copyFrom.EventList;
			return result;
			 
		}
		
		
	}
}