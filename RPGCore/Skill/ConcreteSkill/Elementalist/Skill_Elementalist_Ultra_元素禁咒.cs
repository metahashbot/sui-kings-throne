using System;
using ARPG.Character.Player;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	[Serializable]
	public class Skill_Elementalist_Ultra_元素禁咒 : BaseRPSkill
	{

		[TitleGroup("===配置===")]
		[SerializeField, LabelText("PAM-火元素"), TitleGroup("===配置===/火")]
		public PlayerSkillAnimationMotion _PAM_Fire;

		[SerializeField, LabelText("火-选点范围"), TitleGroup("===配置===/火")]
		public float _fire_IndicatorRange;

		[SerializeField, LabelText("火-判定范围"), TitleGroup("===配置===/火")]
		public float _fire_ProjectileRange;


		[SerializeField, LabelText("PAM-水元素"), TitleGroup("===配置===/水")]
		public PlayerSkillAnimationMotion _PAM_Water;

		[SerializeField, LabelText("水-选点范围 "), TitleGroup("===配置===/水")]
		public float _water_IndicatorRange;

		[SerializeField, LabelText("水-判定范围"), TitleGroup("===配置===/水")]
		public float _water_ProjectileRange;

		[SerializeField, LabelText("水-持续时长 "), TitleGroup("===配置===/水")]
		public float water_Duration;

		[SerializeField, LabelText("PAM-土元素"), TitleGroup("===配置===/土")]
		public PlayerSkillAnimationMotion _PAM_Earth;

		[SerializeField, LabelText("土-作用范围"), TitleGroup("===配置===/土")]
		public float _earth_AffectRange;

		[SerializeField, LabelText("土-击退力度"), TitleGroup("===配置===/土")]
		public float _earth_KnockBackForce;


		[SerializeField, LabelText("PAM-风元素"), TitleGroup("===配置===/风")]
		public PlayerSkillAnimationMotion _PAM_Wind;

		[SerializeField, LabelText("风-选点范围"), TitleGroup("===配置===/风")]
		public float _wind_IndicatorRange;

		[SerializeField, LabelText("风-判定范围"), TitleGroup("===配置===/风")]
		public float _wind_ProjectileRange;

		[SerializeField, LabelText("风-牵引力度"), TitleGroup("===配置===/风")]
		public float _wind_PullForce;
		[SerializeField, LabelText("风-持续时长"), TitleGroup("===配置===/风")]
		public float wind_Duration;

		[ShowInInspector, ReadOnly, LabelText("当前伤害类型"), FoldoutGroup("运行时")]
		private DamageTypeEnum _currentDamageType;
		


		private float _willClearVFXTime;
		protected override void BindingInput()
		{
			base.BindingInput();
			var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
			iar.BattleGeneral.FireBase.performed += _IC_FireAsConfirm;
		}

		protected void _IC_FireAsConfirm(InputAction.CallbackContext context)
		{
			if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中)
			{
				_InternalProgress_ProgressToMiddle();
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
				ApplyStrongStoic();
				return true;
			}
		}
		
		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnCommonDamageTypeChanged_当常规伤害类型发生变动,
					_ABC_ChangePAMContent_OnDamageTypeChanged);
			_currentDamageType = GetCurrentDamageType();
			switch (_currentDamageType)
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					SelfPlayerSkillAnimationMotion = _PAM_Earth;
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					SelfPlayerSkillAnimationMotion = _PAM_Water;
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					SelfPlayerSkillAnimationMotion = _PAM_Fire;
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					SelfPlayerSkillAnimationMotion = _PAM_Wind;
					break;
			}
		}
		
		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (currentTime > _willClearVFXTime)
			{
				_willClearVFXTime = float.MaxValue;
				VFX_GeneralClear();
			}
			
		}

		protected override void PAEC_SpawnVFXFromConfig(PAEC_生成特效配置_SpawnVFXFromConfig paec)
		{
			switch (_currentDamageType)
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					base.PAEC_SpawnVFXFromConfig(paec);
					if (paec.RuntimeVFXInfoRef._VFX_InfoID.Contains("伴生"))
					{
						paec.RuntimeVFXInfoRef.VFX_4_SetLocalScale(_earth_AffectRange);
					}
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					base.PAEC_SpawnVFXFromConfig(paec);
					if (paec.RuntimeVFXInfoRef._VFX_InfoID.Contains("水大场地"))
					{
						paec.RuntimeVFXInfoRef
							._VFX_2_SetPositionToGlobalPosition(GetValidPositionInRange(_water_IndicatorRange))
							.VFX_4_SetLocalScale(_water_ProjectileRange);
					}
					_willClearVFXTime = BaseGameReferenceService.CurrentFixedTime + water_Duration;
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					base.PAEC_SpawnVFXFromConfig(paec);
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					base.PAEC_SpawnVFXFromConfig(paec);
					if (paec.RuntimeVFXInfoRef._VFX_InfoID.Contains("法阵"))
					{
						paec.RuntimeVFXInfoRef
							._VFX_2_SetPositionToGlobalPosition(GetValidPositionInRange(_wind_IndicatorRange))
							.VFX_4_SetLocalScale(_wind_ProjectileRange);
					}
					_willClearVFXTime = BaseGameReferenceService.CurrentFixedTime + wind_Duration;
					break;
			}
		}


		public override void BR_CommonExitEffect()
		{
			BR_ResetAllPAMContent();
			BR_DisableAllIndicator();
			PACConfigInfo_ExitSkill?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}

		public override void BreakResult_DeathBreak()
		{
			base.BreakResult_DeathBreak();
		}



		protected override void PAEC_SpawnLayout(PAEC_生成版面_SpawnLayout paec , bool autoStart = true)
		{
			var layoutRuntime = paec.RelatedConfig.SpawnLayout_NoAutoStart(_characterBehaviourRef, false);
			_layoutList.Add(layoutRuntime);
			if (paec.NeedUniformTimeStamp)
			{
				layoutRuntime.LayoutHandlerFunction.PresetNeedUniformTimeStamp = true;
				layoutRuntime.LayoutHandlerFunction.UniformTimeStamp = Time.frameCount + GetHashCode();
			}
			layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromDirection = RecordedAttackDirection ??
			                                                                 _characterBehaviourRef
				                                                                 .GetCurrentPlayerFaceDirection();
			switch (_currentDamageType)
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition =
						_characterBehaviourRef.transform.position;
					layoutRuntime.LayoutContentInSO.RelatedProjectileScale = _earth_AffectRange;
					if (layoutRuntime.LayoutContentInSO.DamageApplyInfo.BuffEffectArray != null)
					{
						foreach (var perBuff in layoutRuntime.LayoutContentInSO.DamageApplyInfo.BuffEffectArray)
						{
							if (perBuff.BuffType == RolePlay_BuffTypeEnum.UnbalanceMovement_失衡推拉)
							{
								foreach (var blp in perBuff.GetFullBLPList())
								{
									if (blp is Buff_失衡推拉_UnbalanceMovement.BLP_开始失衡推拉_StartUnbalanceMovementBLP
										blp_unbalance)
									{
										blp_unbalance.UnbalancePower = _earth_KnockBackForce;
									}
								}
							}
						}
					}

					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition =
						GetValidPositionInRange(_water_IndicatorRange);
					layoutRuntime.LayoutContentInSO.RelatedProjectileScale = _water_ProjectileRange;
					layoutRuntime.LayoutContentInSO.SeriesInterval = 1f;
					layoutRuntime.LayoutContentInSO.SeriesCount = Mathf.FloorToInt(water_Duration);
					
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition =
						GetValidPositionInRange(_fire_IndicatorRange);
					layoutRuntime.LayoutContentInSO.RelatedProjectileScale = _fire_ProjectileRange;
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition =
						GetValidPositionInRange(_wind_IndicatorRange);
					layoutRuntime.LayoutContentInSO.RelatedProjectileScale = _wind_ProjectileRange;
					layoutRuntime.LayoutContentInSO.SeriesInterval = 1f;
					layoutRuntime.LayoutContentInSO.SeriesCount = Mathf.FloorToInt(wind_Duration);
					if (layoutRuntime.LayoutContentInSO.DamageApplyInfo.BuffEffectArray != null)
					{
						foreach (var perBuff in layoutRuntime.LayoutContentInSO.DamageApplyInfo.BuffEffectArray)
						{
							if (perBuff.BuffType == RolePlay_BuffTypeEnum.DragMovement_牵引推拉)
							{
								foreach (var blp in perBuff.GetFullBLPList())
								{
									if (blp is Buff_牵引推拉_DragMovement.BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP
										blp_drag)
									{
										blp_drag.DragPower = _wind_PullForce;
									}
								}
							}
						}
					}
					break;
			}
			layoutRuntime.LayoutHandlerFunction.StartLayout();
		}



		protected override void PAEC_SpawnIndicator(PAEC_生成选点指示器_SpawnPositionIndicator indicator)
		{
			switch (_currentDamageType)
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					indicator.ActiveAndSetIndicator(_characterBehaviourRef.transform.position,
						_fire_IndicatorRange,
						_characterBehaviourRef.transform.position,
						_fire_IndicatorRange,
						_characterBehaviourRef.GetCurrentPlayerFaceDirection());
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					break;
			}
		}


		protected override void _InternalSkillEffect_SkillDefaultFinishEffect()
		{
			base._InternalSkillEffect_SkillDefaultFinishEffect();
			RemoveStrongStoic();
		}

		private void _ABC_ChangePAMContent_OnDamageTypeChanged(DS_ActionBusArguGroup ds)
		{
			DamageTypeEnum newDamageType = (DamageTypeEnum)ds.IntArgu2.Value;
			BR_ResetAllPAMContent();
			_currentDamageType = newDamageType;
			switch (newDamageType)
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					SelfPlayerSkillAnimationMotion = _PAM_Earth;
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					SelfPlayerSkillAnimationMotion = _PAM_Water;
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					SelfPlayerSkillAnimationMotion = _PAM_Fire;
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					SelfPlayerSkillAnimationMotion = _PAM_Wind;
					break;
			}
		}
		
		public override DS_ActionBusArguGroup ClearBeforeRemove()
		{
			var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
			iar.BattleGeneral.FireBase.performed -= _IC_FireAsConfirm;
			return base.ClearBeforeRemove();
		}


	}
}