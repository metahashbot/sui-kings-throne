using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class Buff_中毒_poisoning : BaseRPBuff
	{




		protected float _nextTakeEffectTime;

		protected float _takeEffectInterval = 1f;

		protected FloatPresentValue_RPDataEntry _currentHP;


		public float _currentTakeEffectHPPartial;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_currentHP = parent.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (currentTime > _nextTakeEffectTime)
			{
				float take = (_currentTakeEffectHPPartial / 100f) * _currentHP.CurrentValue;
				var getNewDAI = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(
					Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
					Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
					DamageTypeEnum.TrueDamage_真伤,
					take,
					DamageProcessStepOption.TrueDamageDPS());
				
				getNewDAI.DamageWorldPosition =
					(Parent_SelfBelongToObject as BaseARPGCharacterBehaviour).transform.position;
				_damageAssistServiceRef.ApplyDamage(getNewDAI);
				_nextTakeEffectTime = currentTime + _takeEffectInterval;
			}
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_Poisoning poisoning:
					_currentTakeEffectHPPartial = poisoning.CurrentHPPartial;
					ResetExistDurationAs(poisoning.NotLessDuration);
					ResetAvailableTimeAs(poisoning.NotLessDuration);
					break;
			}
		}
		
		
		
		

		[Serializable]
		public class BLP_Poisoning : BaseBuffLogicPassingComponent
		{
			public float NotLessDuration;
			public float CurrentHPPartial;


			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_Poisoning>.Release(this);
			}
		}
	}
}