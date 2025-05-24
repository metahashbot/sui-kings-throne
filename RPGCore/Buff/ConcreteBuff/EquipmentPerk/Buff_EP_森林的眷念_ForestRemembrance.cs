using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_森林的眷念_ForestRemembrance : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("SP减少百分比"), SuffixLabel("%")]
			public float SPReduce;
			[SerializeField, LabelText("施法效率提升百分比"), SuffixLabel("%")]
			public float CastEfficiencyBonus;
		}



		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public float CurrentSPReduce { get; private set; }
		public float CurrentEfficiencyBonus { get; private set; }

		private Float_RPDataEntry _entry_Casting;
		private Float_ModifyEntry_RPDataEntry _selfModify_Casting;

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSKillReadyToConsumeSP_技能将要消耗SP,
				_ABC_ProcessSPReduce_OnConsumeSP);

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
					CurrentEfficiencyBonus = buffData.CastEfficiencyBonus;
					CurrentSPReduce = buffData.SPReduce;

					var t = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(
						RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速,
						true);
					if (t == null)
					{
						(Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour)
							.InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速, 0f);
						t = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType
							.SkillCastingAccelerate_技能施法额外加速,
							false);

					}
					_entry_Casting = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(
						RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速,
						false);
					_selfModify_Casting = Float_ModifyEntry_RPDataEntry.GetNewFromPool(buffData.CastEfficiencyBonus,
						RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
						ModifyEntry_CalculatePosition.FrontAdd,
						this);
					t.AddDataEntryModifier(_selfModify_Casting);
					
					break;
			}
		}
		private void _ABC_ProcessSPReduce_OnConsumeSP(DS_ActionBusArguGroup ds)
		{
			var baseSkill = ds.ObjectArgu1 as BaseRPSkill;
			baseSkill.CurrentSPConsume *= (1f - (CurrentEfficiencyBonus / 100f));
		}




		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			_entry_Casting.RemoveEntryModifier( _selfModify_Casting);
			_selfModify_Casting.ReleaseToPool();
			;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSKillReadyToConsumeSP_技能将要消耗SP,
				_ABC_ProcessSPReduce_OnConsumeSP);
			return base.OnBuffPreRemove();
		}





	}
}