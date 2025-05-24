using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;


namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_愤怒伤害_AngerInjury : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("暴击伤害增加数值,40%就是增加40%爆伤")]
			public float DefensePartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		private Float_RPDataEntry _entry_CriticalBonus;
		private Float_ModifyEntry_RPDataEntry _selfModify;



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			_entry_CriticalBonus = Parent_SelfBelongToObject
				.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.CriticalBonus_暴击伤害);
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

					_selfModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(buffData.DefensePartial ,
						RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
						ModifyEntry_CalculatePosition.FrontAdd,
						this);
					_entry_CriticalBonus.AddDataEntryModifier(_selfModify);
					break;
			}
		}


		protected override void ClearAndUnload()
		{
			if (_selfModify != null)
			{
				
			_entry_CriticalBonus.RemoveEntryModifier(_selfModify);
			_selfModify.ReleaseToPool();
			}
			base.ClearAndUnload();
		}



		
		
	}
}