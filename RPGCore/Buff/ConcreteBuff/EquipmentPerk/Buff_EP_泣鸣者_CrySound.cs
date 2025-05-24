using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_泣鸣者_CrySound : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("目标持续时长"), SuffixLabel("秒")]
			public float Duration;
			[SerializeField, LabelText("技能急速提升百分比"), SuffixLabel("%")]
			public float SkillAccelerateBonusPercent;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();


		public float CurrentDuration { get; private set; }
		public float CurrentSkillAccelerateBonusPercent { get; private set; }





		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeTakeToHP_对施加方将要对HP进行伤害计算,
				_ABC_CheckIfNeedAppend);
			return base.OnBuffInitialized(blps);
		}


		private void _ABC_CheckIfNeedAppend(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (!dar.IsDamageCauseCritical)
			{
				return;
			}
			var blp_ = GenericPool<BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify>.Get();

			blp_.TargetUID = GetHashCode().ToString();
			blp_.TargetEntry = RP_DataEntry_EnumType.SkillAccelerate_技能加速;
			blp_.CalculatePosition = ModifyEntry_CalculatePosition.FrontAdd;
			blp_.ModifyValue = CurrentSkillAccelerateBonusPercent;
			blp_.ModifyDuration = CurrentDuration;
			blp_.OverrideStack = true;
			blp_.OverrideStackAs = 1;
			Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(
				RolePlay_BuffTypeEnum.CommonDataEntryModifyStackable_通用数据项修正可叠层,
				Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
				Parent_SelfBelongToObject,
				blp_);
			GenericPool<BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify>.Release(blp_);
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentSkillAccelerateBonusPercent = buffData.SkillAccelerateBonusPercent;
					CurrentDuration = buffData.Duration;
					break;
			}
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeTakeToHP_对施加方将要对HP进行伤害计算,
				_ABC_CheckIfNeedAppend);
			return base.OnBuffPreRemove();
		}






	}
}