using System;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff
{
	[Serializable]
	public class Buff_ChangeCommonDamageType : BaseRPBuff
	{



		public DamageTypeEnum DamageTypeBeforeChange = DamageTypeEnum.NoType_无属性;


		[LabelText("当前的伤害类型"),FoldoutGroup("运行时",true)]
		public DamageTypeEnum CurrentDamageType { get; private set; }

		[SerializeField,LabelText("需要变化为的伤害类型"),FoldoutGroup("配置",true)]
		public DamageTypeEnum ChangeToDamageTypeOnObtain = DamageTypeEnum.NoType_无属性;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			AssignAndBroadcastChangeDamageType(ChangeToDamageTypeOnObtain);
			
		}

		public void AssignAndBroadcastChangeDamageType(DamageTypeEnum newDamageType)
		{
			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnCommonDamageTypeChanged_当常规伤害类型发生变动);
			ds.IntArgu1 = (int) DamageTypeBeforeChange;
			ds.IntArgu2 = (int)newDamageType;
			CurrentDamageType = newDamageType;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.TriggerActionByType(ds);
			DamageTypeBeforeChange = CurrentDamageType;
			var ds_refreshIcon =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_RequireSkillIconRefresh_要求技能图标刷新);
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_refreshIcon);
		}

		public override string ToString()
		{
			return $"当前伤害类型位{CurrentDamageType}，变换前是{DamageTypeBeforeChange}";
		}


	}
}