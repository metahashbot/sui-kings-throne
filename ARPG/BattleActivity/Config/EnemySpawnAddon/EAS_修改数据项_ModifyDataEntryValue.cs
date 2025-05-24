using System;
using System.Collections.Generic;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class EAS_修改数据项_ModifyDataEntryValue : BaseEnemySpawnAddon
	{
		[Serializable]
		public class DataEntryModifyInfo
		{
			public RP_DataEntry_EnumType Type;

			[LabelText("√:覆写数据项 || 口:修改数据项")]
			public bool OverrideOrModify;

			[LabelText("修饰值")]
			public float ModifyValue;

			[InfoBox("如果使用的是乘法算区，则意味着 50 会在这个算区上加50%的效果。加法算区就是直接添加\n" + "实际的原理是为这个数据项增加一个修饰项")]
			[LabelText("使用算区"), HideIf(nameof(OverrideOrModify))]
			public ModifyEntry_CalculatePosition CalculatePosition = ModifyEntry_CalculatePosition.FrontAdd;
		}

		[LabelText("修改信息列表")]
		public List<DataEntryModifyInfo> ModifyInfosList;
		public override void ResetOnReturn()
		{
			GenericPool<EAS_修改数据项_ModifyDataEntryValue>.Release(this);
			return;
		}

		public static EAS_修改数据项_ModifyDataEntryValue GetFromPool(EAS_修改数据项_ModifyDataEntryValue copyFrom = null)
		{
			var newEAS = new EAS_修改数据项_ModifyDataEntryValue();
			// var newEAS = GenericPool<EAS_修改数据项_ModifyDataEntryValue>.Get();
			if (copyFrom != null)
			{
				newEAS.ModifyInfosList = new List<DataEntryModifyInfo>();
				// newEAS.ModifyInfosList = CollectionPool<List<DataEntryModifyInfo>, DataEntryModifyInfo>.Get();
				foreach (var perD in copyFrom.ModifyInfosList)
				{
					var newD = new DataEntryModifyInfo();
					// var newD = GenericPool<DataEntryModifyInfo>.Get();
					newD.Type = perD.Type;
					newD.OverrideOrModify = perD.OverrideOrModify;
					newD.ModifyValue = perD.ModifyValue;
					newD.CalculatePosition = perD.CalculatePosition;
					newEAS.ModifyInfosList.Add(newD);
				}
			}
			return newEAS;
		}
	}
}