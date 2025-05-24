using System;
using ARPG.Character.Config;
using Sirenix.OdinInspector;
namespace ARPG.Character.Inheritance
{

	public enum InheritanceModifyCalculationPosition
	{
		OriginalOverride_覆盖原有 = 0,
		FrontAdd_前加  = 1,
		FrontMultiply_前乘 = 2,
		RearAdd_后加 = 3,
		RearMultiply_后乘 = 4,
		None_无事发生 = 10,
		FinalMul_最终有效性乘算 = 11,
		
	}
	
	[Serializable]
	public abstract class BaseInheritanceComponent
	{
		[VerticalGroup("Valid")]
		[LabelText("满足条件时，修正的计算位置")]
		public InheritanceModifyCalculationPosition CalculationPosition = InheritanceModifyCalculationPosition.None_无事发生;
		[VerticalGroup("Valid")]
		[LabelText("满足条件时，修正值")]
		public float ModifyValue;

		[VerticalGroup("Invalid")]
		[LabelText("不满足时，修正的计算位置")]
		public InheritanceModifyCalculationPosition CalculationPosition_NotValid = InheritanceModifyCalculationPosition.None_无事发生;
		[VerticalGroup("Invalid")]
		[LabelText("不满足时，修正值")]
		public float ModifyValue_NotValid;
	}
	
	[Serializable]
	public sealed class InheritanceComponent_Gender : BaseInheritanceComponent
	{
		[LabelText("√：需要男性；口：需要女性")]
		public bool RequireMale;
		
	}
	
	[TypeInfoBox("对于血统组件。可以有多个纯度阶段，判定是从最高纯度开始算的.\n" +
	             "当有一个纯度被满足而参与计算后，后续的血统组件将被跳过")]
	[Serializable]
	public sealed class InheritanceComponent_Race : BaseInheritanceComponent
	{
		[LabelText("指定种族")]
		public CharacterRaceTypeEnum RequireRaceType;
		[LabelText("纯度。1为全纯。纯度小于目标时，不生效")]
		public float PureRatio = 1f;

	}


	[Serializable]
	public sealed class InheritanceComponent_Family : BaseInheritanceComponent
	{
		[LabelText("指定家族")]
		public CharacterFamilyTypeEnum FamilyType;
		
	}
}