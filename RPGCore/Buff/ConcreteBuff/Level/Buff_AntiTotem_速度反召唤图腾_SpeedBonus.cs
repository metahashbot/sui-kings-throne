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
namespace RPGCore.Buff.ConcreteBuff.Level
{
	[Serializable]
	public class Buff_AntiTotem_速度反召唤图腾_SpeedBonus  : BaseRPBuff , I_BuffTransferWithinPlayer
	{


		public int CurrentStackCount => _currentStackCount;
		[LabelText("关联特效") , TitleGroup("===VFX==="),GUIColor(187f / 255f, 1f, 0f) ,SerializeField]
		public string _VFX_AntiTotem;
		


		[SerializeField, LabelText("每移动速度提升百分比"), SuffixLabel("%"), TitleGroup("===数值===")]
		private float _moveSpeedBonusPerStack = 10f;
		[SerializeField,LabelText("每层攻击速度提升百分比"), SuffixLabel("%"),TitleGroup("===数值===")]
		private float _attackSpeedBonusPerStack = 10f;

		[SerializeField, LabelText("(刷新后)持续时长"), SuffixLabel("秒"), TitleGroup("===数值===")]
		private float _RefreshDuration = 30f;


		private int _currentStackCount = 0;


		private Float_ModifyEntry_RPDataEntry _attackSpeedModifyEntry;
		protected Float_RPDataEntry _attackSpeedEntryRef;
		
		private Float_ModifyEntry_RPDataEntry _moveSpeedModifyEntry;
		protected Float_RPDataEntry _moveSpeedEntryRef;
		private I_RP_Buff_ObjectCanReceiveBuff _originalParent;



		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_currentStackCount = 1;
			_originalParent = parent;
			Parent_SelfBelongToObject = parent;

			//监听其他三种buff的施加，如果有的话立刻移除现在这个
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_RemoveOpponentBuff_OnBuffInit);

			_attackSpeedEntryRef = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			_moveSpeedEntryRef = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			
			RefreshDataStackEffect();
			//_VFX_GetAndSetBeforePlay(_VFX_AntiTotem)._VFX__10_PlayThis();

			ResetAvailableTimeAs(_RefreshDuration);
			ResetExistDurationAs(_RefreshDuration);
		}


		/// <summary>
		/// <para>刷新数据修饰的效果</para>
		/// </summary>
		private void RefreshDataStackEffect()
		{
			if (_moveSpeedModifyEntry == null)
			{
				_moveSpeedModifyEntry = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_currentStackCount * _moveSpeedBonusPerStack,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontMul,
					this);
				_moveSpeedEntryRef.AddDataEntryModifier(_moveSpeedModifyEntry);
				
				_attackSpeedModifyEntry = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_currentStackCount * _attackSpeedBonusPerStack,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontMul,
					this);
				_attackSpeedEntryRef.AddDataEntryModifier(_attackSpeedModifyEntry);
			}
			_moveSpeedModifyEntry.ModifyValue =  _currentStackCount * _moveSpeedBonusPerStack;
			_moveSpeedEntryRef.Recalculate();
			 
			_attackSpeedModifyEntry.ModifyValue = _currentStackCount * _attackSpeedBonusPerStack;
			_attackSpeedEntryRef.Recalculate();
		}

		/// <summary>
		/// <para>如果有试图施加另外两种buff，直接移除自己</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_RemoveOpponentBuff_OnBuffInit(DS_ActionBusArguGroup ds)
		{
			var buffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			if (buffType == RolePlay_BuffTypeEnum.Level_攻击反回血图腾_AttackBonusAntiHeal ||
			    buffType == RolePlay_BuffTypeEnum.Level_防御反免伤图腾_DefenseBonusAntiResist)
			{
				ResetDurationAndAvailableTimeAs(-0.01f, -0.01f, false);
				MarkAsRemoved = true;
			}
		}


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);
			RefreshDataStackEffect();
			_currentStackCount += 1;
			// _VFX_GetAndSetBeforePlay(_VFX_AntiTotem)._VFX__10_PlayThis();
			ResetAvailableTimeAs(_RefreshDuration);
			ResetExistDurationAs(_RefreshDuration);
			return ds;
		}





		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();

			_moveSpeedEntryRef?.RemoveEntryModifier(_moveSpeedModifyEntry);
			_attackSpeedEntryRef?.RemoveEntryModifier(_attackSpeedModifyEntry);
			_moveSpeedModifyEntry?.ReleaseToPool();
			_attackSpeedModifyEntry?.ReleaseToPool();
//			_VFX_JustGet(_VFX_AntiTotem)?.VFX_StopThis(true);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
			ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
			_ABC_RemoveOpponentBuff_OnBuffInit);
			return ds;
		}
		
		public override int? UI_GetBuffContent_Stack()
		{
			return _currentStackCount;
		}

		I_RP_Buff_ObjectCanReceiveBuff I_BuffTransferWithinPlayer.OriginalParent
		{
			get => _originalParent;
			set => _originalParent = value;
		}
		public void ProcessTransfer(I_BuffTransferWithinPlayer transferFrom, PlayerARPGConcreteCharacterBehaviour newPlayer)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_RemoveOpponentBuff_OnBuffInit);
			Parent_SelfBelongToObject = newPlayer;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_RemoveOpponentBuff_OnBuffInit);
		}
	}
}