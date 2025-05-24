using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using Global;
using Global.ActionBus;
using RPGCore.PlayerAnimationMotion;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Equipment.Weapon_Dagger
{
	[Serializable]
	public class WH_Dagger_GiantDagger : WeaponHandler_MultiAttack
	{
		[SerializeField, LabelText("每次命中时动画速度加算")]
		private float _animationSpeedAddonOnHit = 0.1f;

		[SerializeField, LabelText("最大命中导致的速度加值")]
		private float _maxAddon = 3f;

		private float _currentAnimationSpeedAddon = 0f;
		
		[SerializeField,LabelText("多长时间没有再次命中则清空加速效果")]
		private float _clearAddonTime = 1f;

		private float _nextWillClearAddonTime;

		[SerializeField, LabelText("受命中加速的动画名字")]
		private List<string> _animationNamesAffectBySpeedUpList = new List<string>();

		private Dictionary<AnimationInfoBase, float> _originalAnimationSpeedInfo =
			new Dictionary<AnimationInfoBase, float>();


		public override void InitializeOnInstantiate(
			PlayerARPGConcreteCharacterBehaviour behaviour,
			LocalActionBus lab,
			SOConfig_WeaponTemplate configRuntime,
			DamageTypeEnum damageType)
		{
			base.InitializeOnInstantiate(behaviour, lab, configRuntime, damageType);

			foreach (var perConfigName in _animationNamesAffectBySpeedUpList)
			{
				AnimationInfoBase animationConfig = GetAnimationInfoByConfigName(perConfigName);
				if (animationConfig != null)
				{
					_originalAnimationSpeedInfo.Add(animationConfig, animationConfig._animationPresetSpeed);
				}
			}
			behaviour.GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_PLayout_LayoutFirstValidHit_当投射物Layout首次有效命中,
					_ABC_ModifyAnimationSpeedOnHit);
		}


		private int accumulateAttackCount = 0;

		[SerializeField,LabelText("多少次普攻后切换动画")]
		private int _switchAnimationAccumulateCount = 5;
		
		[SerializeField,LabelText("插入动画上限")]
		 private int _insertAnimationLimit = 3;


		[SerializeField,LabelText("额外插入的动画配置")]
		private PlayerWeaponAnimationMotion_ThreePeriodMotion _pam_ToInsert;

		private void InsertAnimation()
		{
			int nextAnimationIndex = CurrentNormalAttackIndex + 1;
			int insertedCount = 0;
			foreach (PlayerWeaponAnimationMotion_ThreePeriodMotion perPAM in SelfAllPresetAnimationMotionInfoList)
			{
				if (perPAM == _pam_ToInsert)
				{
					insertedCount += 1;
				}
			}
			
			if(_insertAnimationLimit > 0 && insertedCount >= _insertAnimationLimit)
			{
				return;
			}

			SelfAllPresetAnimationMotionInfoList.Insert(nextAnimationIndex, _pam_ToInsert);
		}

		private void ClearInsertedAnimation()
		{
			for (int i = SelfAllPresetAnimationMotionInfoList.Count - 1; i >= 0; i--)
			{
				if (SelfAllPresetAnimationMotionInfoList[i] == _pam_ToInsert)
				{
					SelfAllPresetAnimationMotionInfoList.RemoveAt(i);
				}
			}
		}

		private void _ABC_ModifyAnimationSpeedOnHit(DS_ActionBusArguGroup ds)
		{

			_currentAnimationSpeedAddon =
				Mathf.Clamp(_currentAnimationSpeedAddon + _animationSpeedAddonOnHit, 0f, _maxAddon);
			_nextWillClearAddonTime = BaseGameReferenceService.CurrentFixedTime + _clearAddonTime;
			foreach (string perS in _animationNamesAffectBySpeedUpList)
			{
				var configGet = GetAnimationInfoByConfigName(perS);
				if (configGet != null)
				{
					configGet._animationPresetSpeed = _originalAnimationSpeedInfo[configGet] + _currentAnimationSpeedAddon;
				}
			}

			accumulateAttackCount += 1;
			if (accumulateAttackCount >= _switchAnimationAccumulateCount)
			{
				accumulateAttackCount = 0;
				InsertAnimation();
			}
		}

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			if(ct > _nextWillClearAddonTime)
			{
				_currentAnimationSpeedAddon = 0f;
				foreach (string perS in _animationNamesAffectBySpeedUpList)
				{
					var configGet = GetAnimationInfoByConfigName(perS);
					if (configGet != null)
					{
						configGet._animationPresetSpeed = _originalAnimationSpeedInfo[configGet];
					}
				}

				accumulateAttackCount = 0;
				//ClearInsertedAnimation();
			}
		}

		public override void BR_CommonExitEffect()
		{
			base.BR_CommonExitEffect();
			ClearInsertedAnimation();
			
		}

		protected override void ResetNormalAttackIndexToZero()
		{
			base.ResetNormalAttackIndexToZero();
			ClearInsertedAnimation();

		
		}
	}
}