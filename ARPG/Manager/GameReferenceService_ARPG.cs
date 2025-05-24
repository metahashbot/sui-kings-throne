using System;
using System.Collections.Generic;
using ARPG.Camera;
using ARPG.Character;
using ARPG.Character.Enemy;
using ARPG.Common;
using ARPG.Common.HitEffect;
using ARPG.UI;
using ARPG.UI.Panel;
using GameplayEvent;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.Config;
using Global.AssetLoad;
using Global.Audio;
using Global.TimedTaskManager;
using RegionMap;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.DataEntry;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WorldMapScene.RegionMap;
namespace ARPG.Manager
{
	public class GameReferenceService_ARPG : BaseGameReferenceService 
	{
		public static GameReferenceService_ARPG Instance;
		[SerializeField, Required, FoldoutGroup("配置", true), LabelText("GLM_ARPG")]
		public SubGameplayLogicManager_ARPG SubGameplayLogicManagerRef;

		[SerializeField, Required, FoldoutGroup("配置", true), LabelText("摄像机")]
		public MainCameraBehaviour_ARPG CameraBehaviourRef;

		[SerializeField, Required, FoldoutGroup("配置", true), LabelText("UI")]
		public UIManager_ARPG UIManagerRef;
		
		
		public InputAction_ARPG InputActionInstance;

		[SerializeField, Required, FoldoutGroup("配置", true), LabelText("VFX池")]
		public VFXPoolManager VFXPoolManagerInstance;

		
		[SerializeField, Required, FoldoutGroup("配置", true), LabelText("游戏事件管理器")]
		public GameplayEventManager GameplayEventManagerInstance;

		[SerializeField, Required, FoldoutGroup("配置", true), LabelText("通用受击特效管理")]
		public GeneralHitVFXManager GeneralHitVFXManagerInstance;


		[ ShowInInspector,FoldoutGroup("配置", true), LabelText("场景四叉树"),NonSerialized]
		public SceneTerrainQuadManager SceneTerrainQuadManagerInstance;


		[SerializeField, Required, FoldoutGroup("配置", true), LabelText("音频管理")]
		public GeneralAudioManager GeneralAudioManagerInstance;

#region 当前提供的【方向】信息，那个方向是前面？


		

#endregion

		public List<I_EPC_NeedTick> _epcNeedTickList = new List<I_EPC_NeedTick>();


		protected override void Awake()
		{
			_relatedPhase = GeneralGameAssetLoadPhaseEnum.ARPG;
			base.Awake();
			if (Instance == null)
			{ 
				Instance = this;
				CurrentBattleLogicalForwardDirection = Vector3.forward;


				GlobalActionBusInstance = new GlobalActionBus();
				GlobalActionBus.InjectGlobalActionBus(GlobalActionBusInstance);
				TimedTaskManagerInstance = new TimedTaskManager();
				InputActionInstance = new InputAction_ARPG();
				InputActionInstance.Enable();

				UIManagerRef.AwakeInitialize();
				GeneralAudioManagerInstance.AwakeInitialize();
				SubGameplayLogicManagerRef.AwakeInitialize();
				CameraBehaviourRef.AwakeInitialize();
				GameplayEventManagerInstance.AwakeInitialize();
				VFXPoolManagerInstance.InitializeOnAwake();
				GeneralHitVFXManagerInstance.AwakeInitialize();
				SceneTerrainQuadManagerInstance = FindObjectOfType<SceneTerrainQuadManager>();
				if (SceneTerrainQuadManagerInstance != null)
				{
					SceneTerrainQuadManagerInstance.AwakeInitialize();
				}
				InputActionInstance.BattleGeneral.Click.Enable();
			}
			else
			{
				DestroyImmediate(gameObject);
				return;
			}
		}


		protected override void Start()
		{
			base.Start();
		
		
			SubGameplayLogicManagerRef.StartInitialize();
			CameraBehaviourRef.StartInitialize();
			UIManagerRef.StartInitialize();
			VFXPoolManagerInstance.StartInitialize();
			GeneralHitVFXManagerInstance.StartInitialize();
			GeneralAudioManagerInstance.StartInitialize();

			TimedTaskManagerInstance.AddTimeTask(Task_RemoveItemHelper, 3f);
			if (SceneTerrainQuadManagerInstance != null)
			{
				SceneTerrainQuadManagerInstance.StartInitialize();
			}
			GlobalConfigurationAssetHolderHelper.Instance.StartLoadByGRS(GeneralGameAssetLoadPhaseEnum.ARPG);
			
			
			;
			//
			// if (FindObjectOfType<UIP_Dev_RegionMapChangeScene>() == null)
			// {
			// 	var newGO = Addressables.LoadAssetAsync<GameObject>("UIP_Battle_ChangeScene.prefab")
			// 		.WaitForCompletion();
			// 	Instantiate(newGO);
			// 	
			// 	
			// 	
			// 	
			// }

		}
		private void Task_RemoveItemHelper(int a, float b)
		{
			// var items = FindObjectsOfType<EditorProxy_ItemOnMapHelper>();
			// foreach (EditorProxy_ItemOnMapHelper perItem in items)
			// {
			// 	Destroy(perItem);
			// }
		}




		protected override void Update()
		{
			base.Update();
			if (CurrentGRSLoadState == GRSLoadStateEnum.NeedLoad)
			{
				SubGameplayLogicManagerRef.LateInitialize();
				UIManagerRef.LateInitialize();

				
				
				//找到全局的那个，相当于【全局】

				var logicAreaHolder  = FindObjectOfType<EditorProxy_AreaLogicHolder>();


				//此处处理各种预设好的EPC
				var editorProxy = FindObjectOfType<EditorProxy_ARPGEditor>(true);
				var allEPCList = new List<BaseEditorProxyComponent>();

				editorProxy.LateLoadInitialize(allEPCList,
					_epcNeedTickList,
					editorProxy,
					logicAreaHolder);

				CurrentGRSLoadState = GRSLoadStateEnum.LoadDone;
                GlobalActionBusInstance.TriggerActionByType(ActionBus_ActionTypeEnum.G_ARPG_lateLoadDone);
				SetRunningStateToNormal();


				GlobalActionBusInstance.RegisterAction(
					ActionBus_ActionTypeEnum.G_GE_RequireLevelClearConclusion_要求进行通关结算,
					_ABC_TimePassOnLevelClear);
				GlobalActionBusInstance.TriggerActionByType(ActionBus_ActionTypeEnum
					.G_DefaultEventRegisterWaitingEventLaunch_默认注册等待事件触发);

			}
			else if (CurrentGRSLoadState == GRSLoadStateEnum.LoadDone)
			{
				float delta = Time.deltaTime;

				GameplayEventManagerInstance.UpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				TimedTaskManagerInstance.UpdateTick(CurrentTimeInFrameCount, CurrentTimeInSecond, delta);

				SubGameplayLogicManagerRef.UpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				UIManagerRef.UpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				CameraBehaviourRef.UpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				VFXPoolManagerInstance.UpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				 
				GlobalActionBusInstance.UpdateTick( CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				if (SceneTerrainQuadManagerInstance != null)
				{
					SceneTerrainQuadManagerInstance.UpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				}


				foreach (I_EPC_NeedTick epc in _epcNeedTickList)
				{
					epc.UpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				}

				if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKeyDown(KeyCode.RightAlt))
				{
					
// #if UNITY_EDITOR
				if (Input.GetKeyDown(KeyCode.Keypad9)  || Input.GetKeyDown(KeyCode.Alpha9))
				{
					foreach (PlayerARPGConcreteCharacterBehaviour perCharacter in SubGameplayLogicManagerRef.PlayerCharacterBehaviourControllerReference
						.CurrentAllCharacterBehaviourList)
					{
						var bBuff = perCharacter.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
							.ARPG_PlayerUltraPowerUtility_玩家基本UP功能) as Buff_PlayerUltraPowerUtility;
						bBuff.RestoreUltraPowerCDByPartial(100f);
					}
				}

				if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
				{
					foreach (PlayerARPGConcreteCharacterBehaviour perCharacter in SubGameplayLogicManagerRef
						.PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList)
					{
						var chp = perCharacter.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP) as FloatPresentValue_RPDataEntry;
						chp.ResetDataToValue(perCharacter.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP)
							.CurrentValue);
					}
				}

				if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
				{
					foreach (PlayerARPGConcreteCharacterBehaviour perCharacter in SubGameplayLogicManagerRef
						.PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList)
					{
						var chp = perCharacter.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);
						chp.ResetDataToValue(perCharacter.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP)
							.CurrentValue);
					}
				}


				if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
				{
					for (int i = SubGameplayLogicManagerRef.CharacterOnMapManagerReference
							.CurrentAllActiveARPGCharacterBehaviourCollection.Count - 1;
						i >= 0;
						i--)
					{
						var perBehaviour = SubGameplayLogicManagerRef.CharacterOnMapManagerReference
							.CurrentAllActiveARPGCharacterBehaviourCollection[i];
						if (perBehaviour is not EnemyARPGCharacterBehaviour enemy)
						{
							continue;
						}
						var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(enemy,
							SubGameplayLogicManagerRef.PlayerCharacterBehaviourControllerReference
								.CurrentControllingBehaviour,
							DamageTypeEnum.TrueDamage_真伤,
							999999f);
						dai.StepOption = DamageProcessStepOption.TrueDamageDPS();
						dai.DamageWorldPosition = enemy.transform.position;
						var rpds_result = SubGameplayLogicManagerRef.DamageAssistServiceInstance.ApplyDamage(dai
							);
						rpds_result.ReleaseToPool();
					}
				}
				
				
				if(Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
				{
					foreach (PlayerARPGConcreteCharacterBehaviour perCharacter in SubGameplayLogicManagerRef
						.PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList)
					{
						if (perCharacter.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.InvincibleByDirector_All_WD_来自机制的完全无敌) == BuffAvailableType.NotExist)
						{
							perCharacter.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.InvincibleByDirector_All_WD_来自机制的完全无敌,perCharacter,perCharacter);
						}

						BaseRPBuff wd = perCharacter.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum.InvincibleByDirector_All_WD_来自机制的完全无敌);
						wd.ResetAvailableTimeAs(-1f);
						wd.ResetExistDurationAs(-1f);
					}
				}


				if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
				{
					foreach (PlayerARPGConcreteCharacterBehaviour perCharacter in SubGameplayLogicManagerRef.PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList)
					{
						var list = new List<SOConfig_RPSkill>();
						perCharacter.GetRelatedSkillHolder().GetCurrentSkillList(list);
						foreach (SOConfig_RPSkill perSkill in list)
						{
							perSkill.ConcreteSkillFunction.ModifyRemainingCD(false, 0f);
						}
					}
				}

				
				
				//按1，向上推玩家，失衡的，7力度
				if (Input.GetKeyDown(KeyCode.Keypad1) )
				{
					var pc = SubGameplayLogicManagerRef.PlayerCharacterBehaviourControllerReference
						.CurrentControllingBehaviour;
					var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(pc,
						pc,
						DamageTypeEnum.TrueDamage_真伤,
						1,
						DamageProcessStepOption.TrueDamageDPS());
					dai.DamageDirectionV3 = Vector3.forward;
					dai.ForceDirectionV3 = Vector3.forward;
					var blp_unbalance = GenericPool<Buff_失衡推拉_UnbalanceMovement.BLP_开始失衡推拉_StartUnbalanceMovementBLP>
						.Get();
					blp_unbalance.UnbalanceDirection = Vector3.forward;
					blp_unbalance.UnbalancePower = 7;

					var blp_dizzy = GenericPool<BLP_设置持续和有效时间_SetDurationAndTime>.Get();
					blp_dizzy.SetAllAsNotLess(0.5f);
					
					var newBAI = BuffApplyInfo_Runtime.GetFromPool();
					newBAI.BuffType = RolePlay_BuffTypeEnum.UnbalanceMovement_失衡推拉;
					newBAI.BuffLogicPassingComponents.Add(blp_unbalance);
					newBAI.BuffLogicPassingComponents.Add(blp_dizzy);
					
					

					 
					 
					
					dai.BuffApplyInfoList_RuntimeAdd.Add(newBAI);
					
					pc.ReceiveDamage_ReceiveFromRPDS(dai,0);
					blp_unbalance.ReleaseOnReturnToPool();
					blp_dizzy.ReleaseOnReturnToPool();
					;

				}
				//按2，给玩家打999999伤害
				if (Input.GetKeyDown(KeyCode.Keypad2) )
				{
					var pc = SubGameplayLogicManagerRef.PlayerCharacterBehaviourControllerReference
						.CurrentControllingBehaviour;
					var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(pc,
						pc,
						DamageTypeEnum.TrueDamage_真伤,
						99999,
						DamageProcessStepOption.TrueDamageDPS());
					pc.ReceiveDamage_ReceiveFromRPDS(dai, 0);
				}
				//
				//按3，向上推玩家，不失衡的，7力度
				if (Input.GetKeyDown(KeyCode.Keypad3))
				{
					var pc = SubGameplayLogicManagerRef.PlayerCharacterBehaviourControllerReference
						.CurrentControllingBehaviour;
					var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(pc,
						pc,
						DamageTypeEnum.TrueDamage_真伤,
						1,
						DamageProcessStepOption.TrueDamageDPS());
					
					dai.DamageDirectionV3 = Vector3.forward;
					dai.ForceDirectionV3 = Vector3.forward;
					var blp_unbalance = GenericPool<Buff_受击硬直_StiffnessOnHit.BLP_受击硬直通用_StiffnessOnHitBLP>
						.Get();
					blp_unbalance.StiffnessGuaranteed = true;
					blp_unbalance.AttackPower = 7;
					var newBAI = BuffApplyInfo_Runtime.GetFromPool();
					newBAI.BuffLogicPassingComponents.Add(blp_unbalance);
					dai.BuffApplyInfoList_RuntimeAdd.Add(newBAI);

				}
				}

// #endif

			}
		}
		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			if (CurrentGRSLoadState == GRSLoadStateEnum.LoadDone)
			{
				float fixedDelta = Time.fixedDeltaTime * CurrentFixedFrameSpeed;
				GameplayEventManagerInstance.FixedUpdateTick(CurrentFixedTime, CurrentFixedFrame, fixedDelta);

				SubGameplayLogicManagerRef.FixedUpdateTick(CurrentFixedTime, CurrentFixedFrame, fixedDelta);
				GeneralAudioManagerInstance.FixedUpdateTick(CurrentFixedTime, CurrentFixedFrame, fixedDelta);
			}
		}
		private void LateUpdate()
        {
            if (CurrentGRSLoadState == GRSLoadStateEnum.LoadDone)
            {
				float delta = Time.deltaTime;
				CameraBehaviourRef.LateUpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				UIManagerRef.LateUpdateTick(CurrentTimeInSecond, CurrentTimeInFrameCount, delta);
				GameplayEventManagerInstance.LateUpdateTick(CurrentTimeInSecond,
					CurrentTimeInFrameCount,
					Time.deltaTime);
			}
		}


		private void OnDestroy()
		{
			UnloadAndClear();
		}
		public override void UnloadAndClear()
		{
			base.UnloadAndClear();
			SubGameplayLogicManagerRef.ClearAndUnload();
			
			VFXPoolManagerInstance.ClearOnUnload();
			
			
			GameplayEventManagerInstance.UnloadAndClear();
			
			
			
		}

		

		/// <summary>
		/// <para>当关卡完成后，时间将会流逝一段</para>
		/// </summary>
		private void _ABC_TimePassOnLevelClear(DS_ActionBusArguGroup ds)
		{
            GlobalConfigurationAssetHolderHelper.Instance.GlobalConfigSO_Runtime.AddTimeIndex(1);
		}
#if UNITY_EDITOR

		private void OnEnable()
		{
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}
		void OnDisable()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
		}

		void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
		{
			// if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
			// {
			// 	EditorProxy_ARPGEditor editorProxy = FindObjectOfType<EditorProxy_ARPGEditor>();
			// 	foreach (BaseEditorProxyComponent component in editorProxy.RuntimeEditorProxyFunctionInstance
			// 		.EPCList_Serialize)
			// 	{
			// 		component.OnRestoreBackup();
			// 	}
			// 	
			// }
		}

#endif

	}
}