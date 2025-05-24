using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_通用对数据项百分比修饰_CommonModifyDataEntryAsPartial : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;

			[InfoBox("在加算区上，那就是加这么多，比如-30就是减三十\n" + "在乘算区上，是加这么多百分比再乘，比如40就是乘一点四")]
			[SerializeField, LabelText("目标数据修饰")]
			public float Value;
		}


		[SerializeField, LabelText("需要修饰的数据项_不兼容PresentValue"), FoldoutGroup("配置", true)]
		public RP_DataEntry_EnumType DataEntryType;

		[SerializeField, LabelText("算区"), FoldoutGroup("配置", true)]
		public ModifyEntry_CalculatePosition CalculatePosition;



		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		[NonSerialized]
		private Float_RPDataEntry _entry_TargetEntry;

		[NonSerialized]
		private Float_ModifyEntry_RPDataEntry _selfModify;

		public float CurrentModifyValue { get; private set; }

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			if (CalculatePosition == ModifyEntry_CalculatePosition.Original)
			{
				DBug.LogError(
					$" {SelfConfigInstance.name}要求的算区，但是不能为 {ModifyEntry_CalculatePosition.Original}");
			}
			
			_entry_TargetEntry = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(DataEntryType);
			return base.OnBuffInitialized(blps);
		}

		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			return base.OnExistBuffRefreshed(caster, blps);
		}
		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentModifyValue = buffData.Value;
					if (_selfModify == null)
					{
						_selfModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(CurrentModifyValue,
							RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
							CalculatePosition,
							this);
						_entry_TargetEntry.AddDataEntryModifier(_selfModify);
					}
					else
					{
						_selfModify.ModifyValue = CurrentModifyValue;
						_entry_TargetEntry.Recalculate();
					}
					break;
			}
		}


		protected override void ClearAndUnload()
		{
			if (_selfModify != null)
			{
				_entry_TargetEntry.RemoveEntryModifier(_selfModify);

				_selfModify.ReleaseToPool();
			}
			base.ClearAndUnload();
		}



	}
}