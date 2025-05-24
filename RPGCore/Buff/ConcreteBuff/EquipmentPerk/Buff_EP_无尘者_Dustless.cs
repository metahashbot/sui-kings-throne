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
	public class Buff_EP_无尘者_Dustless : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("生效HP比率,填50就是少于50%时生效"), SuffixLabel("%")]
			public float HPPercent;
			[SerializeField, LabelText("免伤,填30就是回30%"), SuffixLabel("%")]
			public float DamageReducePartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public float CurrentRestorePartial { get; private set; }
		public float ActiveSPPercent { get; private set; }


		private Float_RPDataEntry _entry_maxHP;
		private FloatPresentValue_RPDataEntry _pv_currentHP;



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			_entry_maxHP = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
			_pv_currentHP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			return base.OnBuffInitialized(blps);
		}

		private void _ABC_CheckIfNeedReduceDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar == null)
			{
				return;
			}
			var currentV = _pv_currentHP.CurrentValue / _entry_maxHP.CurrentValue;
			if (currentV > ActiveSPPercent)
			{
				return;
			}

			dar.CP_DamageAmount_DPart.MultiplyPart -= (CurrentRestorePartial / 100f);
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentRestorePartial = buffData.DamageReducePartial;
					ActiveSPPercent = buffData.HPPercent;
					break;
			}
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			return base.OnBuffPreRemove();
		}


	}
}