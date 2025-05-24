using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure.BuffLogicPassingComponent
{
	[Serializable]
	public class BLP_设置持续和有效时间_SetDurationAndTime : BaseBuffLogicPassingComponent
	{
		//duration version
		[SerializeField, LabelText("包含对[持续时间]的设置")]
		public bool ContainDurationSet = false;
		[SerializeField, LabelText("改动是不小于的？")] [ShowIf(nameof(ContainDurationSet))]
		public bool IsDurationNotLessThan = true;
		[SerializeField, LabelText("    √重设 | 口修饰")] [ShowIf(nameof(ContainDurationSet))]
		public bool IsDurationReset = false;
		[SerializeField, LabelText("    重设的值")] [ShowIf("@(this.ContainDurationSet && this.IsDurationReset)")]
		public float DurationResetValue = 0f;
		[SerializeField, LabelText("√:修饰为乘算  |  口:修饰为加算")]
		[ShowIf("@(this.ContainDurationSet && !this.IsDurationReset)")]
		public bool IsDurationModifyByMultiply = true;
		[SerializeField, LabelText("修饰值，乘算则直接乘这个数")] [ShowIf("@(this.ContainDurationSet && !this.IsDurationReset)")]
		public float DurationModifyValue = 0f;

		//availableTime Version
		[SerializeField, LabelText("包含对[有效时长]的设置")]
		public bool ContainAvailableTimeSet = false;
		[SerializeField, LabelText("改动是不小于的？")] [ShowIf(nameof(ContainAvailableTimeSet))]
		public bool IsAvailableTimeNotLessThan = true;
		[SerializeField, LabelText("     √重设 | 口修饰")] [ShowIf(nameof(ContainAvailableTimeSet))]
		public bool IsAvailableTimeReset = false;
		[SerializeField, LabelText("    重设的值")] [ShowIf("@(this.ContainAvailableTimeSet && this.IsAvailableTimeReset)")]
		public float AvailableTimeResetValue = 0f;
		[SerializeField, LabelText("√:修饰为乘算  |  口:修饰为加算")]
		[ShowIf("@(this.ContainAvailableTimeSet && !this.IsAvailableTimeReset)")]
		public bool IsAvailableTimeModifyByMultiply = true;
		[SerializeField, LabelText("修饰值，乘算则直接乘这个数")]
		[ShowIf("@(this.ContainAvailableTimeSet && !this.IsAvailableTimeReset)")]
		public float AvailableTimeModifyValue = 0f;


#if UNITY_EDITOR
		[Button("快速设置为 不小于的：")]
		public void SetAllAsNotLess_Button_(float ff)
		{
			SetAllAsNotLess(ff);
		}
		
		[Button("快速设置为 常驻永续的：")]
		public void SetAllAsResident_Button_()
		{
			SetAllAsResident();
		}
#endif
		
		
		
		
		
		public void SetAllAsNotLess(float resetValue)
		{
			ContainDurationSet = true;
			IsDurationNotLessThan = true;
			IsDurationReset = true;
			DurationResetValue = resetValue;

			ContainAvailableTimeSet = true;
			IsAvailableTimeNotLessThan = true;
			IsAvailableTimeReset = true;
			AvailableTimeResetValue = resetValue;

		}
		/// <summary>
		/// <para>设置为-1</para>
		/// </summary>
		public void SetAllAsResident()
		{
			ContainDurationSet = true;
			IsDurationNotLessThan = false;
			IsDurationReset = true;
			DurationResetValue = -1f;
			
			ContainAvailableTimeSet = true;
			IsAvailableTimeNotLessThan = false;
			IsAvailableTimeReset = true;
			AvailableTimeResetValue = -1f;
		}

		public override void ReleaseOnReturnToPool()
		{	
			GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Release(this);
		}
	}
}