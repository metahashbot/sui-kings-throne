using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.RedFang
{
	[Serializable]
	public class Skill_RedFang_Blink : BaseRPSkill
	{
		/*
		 * 前摇：垫一帧，空
		 * 施法中：垫一帧，空
		 * 后摇：闪烁变色，空
		 * 多段脱手：
		 *       脱手部分计时，不使用Layout
		 *     多段前摇：垫一帧，空
		 *     多段施法中：垫一帧，空
		 *     后摇：闪烁变色
		 * 动作期间全强霸体
		 */
		
        
        

		[SerializeField, LabelText("激活后持续时间"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		private float _activeDuration = 3f;

		[SerializeField, LabelText("每次位移间隔"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		private float _blinkInterval = 0.5f;

		[SerializeField, LabelText("每次位移距离"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		private float _blinkDistance = 5f;


		/// <summary>
		/// 下次可用的闪烁时间点
		/// </summary>
		private float _nextAvailableTime;

		/// <summary>
		/// 激活技能的时间点
		/// </summary>
		private float _skillActiveTime;
		

		private List<Vector3> _posRegistered = new List<Vector3>();
		private int capacity = 30;
		private int perSlotIndex = 3;

		private bool currentActiveSpine = false;
		private float _disableSpineTime;


		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);


			// _sai_release =
			// 	GetAnimationInfoByConfigName(configRuntimeInstance.ContentInSO._AN_SkillReleaseSpineAnimationName);

			var SpineDictInArtHelper = _characterBehaviourRef.GetRelatedArtHelper().CharacterAnimationHelperObjectsDict;
			var SpineAtIndex0 = SpineDictInArtHelper[0];

			for (int i = 0; i < capacity; i++)
			{
				_posRegistered.Add(_characterBehaviourRef.transform.position);
			}
			
		}




		private Vector3 _lastPlayerPos;
		
		
		

#region InternalSkillEffect



		public override bool _Internal_TryPrepareSkill()
		{
			//基类的通用处理
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}

			_skillActiveTime = BaseGameReferenceService.CurrentFixedTime;
            
			ApplyStrongStoic();

			OnSkillConsumeSP();
			_nextAvailableTime = BaseGameReferenceService.CurrentFixedTime - 0.01f;
			RegisteredToDisable = false;
			return true;


		}


		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();
			TryProcessTeleport();
		}


		protected override void _InternalSkillEffect_SkillTakeEffect_OnMultiOffhandPart()
		{
			base._InternalSkillEffect_SkillTakeEffect_OnMultiOffhandPart();
			
			
			TryProcessTeleport();
		}



#endregion

#region InternalProgress

		protected override bool _SkillProgress_Offhand_PrepareSkill()
		{
			if (!base._SkillProgress_Offhand_PrepareSkill())
			{
				return false;
			}


			ApplyStrongStoic();
			return true;
		}



		protected override void _InternalProgress_M_A1_CastingWithoutChargeAnimation()
		{
			base._InternalProgress_M_A1_CastingWithoutChargeAnimation();



		}
		protected override void _InternalProgress_Offhand_CompleteOnPost()
		{
			//重新检查时间，因为有可能就在这短短的时间内技能结束了
			
			RemoveStrongStoic();
			
			
			//还没超时，那就是正常的 CompleteOnPost，
			if (BaseGameReferenceService.CurrentFixedTime > (_skillActiveTime + _activeDuration))
			{
				ReturnToIdleAfterSkill(false);
				SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
					PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
			}
			//已经超时了，那就是
			else
			{
				ReturnToIdleAfterSkill(false);
				SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
					PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中;
				_nextAvailableTime = BaseGameReferenceService.CurrentFixedTime + _blinkInterval;
			}
		}

		protected override void _InternalProgress_Post_CompleteOn_Post()
		{
			base._InternalProgress_Post_CompleteOn_Post();
			RemoveStrongStoic();
		}

#endregion


		protected override bool IfReactToInput_OffhandState()
		{
			//检查有没有到下次可用时间
			if (BaseGameReferenceService.CurrentFixedTime < _nextAvailableTime)
			{
				return false;
			}
			if (_Internal_CheckIfContainsFlagBuff(RP_BuffInternalFunctionFlagTypeEnum.DisableCommonMovement_禁用普通移动 |
			                                      RP_BuffInternalFunctionFlagTypeEnum.BlockByStrongStoic_被强霸体屏蔽 |
			                                      RP_BuffInternalFunctionFlagTypeEnum.ResistByWeakStoic_被弱霸体抵抗))
			{
				return false;
			}
			return true;
		}



		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			_posRegistered.Insert(0,_characterBehaviourRef.transform.position);
			if (_posRegistered.Count > capacity)
			{
				_posRegistered.RemoveAt(capacity);
			}

			_lastPlayerPos = _characterBehaviourRef.transform.position;


			if (currentTime > (_skillActiveTime + _activeDuration) )
			{
				if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
				    PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中 ||
				    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
				    PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇 ||
				    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
				    PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段 ||
				    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
				    PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇)
				{
					ReturnToIdleAfterSkill();
					_InternalSkillEffect_SkillDefaultFinishEffect();
					SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
						PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
				}
			}
			
		}


		private bool RegisteredToDisable = false;

		/// <summary>
		/// <para>试图执行位移。需要当前处于[等待闪烁]并且时间允许的情况</para>
		/// </summary>
		protected void TryProcessTeleport()
		{

			Vector3 rawDir = _playerControllerRef.InputResult_inputMoveOnCurrentGameplayDirectionNormalized;

			Vector3 direction = new Vector3(rawDir.x, 0f, rawDir.y);
			if (direction.sqrMagnitude > 0.01f)
			{
				//大于0则说明和右方同向
				var dotR = Vector3.Dot(direction, BaseGameReferenceService.CurrentBattleLogicRightDirection);
				if (dotR > 0f)
				{
					_characterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(false);
				}
				else
				{
					_characterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(true);
				}
			}
			//没有输入，则向当前朝向左右闪烁
			else
			{
				if (_characterBehaviourRef.GetRelatedArtHelper().CurrentFaceLeft)
				{
					direction = BaseGameReferenceService.CurrentBattleLogicLeftDirection;
				}
				else
				{
					direction = BaseGameReferenceService.CurrentBattleLogicRightDirection;
				}
				
			}
            

			float bd = _blinkDistance;
			//如果此时的移速特别低，就不能闪烁
			if (_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速).CurrentValue < 0.1f)
			{
				bd = 0f;
			}

			Vector3 targetPos = _characterBehaviourRef.TryMovePosition_XYZ(direction * bd);
			

			_nextAvailableTime = BaseGameReferenceService.CurrentFixedTime + _blinkInterval;
			RemoveStrongStoic();

		}


		public override void BR_CommonExitEffect()
		{
			base.BR_CommonExitEffect();

			RemoveStrongStoic();


	
		}
		

	}
}