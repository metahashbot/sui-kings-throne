using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class Buff_ColdStack_寒冷可叠层 : BaseRPBuff , I_BuffTransferWithinPlayer
	{

		[SerializeField, LabelText("最大层数")]
		public int MaxStackCount = 8;
		
		[ShowInInspector,LabelText("当前层数")]
		public int CurrentStack { get; protected set; }



		[SerializeField, LabelText("每层降低移动速度百分比"), SuffixLabel("%")]
		public float MoveSpeedDecreasePerStack = 3f;





		private Float_RPDataEntry _entry_MoveSpeed;
		private Float_ModifyEntry_RPDataEntry _modify_MoveSpeed;
		private I_RP_Buff_ObjectCanReceiveBuff _originalParent;



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
			var init = base.OnBuffInitialized(blps);
			if (_modify_MoveSpeed == null)
			{
				_entry_MoveSpeed =
					Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
				_modify_MoveSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontMul);
			}
			CurrentStack = 1;
			_Internal_RefreshStack();

			
			return init;
		}


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);
		

			CurrentStack += 1;
			_Internal_RequireBuffDisplayContent();
			_Internal_RefreshStack();
			ResetDurationAndAvailableTimeAs(BuffInitDuration, BuffInitDuration);
			if (CurrentStack >= MaxStackCount)
			{

				var buff = Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.Frozen_冻结_DJ,
					caster,
					Parent_SelfBelongToObject);
                MarkAsRemoved = true;
				return ds;
			}

			return ds;
		}



		private void _Internal_RefreshStack()
		{
			_modify_MoveSpeed.ModifyValue = CurrentStack * MoveSpeedDecreasePerStack;
			_entry_MoveSpeed.Recalculate();
			
		}








		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			if (_modify_MoveSpeed != null)
			{
				_entry_MoveSpeed?.RemoveEntryModifier(_modify_MoveSpeed);
				_modify_MoveSpeed.ReleaseToPool();
				_modify_MoveSpeed = null;
			}
			
			return base.OnBuffPreRemove();
		}



		public override int? UI_GetBuffContent_Stack()
		{
			return CurrentStack;
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
			_entry_MoveSpeed?.RemoveEntryModifier(_modify_MoveSpeed);


			Parent_SelfBelongToObject = newPlayer;
			_entry_MoveSpeed = newPlayer.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速);
			_entry_MoveSpeed.AddDataEntryModifier(_modify_MoveSpeed);
			_Internal_RequireBuffDisplayContent();
			
			

		}
	}
}