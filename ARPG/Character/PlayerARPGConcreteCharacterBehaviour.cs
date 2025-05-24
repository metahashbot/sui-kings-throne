using System;
using System.Collections.Generic;
using ARPG.Camera;
using ARPG.Character.Base;
using ARPG.Character.Config;
using ARPG.Character.Job;
using ARPG.Character.Player;
using ARPG.Config;
using ARPG.Equipment;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Character;
using Global.GlobalConfig;
using NodeCanvas.Framework;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using static Cinemachine.DocumentationSortingAttribute;
namespace ARPG.Character
{
	/// <summary>
	/// <para>一个玩家控制的ARPG角色。不是整体的玩家控制器。</para>
	/// <para></para>
	/// </summary>
	public class PlayerARPGConcreteCharacterBehaviour : BaseARPGCharacterBehaviour, I_RP_ObjectCanReleaseSkill
	{
#if UNITY_EDITOR

		// redraw constantly
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif

		private static InputAction_ARPG _inputActionRef;
		private static MainCameraBehaviour_ARPG _mainCameraRef;
		private static PlayerCharacterBehaviourController _playerControllerRef;

		public CIC_PlayerJobInfo PlayerJobInfo { get; protected set; }

		
		
		/// <summary>
		/// <para>当前角色的固定槽位。123对应换人的123键。同键不能换</para>
		/// </summary>
		public int CurrentPlayerSlot { get; protected set; }

		public enum ControlMovementDirectionTypeEnum
		{
			LeftOrRight_左右 = 0, Forward_朝里 = 1, Back_朝外 = 2
		}

		public ControlMovementDirectionTypeEnum ControlMovementDirection { get; protected set; } =
			ControlMovementDirectionTypeEnum.LeftOrRight_左右;

		public Vector3 GetCurrentPlayerFaceDirection()
		{
			switch (BaseGameReferenceService.ViewYawDirection)
			{
				//TODO:全是错的！！
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToForward_朝前:
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToLeft_朝左:
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToBack_朝后:
				case BaseGameReferenceService.ViewYawDirectionTypeEnum.ViewToRight_朝右:
					switch (ControlMovementDirection)	
					{
						case ControlMovementDirectionTypeEnum.LeftOrRight_左右:
							if (_selfArtHelper.CurrentFaceLeft)
							{
								return Vector3.left;
							}
							else
							{
								return Vector3.right;
							}
						case ControlMovementDirectionTypeEnum.Forward_朝里:
							return BaseGameReferenceService.CurrentBattleLogicalForwardDirection;
						case ControlMovementDirectionTypeEnum.Back_朝外:
							return -BaseGameReferenceService.CurrentBattleLogicalForwardDirection;
					}
					break;
			}
			return Vector3.zero;
		}



		/// <summary>
		/// <para>静态部分内容的初始化，在GLM的LateLoad时序</para>
		/// </summary>
		public static void StaticInitialize(InputAction_ARPG inputActionArpg)
		{
			_inputActionRef = inputActionArpg;
			_mainCameraRef = GameReferenceService_ARPG.Instance.CameraBehaviourRef;
			_playerControllerRef = GameReferenceService_ARPG.Instance.SubGameplayLogicManagerRef
				.PlayerCharacterBehaviourControllerReference;
		}


		/// <summary>
		/// <para>玩家角色的行动状态：就绪、当前使用、脱手技能、不可使用</para>
		/// </summary>
		public enum PlayerCharacterStateTypeEnum
		{
			None = 0,
			ReadyToUse_就绪使用 = 1,
			Using_当前使用 = 2,
			OutOfHand_脱手技能 = 3,
			UnusableCD_不可使用换下CD = 4,
			ExitBattle_已退场 = 5, }

		public enum PlayerCharacterTransitionTypeEnum
		{
			None = 0,
			SpawnAsReady_生成时就绪 = 1,
			SpawnAsUsing_生成时直接使用 = 2,
			ActiveOutSkill_使用脱手技能 = 3,
			OutSkillEndToUnusable_脱手技能结束 = 4,
			CDDoneReadyToUse_CD结束已就绪 = 5,
			ChangeUp_换出这个角色 = 6,
			ChangeDown_换下这个角色 = 7,
			HPClearAndExit_HP清空并退场 = 8, 
			ReviveAndReturn_复活并回场 = 9,
			
		}

#region 外部引用

#endregion


#region 内部字段

		[SerializeField, Required, LabelText("ArtHelper"), FoldoutGroup("配置", true)]
		protected PlayerARPGArtHelper _selfArtHelper;


		[ShowInInspector, LabelText("数据模型"), FoldoutGroup("运行时", true), NonSerialized]
		protected ARPGPlayerCharacterDataModel _selfDataModelInstance;


		[ShowInInspector, LabelText("角色ID"), FoldoutGroup("运行时", true)]
		public int SelfCharacterID { get; protected set; }

		[ShowInInspector, LabelText("当前行动状态"), FoldoutGroup("运行时", true)]
		public PlayerCharacterStateTypeEnum CurrentState { get; private set; } = PlayerCharacterStateTypeEnum.None;

		[ShowInInspector, LabelText("当前关联的武器信息"), FoldoutGroup("运行时", true), NonSerialized]
		public GlobalConfigSO.PlayerEquipmentInfo CurrentActiveWeaponEquipmentInfo;

		[ShowInInspector, LabelText("当前活跃的武器配置运行时实例"), FoldoutGroup("运行时", true), NonSerialized]
		public SOConfig_WeaponTemplate CurrentActiveWeaponConfigRuntimeInstance;


		[ShowInInspector, LabelText("换人阻塞"), FoldoutGroup("运行时", true), NonSerialized]
		protected Buff_PlayerSwitchCharacterBlock _switchCharacterBlockBuffRef;

#endregion


#region 细节ARPG操作

		[ShowInInspector, LabelText("本次占用前的上一次占用信息")]
		public DS_OccupationInfo LastOccupationInfo;


		/// <summary>
		/// <para>【本次占用信息】并不是一个字段。它是由当前ArtHelper所提供的信息。</para>
		/// </summary>`
		[ShowInInspector, LabelText("本次占用信息")]
		public DS_OccupationInfo CurrentOccupationInfo
		{
			get
			{
				if (_selfArtHelper == null)
				{
					return null;
				}
				if (_selfArtHelper.CurrentMainAnimationInfoRuntime == null)
				{
					return null;
				}
				return _selfArtHelper.CurrentMainAnimationInfoRuntime.OccupationInfo;
			}
		}



		/// <summary>
		/// <para>试图使用一个占用信息来取消当前的占用</para>
		/// <para>可以强制占用</para>
		/// </summary>
		public bool TryOccupyByOccupationInfo(
			DS_OccupationInfo info,
			bool forceOccupy = false,
			FixedOccupiedCancelTypeEnum fixedOccupiedCancelTypeEnum = FixedOccupiedCancelTypeEnum.None_未指定)
		{
			/*
			 * 如果当前没有占用信息，就把上次信息置为传入的，返回占用成功
			 * 如果强制占用
			 *         在编辑期会提示 抢占
			 *         返回成功
			 *     非强制占用
			 *         占用不成功，返回失败
			 *         占用成功，通知被抢占的信息，返回成功
			 */


			if (CurrentOccupationInfo == null)
			{
				LastOccupationInfo = info;
				return true;
			}
			if (forceOccupy)
			{
#if UNITY_EDITOR
				if (info.CancelLevel <= CurrentOccupationInfo.CurrentActiveOccupationLevel)
				{
					string currentName = CurrentOccupationInfo == null ? "无" :
						CurrentOccupationInfo.RelatedInterface == null ? "无" :
						CurrentOccupationInfo.RelatedInterface.ToString() + "的" +
						CurrentOccupationInfo.OccupationInfoConfigName;
					Debug.Log(
						$"角色{name}的占用级过程被强制取消了本应低于其占用的。当前占用为：[{CurrentOccupationInfo}_于_{currentName}]，被【{info.OccupationInfoConfigName}】所取消");
				}
#endif
				LastOccupationInfo?.RelatedInterface?.OnOccupiedCanceledByOther(info, fixedOccupiedCancelTypeEnum);
				LastOccupationInfo = CurrentOccupationInfo;
				return true;
			}
			else
			{
				if (info.CancelLevel > CurrentOccupationInfo.CurrentActiveOccupationLevel)
				{
#if UNITY_EDITOR
					if (LastOccupationInfo != null && LastOccupationInfo.RelatedInterface == null)
					{
						DBug.LogError(
							$"角色{name}在占用级过程中，上一个决策[{LastOccupationInfo}]没有被注入自己的关联接口，这不合理，可能影响到了互相取消的业务，检查一下");
					}
#endif
					if (fixedOccupiedCancelTypeEnum != FixedOccupiedCancelTypeEnum.None_未指定)
					{

						switch (info.OccupationInfoConfigName)
						{
							case { } when info.OccupationInfoConfigName.Contains("死亡离场"):
								fixedOccupiedCancelTypeEnum = FixedOccupiedCancelTypeEnum.DeathBreak_死亡断;
								break;
							case { } when info.OccupationInfoConfigName.Contains("换人离场"):
							case { } when info.OccupationInfoConfigName.Contains("换人入场"):
								fixedOccupiedCancelTypeEnum = FixedOccupiedCancelTypeEnum.Logic_SwitchCharacter_机制换人断;
								break;
							case { } when info.OccupationInfoConfigName.Contains("受击硬直"):
								fixedOccupiedCancelTypeEnum = FixedOccupiedCancelTypeEnum.StiffnessBreak_硬直断;
								break;
						}
					}
					LastOccupationInfo?.RelatedInterface?.OnOccupiedCanceledByOther(info, fixedOccupiedCancelTypeEnum);
					LastOccupationInfo = CurrentOccupationInfo;
					return true;
				}
				else
				{
					return false;
				}
			}
			return false;
		}


		//
		// /// <summary>
		// /// <para>无视占用级和取消级，直接占用</para>
		// /// </summary>
		// public void ForceOccupyByOccupationInfo(DS_OccupationInfo info)
		// {
		// 	LastOccupationInfo?.RelatedInterface?.OnOccupiedCanceledByOther(info.RelatedInterface);
		// 	LastOccupationInfo = CurrentOccupationInfo;
		// 	CurrentOccupationInfo = info;
		// }

#endregion
#region 各种初始化

		public override void InitializeOnInstantiate()
		{
			base.InitializeOnInstantiate();
			_selfDataModelInstance = new ARPGPlayerCharacterDataModel(this);

			_selfArtHelper.InitializeOnInstantiate(_selfActionBusInstance);
			_selfArtHelper.InjectBaseRPBehaviourRef(this);

			ApplySpawnAddonConfig();

			
			_selfActionBusInstance.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_OnAnimationComplete);

			_selfActionBusInstance.RegisterAction(
				ActionBus_ActionTypeEnum.L_Damage_OnFinallyDeath_伤害流程最终死亡,
				_ABC_OnFinallyDead_OnHpReducedTo0);

			_selfActionBusInstance.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_TryReturnToIdleExplicitlyOnPostAnimationDone,
				2);

            GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_CharacterBehaviour_OnEnemyDieGainExp_敌人死亡获得经验,
                _ABC_OnGainExp);

            if (_selfBehaviourNamedType != CharacterNamedTypeEnum.None)
			{
				SelfCharacterID = (int)_selfBehaviourNamedType;
			}

		}

#endregion

		public void SetCharacterSlot(int slot)
		{
			CurrentPlayerSlot = slot;
		}

		public void MakeCharacterStateTransition(PlayerCharacterTransitionTypeEnum transition, bool first = false)
		{
			var lastState = CurrentState;
			switch (transition)
			{
				case PlayerCharacterTransitionTypeEnum.SpawnAsReady_生成时就绪:
					if (CurrentState == PlayerCharacterStateTypeEnum.None)
					{
						CurrentState = PlayerCharacterStateTypeEnum.ReadyToUse_就绪使用;
						gameObject.SetActive(false);
					}
					break;
				case PlayerCharacterTransitionTypeEnum.SpawnAsUsing_生成时直接使用:

					var ds_change = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换);
					ds_change.ObjectArgu1 = this;
					CurrentState = PlayerCharacterStateTypeEnum.Using_当前使用;
					gameObject.SetActive(true);
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_change);

					break;
				case PlayerCharacterTransitionTypeEnum.ActiveOutSkill_使用脱手技能:
					if (CurrentState == PlayerCharacterStateTypeEnum.Using_当前使用)
					{
						CurrentState = PlayerCharacterStateTypeEnum.OutOfHand_脱手技能;
					}
					break;
				case PlayerCharacterTransitionTypeEnum.OutSkillEndToUnusable_脱手技能结束:
					if (CurrentState == PlayerCharacterStateTypeEnum.OutOfHand_脱手技能)
					{
						CurrentState = PlayerCharacterStateTypeEnum.UnusableCD_不可使用换下CD;
					}
					break;
				case PlayerCharacterTransitionTypeEnum.CDDoneReadyToUse_CD结束已就绪:
					if (CurrentState == PlayerCharacterStateTypeEnum.UnusableCD_不可使用换下CD)
					{
						CurrentState = PlayerCharacterStateTypeEnum.ReadyToUse_就绪使用;
						gameObject.SetActive(false);
					}
					break;
				case PlayerCharacterTransitionTypeEnum.ChangeUp_换出这个角色:
					CurrentState = PlayerCharacterStateTypeEnum.Using_当前使用;
					var ds_changeUp =
						new DS_ActionBusArguGroup(
							ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换);
					ds_changeUp.ObjectArgu1 = this;
					PlayerARPGConcreteCharacterBehaviour currentControlled =
						_playerControllerRef.CurrentControllingBehaviour;
					ds_changeUp.ObjectArgu2 = currentControlled;
					if (first)
					{
						ds_changeUp.IntArgu1 = 1;
					}
					
					gameObject.SetActive(true);
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_changeUp);

					break;
				case PlayerCharacterTransitionTypeEnum.ChangeDown_换下这个角色:


					var ds_ani = new DS_ActionBusArguGroup("换人离场",
						AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
						_selfArtHelper.SelfAnimationPlayResult,
						false, FixedOccupiedCancelTypeEnum.Logic_SwitchCharacter_机制换人断);
					_selfActionBusInstance.TriggerActionByType(ds_ani);
					
					
					CurrentState = PlayerCharacterStateTypeEnum.UnusableCD_不可使用换下CD;
					_switchCharacterBlockBuffRef.OnExistBuffRefreshed(this);
					ReturnToIdle(FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断, false);
					gameObject.SetActive(false);
					break;
				case PlayerCharacterTransitionTypeEnum.HPClearAndExit_HP清空并退场:
					CurrentState = PlayerCharacterStateTypeEnum.ExitBattle_已退场;
					var ds_ani_exit = new DS_ActionBusArguGroup("死亡离场",
						AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
						GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
						false, FixedOccupiedCancelTypeEnum.DeathBreak_死亡断);
					_selfActionBusInstance.TriggerActionByType(ds_ani_exit);
					
					gameObject.SetActive(false);
					var ds_exit =
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.G_Player_OnPlayerCharacterExitBattle_当玩家角色退场);
					ds_exit.ObjectArgu1 = this;
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_exit);
					
					
					
					
					break;
				case  PlayerCharacterTransitionTypeEnum.ReviveAndReturn_复活并回场:
					CurrentState = PlayerCharacterStateTypeEnum.ReadyToUse_就绪使用;
					CharacterDataValid = true;
					gameObject.SetActive(false);
					var ds_return = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.G_Player_OnPlayerCharacterReviveAndReturnBattle_当玩家角色复活并返回战场);
					ds_return.ObjectArgu1 = this;
					GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_return);
					break;
			}
			var ds_stateChange =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Player_OnCharacterActiveStateChanged_当该角色的活跃状态被改变);
			ds_stateChange.IntArgu1 = (int)CurrentState;
			ds_stateChange.IntArgu2 = (int)lastState;
			_selfActionBusInstance.TriggerActionByType(ds_stateChange);
		}




		/// <summary>
		/// <para>作为玩家的额外初始化部分。主要是武器装备相关的</para>
		/// </summary>
		public override void InitializeByConfig(
			SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE perLevelConfig,
			SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry gameplayDataEntry,
			SOFE_CharacterResourceInfo.PerTypeInfo resourceEntry,
			Vector3 spawnPos)
        {
            base.InitializeByConfig(perLevelConfig, gameplayDataEntry, resourceEntry, spawnPos);
			var castEntry = _selfDataModelInstance.SelfDataEntry_Database.InitializeFloatDataEntry(
				RP_DataEntry_EnumType.SkillCastingAccelerate_技能施法额外加速,
				0f);
			castEntry.SetLowerBound(-99f);


			//一些从GCAHH额外读的配置内容

			var gcahh = GlobalConfigurationAssetHolderHelper.Instance;
			//基本信息(PlayerBaseInfo)
			GCSO_PerCharacterInfo info = GlobalConfigSO.RuntimeContent()
				.AllCharacterInfoCollection.Find((characterInfo => characterInfo.CharacterID == SelfCharacterID));
            PlayerJobInfo = info.GetComponent<CIC_PlayerJobInfo>();


            //设置当前经验和升级经验
            var levelUpExpEntry = InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.LevelUpExp, 
				info.GetComponent<CIC_PlayerLevelInfo>().LevelUpExp);
            var currentExpEntry = GetSelfRolePlayDataModel().SelfDataEntry_Database.InitializeFloatPresentValueEntry(
				RP_DataEntry_EnumType.CurrentExp, levelUpExpEntry);
			currentExpEntry.ResetDataToValue(info.GetComponent<CIC_PlayerLevelInfo>().CurrentExp);


            //装备:23.10.21 现在已装备的将会直接从 GCAHH的Record里面读了
            List<RecordRuntime_RPGEquipmentInfo> relatedEquipments =
				SelfCharacterRuntimeRecordRef.AllEquippedEquipmentInfoList;






			//初始化职业
			// RP_BattleJobTypeEnum jobType = PlayerBaseInfo.CurrentJob;
			// //应用职业的效果
			// SOConfig_BattleJobContent jobInfo = gcahh.Collection_BattleJobCollection.GetBattleJobContentByType(jobType);
			// if (jobInfo != null)
			// {
			// 	jobInfo.ApplyJobFunctionContentsToBehaviour(this);
			// }



			//一些额外的功能，不在表里的但是大家都要用的东西，比如UP
			// var upMaxEntry =
			// 	_selfDataModelInstance.SelfDataEntry_Database.InitializeFloatDataEntry(RP_DataEntry_EnumType.UPMax_最大UP,
			// 		100f);
			//
			// var upCurrentPresentValue =
			// 	_selfDataModelInstance.SelfDataEntry_Database.InitializeFloatPresentValueEntry(
			// 		RP_DataEntry_EnumType.CurrentUP_当前UP,
			// 		upMaxEntry);
			// upCurrentPresentValue.SetUpperBound(100f);
			// upCurrentPresentValue.SetLowerBound(0f);

			_selfDataModelInstance.SelfBuffHolderInstance.TryApplyBuff_DirectMode(RolePlay_BuffTypeEnum
					.ARPG_PlayerUltraPowerUtility_玩家基本UP功能,
				this);
			_switchCharacterBlockBuffRef =
				_selfDataModelInstance.SelfBuffHolderInstance.TryApplyBuff_DirectMode(
					RolePlay_BuffTypeEnum.ARPG_PlayerSwitchCharacterBlock_玩家更换角色阻塞,
					this) as Buff_PlayerSwitchCharacterBlock;
			

			//var gcahh_weaponCollection = gcahh.Collection_WeaponGameplayTemplateCollection;
			//var gcahh_concreteEquipmentCollection = gcahh.FE_ConcreteNonWeaponEquipment;
			//var gcahh_nonWeaponCollection = gcahh.FE_NonWeaponEquipmentTemplateConfig;

			//应用所有的装备的一级、二级属性效果
			SelfCharacterRuntimeRecordRef.ApplyEquipmentEffect_BasePropertyAndSecondProperty(this);

			//施加装备上的perk，此处包含刻印和前缀


			var perk_equipment = SelfCharacterRuntimeRecordRef.GetActivePerkInfo_IncludePrefix();
			foreach (var perKVP in perk_equipment)
			{
				var s = perKVP.Item1;
				BLP_作为Perk的等级_ActAsPerkLevel blp_perkLevel = GenericPool<BLP_作为Perk的等级_ActAsPerkLevel>.Get();
				blp_perkLevel.Level = perKVP.Item2;
				//查表
				SOConfig_RPBuff buffConfig = gcahh.FE_EquipmentPerkPair.GetBuffConfigByPerkID(s);
				if (buffConfig != null)
				{
					ReceiveBuff_TryApplyBuff(buffConfig.ConcreteBuffFunction.SelfBuffType, this, this, blp_perkLevel);
				}
				GenericPool<BLP_作为Perk的等级_ActAsPerkLevel>.Release(blp_perkLevel);
			}


			//设置武器模板
			SelfCharacterRuntimeRecordRef.ApplyWeaponRuntimeInstance(this);
			

			//检查有没有实际的武器，这个是必须要有的，不然应当是不能进入战斗关卡的
			if (CurrentActiveWeaponEquipmentInfo == null)
			{
				DBug.LogWarning($"玩家角色{name}报告在加载后并没有 有效的武器装备信息，它没有武器，检查一下");
			}
			
			//初始化技能
			_selfDataModelInstance.SelfSkillHolderInstance.InitializeOnInstantiate(SelfCharacterID);
			
			
			
			ReturnToIdle(FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断, false);
		}



#region Tick

#region Input

#endregion

		private float _currentTime;
		private int _currentFrameCount;

		public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.UpdateTick(currentTime, currentFrameCount, delta);
			_currentTime = currentTime;
			_currentFrameCount = currentFrameCount;

			if (_selfDataModelInstance.DataModelInitialized)
			{
				if (CurrentState == PlayerCharacterStateTypeEnum.UnusableCD_不可使用换下CD)
				{
					if (_switchCharacterBlockBuffRef.GetBuffCurrentAvailableType() ==
					    BuffAvailableType.Timeout_AvailableTimeOut)
					{
						MakeCharacterStateTransition(PlayerCharacterTransitionTypeEnum.CDDoneReadyToUse_CD结束已就绪);
					}
				}
			}
			if (CurrentActiveWeaponConfigRuntimeInstance &&
			    CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction != null)
			{
				CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction.UpdateTick(currentTime,
					currentFrameCount,
					delta);
			}
		}



		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			_selfArtHelper.FixedUpdateTick(currentTime, currentFrameCount, delta);
			FixedUpdateRegen(delta);

			switch (CurrentState)
			{
				case PlayerCharacterStateTypeEnum.ReadyToUse_就绪使用:
					break;
				case PlayerCharacterStateTypeEnum.Using_当前使用:

					CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction.FixedUpdateTick(currentTime,
						currentFrameCount,
						delta);


					//处理位移技能的瞬发闪避

					//处理常规的技能Hold时的释放

					//处理常规技能的释放


					//跑动
					if (_playerControllerRef.InputDirect_InputMoveRaw.sqrMagnitude > 0.2f)
					{
						//获取输入在世界前的朝向。
						//
						// float angle = Vector3.SignedAngle(BaseGameReferenceService.CurrentBattleLogicalForwardDirection,
						// 	new Vector3(_playerControllerRef.InputResult_InputMoveV2OnCurrentGameplayDirection.x,
						// 		0f,
						// 		_playerControllerRef.InputResult_InputMoveV2OnCurrentGameplayDirection.y),
						// 	Vector3.up);
						//

						float d = Vector3.Dot(BaseGameReferenceService.CurrentBattleLogicalForwardDirection,
							new Vector3(_playerControllerRef.InputResult_InputMoveV2OnCurrentGameplayDirection.x,
								0f,
								_playerControllerRef.InputResult_InputMoveV2OnCurrentGameplayDirection.y));
						//偏移小于0.33，则认为是左或右
						if (Mathf.Abs(d) < 0.8f)
						{
							ControlMovementDirection = ControlMovementDirectionTypeEnum.LeftOrRight_左右;
						}
						else
						{
							//Forward
							if (d > 0f)
							{
								ControlMovementDirection = ControlMovementDirectionTypeEnum.Forward_朝里;
							}
							//Back
							else
							{
								ControlMovementDirection = ControlMovementDirectionTypeEnum.Back_朝外;
							}
						}


						var movementInfo = CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
							._Cache_ANInfo_BattleRun;
						if (TryOccupyByOccupationInfo(movementInfo.OccupationInfo,
							false,
							FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断))
						{
							var ds_Run = new DS_ActionBusArguGroup(
								CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleRun,
								AnimationPlayOptionsFlagTypeEnum.HoldAnimationProcess_维持动画进度,
								_selfArtHelper.SelfAnimationPlayResult,
								true,
								FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);
							switch (ControlMovementDirection)
							{
								case ControlMovementDirectionTypeEnum.LeftOrRight_左右:
									break;
								case ControlMovementDirectionTypeEnum.Forward_朝里:
									ds_Run.IntArgu1 += (int)AnimationPlayOptionsFlagTypeEnum.DirectionToForward_方向向前;
									break;
								case ControlMovementDirectionTypeEnum.Back_朝外:
									ds_Run.IntArgu1 += (int)AnimationPlayOptionsFlagTypeEnum.DirectionToBack_方向向后;
									break;
							}
							_selfActionBusInstance.TriggerActionByType(ds_Run);
						}
						if (CurrentOccupationInfo.OccupationInfoConfigName.Contains("跑动"))
						{
							_ProcessMovement(_playerControllerRef.InputDirect_InputMoveRaw,
								_playerControllerRef.InputDirect_InputMoveRaw.normalized,
								delta);
						}
					}
					//停下来了，没有移动
					else
					{
						AnimationInfoBase idleInfo = CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
							._Cache_ANInfo_BattleIdle;
						//如果有跑动停止的动画，则会播跑动停止，跑动停止的Complete会在下方自行处理到ReturnToIdle
						if (CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleRunStop != null)
						{
							AnimationInfoBase runStopInfo = CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
								._Cache_ANInfo_BattleRunStop;
							//如果已经处于Idle  或者 跑动停止，不会重复进入
							if (CurrentOccupationInfo.OccupationInfoConfigName.Equals(
								    idleInfo.OccupationInfo.OccupationInfoConfigName,
								    StringComparison.OrdinalIgnoreCase) ||
							    CurrentOccupationInfo.OccupationInfoConfigName.Equals(
								    runStopInfo.OccupationInfo.OccupationInfoConfigName,
								    StringComparison.OrdinalIgnoreCase))
							{
							}
							//如果刚才就是跑动，那播放跑动的动画
							else if (CurrentOccupationInfo.OccupationInfoConfigName.Equals(
								CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleRun
									.ConfigName,
								StringComparison.OrdinalIgnoreCase))
							{
								//试图跑动停止

								var ds_ani = new DS_ActionBusArguGroup(runStopInfo,
									AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
									_selfArtHelper.SelfAnimationPlayResult,
									true,
									FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
								switch (ControlMovementDirection)
								{
									case ControlMovementDirectionTypeEnum.LeftOrRight_左右:
										break;
									case ControlMovementDirectionTypeEnum.Forward_朝里:
										ds_ani.IntArgu1 = (int)AnimationPlayOptionsFlagTypeEnum.DirectionToForward_方向向前;
										break;
									case ControlMovementDirectionTypeEnum.Back_朝外:
										ds_ani.IntArgu1 = (int)AnimationPlayOptionsFlagTypeEnum.DirectionToBack_方向向后;
										break;
								}
								_selfActionBusInstance.TriggerActionByType(ds_ani);
								if (ds_ani.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
								{
									DBug.LogError($"【跑动停止应当是能取消跑动的】，如果出现了这个，则应当是配置问题，检查一下");
								}
							}
							//如果刚才不是跑动，就【试图】直接回idle。idle的取消级是1
							else if (!CurrentOccupationInfo.OccupationInfoConfigName.Equals(
								CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleRun
									.OccupationInfo.OccupationInfoConfigName,
								StringComparison.OrdinalIgnoreCase))
							{
								ReturnToIdle(FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断, true, false);
							}
							//
						}
						//没有跑动停止的动画，一切照旧
						else
						{
							TryMovePosition_OnlyXZ(Vector3.zero, true);
							//如果已经处于Idle了，则不会重复地试图进入idle
							if (CurrentOccupationInfo.OccupationInfoConfigName.Equals(
								idleInfo.OccupationInfo.OccupationInfoConfigName,
								StringComparison.OrdinalIgnoreCase))
							{
							}
							//不处于Idle，则试图占用为idle，并ReturnToIdle
							else
							{
								ReturnToIdle(FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断, true, false);
							}
						}
					}



					break;
				case PlayerCharacterStateTypeEnum.OutOfHand_脱手技能:
					break;
				case PlayerCharacterStateTypeEnum.UnusableCD_不可使用换下CD:
					var buffType = _switchCharacterBlockBuffRef.GetBuffCurrentAvailableType();
					if (buffType == BuffAvailableType.TimeInButNotMeetOtherRequirement ||
					    buffType == BuffAvailableType.Timeout_AvailableTimeOut)
					{
						MakeCharacterStateTransition(PlayerCharacterTransitionTypeEnum.CDDoneReadyToUse_CD结束已就绪);
					}
					break;
			}
		}


		public void _ProcessMovement(
			Vector2 inputMoveRaw,
			Vector2 moveDirectionNormalized,
			float delta,
			float speedMul = 1f)
		{
			Vector3 currentPosition = transform.position;
			float currentMoveSpeed = _selfDataModelInstance.GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速)
				.CurrentValue * speedMul;
			if (currentMoveSpeed < 0f)
			{
				currentMoveSpeed = 0f;
			}
			Vector3 logicalForward = BaseGameReferenceService.CurrentBattleLogicalForwardDirection;
			Vector3 logicRight = BaseGameReferenceService.CurrentBattleLogicRightDirection;
			//这里需要更改移动的真实方向，X轴移动的是当前的逻辑
			var movementOnX = moveDirectionNormalized.x * currentMoveSpeed * delta * logicRight;
			// currentRBPosition.x += moveDirectionNormalized.x * currentMoveSpeed * delta;
			var movementOnZ = moveDirectionNormalized.y * currentMoveSpeed * delta * logicalForward;

			var movement = movementOnX + movementOnZ;
			currentPosition += movement;

			if (inputMoveRaw.x > Mathf.Epsilon)
			{
				_selfArtHelper.SetFaceLeft(false);
			}
			else if (inputMoveRaw.x < -Mathf.Epsilon)
			{
				_selfArtHelper.SetFaceLeft(true);
			}



			// Vector3 alignedPosition = _glmRef.GetAlignedTerrainPosition(currentPosition);
			TryMovePosition_XYZ(movement, true);
		}

#endregion

#region 动画事件

		protected override void _ABC_ProcessAnimatorRequirement_OnRequireAnimator(DS_ActionBusArguGroup ds)
		{
			var currentAnimationInfoRef = ds.ObjectArgu1 as AnimationInfoBase;
			ds.GetObj2AsT<RP_DS_AnimationPlayResult>().Reset();
			float mul = ds.FloatArgu2 ?? 1f;

			//obj1为空，则表示传入的是str，那自己查一下
			if (currentAnimationInfoRef == null)
			{
				var configName = ds.ObjectArguStr as string;
				var findI =
					CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction.SelfAllPresetAnimationInfoList_RuntimeAll.FindIndex(
						info => info.ConfigName.Equals(configName, StringComparison.OrdinalIgnoreCase));
				if (findI == -1)
				{
					DBug.LogError(
						$" 玩家{name}在处理动画需求时，在没有按照配置查找而是在名字查找时，没有在武器中找到配置【{configName}】这不合理" +
						$"，检查一下。注意，如果是【技能】要求播放，则必须要技能自己带着那个动画才行");
					return;
				}
				_selfAnimationPlayerResult.RelatedAnimationInfoRef = currentAnimationInfoRef =
					CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction.SelfAllPresetAnimationInfoList_RuntimeAll[findI];
			}

			AnimationPlayOptionsFlagTypeEnum playOptions = (AnimationPlayOptionsFlagTypeEnum)ds.IntArgu1.Value;
			FixedOccupiedCancelTypeEnum fixedType = ds.IntArgu2.HasValue
				? (FixedOccupiedCancelTypeEnum)ds.IntArgu2.Value : FixedOccupiedCancelTypeEnum.None_未指定;


			//需要检测占用
			if (ds.FloatArgu3.HasValue && ds.FloatArgu3.Value > 0)
			{
				var occu = TryOccupyByOccupationInfo(currentAnimationInfoRef.OccupationInfo, false, fixedType);
				//占用失败了
				if (!occu)
				{ 
					ds.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation = true;
					return;
				} 
			}
			//不检测，相当于直接强制占用
			else
			{
				if (CurrentOccupationInfo != null)
				{
					DBug.Log(
						$"来自角色{name}进行了强制占用，当前为{CurrentOccupationInfo.OccupationInfoConfigName}，占用源为:{currentAnimationInfoRef.OccupationInfo.OccupationInfoConfigName}");

				}
				TryOccupyByOccupationInfo(currentAnimationInfoRef.OccupationInfo, true, fixedType);
			}

			_selfArtHelper.SetAnimation(currentAnimationInfoRef, playOptions, mul);
		}


		private void _ABC_OnAnimationComplete(DS_ActionBusArguGroup ds)
		{
			var configName = ds.ObjectArguStr as string;
			//跑动停止将会被待机所接续
			if (CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleRunStop != null)
			{
				if (configName.Contains(
					CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._ANString_RunStopInBattle,
					StringComparison.OrdinalIgnoreCase))
				{
					ReturnToIdle(FixedOccupiedCancelTypeEnum.ContinueBreak_接续断, true, false);
				}
			}
		}

#endregion



		/// <summary>
		/// <para>当切换当前控制的角色时，试图进行Buff转移</para>
		/// </summary>
		public void ProcessBuffTransferOnChangeCurrentBehaviour(PlayerARPGConcreteCharacterBehaviour newBehaviour)
		{
			_selfDataModelInstance.SelfBuffHolderInstance.ProcessBuffTransfer(newBehaviour);
		}

		public void ReceiveBuffTransfer(I_BuffTransferWithinPlayer toReceive)
		{
			_selfDataModelInstance.SelfBuffHolderInstance.ReceiveTransferFromOtherPlayer(toReceive, this);
		}


		protected override RolePlay_DataModelBase GetSelfRolePlayDataModel()
		{
			return _selfDataModelInstance;
		}

		public RPSkill_SkillHolder GetRelatedSkillHolder()
		{
			return _selfDataModelInstance.SelfSkillHolderInstance;
		}

		public RPBuff_BuffHolder GetRelatedBuffHolder()
		{
			return _selfDataModelInstance.SelfBuffHolderInstance;
		}

		public override RolePlay_ArtHelperBase GetSelfRolePlayArtHelper()
		{
			return _selfArtHelper;
		}
		
		
		

		/// <summary>
		/// <para>玩家角色去世。玩家角色HP已经归0</para>
		/// <para>角色退场</para>
		/// </summary>
		protected override void _ABC_OnFinallyDead_OnHpReducedTo0(DS_ActionBusArguGroup rpds)
		{
			MakeCharacterStateTransition(PlayerCharacterTransitionTypeEnum.HPClearAndExit_HP清空并退场);

			//取消激活，并不会销毁
			gameObject.SetActive(false);
		}



		/// <summary>
		/// <para>如果角色已经退场，则复活它</para>
		/// </summary>
		public void ReviveCharacter()
		{
			if (CurrentState != PlayerCharacterStateTypeEnum.ExitBattle_已退场)
			{
				DBug.LogError($"玩家角色{name}并没有退场，但此时要求复活它，这不合理，检查一下");
			}


			var hpMaxEntry = GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			var hpPresent = GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			hpPresent.ResetDataToValue(hpMaxEntry.CurrentValue);
			var spMaxEntry = GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP);
			var spPresent = GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);
			spPresent.ResetDataToValue(spMaxEntry.CurrentValue);


			MakeCharacterStateTransition(PlayerCharacterTransitionTypeEnum.ReviveAndReturn_复活并回场);
		}

		private float regen_totalDelta = 0f;
        private void FixedUpdateRegen(float delta)
        {
			regen_totalDelta += delta;
			if (regen_totalDelta > 1f )
            {
				regen_totalDelta -= 1f;
                var hpPresent = GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
                var hpsPresent = GetFloatDataEntryByType(RP_DataEntry_EnumType.HPSRegen_每秒HP回复);
                hpPresent.ResetDataToValue(hpPresent.CurrentValue + hpsPresent.CurrentValue);
                var spPresent = GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);
                var spsPresent = GetFloatDataEntryByType(RP_DataEntry_EnumType.SPSRegen_每秒SP回复);
                spPresent.ResetDataToValue(spPresent.CurrentValue + spsPresent.CurrentValue);
            }
        }


        /// <summary>
        /// <para>获取经验</para>
        /// </summary>
        private void _ABC_OnGainExp(DS_ActionBusArguGroup ds)
        {
			var currentExpEntry = GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentExp);
			var levelUpExpEntry = GetFloatDataEntryByType(RP_DataEntry_EnumType.LevelUpExp);
			var levelEntry = GetFloatDataEntryByType(RP_DataEntry_EnumType.CharacterLevel);
			int totalExp = ds.IntArgu1.Value + currentExpEntry.GetRoundIntValue();
            var levelUpExp = levelUpExpEntry.GetRoundIntValue();

            // 升级流程
            if (totalExp >= levelUpExp)
            {
				for(int i = 1; (i < 30) && (totalExp >= levelUpExpEntry.GetRoundIntValue()); i++)
                {
					totalExp -= levelUpExpEntry.GetRoundIntValue();
                    int newLevel = levelEntry.GetRoundIntValue() + 1;
                    levelEntry.ResetDataToValue(newLevel);
                    int newLevelUpEntry = GlobalConfigurationAssetHolderHelper.GetGCAHH().
                        FE_ARPGLevelUpExpConfig.GetLevelUpExp(newLevel);
                    levelUpExpEntry.ResetDataToValue(newLevelUpEntry);
                }
                //while (totalExp >= levelUpExpEntry.GetRoundIntValue())
                //{
                //    totalExp -= levelUpExpEntry.GetRoundIntValue();
                //    int newLevel = levelEntry.GetRoundIntValue() + 1;
                //    levelEntry.ResetDataToValue(newLevel);
                //    int newLevelUpEntry = GlobalConfigurationAssetHolderHelper.GetGCAHH().
                //        FE_ARPGLevelUpExpConfig.GetLevelUpExp(newLevel);
                //    levelUpExpEntry.ResetDataToValue(newLevelUpEntry);
                //}
                RefreshPlayerData();
                _playerControllerRef.CurrentControllingBehaviour
					.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.LevelUp_升级, null, null);
            }

            currentExpEntry.ResetDataToValue(totalExp);
        }
        private void RefreshPlayerData()
        {
            var type = (CharacterNamedTypeEnum)SelfCharacterID;
            int level = GetFloatDataEntryByType(RP_DataEntry_EnumType.CharacterLevel).GetRoundIntValue();
            var playerConfig = GlobalConfigurationAssetHolderHelper.GetGCAHH().
				FE_DataEntryInitConfig_BaseRPG.GetConfigByTypeAndLevel(type, level);
			GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Strength_主力量).ResetDataToValue(playerConfig.M_Strength);
            GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Dexterity_主敏捷).ResetDataToValue(playerConfig.M_Dexterity);
            GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Vitality_主体质).ResetDataToValue(playerConfig.M_Vitality);
            GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Spirit_主精神).ResetDataToValue(playerConfig.M_Spirit);
            GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Intellect_主智力).ResetDataToValue(playerConfig.M_Intelligence);
            GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Charm_主魅力).ResetDataToValue(playerConfig.M_Charming);
        }

        public Float_RPDataEntry ReleaseSkill_GetRelatedFloatDataEntry(RP_DataEntry_EnumType type)
		{
			return _selfDataModelInstance.GetFloatDataEntry(type);
		}
		public FloatPresentValue_RPDataEntry ReleaseSkill_GetPresentDataEntry(RP_DataEntry_EnumType type)
		{
			return _selfDataModelInstance.GetFloatPresentValue(type);
		}
		public BuffAvailableType ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return _selfDataModelInstance.SelfBuffHolderInstance.CheckTargetBuff(type);
		}
		public BaseRPBuff ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum buffType)
		{
			return _selfDataModelInstance.SelfBuffHolderInstance.GetTargetBuff(buffType);
		}
		public LocalActionBus ReleaseSkill_GetActionBus()
		{
			return _selfActionBusInstance;
		}
		public BaseRPSkill ReleaseSkill_GetTargetSkill(RPSkill_SkillTypeEnum skillType)
		{
			return _selfDataModelInstance.SelfSkillHolderInstance.GetSkillRuntimeByType(skillType)
				.ConcreteSkillFunction;
		}
		public RolePlay_ArtHelperBase ReleaseSkill_GetRelatedArtHelper()
		{
			return _selfArtHelper;
		}
		public Vector3 GetCasterFromPosition(bool alignY = true)
		{
			return transform.position;
		}



		

		// private void _ABC_EnterAbnormal_OnAbnormalBegin(DS_ActionBusArguGroup ds)
		// {
		// 	
		// 	var ds_animationConfig = ds.ObjectArguStr as string;
		// 	var buff_from = ds.ObjectArgu1 as Buff_非失衡异常_NonUnbalanceAbnormalState;
		//
		// 	var applyResult = ds.ObjectArgu2 as Buff_非失衡异常_NonUnbalanceAbnormalState.RPDS_AbnormalApplyResult;
		// 	
		// 	if (buff_from is null)
		// 	{
		// 		DBug.LogError($"色{name}的非失衡异常占用响应中，传入的ds.obj1是null，没有转换为 Buff_非失衡异常_NonUnbalanceAbnormalState");
		// 		applyResult.ApplySuccess = false;
		// 		return;
		// 	}
		// 	var animationConfigRef = CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
		// 		.GetAnimationInfoByConfigName(ds_animationConfig);
		// 	if (animationConfigRef == null)
		// 	{ 
		// 		 DBug.LogError($"角色{name}的非失衡异常占用响应中，没有找到要求的动画配置{ds_animationConfig}");
		// 		 applyResult.ApplySuccess = false;
		// 		return;
		// 	}
		// 	if (!TryOccupyByOccupationInfo(animationConfigRef.OccupationInfo))
		// 	{
		// 		DBug.Log($"角色{name}的异常占用失败了，来自buff{buff_from}，要求的动画是{ds_animationConfig}");
		// 		applyResult.ApplySuccess = false;
		// 		return;
		// 	}
		// 	DS_ActionBusArguGroup _ds_animation =
		// 		new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Player_AbnormalRequireAnimation_异常要求动画);
		// 	_ds_animation.ObjectArgu1 = animationConfigRef;
		// 	_selfActionBusInstance.TriggerActionByType(_ds_animation);
		// 	
		// }
		
		// /// <summary>
		// /// 在强制位移结束之后，回到待机状态。由于强制位移的结束并不是动画的结束，所以不会走正常的 动画结束后-调整占用-自动被待机占用
		// /// </summary>
		// /// <param name="ds"></param>
		// private void _ABC_RestoreToIdleFromUnbalanceOrAbnormal_OnStiffnessEndOrAbnormalEnd(DS_ActionBusArguGroup ds)
		// {
		// 	AnimationInfoBase info = CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleIdle;
		// 	LastOccupationInfo?.RelatedInterface?.OnOccupiedCanceledByOther(info.OccupationInfo);
		// 	var ds_enterIdle = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
		// 		.L_PCLogic_OnPlayerEnterBattleIdleFromLogic_由具体角色行为报告已经进入战斗待机状态);
		// 	_selfActionBusInstance.TriggerActionByType(ds_enterIdle);
		// }


		// /// <summary>
		// /// <para>显式地要求待机。如果没有待机成功会打Log，便于检查逻辑问题</para>
		// /// </summary>
		// private void _ReturnToIdleExplicitly(bool isFirst = false)
		// {
		// 	if (isFirst)
		// 	{
		// 		var ds_idleNonCheck = new DS_ActionBusArguGroup(
		// 			CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleIdle,
		// 			AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
		// 			_selfArtHelper.SelfAnimationPlayResult,
		// 			false,
		// 			FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);
		// 		_selfActionBusInstance.TriggerActionByType(ds_idleNonCheck);
		// 	}
		// 	else
		// 	{
		// 		if (!TryOccupyByOccupationInfo(CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
		// 			._Cache_ANInfo_BattleIdle.OccupationInfo))
		// 		{
		// 			DBug.LogWarning($"来自显式待机的占用失败了，这并不合理，什么情况");
		// 			return;
		// 		}
		// 		else
		// 		{
		// 			var ds_idleNonCheck = new DS_ActionBusArguGroup(
		// 				CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleIdle,
		// 				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
		// 				_selfArtHelper.SelfAnimationPlayResult,
		// 				false,
		// 				FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);
		// 			_selfActionBusInstance.TriggerActionByType(ds_idleNonCheck);
		// 		}
		// 	}
		// }


		/// <summary>
		/// 试图回到Idle。可选是否检查占用以及失败后是否提示
		/// <para>某些逻辑上的【返回idle】是强制性的，这种情况下不需要检查占用</para>
		/// <para>有些时候的【返回idle】并不强制（比如无移动输入时），这时候会检查</para>
		/// </summary>
		private void ReturnToIdle(FixedOccupiedCancelTypeEnum occuType,bool checkOccupy = true,bool showLog= true)
		{
			var ds_idle = new DS_ActionBusArguGroup(
				CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction._Cache_ANInfo_BattleIdle,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_selfArtHelper.SelfAnimationPlayResult,
				checkOccupy,
				occuType);
			_selfActionBusInstance.TriggerActionByType(ds_idle);
			if(showLog && ds_idle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				DBug.LogWarning($"角色{name}的待机占用失败了，这并不合理，什么情况");
			}

		}


		/// <summary>
		/// <para>在一切包含“后摇”的动画结束时，都试图直接返回idle</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_TryReturnToIdleExplicitlyOnPostAnimationDone(DS_ActionBusArguGroup ds)
		{
			var configName = ds.GetObj2AsT<AnimationInfoBase>().ConfigName;
			if (configName.Contains("后摇"))
			{
				ReturnToIdle(FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断, true, false);
			}


		}
		public I_RP_ContainVFXContainer GetVFXHolderInterface()
		{
			return _selfArtHelper as I_RP_ContainVFXContainer;
		}

        protected override void OnDestroy()
        {
			base.OnDestroy();

			GameObject.DestroyImmediate(CurrentActiveWeaponConfigRuntimeInstance);
        }
    }
}