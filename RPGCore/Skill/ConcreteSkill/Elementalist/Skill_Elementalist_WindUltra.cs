// using System;
// using System.Collections.Generic;
// using ARPG.Character;
// using ARPG.Character.Base;
// using ARPG.Character.Enemy;
// using ARPG.Manager;
// using GameplayEvent;
// using GameplayEvent.SO;
// using Global;
// using Global.ActionBus;
// using RPGCore.DataEntry;
// using RPGCore.Interface;
// using RPGCore.Projectile.Layout;
// using RPGCore.Skill.Config;
// using RPGCore.Skill.SkillSelector;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.InputSystem;
// namespace RPGCore.Skill.ConcreteSkill.Elementalist
// {
// 	public class Skill_Elementalist_WindUltra : BaseRPSkill, I_SkillAsUltraSkill, I_SkillContainStoicToStiffness , I_SkillNeedShowProgress , I_SkillContainResistToAbnormal
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
//
// 		[SerializeField, LabelText("vfx_施法准备"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_prepare;
//
// 		private PerVFXInfo _VfxInfo_Prepare;
//
//
// 		[SerializeField, LabelText("vfx_施法释放"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_release;
//
// 		private PerVFXInfo _vfxInfo_Release;
//
//
// 		[SerializeField, LabelText("vfx_法阵目的地"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		private string _vfx_destination;
//
// 		private PerVFXInfo _vfxInfo_Destination;
//
//
// 		[SerializeField, LabelText("使用的版面"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public SOConfig_ProjectileLayout RelatedLayoutConfig;
// 		
//
// 		[SerializeField, LabelText("场地持续时间"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float FieldDuration = 8f;
//
// 		[SerializeField, LabelText("最大施法距离半径"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float CastRangeRadius = 6f;
//
// 		[SerializeField, LabelText("场地半径"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float FieldRadius = 10f;
//
// 		[SerializeField, LabelText("牵引等效每秒距离-对重量1"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float PullDistancePerSecond = 1.8f;
// 		
// 		[SerializeField,LabelText("每重量降低牵引速度百分比-累乘"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float PullDistancePerWeight = 0.20f;
// 		
//
// 		[SerializeField, LabelText("牵引最大力度"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float MaxPullForce = 5f;
// 		
// 		
// 		
// 		
// 		
// 		[SerializeField, LabelText("需要触发的游戏事件——开始准备"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/事件", Alignment = TitleAlignments.Centered)]
// 		protected SOConfig_PrefabEventConfig _ge_OnPrepare;
// 		[SerializeField, LabelText("需要触发的游戏事件——确定释放"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/事件", Alignment = TitleAlignments.Centered)]
// 		protected SOConfig_PrefabEventConfig _ge_OnRelease;
//
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
// 		
//
//
//
//
// 		private enum SkillStateTypeEnum
// 		{
// 			None_无事发生 = 0,
// 			Preparing_施法前摇 = 1,
// 			Holding_等待选点 = 2,
// 			Releasing_正在施法 = 3,
// 			TakingEffect_技能生效中 = 5, }
//
// 		private SkillStateTypeEnum _skillState = SkillStateTypeEnum.None_无事发生;
//
//
//
//
// 		private CharacterOnMapManager _comRef;
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
//
// 		}
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
// 		protected virtual void _IC_ReceiveSkillInput_NormalPerformed(InputAction.CallbackContext context)
// 		{
//
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
//
// 					if (!CheckIfDataEntryEnough())
// 					{
// 						return;
// 					}
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
// 			
// 		}
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
//
// 		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
// 		{
// 			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
// 			switch (_skillState)
// 			{
// 				case SkillStateTypeEnum.Preparing_施法前摇:
// 				case SkillStateTypeEnum.Holding_等待选点:
// 					UpdateIndicator();
// 					(this as I_SkillNeedShowProgress).ShowProgressTick(currentTime, currentFrameCount, delta);
// 					break;
// 				case SkillStateTypeEnum.Releasing_正在施法:
// 					if (currentTime > _willSpawnFieldTime)
// 					{
// 						_skillState = SkillStateTypeEnum.TakingEffect_技能生效中;
// 						_Internal_GeneralRequireAnimationEvent(_sai_ending);
// 						_nextTakeEffectTime = currentTime;
// 						_willEndFieldTime = currentTime + FieldDuration;
//
// 						_vfxInfo_Destination = _VFX_GetOrInstantiateNew(_vfx_destination)
// 							._VFX_2_SetPositionToGlobalPosition(_registeredSpawnPosition).VFX_4_SetLocalScale(FieldRadius)._VFX__10_PlayThis();
// 						var configRuntime = RelatedLayoutConfig.SpawnLayout_NoAutoStart(_characterBehaviourRef);
// 						configRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition = _registeredSpawnPosition;
// 						configRuntime.LayoutContentInSO.RelatedProjectileScale = FieldRadius;
// 						configRuntime.LayoutHandlerFunction.StartLayout();
// 					}
// 					break;
// 				case SkillStateTypeEnum.TakingEffect_技能生效中:
// 					if (currentTime > _willEndFieldTime)
// 					{
// 						_skillState = SkillStateTypeEnum.None_无事发生;
// 						ClearVFX();
// 						return;
// 					}
// 					SkillTakeEffect_Pull(currentTime, currentFrameCount, delta);
// 					if (currentTime > _nextTakeEffectTime)
// 					{
// 						SkillTakeEffect_TakeDamage();
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
// 		
// 		protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
// 		{
// 			if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_prepare.OccupationInfo))
// 			{
// 				DBug.LogWarning($"技能风超 准备 占用失败了，来自占用信息{_sai_prepare.OccupationInfo.OccupationInfoConfigName}");
// 			}
//
// 			_skillState = SkillStateTypeEnum.Preparing_施法前摇;
// 			var ds = base.OnSkillBeginPrepare(autoLaunch);
// 			(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
// 			(this as I_SkillContainResistToAbnormal).AddResist_SkillVersion();
// 			_PrepareIndicator();
// 			OnSkillResetCoolDown();
// 			OnSkillConsumeSP();
//
// 			_Internal_GeneralRequireAnimationEvent(_sai_prepare, true);
//
// 			
// 			if (_ge_OnPrepare != null)
// 			{
// 				GameplayEventManager.Instance.StartGameplayEvent(_ge_OnPrepare);
// 			}
//
// 			
//
// 			if (_VfxInfo_Prepare == null)
// 			{
// 				_VfxInfo_Prepare = _VFX_GetOrInstantiateNew(_vfx_prepare);
// 			}
// 			_VfxInfo_Prepare?._VFX_1_ApplyPresetTransformOffset(_characterBehaviourRef.GetRelatedVFXContainer())
// 				._VFX__10_PlayThis();
//
//
//
// 			return ds;
// 		}
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
// 				DBug.LogWarning($"技能风U  释放占用  失败，占用信息{_sai_release.OccupationInfo}");
// 			}
// 			DeactivateIndicator();
//
//
// 			_skillState = SkillStateTypeEnum.Releasing_正在施法;
// 			_VfxInfo_Prepare.VFX_StopThis();
//
//
// 			if (_ge_OnRelease != null)
// 			{
// 				GameplayEventManager.Instance.StartGameplayEvent(_ge_OnRelease);
// 			}
//
// 			_registeredSpawnPosition = GetValidSpawnPosition();
// 			
// 			
// 			_vfxInfo_Release = _VFX_GetOrInstantiateNew(_vfx_release)._VFX__10_PlayThis();
// 			_willSpawnFieldTime = BaseGameReferenceService.CurrentFixedTime + _releaseEffectDelay;
//
//
//
// 			_Internal_GeneralRequireAnimationEvent(_sai_release, true);
//
//
//
//
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
// 		/// <summary>
// 		/// <para>技能生效：造成伤害部分</para>
// 		/// </summary>
// 		protected void SkillTakeEffect_TakeDamage()
// 		{
// 		}
//
//
// 		
// 		protected void SkillTakeEffect_Pull(float ct,int cf , float delta)
// 		{
// 			//get character 
// 			List<EnemyARPGCharacterBehaviour> list = _comRef.GetEnemyListInRange(_registeredSpawnPosition, FieldRadius);
//
// 			foreach (EnemyARPGCharacterBehaviour perBehaviour in list)
// 			{
// 				float behaviourWeight = perBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.MovementMass_重量)
// 					.CurrentValue;
// 				if (behaviourWeight > MaxPullForce)
// 				{
// 					continue;
// 				}
//
// 				var direction = _registeredSpawnPosition - perBehaviour.transform.position;
// 				direction.Normalize();
//
// 				float pow = behaviourWeight - 1f;
// 				float t = Mathf.Pow((1f - PullDistancePerWeight), pow);
// 				float distance = PullDistancePerSecond * t * delta;
//
//
// 				perBehaviour.TryMovePosition_OnlyXZ(distance * direction);
// 			}
//
// 		}
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
// 					DBug.LogWarning($"技能风超杀 holding循环占用失败了，这不合理，占用信息{_sai_holding.OccupationInfo}");
// 				}
// 				_skillState = SkillStateTypeEnum.Holding_等待选点;
// 				_Internal_GeneralRequireAnimationEvent(_sai_holding);
// 				if (BaseGameReferenceService.CurrentActiveInput == CurrentActiveInputEnum.PureKeyboard)
// 				{
// 					//只有等待选点的时候才可以释放，然后选点释放
// 					OnSkillFirstNewRelease();
// 				}
//
//
// 			}
// 			else if (configName.Equals(_sai_ending.ConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				// _Internal_BroadcastSkillReleaseFinish();
// 				(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
// 				(this as I_SkillContainResistToAbnormal).RemoveResist_SkillVersion();
// 			}
// 		}
//
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
//
// 		/// <summary>
// 		/// <para>清理指示器、敌人身上的特效、自己身上的特效</para>
// 		/// </summary>
// 		private void ClearVFX()
// 		{
// 			_vfxInfo_Destination.VFX_StopThis(true);
// 			_vfxInfo_Release.VFX_StopThis(true);
// 			DeactivateIndicator();
//
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
// 			indicatorPos = SubGameplayLogicManager_ARPG.Instance.GetAlignedTerrainPosition(indicatorPos) ??
// 			               indicatorPos;
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
// 		public AnimationInfoBase ActAsSheetAnimationInfo => _sai_prepare;
// 		BaseRPSkill I_SkillNeedShowProgress._selfSkillRef_Interface => this;
// 	}
// }