using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Enemy;
using ARPG.Character.Player;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Skill.RedFang;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
namespace RPGCore.Skill.ConcreteSkill.RedFang
{
	[TypeInfoBox("猩红血池\n" + "伤害间隔是技能内部处理的，所以版面应当是【一次性】的")]
	[Serializable]
	public class Skill_RedFang_Sacrifice : BaseRPSkill
	{



		/// <summary>
		/// <para>当前充能？</para>
		/// </summary>
		public bool ActAsCharged { get; private set; }
		
		public float ThisTimeAccumulatedDamage { get; private set; }


		[SerializeField, LabelText("PAM记录_前摇_普通的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_NormalPrepareAnimation;
		[SerializeField, LabelText("PAM记录_前摇_充能的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_ChargedPrepareAnimation;



		[SerializeField, LabelText("PAM记录_作用_普通的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_NormalReleaseAnimation;
		[SerializeField, LabelText("PAM记录_作用_充能的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_ChargedReleaseAnimation;


		[SerializeField, LabelText("PAM记录_后摇_普通的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_NormalPostAnimation;
		[SerializeField, LabelText("PAM记录_后摇_充能的"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _pam_ani_ChargedPostAnimation;

		
		
		[SerializeField,LabelText("作用中时，每隔多长时间生成一次版面") , TitleGroup("===配置===")]
		private float _layoutGenerateInterval = 0.5f;
		
		[SerializeField,LabelText("未充能版面"), TitleGroup("===配置===")]
		public SOConfig_ProjectileLayout _layout_Normal;
		
		[SerializeField,LabelText("充能版面"), TitleGroup("===配置===")]
		public SOConfig_ProjectileLayout _layout_Charged;

		private float _nextGenerateTime;
		
		 
		
		[SerializeField,LabelText("作用中时，移动速度乘数") , TitleGroup("===配置===")]
		private float _speedMul = 1.25f;



		[SerializeField, LabelText("普通：每造成这么多伤害回1血"), TitleGroup("===配置==="), SuffixLabel("%")]
		protected float _DamageAmountPerHeal = 50f;
		[SerializeField, LabelText("普通：每造成这么多伤害加1%攻"), TitleGroup("===配置==="), SuffixLabel("%")]
		protected float _DamageAmountPerBonus = 10f;
		[SerializeField, LabelText("充能：结束后回血的伤害量比率"), TitleGroup("===配置==="), SuffixLabel("%")]
		protected float _charged_DamageAmountPerHeal = 75f;
		[SerializeField, LabelText("充能：超出后转换攻击力比例"), TitleGroup("===配置==="), SuffixLabel("%")]
		protected float _charged_DamageAmountPerBonus = 20f;
		
		[SerializeField, LabelText("转换攻击力持续时间"), TitleGroup("===配置===")]
		protected float _damageConvert_AffectDuration = 10f;

		[SerializeField, LabelText("普通：产生影响半径"), TitleGroup("===配置===")]
		protected float _damageRadius;

		[SerializeField, LabelText("充能：产生影响半径"), TitleGroup("===配置===")]
		protected float _charged_damageRaidus;
		/// <summary>
		/// <para>已记录为将要恢复为不充能。这将在Post(技能默认结束)那里确定恢复</para>
		/// </summary>
		private bool _registerRestoreNonCharged;

		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
					_ABC_ChangePAMToCharged);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved, _ABC_ChangePAMToNormal);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
					_ABC_CheckIfAccumulateDamageTakenThisTime_OnTakenToHP);
		}





		protected override void _IC_ReceiveSkillInput_Cancel(InputAction.CallbackContext context)
		{
			base._IC_ReceiveSkillInput_Cancel(context);
			if (SelfPlayerSkillAnimationMotion._MiddleEffecting_CanCancel &&
			    SelfPlayerSkillAnimationMotion._Middle_EffectingTime >
			    SelfPlayerSkillAnimationMotion._MiddleEffecting_MinDuration &&
			    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中)
			{
				_InternalProgress_ProgressToPost();
			}
		}


		public override bool _Internal_TryPrepareSkill()
		{
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}
			else
			{
				ApplyDirectorInvincible();
				ThisTimeAccumulatedDamage = 0f;

				
				
				
				
				return true;
			}
		}

		protected override void PAEC_SpawnVFXFromConfig(PAEC_生成特效配置_SpawnVFXFromConfig paec)
		{
			base.PAEC_SpawnVFXFromConfig(paec);
			if (paec.RuntimeVFXInfoRef._VFX_InfoID.Contains("持续"))
			{
				paec.RuntimeVFXInfoRef.VFX_4_SetLocalScale(ActAsCharged ? _charged_damageRaidus : _damageRadius);
			}
			
		}

		/// <summary>
		/// <para>当猩月黯影buff添加时，将PAM更换为充能的</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ChangePAMToCharged(DS_ActionBusArguGroup ds)
		{
			if ((RolePlay_BuffTypeEnum)ds.IntArgu1.Value !=
			    RolePlay_BuffTypeEnum.RedFang_BloodOverflow_血气已满 )
			{
				return;
			}

			ActAsCharged = true;
			SelfPlayerSkillAnimationMotion._ancn_PrepareAnimationName = _pam_ani_ChargedPrepareAnimation;
			SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName = _pam_ani_ChargedReleaseAnimation;
			SelfPlayerSkillAnimationMotion._ancn_PostPartAnimationName = _pam_ani_ChargedPostAnimation;
		}


		/// <summary>
		/// <para>当猩月黯影buff移除时，将PAM更换为普攻的</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ChangePAMToNormal(DS_ActionBusArguGroup ds)
		{
			
			if ((RolePlay_BuffTypeEnum)ds.IntArgu1.Value !=
			    RolePlay_BuffTypeEnum.RedFang_BloodOverflow_血气已满)
			{
				return;
			}
			_registerRestoreNonCharged = true;

		}



		
		



		protected override void _InternalProgress_Post_CompleteOn_Post()
		{
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
			ReturnToIdleAfterSkill();
			RemoveDirectorInvincible();
		}

		
		
		
		

		/// <summary>
		/// <para>当在中段作用时再次输入，等效取消技能</para>
		/// </summary>
		/// <param name="context"></param>
		protected override void _IC_ReceiveSkillInput_Start(InputAction.CallbackContext context)
		{
			base._IC_ReceiveSkillInput_Start(context);
			if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中)
			{
				_InternalProgress_ProgressToPost();
			}
		}



		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			//依然可以正常移动
			if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中 ||
			    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.Prepare_前摇 ||
			    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.Post_后摇)
			{
				Vector3 currentPosition = _characterBehaviourRef.transform.position;
				float currentMoveSpeed =
					_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速).CurrentValue *
					_speedMul;
				if (currentMoveSpeed < 0f)
				{
					currentMoveSpeed = 0f;
				}
				Vector3 logicalForward = BaseGameReferenceService.CurrentBattleLogicalForwardDirection;
				Vector3 logicRight = BaseGameReferenceService.CurrentBattleLogicRightDirection;
				//这里需要更改移动的真实方向，X轴移动的是当前的逻辑
				var movementOnX = _playerControllerRef.InputDirect_InputMoveRaw.normalized.x * currentMoveSpeed * delta * logicRight;
				// currentRBPosition.x += moveDirectionNormalized.x * currentMoveSpeed * delta;
				var movementOnZ = _playerControllerRef.InputDirect_InputMoveRaw.normalized.y * currentMoveSpeed * delta * logicalForward;

				var movement = movementOnX + movementOnZ;
				currentPosition += movement;

				// Vector3 alignedPosition = _glmRef.GetAlignedTerrainPosition(currentPosition);
				_characterBehaviourRef.TryMovePosition_XYZ(movement, true);
				
			}
			if((SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中 ||
			    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.Prepare_前摇))
			{
				if (currentTime > _nextGenerateTime)
				{
					_nextGenerateTime = currentTime + _layoutGenerateInterval;
					SOConfig_ProjectileLayout lr;
					if (ActAsCharged)
					{
						lr = _layout_Charged;
					}
					else
					{
						lr = _layout_Normal;
					}
					var t  =lr.SpawnLayout_NoAutoStart(_characterBehaviourRef);
					t.LayoutContentInSO.RelatedProjectileScale = ActAsCharged ? _charged_damageRaidus : _damageRadius;
					t.LayoutHandlerFunction.OverrideSpawnFromPosition = _characterBehaviourRef.transform.position;
					t.LayoutHandlerFunction.StartLayout();

				}
			}
		}

		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();
		}

		
		
		/// <summary>
		/// <para>累加伤害，用于技能结束后回血</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_CheckIfAccumulateDamageTakenThisTime_OnTakenToHP(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar.RelatedProjectileRuntimeRef == null)
			{
				return;
			}
			if (!SkillPowerAffectLayoutUID.Contains(dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference
				.LayoutContentInSO.LayoutUID))
			{
				return;
			}
			ThisTimeAccumulatedDamage += dar.DamageResult_TakenOnHP;

		}

		/// <summary>
		/// 当技能“结束”的时候，结算吸血伤害，增加攻击力
		/// </summary>
		protected override void _InternalSkillEffect_SkillDefaultFinishEffect()
		{
			base._InternalSkillEffect_SkillDefaultFinishEffect();
			
			
			var amount = (ThisTimeAccumulatedDamage / ( (ActAsCharged ? _charged_DamageAmountPerHeal :_DamageAmountPerHeal)));

			var rpds_dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(_characterBehaviourRef,
				_characterBehaviourRef,
				DamageTypeEnum.Heal_治疗,
				amount,
				DamageProcessStepOption.HealDPS());
			SubGameplayLogicManager_ARPG.Instance.DamageAssistServiceInstance.ApplyDamage(rpds_dai).ReleaseToPool();
			

			var blp_attack = GenericPool<BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify>.Get();
			blp_attack.ModifyValue = ThisTimeAccumulatedDamage / ((ActAsCharged ? _charged_DamageAmountPerBonus
				: _DamageAmountPerBonus));
			blp_attack.CalculatePosition = ModifyEntry_CalculatePosition.FrontMul;
			blp_attack.ModifyDuration = _damageConvert_AffectDuration;
			blp_attack.TargetEntry = RP_DataEntry_EnumType.AttackPower_攻击力;
			blp_attack.TargetUID = "血池加攻";
			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(
				RolePlay_BuffTypeEnum.CommonDataEntryModifyStackable_通用数据项修正可叠层,
				_characterBehaviourRef,
				_characterBehaviourRef,
				blp_attack);
			blp_attack.ReleaseOnReturnToPool();
			
			
			ThisTimeAccumulatedDamage = 0f;


			ActAsCharged = false;
			SelfPlayerSkillAnimationMotion._ancn_PrepareAnimationName = _pam_ani_NormalPrepareAnimation;
			SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName = _pam_ani_NormalReleaseAnimation;
			SelfPlayerSkillAnimationMotion._ancn_PostPartAnimationName = _pam_ani_NormalPostAnimation;
			
		}

		public override void BR_CommonExitEffect()
		{
			base.BR_CommonExitEffect();
			ThisTimeAccumulatedDamage = 0f;

		}
		
		

		public override DS_ActionBusArguGroup ClearBeforeRemove()
		{
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
					_ABC_ChangePAMToCharged);
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved, _ABC_ChangePAMToNormal);
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
					_ABC_CheckIfAccumulateDamageTakenThisTime_OnTakenToHP);
			return base.ClearBeforeRemove();
		}





		public override Sprite GetCurrentSprite(DamageTypeEnum @override = DamageTypeEnum.None)
		{
			if (ActAsCharged)
			{
				return SpritePairs.Find((pair => pair.Desc.Contains("已充能"))).SpriteAsset;
			}
			else
			{
				return SpritePairs.Find((pair => pair.Desc.Contains("未充能"))).SpriteAsset;
			}
		}

	}
}