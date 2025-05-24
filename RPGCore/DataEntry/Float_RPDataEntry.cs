using System.Collections.Generic;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.DataEntry
{
public class Float_RPDataEntry : RP_DataEntry_Base
	{
		[ShowInInspector, ReadOnly, LabelText("上界")]
		public float UpperBoundValue { get; private set; }

		[ShowInInspector, ReadOnly, LabelText("下界")]
		public float LowerBoundValue { get; private set; }


		private class FloatDataEntry_CalculateInfoClass
		{
			public float FrontAdd;
			public float FrontMul;
			public float RearAdd;
			public float RearMul;

			public void Clear()
			{
				FrontAdd = 0f;
				FrontMul = 0f;
				RearAdd = 0f;
				RearMul = 0f;
			}
		}

		private FloatDataEntry_CalculateInfoClass FloatDataEntryCalculateInfo;


		private float _cachedData;
		[ShowInInspector, LabelText("当前值"), PropertyOrder(-1)]
		public float CurrentValue => GetCurrentValue();

		public virtual float GetCurrentValue()
		{
			//标记为Dirty就重新计算
			if (IsDirty)
			{
				Recalculate();
				return _cachedData;
			}
			//否则直接返回
			else
			{
				return _cachedData;
			}
		}

		public int GetRoundIntValue()
		{
			return Mathf.RoundToInt(GetCurrentValue());
		}

		public int GetCeilIntValue()
		{
			return Mathf.CeilToInt(GetCurrentValue());
		}

		public int GetFloorIntValue()
		{
			return Mathf.FloorToInt(GetCurrentValue());
		}


		/// <summary>
		/// <para>重新计算cachedData的过程</para>
		/// </summary>
		public override void Recalculate()
		{
			// base.Recalculate();
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变);
			ds.FloatArgu2 = _cachedData;

			_cachedData = 0f;
			FloatDataEntryCalculateInfo.Clear();
			float originalData = 0f;
			foreach (Float_ModifyEntry_RPDataEntry modifyEntry in _selfFloatModifyEntryList)
			{
				int calculatePos = (int)modifyEntry._CalculatePosition;
				if (calculatePos == 1)
				{
					originalData += modifyEntry.ModifyValue;
				}
				else if (calculatePos < 101)
				{
					FloatDataEntryCalculateInfo.FrontAdd += modifyEntry.ModifyValue;
				}
				else if (calculatePos < 201)
				{
					FloatDataEntryCalculateInfo.FrontMul += modifyEntry.ModifyValue;
				}
				else if (calculatePos < 301)
				{
					FloatDataEntryCalculateInfo.RearAdd += modifyEntry.ModifyValue;
				}
				else
				{
					FloatDataEntryCalculateInfo.RearMul += modifyEntry.ModifyValue;
				}
			}

			_cachedData =
				(((originalData + FloatDataEntryCalculateInfo.FrontAdd) *
				  (1f + FloatDataEntryCalculateInfo.FrontMul / 100f)) + FloatDataEntryCalculateInfo.RearAdd) *
				(1f + FloatDataEntryCalculateInfo.RearMul / 100f);

			if (ContainLowerBound)
			{
				if (_cachedData < LowerBoundValue)
				{
					_cachedData = LowerBoundValue;
					// //由于此次修改导致其越界，则需要把最后一次修改的值改掉
					// Float_ModifyEntry_RPDataEntry lastModify =
					// 	_selfFloatModifyEntryList[_selfFloatModifyEntryList.Count - 1];
					// float restoreData = _cachedData - lastModify.ModifyValue;
					// float editModify = LowerBoundValue - restoreData;
					// lastModify.ModifyValue = editModify;
					// _cachedData = LowerBoundValue;
					// _selfFloatModifyEntryList.Remove(lastModify);
					//
					//
					// if (!Mathf.Approximately(editModify, 0f))
					// {
					// 	_selfFloatModifyEntryList.Add(lastModify);
					// }
				}
			}

			if (ContainUpperBound)
			{
				if (_cachedData > UpperBoundValue)
				{
					_cachedData = UpperBoundValue;
					// Float_ModifyEntry_RPDataEntry lastModify =
					// 	_selfFloatModifyEntryList[_selfFloatModifyEntryList.Count - 1];
					// float restoreData = _cachedData - lastModify.ModifyValue;
					// float editModify = UpperBoundValue - restoreData;
					// lastModify.ModifyValue = editModify;
					//
					// _cachedData = UpperBoundValue;
					// _selfFloatModifyEntryList.Remove(lastModify);
					// if (!Mathf.Approximately(editModify, 0f) && !Mathf.Approximately(lastModify.ModifyValue, 0f))
					// {
					// 	_selfFloatModifyEntryList.Add(lastModify);
					// }
				}
			}

			ds.ObjectArgu1 = this;
			ds.FloatArgu1 = _cachedData;
			ds.IntArgu1 = (int)RP_DataEntryType;
			ds.ObjectArgu2 = _relatedDatabaseReference.SelfRelatedRPObjectReference;
			_relatedDatabaseReference.SelfRelatedRPObjectReference.GetRelatedActionBus()
				.TriggerActionByType(ds);
			
			IsDirty = false;
		}
		// protected override void DataCombine()
		// {
		//     float newBaseValue = 0f;
		//     FloatDataEntryCalculateInfo.Clear();
		//     _removeCacheList.Clear();
		//     float originalData = 0f;
		//     foreach (var modifyEntryPair in _selfFloatModifyEntryDict)
		//     {
		//         var modifyEntryKey = modifyEntryPair.Key;
		//         var modifyEntry = modifyEntryPair.Value;
		//         if (modifyEntry.ModifyTimeStamp > (Time.time + 2f * dataAutoCombine_ModifyTimeThreshold)
		//             ||
		//             modifyEntry.ModifyVersion > (DataEntryModifyVersion - 2 * dataAutoCombine_ModifyVersionThreshold))
		//         {
		//             _removeCacheList.Add(modifyEntryKey);
		//             int calculatePos = (int)modifyEntry._CalculatePosition;
		//             if (calculatePos == 1)
		//             {
		//                 originalData += modifyEntry.ModifyValue;
		//             }
		//             else if (calculatePos < 101)
		//             {
		//                 FloatDataEntryCalculateInfo.FrontAdd += modifyEntry.ModifyValue;
		//             }
		//             else if (calculatePos < 201)
		//             {
		//                 FloatDataEntryCalculateInfo.FrontMul += modifyEntry.ModifyValue;
		//             }
		//             else if (calculatePos < 301)
		//             {
		//                 FloatDataEntryCalculateInfo.RearAdd += modifyEntry.ModifyValue;
		//             }
		//             else
		//             {
		//                 FloatDataEntryCalculateInfo.RearMul += modifyEntry.ModifyValue;
		//             }
		//         }
		//     }
		//
		//     newBaseValue = (
		//         ((originalData + FloatDataEntryCalculateInfo.FrontAdd) *
		//          (1f + FloatDataEntryCalculateInfo.FrontMul / 100f)) +
		//         FloatDataEntryCalculateInfo.RearAdd) * (1f + FloatDataEntryCalculateInfo.RearMul / 100f);
		//
		//     
		//     foreach (int needRemoveKey in _removeCacheList)
		//     {
		//         _selfFloatModifyEntryDict.Remove(needRemoveKey);
		//     }
		//
		//     AddDataEntryModifier(
		//         new Float_ModifyEntry_RPDataEntry(newBaseValue).SetModifyFromEnum(RPDM_DataEntry_ModifyFrom
		//             .Initialize_FromDataCombine));
		//     
		//
		// }


		/// <summary>
		/// <para>由于这个函数总是在Initial的时候调用，那个时候通常还没有任何DataModifyEntry，所以不需要重新计算，只需要置bool就行了</para>
		/// <para>重新计算上下界的业务在添加新的DataModify的时候，如果表明有上下界，则添加Modify的同时会重新计算</para>
		/// </summary>
		public override RP_DataEntry_Base SetBoundByInitial_Default(bool setUpper, bool setLower)
		{
			ContainUpperBound = setUpper;
			if (setUpper)
			{
				UpperBoundValue = 0f;
				foreach (Float_ModifyEntry_RPDataEntry modifyEntry in _selfFloatModifyEntryList)
				{
					if ((int)modifyEntry.ModifyFrom < 100)
					{
						UpperBoundValue += modifyEntry.ModifyValue;
					}
				}
			}

			ContainLowerBound = setLower;
			if (setLower)
			{
				LowerBoundValue = 0f;
			}

			return this;
		}

		protected override void RecalculateUpperBound()
		{
			foreach (Float_ModifyEntry_RPDataEntry modifyEntry in _selfFloatModifyEntryList)
			{
				if ((int)modifyEntry.ModifyFrom > 0 && (int)modifyEntry.ModifyFrom < 100)
				{
					UpperBoundValue += modifyEntry.ModifyValue;
				}
			}
		}

		/// <summary>
		/// <para>设置上界。如果reset为false，则是修改而不是覆盖</para>
		/// <para>但之后如果再次添加了Original来源的modify，则依然会按照默认逻辑重新计算上界</para>
		/// </summary>
		public void SetUpperBound(float targetValue, bool reset = true)
		{
			ContainUpperBound = true;
			if (reset)
			{
				UpperBoundValue = targetValue;
			}
			else
			{
				UpperBoundValue += targetValue;
			}
		}


		/// <summary>
		/// <para>设置下界。如果reset为false，则是修改而不是覆盖</para>
		/// <para>但之后如果再次添加了Original来源的modify，则依然会按照默认逻辑重新计算上界</para>
		/// </summary>
		public void SetLowerBound(float targetValue, bool reset = true)
		{
			ContainLowerBound = true;
			if (reset)
			{
				LowerBoundValue = targetValue;
			}
			else
			{
				LowerBoundValue += targetValue;
			}
		}




		
		public virtual void ReplaceOriginalValue(float targetValue)
		{
			var modifier = _selfFloatModifyEntryList.Find((entry =>
				entry._CalculatePosition == ModifyEntry_CalculatePosition.Original));
			modifier.ModifyValue = targetValue;
			Recalculate();
		}

		/// <summary>
		/// <para>直接设置当前值为这个数，并且默认清除所有之前记录的东西</para>
		/// <para>这很危险！因为会把所有有关的Modifier一并清理，这样那些buff啥的就有可能失效了！除非真的是需要“重设”</para>
		/// <para>如果只是想替代原始值，应当使用ReplaceOriginalValue()的方法</para>
		/// <para>！！：如果是正在操作一个"当前值"，那你应当看到这个注释，因为"当前值"是FloatPresentValue。</para>
		/// <para>clear为true时，所有数据都会被清除，设置的数会被当成新的Original</para>
		/// <para>clear为false时，Original数据不会被清除，其他的会全部清除</para>
		/// </summary>
		public virtual void ResetDataToValue(float targetValue, bool clearOriginal = true)
		{
			_cachedData = targetValue;
			IsDirty = false;

			if (clearOriginal)
			{
				foreach (var perEntry in _selfFloatModifyEntryList)
				{
					GenericPool<Float_ModifyEntry_RPDataEntry>.Release(perEntry);
				}
				_selfFloatModifyEntryList.Clear();
				Float_ModifyEntry_RPDataEntry newModifier = Float_ModifyEntry_RPDataEntry.GetNewFromPool(targetValue,
					RPDM_DataEntry_ModifyFrom.Initialize_FromReset_被重置,
					ModifyEntry_CalculatePosition.Original,
					this);
				AddDataEntryModifier(newModifier);
			}
			else
			{
				List<int> needToRemoveList = new List<int>();
				for (int i = 0; i < _selfFloatModifyEntryList.Count; i++)
				{
					if (_selfFloatModifyEntryList[i]._CalculatePosition == ModifyEntry_CalculatePosition.Original)
					{
						continue;
					}
					else
					{
						needToRemoveList.Add(i);
					}
				}
				//倒叙删除
				for (int i = needToRemoveList.Count - 1; i >= 0; i--)
				{
					_selfFloatModifyEntryList.RemoveAt(needToRemoveList[i]);
				}
				needToRemoveList.Clear();
			}
		}

		
		
		


		public static Float_RPDataEntry GetFromPool(RP_DataEntry_EnumType modelPropertyType,
			DataEntry_Database database)
		{
			Float_RPDataEntry entry = GenericPool<Float_RPDataEntry>.Get();
			entry.RP_DataEntryType = modelPropertyType;
			entry.FloatDataEntryCalculateInfo = GenericPool<FloatDataEntry_CalculateInfoClass>.Get();
			entry.FloatDataEntryCalculateInfo.Clear();
			entry._relatedDatabaseReference = database;
			entry._removeCacheIndexList = CollectionPool<List<int>, int>.Get();
			entry._removeCacheIndexList.Clear();
			entry.ContainUpperBound = false;
			entry.ContainLowerBound = false;
			entry.IsDirty = true;
			entry._selfFloatModifyEntryList =
				CollectionPool<List<Float_ModifyEntry_RPDataEntry>, Float_ModifyEntry_RPDataEntry>.Get();
			entry._selfFloatModifyEntryList.Clear();

			return entry;

		}
		
		

		public override void ClearBeforeDestroy()
		{
			
			UpperBoundValue = 0f;
			LowerBoundValue = 0f;
			if (FloatDataEntryCalculateInfo != null)
			{
				GenericPool<FloatDataEntry_CalculateInfoClass>.Release(FloatDataEntryCalculateInfo);
			}
			for (int i = _selfFloatModifyEntryList.Count - 1; i >= 0; i--)
			{
				GenericPool<Float_ModifyEntry_RPDataEntry>.Release(_selfFloatModifyEntryList[i]);
			}
			_selfFloatModifyEntryList.Clear();
			CollectionPool<List<Float_ModifyEntry_RPDataEntry>, Float_ModifyEntry_RPDataEntry>.Release(
				_selfFloatModifyEntryList);
			base.ClearBeforeDestroy();
		
		}
	}
}