//#pragma warning disable CS0162
//#pragma warning disable CS0414


using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;


namespace RPGCore.DataEntry
{
	/// <summary>
	/// <para>作为RPG数据模型中的一项数据</para>
	/// <para>每个数据模型中会有很多项数据</para>
	/// </summary>
	public abstract class RP_DataEntry_Base
	{
#region 数据合并

		//230228数据合并的功能注释了。目前还完全用不到
		// protected static int dataAutoCombine_ModifyVersionThreshold = 50;
		// protected static float dataAutoCombine_ModifyTimeThreshold = 30f;
		// public static void SetDataAutoCombineData(int version, float time)
		// {
		// 	dataAutoCombine_ModifyVersionThreshold = version;
		// 	dataAutoCombine_ModifyTimeThreshold = time;
		// }

		// /// <summary>
		// /// <para>数据是否需要自动合并？</para>
		// /// <para>    会在数据项版本和时间超过2X合并间隔后，将1X合并间隔之前的数据合并；</para>
		// /// </summary>
		// public bool DataNeedAutoCombine { get; protected set; } = true;
		// /// <summary>
		// /// <para>数据是否经历过合并计算？如果数据项出现异常，则可以考虑关闭改项目的数据合并</para>
		// /// </summary>
		// public bool DataCombined { get; protected set; }
		// /// <summary>
		// /// <para>数据项修饰版本。用作合并计算使用。会根据默认间隔进行合并，可以禁用合并</para>
		// /// </summary>
		// public int DataEntryModifyVersion { get; protected set; }
		// /// <summary>
		// /// <para>数据项修饰时间点。用作合并计算使用</para>
		// /// </summary>
		// public float DataEntryModifyTimestamp { get; protected set; }
		//
		// public void SetDataAutoCombine(bool target)
		// {
		// 	DataCombined = target;
		// }

#endregion
		protected DataEntry_Database _relatedDatabaseReference;

		/// <summary>
		/// <para>用于数据项移除时的List</para>
		/// </summary>
		protected List<int> _removeCacheIndexList = new List<int>();



		/// <summary>
		/// <para>对应RPDM的某项数据</para>
		/// </summary>
		[ShowInInspector, LabelText("数据类型")]
		public RP_DataEntry_EnumType RP_DataEntryType { get; protected set; }

		/// <summary>
		/// <para>标记是否为脏</para>
		/// <para>标记为脏的，会在Tick的时候重新计算自身的值</para>
		/// </summary>
		[ShowInInspector, LabelText("当前脏？")]
		public bool IsDirty { get; protected set; }

		/// <summary>
		/// <para>拥有上界？</para>
		/// </summary>
		[ShowInInspector, LabelText("有上界吗？"), HorizontalGroup("Bound")]
		public bool ContainUpperBound { get; protected set; }

		/// <summary>
		/// <para>拥有下界？</para>
		/// </summary>
		[ShowInInspector, LabelText("有下界吗？"), HorizontalGroup("Bound")]
		public bool ContainLowerBound { get; protected set; }

		/// <summary>
		/// <para>在标记为Dirty的时候，执行重新计算</para>
		/// <para>具体子类自行处理重新计算的操作。</para>
		/// <para>基类只负责进行数据合并的触发，如何合并也是子类进行的</para>
		/// </summary>
		public abstract void Recalculate();
		// {
		// if (DataNeedAutoCombine)
		// {
		// 	if (DataEntryModifyVersion / dataAutoCombine_ModifyVersionThreshold > 1)
		// 	{
		// 		DataCombine();
		// 	}
		// 	else if (Time.time > (DataEntryModifyTimestamp + 2f * dataAutoCombine_ModifyTimeThreshold))
		// 	{
		// 		DataCombine();
		// 	}
		// }
		// }

		// protected abstract void DataCombine();


		/// <summary>
		/// <para>自身数据作用记录的列表。</para>
		/// <para>计算数据就是遍历这个</para>
		/// </summary>
		[ShowInInspector, LabelText("修饰项记录容器")]
		protected List<Float_ModifyEntry_RPDataEntry> _selfFloatModifyEntryList =
			new List<Float_ModifyEntry_RPDataEntry>();

		public bool ModifyListContains(Float_ModifyEntry_RPDataEntry modify)
		{
			return _selfFloatModifyEntryList.Contains(modify);
		}

#if UNITY_EDITOR

		public List<Float_ModifyEntry_RPDataEntry> _E_GetDataModifyDictionary()
		{
			return _selfFloatModifyEntryList;
		}

#endif

		/// <summary>
		/// <para>获取数据项的原始值。会计算Modify记录里面CP为Original的项目</para>
		/// </summary>
		public float GetOriginalData()
		{
			float result = 0f;
			foreach (Float_ModifyEntry_RPDataEntry value in _selfFloatModifyEntryList)
			{
				if (value._CalculatePosition == ModifyEntry_CalculatePosition.Original)
				{
					result += value.ModifyValue;
				}
			}

			return result;
		}


		/// <summary>
		/// <para>设置数据 上下界。默认实现，即通过Modifier为Initial的来设置上界，下界默认为0</para>
		/// </summary>
		public abstract RP_DataEntry_Base SetBoundByInitial_Default(bool setLowerBound = true,
			bool setUpperBound = true);


		/// <summary>
		/// <para>添加新的ModifyEntry</para>
		/// </summary>
		public virtual Float_ModifyEntry_RPDataEntry AddDataEntryModifier(Float_ModifyEntry_RPDataEntry modifyEntry)
		{
			
#if UNITY_EDITOR
			if (_selfFloatModifyEntryList.Contains(modifyEntry))
			{
				DBug.LogError($" {(_relatedDatabaseReference.SelfRelatedRPObjectReference as MonoBehaviour).name}" +
				              $"的数据项{RP_DataEntryType}在添加修饰 修饰来源:{modifyEntry.ModifyFrom}|修饰值{modifyEntry.ModifyValue}的时候，" +
				              $"发现它已经有这个修饰了，可能是引用管理出现了问题");
			}
#endif

			_selfFloatModifyEntryList.Add(modifyEntry);
			modifyEntry.ModifyOnEntryRef = this;
			Recalculate();
			
			DS_ActionBusArguGroup ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_DataEntry_OnDataEntryReceiveNewModifyEntry_数据项接受了新的修饰项);
			ds.ObjectArgu1 = this;
			_relatedDatabaseReference.SelfRelatedRPObjectReference.GetRelatedActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryReceiveNewModifyEntry_数据项接受了新的修饰项,
					ds);
			return modifyEntry;
		}


		/// <summary>
		/// <para>移除所有记录的DataEntryModifier中匹配ID的项目</para>
		/// </summary>
		public void RemoveTargetModifierByFromID(RPDM_DataEntry_ModifyFrom from)
		{
			_removeCacheIndexList.Clear();

			for (int i = 0; i < _selfFloatModifyEntryList.Count; i++)
			{
				if (_selfFloatModifyEntryList[i].ModifyFrom == from)
				{
					_removeCacheIndexList.Add(i);
				}
			}
			for (int i = _removeCacheIndexList.Count - 1; i >= 0; i--)
			{
				_selfFloatModifyEntryList.RemoveAt(_removeCacheIndexList[i]);
			}

			IsDirty = true;
			Recalculate();
		}
		
		
		
		/// <summary>
		/// <para>移除Modify并不会将Modify释放回Pool。如果需要释放，一定需要手动调用</para>
		/// </summary>
		/// <param name="entry"></param>
		public void RemoveEntryModifier(Float_ModifyEntry_RPDataEntry entry)
		{
			if (_selfFloatModifyEntryList.Contains(entry))
			{
				_selfFloatModifyEntryList.Remove(entry);
				Recalculate();
			}

			else
			{
				DBug.LogWarning($"{(_relatedDatabaseReference.SelfRelatedRPObjectReference as MonoBehaviour).name}" +
				                $"的数据项{RP_DataEntryType}在移除修饰 修饰来源:{entry.ModifyFrom}|修饰值{entry.ModifyValue}的时候，" +
				                $"发现它根本没有这个修饰，可能是引用管理出现了问题");
			}
		}


		protected virtual void RecalculateUpperBound()
		{
		}



		public virtual void ClearBeforeDestroy()
		{
			_relatedDatabaseReference = null;
			if (_removeCacheIndexList != null)
			{
				_removeCacheIndexList.Clear();
				CollectionPool<List<int>, int>.Release(_removeCacheIndexList);
			}
			ContainUpperBound = false;
			ContainLowerBound = false;
			IsDirty = true;
			RP_DataEntryType = RP_DataEntry_EnumType.None;
			if (_selfFloatModifyEntryList != null)
			{
				for (int i = _selfFloatModifyEntryList.Count - 1; i >= 0; i--)
				{
					Float_ModifyEntry_RPDataEntry tmpEntry = _selfFloatModifyEntryList[i];
//					GenericPool<Float_ModifyEntry_RPDataEntry>.Release(tmpEntry);
				}
				_selfFloatModifyEntryList.Clear();
				// CollectionPool<List<Float_ModifyEntry_RPDataEntry>, Float_ModifyEntry_RPDataEntry>.Release(
				// 	_selfFloatModifyEntryList);
			}
		}
	}
}