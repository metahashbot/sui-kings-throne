// using System;
// using System.Collections.Generic;
// using ARPG.Character;
// using ARPG.Character.Base;
// using ARPG.Character.Base.CustomSpineData;
// using ARPG.Character.Enemy.AI.Decision;
// using ARPG.Manager;
// using Global;
// using Global.ActionBus;
// using RPGCore.Buff.ConcreteBuff;
// using RPGCore.Interface;
// using RPGCore.Projectile.Layout;
// using RPGCore.Skill.Config;
// using RPGCore.Skill.SkillSelector;
// using RPGCore.UtilityDataStructure;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.Pool;
// using WorldMapScene.Character;
// using Object = UnityEngine.Object;
//
// namespace RPGCore.Skill.ConcreteSkill
// {
// 	[TypeInfoBox("剑士——剑士冲刺A1\n")]
// 	[Serializable]
// 	public class Skill_Swordman_DashA1 : BaseRPSkill, I_SkillIsDisplacementSkill, I_SkillContainStoicToStiffness
// 	{
// 		private DisplacementSkillInfo _relatedDisplacementSkillInfo;
//
// 		[SerializeField, LabelText("开启快速施法？"), FoldoutGroup("配置", true)]
// 		public bool QuickRelease = true;
//
// 		[SerializeField, Required, LabelText("指示器：外圈"), FoldoutGroup("配置", true)]
// 		private GameObject _prefab_Indicator_OuterRange;
//
// 		private SkillIndicator_Range _indicator_OuterRange_Instance;
//
// 		[SerializeField, Required, LabelText("指示器：线"), FoldoutGroup("配置", true)]
// 		private GameObject _prefab_Indicator_Line;
//
// 		private SkillIndicator_Line _indicator_Line_Instance;
//
//
// 		[SerializeField, LabelText("VFX_技能准备"), FoldoutGroup("配置/VFX", true), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _VFX_DashPrepare;
//
//
// 		[SerializeField, LabelText("VFX_技能准备结束"), FoldoutGroup("配置/VFX", true), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _VFX_DashPrepareDone;
//
// 		protected PerVFXInfo _vfxRuntimeInfo_DashPrepare;
//
//
// 		[SerializeField, LabelText("VFX_冲刺过程原地的风"), FoldoutGroup("配置/VFX", true), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _VFX_DashOriginalPositionWind;
//
//
// 		[SerializeField, LabelText("VFX_冲刺的跟随_固定长度"), FoldoutGroup("配置/VFX", true), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _VFX_VFXMainFollow;
//
// 		protected PerVFXInfo _vfxInfo_MainFollow;
//
// 		[SerializeField, LabelText("地面每多少距离烧焦一次"), FoldoutGroup("配置/VFX", true), GUIColor(187f / 255f, 1f, 0f)]
// 		public float _VFX_GroundEffectPerDistance = 0.5f;
//
//
// 		[SerializeField, LabelText("VFX_冲刺跟随_可拉伸的"), FoldoutGroup("配置/VFX", true), GUIColor(187f / 255f, 1f, 0f)]
// 		public string _VFX_DashExtendableFollow;
//
// 		[SerializeField, LabelText("冲刺跟随可拉伸距离"), FoldoutGroup("配置/VFX", true)]
// 		public float _VFX_MinDashExtendableDistance = 4.5f;
//
//
// 		[SerializeField, LabelText("_结束动作动画配置名字"), FoldoutGroup("配置/动画", true),
// 		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
// 		private string _AN_DashEndSpineName;
//
// 		private AnimationInfoBase _sai_DashEnd;
//
// 		[SerializeField, LabelText("施法循环动画配置名字"), FoldoutGroup("配置/动画", true),
// 		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
// 		private string _an_PrepareDoneLooping;
//
//
// 		[SerializeField, LabelText("最大可用的位移距离"), FoldoutGroup("配置/数值", true)]
// 		public float MaxDisplacementRange = 8f;
//
//
// 		[SerializeField, LabelText("每单位冲刺的移动速度"), FoldoutGroup("配置/数值", true)]
// 		public float DashMovementSpeed = 12f;
//
// 		[SerializeField, LabelText("冲刺完成度曲线"), FoldoutGroup("配置/数值", true)]
// 		public AnimationCurve DashFinishCurve;
//
// 		[SerializeField, LabelText("覆盖版面的伤害信息"), FoldoutGroup("配置/数值", true)]
// 		public ConSer_DamageApplyInfo OverrideDamageApplyInfo;
//
//
// 		[ShowInInspector, LabelText("上次评估的位置"), FoldoutGroup("运行时", true)]
// 		private Vector3 _lastEvaluatePosition;
//
//
// 		[ShowInInspector, LabelText("当前造成伤害的戳"), FoldoutGroup("运行时", true)]
// 		private int _currentDamageStamp;
//
//
// 		[ShowInInspector, LabelText("关联的运行时PLayout"), FoldoutGroup("运行时", true)]
// 		private SOConfig_ProjectileLayout _relatedProjectileLayoutRuntimeInstance;
//
//
// 		private AnimationInfoBase _sai_prepare;
// 		private AnimationInfoBase _sai_release;
// 		private AnimationInfoBase _sai_prepareLoop;
//
//
// 		private enum SkillStateType
// 		{
// 			None_无事发生 = 0,
// 			Prepare_准备中 = 1,
// 			Holding_等待释放 = 2,
// 			Releasing_释放中 = 3,
// 			Ending_结束后摇 = 4, }
// 		private SkillStateType _selfSkillState;
//
//
// 		public override void InitOnObtain(
// 			RPSkill_SkillHolder skillHolderRef,
// 			SOConfig_RPSkill configRuntimeInstance,
// 			I_RP_ObjectCanReleaseSkill parent,
// 			SkillSlotTypeEnum slot)
// 		{
// 			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
// 			_selfSkillState = SkillStateType.None_无事发生;
// 			// _sai_prepare =
// 			// 	GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 			// 		._AN_SkillPrepareSpineAnimationName);
// 			_sai_prepare.OccupationInfo.RelatedInterface = this;
// 			// _sai_release =
// 			// 	GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 			// 		._AN_SkillReleaseSpineAnimationName);
// 			_sai_release.OccupationInfo.RelatedInterface = this;
// 			_sai_prepareLoop = GetAnimationInfoByConfigName(_an_PrepareDoneLooping);
// 			_sai_prepareLoop.OccupationInfo.RelatedInterface = this;
// 			_sai_DashEnd = GetAnimationInfoByConfigName(_AN_DashEndSpineName);
// 			// configRuntimeInstance.ContentInSO.DefaultProjectileLayout.LayoutContentInSO.DamageApplyInfo =
// 			// 	OverrideDamageApplyInfo;
// 			
// 		}
//
//
// 		protected override void BindingInput()
// 		{
// 			InputAction_ARPG inputActionRef = GameReferenceService_ARPG.Instance.InputActionInstance;
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
//
// 			InputAction ia = GetTargetInputActionRef(SkillSlot);
// 			ia.performed += _IC_ReceiveSkillInput_NormalPerformed;
//
// 			inputActionRef.BattleGeneral.FireBase.performed += _IC_ReceiveFireInput;
// 			inputActionRef.BattleGeneral.Cancel.performed += _IC_ReceiveCancelInput;
// 		}
//
// 		protected override void UnbindInput()
// 		{
// 			InputAction_ARPG inputActionRef = GameReferenceService_ARPG.Instance.InputActionInstance;
//
// 			InputAction ia = GetTargetInputActionRef(SkillSlot);
// 			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
// 			{
// 				return;
// 			}
//
// 			ia.performed -= _IC_ReceiveSkillInput_NormalPerformed;
//
// 			inputActionRef.BattleGeneral.FireBase.performed -= _IC_ReceiveFireInput;
// 			inputActionRef.BattleGeneral.Cancel.performed -= _IC_ReceiveCancelInput;
// 		}
//
// 		/// <summary>
// 		/// <para>接收到同一技能的输入——技能按键本身</para>
// 		/// </summary>
// 		protected void _IC_ReceiveSkillInput_NormalPerformed(InputAction.CallbackContext context)
// 		{
// 			//不是自己，无事发生
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
// 			OnSkillSlotReceiveInput();
//
//
//
// 			switch (_selfSkillState)
// 			{
// 				case SkillStateType.None_无事发生:
//
// 					//
// 					if (!IfReactToInput())
// 					{
// 						return;
// 					}
// 					if (!CheckIfDataEntryEnough())
// 					{
// 						return;
// 					}
// 					if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_prepare.OccupationInfo))
// 					{
// 						return;
// 					}
// 					//到这里了，开始准备
// 					OnSkillBeginPrepare();
//
// 					break;
// 				case SkillStateType.Prepare_准备中:
// 					//准备过程中不响应
// 					break;
// 				case SkillStateType.Holding_等待释放:
// 					//如果是自动施法，那应当不会有这个状态，在Complete的时候就直接释放了
// 					if (!QuickRelease)
// 					{
// 						OnSkillFirstNewRelease();
// 					}
// 					break;
// 				case SkillStateType.Releasing_释放中:
// 					//不响应
// 					break;
// 				case SkillStateType.Ending_结束后摇:
// 					//不响应
// 					break;
// 			}
// 		}
//
//
//
// 		/// <summary>
// 		/// <para>开始“准备”。将会播放准备动画，状态设置为准备中。准备动画结束后会</para>
// 		/// </summary>
// 		/// <param name="autoLaunch"></param>
// 		/// <returns></returns>
// 		protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
// 		{
// 			var ds = base.OnSkillBeginPrepare(autoLaunch);
//
//
//
// 			DS_ActionBusArguGroup animationDS = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
// 				.L_Skill_RequireAnimationByAnimationInfo_技能要求动画操作);
// 			animationDS.ObjectArgu1 = _sai_prepare;
// 			animationDS.ObjectArgu2 = this;
// 			_selfActionBusRef.TriggerActionByType(animationDS);
//
// 			ClearVFXObject();
//
// 			// _indicator_InnerRange_Instance =
// 			// 	Object.Instantiate(_prefab_Indicator_InnerRage).GetComponent<SkillIndicator_Range>();
// 			_indicator_OuterRange_Instance =
// 				Object.Instantiate(_prefab_Indicator_OuterRange).GetComponent<SkillIndicator_Range>();
// 			_indicator_Line_Instance = Object.Instantiate(_prefab_Indicator_Line).GetComponent<SkillIndicator_Line>();
//
//
// 			_indicator_OuterRange_Instance?.SetRadius(MaxDisplacementRange);
// 			_indicator_OuterRange_Instance?.SetFromPosition(_characterBehaviourRef.transform.position);
//
//
//
// 			//准备时的特效
// 			_vfxRuntimeInfo_DashPrepare = _VFX_GetOrInstantiateNew(_VFX_DashPrepare, true, GetCurrentDamageType)
// 				._VFX_2_SetAnchorToOnlyScaleVFXHolder(this)._VFX__10_PlayThis();
//
// 			return ds;
// 		}
//
// 		/// <summary>
// 		/// <para>释放的那一下。构建位移信息，播放相关特效，设置并生成相关版面</para>
// 		/// </summary>
// 		protected override DS_ActionBusArguGroup OnSkillFirstNewRelease(bool autoLaunch = true)
// 		{
// 			_selfSkillState = SkillStateType.Releasing_释放中;
//
// 			var ds = base.OnSkillFirstNewRelease(autoLaunch);
// 			if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_release.OccupationInfo))
// 			{
// 				DBug.LogWarning($"技能冲刺的释放占用失败了，这并不合理，检查一下。占用信息:{_sai_release.OccupationInfo}");
// 			}
//
// 			_currentDamageStamp = this.GetHashCode() + Time.frameCount;
// 			_vfxRuntimeInfo_DashPrepare.VFX_StopThis();
// 			OnSkillResetCoolDown();
// 			OnSkillConsumeSP();
// 			(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
//
//
// 			// ClearVFXObject();
//
// 			var damageTypeBuff =
// 				RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum.ChangeCommonDamageType_常规伤害类型更改)
// 					as Buff_ChangeCommonDamageType;
// 			// SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.LayoutContentInSO.DamageApplyInfo
// 			// 	.DamageType = damageTypeBuff == null ? DamageTypeEnum.NoType_无属性 : damageTypeBuff.CurrentDamageType;
//
// 			// _relatedProjectileLayoutRuntimeInstance =
// 			// 	SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout.SpawnLayout(_characterBehaviourRef);
// 			_relatedProjectileLayoutRuntimeInstance.LayoutHandlerFunction.DamageTimeStamp = _currentDamageStamp;
//
//
// 			_lastEvaluatePosition = _characterBehaviourRef.transform.position;
// 			DS_ActionBusArguGroup animationDS = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
// 				.L_Skill_RequireAnimationByAnimationInfo_技能要求动画操作);
//
// 			// animationDS.ObjectArgu1 =
// 			// 	GetAnimationInfoByConfigName(SelfSkillConfigRuntimeInstance.ContentInSO
// 			// 		._AN_SkillReleaseSpineAnimationName);
// 			animationDS.ObjectArgu2 = this;
//
// 			_selfActionBusRef.TriggerActionByType(animationDS);
//
// 			//需要此时的输入位置，记录它的输入位置
//
// 			if (_relatedDisplacementSkillInfo == null)
// 			{
// 				_relatedDisplacementSkillInfo = GenericPool<DisplacementSkillInfo>.Get();
// 				_relatedDisplacementSkillInfo.SelfSegmentList =
// 					CollectionPool<List<DisplacementSkillInfo.DisplacementSkillInfo_Segment>,
// 						DisplacementSkillInfo.DisplacementSkillInfo_Segment>.Get();
// 			}
//
// 			_relatedDisplacementSkillInfo.SelfSegmentList.Clear();
// 			DisplacementSkillInfo.DisplacementSkillInfo_Segment tmpNewSegment =
// 				GenericPool<DisplacementSkillInfo.DisplacementSkillInfo_Segment>.Get();
// 			_relatedDisplacementSkillInfo.SelfSegmentList.Add(tmpNewSegment);
//
// 			//一个常规的OuterRange计算方法，当输入点大于OuterRange时，就是OuterRange的距离，否则就是实际距离
//
//
// 			Vector3 inputPosition = _playerControllerRef.InputResult_InputLogicPosition;
// 			var casterPosition = RelatedRPSkillCaster.GetCasterFromPosition();
// 			Vector3 resultDashDest;
// 			Vector3 inputDirectionNor = (inputPosition - casterPosition).normalized;
// 			float inputLength = Vector3.Distance(inputPosition, RelatedRPSkillCaster.GetCasterFromPosition());
// 			resultDashDest = casterPosition + inputDirectionNor * MaxDisplacementRange;
//
// 			float dashLength = Vector3.Distance(casterPosition, resultDashDest);
//
// 			tmpNewSegment.DisplacementSegmentDuration = dashLength / DashMovementSpeed;
// 			tmpNewSegment.DisplacementSegmentFromPosition = casterPosition;
// 			tmpNewSegment.DisplacementSegmentTargetPosition = resultDashDest;
// 			tmpNewSegment.DisplacementSegmentCurve = DashFinishCurve;
// 			tmpNewSegment.SegmentStartTime = BaseGameReferenceService.CurrentFixedTime;
//
// 			(_characterBehaviourRef.GetSelfRolePlayArtHelper() as PlayerARPGArtHelper).SetFaceLeft(casterPosition.x >
// 			                                                                                       resultDashDest.x);
//
//
// 			//VFX：释放时原地的  风
//
// 			var vfx_originalWind = _VFX_GetOrInstantiateNew(_VFX_DashOriginalPositionWind, true, GetCurrentDamageType)
// 				?._VFX_2_SetPositionToGlobalPosition(this)?._VFX__3_SetDirectionOnForwardOnGlobalY0(this,
// 					(tmpNewSegment.DisplacementSegmentTargetPosition - casterPosition)).VFX_4_SetLocalScale(1f)
// 				._VFX__10_PlayThis();
//
//
// 			_vfxInfo_MainFollow = _VFX_GetOrInstantiateNew(_VFX_VFXMainFollow, true, GetCurrentDamageType)?._VFX_1_ApplyPresetTransformOffset(_characterBehaviourRef.GetVFXHolderInterface())
// 				._VFX_2_SetAnchorToOnlyScaleVFXHolder(this)
// 				._VFX__3_SetDirectionOnForwardOnGlobalY0(this,
// 					(tmpNewSegment.DisplacementSegmentTargetPosition - casterPosition)).VFX_4_SetLocalScale(1f)
// 				._VFX__10_PlayThis();
//
// 			var vfx_stretch = _VFX_GetOrInstantiateNew(_VFX_DashExtendableFollow,true, GetCurrentDamageType)
// 				?._VFX_1_ApplyPresetTransformOffset(_characterBehaviourRef.GetVFXHolderInterface())
// 				._VFX__3_SetDirectionOnForwardOnGlobalY0(this,
// 					(tmpNewSegment.DisplacementSegmentTargetPosition - casterPosition))._VFX__10_PlayThis();
//
//
//
// 			// //直接把所有烧焦效果都发出来，不管了，直接扔到池里面去
// 			// var vfxGround = _VFX_GetByUID(_VFX_GroundEffect);
// 			// if (vfxGround.IsValid())
// 			// {
// 			// 	//读segment
// 			// 	float movementDistance = Vector3.Distance(tmpNewSegment.DisplacementSegmentTargetPosition,
// 			// 		tmpNewSegment.DisplacementSegmentFromPosition);
// 			// 	Vector3 dir = (tmpNewSegment.DisplacementSegmentTargetPosition -
// 			// 	               tmpNewSegment.DisplacementSegmentFromPosition).normalized;
// 			// 	dir.y = 0f;
// 			// 	for (float fromDis = 0f; fromDis < movementDistance; fromDis += _VFX_GroundEffectPerDistance)
// 			// 	{
// 			// 		var newCurrentPS = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(vfxGround.Prefab);
// 			// 		if (newCurrentPS != null)
// 			// 		{
// 			//
// 			// 			newCurrentPS.transform.position = tmpNewSegment.DisplacementSegmentFromPosition + dir * fromDis;
// 			// 			newCurrentPS.transform.localPosition += Vector3.up * 0.1f;
// 			// 			newCurrentPS.Play();
// 			// 		}
// 			// 	}
// 			//
// 			// 	
// 			// }
//
//
//
//
// 			return ds;
// 		}
//
// 		/// <summary>
// 		/// <para>输入——接收到了开火输入。尝试释放</para>
// 		/// <para>只有非自动施法的时候，才会接受这个响应</para>
// 		/// </summary>
// 		/// <param name="context"></param>
// 		protected void _IC_ReceiveFireInput(InputAction.CallbackContext context)
// 		{
// 			//不是自己，无事发生
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
// 			if (_selfSkillState == SkillStateType.Prepare_准备中)
// 			{
// 				OnSkillFirstNewRelease();
// 			}
// 		}
//
// 		/// <summary>
// 		/// <para>试图取消。只有正在准备的时候才能取消。冲出去了就不能取消了</para>
// 		/// </summary>
// 		/// <param name="context"></param>
// 		protected void _IC_ReceiveCancelInput(InputAction.CallbackContext context)
// 		{
// 			//不是自己，无事发生
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
// 			// if (_selfSkillState == SkillStateType.Prepare_准备中)
// 			// {
// 			// 	_selfSkillState = SkillStateType.None_无事发生;
// 			// 	ReturnToIdleAfterSkill();
// 			// 	ClearVFXObject();
// 			// }
// 		}
//
//
// 		public override SkillReadyTypeEnum GetSkillReadyType()
// 		{
// 			if (_selfSkillState != SkillStateType.None_无事发生)
// 			{
// 				return SkillReadyTypeEnum.Using_还在使用;
// 			}
// 			SkillReadyTypeEnum baseType = base.GetSkillReadyType();
// 			if (baseType == SkillReadyTypeEnum.Ready)
// 			{
// 				if (_selfSkillState != SkillStateType.None_无事发生)
// 				{
// 					return SkillReadyTypeEnum.BlockByCD;
// 				}
// 			}
// 			return baseType;
// 		}
//
//
//
// 		public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
// 		{
// 			base.UpdateTick(currentTime, currentFrameCount, delta);
// 		}
//
//
// 		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
// 		{
// 			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
//
// 			//如果正在活跃，则此时需要处理指示器的刷新，和朝向的刷新
//
// 			switch (_selfSkillState)
// 			{
// 				case SkillStateType.None_无事发生:
// 					break;
// 				case SkillStateType.Prepare_准备中:
// 					Vector3 inputPos = _playerControllerRef.InputResult_InputLogicPosition;
// 					var casterPos = RelatedRPSkillCaster.GetCasterFromPosition();
// 					if (casterPos.x > inputPos.x)
// 					{
// 						_characterArtHelperRef.SetFaceLeft(true);
// 					}
// 					else
// 					{
// 						_characterArtHelperRef.SetFaceLeft(false);
// 					}
//
// 					Vector3 inputPosition = _playerControllerRef.InputResult_InputLogicPosition;
// 					var casterPosition = RelatedRPSkillCaster.GetCasterFromPosition();
// 					Vector3 resultDashDest;
// 					var inputDirectionNor = (inputPosition - casterPosition).normalized;
// 					float inputLength = Vector3.Distance(inputPosition, RelatedRPSkillCaster.GetCasterFromPosition());
//
// 					resultDashDest = casterPosition + inputDirectionNor * MaxDisplacementRange;
//
//
// 					//处理指示器的刷新
// 					if (_indicator_OuterRange_Instance != null)
// 					{
// 						_indicator_OuterRange_Instance.UpdateTick(_characterBehaviourRef.transform.position);
// 					}
// 					if (_indicator_Line_Instance != null)
// 					{
// 						_indicator_Line_Instance.UpdateTick(_characterBehaviourRef.transform.position, resultDashDest);
// 					}
// 					break;
// 				case SkillStateType.Holding_等待释放:
// 					break;
// 				case SkillStateType.Releasing_释放中:
// 					DisplacementSkillInfo.DisplacementSkillInfo_Segment activeSegment =
// 						_relatedDisplacementSkillInfo.SelfSegmentList[0];
//
//
// 					var currentEvaluateT = (currentTime - activeSegment.SegmentStartTime) /
// 					                       activeSegment.DisplacementSegmentDuration;
// 					var evaluatePartial = activeSegment.DisplacementSegmentCurve.Evaluate(currentEvaluateT);
//
// 					var evaluatePosition = Vector3.Lerp(activeSegment.DisplacementSegmentFromPosition,
// 						activeSegment.DisplacementSegmentTargetPosition,
// 						evaluatePartial);
// 					Vector3 deltaMovement = evaluatePosition - _lastEvaluatePosition;
//
// 					_characterBehaviourRef.TryMovePosition_XYZ(deltaMovement, true);
//
//
// 					_lastEvaluatePosition = evaluatePosition;
//
// 					//大于1f，技能结束了
// 					if (currentEvaluateT > 1f)
// 					{
// 						_selfSkillState = SkillStateType.None_无事发生;
// 						_characterBehaviourRef.TryMovePosition_XYZ(Vector3.zero, true);
// 						_relatedProjectileLayoutRuntimeInstance.StopLayout();
// 						(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
//
// 						//技能结束，播放结束动画
// 						if (_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_DashEnd.OccupationInfo))
// 						{
// 							var ds_AniLoop = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
// 								.L_Skill_RequireAnimationByAnimationInfo_技能要求动画操作);
// 							ds_AniLoop.ObjectArgu1 = _sai_DashEnd;
// 							ds_AniLoop.ObjectArgu2 = this;
// 							_selfActionBusRef.TriggerActionByType(ds_AniLoop);
// 							ClearVFXObject();
// 						}
// 						else
// 						{
// 							DBug.LogWarning($" 技能突刺的结束阶段的占用失败了，这并不合理");
// 						}
// 					}
// 					break;
// 				case SkillStateType.Ending_结束后摇:
// 					break;
// 			}
// 		}
//
//
// 		protected override void SkillCDTick(float delta)
// 		{
// 			base.SkillCDTick(delta);
// 		}
// 		protected override bool IfSkillCanCDTick()
// 		{
// 			if (_selfSkillState != SkillStateType.None_无事发生)
// 			{
// 				return false;
// 			}
// 			return true;
// 		}
//
//
// 		protected override void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
// 		{
// 			if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
// 			{
// 				return;
// 			}
//
// 			string configName = (ds.ObjectArguStr as string);
//
// 			//准备动作完成
// 			if (string.Equals(configName, _sai_prepare.ConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				var ds_AniLoop = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
// 					.L_Skill_RequireAnimationByAnimationInfo_技能要求动画操作);
// 				ds_AniLoop.ObjectArgu1 = GetAnimationInfoByConfigName(_an_PrepareDoneLooping);
// 				ds_AniLoop.ObjectArgu2 = this;
// 				_selfActionBusRef.TriggerActionByType(ds_AniLoop);
//
// 				_selfSkillState = SkillStateType.Holding_等待释放;
// 				//VFX：技能准备完成
// 				var vfx_prepareDone = _VFX_GetOrInstantiateNew(_VFX_DashPrepareDone, true, GetCurrentDamageType)
// 					._VFX_2_SetAnchorToOnlyScaleVFXHolder(this)._VFX__10_PlayThis();
//
// 				if (QuickRelease)
// 				{
// 					OnSkillFirstNewRelease();
// 				}
// 			}
// 			//结束动画完成
// 			else if (string.Equals(configName, _sai_DashEnd.ConfigName, StringComparison.OrdinalIgnoreCase))
// 			{
// 				// _Internal_BroadcastSkillReleaseFinish();
// 				_vfxRuntimeInfo_DashPrepare.VFX_StopThis();
// 				_selfSkillState = SkillStateType.None_无事发生;
// 				ReturnToIdleAfterSkill();
// 			}
// 		}
//
//
// 		protected override void _ABC_OnSpineGeneralAnimationEventWithString(DS_ActionBusArguGroup ds)
// 		{
//
// 		}
//
//
// #region 接口实现
//
// 		DisplacementSkillInfo I_SkillIsDisplacementSkill.RelatedDisplacementSkillInfo
// 		{
// 			get => _relatedDisplacementSkillInfo;
// 			set => _relatedDisplacementSkillInfo = value;
// 		}
//
// #endregion
//
// 		protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
// 		{
// 			if (oInfo != null)
// 			{
// 				if (oInfo.OccupationInfoConfigName.Equals(_sai_prepare.OccupationInfo.OccupationInfoConfigName,
// 					    StringComparison.OrdinalIgnoreCase) ||
// 				    oInfo.OccupationInfoConfigName.Equals(_sai_release.OccupationInfo.OccupationInfoConfigName,
// 					    StringComparison.OrdinalIgnoreCase) ||
// 				    oInfo.OccupationInfoConfigName.Equals(_sai_prepareLoop.OccupationInfo.OccupationInfoConfigName,
// 					    StringComparison.OrdinalIgnoreCase) || oInfo.OccupationInfoConfigName.Equals(
// 					    _sai_DashEnd.OccupationInfo.OccupationInfoConfigName,
// 					    StringComparison.OrdinalIgnoreCase))
// 				{
// 					return true;
// 				}
// 			}
// 			return false;
// 		}
// 		// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupyFromInfo)
// 		// {
// 		// 	var fixedType = GetFixedOccupiedCancelType(occupyFromInfo);
// 		// 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupyFromInfo);
// 		// 	switch (_selfSkillState)
// 		// 	{
// 		// 		case SkillStateType.None_无事发生:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					_selfSkillState = SkillStateType.None_无事发生;
// 		// 					Clear_FullClearAllSkillContentImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		// 		case SkillStateType.Prepare_准备中:
// 		// 		case SkillStateType.Holding_等待释放:
// 		// 		case SkillStateType.Releasing_释放中:
// 		// 		case SkillStateType.Ending_结束后摇:
// 		// 			switch (fixedType)
// 		// 			{
// 		// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
// 		// 					if (!isSelf)
// 		// 					{
// 		// 						_selfSkillState = SkillStateType.None_无事发生;
// 		// 						Clear_PartialClearNotImmediate();
// 		// 					}
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
// 		// 					_selfSkillState = SkillStateType.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
// 		// 					_selfSkillState = SkillStateType.None_无事发生;
// 		// 					Clear_FullClearAllSkillContentImmediate();
// 		// 					break;
// 		// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
// 		// 					_selfSkillState = SkillStateType.None_无事发生;
// 		// 					Clear_PartialClearNotImmediate();
// 		// 					break;
// 		// 				default:
// 		// 					throw new ArgumentOutOfRangeException();
// 		// 			}
// 		// 			break;
// 		//
// 		// 		default:
// 		// 			throw new ArgumentOutOfRangeException();
// 		// 	}
// 		// }
// 		public override void Clear_FullClearAllSkillContentImmediate()
// 		{
// 			_relatedProjectileLayoutRuntimeInstance.StopLayout();
// 			(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
// 			VFX_GeneralClear(true);
// 		}
// 		public override void Clear_PartialClearNotImmediate()
// 		{
// 			_relatedProjectileLayoutRuntimeInstance.StopLayout();
// 			(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
// 			VFX_GeneralClear();
// 		}
//
// 		
//
//
// 		public override DS_ActionBusArguGroup ClearBeforeRemove()
// 		{
// 			var ds = base.ClearBeforeRemove();
// 			ClearVFXObject();
//
//
// 			if (_relatedDisplacementSkillInfo != null)
// 			{
// 				if (_relatedDisplacementSkillInfo.SelfSegmentList != null)
// 				{
// 					foreach (DisplacementSkillInfo.DisplacementSkillInfo_Segment perSegment in
// 						_relatedDisplacementSkillInfo.SelfSegmentList)
// 					{
// 						GenericPool<DisplacementSkillInfo.DisplacementSkillInfo_Segment>.Release(perSegment);
// 					}
//
// 					CollectionPool<List<DisplacementSkillInfo.DisplacementSkillInfo_Segment>,
// 							DisplacementSkillInfo.DisplacementSkillInfo_Segment>
// 						.Release(_relatedDisplacementSkillInfo.SelfSegmentList);
// 				}
//
// 				GenericPool<DisplacementSkillInfo>.Release(_relatedDisplacementSkillInfo);
// 			}
//
//
// 			return ds;
// 		}
//
//
// 		/// <summary>
// 		/// <para>清理所有还在运转的特效</para>
// 		/// </summary>
// 		private void ClearVFXObject()
// 		{
// 			//VFX_GeneralClear();
//
// 			if (_indicator_Line_Instance != null)
// 			{
// 				Object.Destroy(_indicator_Line_Instance.gameObject);
// 			}
//
// 			if (_indicator_OuterRange_Instance != null)
// 			{
// 				Object.Destroy(_indicator_OuterRange_Instance.gameObject);
// 			}
// 		}
// 		public override Sprite GetCurrentSprite(DamageTypeEnum @override = DamageTypeEnum.None)
// 		{
// 			DamageTypeEnum fd = DamageTypeEnum.None;
//
// 			if (@override != DamageTypeEnum.None)
// 			{
// 				fd = @override;
// 			}
// 			else
// 			{
// 				fd = (RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
// 					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType).CurrentDamageType;
// 			}
// 			switch (fd)
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