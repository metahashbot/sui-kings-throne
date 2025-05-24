using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_坚毅不倒_Steadfastness : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("韧性增加百分比"), SuffixLabel("%")]
			public float ToughnessPartial;
		}




		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		private Float_RPDataEntry _entry_Toughness;

		private Float_ModifyEntry_RPDataEntry _selfModify;

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			_entry_Toughness = Parent_SelfBelongToObject.
				ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.Toughness_韧性);
			return base.OnBuffInitialized(blps);
		}
		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					_selfModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(buffData.ToughnessPartial,
						RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
						ModifyEntry_CalculatePosition.FrontMul,
						this);
					_entry_Toughness.AddDataEntryModifier(_selfModify);
					break;
			}
		}


		protected override void ClearAndUnload()
		{
			if (_selfModify != null)
			{
				_entry_Toughness.RemoveEntryModifier(_selfModify);
				_selfModify.ReleaseToPool();
				;
			}
			base.ClearAndUnload();
		}



	}
}