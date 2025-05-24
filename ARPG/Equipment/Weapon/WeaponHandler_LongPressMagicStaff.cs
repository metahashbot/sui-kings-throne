using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Manager;
using GameplayEvent;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
namespace ARPG.Equipment
{
	[TypeInfoBox("长按连发的法杖类武器")]
	[Serializable]
	public class WeaponHandler_LongPressMagicStaff : BaseWeaponHandler
	{

		// public enum LongPressAttackType
		// {
		// 	ShootOnPointer_在指针位置发射 = 0, ShootByDirection_根据方向从玩家发射 = 1,
		// }
		//
		//
		// [LabelText("在长按时的移动速度倍率，0就是不能动"), SerializeField, FoldoutGroup("配置", true)]
		// public float MoveSpeedMultiplierOnLongPress = 0f;
		//
		// [LabelText("长按每发发射间隔"), SerializeField, FoldoutGroup("配置", true)]
		// public float LongPressShootInterval = 1f;
		//
		// [SerializeReference, LabelText("使用的攻击方式"), FoldoutGroup("配置", true)]
		// public LongPressAttackType AttackType;
		//
		// [LabelText("打出一发之后多久没有有效的普攻输入会停止普攻"), FoldoutGroup("配置", true), SerializeField]
		// public float TimeDurationBeforeAutoRelease = 0.2f;
		//
		//
		//
		// [SerializeField, LabelText("动画配置名_普攻前摇"), FoldoutGroup("配置", true),
		//  GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		// public string _anc_NormalAttackBegin;
		//
		// protected AnimationInfoBase _sai_NormalAttackBegin;
		//
		// [SerializeField, LabelText("动画配置名_普攻后摇"), FoldoutGroup("配置", true),
		//  GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		// public string _anc_NormalAttackEnd;
		//
		// protected AnimationInfoBase _sai_normalAttackEnd;
		//
		// [SerializeField, LabelText("动画配置名_普攻循环"), FoldoutGroup("配置", true),
		//  GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		// public string _anc_NormalAttackLoop;
		//
		// [SerializeField,LabelText("√:发射从本体位置 || 口:发射从挂点位置")]
		// [FoldoutGroup("配置", true)]
		// public bool _shootFromSelfPosition = true;
		//
		//  [HideIf(nameof(_shootFromSelfPosition)), SerializeField, LabelText("    发射挂点配置名"), FoldoutGroup("配置", true),
		//   GUIColor(206f / 255f, 177f / 255f, 227f / 255f)]
		// public string _shootAnchorConfigName;
		//
		//
		//
		//
		// protected AnimationInfoBase _sai_normalAttackLoop;
		//
		//
		//
		// protected bool _inputHolding = false;
		//
		//
		//
		//
		// [LabelText("上一次发射的时间点"), ShowInInspector, FoldoutGroup("运行时", true)]
		// protected float _lastShootTime;
		// [LabelText("下一次可以发射的时间点"), ShowInInspector, FoldoutGroup("运行时", true)]
		// protected float _nextShootTime;
		//
		//
		// [LabelText("当前能够进行射击吗 - 需要在循环动画的时候"), ShowInInspector, FoldoutGroup("运行时", true)]
		// protected bool _currentAbleToShoot = false;
		//
		//
		//
		// [SerializeField,LabelText("确定执行射击时的触发事件"),FoldoutGroup("配置",true),
		//  GUIColor(255f / 255f, 248f / 255f, 10f / 255f)]
		//  private List<SOConfig_PrefabEventConfig> _list_OnShootTriggerEvent;
		//
		//
		//
		// public override void InitializeOnInstantiate(
		// 	PlayerARPGConcreteCharacterBehaviour behaviour,
		// 	LocalActionBus lab,
		// 	SOConfig_WeaponTemplate configRuntime)
		// {
		// 	base.InitializeOnInstantiate(behaviour, lab, configRuntime);
		// 	_currentAbleToShoot = false;
		// 	_sai_normalAttackEnd = GetAnimationInfoByConfigName(_anc_NormalAttackEnd);
		// 	_sai_normalAttackLoop = GetAnimationInfoByConfigName(_anc_NormalAttackLoop);
		// 	_sai_NormalAttackBegin = GetAnimationInfoByConfigName(_anc_NormalAttackBegin);
		// 	//任何技能的释放都会停止普攻
		// 	lab.RegisterAction(ActionBus_ActionTypeEnum.L_Skill_OnSkillBeginPrepare_技能开始进行准备,
		// 		_ABC_StopAttackInput_OnSkillPrepare);
		// 	GlobalActionBus.GetGlobalActionBus().RegisterAction(
		// 		ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
		// 		_ABC_ClearInputState_OnCurrentActiveCharacterChanged);
		// }
		//
		//
		// private void _ABC_StopAttackInput_OnSkillPrepare(DS_ActionBusArguGroup ds)
		// {
		// 	ResetInputState();
		// }
		//
		// /// <summary>
		// /// <para>清理输入状态，常见于技能准备时、换角色时</para>
		// /// </summary>
		// private void ResetInputState()
		// {
		// 	_inputHolding = false;
		// 	_currentAbleToShoot = false;
		// }
		//
		// private void _ABC_ClearInputState_OnCurrentActiveCharacterChanged(DS_ActionBusArguGroup ds)
		// {
		// 	_inputHolding = false;
		// 	_currentAbleToShoot = false;
		// 	var activePlayer = ds.ObjectArgu1 as PlayerARPGConcreteCharacterBehaviour;
		// 	if (activePlayer != RelatedCharacterBehaviourRef)
		// 	{
		// 		ResetInputState();
		// 	}
		// }
		//
		//
		// public override void OnOccupiedCanceledByOther(DS_OccupationInfo occupySourceInfo,
		// 	FixedOccupiedCancelTypeEnum cancelType = FixedOccupiedCancelTypeEnum.None_未指定)
		// {
		// 	//如果不是自己的打断，那么就停止普攻
		// 	var occupyFromStr = occupySourceInfo.OccupationInfoConfigName;
		// 	if (occupyFromStr.Equals(_anc_NormalAttackBegin, StringComparison.OrdinalIgnoreCase) ||
		// 	    occupyFromStr.Equals(_anc_NormalAttackEnd, StringComparison.OrdinalIgnoreCase) ||
		// 	    occupyFromStr.Equals(_anc_NormalAttackLoop, StringComparison.OrdinalIgnoreCase))
		// 	{
		// 		return;
		// 	}
		// 	else
		// 	{
		// 		ResetInputState();
		// 	}
		// 	base.OnOccupiedCanceledByOther(occupySourceInfo);
		// }
		// /// <summary>
		// /// <para>接到了普攻输入。如果当前处于正在攻击</para>
		// /// </summary>
		// /// <param name="context"></param>
		// public override void _IC_OnNormalAttackPerformed(InputAction.CallbackContext context)
		// {
		// 	if (!CurrentReactingToInput)
		// 	{
		// 		return;
		// 	}
		// 	
		// 	_inputHolding = true;
		// 	if (RelatedCharacterBehaviourRef == null)
		// 	{
		// 		UnityEngine.Object.Destroy(SelfConfigRuntimeInstance);
		// 		return;
		// 	}
		// 	if (RelatedCharacterBehaviourRef.TryOccupyByOccupationInfo(_sai_NormalAttackBegin.OccupationInfo))
		// 	{
		// 		BeginNewNormalAttack(BaseGameReferenceService.CurrentFixedTime, _anc_NormalAttackBegin);
		// 	}
		// }
		//
		// public override void _IC_OnNormalAttackInputCanceled(InputAction.CallbackContext context)
		// {
		// 	if (!CurrentReactingToInput)
		// 	{
		// 		return;
		// 	}
		// 	if (RelatedCharacterBehaviourRef == null)
		// 	{
		// 		UnityEngine.Object.Destroy(SelfConfigRuntimeInstance);
		// 		return;
		// 	}
		// 	base._IC_OnNormalAttackInputCanceled(context);
		// 	_inputHolding = false;
		// }
		//
		//
		// public override void FixedUpdateTick(float ct, int cf, float delta)
		// {
		// 	base.FixedUpdateTick(ct, cf, delta);
		// 	GetAndOffsetCurrentInputPositionAndDirection();
		// 	
		//
		// 	//如果当前正在普攻
		// 	//如果输入是取消的，则检查上一次输入时间，如果超过了一定时间，则自动结束普攻
		// 	if (!_inputHolding)
		// 	{
		// 		if (_lastShootTime + TimeDurationBeforeAutoRelease < ct && _currentAbleToShoot)
		// 		{
		// 			//自动结束普攻
		//
		//
		// 			EndNormalAttack();
		// 			return;
		// 		}
		// 		//没超过，则暂时还不放
		// 		else
		// 		{
		// 		}
		// 	}
		// 	//还按着呢
		// 	else
		// 	{
		// 		if (RelatedCharacterBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.UnbalanceMovement_失衡推拉) ==
		// 		    BuffAvailableType.Available_TimeInAndMeetRequirement ||
		// 		    RelatedCharacterBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Frozen_冻结_DJ) ==
		// 		    BuffAvailableType.Available_TimeInAndMeetRequirement ||
		// 		    RelatedCharacterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum.Dizzy_眩晕) ==
		// 		    BuffAvailableType.Available_TimeInAndMeetRequirement)
		// 		{
		// 			return;
		// 		}
		// 		
		// 		
		// 		
		// 		Vector2 movement = _playerControllerRef.InputDirect_InputMoveRaw;
		// 		
		// 		if (MoveSpeedMultiplierOnLongPress > 0f)
		// 		{
		// 			RelatedCharacterBehaviourRef._ProcessMovement(movement,
		// 				movement.normalized,
		// 				Time.fixedDeltaTime,
		// 				MoveSpeedMultiplierOnLongPress);
		// 		}
		//
		//
		// 		if (_currentAbleToShoot)
		// 		{
		// 			//始终朝着指向方向
		// 			if (Vector3.Dot(GameReferenceService_ARPG.CurrentBattleLogicRightDirection,
		// 				RecordedAttackDirection.Value) > 0f)
		// 			{
		// 				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(false);
		// 			}
		// 			else
		// 			{
		// 				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(true);
		// 			}
		//
		// 			//当前时间超过了 上次时间+射击间隔， 则可以再次射击了
		// 			if (BaseGameReferenceService.CurrentFixedTime > (_lastShootTime + LongPressShootInterval))
		// 			{
		// 				ProcessNewShoot();
		// 			}
		// 		}
		// 	}
		// }
		//
		//
		//
		//
		//
		//
		//
		// /// <summary>
		// /// <para>释放掉普攻了</para>
		// /// </summary>
		// private void EndNormalAttack()
		// {
		// 	_inputHolding = false;
		// 	_currentAbleToShoot = false;
		// 	_selfActionBusRef.TriggerActionByType(ActionBus_ActionTypeEnum.L_PCLogic_OnPlayerEndNormalAttack_玩家结束普攻,
		// 		new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PCLogic_OnPlayerEndNormalAttack_玩家结束普攻));
		// 	AnimationInfoBase _sai_end = GetAnimationInfoByConfigName(_anc_NormalAttackEnd);
		//
		// 	if (!RelatedCharacterBehaviourRef.TryOccupyByOccupationInfo(_sai_end.OccupationInfo))
		// 	{
		// 		DBug.LogWarning(
		// 			$"武器配置:{SelfConfigRuntimeInstance.name}的结束普攻的占用失败了，这可能并不合理，检查一下。配置信息{_sai_end.OccupationInfo}");
		// 	}
		// 	else
		// 	{
		// 		DS_ActionBusArguGroup ds_ani =
		// 			new DS_ActionBusArguGroup(_sai_end, AnimationPlayOptionsFlagTypeEnum.Default_缺省状态, false);
		// 		_selfActionBusRef.TriggerActionByType(ds_ani);
		// 	}
		// }
		//
		//
		// /// <summary>
		// /// <para>进行一次射击。</para>
		// /// </summary>
		// protected void ProcessNewShoot()
		// {
		// 	
		// 	_lastShootTime = BaseGameReferenceService.CurrentFixedTime;
		// 	SpawnRelatedLayout();
		//
		// }
		//
		//
		// protected void SpawnRelatedLayout()
		// {
		// 	if (_list_OnShootTriggerEvent != null && _list_OnShootTriggerEvent.Count > 0)
		// 	{
		// 		foreach (SOConfig_PrefabEventConfig perEventConfig in _list_OnShootTriggerEvent)
		// 		{
		// 			GameplayEventManager.Instance.StartGameplayEvent(perEventConfig);
		// 		}
		// 	}
		// 	if (SelfConfigRuntimeInstance.WeaponFunction.PresetAttackDirectionType ==
		// 	    WeaponAttackDirectionTypeEnum.PointerDirectionInstant_瞬时的输入方向 ||
		// 	    SelfConfigRuntimeInstance.WeaponFunction.PresetAttackDirectionType ==
		// 	    WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainInstant_瞬时的指针位置)
		// 	{
		// 		GetAndOffsetCurrentInputPositionAndDirection();
		// 	}
		// 	switch (AttackType)
		// 	{
		// 		case LongPressAttackType.ShootOnPointer_在指针位置发射:
		// 			break;
		// 		case LongPressAttackType.ShootByDirection_根据方向从玩家发射:
		// 			var runtimeInstance =
		// 				SelfConfigRuntimeInstance.WeaponFunction.DefaultWeaponProjectileLayout
		// 					.SpawnLayout_NoAutoStart(RelatedCharacterBehaviourRef);
		// 			if (_shootFromSelfPosition)
		// 			{
		// 				
		// 			runtimeInstance.LayoutHandlerFunction.OverrideSpawnFromPosition =
		// 				RelatedCharacterBehaviourRef.transform.position;
		// 			}
		// 			else
		// 			{
		// 				runtimeInstance.LayoutHandlerFunction.OverrideSpawnFromPosition =
		// 					(RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper() as I_RP_ContainVFXContainer)
		// 					.GetVFXHolderGlobalPosition(_shootAnchorConfigName).Item1;
		// 			}
		// 			runtimeInstance.LayoutHandlerFunction.OverrideSpawnFromDirection = RecordedAttackDirection.Value;
		// 			runtimeInstance.LayoutHandlerFunction.StartLayout();
		// 			break;
		// 	}
		// }
		//
		// /// <summary>
		// /// 在纯键盘环境下，会试图自动瞄准最近的敌人
		// /// </summary>
		// protected override void GetAndOffsetCurrentInputPositionAndDirection()
		// {
		// 	base.GetAndOffsetCurrentInputPositionAndDirection();
		// 	if (BaseGameReferenceService.CurrentActiveInput == CurrentActiveInputEnum.PureKeyboard)
		// 	{
		// 		var tryGet =
		// 			SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference.GetEnemyListInRangeSortByDistance(
		// 				RelatedCharacterBehaviourRef.transform.position,
		// 				20f,
		// 				false);
		// 		//没扫到怪，那就不修改
		// 		if (tryGet == null || tryGet.Count == 0)
		// 		{
		// 		}
		// 		//扫到了，选成最近的那个
		// 		else
		// 		{
		// 			RecordedAttackDirection = (tryGet[0].transform.position - RelatedCharacterBehaviourRef.transform.position).normalized;
		// 		}
		// 	}
		// }
		//
		// protected override void _ABC_CheckAnimationComplete_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
		// {		
		// 	base._ABC_CheckAnimationComplete_OnGeneralAnimationComplete(ds);
		// 	string aniConfigName = ds.ObjectArguStr as string;
  //           
		// 	
		// 	//前摇结束了，可以开始发射了
		// 	if (aniConfigName.Equals(_sai_NormalAttackBegin.ConfigName, StringComparison.OrdinalIgnoreCase))
		// 	{
		// 		_currentAbleToShoot = true;
		// 		if (!RelatedCharacterBehaviourRef.TryOccupyByOccupationInfo(_sai_normalAttackLoop.OccupationInfo))
		// 		{
		// 			DBug.LogWarning($"普攻循环占用失败了，这并不合理，检查一下。配置信息{_sai_normalAttackLoop.OccupationInfo}");
		// 		}
		// 		var ds_ani = new DS_ActionBusArguGroup(_sai_normalAttackLoop, AnimationPlayOptionsFlagTypeEnum
		// 			.Default_缺省状态, false);
		// 		RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_ani);
  //               
		// 		SpawnRelatedLayout();
		// 		_lastShootTime = BaseGameReferenceService.CurrentFixedTime;
		// 	}
		// 	else if (aniConfigName.Equals(_sai_normalAttackEnd.ConfigName, StringComparison.OrdinalIgnoreCase))
		// 	{
		// 		if (!RelatedCharacterBehaviourRef.TryOccupyByOccupationInfo(_Cache_ANInfo_BattleIdle.OccupationInfo))
		// 		{
		// 			DBug.LogWarning($"普攻结束占用失败了，这并不合理，检查一下。配置信息{_Cache_ANInfo_BattleIdle.OccupationInfo}");
		// 		}
		//
		// 		var ds_ani = new DS_ActionBusArguGroup( _Cache_ANInfo_BattleIdle, AnimationPlayOptionsFlagTypeEnum
		// 			.Default_缺省状态, false);
		// 		RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_ani);
		// 	}
		// }


		// public override bool IfContainsRelatedAnimationConfigName(string configName)
		// {
		// 	// var skills= 
		// 	// RelatedCharacterBehaviourRef.GetRelatedSkillHolder().GetCurrentSkillList();
		// 	// SOConfig_RPSkill skill1;
		// 	
		// }
		public override bool IfContainsRelatedAnimationConfigName(string configName)
		{
			throw new NotImplementedException();
		}
	}




}