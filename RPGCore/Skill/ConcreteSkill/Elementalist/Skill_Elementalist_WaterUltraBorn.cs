// using System;
// using System.Collections.Generic;
// using ARPG.Character;
// using ARPG.Character.Base;
// using ARPG.Character.Base.CustomSpineData;
// using ARPG.Character.Enemy;
// using ARPG.Manager;
// using GameplayEvent;
// using GameplayEvent.SO;
// using Global;
// using Global.ActionBus;
// using RPGCore.Interface;
// using RPGCore.Projectile.Layout;
// using RPGCore.Projectile.Layout.LayoutComponent;
// using RPGCore.Skill.Config;
// using RPGCore.Skill.SkillSelector;
// using RPGCore.UtilityDataStructure;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.InputSystem;
// namespace RPGCore.Skill.ConcreteSkill.Elementalist
// {
// 	[Serializable]
// 	public class Skill_Elementalist_WaterUltraBorn : BaseRPSkill, I_SkillAsUltraSkill, I_SkillContainStoicToStiffness , I_SkillNeedShowProgress , I_SkillContainResistToAbnormal
// 	{
//
//
// 		private AnimationInfoBase _sai_prepare;
//
// 		private AnimationInfoBase _sai_release;
//
// 		[SerializeField, LabelText("an_holding动作"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
// 		private string _an_holding;
// 		private AnimationInfoBase _sai_holding;
//
// 		[SerializeField, LabelText("an_结束动作"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
// 		private string _an_end;
// 		private AnimationInfoBase _sai_ending;
//
// 		[SerializeField, LabelText("自身释放延迟"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered)]
// 		private float _releaseEffectDelay = 4f;
//
//
// 		[SerializeField, LabelText("指示器_选点大范围"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered)]
// 		private GameObject _indicatorPrefab_OuterRange;
//
// 		private SkillIndicator_Range _indicatorObject_OuterRange;
//
// 		[SerializeField, LabelText("指示器_选点位置"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered)]
// 		private GameObject _indicatorPrefab_InnerPosition;
//
// 		private SkillIndicator_Range _indicatorObject_InnerPosition;
//
//
// 		[SerializeField, LabelText("vfx_准备前摇"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_prepare;
//
// 		private PerVFXInfo _vfxInfo_Prepare;
//
//
// 		[SerializeField, LabelText("vfx_场地特效准备"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_releaseOnGroundPrelude;
//
// 		private PerVFXInfo _vfxInfo_Release;
//
//
// 		[SerializeField, LabelText("vfx_场地特效_A"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_releaseOnGround_TypeA;
//
// 		private PerVFXInfo _vfxInfo_ReleaseOnGround_TypeA;
//
//
//
//
// 		[SerializeField, LabelText("vfx_场地特效_B"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_releaseOnGround_TypeB;
//
// 		private PerVFXInfo _vfxInfo_ReleaseOnGround_TypeB;
//
//
//
//
// 		[SerializeField, LabelText("vfx_场地特效_C"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_releaseOnGround_TypeC;
//
// 		private PerVFXInfo _vfxInfo_ReleaseOnGround_TypeC;
//
//
//
// 		[SerializeField, LabelText("vfxPrefab_持续受击"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private GameObject _vfxPrefab_onHit;
//
// 		[SerializeField, LabelText("vfxPrefab_持续回复"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private GameObject _vfxPrefab_onHeal;
//
// 		[SerializeField, LabelText("回复血量百分比"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
// 		public float _heal_Partial;
//
// 		[SerializeField, LabelText("伤害覆盖"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
// 		public ConSer_DamageApplyInfo DamageApplyInfoOverride;
// 		[SerializeField, LabelText("治疗覆盖_会内部处理一个默认版面"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
// 		public ConSer_DamageApplyInfo HealApplyInfoOverride;
//
// 		[SerializeField, LabelText("场地持续时间"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
// 		public float FieldDuration = 8f;
//
// 		[SerializeField, LabelText("最大施法距离半径"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
// 		public float CastRangeRadius = 6f;
//
// 		[SerializeField, LabelText("场地半径"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
// 		public float FieldRadius = 10f;
// 		[SerializeField, LabelText("每次作用时间间隔"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
// 		public float TakeEffectInterval = 1f;
//
//
// 		[SerializeField, LabelText("需要触发的游戏事件——开始准备"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/事件", Alignment = TitleAlignments.Centered)]
// 		protected SOConfig_PrefabEventConfig _ge_OnPrepare;
// 		[SerializeField, LabelText("需要触发的游戏事件——确定释放"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/事件", Alignment = TitleAlignments.Centered)]
// 		protected SOConfig_PrefabEventConfig _ge_OnRelease;
//
// 		protected SOConfig_ProjectileLayout _layoutTemplate_Heal;
//
//
// 		private CharacterOnMapManager _comRef;
//
// 		/// <summary>
// 		/// <para>记录的将要生成的位置</para>
// 		/// </summary>
// 		protected Vector3 _registeredSpawnPosition;
//
// 		/// <summary>
// 		/// <para>将会生成场地的时间点</para>
// 		/// </summary>
// 		private float _willSpawnFieldTime;
//
// 		/// <summary>
// 		/// <para>将会结束场地的时间点，也是结束技能的时间点</para>
// 		/// </summary>
// 		private float _willEndFieldTime;
//
// 		/// <summary>
// 		/// <para>下一次执行效果的时间点</para>
// 		/// </summary>
// 		private float _nextTakeEffectTime;
//
//
// 		private enum SkillStateTypeEnum
// 		{
// 			None_无事发生 = 0,
// 			Preparing_施法前摇 = 1,
// 			Holding_等待选点 = 2,
// 			Releasing_正在施法 = 3,
// 			TakingEffect_技能生效中 = 5,  
// 		}
//
// 		private SkillStateTypeEnum _skillState = SkillStateTypeEnum.None_无事发生;
//
//
// 		public override void InitOnObtain(
// 			RPSkill_SkillHolder skillHolderRef,
// 			SOConfig_RPSkill configRuntimeInstance,
// 			I_RP_ObjectCanReleaseSkill parent,
// 			SkillSlotTypeEnum slot)
// 		{
// 			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
// 			// _sai_prepare =
// 			// 	GetAnimationInfoByConfigName(configRuntimeInstance.ContentInSO._AN_SkillPrepareSpineAnimationName);
// 			// _sai_release =
// 			// 	GetAnimationInfoByConfigName(configRuntimeInstance.ContentInSO._AN_SkillReleaseSpineAnimationName);
// 			_sai_holding = GetAnimationInfoByConfigName(_an_holding);
// 			_sai_ending = GetAnimationInfoByConfigName(_an_end);
// 			_comRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
// 			// _layoutTemplate_Heal =
// 				// UnityEngine.Object.Instantiate(configRuntimeInstance.ContentInSO.DefaultProjectileLayout);
// 			
// 		}
//
//
// 		protected override void BindingInput()
// 		{
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
// 			var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
// 			var ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed += _IC_ReceiveSkillInput_NormalPerformed;
// 			iar.BattleGeneral.FireBase.performed += _IC_ReceiveFireInput_;
// 		}
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
// 			var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
// 			iar.BattleGeneral.FireBase.performed -= _IC_ReceiveFireInput_;
// 		}
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
// 			OnSkillSlotReceiveInput();
// 			switch (_skillState)
// 			{
// 				//试试能不能前摇
// 				case SkillStateTypeEnum.None_无事发生:
// 					if (!IfReactToInput())
// 					{
// 						return;
// 					}
// 					if (!CheckIfDataEntryEnough())
// 					{
// 						return;
// 					}
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
// 				case SkillStateTypeEnum.Preparing_施法前摇:
// 					break;
// 				case SkillStateTypeEnum.Holding_等待选点:
// 					break;
// 				case SkillStateTypeEnum.Releasing_正在施法:
// 					break;
// 				case SkillStateTypeEnum.TakingEffect_技能生效中:
// 					break;
// 			}
// 		}
//
//
//
// 		protected virtual void _IC_ReceiveFireInput_(InputAction.CallbackContext context)
// 		{
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
// 			if (_skillState != SkillStateTypeEnum.Holding_等待选点)
// 			{
// 				return;
// 			}
//
//
// 			//只有等待选点的时候才可以释放，然后选点释放
// 			OnSkillFirstNewRelease();
// 		}
//
//
// 		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
// 		{
// 			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
// 			switch (_skillState)
// 			{
// 				case SkillStateTypeEnum.Preparing_施法前摇:
// 				case SkillStateTypeEnum.Holding_等待选点:
// 					(this as I_SkillNeedShowProgress).ShowProgressTick( currentTime, currentFrameCount, delta);
// 					UpdateIndicator();
// 					break;
// 				case SkillStateTypeEnum.Releasing_正在施法:
// 					if (currentTime > _willSpawnFieldTime)
// 					{
// 						_skillState = SkillStateTypeEnum.TakingEffect_技能生效中;
// 						_Internal_GeneralRequireAnimationEvent(_sai_ending);
// 						
// 						_nextTakeEffectTime = currentTime;
// 						_willEndFieldTime = currentTime + FieldDuration;
// 						_vfxInfo_ReleaseOnGround_TypeB =
// 							_VFX_GetOrInstantiateNew(_vfx_releaseOnGround_TypeB)
// 								._VFX_2_SetPositionToGlobalPosition(_registeredSpawnPosition);
// 						_vfxInfo_ReleaseOnGround_TypeB
// 							._VFX__5_OffsetLocalPositionByWorldUp(_vfxInfo_ReleaseOnGround_TypeB.OffsetLength.y)
// 							.VFX_4_SetLocalScale(FieldRadius)._VFX__10_PlayThisWithoutRelocated();
// 						_vfxInfo_ReleaseOnGround_TypeA = _VFX_GetOrInstantiateNew(_vfx_releaseOnGround_TypeA)
// 							._VFX_2_SetPositionToGlobalPosition(_registeredSpawnPosition);
// 						_vfxInfo_ReleaseOnGround_TypeA._VFX__5_OffsetLocalPositionByWorldUp(_vfxInfo_ReleaseOnGround_TypeA.OffsetLength.y)
// 							.VFX_4_SetLocalScale(FieldRadius)._VFX__10_PlayThisWithoutRelocated();
// 						
// 					}
// 					break;
// 				case SkillStateTypeEnum.TakingEffect_技能生效中:
// 					if (currentTime > _willEndFieldTime)
// 					{
// 						_skillState = SkillStateTypeEnum.None_无事发生;
// 						ClearVFX();
// 						return;
// 					}
// 					if (currentTime > _nextTakeEffectTime)
// 					{
// 						OnSkillTakeEffect();
// 					}
//
// 					break;
// 			}
// 		}
//
// 		protected override bool IfSkillCanCDTick()
// 		{
// 			if (_skillState == SkillStateTypeEnum.None_无事发生 || _skillState == SkillStateTypeEnum.TakingEffect_技能生效中)
// 			{
// 				return true;
// 			}
// 			else
// 			{
// 				return false;
// 			}
// 		}
//
// 		protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
// 		{
// 			if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_prepare.OccupationInfo))
// 			{
// 				DBug.LogWarning($"技能水超杀 准备 占用失败了，来自占用信息{_sai_prepare.OccupationInfo.OccupationInfoConfigName}");
// 			}
//
// 			var ds = base.OnSkillBeginPrepare(autoLaunch);
// 			(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
// 			(this as I_SkillContainResistToAbnormal).AddResist_SkillVersion();
// 			_PrepareIndicator();
// 			OnSkillResetCoolDown();
// 			OnSkillConsumeSP();
//
//
// 			_Internal_GeneralRequireAnimationEvent(_sai_prepare, true);
// 			
// 			
// 			
// 			if (_ge_OnPrepare != null)
// 			{
// 				GameplayEventManager.Instance.StartGameplayEvent(_ge_OnPrepare);
// 			}
//
//
// 			if (_vfxInfo_Prepare == null)
// 			{
// 				_vfxInfo_Prepare = _VFX_GetOrInstantiateNew(_vfx_prepare);
// 			}
// 			_vfxInfo_Prepare._VFX_1_ApplyPresetTransformOffset(_characterBehaviourRef.GetRelatedVFXContainer())._VFX__10_PlayThis();
//
//
//
// 			return ds;
// 		}
//
//
// #region Indicator
//
// 		private void _PrepareIndicator()
// 		{
// 			if (_indicatorPrefab_OuterRange != null)
// 			{
// 				if (_indicatorObject_OuterRange == null)
// 				{
// 					_indicatorObject_OuterRange = UnityEngine.Object.Instantiate(_indicatorPrefab_InnerPosition)
// 						.GetComponent<SkillIndicator_Range>();
// 				}
// 				_indicatorObject_OuterRange.gameObject.SetActive(true);
// 				_indicatorObject_OuterRange.SetRadius(CastRangeRadius);
// 				_indicatorObject_OuterRange.SetFromPosition(_characterBehaviourRef.transform.position);
// 			}
// 			if (_indicatorPrefab_InnerPosition != null)
// 			{
// 				if (_indicatorObject_InnerPosition == null)
// 				{
// 					_indicatorObject_InnerPosition = UnityEngine.Object.Instantiate(_indicatorPrefab_InnerPosition)
// 						.GetComponent<SkillIndicator_Range>();
// 				}
// 				_indicatorObject_InnerPosition.gameObject.SetActive(true);
// 				_indicatorObject_InnerPosition.SetRadius(FieldRadius);
// 				_indicatorObject_InnerPosition.SetFromPosition(_characterBehaviourRef.transform.position);
// 			}
// 		}
//
// 		private void UpdateIndicator()
// 		{
// 			if (_indicatorObject_InnerPosition != null && _indicatorObject_InnerPosition.gameObject.activeInHierarchy)
// 			{
// 				Vector3 indicatorPos = GetValidSpawnPosition();
// 				_indicatorObject_InnerPosition.SetFromPosition(indicatorPos);
// 			}
// 		}
//
// 		private Vector3 GetValidSpawnPosition()
// 		{
// 			var currentInputPosition = _playerControllerRef.InputResult_InputLogicPosition;
// 			var distance = Vector3.Distance(currentInputPosition, _characterBehaviourRef.transform.position);
// 			Vector3 indicatorPos = currentInputPosition;
// 			if (distance > CastRangeRadius)
// 			{
// 				var currentSelfPosY0 = _characterBehaviourRef.transform.position;
// 				currentSelfPosY0.y = 0f;
// 				var input_y0 = _playerControllerRef.InputResult_InputLogicPosition;
// 				input_y0.y = 0f;
// 				var dir = (input_y0 - currentSelfPosY0).normalized;
// 				var clampPos = _characterBehaviourRef.transform.position + dir * CastRangeRadius;
// 				indicatorPos = clampPos;
// 			}
// 			indicatorPos = SubGameplayLogicManager_ARPG.Instance.GetAlignedTerrainPosition(indicatorPos) ?? indicatorPos;
// 			return indicatorPos;
// 		}
//
//
// 		private void DeactivateIndicator()
// 		{
// 			if (_indicatorObject_InnerPosition != null)
// 			{
// 				_indicatorObject_InnerPosition.gameObject.SetActive(false);
// 			}
// 			if (_indicatorObject_OuterRange != null)
// 			{
// 				_indicatorObject_OuterRange.gameObject.SetActive(false);
// 			}
// 		}
//
// #endregion
//
//
// 		/// <summary>
// 		/// <para>开始释放。会开始播那个release动画，</para>
// 		/// </summary>
// 		/// <param name="autoLaunch"></param>
// 		/// <returns></returns>
// 		protected override DS_ActionBusArguGroup OnSkillFirstNewRelease(bool autoLaunch = true)
// 		{
// 			var ds = base.OnSkillFirstNewRelease(autoLaunch);
// 			(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
// 			(this as I_SkillContainResistToAbnormal).AddResist_SkillVersion();
// 			if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_release.OccupationInfo))
// 			{
// 				DBug.LogWarning($"技能水U  释放占用  失败，占用信息{_sai_release.OccupationInfo}");
// 			}
// 			DeactivateIndicator();
//
//
// 			_skillState = SkillStateTypeEnum.Releasing_正在施法;
// 			_vfxInfo_Prepare.VFX_StopThis();
// 			
// 			
// 			if (_ge_OnRelease != null)
// 			{
// 				GameplayEventManager.Instance.StartGameplayEvent(_ge_OnRelease);
// 			}
//
// 			_registeredSpawnPosition = GetValidSpawnPosition();
// 			//生成那个能量汇聚的特效
// 			_vfxInfo_Release = _VFX_GetOrInstantiateNew(_vfx_releaseOnGroundPrelude)._VFX__10_PlayThis();
// 			_willSpawnFieldTime = BaseGameReferenceService.CurrentFixedTime + _releaseEffectDelay;
//
//
//
// 			_Internal_GeneralRequireAnimationEvent(_sai_release);
//
//
//
//
//
// 			return ds;
// 		}
// 		
// 		
//
// 		private struct _VFXPair_Enemy
// 		{
// 			public EnemyARPGCharacterBehaviour EnemyRef;
// 			public ParticleSystem EffectRef;
// 		}
//
//
//
//
// 		/// <summary>
// 		/// <para>在范围内的敌人们</para>
// 		/// <para>每次生效的时候都会更新，已经不在里面的就移调，还在的就接着让它在</para>
// 		/// </summary>
// 		private List<_VFXPair_Enemy> _enemiesInField = new List<_VFXPair_Enemy>();
//
//
// 		protected void OnSkillTakeEffect()
// 		{
// 			//执行伤害
// 			//先清理现有的
// 			_nextTakeEffectTime = BaseGameReferenceService.CurrentFixedTime + TakeEffectInterval;
// 			var vfxPoolRef = VFXPoolManager.Instance;
// 			foreach (_VFXPair_Enemy perInfo in _enemiesInField)
// 			{
// 				VFXPoolManager.StopAndResetAnchorToPool(perInfo.EffectRef);
// 			}
// 			List<EnemyARPGCharacterBehaviour> tmpList =
// 				_comRef.GetEnemyListInRange(_registeredSpawnPosition, FieldRadius);
//
// 			// var layout = SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout;
// 			// LC_JustSpawn justSpawn =
// 			// 	layout.LayoutContentInSO.LayoutComponentList.Find((component => component is LC_JustSpawn)) as
// 			// 		LC_JustSpawn;
// 			// justSpawn.SetLifetime = TakeEffectInterval;
// 			//
// 			// layout.LayoutContentInSO.DamageApplyInfo = DamageApplyInfoOverride;
// 			// layout.LayoutContentInSO.RelatedProjectileScale = FieldRadius;
// 			// var layoutRuntime = layout.SpawnLayout_NoAutoStart(_characterBehaviourRef);
// 			// layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition = _registeredSpawnPosition;
// 			// layoutRuntime.LayoutHandlerFunction.StartLayout();
// 			//
// 			// //再来一个用来治疗的。治疗的层是仅对玩家神效，在1<<2上
// 			// var justSpawn_heal = _layoutTemplate_Heal.LayoutContentInSO.LayoutComponentList.Find((component =>
// 			// 	component is LC_JustSpawn)) as LC_JustSpawn;
// 			// justSpawn_heal.SetLifetime = TakeEffectInterval;
// 			// _layoutTemplate_Heal.LayoutContentInSO.DamageApplyInfo = HealApplyInfoOverride;
// 			// _layoutTemplate_Heal.LayoutContentInSO.RelatedProjectileScale = FieldRadius;
// 			// _layoutTemplate_Heal.LayoutContentInSO.CollisionInfo.CollisionLayerMask = 1 << 1;
// 			// var layoutRuntime_heal = _layoutTemplate_Heal.SpawnLayout_NoAutoStart(_characterBehaviourRef, false);
// 			// layoutRuntime_heal.LayoutHandlerFunction.OverrideSpawnFromPosition = _registeredSpawnPosition;
// 			// layoutRuntime_heal.LayoutHandlerFunction.StartLayout();
// 			
// 			
//
//
// 			//然后给他们塞
// 			foreach (EnemyARPGCharacterBehaviour perBehaviour in tmpList)
// 			{
// 				_VFXPair_Enemy newInfoPair = new _VFXPair_Enemy();
// 				newInfoPair.EnemyRef = perBehaviour;
// 				newInfoPair.EffectRef = vfxPoolRef.GetParticleSystemRuntimeByPrefab(_vfxPrefab_onHit);
// 				var pp = (perBehaviour.GetRelatedArtHelper() as I_RP_ContainVFXContainer).GetVFXHolderTransformAndRegisterVFX("仅自身",
// 					newInfoPair.EffectRef.gameObject);
// 				newInfoPair.EffectRef.transform.localScale = pp.Item2 * Vector3.one;
// 				newInfoPair.EffectRef.transform.localPosition = Vector3.zero;
// 				newInfoPair.EffectRef.transform.SetParent(pp.Item1, false);
//
// 				newInfoPair.EffectRef.Play();
// 				_enemiesInField.Add(newInfoPair);
// 			}
// 		}
//
// 		
// 		
// 		
// 		protected override void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
// 		{
// 			base._ABC_OnGeneralAnimationComplete(ds);
// 			var configName = ds.ObjectArguStr as string;
// 			//准备动作完成，将会开始播放holding循环
// 			if (configName.Equals(_sai_prepare.ConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_holding.OccupationInfo))
// 				{
// 					DBug.LogWarning($"技能水超杀 holding循环占用失败了，这不合理，占用信息{_sai_holding.OccupationInfo}");
// 				}
// 				_skillState = SkillStateTypeEnum.Holding_等待选点;
// 				_Internal_GeneralRequireAnimationEvent(_sai_holding);
// 				if (BaseGameReferenceService.CurrentActiveInput == CurrentActiveInputEnum.PureKeyboard)
// 				{
// 					//只有等待选点的时候才可以释放，然后选点释放
// 					OnSkillFirstNewRelease();
// 				}
//
// 			}
// 			else if (configName.Equals(_sai_ending.ConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				_skillState = SkillStateTypeEnum.None_无事发生;
// 				// _Internal_BroadcastSkillReleaseFinish();
// 				(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
// 				(this as I_SkillContainResistToAbnormal).RemoveResist_SkillVersion();
// 			}
// 		}
//
//
// 		/// <summary>
// 		/// <para>清理指示器、敌人身上的特效、自己身上的特效</para>
// 		/// </summary>
// 		private void ClearVFX()
// 		{
// 			_vfxInfo_ReleaseOnGround_TypeC = _VFX_GetOrInstantiateNew(_vfx_releaseOnGround_TypeC)
// 				._VFX_2_SetPositionToGlobalPosition(_vfxInfo_ReleaseOnGround_TypeB.CurrentActiveRuntimePSPlayProxyRef.transform.position).VFX_4_SetLocalScale(FieldRadius)
// 				._VFX__10_PlayThisWithoutRelocated();
// 			_vfxInfo_ReleaseOnGround_TypeB.VFX_StopThis();
// 			_vfxInfo_ReleaseOnGround_TypeA.VFX_StopThis(true);
// 			foreach (_VFXPair_Enemy perE in _enemiesInField)
// 			{
// 				perE.EffectRef.Stop();
// 			}
// 			DeactivateIndicator();
// 			_enemiesInField.Clear();
//
// 		}
//
//
//
// 		// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupyFromInfo)
// 		// {
// 		// 	var fixedType = GetFixedOccupiedCancelType(occupyFromInfo);
// 		// 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupyFromInfo);
// 		// 	switch (_skillState)
// 		// 	{
// 		// 		case SkillStateTypeEnum.None_无事发生:
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Preparing_施法前摇:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					if (!isSelf)
// 		// 					{
// 		// 						C_Debug_LogOccupyNotImplementInfo(occupyFromInfo);
// 		// 						return;
// 		// 					}
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					C_Debug_LogOccupyNotImplementInfo(occupyFromInfo);
// 		// 					return;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					break;
// 		// 				
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Holding_等待选点:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					if (!isSelf)
// 		// 					{
// 		// 						C_Debug_LogOccupyNotImplementInfo(occupyFromInfo);
// 		// 						return;
// 		// 					}
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					C_Debug_LogOccupyNotImplementInfo(occupyFromInfo);
// 		// 					return;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		case SkillStateTypeEnum.Releasing_正在施法:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					if (!isSelf)
// 		// 					{
// 		// 						C_Debug_LogOccupyNotImplementInfo(occupyFromInfo);
// 		// 						return;
// 		// 					}
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					C_Debug_LogOccupyNotImplementInfo(occupyFromInfo);
// 		// 					return;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		case SkillStateTypeEnum.TakingEffect_技能生效中:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					//都无事发生
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 					//无事发生
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					_skillState = SkillStateTypeEnum.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			
// 		// 			break;
// 		// 		default:
// 		// 			throw new ArgumentOutOfRangeException();
// 		// 	}
// 		// }
// 		public override void Clear_FullClearAllSkillContentImmediate()
// 		{
// 			VFX_GeneralClear(true);
// 			DeactivateIndicator();
// 		}
// 		public override void Clear_PartialClearNotImmediate()
// 		{
// 			VFX_GeneralClear();
// 			DeactivateIndicator();
// 		}
// 		protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
// 		{
// 			var str = oInfo.OccupationInfoConfigName;
// 			if (str.Equals(_sai_holding.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase) ||
// 			    str.Equals(_sai_release.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase) ||
// 			    str.Equals(_sai_prepare.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				return true;
// 			}
//
// 			return false;
// 		}
//
//
// 		public override DS_ActionBusArguGroup ClearBeforeRemove()
// 		{
// 			DeactivateIndicator();
// 			if (_indicatorObject_InnerPosition != null)
// 			{
// 				UnityEngine.Object.Destroy(_indicatorObject_InnerPosition.gameObject);
// 				
// 			}
// 			if (_indicatorObject_OuterRange != null)
// 			{
// 				UnityEngine.Object.Destroy(_indicatorObject_OuterRange.gameObject);
// 			}
// 			
// 			return base.ClearBeforeRemove();
// 		}
// 		public AnimationInfoBase ActAsSheetAnimationInfo => _sai_prepare;
// 		BaseRPSkill I_SkillNeedShowProgress._selfSkillRef_Interface => this;
// 	}
// }