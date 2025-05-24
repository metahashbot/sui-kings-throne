using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_极限武力_UltimateForce : BaseRPBuff
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("攻击力增加百分比"), SuffixLabel("%")]
			public float AttackPower;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();


		private Float_RPDataEntry _entry_attack;

		private Float_ModifyEntry_RPDataEntry _selfModify;


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			_entry_attack =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
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

					_selfModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(buffData.AttackPower ,
						RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
						ModifyEntry_CalculatePosition.FrontMul,
						this);
					_entry_attack.AddDataEntryModifier(_selfModify);
					break;
			}
		}




		protected override void ClearAndUnload()
		{
			if(_selfModify!=null)
			{
				_entry_attack.RemoveEntryModifier(_selfModify);
				_selfModify.ReleaseToPool();
			}
			base.ClearAndUnload(); 
		}



	}
}