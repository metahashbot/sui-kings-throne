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
	public class Buff_AntiTotem_防御反免伤图腾_DefenseBonus : BaseRPBuff , I_BuffTransferWithinPlayer
	{



		public int CurrentStackCount => _currentStackCount;

		[LabelText("关联特效"), TitleGroup("===VFX==="), GUIColor(187f / 255f, 1f, 0f), SerializeField]
		public string _VFX_AntiTotem;



		[SerializeField, LabelText("每层防御提升百分比"), SuffixLabel("%"), TitleGroup("===数值===")]
		private float _attackBonusPerStack = 10f;

		[SerializeField, LabelText("(刷新后)持续时长"), SuffixLabel("秒"), TitleGroup("===数值===")]
		private float _RefreshDuration = 30f;


		private int _currentStackCount = 0;


		private Float_ModifyEntry_RPDataEntry _modifyRef;
		protected Float_RPDataEntry _defenseEntryRef;
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
			//监听其他三种buff的施加，如果有的话立刻移除现在这个
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_RemoveOpponentBuff_OnBuffInit);

			_defenseEntryRef = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.Defense_防御力);
			RefreshDataStackEffect();
			_VFX_GetAndSetBeforePlay(_VFX_AntiTotem)._VFX__10_PlayThis();

			ResetAvailableTimeAs(_RefreshDuration);
			ResetExistDurationAs(_RefreshDuration);
		}


		/// <summary>
		/// <para>刷新数据修饰的效果</para>
		/// </summary>
		private void RefreshDataStackEffect()
		{
			if (_modifyRef == null)
			{
				_modifyRef = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_currentStackCount * _attackBonusPerStack,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontMul,
					this);
				_defenseEntryRef.AddDataEntryModifier(_modifyRef);
			}
			_modifyRef.ModifyValue = _currentStackCount * _attackBonusPerStack;
			_defenseEntryRef.Recalculate();
		}

		/// <summary>
		/// <para>如果有试图施加另外两种buff，直接移除自己</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_RemoveOpponentBuff_OnBuffInit(DS_ActionBusArguGroup ds)
		{
			var buffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			if (buffType == RolePlay_BuffTypeEnum.Level_速度反召唤图腾_SpeedBonusAntiSummon ||
			    buffType == RolePlay_BuffTypeEnum.Level_攻击反回血图腾_AttackBonusAntiHeal)
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
			_VFX_GetAndSetBeforePlay(_VFX_AntiTotem)._VFX__10_PlayThis();
			ResetAvailableTimeAs(_RefreshDuration);
			ResetExistDurationAs(_RefreshDuration);
			return ds;
		}





		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();

			if (_defenseEntryRef != null)
			{
				if (_modifyRef != null)
				{
					_defenseEntryRef.RemoveEntryModifier(_modifyRef);
					_modifyRef.ReleaseToPool();
				}
			}
			_VFX_JustGet(_VFX_AntiTotem)?.VFX_StopThis(true);
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