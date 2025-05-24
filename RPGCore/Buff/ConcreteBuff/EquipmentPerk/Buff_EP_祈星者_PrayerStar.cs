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
	public class Buff_EP_祈星者_PrayerStar : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("生效HP比率,填50就是高于50%时生效"), SuffixLabel("%")]
			public float HPPercent;
			[SerializeField, LabelText("移速提升,填30就是增加30%"), SuffixLabel("%")]
			public float MoveSpeedBonusPercent;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public float CurrentMoveSpeedBonusPercent { get; private set; }
		public float ActiveHPPercent { get; private set; }


		private Float_RPDataEntry _entry_MaxHP;
		private FloatPresentValue_RPDataEntry _pv_currentHP;

		private Float_RPDataEntry _entry_MoveSpeed;
		private Float_ModifyEntry_RPDataEntry _modifyOnMoveSpeed;


		private bool currentModify = false;


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			currentModify = false;


			_entry_MaxHP = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
			_pv_currentHP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			_entry_MoveSpeed =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);


			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_CheckIfNeedModifyMoveSpeed);
			return base.OnBuffInitialized(blps);
		}


		private void Activate()
		{
			if (currentModify)
			{
				return;
			}
			currentModify = true;
			_entry_MoveSpeed.AddDataEntryModifier(_modifyOnMoveSpeed);
		}

		private void Deactivate()
		{
			if (!currentModify)
			{
				return;
			}
			currentModify = false;

			_entry_MoveSpeed.RemoveEntryModifier(_modifyOnMoveSpeed);
			
		}
		private void _ABC_CheckIfNeedModifyMoveSpeed(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is FloatPresentValue_RPDataEntry pv)
			{
				if (pv.RP_DataEntryType == RP_DataEntry_EnumType.CurrentHP_当前HP)
				{
					float v = pv.CurrentValue / _entry_MaxHP.CurrentValue;
					if (v > ActiveHPPercent)
					{
						Deactivate();
					}
					else
					{
						Activate();
					}
				}
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
					CurrentMoveSpeedBonusPercent = buffData.MoveSpeedBonusPercent;
					ActiveHPPercent = buffData.HPPercent;
					_modifyOnMoveSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(CurrentMoveSpeedBonusPercent,
						RPDM_DataEntry_ModifyFrom.FromEquipment_来自装备,
						ModifyEntry_CalculatePosition.FrontMul,
						this);

					float p = _pv_currentHP.CurrentValue / _entry_MaxHP.CurrentValue;

					if (p > ActiveHPPercent)
					{
						Activate();
					}

					break;
			}
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_CheckIfNeedModifyMoveSpeed);
			if (_entry_MoveSpeed != null)
			{

				_entry_MoveSpeed.RemoveEntryModifier(_modifyOnMoveSpeed);
				_modifyOnMoveSpeed.ReleaseToPool();
				
			}

			return base.OnBuffPreRemove();
		}


	}
}