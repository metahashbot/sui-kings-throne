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
	public sealed class BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval : BaseBuffLogicPassingComponent
	{
		[LabelText( "修饰组ID。同组会合并")]
		public string TargetUID;
		[LabelText("逻辑标签")]
		public RP_BuffInternalFunctionFlagTypeEnum InternalFunctionFlagType = RP_BuffInternalFunctionFlagTypeEnum.None;
		public RP_DataEntry_EnumType TargetEntry;
		public ModifyEntry_CalculatePosition CalculatePosition;
	
		[LabelText("作用间隔")]
		public float Interval;
		[LabelText("修饰值")]
		public float ModifyValue;
		[LabelText("修饰时长")]
		public float ModifyDuration;
		
		[LabelText("时长被韧性抵抗？")]
		public bool MayResistByToughness = true;
        
		[LabelText("包含对最大层数的覆写？")]
		public bool OverrideStack = false;
		[LabelText("将最大层数覆写为"), ShowIf(nameof(OverrideStack))]
		public int OverrideStackAs = 3;
		
		[LabelText("需要显示到UI?")]
		public bool NeedDisplayOnUI = false;

		[SerializeField,LabelText("    显示于UI的信息"),ShowIf( nameof(NeedDisplayOnUI))]
		public ConcreteBuffDisplayOnUIInfo DisplayContent;
		
		
		public static BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval GetFromPool(
			string componentUID,
			RP_DataEntry_EnumType entry,
			ModifyEntry_CalculatePosition cp,
			float interval,
			float value,
			float duration,
			bool overrideStack,
			bool resistByToughness = false,
			int overrideStackAs = 3)
		{
			BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval newOne =
				new BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval();
			// BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval newOne =
			// 	GenericPool<BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval>.Get();
			newOne.TargetUID = componentUID;
			newOne.TargetEntry = entry;
			newOne.CalculatePosition = cp;
			newOne.Interval = interval;
			newOne.ModifyValue = value;
			newOne.ModifyDuration = duration;
			newOne.OverrideStack = overrideStack;
			newOne.OverrideStackAs = overrideStackAs;
			newOne.MayResistByToughness = resistByToughness;
			 newOne.NeedDisplayOnUI = false;
			return newOne;
		}
        
        
		public static BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval GetFromPool(
			BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval copy)
		{
			BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval newOne =
				new BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval();
			// BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval newOne =
			// 	GenericPool<BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval>.Get();
			newOne.TargetUID = copy.TargetUID;
			newOne.TargetEntry = copy.TargetEntry;
			newOne.CalculatePosition = copy.CalculatePosition;
			newOne.Interval = copy.Interval;
			newOne.ModifyValue = copy.ModifyValue;
			newOne.ModifyDuration = copy.ModifyDuration;
			newOne.OverrideStack = copy.OverrideStack;
			newOne.OverrideStackAs = copy.OverrideStackAs;
			newOne.MayResistByToughness = copy.MayResistByToughness;
			newOne.NeedDisplayOnUI = copy.NeedDisplayOnUI;
			newOne.DisplayContent = copy.DisplayContent;
			return newOne;
		}


		public override void ReleaseOnReturnToPool()
		{
			throw new NotImplementedException();
		}
	}
}