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
	public class Buff_EP_沉稳步伐_SteadyStep : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{

		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("位移重量的修正百分比"), SuffixLabel("%")]
			public float MassWeightPartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();


		public float CurrentMassWeightOffset { get; private set; }

		private Float_RPDataEntry _entry_Mass;

		private Float_ModifyEntry_RPDataEntry _selfModify;




		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			_entry_Mass =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(
					RP_DataEntry_EnumType.MovementMass_重量);
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
					CurrentMassWeightOffset = buffData.MassWeightPartial;
					_selfModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(
						buffData.MassWeightPartial ,
						RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
						ModifyEntry_CalculatePosition.FrontMul,
						this);
					_entry_Mass.AddDataEntryModifier(_selfModify);
					break;
			}
		}

		protected override void ClearAndUnload()
		{
			if (_selfModify != null)
			{
				_entry_Mass.RemoveEntryModifier(_selfModify);
				_selfModify.ReleaseToPool();
			}
			base.ClearAndUnload();
		}

	}
}