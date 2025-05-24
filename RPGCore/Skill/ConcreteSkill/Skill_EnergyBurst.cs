// using System;
// using ARPG.Character;
// using ARPG.Character.Base;
// using ARPG.Character.Base.CustomSpineData;
// using ARPG.Manager;
// using GameplayEvent;
// using GameplayEvent.SO;
// using Global.ActionBus;
// using RPGCore.Buff;
// using RPGCore.Buff.BuffHolder;
// using RPGCore.Buff.ConcreteBuff;
// using RPGCore.Buff.ConcreteBuff.Skill.Swordman;
// using RPGCore.DataEntry;
// using RPGCore.Interface;
// using RPGCore.Projectile.Layout;
// using RPGCore.Skill.Config;
// using RPGCore.UtilityDataStructure;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.InputSystem;
// namespace RPGCore.Skill.ConcreteSkill
// {
// 	[TypeInfoBox("技能：大技能，能量狂涌")]
// 	[Serializable]
// 	public class Skill_EnergyBurst : BaseRPSkill , I_SkillContainStoicToStiffness , I_SkillAsUltraSkill
// 	{
//
// 		[SerializeField, LabelText("技能持续时长"), FoldoutGroup("配置", true)]
// 		public float SkillDuration = 10f;
//
// 		[LabelText("攻击力提高的比率，25表示提升25%")]
// 		[ShowInInspector, FoldoutGroup("配置", true)]
// 		public float AttackPowerBonus = 25;
//
// 		[LabelText("攻击力提高的算区，默认前乘")]
// 		[ShowInInspector, FoldoutGroup("配置", true)]
// 		public ModifyEntry_CalculatePosition AttackPowerBonusCalculatePosition = ModifyEntry_CalculatePosition.FrontMul;
//
//
//
// 		[LabelText("技能CD减少量")]
// 		[ShowInInspector, FoldoutGroup("配置", true), SuffixLabel("%")]
// 		public float SkillCDReducePartial = 80f;
// 		
// 		[SerializeField,LabelText("VFX_准备阶段"),FoldoutGroup("配置",true), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _vfxConfigName_Prepare;
// 		[SerializeField,LabelText("VFX_释放阶段"),FoldoutGroup("配置",true), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _vfxConfigName_Release;
// 		
// 		[SerializeField,LabelText("技能操控的GEM镜头动作"),FoldoutGroup("配置",true), GUIColor(187f / 255f, 1f, 0f)]
// 		protected SOConfig_PrefabEventConfig _GEMActionName;
//
//
// 		[SerializeField, LabelText("爆炸时使用的版面——伤害覆写"), FoldoutGroup("配置", true)]
// 		protected ConSer_DamageApplyInfo DamageApplyOverride;
//
//
// 		[SerializeField, LabelText("爆炸时使用的版面——NWay覆写"), FoldoutGroup("配置", true)]
// 		public LC_NWay NWayOverride;
//
//
//
// 		private AnimationInfoBase _sai_release;
// 		
// 		[SerializeField,LabelText("动画事件_释放判定"),FoldoutGroup("配置",true)]
// 		protected string _animEventName_ReleaseTakeEffect;
//
//
// 		protected Float_RPDataEntry _entry_UP;
// 		
// 		private GameplayEventManager _gameplayEventManagerRef;
//
// 		private enum SkillStateTypeEnum
// 		{
// 			None_无事发生 = 0,
// 			Releasing_释放中 = 1,
// 			Active_生效中 =  2,
// 		}
//
// 		private SkillStateTypeEnum _skillState = SkillStateTypeEnum.None_无事发生;
//
// 		protected override bool IfSkillCanCDTick()
// 		{
// 			if (_skillState == SkillStateTypeEnum.None_无事发生)
// 			{
// 				return true;
// 			}
// 			else
// 			{
// 				return false;
// 			}
// 		}
//
// 		public override void InitOnObtain(RPSkill_SkillHolder skillHolderRef, SOConfig_RPSkill configRuntimeInstance,
// 			I_RP_ObjectCanReleaseSkill parent, SkillSlotTypeEnum slot)
// 		{
// 			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
//
// 			_playerControllerRef = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
// 			_characterBehaviourRef = parent as PlayerARPGConcreteCharacterBehaviour;
// 			_characterArtHelperRef = _characterBehaviourRef.GetSelfRolePlayArtHelper() as PlayerARPGArtHelper;
// 			// _sai_release = GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 			// 	._AN_SkillReleaseSpineAnimationName);
// 			_sai_release.OccupationInfo.RelatedInterface = this;
// 			// SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.LayoutContentInSO.DamageApplyInfo =
// 			// 	DamageApplyOverride;
// 			// var nwayIndex = SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.LayoutContentInSO.LayoutComponentList
// 			// 	.FindIndex((component => component is LC_NWay));
// 			// if (nwayIndex == -1)
// 			// {
// 			// 	SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.LayoutContentInSO.LayoutComponentList
// 			// 		.Add(NWayOverride);
// 			// }
// 			// else
// 			// {
// 			// 	SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.LayoutContentInSO.LayoutComponentList
// 			// 		[nwayIndex] = NWayOverride;
// 			// }
//
// 			_entry_UP = _characterBehaviourRef.ReleaseSkill_GetPresentDataEntry(RP_DataEntry_EnumType
// 				.CurrentUP_当前UP);
// 			
// 			_gameplayEventManagerRef = GameplayEventManager.Instance;
// 		}
//
// 		public override SkillReadyTypeEnum GetSkillReadyType()
// 		{
// 			if (_skillState != SkillStateTypeEnum.None_无事发生)
// 			{
// 				return SkillReadyTypeEnum.Using_还在使用;
// 			}
// 			
// 			if (_entry_UP.CurrentValue < 100f)
// 			{
// 				return SkillReadyTypeEnum.BlockByCD;
// 			}
// 			else
// 			{
// 				return base.GetSkillReadyType();
// 			}
// 			
// 		}
//
// 		protected override void BindingInput()
// 		{
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
//
// 			InputAction ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed += _IC_ReceiveSkillInput_NormalPerformed;
// 		}
//
//
// 		protected override void UnbindInput()
// 		{
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
//
// 			InputAction ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed -= _IC_ReceiveSkillInput_NormalPerformed;
// 		}
//
//
//
// 		protected virtual void _IC_ReceiveSkillInput_NormalPerformed(InputAction.CallbackContext context)
// 		{
// 			//不是自己，无事发生
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
// 			
//
// 			/*
// 			 * 如果当前技能就是活跃的，则表明快速施法
// 			 */
// 			OnSkillSlotReceiveInput();
// 			switch (_skillState)
// 			{
// 				case SkillStateTypeEnum.None_无事发生:
// 					if (!IfReactToInput())
// 					{
// 						return; 
// 					}
// 					//如果对应buff还没消掉，则也不会释放
// 					if (_characterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum
// 						.FromSkill_EnergyBurst_能量狂涌) == BuffAvailableType.Available_TimeInAndMeetRequirement)
// 					{
// 						return;
// 					}
//
//
//
// 					if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_release.OccupationInfo))
// 					{
// 						return;
// 					}
//
// 					//到这里了，开始准备
// 					OnSkillBeginPrepare();
//
// 					break;
// 				case SkillStateTypeEnum.Releasing_释放中:
// 					break;
// 				case SkillStateTypeEnum.Active_生效中:
// 					break;
// 			}
//
//
// 		}
//
//
// 		protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
// 		{
// 			var ds = base.OnSkillBeginPrepare(autoLaunch);
// 			(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
// 			_skillState = SkillStateTypeEnum.Releasing_释放中;
// 			OnSkillResetCoolDown();
// 			OnSkillConsumeSP();
//
// 			var vfxPrepare = _VFX_GetOrInstantiateNew(_vfxConfigName_Prepare, true, GetCurrentDamageType)._VFX_2_SetPositionToGlobalPosition(this)
// 				._VFX__10_PlayThis();
// 			// vfxPrepare.CurrentActiveRuntimePSPlayProxyRef.transform.localPosition += Vector3.down * 0.35f;
// 			_entry_UP.ResetDataToValue(0f);
// 			
// 			//发出要求动画机响应的事件
// 			OnSkillFirstNewRelease();
//
// 			DS_ActionBusArguGroup animationDS =
// 				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_RequireAnimationByAnimationInfo_技能要求动画操作);
// 			animationDS.ObjectArgu1 = _sai_release;
// 			animationDS.ObjectArgu2 = this;
// 			
// 			_gameplayEventManagerRef.StartGameplayEvent(_GEMActionName);
// 			
// 			_selfActionBusRef.TriggerActionByType(animationDS);
//
//
//
// 			return ds;
// 		}
//
// 		protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
// 		{
// 			var str = oInfo.OccupationInfoConfigName;
// 			if (str.Equals(_sai_release.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				return true;
// 			}
// 			else
// 			{
// 				return false;
// 			}
// 		}
//
// 		protected override void _ABC_OnSpineGeneralAnimationEventWithString(DS_ActionBusArguGroup ds)
// 		{
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
//
// 			if (string.Equals(ds.ObjectArgu2 as string, _animEventName_ReleaseTakeEffect, StringComparison.OrdinalIgnoreCase))
// 			{
// 				var vfx_release = _VFX_GetOrInstantiateNew(_vfxConfigName_Release, true, GetCurrentDamageType)._VFX_2_SetPositionToGlobalPosition(this)
// 					._VFX__10_PlayThis();
// 				_skillState = SkillStateTypeEnum.Active_生效中;
// 				//在此施加buff
// 				var applyResult = _characterBehaviourRef.ReceiveBuff_TryApplyBuff(
// 					RolePlay_BuffTypeEnum.FromSkill_EnergyBurst_能量狂涌,
// 					_characterBehaviourRef,
// 					_characterBehaviourRef);
// 				if (applyResult == BuffApplyResultEnum.Success ||
// 				    applyResult == BuffApplyResultEnum.AlreadyExistsAndRefresh)
// 				{
// 					Buff_EnergyBurst targetBuff =
// 						_characterBehaviourRef.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
// 							.FromSkill_EnergyBurst_能量狂涌) as Buff_EnergyBurst;
// 					targetBuff.ResetExistDurationAs(SkillDuration);
// 					targetBuff.ResetAvailableTimeAs(SkillDuration);
// 					targetBuff.AttackPowerBonus = AttackPowerBonus;
// 					targetBuff.AttackPowerBonusCalculatePosition = AttackPowerBonusCalculatePosition;
// 					targetBuff.SkillCDReducePartial = SkillCDReducePartial;
// 					targetBuff.OnExistBuffRefreshed(_characterBehaviourRef);
//
// 					var damageTypeBuff = RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
// 							.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
//
// 					// SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.LayoutContentInSO.DamageApplyInfo
// 					// 	.DamageType = damageTypeBuff.CurrentDamageType;
// 					// var layoutRuntime =
// 					// 	SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.SpawnLayout(
// 					// 		RelatedRPSkillCaster as I_RP_Projectile_ObjectCanReleaseProjectile);
//
//
//
// 				}
// 			}
// 			
// 		}
// 		protected override void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
// 		{
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
// 			if (string.Equals(ds.ObjectArguStr as string,
// 				_sai_release.ConfigName,
// 				StringComparison.OrdinalIgnoreCase))
// 			{
// 				(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
// 				ReturnToIdleAfterSkill();
// 			}
// 		}
// 		// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupySourceInfo)
// 		// {
// 		// 	var fixedType = GetFixedOccupiedCancelType(occupySourceInfo);
// 		// 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupySourceInfo);
// 		// 	switch (_skillState)
// 		// 	{
// 		// 		case SkillStateTypeEnum.None_无事发生:
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Releasing_释放中:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Active_生效中:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		default:
// 		// 			throw new ArgumentOutOfRangeException();
// 		// 	}
// 		// }
// 		public override void Clear_FullClearAllSkillContentImmediate()
// 		{
// 			(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
// 			_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.FromSkill_EnergyBurst_能量狂涌);
// 			VFX_GeneralClear(true);
// 		}
// 		public override void Clear_PartialClearNotImmediate()
// 		{
// 			(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
// 			_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.FromSkill_EnergyBurst_能量狂涌);
// 			VFX_GeneralClear();
// 		}
//
// 		public override Sprite GetCurrentSprite(DamageTypeEnum @override = DamageTypeEnum.None)
// 		{
// 			switch ((RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
// 				.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType).CurrentDamageType)
// 			{
// 				case DamageTypeEnum.YuanNengGuang_源能光:
// 					return SpritePairs.Find((pair => pair.Desc.Contains("光"))).SpriteAsset;
// 				case DamageTypeEnum.YuanNengDian_源能电:
// 					return SpritePairs.Find((pair => pair.Desc.Contains("电"))).SpriteAsset;
// 			}
// 			return null;
// 		}
// 	}
// }