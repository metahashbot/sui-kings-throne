using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Character.Player;
using ARPG.Equipment;
using ARPG.Manager;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Buff;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Swordman
{
	[Serializable]
	public class Skill_Swordman_Dash : BaseRPSkill
	{
		
		[SerializeField, LabelText("搜索时 额外的不搜索类型们"),]
		[TitleGroup("===配置===")]
		public List<CharacterNamedTypeEnum> _notSearchEnemyTypes = new List<CharacterNamedTypeEnum>();


		[SerializeField,LabelText("吸附搜索范围")]
		[TitleGroup("===配置===")]
		public float _searchRange = 10f;
		
		[LabelText("未强化中段动画"), SerializeField] 
		private string _ancn_middle_normal;

		[LabelText("强化后中段动画"), SerializeField,]
		private string _ancn_middle_charged;


		private SOConfig_ProjectileLayout _selfLayoutRef;

		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
					_ABC__CheckIfRefreshCD );
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Skill_OnSkillBeginPrepare_技能开始进行准备,
					_ABC_ChangeSkillMotionToCharged_OnStrikePrepare);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Skill_OnSkillDefaultFinish_技能默认结束了,
					_ABC_ChangeSkillMotionOnSkillEnd);
		}

		protected override void _IC_ReceiveSkillInput_Start(InputAction.CallbackContext context)
		{
			base._IC_ReceiveSkillInput_Start(context);
		}
		
		

		private List<EnemyARPGCharacterBehaviour> _tmpSearchList = new List<EnemyARPGCharacterBehaviour>();
		public override bool _Internal_TryPrepareSkill()
		{
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}
			else
			{
				ApplyStrongStoic();
				//移动到最近标记位置

				var get = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
					.GetEnemyListInRange(_characterBehaviourRef.transform.position, _searchRange)
					.ClipEnemyListOnDefaultType().ClipEnemyListOnCharacterType(_notSearchEnemyTypes)
					.ClipEnemyListOnInvincibleBuff();

				for (int i = get.Count - 1; i >= 0; i--)
				{
					if (get[i].ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
						    .FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough) == BuffAvailableType.NotExist)
					{
						get.RemoveAt(i);
					}
				}

				if (get.Count == 0)
				{
					return true;
				}
                
				var originalDirection = RecordedAttackDirection.Value;

				for (int i = get.Count - 1; i >= 1; i--)
				{
					var tmpDir = Vector3.Angle(get[i].transform.position - _characterBehaviourRef.transform.position,
                    					originalDirection);
					var tmpDir2 = Vector3.Angle(get[i-1].transform.position - _characterBehaviourRef.transform.position,
						originalDirection);
					if (tmpDir > tmpDir2)
					{
						var tmp = get[i];
						get[i] = get[i-1];
						get[i - 1] = tmp;
					}
                    
				}
				RecordedAttackDirection = get[0].transform.position;
				
			}
			return true;
		}


		protected override void GetAndOffsetCurrentInputPositionAndDirection()
		{
			base.GetAndOffsetCurrentInputPositionAndDirection();
		}

		protected override void _Internal_TryInputToPrepareTick()
		{
			if (_InputHolding)
			{
				switch (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType)
				{
					case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
						case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
						//未指定，那就是不在自己这。有可能是待使用，也有可能是被断出去了。

						//进行更详细的判断，是需要开始准备了，还是在干别的

						if (!IfReactToInput())
						{
							return;
						}
						if (!_Internal_TryPrepareSkill())
						{
							return;
						}

						break;
					case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
						//处理输入确定
						_InternalProgress_ProgressToMiddle();

						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中:

						if (!IfReactToInput_OffhandState())
						{
							return;
						}
						_SkillProgress_Offhand_PrepareSkill();
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
		private void _ABC_ChangeSkillMotionToCharged_OnStrikePrepare(DS_ActionBusArguGroup ds)
		{
			if (!ds.TryGetObj1As(out BaseRPSkill skill))
			{
				return;
			}

			if (skill.SkillType != RPSkill_SkillTypeEnum.Swordman_QuickStrike_A1_剑士迅捷斩击)
			{
				return;
			}

			SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName = _ancn_middle_charged;
			
			
			
			

		}

		protected override void _InternalProgress_ProgressToMiddle()
		{
			base._InternalProgress_ProgressToMiddle();

			var weapon =
				(_characterBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction as WeaponHandler_亚瑟实验);
			weapon.SpecificAttackIndex = 3;
		}



		private void _ABC_ChangeSkillMotionOnSkillEnd(DS_ActionBusArguGroup ds)
		{
			if (!ds.TryGetObj1As(out BaseRPSkill skill))
			{
				return;
			}

			if (skill.SkillType != RPSkill_SkillTypeEnum.Swordman_QuickStrike_A1_剑士迅捷斩击)
			{
				return;
			}
		}

		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();			
			SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName = _ancn_middle_normal;

		}

		
		
		
		
		
		/// <summary>
		/// 当投射物命中时，检查是否需要刷新（由于命中破绽）
		/// </summary>
		/// <param name="ds"></param>
		protected void _ABC__CheckIfRefreshCD(DS_ActionBusArguGroup ds)
		{
			//首先需要caster是自己
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (!ReferenceEquals(dar.Caster, _characterBehaviourRef))
			{
				return;
			}
			//然后需要是自己打的Layout
			if (dar.RelatedProjectileRuntimeRef == null)
			{
				return;
			}
			if (!SkillPowerAffectLayoutUID.Contains(dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference
				.LayoutContentInSO.LayoutUID))
			{
				return;
			}
			//然后接收方需要有看破标签
			if (dar.Receiver.ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
				    .FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough) !=
			    BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				return;
			}
			
			//如果有，那就刷新CD
			ModifyRemainingCD(false, 0f);

		}
		
		
		


		protected override void _InternalProgress_Post_CompleteOn_Post()
		{
			base._InternalProgress_Post_CompleteOn_Post();
			RemoveStrongStoic();
		}

		protected override void PAEC_SpawnVFXFromConfig(PAEC_生成特效配置_SpawnVFXFromConfig paec)
		{
			base.PAEC_SpawnVFXFromConfig(paec);

		}


		public override FixedOccupiedCancelTypeEnum OnOccupiedCanceledByOther(
			DS_OccupationInfo occupySourceInfo,
			FixedOccupiedCancelTypeEnum explicitType = FixedOccupiedCancelTypeEnum.None_未指定,
			bool invokeBreakResult = true)
		{
			explicitType = base.OnOccupiedCanceledByOther(occupySourceInfo, explicitType, false);

			if (explicitType != FixedOccupiedCancelTypeEnum.ContinueBreak_接续断 &&
			    !occupySourceInfo.OccupationInfoConfigName.Contains("普攻"))
			{
				(_characterBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction as WeaponHandler_亚瑟实验)
					.SpecificAttackIndex = null;
			}
			
			ProcessBreakResultByOccupiedCancelType(explicitType, occupySourceInfo);
			return explicitType;
		}


		public override void BR_CommonExitEffect()
		{
			base.BR_CommonExitEffect();
			SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName = _ancn_middle_normal;
			RemoveStrongStoic();
		}

		

		public override DS_ActionBusArguGroup ClearBeforeRemove()
		{
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
					_ABC__CheckIfRefreshCD);
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_Skill_OnSkillBeginPrepare_技能开始进行准备,
					_ABC_ChangeSkillMotionToCharged_OnStrikePrepare);
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_Skill_OnSkillDefaultFinish_技能默认结束了,
					_ABC_ChangeSkillMotionOnSkillEnd);
			return base.ClearBeforeRemove();
		}
		


		protected override void PAEC_SpawnLayout(PAEC_生成版面_SpawnLayout paec , bool autoStart =true)
		{
			_selfLayoutRef = paec.RelatedConfig.SpawnLayout_NoAutoStart(_characterBehaviourRef);
			_selfLayoutRef.LayoutHandlerFunction.PresetNeedUniformTimeStamp = true;
			_selfLayoutRef.LayoutHandlerFunction.UniformTimeStamp = Time.frameCount + GetHashCode();
			_selfLayoutRef.LayoutHandlerFunction.OverrideSpawnFromDirection = RecordedAttackDirection ??
			                                                                  _characterBehaviourRef
				                                                                  .GetCurrentPlayerFaceDirection();
			_selfLayoutRef.LayoutHandlerFunction.StartLayout();
			_layoutList.Add(_selfLayoutRef);
		}
		
		protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
		{
			if (oInfo.OccupationInfoConfigName.Equals(_ancn_middle_charged) || oInfo.OccupationInfoConfigName.Equals(_ancn_middle_normal))
			{
				return true;
			}
			return base._Internal_CheckIfBreakAnimationIsSelf(oInfo);
		}
		
		
		
		
		
		
		
	}
}