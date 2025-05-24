using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Utility
{
	[Serializable]
	[TypeInfoBox("玩家切换角色的阻塞")]
	public class Buff_PlayerSwitchCharacterBlock : BaseRPBuff
	{
	
		[LabelText("默认单次阻塞时长"),FoldoutGroup("配置",true),SerializeField]
		public float DefaultBlockDuration;

		[LabelText("当前单次阻塞时长"), FoldoutGroup("运行时", true), NonSerialized]
		public float CurrentSingleBlockDuration;
		

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{

			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			CurrentSingleBlockDuration = DefaultBlockDuration;

			ResetExistDurationAs(-1f);

		}



		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			ResetAvailableTimeAs(CurrentSingleBlockDuration);
			return base.OnExistBuffRefreshed(caster, blps);
		}




	}
}