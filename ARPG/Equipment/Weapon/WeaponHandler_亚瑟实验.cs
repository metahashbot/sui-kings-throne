using System;
using ARPG.Character.Base;
using ARPG.Character.Player;
using Global;
using Global.ActionBus;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPG.Equipment
{
    [Serializable]
    public class WeaponHandler_亚瑟实验 : WeaponHandler_MultiAttack
    {

        private bool _currentMayReplayPAM = false;


        [LabelText("更换动画_第四段原_前摇"), FoldoutGroup("配置", true), TitleGroup("配置/动画")]
        public string original_4_prepare;


        [LabelText("更换动画_第四段更换_前摇"), FoldoutGroup("配置", true), TitleGroup("配置/动画")]
        public string new_4_prepare;



        [LabelText("更换动画_第四段原_中段"), FoldoutGroup("配置", true), TitleGroup("配置/动画")]
        public string original_4_middle;

        [LabelText("更换动画_第四段更换_中段"), FoldoutGroup("配置", true), TitleGroup("配置/动画")]
        public string new_4_middle;


        protected override bool _InternalProcess_TryBeginNewNormalAttack()
        {
	        _currentMayReplayPAM = false;
	        return base._InternalProcess_TryBeginNewNormalAttack();
        }

        protected override bool _InternalProcess_TryBeginNextNormalAttack()
        {
	        return base._InternalProcess_TryBeginNextNormalAttack();
        }

        /// <summary>
		/// <para>输入的 Start。在此开始处理普攻</para>
		/// </summary>
		/// <param name="context"></param>
		public override void _IC_OnStartNormalAttackInput(InputAction.CallbackContext context)
		{
			_inputHolding = true;
			
			//当前没有正在处理的普攻，则试图普攻

			//当前没有正在处理的普攻，则试图普攻
			if (CurrentNormalAttackIndex == -1 || SpecificAttackIndex.HasValue)
			{
				if (!SpecificAttackIndex.HasValue)
				{
					//当前不响应新的输入，那就直接返回
					if (!GetCurrentReactToNewNormalAttackOnInputStart())
					{
						return;
					}
					CurrentNormalAttackIndex = 0;
					_InternalProcess_TryBeginNewNormalAttack();
				}
				else
				{
					CurrentNormalAttackIndex = SpecificAttackIndex.Value - 1;
					if (_InternalProcess_TryBeginNextNormalAttack())
					{
						SpecificAttackIndex = null;
					}
				}

				return;
			}
			//当前不是-1，则说明正在普攻。则处理它
			else
			{
				PlayerWeaponAnimationMotion current = GetCurrentPlayerAnimationMotion();

				LastValidNormalAttackInputTime = BaseGameReferenceService.CurrentTimeInSecond;
				switch (current.PlayerAnimationMotionProgressType)
				{
					case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
						Debug.LogError($"当前普攻索引为{CurrentNormalAttackIndex}，但是状态为NoneSelf，这不应该发生");
						break;
					case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
						if (_currentMayReplayPAM){
							
						
							SelfAllPresetAnimationMotionInfoList[3]._ancn_PrepareAnimationName = new_4_prepare;
							SelfAllPresetAnimationMotionInfoList[3]._ancn_MiddlePartAnimationName = new_4_middle;
							_currentMayReplayPAM = false;
							_InternalProcess_TryReplayCurrentNormalAttack();

						}
                        
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
						if (_currentMayReplayPAM)
						{
							var currentPAM = GetCurrentPlayerAnimationMotion();
							currentPAM._ancn_PrepareAnimationName = new_4_prepare;
							currentPAM._ancn_MiddlePartAnimationName = new_4_middle;
							_InternalProcess_TryReplayCurrentNormalAttack();
							_currentMayReplayPAM = false;
						}
                        
						break;
					case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
						//在整个后摇阶段，可以经由输入接续到下一段
						_InternalProcess_TryBeginNextNormalAttack();
						break;
				}
			}
		}

		protected override bool TryAutoContinueNormalAttack_OnMiddlePartComplete()
		{
			if (CurrentNormalAttackIndex != 3)
			{
				//获取当前这个普通信息的预输入时长配置
				float thresholdTime = SelfAllPresetAnimationMotionInfoList[CurrentNormalAttackIndex]
					.TimeDurationBeforeAutoRelease;

				//如果当前时长已经超过了预输入时长，那就可以【自动】接续普攻

				//需要当前输入依然有效：

				if (!_longPressAsAutoContinue || !_inputHolding)
				{
					return false;
				}
				if ((BaseGameReferenceService.CurrentTimeInSecond - LastValidNormalAttackInputTime) < thresholdTime)
				{
					return false;
				}
				return _InternalProcess_TryBeginNextNormalAttack();
			}

			return false;
		}
        
        
        

		/// <summary>
		/// <para>执行下一次普攻。如果是全新普攻那调用的不是这个。</para>
		/// </summary>
		protected virtual bool _InternalProcess_TryReplayCurrentNormalAttack()
		{
			var cpam = GetCurrentPlayerAnimationMotion();
			cpam.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;

			cpam._prepare_InChargeTime = 0f;
			cpam._Middle_InChargeTime = 0f;
			cpam._prepareAnimation_ChargeMarkedAsRelease = false;
			cpam._middleAnimation_ChargeMarkedAsRelease = false;
			_inputHolding = false;
			//索引是循环的
		
			var nextPAM = SelfAllPresetAnimationMotionInfoList[CurrentNormalAttackIndex];

			var ani = GetAnimationInfoByConfigName(nextPAM._ancn_PrepareAnimationName);



			GetAndOffsetCurrentInputPositionAndDirection();

			if (PresetAttackDirectionType == WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向)
			{
				if (RecordedAttackDirection.HasValue)
				{
					if (Vector3.Dot(RecordedAttackDirection.Value,
						BaseGameReferenceService.CurrentBattleLogicRightDirection) > 0f)
					{
						(_playerControllerRef.CurrentControllingBehaviour.GetSelfRolePlayArtHelper() as
							BaseARPGArtHelper)?.SetFaceLeft(false);
					}
					else
					{
						(_playerControllerRef.CurrentControllingBehaviour.GetSelfRolePlayArtHelper() as
							BaseARPGArtHelper)?.SetFaceLeft(true);
					}
				}
			}
			
			
			var ds_ani = new DS_ActionBusArguGroup(ani,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			ds_ani.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				nextPAM._Prepare_AccelerateByCastingAccelerate ? nextPAM._prepareAnimation_CastingAccelerateMultiplier
					: 0f,
				nextPAM._Prepare_AccelerateByAttackSpeed ? nextPAM._prepareAnimation_AttackSpeedAccelerateMultiplier
					: 0f);
			
			
			RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_ani);

			var ds_result = ds_ani.GetObj2AsT<RP_DS_AnimationPlayResult>();

			//被占用，并没有成功播放
			if (ds_result.PlayBlockedByOccupation)
			{
				DBug.LogWarning($"在正常接续普攻的时候，出现了接续失败的情况，当前索引{CurrentNormalAttackIndex}，这其实并不应该，检查一下");
				return false;
			}

			nextPAM._prepare_InChargeTime = 0f;
			if (nextPAM._Prepare_IsCharging)
			{
				nextPAM.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中;
			}
			else
			{
				nextPAM.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.Prepare_前摇;
			}

			// _currentAbleToContinueAttack = false;
			_thisIndexStartTime = BaseGameReferenceService.CurrentTimeInSecond;
			_thisIndexStartPosition = RelatedCharacterBehaviourRef.transform.position;
			LastValidNormalAttackInputTime = BaseGameReferenceService.CurrentTimeInSecond;

			return true;
		}


		public override void BR_CommonExitEffect()
		{
			base.BR_CommonExitEffect();
			//_currentMayReplayPAM = false;
			SelfAllPresetAnimationMotionInfoList[3]._ancn_PrepareAnimationName = original_4_prepare;
			SelfAllPresetAnimationMotionInfoList[3]._ancn_MiddlePartAnimationName = original_4_middle;
		}

		protected override void PAEC_DefaultProcessPAEC(BasePlayerAnimationEventCallback paec)
		{
			base.PAEC_DefaultProcessPAEC(paec);
			switch (paec)
			{
				case PAEC_亚瑟的连击 paeca:
					
					_currentMayReplayPAM = paeca.ToggleToOn;
					break;
			}
		}
    }
}