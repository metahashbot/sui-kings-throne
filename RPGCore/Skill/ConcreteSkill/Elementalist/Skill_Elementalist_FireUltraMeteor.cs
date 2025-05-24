using System;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Manager;
using GameplayEvent;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.Skill.SkillSelector;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	// [Serializable]
	// public class Skill_Elementalist_FireUltraMeteor : BaseRPSkill , I_SkillContainStoicToStiffness , I_SkillNeedShowProgress ,
	// 	I_SkillContainResistToAbnormal
	// {
	// 	private AnimationInfoBase _sai_prepare;
	//
	// 	private AnimationInfoBase _sai_release;
	//
	//
	// 	[SerializeField, LabelText("an_holding动作"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
	// 	private string _an_holding;
	//
	// 	private AnimationInfoBase _sai_holding;
	//
	//
	// 	[SerializeField, LabelText("an_结束动作"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
	// 	private string _an_end;
	// 	private AnimationInfoBase _sai_ending;
	//
	//
	//
	// 	[SerializeField, LabelText("释放过程上升高度(曲线Y)"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered)]
	// 	private float _releaseCharacterLiftHeight;
	//
	// 	[SerializeField, LabelText("释放过程 上升整体时间(曲线X)"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered)]
	// 	private float _release_CharacterLiftDuration;
	// 	
	// 	[SerializeField,LabelText("释放过程—上升过程曲线"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered)]
	// 	private AnimationCurve _release_CharacterLiftCurve;
	// 	
	// 	
	//
	//
	//
	// 	[SerializeField, LabelText("指示器_选点大范围"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered)]
	// 	private GameObject _indicatorPrefab_OuterRange;
	//
	// 	private SkillIndicator_Range _indicatorObject_OuterRange;
	//
	// 	[SerializeField, LabelText("指示器_选点位置"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered)]
	// 	private GameObject _indicatorPrefab_InnerPosition;
	//
	// 	private SkillIndicator_Range _indicatorObject_InnerPosition;
	//
	//
	// 	[SerializeField, LabelText("vfx_施法特效"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
	// 	private string _vfx_prepare;
	//
	// 	private PerVFXInfo _vfxInfo_Prepare;
	//
	// 	[SerializeField, LabelText("vfx_释放(腾空)"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
	// 	private string _vfx_release;
	//
	// 	private PerVFXInfo _vfxInfo_Release;
	//
	// 	[SerializeField, LabelText("vfx_判定(火球爆炸)"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
	// 	private string _vfx_takeEffect;
	// 	[SerializeField, LabelText("vfx_判定(火球爆炸)相较于效果半径的乘数"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered)]
	// 	protected float _vfxTakeEffectSizeMulToEffectRadius = 1f;
	// 	[SerializeField, LabelText("vfx_当火球距离地面多高的时候播放判定特效"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered)]
	// 	protected float _vfx_takeEffectHeight = 0.5f;
	//
	// 	protected bool _needToPlayTakeEffectHeight;
	//
	// 	[SerializeField, LabelText("LC_Drop覆写"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
	// 	private LC_Drop _lcDropOverride;
	//
	// 	[SerializeField, LabelText("伤害覆写"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
	// 	private ConSer_DamageApplyInfo _damageApplyOverride;
	//
	//
	// 	[SerializeField, LabelText("版面生成延迟_以曲线开始采样为始"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
	// 	protected float _delay_layoutSpawn = 4.9f;
	//
	// 	[SerializeField, LabelText("最大施法距离半径"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
	// 	public float CastRangeRadius = 10f;
	// 	
	// 	[SerializeField,LabelText("效果半径-也是投射物尺寸"),FoldoutGroup("配置",true),
	// 	TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
	// 	public float EffectRadius = 10f;
	// 	
	// 	
	// 	
	// 	[SerializeField, LabelText("需要触发的游戏事件——开始准备"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/事件", Alignment = TitleAlignments.Centered)]
	// 	protected SOConfig_PrefabEventConfig _ge_OnPrepare;
	// 	[SerializeField, LabelText("需要触发的游戏事件——确定释放"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/事件", Alignment = TitleAlignments.Centered)]
	// 	protected SOConfig_PrefabEventConfig _ge_OnRelease;
	//
	//
	//
	// 	/// <summary>
	// 	/// <para>记录的将要生成的位置</para>
	// 	/// </summary>
	// 	protected Vector3 _registeredSpawnPosition;
	//
	// 	
	//
	// 	
	// 	
	//
	// 	private enum SkillStateTypeEnum
	// 	{
	// 		None_无事发生 = 0,
	// 		Preparing_施法前摇 = 1,
	// 		Holding_等待选点 =2,
	// 		/// <summary>
	// 		/// 正在施法就是飞上去再下来的那一段
	// 		/// </summary>
	// 		Releasing_正在施法 = 3,
	// 		Ending_施法后摇 =4,
	// 	}
	//
	//
	// 	private SkillStateTypeEnum _skillState = SkillStateTypeEnum.None_无事发生;
	//
	//
	// 	/// <summary>
	// 	/// <para>将会结束Releasing过程，转向Ending过程的时间点</para>
	// 	/// </summary>
	// 	private float _willEndReleaseTime;
	//
	// 	/// <summary>
	// 	/// <para>上升过程的起始时间点</para>
	// 	/// </summary>
	// 	private float _liftStartTime;
	//
	//
	// 	
	// 	/// <summary>
	// 	/// 上升过程的起始位置
	// 	/// </summary>
	// 	private Vector3 _liftFromPosition;
	//
	//
	// 	private SOConfig_ProjectileLayout _layoutRuntimeRef;
	//
	//
	// 	public override void InitOnObtain(
	// 		RPSkill_SkillHolder skillHolderRef,
	// 		SOConfig_RPSkill configRuntimeInstance,
	// 		I_RP_ObjectCanReleaseSkill parent,
	// 		SkillSlotTypeEnum slot)
	// 	{
	// 		base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
	// 		// _sai_prepare =
	// 		// 	GetAnimationInfoByConfigName(configRuntimeInstance.ContentInSO._AN_SkillPrepareSpineAnimationName);
	// 		// _sai_release =
	// 		// 	GetAnimationInfoByConfigName(configRuntimeInstance.ContentInSO._AN_SkillReleaseSpineAnimationName);
	// 		_sai_holding = GetAnimationInfoByConfigName(_an_holding);
	// 		_sai_ending = GetAnimationInfoByConfigName(_an_end);
	// 	}
	//
	//
	// 	protected override void BindingInput()
	// 	{
	// 		if (SkillSlot == SkillSlotTypeEnum.None_未装备)
	// 		{
	// 			return;
	// 		}
	// 		var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
	// 		var ia = GetTargetInputActionRef(SkillSlot);
	// 		ia.performed += _IC_ReceiveSkillInput_NormalPerformed;
	// 		iar.BattleGeneral.FireBase.performed += _IC_ReceiveFireInput_;
	// 	}
	// 	
	// 	
	// 	
	// 	protected override void UnbindInput()
	// 	{
	// 		if (SkillSlot == SkillSlotTypeEnum.None_未装备)
	// 		{
	// 			return;
	// 		}
	//
	// 		InputAction ia = GetTargetInputActionRef(SkillSlot);
	// 		ia.performed -= _IC_ReceiveSkillInput_NormalPerformed;
	// 		var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
	// 		iar.BattleGeneral.FireBase.performed -= _IC_ReceiveFireInput_;
	// 	}
	//
	// 	protected virtual void _IC_ReceiveSkillInput_NormalPerformed(InputAction.CallbackContext context)
	// 	{
	// 		//不是自己，无事发生
	// 		if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
	// 		{
	// 			return;
	// 		}
	//
	// 		OnSkillSlotReceiveInput();
	// 		switch (_skillState)
	// 		{
	// 			//试试能不能前摇
	// 			case SkillStateTypeEnum.None_无事发生:
	// 				if (!IfReactToInput())
	// 				{
	// 					return;
	// 				}
	// 				if (!CheckIfDataEntryEnough())
	// 				{
	// 					return;
	// 				}
	// 				if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_release.OccupationInfo))
	// 				{
	// 					return;
	// 				}
	//
	// 				//到这里了，开始准备
	// 				OnSkillBeginPrepare();
	//
	// 				break;
	// 			case SkillStateTypeEnum.Preparing_施法前摇:
	// 				break;
	// 			case SkillStateTypeEnum.Holding_等待选点:
	// 				break;
	// 			case SkillStateTypeEnum.Releasing_正在施法:
	// 				break;
	// 			case SkillStateTypeEnum.Ending_施法后摇:
	// 				break;
	// 		}
	// 	}
	//
	//
	// 	protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
	// 	{
	// 		if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_prepare.OccupationInfo))
	// 		{
	// 			DBug.LogWarning($"技能水超杀 准备 占用失败了，来自占用信息{_sai_prepare.OccupationInfo.OccupationInfoConfigName}");
	// 		}
	// 		
	// 		var ds= base.OnSkillBeginPrepare(autoLaunch);
	//
	// 		(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
	// 		(this as I_SkillContainResistToAbnormal).AddResist_SkillVersion();
	// 		_PrepareIndicator();
	// 		OnSkillResetCoolDown();
	// 		OnSkillConsumeSP();
	//
	//
	// 		_Internal_GeneralRequireAnimationEvent(_sai_prepare, true);
	// 		
	// 		if (_ge_OnPrepare != null)
	// 		{
	// 			GameplayEventManager.Instance.StartGameplayEvent(_ge_OnPrepare);
	// 		}
	//
	//
	// 		_vfxInfo_Prepare = _VFX_GetOrInstantiateNew(_vfx_prepare)._VFX__10_PlayThis();
	//
	// 		
	// 		
	// 		
	// 		
	// 		return ds;
	// 	}
	//
	// 	protected virtual void _IC_ReceiveFireInput_(InputAction.CallbackContext context)
	// 	{
	// 		if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
	// 		{
	// 			return;
	// 		}
	// 		if (_skillState != SkillStateTypeEnum.Holding_等待选点)
	// 		{
	// 			return;
	// 		}
	//
	//
	// 		//只有等待选点的时候才可以释放，然后选点释放
	// 		OnSkillFirstNewRelease();
	// 	}
	//
	// 	protected override bool IfSkillCanCDTick()
	// 	{
	// 		if (_skillState == SkillStateTypeEnum.None_无事发生)
	// 		{
	// 			return true;
	// 		}
	// 		else
	// 		{
	// 			return false;
	// 		}
	// 	}
	//
	//
	// 	public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
	// 	{
	// 		base.UpdateTick(currentTime, currentFrameCount, delta);
	// 		switch (_skillState)
	// 		{
	// 			case SkillStateTypeEnum.Preparing_施法前摇:
	// 				break;
	// 			case SkillStateTypeEnum.Holding_等待选点:
	// 				UpdateIndicator();
	// 				break;
	// 		}
	// 	}
	//
	// 	public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
	// 	{
	// 		base.FixedUpdateTick(currentTime, currentFrameCount, delta);
	// 		
	// 		switch (_skillState)
	// 		{
	// 			case SkillStateTypeEnum.Preparing_施法前摇:
	// 			case SkillStateTypeEnum.Holding_等待选点:
	// 				break;
	// 			case SkillStateTypeEnum.Releasing_正在施法:
	// 				var acce = _characterBehaviourRef.GetFloatDataEntryByType(
	// 					RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速,
	// 					true);
	// 				float progressDuration = _delay_layoutSpawn;
	//
	// 				if (acce != null)
	// 				{
	// 					progressDuration = _delay_layoutSpawn / (1f + acce.CurrentValue / 100f);
	// 				}
	// 				
	// 				
	// 				
	// 				
	// 				(this as I_SkillNeedShowProgress).ShowProgressTick(currentTime, currentFrameCount, delta,
	// 					(currentTime - _liftStartTime) / (progressDuration) );
	// 				//采样过程曲线，实现一个“角色上升”过程
	// 				float eva = _release_CharacterLiftCurve.Evaluate((currentTime - _liftStartTime) /
	// 				                                                 _release_CharacterLiftDuration);
	// 				float height = _releaseCharacterLiftHeight * eva;
	//
	// 				var artHelper = _characterBehaviourRef.GetSelfRolePlayArtHelper();
	// 				var rotAnchor = artHelper._rotateAnchor;
	// 				Vector3 currentCharacterUpDirection = rotAnchor.InverseTransformDirection(rotAnchor.up);
	// 				Vector3 offset = currentCharacterUpDirection * height;
	// 				artHelper.transform.localPosition = offset;
	// 				
	// 				
	//
	//
	// 				if (currentTime > _willEndReleaseTime) 
	// 				{
	// 					_skillState = SkillStateTypeEnum.None_无事发生;
	// 					_Internal_GeneralRequireAnimationEvent(_sai_ending);
	//
	// 				}
	// 				break;
	// 		}
	// 		if (_needToPlayTakeEffectHeight)
	// 		{
	// 			if (_layoutRuntimeRef.LayoutHandlerFunction.GetProjectileBehaviourCollectionAtSeriesAndShoots(0, 0) != null &&
	// 			    _layoutRuntimeRef.LayoutHandlerFunction.GetProjectileBehaviourCollectionAtSeriesAndShoots(0, 0).Count > 0)
	// 			{
	// 				var projectileRuntime = _layoutRuntimeRef.LayoutHandlerFunction
	// 					.GetProjectileBehaviourCollectionAtSeriesAndShoots(0, 0)[0];
	// 				var heightDiff = projectileRuntime.RelatedGORef.transform.position.y - _registeredSpawnPosition.y;
	// 				if (heightDiff < _vfx_takeEffectHeight)
	// 				{
	// 					_skillState = SkillStateTypeEnum.None_无事发生;
	// 					_needToPlayTakeEffectHeight = false;
	// 					_VFX_GetOrInstantiateNew(_vfx_takeEffect)
	// 						._VFX_2_SetPositionToGlobalPosition(_registeredSpawnPosition)
	// 						.VFX_4_SetLocalScale(_vfxTakeEffectSizeMulToEffectRadius * EffectRadius)
	// 						._VFX__10_PlayThis();
	// 				}
	// 				
	// 			}
	// 		}
	// 	}
	//
	// 	private void _PrepareIndicator()
	// 	{
	// 		if (_indicatorPrefab_OuterRange != null)
	// 		{
	// 			if (_indicatorObject_OuterRange == null)
	// 			{
	// 				_indicatorObject_OuterRange = UnityEngine.Object.Instantiate(_indicatorPrefab_InnerPosition)
	// 					.GetComponent<SkillIndicator_Range>();
	// 			}
	// 			_indicatorObject_OuterRange.gameObject.SetActive(true);
	// 			_indicatorObject_OuterRange.SetRadius(CastRangeRadius);
	// 			_indicatorObject_OuterRange.SetFromPosition(_characterBehaviourRef.transform.position);
	// 		}
	// 		if (_indicatorPrefab_InnerPosition != null)
	// 		{
	// 			if (_indicatorObject_InnerPosition == null)
	// 			{
	// 				_indicatorObject_InnerPosition = UnityEngine.Object.Instantiate(_indicatorPrefab_InnerPosition)
	// 					.GetComponent<SkillIndicator_Range>();
	// 			}
	// 			_indicatorObject_InnerPosition.gameObject.SetActive(true);
	// 			_indicatorObject_InnerPosition.SetRadius(EffectRadius);
	// 			_indicatorObject_InnerPosition.SetFromPosition(_characterBehaviourRef.transform.position);
	// 		}
	// 	}
	// 	private void UpdateIndicator()
	// 	{
	// 		if (_indicatorObject_InnerPosition != null && _indicatorObject_InnerPosition.gameObject.activeInHierarchy)
	// 		{
	// 			Vector3 indicatorPos = GetValidSpawnPosition();
	// 			_indicatorObject_InnerPosition.SetFromPosition(indicatorPos);
	// 		}
	// 	}
	//
	// 	private Vector3 GetValidSpawnPosition()
	// 	{
	// 		var currentInputPosition = _playerControllerRef.InputResult_InputLogicPosition;
	// 		var distance = Vector3.Distance(currentInputPosition, _characterBehaviourRef.transform.position);
	// 		Vector3 indicatorPos = currentInputPosition;
	// 		if (distance > CastRangeRadius)
	// 		{
	// 			var currentSelfPosY0 = _characterBehaviourRef.transform.position;
	// 			currentSelfPosY0.y = 0f;
	// 			var input_y0 = _playerControllerRef.InputResult_InputLogicPosition;
	// 			input_y0.y = 0f;
	// 			var dir = (input_y0 - currentSelfPosY0).normalized;
	// 			var clampPos = _characterBehaviourRef.transform.position + dir * CastRangeRadius;
	// 			indicatorPos = clampPos;
	// 		}
	// 		indicatorPos = SubGameplayLogicManager_ARPG.Instance.GetAlignedTerrainPosition(indicatorPos) ?? indicatorPos;
	// 		return indicatorPos;
	// 	}
	//
	//
	// 	protected override DS_ActionBusArguGroup OnSkillFirstNewRelease(bool autoLaunch = true)
	// 	{
	// 		var ds = base.OnSkillFirstNewRelease(autoLaunch);
	// 		(this as I_SkillContainStoicToStiffness).StartStiffness_SkillVersion();
	// 		(this as I_SkillContainResistToAbnormal).AddResist_SkillVersion();
	// 		if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_release.OccupationInfo))
	// 		{
	// 			DBug.LogWarning($"技能火U  释放占用  失败，占用信息{_sai_release.OccupationInfo}");
	// 		}
	// 		DeactivateIndicator();
	// 		_skillState = SkillStateTypeEnum.Releasing_正在施法;
	// 		_vfxInfo_Prepare.VFX_StopThis();
	//
	//
	// 		if (_ge_OnRelease != null)
	// 		{
	// 			GameplayEventManager.Instance.StartGameplayEvent(_ge_OnRelease);
	// 		}
	//
	// 		_registeredSpawnPosition = GetValidSpawnPosition();
	// 		//生成那个能量汇聚的特效
	// 		_vfxInfo_Release = _VFX_GetOrInstantiateNew(_vfx_release)._VFX__10_PlayThis();
	// 		_willEndReleaseTime = BaseGameReferenceService.CurrentFixedTime + _release_CharacterLiftDuration;
	// 		_liftStartTime = BaseGameReferenceService.CurrentFixedTime;
	//
	// 		// var layoutRef = SelfSkillConfigRuntimeInstance.ContentInSO.DefaultProjectileLayout;
	// 		// layoutRef.LayoutContentInSO.DamageApplyInfo = _damageApplyOverride;
	// 		// var lc_dropIndex =
	// 		// 	layoutRef.LayoutContentInSO.LayoutComponentList.FindIndex(((component => component is LC_Drop))) ;
	// 		// layoutRef.LayoutContentInSO.LayoutComponentList[lc_dropIndex] = _lcDropOverride;
	// 		//
	// 		// layoutRef.LayoutContentInSO.RelatedProjectileScale = EffectRadius;
	// 		// layoutRef.LayoutContentInSO.OverallStartDelay = _delay_layoutSpawn;
	// 		// _layoutRuntimeRef = layoutRef.SpawnLayout_NoAutoStart(_characterBehaviourRef);
	// 		// _layoutRuntimeRef.LayoutHandlerFunction.OverrideSpawnFromPosition = _registeredSpawnPosition;
	// 		// _layoutRuntimeRef.LayoutHandlerFunction.StartLayout();
	// 		// _needToPlayTakeEffectHeight = true;
	//
	//
	//
	// 		_Internal_GeneralRequireAnimationEvent(_sai_release, true);
	// 		
	// 		
	// 		
	//
	// 		return ds;
	// 	}
	//
	//
	//
	//
	// 	protected override void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
	// 	{
	// 		base._ABC_OnGeneralAnimationComplete(ds);
	// 		var configName = ds.ObjectArguStr as string;
	// 		//准备动作完成，将会开始播放holding循环
	// 		if (configName.Equals(_sai_prepare.ConfigName, StringComparison.OrdinalIgnoreCase))
	// 		{
	// 			if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_holding.OccupationInfo))
	// 			{
	// 				DBug.LogWarning($"技能火超杀 holding循环占用失败了，这不合理，占用信息{_sai_holding.OccupationInfo}");
	// 			}
	// 			_skillState = SkillStateTypeEnum.Holding_等待选点;
	// 			_Internal_GeneralRequireAnimationEvent(_sai_holding, true);
	// 			if (BaseGameReferenceService.CurrentActiveInput == CurrentActiveInputEnum.PureKeyboard)
	// 			{
	// 				//只有等待选点的时候才可以释放，然后选点释放
	// 				OnSkillFirstNewRelease();
	// 			}
	//
	// 		}
	// 		else if (configName.Equals(_sai_ending.ConfigName, StringComparison.OrdinalIgnoreCase))
	// 		{
	// 			// _Internal_BroadcastSkillReleaseFinish();
	// 			(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
	// 			(this as I_SkillContainResistToAbnormal).RemoveResist_SkillVersion();
	// 		}
	// 	}
	// 	// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupySourceInfo)
	// 	// {
	// 	// 	var fixedType = GetFixedOccupiedCancelType(occupySourceInfo);
	// 	// 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupySourceInfo);
	// 	// 	switch (_skillState)
	// 	// 	{
	// 	// 		case SkillStateTypeEnum.Preparing_施法前摇:
	// 	// 			switch (fixedType)
	// 	// 			{
	// 	// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					return;
	// 	// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					return;
	// 	// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
	// 	// 					_skillState = SkillStateTypeEnum.None_无事发生;
	// 	// 					Clear_PartialClearNotImmediate();
	// 	// 					return;
	// 	// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					return;
	// 	// 			}
	// 	// 			break;
	// 	// 		case SkillStateTypeEnum.Holding_等待选点:
	// 	// 			switch (fixedType)
	// 	// 			{
	// 	// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
	// 	// 					_skillState = SkillStateTypeEnum.None_无事发生;
	// 	// 					Clear_PartialClearNotImmediate();
	// 	// 					break;
	// 	// 				default:
	// 	// 					throw new ArgumentOutOfRangeException();
	// 	// 			}
	// 	// 			break;
	// 	// 		case SkillStateTypeEnum.Releasing_正在施法:
	// 	// 			switch (fixedType)
	// 	// 			{
	// 	// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
	// 	// 					_skillState = SkillStateTypeEnum.None_无事发生;
	// 	// 					Clear_PartialClearNotImmediate();
	// 	// 					break;
	// 	// 				default:
	// 	// 					throw new ArgumentOutOfRangeException();
	// 	// 			}
	// 	// 			break;
	// 	// 		case SkillStateTypeEnum.Ending_施法后摇:
	// 	// 			switch (fixedType)
	// 	// 			{
	// 	// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
	// 	// 					if (!isSelf)
	// 	// 					{
	// 	// 						_skillState = SkillStateTypeEnum.None_无事发生;
	// 	// 						Clear_PartialClearNotImmediate();
	// 	// 					}
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
	// 	// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
	// 	// 					_skillState = SkillStateTypeEnum.None_无事发生;
	// 	// 					Clear_PartialClearNotImmediate();
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
	// 	// 					_skillState = SkillStateTypeEnum.None_无事发生;
	// 	// 					Clear_PartialClearNotImmediate();
	// 	// 					break;
	// 	// 				default:
	// 	// 					throw new ArgumentOutOfRangeException();
	// 	// 			}
	// 	// 			break;
	// 	// 	}
	// 	// }
	// 	public override void Clear_FullClearAllSkillContentImmediate()
	// 	{
	// 		(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
	// 		(this as I_SkillContainResistToAbnormal).RemoveResist_SkillVersion();
	// 		VFX_GeneralClear(true);
	// 		DeactivateIndicator();
	// 	}
	// 	public override void Clear_PartialClearNotImmediate()
	// 	{
	// 		(this as I_SkillContainStoicToStiffness).EndStiffness_SkillVersion();
	// 		 (this as I_SkillContainResistToAbnormal).RemoveResist_SkillVersion()
	// 		;
	// 		VFX_GeneralClear();
	// 		DeactivateIndicator();
	// 	}
	//
	// 	protected override void VFX_GeneralClear(bool immediateStop = false)
	// 	{
	// 		DeactivateIndicator();
	// 		base.VFX_GeneralClear(immediateStop);
	// 	}
	//
	// 	protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
	// 	{
	// 		var str = oInfo.OccupationInfoConfigName;
	// 		if (str.Equals(_sai_holding.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase) ||
	// 		    str.Equals(_sai_release.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase) ||
	// 		    str.Equals(_sai_prepare.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase))
	// 		{
	// 			return true;
	// 		}
	//
	// 		return false;
	// 	}
	//
	//
	//
	//
	// 	private void DeactivateIndicator()
	// 	{
	// 		if (_indicatorObject_InnerPosition != null)
	// 		{
	// 			_indicatorObject_InnerPosition.gameObject.SetActive(false);
	// 		}
	// 		if (_indicatorObject_OuterRange != null)
	// 		{
	// 			_indicatorObject_OuterRange.gameObject.SetActive(false);
	// 		}
	// 	}
	//
	// 	public AnimationInfoBase ActAsSheetAnimationInfo => _sai_release;
	// 	BaseRPSkill I_SkillNeedShowProgress._selfSkillRef_Interface => this;
	// }
}