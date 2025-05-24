using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Player;
using ARPG.Common;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Utility;
using RPGCore;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

namespace ARPG.Equipment
{
	[Serializable]
	public class WeaponHandler_MultiAttack : BaseWeaponHandler
	{

#region 配置

		[LabelText("长按输入视作自动接续吗？"), SerializeField]
		protected bool _longPressAsAutoContinue = true;


		[SerializeField, LabelText("PAM-玩家动画动作"),ListDrawerSettings( ShowIndexLabels = true)]
		public List<PlayerWeaponAnimationMotion_ThreePeriodMotion> SelfAllPresetAnimationMotionInfoList;

#endregion

#region 运行时

		protected int _currentNormalAttackIndex;
		/// <summary>
		/// <para>为-1的时候</para>
		/// </summary>
		[ShowInInspector, LabelText("当前普攻索引"), FoldoutGroup("运行时", true)]
		public virtual int CurrentNormalAttackIndex
		{
			get => _currentNormalAttackIndex;
			protected set
			{
				_currentNormalAttackIndex = value;
			}
		}




		protected bool _inputHolding = false;
		protected int _selfMaxIndex;

		[ShowInInspector, LabelText("当前这个索引的起始时间点"), FoldoutGroup("运行时"), ReadOnly]
		protected float _thisIndexStartTime;

		[ShowInInspector, LabelText("当前这个索引的起始位置"), FoldoutGroup("运行时"), ReadOnly]
		protected Vector3 _thisIndexStartPosition;

		// [ShowInInspector, LabelText("当前还能接续普攻吗"), FoldoutGroup("运行时"), ReadOnly]
		// protected bool _currentAbleToContinueAttack;

		protected Float_RPDataEntry _entry_AttackSpeed;
		protected Float_RPDataEntry _entry_CastSpeed;

		public Nullable<int> SpecificAttackIndex = null;

#endregion
#region 初始化

		public override void InitializeOnInstantiate(
			PlayerARPGConcreteCharacterBehaviour behaviour,
			LocalActionBus lab,
			SOConfig_WeaponTemplate configRuntime,
			DamageTypeEnum damageType)
		{
			base.InitializeOnInstantiate(behaviour, lab, configRuntime, damageType);
			_selfMaxIndex = SelfAllPresetAnimationMotionInfoList.Count - 1;
			// _currentAbleToContinueAttack = false;
			CurrentNormalAttackIndex = -1;


			_entry_AttackSpeed = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			_entry_CastSpeed = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速);
		}

#endregion
#region 基本逻辑

		public override bool IfContainsRelatedAnimationConfigName(string configName)
		{
			foreach (PlayerWeaponAnimationMotion_ThreePeriodMotion perPAM in SelfAllPresetAnimationMotionInfoList)
			{
				if (perPAM.ContainsAnimationConfig(configName))
				{
					return true;
				}
			}
			return false;
		}

		public override PlayerWeaponAnimationMotion GetCurrentPlayerAnimationMotion()
		{
			if (SelfAllPresetAnimationMotionInfoList.Count == 0 ||
			    CurrentNormalAttackIndex >= SelfAllPresetAnimationMotionInfoList.Count)
			{
				DBug.LogError($" {SelfConfigRuntimeInstance.name}的普攻动画配置为空，或者当前普攻索引超出了范围");
				return null;
			}
			if (CurrentNormalAttackIndex == -1)
			{
				return null;
			}
			return SelfAllPresetAnimationMotionInfoList[CurrentNormalAttackIndex];
		}


		/// <summary>
		/// <para>记录当前输入位置和方向，并根据当前的攻击信息进行一些修正和重新校准</para>
		/// </summary>
		protected override void GetAndOffsetCurrentInputPositionAndDirection()
		{
			base.GetAndOffsetCurrentInputPositionAndDirection();
			//包含攻击角度限制，则校准一下方向
			var currentPAM = GetCurrentPlayerAnimationMotion() as PlayerWeaponAnimationMotion_ThreePeriodMotion;


			if (currentPAM.ContainAttackDirectionRestriction)
			{
				//限制角度的一半，
				float restrictAngleHalf = currentPAM.AttackDirectionRestrictAngle / 2f;
				//在左边还是右边
				float dotR = Vector3.Dot(BaseGameReferenceService.CurrentBattleLogicRightDirection,
					RecordedAttackDirection.Value);
				//在左边,那就去拿个世界左来算角度
				if (dotR < 0f)
				{
					float currentAngle = Vector3.SignedAngle(BaseGameReferenceService.CurrentBattleLogicLeftDirection,
						RecordedAttackDirection.Value,
						Vector3.up);
					//左侧靠下
					if (currentAngle < 0f)
					{
						currentAngle = Mathf.Abs(currentAngle);
						currentAngle = Mathf.Clamp(currentAngle, 0f, restrictAngleHalf);
						var rot = MathExtend.Vector3RotateOnXOZ(
							BaseGameReferenceService.CurrentBattleLogicLeftDirection,
							currentAngle);
						RecordedAttackDirection = rot;
					}
					//左侧靠上，顺时针转
					else
					{
						currentAngle = Mathf.Abs(currentAngle);
						currentAngle = Mathf.Clamp(currentAngle, 0f, restrictAngleHalf);
						var rot = MathExtend.Vector3RotateOnXOZ(
							BaseGameReferenceService.CurrentBattleLogicLeftDirection,
							-currentAngle);
						RecordedAttackDirection = rot;
					}
				}
				//在右边
				else
				{
					float currentAngle = Vector3.SignedAngle(BaseGameReferenceService.CurrentBattleLogicRightDirection,
						RecordedAttackDirection.Value,
						Vector3.up);
					//右侧靠上，逆时针转
					if (currentAngle < 0f)
					{
						currentAngle = Mathf.Abs(currentAngle);
						currentAngle = Mathf.Clamp(currentAngle, 0f, restrictAngleHalf);
						var rot = MathExtend.Vector3RotateOnXOZ(
							BaseGameReferenceService.CurrentBattleLogicRightDirection,
							currentAngle);
						RecordedAttackDirection = rot;
					}
					//右侧靠下，顺时针转
					else
					{
						currentAngle = Mathf.Abs(currentAngle);
						currentAngle = Mathf.Clamp(currentAngle, 0f, restrictAngleHalf);
						var rot = MathExtend.Vector3RotateOnXOZ(
							BaseGameReferenceService.CurrentBattleLogicRightDirection,
							-currentAngle);
						RecordedAttackDirection = rot;
					}
				}
			}
		}


		protected virtual float GetLogicOffsetAnimationPlaySpeed(float castMul, float attackMul)
		{
			float result = 1f;
			float bonus = 0f;
			bonus += castMul * _entry_CastSpeed.GetCurrentValue();
			bonus += attackMul * _entry_AttackSpeed.GetCurrentValue();

			result += bonus / 100f;
			return result;
		}


		protected override void ResetAllPAMContent()
		{
			foreach (var perPAM in SelfAllPresetAnimationMotionInfoList)
			{
				perPAM.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
				perPAM._prepare_InChargeTime = 0f;
				perPAM._Middle_InChargeTime = 0f;
				perPAM._Middle_EffectingTime = 0f;
			}
		}

#endregion
#region 动画基本响应

		protected override void _ABC_CheckAnimationComplete_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
		{
			if (CurrentNormalAttackIndex == -1)
			{
				return;
			}
			else
			{
				base._ABC_CheckAnimationComplete_OnGeneralAnimationComplete(ds);
				PlayerWeaponAnimationMotion currentPAM = GetCurrentPlayerAnimationMotion();
				switch (currentPAM.PlayerAnimationMotionProgressType)
				{
					case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
						break;
					case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:

						//看看是谁结束了
						//如果是一个不蓄力的前摇，则自动接续到下一段
						_InternalProgress_CompleteOn_Prepare(ds.ObjectArguStr as string,
							ds.GetObj2AsT<SheetAnimationInfo_帧动画配置>(),
							ds.GetObj1AsT<BaseCharacterSheetAnimationHelper>());

						break;
					case PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中:
					case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:

						//前摇蓄力和前摇等待 的动画结束没有实际意义，需要等输入
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
						//不蓄力，自动流转
						_InternalProgress_ProgressToPost();

						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.Post_后摇:


						_InternalProcess_ReturnToIdleOnPostComplete();

						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇:
						break;
				}
			}
		}

#endregion



#region 输入响应

		/// <summary>
		/// <para>输入的 Start。在此开始处理普攻</para>
		/// </summary>
		/// <param name="context"></param>
		public override void _IC_OnStartNormalAttackInput(InputAction.CallbackContext context)
		{
			_inputHolding = true;

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
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
						//在整个后摇阶段，可以经由输入接续到下一段
						_InternalProcess_TryBeginNextNormalAttack();
						break;
				}
			}
		}



		/// <summary>
		/// <para>接收到了普攻输入。如果当前正在攻击（处于后摇，则调用对应方法检测是否需要接续普攻）</para>
		/// </summary>
		public override void _IC_OnNormalAttackPerformed(InputAction.CallbackContext context)
		{
		}




		public override void _IC_OnNormalAttackInputCanceled(InputAction.CallbackContext context)
		{
			_inputHolding = false;
			var cpm = GetCurrentPlayerAnimationMotion();
			if (cpm == null)
			{
				return;
			}
			switch (GetCurrentPlayerAnimationMotion().PlayerAnimationMotionProgressType)
			{
				case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
					break;
				case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
					cpm._prepareAnimation_ChargeMarkedAsRelease = true;
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中:
					cpm._prepareAnimation_ChargeMarkedAsRelease = true;
					//如果已经此时已经释放掉，检查一下是不是立刻触发
					if (cpm._prepare_InChargeTime > cpm._Prepare_MinChargeTime)
					{
						TryAutoContinueNormalAttack_OnMiddlePartComplete();
						return;
					}
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
					break;
				case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇:
					break;
			}
		}

#endregion





#region 动画 & 逻辑的各个子部分

		/// <summary>
		/// <para>试图开启全新普攻，当前索引要从-1变成0了</para>
		/// </summary>
		protected virtual bool _InternalProcess_TryBeginNewNormalAttack()
		{
			var pam_0 = SelfAllPresetAnimationMotionInfoList[0];
			var ani = GetAnimationInfoByConfigName(pam_0._ancn_PrepareAnimationName);


			var ds_ani = new DS_ActionBusArguGroup(ani,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			ds_ani.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				pam_0._Prepare_AccelerateByCastingAccelerate ? pam_0._prepareAnimation_CastingAccelerateMultiplier : 0f,
				pam_0._Prepare_AccelerateByAttackSpeed ? pam_0._prepareAnimation_AttackSpeedAccelerateMultiplier : 0f);
			RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_ani);

			var ds_result = ds_ani.GetObj2AsT<RP_DS_AnimationPlayResult>();

			//被占用，并没有成功播放
			if (ds_result.PlayBlockedByOccupation)
			{
				CurrentNormalAttackIndex = -1;
				return false;
			}
			CurrentNormalAttackIndex = 0;

			pam_0._prepare_InChargeTime = 0f;
			pam_0._Middle_InChargeTime = 0f;
			pam_0._prepareAnimation_ChargeMarkedAsRelease = false;
			pam_0._middleAnimation_ChargeMarkedAsRelease = false;
			pam_0._Middle_EffectingTime = 0f;
			if (pam_0._Prepare_IsCharging)
			{
				pam_0.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中;
			}
			else
			{
				pam_0.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.Prepare_前摇;
			}


			// _currentAbleToContinueAttack = false;
			_thisIndexStartTime = BaseGameReferenceService.CurrentTimeInSecond;
			_thisIndexStartPosition = RelatedCharacterBehaviourRef.transform.position;
			LastValidNormalAttackInputTime = BaseGameReferenceService.CurrentTimeInSecond;

			GetAndOffsetCurrentInputPositionAndDirection();

			switch (PresetAttackDirectionType)
			{
				case WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向:
					case WeaponAttackDirectionTypeEnum.RegisteredCharacterMoveDirectionThenPointer_记录的角色移动方向后指针:
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
					break;
		
			}

			return true;
		}


		protected virtual void ResetNormalAttackIndexToZero()
		{

			CurrentNormalAttackIndex = 0;
		}

		/// <summary>
		/// <para>执行下一次普攻。如果是全新普攻那调用的不是这个。</para>
		/// </summary>
		protected virtual bool _InternalProcess_TryBeginNextNormalAttack()
		{
			var cpam = GetCurrentPlayerAnimationMotion();
			cpam.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;

			cpam._prepare_InChargeTime = 0f;
			cpam._Middle_InChargeTime = 0f;
			cpam._Middle_EffectingTime = 0f;
			cpam._prepareAnimation_ChargeMarkedAsRelease = false;
			cpam._middleAnimation_ChargeMarkedAsRelease = false;
			// _currentAbleToContinueAttack = false;
			CurrentNormalAttackIndex += 1;
			//索引是循环的
			if (CurrentNormalAttackIndex >= SelfAllPresetAnimationMotionInfoList.Count)
			{
				ResetNormalAttackIndexToZero();
			}
			var nextPAM = SelfAllPresetAnimationMotionInfoList[CurrentNormalAttackIndex];

			var ani = GetAnimationInfoByConfigName(nextPAM._ancn_PrepareAnimationName);


			GetAndOffsetCurrentInputPositionAndDirection();

			switch (PresetAttackDirectionType)
			{
				case WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向:
				case WeaponAttackDirectionTypeEnum.RegisteredCharacterMoveDirectionThenPointer_记录的角色移动方向后指针:
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
					break;

			}

			var ds_ani = new DS_ActionBusArguGroup(ani,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
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



#region 中段部分

#endregion




		/// <summary>
		/// <para>试图【自动】接续下一次普攻。</para>
		/// <para>注意这是【自动】的。来自输入的手动接续并不在这里。这里处理的是【中段】结束后的预输入，会在中段结束后跳过后摇部分而直接转到下一段普攻的前摇</para>
		/// <para>在整段后摇动画中，都可以接续到下一次普攻</para>
		/// <para>如果返回false，则表明并没有自动接续，需要在后摇的某个时刻等待输入。</para>
		/// </summary>
		protected virtual bool TryAutoContinueNormalAttack_OnMiddlePartComplete()
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

		/// <summary>
		///<para>流转到【后摇】部分</para>
		/// </summary>
		protected virtual void _InternalProgress_ProgressToPost()
		{
			//试试自动接续。
			//自动接续成功了，相当于预输入有效，跳过后摇的，直接进入下一段普攻的前摇
			if (TryAutoContinueNormalAttack_OnMiddlePartComplete())
			{
				return;
			}
			//没有自动接续，则开始后摇动画
			else
			{
				var pam = GetCurrentPlayerAnimationMotion();
				var ds_post = new DS_ActionBusArguGroup(pam._ancn_PostAnimationName,
					AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
					RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
					true,
					FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
				RelatedCharacterBehaviourRef.GetRelatedActionBus().TriggerActionByType(ds_post);
				var playR = ds_post.GetObj2AsT<RP_DS_AnimationPlayResult>();
				if (playR.PlayBlockedByOccupation)
				{
					DBug.LogError($"在普攻的后摇阶段，被占用了，这不应该发生");
					return;
				}
				pam.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.Post_后摇;
			}
			;
		}


		protected virtual void _InternalProcess_ReturnToIdleOnPostComplete()
		{
			var pam = GetCurrentPlayerAnimationMotion();
			pam.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
			var ds_return = new DS_ActionBusArguGroup(_Cache_ANInfo_BattleIdle,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				false,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			RelatedCharacterBehaviourRef.GetRelatedActionBus().TriggerActionByType(ds_return);
			CurrentNormalAttackIndex = -1;
		}

#endregion

#region 动画响应

		protected override void PAEC_PlayVFXOnAnimationCallback(PAEC_生成特效配置_SpawnVFXFromConfig paec)
		{
			// BasePlayerAnimationEventCallback t = paec.ExecuteByWeapon(RelatedCharacterBehaviourRef, this, false);.
			var vfxInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList_Runtime,
				paec._vfx_UIDToPlay,
				paec._vfx_IncludeDamageVariant,
				GetCurrentDamageType);
			paec.RuntimeVFXInfoRef =
				vfxInfo._VFX_GetPSHandle(true, RelatedCharacterBehaviourRef.GetRelatedVFXContainer());
			if (paec._vfx_IncludeExtraAlign)
			{
				paec.RuntimeVFXInfoRef._VFX_0_ResetTransform()
					._VFX_1_ApplyPresetTransform(RelatedCharacterBehaviourRef.GetVFXHolderInterface());
				if (paec._vfx_AlignByForwardOrRight)
				{
					if (paec._vfx_AlignToWorld)
					{
						vfxInfo._VFX__3_SetDirectionOnForwardOnGlobalY0(RecordedAttackDirection ??
						                                                RelatedCharacterBehaviourRef
							                                                .GetCurrentPlayerFaceDirection());
					}
					else
					{
						vfxInfo._VFX__3_SetDirectionOnForwardOnLocal(RecordedAttackDirection ??
						                                             RelatedCharacterBehaviourRef
							                                             .GetCurrentPlayerFaceDirection());
					}
				}
				else
				{
					if (paec._vfx_AlignToWorld)
					{
						vfxInfo._VFX__3_SetDirectionOnRightOnGlobalY0(-RecordedAttackDirection ??
						                                              -RelatedCharacterBehaviourRef
							                                              .GetCurrentPlayerFaceDirection());
					}
					else
					{
						vfxInfo._VFX__3_SetDirectionOnRightOnLocalY0(-RecordedAttackDirection ??
						                                             -RelatedCharacterBehaviourRef
							                                             .GetCurrentPlayerFaceDirection());
					}
				}
				paec.RuntimeVFXInfoRef._VFX__10_PlayThis(true, false);
			}
			else
			{
				paec.RuntimeVFXInfoRef._VFX__10_PlayThis(true, true);
			}

			switch (paec)
			{
				case PAEC_响应前摇蓄力的特效配置_SpawnVFXConfigAffectByCharge paec_Charge:
					if (paec_Charge.AffectByPrepare)
					{
						var chargedTime = GetCurrentPlayerAnimationMotion()._prepare_InChargeTime;
						var maxTime = GetCurrentPlayerAnimationMotion()._Prepare_MaxChargeTime;
						var minTime = GetCurrentPlayerAnimationMotion()._Prepare_MinChargeTime;
						var mul = (chargedTime - minTime) / (maxTime - minTime);
						var evaFromCurve = paec_Charge.SizeMultiplyCurve.Evaluate(mul);
						var valueFromCurve = evaFromCurve * paec_Charge.SizeMultiplyAtFullCharge;
						vfxInfo.VFX_4_AddScale(valueFromCurve, false);
					}
					else
					{
					}

					break;
			}
		}



		protected override void PAEC_StartDisplacement(PAEC_开始一段位移_StartDisplacement paec, Vector3 direction)
		{
			switch (paec)
			{
				case PAEC_调整作用中时长的位移_StartDisplacementAndModifyEffectDuration paec_modify:
					var chargedTime = GetCurrentPlayerAnimationMotion()._prepare_InChargeTime;
					var maxTime = GetCurrentPlayerAnimationMotion()._Prepare_MaxChargeTime;
					var minTime = GetCurrentPlayerAnimationMotion()._Prepare_MinChargeTime;
					var mul = (chargedTime - minTime) / (maxTime - minTime);
					var evaFromCurve = paec_modify.ModifyDurationCurve.Evaluate(mul);
					var value_Duration = evaFromCurve * paec_modify.MaxAffectDuration *
					                     paec_modify.GetConfigDisplacementTimeMax;
					var value_distance = evaFromCurve * paec_modify.MaxDisplacementDistance *
					                     paec_modify.GetConfigDisplacementDistance;
					var cpam = GetCurrentPlayerAnimationMotion();
					cpam._MiddleEffecting_MinDuration = value_Duration;
					cpam._MiddleEffecting_MaxDuration = value_Duration;
					paec.RuntimeDisplacementTimeMax = value_Duration;
					paec.RuntimeDisplacementDistance = value_distance;
					paec_modify.StartDisplacement(RelatedCharacterBehaviourRef.transform.position,
						RelatedCharacterBehaviourRef,
						direction);
					break;
				default:
					base.PAEC_StartDisplacement(paec, direction);
					break;
			}
		}



		protected override void PAEC_PlayTimelineVFX(PAEC_播放Timeline特效_PlayTimelineVFX paec_timeline)
		{
			/*
			 * 在播放Timeline的时候，
			 * A:如果仅需要正左正右播放，那就把Director的旋转置空。在这种情况下，Director的朝向（蓝色轴）是朝前方的
			 * B: 如果需要校准到朝向，
			 */

			var tt = paec_timeline.RelatedTimeline;

			//匹配伤害类型
			if (paec_timeline.MatchDamageType)
			{
				if (!paec_timeline.DamageTypeList.Contains(GetCurrentDamageType()))
				{
					return;
				}
			}
			var pd = RelatedCharacterBehaviourRef.GetRelatedPlayableDirector();

			if (pd.playableAsset != null)
			{
				pd.Stop();
				pd.time = 0f;
			}
			if (!pd.gameObject.activeInHierarchy)
			{
				pd.gameObject.SetActive(true);
			}

			Vector3 attackDirection = Vector3.zero;
			switch (PresetAttackDirectionType)
			{
				case WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向:
					attackDirection = RecordedAttackDirection.Value;
					break;
				case WeaponAttackDirectionTypeEnum.PointerDirectionInstant_瞬时的输入方向:
					attackDirection = _playerControllerRef.InputResult_AimDirectionOnCurrentGameplayDirectionNormalized;
					break;
				case WeaponAttackDirectionTypeEnum.RegisteredCharacterMovementDirection_记录的角色移动方向:
					break;
				case WeaponAttackDirectionTypeEnum.ControlledByHandler_由具体的Handler实现:
					break;
				case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainRegistered_记录的指针位置:
					attackDirection = (RecordedAttackDirection.Value - RelatedCharacterBehaviourRef.transform.position)
						.normalized;
					break;
				case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainInstant_瞬时的指针位置:
					attackDirection = _playerControllerRef.InputResult_InputPositionOnFloor ?? Vector3.zero;
					break;
				case WeaponAttackDirectionTypeEnum.CharacterPosition_角色位置:
					break;
			}
			Vector3 pdDirection = Vector3.zero;
			if (paec_timeline.UseAimDirection)
			{
				pdDirection = RecordedAttackDirection ?? RelatedCharacterBehaviourRef.GetCurrentPlayerFaceDirection();

				pd.transform.rotation = Quaternion.LookRotation(pdDirection, Vector3.up);
			}
			else
			{
				// pdDirection = RelatedCharacterBehaviourRef.GetCurrentPlayerFaceDirection();
				pd.transform.localRotation = Quaternion.identity;
			}



			pd.playableAsset = tt;
			pd.Play();
		}

#endregion

#region 处理打断

		public override FixedOccupiedCancelTypeEnum OnOccupiedCanceledByOther(
			DS_OccupationInfo occupySourceInfo,
			FixedOccupiedCancelTypeEnum explicitType = FixedOccupiedCancelTypeEnum.None_未指定,
			bool invokeBreakResult = true)
		{
			return base.OnOccupiedCanceledByOther(occupySourceInfo, explicitType, invokeBreakResult);
		}




		public override void BR_CommonExitEffect()
		{
			base.BR_CommonExitEffect();
			CurrentNormalAttackIndex = -1;
		}
		
		

#endregion

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);


			//按当前的逻辑，CIndex不为-1则表示 有那么一个普攻信息正在活跃。那么处理对应的事件
			if (CurrentNormalAttackIndex != -1)
			{
				var currentPAM = GetCurrentPlayerAnimationMotion();
				//处理PAEC
				foreach (var perConfig in SelfPAECRuntimeConfig_All)
				{
					foreach (var perAEC in perConfig.PACConfigInfo.AllAECList_Runtime)
					{
						switch (perAEC)
						{
							case PAEC_开始一段位移_StartDisplacement paec开始一段位移StartDisplacement:
								if (paec开始一段位移StartDisplacement.DisplacementActive)
								{
									Vector3 direction = Vector3.zero;

									switch (paec开始一段位移StartDisplacement.DisplacementType)
									{
										case WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向:
											direction = RecordedAttackDirection.Value;
											break;
										case WeaponAttackDirectionTypeEnum.PointerDirectionInstant_瞬时的输入方向:
											direction = _playerControllerRef
												.InputResult_AimDirectionOnCurrentGameplayDirectionNormalized;
											break;
										case WeaponAttackDirectionTypeEnum.RegisteredCharacterMovementDirection_记录的角色移动方向:
											break;
										case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainRegistered_记录的指针位置:
											direction = RecordedAttackPosition.Value -
											            RelatedCharacterBehaviourRef.transform.position;
											break;
										case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainInstant_瞬时的指针位置:
											direction = _playerControllerRef.InputResult_InputPositionOnFloor ??
											            Vector3.zero;
											break;
										case WeaponAttackDirectionTypeEnum.CharacterPosition_角色位置:
											break;
									}
									paec开始一段位移StartDisplacement.FixedUpdateTick_ProcessDisplacement(ct,
										delta,
										RelatedCharacterBehaviourRef);
								}

								break;
						}
					}
				}


				//处理PAM
				switch (currentPAM.PlayerAnimationMotionProgressType)
				{
					case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
						break;
					case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中:
						currentPAM._prepare_InChargeTime += delta;
						if (currentPAM._prepare_InChargeTime > currentPAM._Prepare_MinChargeTime)
						{
							//已经取消蓄力了，并且满足了最小时间，那就释放掉
							if (currentPAM._prepareAnimation_ChargeMarkedAsRelease)
							{
								_InternalProgress_ProgressToMiddle();
								return;
							}
						}
						//已经超出最大蓄力时间了，那就直接释放掉
						if (currentPAM._prepare_InChargeTime > currentPAM._Prepare_MaxChargeTime)
						{
							currentPAM._prepareAnimation_ChargeMarkedAsRelease = false;
							_InternalProgress_ProgressToMiddle();
						}
						break;
					case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
						currentPAM._Middle_EffectingTime += delta;
						//持续时间已经超过最大时间了，向后流转
						if (currentPAM._Middle_EffectingTime > currentPAM._MiddleEffecting_MaxDuration)
						{
							_InternalProgress_ProgressToPost();
						}
						break;
					case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇:
						break;
				}
			}
			else
			{
				if (_inputHolding)
				{
					if (_InternalProcess_TryBeginNewNormalAttack())
					{
					}
				}
			}
		}



	}

}