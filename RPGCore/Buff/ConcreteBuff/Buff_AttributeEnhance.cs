using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Player.Ally;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff
{
	[Serializable]
	public class Buff_AttributeEnhance : BaseRPBuff
    {
        [LabelText("强化的属性")]
        public RP_DataEntry_EnumType EntryType;
        [LabelText("强化数值")]
        public float EnhanceValue;
        [LabelText("计算位置")]
        public ModifyEntry_CalculatePosition CalculatePosition;
		private Float_RPDataEntry dataEntry;
		private Float_ModifyEntry_RPDataEntry modifyEntry;

        public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
            dataEntry = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(EntryType);
            modifyEntry = Float_ModifyEntry_RPDataEntry.GetNewFromPool(
                EnhanceValue, RPDM_DataEntry_ModifyFrom.FromBuff_Buff, CalculatePosition);
            dataEntry.AddDataEntryModifier(modifyEntry);
            dataEntry.Recalculate();
            return ds;
		}

        public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
        {
            var ds = base.OnExistBuffRefreshed(caster, blps);
            modifyEntry.ModifyValue += EnhanceValue;
            dataEntry.Recalculate();
            return ds;
        }
	}
}