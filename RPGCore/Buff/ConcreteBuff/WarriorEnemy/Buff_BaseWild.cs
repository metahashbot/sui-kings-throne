using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.WarriorEnemy
{
	[TypeInfoBox("基本狂化：附带弱霸体，获得攻速攻击力移速的百分比提高")]
	[Serializable]
	public class Buff_BaseWild : BaseRPBuff
	{

		[LabelText("最大攻速提升"), FoldoutGroup("配置", true), TitleGroup("配置"), SuffixLabel("%")]
		public float AttackSpeedRate = 50f;

		[LabelText("最大攻击力提升"), FoldoutGroup("配置", true), TitleGroup("配置"), SuffixLabel("%")]
		public float AttackDamageRate = 50f;

		[LabelText("最大移速提升"), FoldoutGroup("配置", true), TitleGroup("配置"), SuffixLabel("%")]
		public float MoveSpeedRate = 50f;

		[SerializeField, LabelText("常驻特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _Vfx_WildLoop;

		protected PerVFXInfo _vfxInfo_WildLoop;


		[SerializeField, LabelText("能力被破坏时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_AbilityBreak;

		protected PerVFXInfo _vfxInfo_AbilityBreak;


		protected Float_ModifyEntry_RPDataEntry _modify_AttackSpeed;
		protected Float_RPDataEntry _entry_attackSpeed;
		protected Float_ModifyEntry_RPDataEntry _modify_AttackDamage;
		protected Float_RPDataEntry _entry_AttackDamage;
		protected Float_ModifyEntry_RPDataEntry _modify_MoveSpeed;
		protected Float_RPDataEntry _entry_MoveSpeed;





		protected Float_RPDataEntry _entry_MaxHP;


		/// <summary>
		/// <para>能力当前活跃吗</para>
		/// </summary>
		protected bool AbilityActive = false;



		[SerializeField, LabelText("重新恢复效果的时长"), FoldoutGroup("配置", true), TitleGroup("配置/数值"), SuffixLabel("秒")]
		protected float _recoveryDuration = 10f;
		protected float _willRecoverTime;


		private Buff_弱霸体_WeakStoic _weakStoicBuffRef;





		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_ProcessModifyValueOnHPChanged_OnCurrentHPChanged);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_DisableAbility_OnOverload);


			_entry_MaxHP = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
			_entry_MoveSpeed =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			_entry_attackSpeed =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			_entry_AttackDamage =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);

			_modify_AttackSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul,
				this);

			_modify_AttackDamage = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul,
				this);

			_modify_MoveSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul,
				this);
		}



		/// <summary>
		/// <para>超载时禁用能力</para>
		/// </summary>
		/// <param name="ds"></param>
		protected virtual void _ABC_DisableAbility_OnOverload(DS_ActionBusArguGroup ds)
		{
			var targetBuffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			if (targetBuffType == RolePlay_BuffTypeEnum.ElementSecondOverload_ChaoZai_二级超载)
			{
				DisableAbility();
			}
		}
		protected virtual void EnableAbility()
		{
			AbilityActive = true;
			var blpSetDuration = GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Get();
			blpSetDuration.SetAllAsResident();
			BuffApplyResultEnum r = Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体,
				Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
				Parent_SelfBelongToObject,
				blpSetDuration);
			blpSetDuration.ReleaseOnReturnToPool();
			_weakStoicBuffRef = Parent_SelfBelongToObject.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体) as Buff_弱霸体_WeakStoic;
			
			
			
			_vfxInfo_WildLoop = _VFX_GetAndSetBeforePlay(_Vfx_WildLoop)._VFX__10_PlayThis(true,true);

			
			ConcreteModifyValueProcess(
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP));
			
			
			_entry_attackSpeed.AddDataEntryModifier(_modify_AttackSpeed);
			_entry_AttackDamage.AddDataEntryModifier(_modify_AttackDamage);
			_entry_MoveSpeed.AddDataEntryModifier(_modify_MoveSpeed);
		}

		protected virtual void DisableAbility()
		{
			if (!AbilityActive)
			{
				return;
			}

			AbilityActive = false;
			_vfxInfo_WildLoop.VFX_StopThis(true);
			_vfxInfo_AbilityBreak = _VFX_GetAndSetBeforePlay(_vfx_AbilityBreak)._VFX__10_PlayThis();


			_weakStoicBuffRef.ResetExistDurationAs(0f);
			_weakStoicBuffRef.ResetAvailableTimeAs(0f);
			
			
			_entry_attackSpeed?.RemoveEntryModifier(_modify_AttackSpeed);
			_modify_AttackSpeed?.ReleaseToPool();
			_modify_AttackSpeed = null;
			_entry_AttackDamage?.RemoveEntryModifier(_modify_AttackDamage);
			_modify_AttackDamage?.ReleaseToPool();
			_modify_AttackDamage = null;
			_entry_MoveSpeed?.RemoveEntryModifier(_modify_MoveSpeed);
			_modify_MoveSpeed?.ReleaseToPool();
			_modify_MoveSpeed = null;
			_willRecoverTime = BaseGameReferenceService.CurrentFixedTime + _recoveryDuration;
		}



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			if (AttackSpeedRate > float.Epsilon || AttackDamageRate > float.Epsilon || MoveSpeedRate > float.Epsilon)
			{
				EnableAbility();
			}


			return base.OnBuffInitialized(blps);
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			switch (blp)
			{
				case BLP_基本狂化_SingleBattleWild blp_SingleBattleWild:

					AttackSpeedRate = blp_SingleBattleWild.AttackSpeedRate;
					AttackDamageRate = blp_SingleBattleWild.AttackDamageRate;
					MoveSpeedRate = blp_SingleBattleWild.MoveSpeedRate;



					break;
			}
		}


		protected virtual void _ABC_ProcessModifyValueOnHPChanged_OnCurrentHPChanged(DS_ActionBusArguGroup ds)
		{
			if (!AbilityActive)
			{
				return;
			}
			switch (ds.ObjectArgu1)
			{
				case FloatPresentValue_RPDataEntry floatPresentValueRPDataEntry:
					if (floatPresentValueRPDataEntry.RP_DataEntryType == RP_DataEntry_EnumType.CurrentHP_当前HP)
					{
						ConcreteModifyValueProcess(floatPresentValueRPDataEntry);
					}
					break;
			}
		}


		protected virtual void ConcreteModifyValueProcess(FloatPresentValue_RPDataEntry floatPresentValueRPDataEntry)
		{
			var currentLoseHPPartial = 1f - floatPresentValueRPDataEntry.CurrentValue / _entry_MaxHP.CurrentValue;

			_modify_AttackDamage.ModifyValue = AttackDamageRate * currentLoseHPPartial;
			_modify_AttackSpeed.ModifyValue = AttackSpeedRate * currentLoseHPPartial;
			_modify_MoveSpeed.ModifyValue = MoveSpeedRate * currentLoseHPPartial;

			_entry_AttackDamage.Recalculate();
			_entry_MoveSpeed.Recalculate();
			_entry_attackSpeed.Recalculate();
			;
			
		}



		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (!AbilityActive && currentTime > _willRecoverTime)
			{
				EnableAbility();
			}
		}

		protected override void ClearAndUnload()
		{
			base.ClearAndUnload();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_DisableAbility_OnOverload);
		}


		
		
		[Serializable]
		public class BLP_基本狂化_SingleBattleWild : BaseBuffLogicPassingComponent
		{
			[LabelText("攻击速度提升比率"), FoldoutGroup("配置", true), SuffixLabel("%")]
			public float AttackSpeedRate = 0f;
			[LabelText("攻击伤害提升比率"), FoldoutGroup("配置", true), SuffixLabel("%")]
			public float AttackDamageRate = 0f;
			[LabelText("移动速度提升比率"), FoldoutGroup("配置", true), SuffixLabel("%")]
			public float MoveSpeedRate = 0f;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_基本狂化_SingleBattleWild>.Release(this);
			}
		}

	}
}