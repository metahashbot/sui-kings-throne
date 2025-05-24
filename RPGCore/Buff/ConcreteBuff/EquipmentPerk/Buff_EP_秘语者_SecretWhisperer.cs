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
	public class Buff_EP_秘语者_SecretWhisperer : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("生效SP比率,填50就是高于50%时生效"), SuffixLabel("%")]
			public float SPPercent;
			[SerializeField, LabelText("增伤,填30就是增加30%"), SuffixLabel("%")]
			public float DamageBonusPercent;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public float CurrentDamageBonusPercent { get; private set; }
		public float ActiveSPPercent { get; private set; }


		private Float_RPDataEntry _entry_maxSP;
		private FloatPresentValue_RPDataEntry _pv_currentSP;




		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_SkillPower_技能威力计算,
				_ABC_CheckIfNeedAddDamage);
			
			_entry_maxSP = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.SPMax_最大SP);
			_pv_currentSP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentSP_当前SP);

			return base.OnBuffInitialized(blps);
		}


		private void _ABC_CheckIfNeedAddDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar == null)
			{
				return;
			}
			
			if (!dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerSkillAttack_玩家技能伤害))
			{
				return;
			}

			var currentV = _pv_currentSP.CurrentValue / _entry_maxSP.CurrentValue;
			if (currentV < ActiveSPPercent)
			{
				return;
			}
			
			dar.CP_SkillPower.MultiplyPart += (CurrentDamageBonusPercent / 100f);

		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentDamageBonusPercent = buffData.DamageBonusPercent;
					ActiveSPPercent = buffData.SPPercent;
					break;
			}
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_SkillPower_技能威力计算,
				_ABC_CheckIfNeedAddDamage);
			return base.OnBuffPreRemove();
		}


	}
}