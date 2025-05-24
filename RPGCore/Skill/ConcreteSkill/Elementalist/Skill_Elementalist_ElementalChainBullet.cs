using System;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Skill.Elementalist;
using RPGCore.Buff.ConcreteBuff.Skill.Swordman;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	[Serializable]
	[TypeInfoBox("上一个Buff，不需要准备动画，直接释放")]
	public class Skill_Elementalist_ElementalChainBullet : BaseRPSkill
	{
	
	
	
		[SerializeField, LabelText("持续时长"), FoldoutGroup("配置", true),
		 TitleGroup("配置/Buff", Alignment = TitleAlignments.Centered)]
		public float ExistDuration = 7f;
	
		[SerializeField, LabelText("发射间隔"), FoldoutGroup("配置", true),
		 TitleGroup("配置/Buff", Alignment = TitleAlignments.Centered)]
		public float ShootInterval = 0.5f;
	
		[SerializeField, LabelText("环绕数量"), FoldoutGroup("配置", true),
		 TitleGroup("配置/Buff", Alignment = TitleAlignments.Centered)]
		public int SurroundCount = 3;
	
		[SerializeField, LabelText("环绕半径"), FoldoutGroup("配置", true),
		 TitleGroup("配置/环绕", Alignment = TitleAlignments.Centered)]
		public float BulletSurroundRadius;
	
	
		[SerializeField, LabelText("环绕角速度"), FoldoutGroup("配置", true),
		 TitleGroup("配置/环绕", Alignment = TitleAlignments.Centered)]
		public float BulletSurroundAngleSpeed = 360f;
		// [SerializeField, LabelText("环绕时高度变化曲线_本地修正"), FoldoutGroup("配置", true),
		//  TitleGroup("配置/环绕", Alignment = TitleAlignments.Centered)]
		// public AnimationCurve SurroundHeightCurve;
	
		[SerializeField, LabelText("子弹飞行速度"), FoldoutGroup("配置", true),
		 TitleGroup("配置/版面", Alignment = TitleAlignments.Centered)]
		public float BulletFlySpeed = 5f;
	
		[SerializeField, LabelText("子弹诱导最大角速度"), FoldoutGroup("配置", true),
		 TitleGroup("配置/版面", Alignment = TitleAlignments.Centered)]
		public float BulletTrackingAngleSpeed = 120f;
	
		[SerializeField, LabelText("子弹最大生命周期"), FoldoutGroup("配置", true),
		 TitleGroup("配置/版面", Alignment = TitleAlignments.Centered)]
		public float BulletLifetime = 5f;
	
		[SerializeField, LabelText("覆写的伤害信息"), FoldoutGroup("配置", true),
		 TitleGroup("配置/版面", Alignment = TitleAlignments.Centered)]
		public ConSer_DamageApplyInfo DamageApplyInfoOverride;
	
		private PerVFXInfo _vfxInfo_prepare;
	
	
		private AnimationInfoBase _sai_Release;
	
		private Buff_ElementalChainBullet targetBuffRef;
	
	
	
		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			// _sai_Release =
			// 	GetAnimationInfoByConfigName(configRuntimeInstance.ContentInSO._AN_SkillReleaseSpineAnimationName);
			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRemoved,
				_ABC_SetStateToNone_OnRelatedBuffRemove);
		}



		/// <summary>
		/// <para>对于原始准备来说：如果Buff有效，则应当为移除buff并进入CD。</para>
		/// </summary>
		/// <returns></returns>
		protected override bool IfReactToInput(
			bool checkCurrentActivePlayer = true,
			bool checkDataEntryEnough = true,
			bool checkReadyType = true,
			bool checkRunningState = true)
		{
			if (_characterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum
				.FromSkill_Elementalist_ElementalChainBullet_元素使奥术飞弹) == BuffAvailableType.NotExist)
			{
				return base.IfReactToInput(checkCurrentActivePlayer,
					checkDataEntryEnough,
					checkReadyType,
					checkRunningState);

			}
			else
			{
				if (checkCurrentActivePlayer)
				{
					if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
					{
						return false;
					}
				}
				if (checkRunningState)
				{
					switch (BaseGameReferenceService.GameRunningState)
					{
						case BaseGameReferenceService.GameRunningStateTypeEnum.None_未指定:
						case BaseGameReferenceService.GameRunningStateTypeEnum.Loading_加载中:
						case BaseGameReferenceService.GameRunningStateTypeEnum.Paused_暂停:
							return false;
					}
				}

				return true;
			}
		}


		/// <summary>
		/// <para>当元素球buff存在时，不会进入CD</para>
		/// </summary>
		/// <returns></returns>
		protected override bool IfSkillCanCDTick()
		{
			if (_characterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum
				.FromSkill_Elementalist_ElementalChainBullet_元素使奥术飞弹) != BuffAvailableType.NotExist)
			{
				return false;
			}
			return base.IfSkillCanCDTick();
		}


		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();

			var duration = GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Get();
			var chainBulletConfig = GenericPool<Buff_ElementalChainBullet.BLP_奥术飞弹初始化参数_ChainBulletInitBLP>.Get();
			duration.SetAllAsNotLess(ExistDuration);
            chainBulletConfig.BulletLifetime = BulletLifetime;
			chainBulletConfig.BulletFlySpeed = BulletFlySpeed;
			chainBulletConfig.BulletSurroundAngleSpeed = BulletSurroundAngleSpeed;
			chainBulletConfig.BulletSurroundRadius = BulletSurroundRadius;
			chainBulletConfig.SurroundCount = SurroundCount;
			chainBulletConfig.ShootInterval = ShootInterval;
			chainBulletConfig.BulletTrackingAngleSpeed = BulletTrackingAngleSpeed;


			var applyResult = _characterBehaviourRef.ReceiveBuff_TryApplyBuff(
				RolePlay_BuffTypeEnum.FromSkill_Elementalist_ElementalChainBullet_元素使奥术飞弹,
				_characterBehaviourRef,
				_characterBehaviourRef,
				duration,
				chainBulletConfig);
			 duration.ReleaseOnReturnToPool();
			 chainBulletConfig.ReleaseOnReturnToPool();
		}

		protected override void SkillCDTick(float delta)
		{
			base.SkillCDTick(delta);
		}




		private void _ABC_SetStateToNone_OnRelatedBuffRemove(DS_ActionBusArguGroup ds)
		{
			var targetBuff = ds.ObjectArgu1 as BaseRPBuff;
			if (targetBuff is Buff_ElementalChainBullet)
			{
				
			}
			targetBuffRef = null;
		}

		public override void BR_CommonExitEffect()
		{
			base.BR_CommonExitEffect();
			if (targetBuffRef != null)
			{
				targetBuffRef.ResetAvailableTimeAs(0.02f);
				targetBuffRef.ResetExistDurationAs(0.02f);
			}
			VFX_GeneralClear(true);
			_vfxInfo_prepare?.VFX_StopThis(true);
		}



		public override void BreakResult_DeathBreak()
		{
			base.BreakResult_DeathBreak();
			if (targetBuffRef != null)
			{
				targetBuffRef.ResetAvailableTimeAs(0.02f);
				targetBuffRef.ResetExistDurationAs(0.02f);
			}
			VFX_GeneralClear(true);
			_vfxInfo_prepare?.VFX_StopThis(true);
			
		}
		public override void BreakResult_SwitchCharacter()
		{
			base.BreakResult_SwitchCharacter();

			if (targetBuffRef != null)
			{
				targetBuffRef.ResetAvailableTimeAs(0.02f);
				targetBuffRef.ResetExistDurationAs(0.02f);
			}
			VFX_GeneralClear(true);
			_vfxInfo_prepare?.VFX_StopThis(true);
		}

		
		public override Sprite GetCurrentSprite(DamageTypeEnum @override = DamageTypeEnum.None)
		{
			DamageTypeEnum fd = DamageTypeEnum.None;
	
			if (@override != DamageTypeEnum.None)
			{
				fd = @override;
			}
			else
			{
				fd = (RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType).CurrentDamageType;
			}
			switch (fd)
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					return SpritePairs.Find((pair => pair.Desc.Contains("土"))).SpriteAsset;
				case DamageTypeEnum.AoNengShui_奥能水:
					return SpritePairs.Find((pair => pair.Desc.Contains("水"))).SpriteAsset;
				case DamageTypeEnum.AoNengHuo_奥能火:
					return SpritePairs.Find((pair => pair.Desc.Contains("火"))).SpriteAsset;
				case DamageTypeEnum.AoNengFeng_奥能风:
					return SpritePairs.Find((pair => pair.Desc.Contains("风"))).SpriteAsset;
			}
			return null;
		}
	}
}