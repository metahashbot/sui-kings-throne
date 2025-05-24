using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
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
	[System.Serializable]
	public class Buff_战斗狂化_SingleBattleWild : Buff_BaseWild
	{

		[LabelText("狂暴_无视防御值"), SerializeField, FoldoutGroup("配置", true)]
		public float Rampage_DefenseIgnore = 0f;

		protected Float_ModifyEntry_RPDataEntry _modify_DefenseIgnore;

		[LabelText("愤怒_额外眩晕时长"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
		public float Anger_StunTime = 0f;

		[LabelText("渴血_回血之于伤害比例"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("%")]
		public float BloodThirsty_HealRate = 0f;

		[LabelText("自愈_回血比例"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("%")]
		public float SelfHealing_HealRate = 0f;


		[LabelText("警示时间"), FoldoutGroup("配置", true), TitleGroup("配置"), SuffixLabel("秒")]
		public float WarningTime;


		[LabelText("效果持续时间"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
		public float EffectDuration;


		[LabelText("间隔_上次效果结束到下次警示开始"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
		public float IntervalBetweenEffect;


		[SerializeField, LabelText("充能时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_charge;

		protected bool charge_flag = true;
		protected PerVFXInfo _vfxInfo_Charge;

		[SerializeField, LabelText("警示时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_warning;

		protected PerVFXInfo _vfxInfo_Warning;


		[SerializeField, LabelText("释放时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_release;

		[SerializeField, LabelText("自愈时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_SelfHealing;





		protected enum BattleWildStateTypeEnum
		{
			None_无事发生 = 0,
			Interval_间隔中 = 1,
			Warning_警示中 = 2,
			Effect_生效中 = 3, }


		protected BattleWildStateTypeEnum CurrentState = BattleWildStateTypeEnum.None_无事发生;

		/// <summary>
		/// 下次更换状态的时间点
		/// </summary>
		protected float _nextChangeStateTime;




		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_战斗狂化_BattleWildLogicPassing singleBattleWild:
					Rampage_DefenseIgnore = singleBattleWild.Rampage_DefenseIgnore;
					Anger_StunTime = singleBattleWild.Anger_StunTime;
					BloodThirsty_HealRate = singleBattleWild.BloodThirsty_HealRate;
					SelfHealing_HealRate = singleBattleWild.SelfHealing_HealRate;
					WarningTime = singleBattleWild.WarningTime;
					EffectDuration = singleBattleWild.EffectDuration;
					IntervalBetweenEffect = singleBattleWild.IntervalBetweenEffect;


					break;
			}
		}


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
			if (blps != null && blps.Count > 0)
			{
				foreach (BaseBuffLogicPassingComponent perBLP in blps)
				{
					ProcessPerBLP(perBLP);
				}
			}
			if (Rampage_DefenseIgnore > float.Epsilon || Anger_StunTime > float.Epsilon ||
			    BloodThirsty_HealRate > float.Epsilon || SelfHealing_HealRate > float.Epsilon ||
			    AttackSpeedRate > float.Epsilon || AttackDamageRate > float.Epsilon || MoveSpeedRate > float.Epsilon)
			{
				EnableAbility();
				_vfxInfo_WildLoop?.VFX_StopThis(true);
				_vfxInfo_WildLoop = _VFX_GetAndSetBeforePlay(_Vfx_WildLoop);
			}



			DS_ActionBusArguGroup ds =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了);
			ds.IntArgu1 = (int)SelfBuffType;
			ds.ObjectArgu1 = this;
			ds.ObjectArgu2 = blps;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ActionBus_ActionTypeEnum
					.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				ds);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeTakeToHP_对施加方将要对HP进行伤害计算,
				_ABC_AppendDizzy_OnLayoutBuildDamageApplyInfo);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeFrontAdd_对施加方将要进行前加攻防算区计算,
				_ABC_AppendDefenseIgnore_);

			return ds;
		}





		protected override void EnableAbility()
		{
			base.EnableAbility();
			CurrentState = BattleWildStateTypeEnum.Interval_间隔中;
			_nextChangeStateTime = BaseGameReferenceService.CurrentFixedTime + IntervalBetweenEffect;
		}

		protected override void DisableAbility()
		{
			base.DisableAbility();
			_vfxInfo_Warning?.VFX_StopThis(true);
			_vfxInfo_Charge?.VFX_StopThis(true);
			_vfxInfo_WildLoop?.VFX_StopThis(true);
			CurrentState = BattleWildStateTypeEnum.None_无事发生;
		}

		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			switch (CurrentState)
			{
				case BattleWildStateTypeEnum.None_无事发生:
					break;
				case BattleWildStateTypeEnum.Interval_间隔中:
					if (charge_flag)
					{
						_vfxInfo_Charge = _VFX_GetAndSetBeforePlay(_vfx_charge);
						float mulSimSpeed = 10 / IntervalBetweenEffect;
						_vfxInfo_Charge._VFX__6_SetSimulationSpeed(mulSimSpeed)._VFX__10_PlayThis();
						charge_flag = false;
					}

					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = BattleWildStateTypeEnum.Warning_警示中;
						_vfxInfo_Charge.VFX_StopThis(true);
						charge_flag = true;
						_nextChangeStateTime = _nextChangeStateTime + WarningTime;
						_vfxInfo_Warning = _VFX_GetAndSetBeforePlay(_vfx_warning)?._VFX__10_PlayThis();
					}
					break;
				case BattleWildStateTypeEnum.Warning_警示中:
					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = BattleWildStateTypeEnum.Effect_生效中;
						_VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();

						if (SelfHealing_HealRate > 0f)
						{
							_VFX_GetAndSetBeforePlay(_vfx_SelfHealing)?._VFX__10_PlayThis();
						}

						if (_vfx_warning != null)
						{
							_vfxInfo_Warning.VFX_StopThis(true);
						}
						_nextChangeStateTime = _nextChangeStateTime + EffectDuration;
						ActiveBattleWildEffect();
					}
					break;
				case BattleWildStateTypeEnum.Effect_生效中:
					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = BattleWildStateTypeEnum.Interval_间隔中;
						_nextChangeStateTime = _nextChangeStateTime + IntervalBetweenEffect;

						AbilityActive = false;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}



		/// <summary>
		/// <para>激活单体战斗狂化效果</para>
		/// </summary>
		protected virtual void ActiveBattleWildEffect()
		{
			AbilityActive = true;


			if (SelfHealing_HealRate > float.Epsilon)
			{
				HealSelf();
			}
		}


		private void _ABC_AppendDefenseIgnore_(DS_ActionBusArguGroup ds)
		{
			if (!AbilityActive)
			{
				return;
			}
			if (Rampage_DefenseIgnore < float.Epsilon)
			{
				return;
			}
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			dar.CP_APart_DefenseIgnore.AddonPart += 1;
			dar.CP_APart_DefenseIgnore.MultiplyPart += Rampage_DefenseIgnore / 100f;
		}

		private void _ABC_AppendDizzy_OnLayoutBuildDamageApplyInfo(DS_ActionBusArguGroup ds)
		{
			if (!AbilityActive)
			{
				return;
			}
			//施加眩晕
			if (Anger_StunTime > float.Epsilon)
			{
				BLP_设置持续和有效时间_SetDurationAndTime blp = GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Get();
				blp.SetAllAsNotLess(Anger_StunTime);
				var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
				(dar.Receiver as I_RP_Buff_ObjectCanReceiveBuff).ReceiveBuff_TryApplyBuff(
					RolePlay_BuffTypeEnum.Dizzy_眩晕,
					Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
					dar.Caster as I_RP_Buff_ObjectCanReceiveBuff,
					blp);
				blp.ReleaseOnReturnToPool();
			}

			if (BloodThirsty_HealRate > float.Epsilon)
			{
				var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
				var taken = dar.DamageResult_TakenOnHP;
				var heal = taken * (BloodThirsty_HealRate / 100f);
				var selfHP =
					Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
				_damageAssistServiceRef.ApplyDamage((RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(
					Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
					Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
					DamageTypeEnum.Heal_治疗,
					heal,
					DamageProcessStepOption.HealDPS()))).ReleaseToPool();
			}
		}



		private void HealSelf()
		{
			Float_RPDataEntry maxHP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
			FloatPresentValue_RPDataEntry currentHP =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			var amount = maxHP.CurrentValue * (SelfHealing_HealRate / 100f);
			currentHP.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(amount,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontAdd,
				this));
		}




		protected override void ClearAndUnload()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_PLayout_Damage_OnBuildRuntimeDamageApplyInfo_当来自投射物Layout的运行时伤害信息被构建,
				_ABC_AppendDizzy_OnLayoutBuildDamageApplyInfo);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeFrontAdd_对施加方将要进行前加攻防算区计算,
				_ABC_AppendDefenseIgnore_);

			base.ClearAndUnload();
		}


		[Serializable]
		public class BLP_战斗狂化_BattleWildLogicPassing : BLP_基本狂化_SingleBattleWild
		{
			[LabelText("狂暴_无视防御值"), SerializeField, FoldoutGroup("配置", true)]
			public float Rampage_DefenseIgnore = 0f;

			[LabelText("愤怒_额外眩晕时长"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
			public float Anger_StunTime = 0f;

			[LabelText("渴血_回血之于伤害比例"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("%")]
			public float BloodThirsty_HealRate = 0f;

			[LabelText("自愈_回血比例"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("%")]
			public float SelfHealing_HealRate = 0f;

			[LabelText("警示时间"), FoldoutGroup("配置", true), SuffixLabel("秒")]
			public float WarningTime;


			[LabelText("效果持续时间"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
			public float EffectDuration;


			[LabelText("间隔_上次效果结束到下次警示开始"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
			public float IntervalBetweenEffect;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_战斗狂化_BattleWildLogicPassing>.Release(this);
			}
		}


	}
}