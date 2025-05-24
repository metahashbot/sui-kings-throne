using System;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Player;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile;
using RPGCore.Projectile.Layout;
using RPGCore.Projectile.Layout.LayoutComponent;
using RPGCore.Skill.Config;
using RPGCore.Skill.SkillSelector;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	[TypeInfoBox("元素使——元素闪烁")]
	[Serializable]
	public class Skill_Elementalist_ElementalBlink : BaseRPSkill
	{
		[SerializeField,LabelText("投射物ID：火"),TitleGroup("===配置==="),HorizontalGroup("===配置===/1")]
		private string _projectileType_Fire;
		
		[SerializeField,LabelText("投射物ID：水") ,TitleGroup("===配置==="),HorizontalGroup("===配置===/1")]
		private string _projectileType_Water;
		
		[SerializeField,LabelText("投射物ID：土"),TitleGroup("===配置==="),HorizontalGroup("===配置===/2")]
		 private string _projectileType_Earth;
		
		[SerializeField,LabelText("投射物ID：风"),TitleGroup("===配置==="),HorizontalGroup("===配置===/2")]
		private string _projectileType_Wind;


		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			parent.ReleaseSkill_GetActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_PLayout_LayoutWillDestroy_当投射物Layout将要销毁,
					_ABC_CheckIfEndSkillOnRelatedLayoutClear);
		}


		public override bool _Internal_TryPrepareSkill()
		{
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}
			else
			{
				QuickFaceToPointer();
				return true;
			}
		}
		protected override void _InternalProgress_Offhand_CompleteOnPost()
		{
			ReturnToIdleAfterSkill();
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
		}

		protected override bool _SkillProgress_Offhand_PrepareSkill()
		{
			if (!base._SkillProgress_Offhand_PrepareSkill())
			{
				return false;
			}
			else
			{
				QuickFaceToPointer();
				return true;
			}
		}
		protected override bool IfReactToInput_OffhandState()
		{
			if (_Internal_CheckIfContainsFlagBuff(RP_BuffInternalFunctionFlagTypeEnum.DisableCommonMovement_禁用普通移动 |
			                                      RP_BuffInternalFunctionFlagTypeEnum.BlockByStrongStoic_被强霸体屏蔽 | 
			                                      RP_BuffInternalFunctionFlagTypeEnum.BlockByWeakStoic_被弱霸体屏蔽 | RP_BuffInternalFunctionFlagTypeEnum.ResistByWeakStoic_被弱霸体抵抗))
			{
				return false;
			}

			return true;
		}


		protected override void _InternalSkillEffect_SkillTakeEffect_OnMultiOffhandPart()
		{
			base._InternalSkillEffect_SkillTakeEffect_OnMultiOffhandPart();		
			//瞬移！
			if (_layoutList.Count == 0 || _layoutList[0] == null || _layoutList[0].LayoutHandlerFunction == null)
			{
				DBug.LogError( "没有找到投射物Layout，无法瞬移，这是不合理的");
				return;
			}

			var pp = _layoutList[0].LayoutHandlerFunction.GetAllSeriesDict()[(0, 0)][0];
			var pos = pp.RelatedGORef.transform.position;
			var delta = pos - _characterBehaviourRef.transform.position ;
			_characterBehaviourRef.TryMovePosition_XYZ(delta);
			_layoutList[0].LayoutHandlerFunction.ClearLayout(true);

		}

		protected override void PAEC_SpawnLayout(PAEC_生成版面_SpawnLayout paec, bool autoStart = true)
		{
			var _selfLayoutRef = paec.RelatedConfig.SpawnLayout_NoAutoStart(_characterBehaviourRef);
			_selfLayoutRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Clear();
			switch (GetCurrentDamageType())
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					_selfLayoutRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add( _projectileType_Earth);
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					_selfLayoutRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add(_projectileType_Water);
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					_selfLayoutRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add(_projectileType_Fire);
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					_selfLayoutRef.LayoutContentInSO.RelatedProjectileTypeDisplayNameList.Add(_projectileType_Wind);
					break;
			}
			_selfLayoutRef.LayoutHandlerFunction.PresetNeedUniformTimeStamp = true;
			_selfLayoutRef.LayoutHandlerFunction.UniformTimeStamp = Time.frameCount + GetHashCode();
			_selfLayoutRef.LayoutHandlerFunction.OverrideSpawnFromDirection = RecordedAttackDirection ??
			                                                                  _characterBehaviourRef
				                                                                  .GetCurrentPlayerFaceDirection();
			_selfLayoutRef.LayoutHandlerFunction.StartLayout();
			_layoutList.Add(_selfLayoutRef);
		}


		/// <summary>
		/// <para>当自己的那个版面销毁的时候，技能会直接强制结束，哪怕没有试图位移</para>
		/// </summary>
		/// <param name="ds"></param>
		protected void _ABC_CheckIfEndSkillOnRelatedLayoutClear(DS_ActionBusArguGroup ds)
		{
			var layoutHandlerRef = ds.GetObj1AsT<BaseLayoutHandler>();
			//就是自己的！
			if (!SkillPowerAffectLayoutUID.Contains(layoutHandlerRef.RelatedProjectileLayoutConfigRef.LayoutContentInSO
				.LayoutUID))
			{
				return;
			}

			if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中 ||
			    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇 ||
			    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇 ||
			    SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段)
			{
				BR_CommonExitEffect();
				BR_ResetAllPAMContent();
				SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum
					.NoneSelf_不是自己;
			}


			var findIndex = _layoutList.FindIndex((layout =>
				ReferenceEquals(layout, layoutHandlerRef.RelatedProjectileLayoutConfigRef)));
			_layoutList.RemoveAt(findIndex);

		}




		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
		}


		public override DS_ActionBusArguGroup ClearBeforeRemove()
		{
			_characterBehaviourRef.ReleaseSkill_GetActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_PLayout_LayoutWillDestroy_当投射物Layout将要销毁,
					_ABC_CheckIfEndSkillOnRelatedLayoutClear);
			return base.ClearBeforeRemove();
		}


	}
}