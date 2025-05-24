using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Equipment;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Skill.RedFang
{
	[Serializable]
	public class Buff_ScarletDarkness : BaseRPBuff , I_RPLogicCanApplyStoic
	{

		[SerializeField, LabelText("伤害减免量百分比"), FoldoutGroup("配置/数值"), SuffixLabel("%")]
		public float DamageReducePercentage = 30f;

		[SerializeField, LabelText("吸血百分比"), FoldoutGroup("配置/数值"), SuffixLabel("%")]
		public float BloodSuckingPercentage = 30f;

		[SerializeField, LabelText("攻速提升百分比"), FoldoutGroup("配置/数值"), SuffixLabel("%")]
		public float AttackSpeedBonusPercentage = 30f;

		[SerializeField, LabelText("真伤追加百分比"), FoldoutGroup("配置/数值"), SuffixLabel("%")]
		public float TrueDamageBonusPercentage = 30f;
		
		
		[SerializeField, LabelText("周身特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_general;

		private PerVFXInfo _vfxInfo_General;
		

		protected PlayerARPGConcreteCharacterBehaviour _characterBehaviourRef;

		protected Float_RPDataEntry _entry_AttackSpeed;
		protected Float_ModifyEntry_RPDataEntry _modify_AttackSpeed;

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_characterBehaviourRef = Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour;
			(Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour).GetSelfRolePlayArtHelper()
				.ActivateGhostEffect();
			ApplyWeakStoic();
			_entry_AttackSpeed = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			
			_modify_AttackSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(AttackSpeedBonusPercentage / 100f,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontAdd,
				this);
			
			_entry_AttackSpeed.AddDataEntryModifier(_modify_AttackSpeed);

			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,_ABC_ProcessAppendEffect);
			
			
			//把武器里的所有"未充能"换成"已充能"
			var currentWeaponConfigRef = _characterBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance;
			var weaponHandlerAsMARef = currentWeaponConfigRef.WeaponFunction as WeaponHandler_MultiAttack;
			if (weaponHandlerAsMARef != null)
			{
				foreach (var perPAM in weaponHandlerAsMARef.SelfAllPresetAnimationMotionInfoList)
				{
					perPAM._ancn_PrepareAnimationName= perPAM._ancn_PrepareAnimationName.Replace("未充能", "已充能");
					perPAM._ancn_MiddlePartAnimationName = perPAM._ancn_MiddlePartAnimationName.Replace("未充能", "已充能");
					perPAM._ancn_PostAnimationName = perPAM._ancn_PostAnimationName.Replace("未充能", "已充能");
				}
			}
			
			//直接施加已满的buff，并移除叠层的那个
			Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.BloodGasValue_血气值);
			Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.RedFang_BloodOverflow_血气已满,
				Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff, 
				Parent_SelfBelongToObject);
			
			
			_vfxInfo_General?.VFX_StopThis(true);
			_vfxInfo_General = _VFX_GetAndSetBeforePlay(_vfx_general)?._VFX__10_PlayThis();
			
			
			
			return ds;
		}



		private void _ABC_ProcessAppendEffect(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerNormalAttack_玩家普攻伤害) ||
			    dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerSkillAttack_玩家技能伤害) ||
			    dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerUltraAttack_玩家超杀伤害))
			{
				//吸血
				var _heal_dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(_characterBehaviourRef,
					_characterBehaviourRef,
					DamageTypeEnum.Heal_治疗,
					dar.DamageResult_TakenOnHP *( BloodSuckingPercentage)/ 100f,
					DamageProcessStepOption.HealDPS());
				_damageAssistServiceRef.ApplyDamage(_heal_dai).ReleaseToPool();

				//追加真伤
				var _damage_apped = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(dar.Receiver,
					_characterBehaviourRef,
					DamageTypeEnum.TrueDamage_真伤,
					dar.DamageResult_TakenOnHP  *(TrueDamageBonusPercentage / 100f),
					DamageProcessStepOption.TrueDamageDPS());
				_damageAssistServiceRef.ApplyDamage(_damage_apped).ReleaseToPool();
				
			}
		}
		
		
		


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			(Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour).GetSelfRolePlayArtHelper()
				.DeactivateGhostEffect();
			RemoveWeakStoic();
			Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.RedFang_BloodOverflow_血气已满);
			Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.BloodGasValue_血气值,
				Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
				Parent_SelfBelongToObject);

			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
				_ABC_ProcessAppendEffect);
			var currentWeaponConfigRef = _characterBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance;
			var weaponHandlerAsMARef = currentWeaponConfigRef.WeaponFunction as WeaponHandler_MultiAttack;
			if (weaponHandlerAsMARef != null)
			{
				foreach (var perPAM in weaponHandlerAsMARef.SelfAllPresetAnimationMotionInfoList)
				{
					perPAM._ancn_PrepareAnimationName = perPAM._ancn_PrepareAnimationName.Replace("已充能", "未充能");
					perPAM._ancn_MiddlePartAnimationName = perPAM._ancn_MiddlePartAnimationName.Replace("已充能", "未充能");
					perPAM._ancn_PostAnimationName = perPAM._ancn_PostAnimationName.Replace("已充能", "未充能");
				}
			}

			return base.OnBuffPreRemove();
		}


		protected virtual void ApplyWeakStoic(float duration = -1f)
		{
			var blp_stoic = GenericPool<BLP_霸体施加信息_StoicApplyInfoBLP>.Get();
			blp_stoic.Applier = this;
			blp_stoic.RemainingDuration = duration;
			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体,
				_characterBehaviourRef,
				_characterBehaviourRef,
				blp_stoic);
			blp_stoic.ReleaseOnReturnToPool();
		}

		protected virtual void RemoveWeakStoic()
		{
			if (_characterBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体) ==
			    BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				var blp_StoicRemove = GenericPool<BLP_霸体移除信息_StoicRemoveInfoBLP>.Get();
				blp_StoicRemove.Applier = this;
				_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体,
					_characterBehaviourRef,
					_characterBehaviourRef,
					blp_StoicRemove);
				blp_StoicRemove.ReleaseOnReturnToPool();

			}
		}


	}
}