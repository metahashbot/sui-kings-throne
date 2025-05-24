using System;
using System.Collections.Generic;
using RPGCore.Buff;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	public sealed class BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify : BaseBuffLogicPassingComponent
	{
		[LabelText("修饰组ID。同组会合并")]
		public string TargetUID;
		[LabelText("逻辑标签") ]
		public RP_BuffInternalFunctionFlagTypeEnum InternalFunctionFlagType = RP_BuffInternalFunctionFlagTypeEnum.None;
		[LabelText("目标数据项")]
		public RP_DataEntry_EnumType TargetEntry;
		[LabelText("目标计算位置")]
		public ModifyEntry_CalculatePosition CalculatePosition;
		[LabelText("修饰值。如果是乘算则为百分比")]
		public float ModifyValue;
		[LabelText("修饰持续时间")]
		public float ModifyDuration;
		[LabelText("包含对最大层数的覆写？")]
		public bool OverrideStack = false;
		[InfoBox("对于已经有的层， 如果覆写层数限制，则超出层数的部分会被移除，且触发爆层效果")]
		[LabelText("将最大层数覆写为"), ShowIf(nameof(OverrideStack))]
		public int OverrideStackAs = 3;

		[LabelText("需要显示到UI?")]
		public bool NeedDisplayOnUI = false;

		[SerializeField, LabelText("    显示于UI的信息"), ShowIf(nameof(NeedDisplayOnUI))]
		public ConcreteBuffDisplayOnUIInfo DisplayContent;



		[LabelText("包含对爆层后的额外操作？"), SerializeField]
		public bool ContainStackFlowOperation;
		
		[LabelText("    爆层后清除当前组？"), SerializeField, ShowIf(nameof(ContainStackFlowOperation))]
		public bool ClearCurrentGroupOnStackFull = false;

		[LabelText("   爆层后的额外Buff施加"), ShowIf(nameof(ContainStackFlowOperation)), SerializeField]
		public List<ConSer_BuffApplyInfo> StackFlowOperationList;
		/*
		 *
		 *
		 *
		 */


		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify>.Release(this);
		}
	}
}