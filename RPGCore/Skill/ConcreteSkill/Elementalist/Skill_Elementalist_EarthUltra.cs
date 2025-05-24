using System;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Player;
using ARPG.Manager;
using GameplayEvent;
using GameplayEvent.SO;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	// [Serializable]
	// public class Skill_Elementalist_EarthUltraBaseRPSkill : BaseRPSkill
	// {
	//
	//
	// 	private AnimationInfoBase _sai_prepare;
	//
	// 	private AnimationInfoBase _sai_release;
	//
	//
	// 	[SerializeField, LabelText("作用范围——覆写版面和伴生尺寸"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered)]
	// 	private float _overrideRangeSize = 5f;
	// 	
	// 	
	//
	//
	// 	[SerializeField, LabelText("an_结束动作"), FoldoutGroup("配置", true),
	// 	 TitleGroup("配置/动画", Alignment = TitleAlignments.Centered), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
	// 	private string _an_end;
	// 	private AnimationInfoBase _sai_ending;
	// 	protected enum SkillStateTypeEnum
	// 	{
	// 		None_无事发生= 0,
	// 		Preparing_准备中 = 1,
	// 		ReleasingAndTakeEffect_施法生效中 = 2,
	// 		
	// 	}
	//
	// 	protected SkillStateTypeEnum _selfSkillState;
	//
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
	// 		_sai_ending = GetAnimationInfoByConfigName(_an_end);
	//
	// 		foreach (var r in AnimationEventCallbacks)
	// 		{
	// 			switch (r)
	// 			{
	// 				case PAEC_生成版面_SpawnLayout paec生成版面SpawnLayout:
	// 					paec生成版面SpawnLayout.RelatedConfig.LayoutContentInSO.RelatedProjectileScale = _overrideRangeSize;
	// 					break;
	// 			}
	// 		}
	// 	}
	//
	// 	protected void _IC_ReceiveFireInput_(InputAction.CallbackContext context)
	// 	{
	// 		
	// 	}
	//
	// 	protected void _IC_ReceiveSkillInput_NormalPerformed(InputAction.CallbackContext callback)
	// 	{
	//
	// 		//不是自己，无事发生
	// 		if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
	// 		{
	// 			return;
	// 		}
	//
	// 		OnSkillSlotReceiveInput();
	// 		switch (_selfSkillState)
	// 		{
	// 			case SkillStateTypeEnum.None_无事发生:
	// 				if (!IfReactToInput())
	// 				{
	// 					return;
	// 				}
	// 				if (!CheckIfDataEntryEnough())
	// 				{
	// 					return;
	// 				}
	//
	// 				if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_release.OccupationInfo))
	// 				{
	// 					return;
	// 				}
	//
	//
	// 				//到这里了，开始准备
	// 				OnSkillBeginPrepare();
	// 				break;
	// 			case SkillStateTypeEnum.Preparing_准备中:
	// 				break;
	// 			case SkillStateTypeEnum.ReleasingAndTakeEffect_施法生效中:
	// 				break;
	// 			default:
	// 				throw new ArgumentOutOfRangeException();
	// 		}
	// 		
	// 	}
	// 	protected override void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
	// 	{
	// 		base._ABC_OnGeneralAnimationComplete(ds);
	// 		var configName = ds.ObjectArguStr as string;
	// 		//准备完成后自动释放
	// 		if (configName.Equals(_sai_prepare.ConfigName, StringComparison.OrdinalIgnoreCase))
	// 		{
	// 			OnSkillFirstNewRelease();
	// 		}
	// 		else if (configName.Equals(_sai_release.ConfigName, StringComparison.OrdinalIgnoreCase))
	// 		{
	// 			_selfSkillState = SkillStateTypeEnum.ReleasingAndTakeEffect_施法生效中;
	// 			_Internal_GeneralRequireAnimationEvent(_sai_ending);
	// 			_selfSkillState = SkillStateTypeEnum.None_无事发生;
	// 		}
	// 		else if(configName.Equals( _sai_ending.ConfigName, StringComparison.OrdinalIgnoreCase))
	// 		{
	// 			_selfSkillState = SkillStateTypeEnum.None_无事发生;
	// 			// _Internal_BroadcastSkillReleaseFinish();
	// 			Clear_PartialClearNotImmediate();
	// 		}
	// 	}
	//
	//
	// 	protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
	// 	{
	// 		if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_prepare.OccupationInfo))
	// 		{
	// 			DBug.LogWarning($"技能土超杀 准备 占用失败了，来自占用信息{_sai_prepare.OccupationInfo.OccupationInfoConfigName}");
	// 		}
	// 		base.OnSkillBeginPrepare(autoLaunch);
	//
	//
	// 	
	// 		OnSkillResetCoolDown();
	// 		OnSkillConsumeSP();
	//
	// 		_Internal_GeneralRequireAnimationEvent(_sai_prepare, true);
	//
	// 		
	// 		return base.OnSkillBeginPrepare(autoLaunch);
	// 	}
	//
	//
	// 	/// <summary>
	// 	/// <para>开始释放。会开始播那个release动画，</para>
	// 	/// </summary>
	// 	/// <param name="autoLaunch"></param>
	// 	/// <returns></returns>
	// 	protected override DS_ActionBusArguGroup OnSkillFirstNewRelease(bool autoLaunch = true)
	// 	{
	// 		var ds = base.OnSkillFirstNewRelease(autoLaunch);
	// 		if (!_characterBehaviourRef.TryOccupyByOccupationInfo(_sai_release.OccupationInfo))
	// 		{
	// 			DBug.LogWarning($"技能土U  释放占用  失败，占用信息{_sai_release.OccupationInfo}");
	// 		}
	// 		
	// 		
	// 		_selfSkillState = SkillStateTypeEnum.Preparing_准备中;
	//
	// 		
	// 		//生成那个能量汇聚的特效
	//
	//
	//
	// 		_Internal_GeneralRequireAnimationEvent(_sai_release, true);
	//
	// 		return ds;
	// 	}
	//
	//
	// 	
	// 	
	//
	// 	
	// 	public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
	// 	{
	// 		base.FixedUpdateTick(currentTime, currentFrameCount, delta);
	// 		switch (_selfSkillState)
	// 		{
	// 			case SkillStateTypeEnum.Preparing_准备中:
	// 				(this as  I_SkillNeedShowProgress).ShowProgressTick(currentTime,currentFrameCount,delta);
	// 				break;
	// 		}
	// 		
	// 	}
	//
	//
	//
	//
	// 	protected override bool IfSkillCanCDTick()
	// 	{
	// 		if (_selfSkillState == SkillStateTypeEnum.None_无事发生)
	// 		{
	// 			return true;
	// 		}
	// 		else
	// 		{
	// 			return false;
	// 		}
	// 	}
	// 	protected override bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
	// 	{
	// 		var str = oInfo.OccupationInfoConfigName;
	// 		if (str.Equals(_sai_release.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase) ||
	// 		    str.Equals(_sai_prepare.OccupationInfo.OccupationInfoConfigName, StringComparison.OrdinalIgnoreCase))
	// 		{
	// 			return true;
	// 		}
	//
	// 		return false;
	// 	}
	// 	// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupySourceInfo)
	// 	// {
	// 	// 	var fixedType = GetFixedOccupiedCancelType(occupySourceInfo);
	// 	// 	var isSelf = _Internal_CheckIfBreakAnimationIsSelf(occupySourceInfo);
	// 	// 	switch (_selfSkillState)
	// 	// 	{
	// 	// 		case SkillStateTypeEnum.None_无事发生:
	// 	// 			break;
	// 	// 		case SkillStateTypeEnum.Preparing_准备中:
	// 	// 			switch (fixedType)
	// 	// 			{
	// 	// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
	// 	// 					if (!isSelf)
	// 	// 					{
	// 	// 						C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 						return;
	// 	// 					}
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
	// 	// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
	// 	// 					C_Debug_LogOccupyNotImplementInfo(occupySourceInfo);
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
	// 	// 					break;
	// 	// 				default:
	// 	// 					throw new ArgumentOutOfRangeException();
	// 	// 			}
	// 	// 			break;
	// 	// 		case SkillStateTypeEnum.ReleasingAndTakeEffect_施法生效中:
	// 	// 			switch (fixedType)
	// 	// 			{
	// 	// 				case FixedOccupiedCancelTypeEnum.NonFixedType_非固定预设类型:
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Unbalance_失衡:
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Switch_切换角色:
	// 	// 					_selfSkillState = SkillStateTypeEnum.None_无事发生;
	// 	// 					Clear_PartialClearNotImmediate();
	// 	// 					break;
	// 	// 				case FixedOccupiedCancelTypeEnum.Dead_死亡:
	// 	// 					break;
	// 	// 				default:
	// 	// 					throw new ArgumentOutOfRangeException();
	// 	// 			}
	// 	// 			break;
	// 	// 		default:
	// 	// 			throw new ArgumentOutOfRangeException();
	// 	// 	}
	// 	// }
	// 	public override void Clear_FullClearAllSkillContentImmediate()
	// 	{
	// 		VFX_GeneralClear(true);
	// 		
	// 	}
	// 	public override void Clear_PartialClearNotImmediate()
	// 	{
	// 		VFX_GeneralClear();
	// 	}
	// 	public AnimationInfoBase ActAsSheetAnimationInfo => _sai_prepare;
	// 	BaseRPSkill I_SkillNeedShowProgress._selfSkillRef_Interface => this;
	// }
}