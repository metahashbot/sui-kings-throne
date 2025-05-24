using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_青冥弦月_DarkSkyStringMoon : BaseRPBuff , I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("恢复最大的百分比,填30就是回30%"), SuffixLabel("%")]
			public float RestorePartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();


		private Float_RPDataEntry _entry_MaxSP;
		private FloatPresentValue_RPDataEntry _entry_CurrentSP;


		public float CurrentRestorePartial;

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSkillDefaultFinish_技能默认结束了,
				_ABC_CheckIfRestoreSP_OnSkillNormalReleaseEnd);
			_entry_MaxSP = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.SPMax_最大SP);
			_entry_CurrentSP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentSP_当前SP);
			return base.OnBuffInitialized(blps);
		}



		private void _ABC_CheckIfRestoreSP_OnSkillNormalReleaseEnd(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is BaseRPSkill skill)
			{
				var restore = _entry_MaxSP.CurrentValue * (CurrentRestorePartial / 100f);
				_entry_CurrentSP.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(restore,
					RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
					ModifyEntry_CalculatePosition.FrontAdd,
					this));
			}
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentRestorePartial = buffData.RestorePartial;
					break;
			}
		}
		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSkillDefaultFinish_技能默认结束了,
				_ABC_CheckIfRestoreSP_OnSkillNormalReleaseEnd);

			return base.OnBuffPreRemove();
		}
	}
}