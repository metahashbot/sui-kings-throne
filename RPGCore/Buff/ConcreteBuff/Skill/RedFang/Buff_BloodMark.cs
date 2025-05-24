using System;
using System.Collections.Generic;
using ARPG.DropItem;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
namespace RPGCore.Buff.ConcreteBuff.Skill.RedFang
{
	/// <summary>
	/// <para>监听L_Damage_OnDamagePreTakenOnHP_伤害将要计算到HP上之前 ，用来处理增加血球恢复量的效果</para>
	/// </summary>
	[Serializable]
	public class Buff_BloodMark : BaseRPBuff
	{

		[SerializeField, LabelText("初始最大层数"), FoldoutGroup("配置", true)]
		private int _initMaxStack = 5;

		[NonSerialized, ShowInInspector, LabelText("当前最大层数"), FoldoutGroup("运行时", true)]
		public int CurrentMaxStack;


		[ShowInInspector, LabelText("当前层数"), FoldoutGroup("运行时", true)]
		public int CurrentStackCount { get; protected set; }


		[SerializeField, LabelText("每层提供的伤害增加"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float AttackPowerPerStackPercentage = 10f;


		[SerializeField, LabelText("血球拾取 —— 治疗量提升百分比"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float HealPowerBonusPercentage = 30f;

		[SerializeField, LabelText("血球拾取 —— 最大生命值护盾百分比"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float HealthShieldBonusPercentage = 15f;




		protected Float_RPDataEntry _entry_AttackPower;

		protected Float_ModifyEntry_RPDataEntry _modifyEntry_AttackPower;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			CurrentMaxStack = _initMaxStack;
			CurrentStackCount = 0;
		}
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);

			_entry_AttackPower =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
			_modifyEntry_AttackPower = _entry_AttackPower.AddDataEntryModifier(
				Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontMul));
			// Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
			// 	ActionBus_ActionTypeEnum.L_Damage_OnHealPreTakenOnHP_治疗将要计算到HP上之前,
			// 	_ABC_ProcessHealBonusOnPickedBigHPBall_OnHealPreTakenHP);

			return ds;
		}

		private void _ABC_ProcessHealBonusOnPickedBigHPBall_OnHealPreTakenHP(DS_ActionBusArguGroup ds)
		{
			// var damageApplyResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			// if (damageApplyResult != null && damageApplyResult.ExtraInfo != null)
			// {
			// 	if (damageApplyResult.ExtraInfo is PerDropItemConfig dropItem)
			// 	{
			// 		switch (dropItem.PresetDropItemType)
			// 		{
			// 			case DropItemTypeEnum.SmallHP_小血球:
			// 			case DropItemTypeEnum.MediumHP_中血球:
			// 			case DropItemTypeEnum.LargeHP_大血球:
			// 				damageApplyResult.TakenOnHealthPower *= (1f + HealPowerBonusPercentage / 100f);
			// 				//TODO刷新护盾
			// 				break;
			// 		}
			// 		if (dropItem.PresetDropItemType == DropItemTypeEnum.LargeHP_大血球 || dropItem.PresetDropItemType == DropItemTypeEnum.MediumHP_中血球 ||
			// 		     dropItem.PresetDropItemType == DropItemTypeEnum.SmallHP_小血球)
			// 		{
			// 			AddBloodMark(1);
			// 		}
			// 	}
			// }
		}

		public bool ConsumeBloodMark(int mark)
		{
			if (CurrentStackCount < mark)
			{
				Debug.LogError($"鲜血咒印试图消耗{mark}层，但当前只有{CurrentStackCount}层，不能这么做");
				return true;
			}
			CurrentStackCount -= mark;
			GlobalActionBus.GetGlobalActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.G_UI_RequireSkillIconRefresh_要求技能图标刷新);
			_modifyEntry_AttackPower.ModifyValue = AttackPowerPerStackPercentage * CurrentStackCount;
			_entry_AttackPower.Recalculate();

			return false;
		}

		public bool AddBloodMark(int stack)
		{
			bool added = false;
			if (CurrentStackCount < CurrentMaxStack)
			{
				_modifyEntry_AttackPower.ModifyValue = AttackPowerPerStackPercentage * CurrentStackCount;
				_entry_AttackPower.Recalculate();
				added = true;
			}
			CurrentStackCount += stack;
			GlobalActionBus.GetGlobalActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.G_UI_RequireSkillIconRefresh_要求技能图标刷新);
			CurrentStackCount = Mathf.Clamp(CurrentStackCount, 0, CurrentMaxStack);


			return added;
		}



		public override int? UI_GetBuffContent_Stack()
		{
			return CurrentStackCount;
		}
	}
}