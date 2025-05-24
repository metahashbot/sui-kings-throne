using System;
using Sirenix.OdinInspector;
namespace RPGCore.DataEntry
{
	[Serializable]
	public class ConSer_DataEntryInitializeConfig
	{
		[LabelText("数据类型")]
		public RP_DataEntry_EnumType EntryType;
		[LabelText("基本值")]
		public float BaseValue;
		[LabelText("包含上界吗")]
		public bool ContainUpperBound;
		[LabelText("上界基本值"), ShowIf(nameof(ContainUpperBound))]
		public float BaseUpperBound;
		[LabelText("包含下界吗")]
		public bool ContainLowerBound;
		[LabelText("下界基本值"), ShowIf(nameof(ContainLowerBound))]
		public float BaseLowerBound;

	}
}