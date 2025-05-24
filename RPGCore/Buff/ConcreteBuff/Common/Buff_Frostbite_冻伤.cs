using System;
using System.Collections.Generic;
using ARPG.Character;
using Global;
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
	public class Buff_Frostbite_冻伤 : BaseRPBuff , I_BuffTransferWithinPlayer
	{
		private I_RP_Buff_ObjectCanReceiveBuff _originalParent;


		[SerializeField, LabelText("攻速移速施法效率降低百分比"), TitleGroup("===具体配置==="), SuffixLabel("%")]
		public float _entryReducePartial = 30f;

		[SerializeField, LabelText("丢失生命值的百分比"), TitleGroup("===具体配置==="), SuffixLabel("%")]
		public float _entryLoseHpPartial = 10f;

		[SerializeField, LabelText("丢失生命值的间隔"), TitleGroup("===具体配置==="), SuffixLabel("秒")]
		public float _entryLoseHpInterval = 5f;

		[SerializeField, LabelText("vfx_冻伤特效配置名")] 
		public string _vfxID;
		private PerVFXInfo _vfxInfo_Frostbite;
		
		
		private Float_RPDataEntry _entry_AttackSpeed;
		private Float_ModifyEntry_RPDataEntry _modify_AttackSpeed;
		private Float_RPDataEntry _entry_MoveSpeed;
		private Float_ModifyEntry_RPDataEntry _modify_MoveSpeed;
		private Float_RPDataEntry _entry_CastSpeed;
		private Float_ModifyEntry_RPDataEntry _modify_CastSpeed;

		private FloatPresentValue_RPDataEntry _entry_CurrentHP;
		
		private float _nextLoseHpTime = 0f;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
					_ABC_BlockColdStack_OnBuffPreAdd);


			_entry_AttackSpeed = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
			_modify_AttackSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(-_entryReducePartial,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul);
			_entry_AttackSpeed.AddDataEntryModifier(_modify_AttackSpeed);
			
			_entry_MoveSpeed = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			_modify_MoveSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(-_entryReducePartial,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul);
			_entry_MoveSpeed.AddDataEntryModifier(_modify_AttackSpeed);


			_entry_CastSpeed =
				parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速);
			_modify_CastSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(-_entryReducePartial,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontAdd);
			_entry_CurrentHP = parent.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
            

		}



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_nextLoseHpTime = BaseGameReferenceService.CurrentFixedTime;
			_vfxInfo_Frostbite = _VFX_GetAndSetBeforePlay(_vfxID, false)
				._VFX_1_ApplyPresetTransform(Parent_SelfBelongToObject.GetRelatedVFXContainer())
				._VFX__10_PlayThis(true, true);
			Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.ColdStack_严寒可叠层);
			return ds;
		}


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
		{
			_nextLoseHpTime = BaseGameReferenceService.CurrentFixedTime;
			_vfxInfo_Frostbite = _VFX_GetAndSetBeforePlay(_vfxID, false)._VFX__10_PlayThis(true, true);
			Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.ColdStack_严寒可叠层);
			return base.OnExistBuffRefreshed(caster, blps);
		}



		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (currentTime > _nextLoseHpTime)
			{
				_nextLoseHpTime = currentTime + _entryLoseHpInterval;
				var loseHp = _entry_CurrentHP.CurrentValue * (_entryLoseHpPartial / 100f);
				
				
				RP_DS_DamageApplyInfo damage = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(
					Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
					Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
					DamageTypeEnum.TrueDamage_真伤,
					loseHp,
					DamageProcessStepOption.TrueDamageDirectValueDPS());
				
				_damageAssistServiceRef.ApplyDamage(damage).ReleaseToPool();
				// damage.ReleaseBeforeToPool();
			}
		}



		/// <summary>
		/// <para>已经有冻伤的时候，不会再添加寒冷了</para>
		/// </summary>
		private void _ABC_BlockColdStack_OnBuffPreAdd(DS_ActionBusArguGroup ds)
		{

			if ((RolePlay_BuffTypeEnum)ds.IntArgu1.Value !=
			    RolePlay_BuffTypeEnum.ColdStack_严寒可叠层)
			{
				return;
			}

			var result = ds.GetObj2AsT<RP_DS_BuffApplyResult>();
			result.BlockByOtherBuff = true;
			
			
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();
			_vfxInfo_Frostbite?.VFX_StopThis(true);
			
			return ds;
		}



		I_RP_Buff_ObjectCanReceiveBuff I_BuffTransferWithinPlayer.OriginalParent
		{
			get => _originalParent;
			set => _originalParent = value;
		}
		public void ProcessTransfer(I_BuffTransferWithinPlayer transferFrom, PlayerARPGConcreteCharacterBehaviour newPlayer)
		{

			_entry_AttackSpeed?.RemoveEntryModifier(_modify_MoveSpeed);
			_entry_MoveSpeed?.RemoveEntryModifier(_modify_MoveSpeed);
			_entry_CastSpeed?.RemoveEntryModifier(_modify_CastSpeed);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_BlockColdStack_OnBuffPreAdd);

			Parent_SelfBelongToObject = newPlayer;


			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_BlockColdStack_OnBuffPreAdd);
			_entry_AttackSpeed = newPlayer.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
			_entry_AttackSpeed.AddDataEntryModifier(_modify_AttackSpeed);
			_entry_MoveSpeed = newPlayer.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			_entry_MoveSpeed.AddDataEntryModifier(_modify_AttackSpeed);
			_entry_CastSpeed =
				newPlayer.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速);
			_entry_CastSpeed.AddDataEntryModifier(_modify_CastSpeed);
			_entry_CurrentHP = newPlayer.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			_Internal_RequireBuffDisplayContent();
			
			 



		}



		public override string UI_GetBuffContent_RemainingTimeText()
		{
			return "";
		}
	}
}