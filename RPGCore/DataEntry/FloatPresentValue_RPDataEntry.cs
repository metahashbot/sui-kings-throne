using System;
using System.Collections.Generic;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.DataEntry
{
	/// <summary>
	/// <para>作为当前值的数据项。用于“当前”的数据，其内部不会记录历史修饰项，所有的计算即刻生效。</para>
	/// <para>一定需要一个常规DataEntry作为"最大值"数据引用</para>
	/// </summary>
	public class FloatPresentValue_RPDataEntry : Float_RPDataEntry
	{
		[ShowInInspector,LabelText("关联最大值数据项引用")]
		public Float_RPDataEntry MaxValueRef { get; protected set; }


		[ShowInInspector,LabelText("当前上界")]
		public float CurrentUpperBound => MaxValueRef.CurrentValue;
		public float CurrentLowerBound { get; protected set; }
		
		


		private float _cacheData;
		public override float GetCurrentValue()
		{
			return _cacheData;
		}


		/// <summary>
		/// <para>PresentValue并不会储存DataEntry，所以替换原始值就相当于重设值了</para>
		/// </summary>
		/// <param name="targetValue"></param>
		public override void ReplaceOriginalValue(float targetValue)
		{
			ResetDataToValue(targetValue);
		}

		/// <summary>
		/// <para>对于PresentValue，就是直接重设cacheData。正常行为</para>
		/// </summary>
		public override void ResetDataToValue(float targetValue, bool clearOriginal = true)
		{
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变);
			ds.FloatArgu2 = _cacheData;
			_cacheData = targetValue;
			ds.FloatArgu1 = _cacheData;
			ds.IntArgu1 = (int)RP_DataEntryType;
			ds.ObjectArgu1 = this;
			ds.ObjectArgu2 = _relatedDatabaseReference.SelfRelatedRPObjectReference;
			_relatedDatabaseReference.SelfRelatedRPObjectReference.GetRelatedActionBus().TriggerActionByType(ds);

		}


		public override void Recalculate()
		{
			
		}
		public override RP_DataEntry_Base SetBoundByInitial_Default(bool setLowerBound, bool setUpperBound)
		{
			ContainLowerBound = setLowerBound;
			if (ContainLowerBound)
			{
				CurrentLowerBound = 0f;
			}
			
			return this;
		}
		/// <summary>
		/// PresetValue并不会记录修饰项。此处只是立刻运算了修饰项的影响。返回前就会将传入的modifyEntry释放掉。返回值始终是null
		/// <para>original将视作Reset。FA和RA都当做Add算，FM和RM都当做Mul算，并没有实际的计算位置含义</para>
		/// </summary>
		public override Float_ModifyEntry_RPDataEntry AddDataEntryModifier(Float_ModifyEntry_RPDataEntry modifyEntry)
		{
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变);
			ds.FloatArgu2 = _cacheData;
			
			switch (modifyEntry._CalculatePosition)
			{
				case ModifyEntry_CalculatePosition.Original:
					ResetDataToValue(modifyEntry.ModifyValue);
					break;
				case ModifyEntry_CalculatePosition.FrontAdd:
				case ModifyEntry_CalculatePosition.RearAdd:
					_cacheData += modifyEntry.ModifyValue;
					break;
				case ModifyEntry_CalculatePosition.FrontMul:
				case ModifyEntry_CalculatePosition.RearMul:
					_cacheData *= modifyEntry.ModifyValue;
					break;
			}
			if (ContainUpperBound)
			{
				if (_cacheData > CurrentUpperBound)
				{
					_cacheData = CurrentUpperBound;
				}
			}
			if (ContainLowerBound)
			{
				if (_cacheData < CurrentLowerBound)
				{
					_cacheData = CurrentLowerBound;
				}
			}

// #if UNITY_EDITOR
			//在编辑期，会将修饰项存一下，用作调试
			_selfFloatModifyEntryList.Add(modifyEntry);

// #else
// //非编辑期就直接放掉了
			// GenericPool<Float_ModifyEntry_RPDataEntry>.Release(modifyEntry);
//
// #endif

			ds.FloatArgu1 = _cacheData;
			ds.ObjectArgu1 = this;
			ds.ObjectArgu2 = _relatedDatabaseReference.SelfRelatedRPObjectReference;
			ds.IntArgu1 = (int)RP_DataEntryType;
			_relatedDatabaseReference.SelfRelatedRPObjectReference.GetRelatedActionBus().TriggerActionByType(ds);
			
			return null;
		}

		public new static FloatPresentValue_RPDataEntry GetFromPool(
			RP_DataEntry_EnumType type,
			DataEntry_Database database,Float_RPDataEntry maxValueRef)
		{
			FloatPresentValue_RPDataEntry entry = GenericPool<FloatPresentValue_RPDataEntry>.Get();
			entry.RP_DataEntryType = type;
			entry._relatedDatabaseReference = database;
			entry.ContainUpperBound = false;
			entry.ContainLowerBound = false;
// #if UNITY_EDITOR
			entry._selfFloatModifyEntryList =
				CollectionPool<List<Float_ModifyEntry_RPDataEntry>, Float_ModifyEntry_RPDataEntry>.Get();
// #else
			// entry._selfFloatModifyEntryList = null;
// #endif
			entry.MaxValueRef = maxValueRef;
			return entry;


		}
	}
}