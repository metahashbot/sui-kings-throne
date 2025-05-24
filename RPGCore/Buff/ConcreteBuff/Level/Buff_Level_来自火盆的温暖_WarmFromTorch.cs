using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Level
{
	[Serializable]
	public class Buff_Level_来自火盆的温暖_WarmFromTorch : BaseRPBuff , I_BuffTransferWithinPlayer
	{


		[SerializeField, LabelText("驱散冻伤的层数")]
		private int _stackCount_ToRemoveFrostbite = 12;
		private I_RP_Buff_ObjectCanReceiveBuff _originalParent;


		public int CurrentStackCount { get; private set; }

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			CurrentStackCount = 1;
		}


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
		{
			ResetDurationAndAvailableTimeAs(BuffInitDuration, BuffInitAvailableTime);
			CurrentStackCount += 1;
			if (CurrentStackCount >= _stackCount_ToRemoveFrostbite)
			{
				Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.Frostbite_冻伤);
				CurrentStackCount = 1;
			}
			
			return base.OnExistBuffRefreshed(caster, blps);
		}

		public override int? UI_GetBuffContent_Stack()
		{
			return CurrentStackCount;
		}


		public override string UI_GetBuffContent_RemainingTimeText()
		{
			return "";
		}

		I_RP_Buff_ObjectCanReceiveBuff I_BuffTransferWithinPlayer.OriginalParent
		{
			get => _originalParent;
			set => _originalParent = value;
		}
		public void ProcessTransfer(I_BuffTransferWithinPlayer transferFrom, PlayerARPGConcreteCharacterBehaviour newPlayer)
		{
			Parent_SelfBelongToObject = newPlayer;
			_Internal_RequireBuffDisplayContent();
			;
		}
	}
}