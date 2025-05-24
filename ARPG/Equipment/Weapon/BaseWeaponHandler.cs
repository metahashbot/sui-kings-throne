using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision;
using ARPG.Character.Player;
using ARPG.Common;
using ARPG.Manager;
using GameplayEvent;
using Global;
using Global.ActionBus;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile.Layout;
using RPGCore.Projectile.Layout.LayoutComponent;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ARPG.Equipment
{
	[Serializable]
	public abstract class BaseWeaponHandler : I_RP_NeedReactToOccupy
	{
#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif
		protected static PlayerCharacterBehaviourController _playerControllerRef;

#region 架构运行时

		[ShowInInspector, LabelText("当前吸附状态")]
		[FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public PlayerAutoAbsorbRuntimeInfo CurrentAutoAbsorbInfo { get; protected set; } =
			new PlayerAutoAbsorbRuntimeInfo();


		[ShowInInspector, LabelText("运行时武器SO引用")]
		[FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public SOConfig_WeaponTemplate SelfConfigRuntimeInstance { get; protected set; }

		[NonSerialized, LabelText("当前指定的武器【伤害类型】")]
		[FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public DamageTypeEnum CurrentAssignedDamageType;

		protected Buff_ChangeCommonDamageType _damageTypeB;

		protected Buff_ChangeCommonDamageType _damageTypeBuffRef
		{
			get
			{
				if (_damageTypeB == null)
				{ 
					_damageTypeB = RelatedCharacterBehaviourRef.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
						.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
				}
				return _damageTypeB;
			}
		}

#endregion


		protected List<ScriptableObject> _runtimeSOConfigCopyList = new List<ScriptableObject>();


#region 成套配置信息
		
		[SerializeField, LabelText("【动画】 配置文件们")]
		public List<SOConfig_PresetAnimationInfoBase> PresetAnimationInfoList_File =
			new List<SOConfig_PresetAnimationInfoBase>();
		
		
		[NonSerialized, LabelText("预设动画信息 - 运行时所有")] [FoldoutGroup("运行时",
			false,
			VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public List<AnimationInfoBase> SelfAllPresetAnimationInfoList_RuntimeAll = new List<AnimationInfoBase>();

		
		[SerializeField, LabelText("【作用】 配置文件们")]
		public List<SOConfig_PresetPlayerAnimationEventCallback> SelfAllAnimationEventCallbackList_File =
			new List<SOConfig_PresetPlayerAnimationEventCallback>();
		
		[NonSerialized, LabelText("【作用 - 运行时所有")] [FoldoutGroup("运行时",
			false,
			VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public List<SOConfig_PresetPlayerAnimationEventCallback> SelfPAECRuntimeConfig_All = new List<
			SOConfig_PresetPlayerAnimationEventCallback>();
			 
		
		public PACConfigInfo PACConfigInfo_Runtime { get; protected set; } = new PACConfigInfo();
		
		
		public List<PerVFXInfo> AllVFXInfoList_Runtime { get; protected set; } = new List<PerVFXInfo>();

		[SerializeField,LabelText("【特效】 配置文件们")]
		public List<SOConfig_PresetVFXInfoGroup > PresetVFXInfoList_File = new List<SOConfig_PresetVFXInfoGroup>();

#endregion

#region PAM动画信息

		[LabelText("动画_跑动"), FoldoutGroup("默认动画"), SerializeField]
		[GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ANString_RunInBattle = "跑动";

		[LabelText("动画_跑动停止"), FoldoutGroup("默认动画"), SerializeField]
		[GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ANString_RunStopInBattle = "跑动停止";

		[LabelText("动画_战斗待机"), FoldoutGroup("默认动画"), SerializeField]
		[GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ANString_IdleInBattle = "战斗待机";


		[LabelText("动画_入场"), FoldoutGroup("默认动画"), SerializeField]
		[GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ANString_SwitchEnter = "换人入场";

		[LabelText("动画_离场"), FoldoutGroup("默认动画"), SerializeField]
		[GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _ANString_SwitchExit = "换人离场";

#endregion

#region Gameplay逻辑们

		[LabelText("预设的攻击瞄准方式")]
		public WeaponAttackDirectionTypeEnum PresetAttackDirectionType;

#endregion


#region 运行时数据

		[NonSerialized, ShowInInspector, ReadOnly, LabelText("Cache_战斗跑动的配置"), FoldoutGroup("运行时", true)]
		public AnimationInfoBase _Cache_ANInfo_BattleRun;


		[NonSerialized, ShowInInspector, ReadOnly, LabelText("Cache_战斗跑动停止的配置"), FoldoutGroup("运行时", true)]
		public AnimationInfoBase _Cache_ANInfo_BattleRunStop;

		[NonSerialized, ShowInInspector, ReadOnly, LabelText("Cache_战斗待机的配置"), FoldoutGroup("运行时", true)]
		public AnimationInfoBase _Cache_ANInfo_BattleIdle;


		[NonSerialized, ShowInInspector, ReadOnly, LabelText("Cache_失衡的配置"), FoldoutGroup("运行时", true)]
		public AnimationInfoBase _Cache_ANInfo_BattleUnbalancing;

		[NonSerialized, ShowInInspector, ReadOnly, LabelText("Cache_角色入场的配置"), FoldoutGroup("运行时", true)]
		public AnimationInfoBase _Cache_ANInfo_SwitchEnter;

		[NonSerialized, ShowInInspector, ReadOnly, LabelText("Cache_角色离场的配置"), FoldoutGroup("运行时", true)]
		public AnimationInfoBase _Cache_ANInfo_SwitchExit;




		[FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		[ShowInInspector, LabelText("关联的角色引用")]
		public PlayerARPGConcreteCharacterBehaviour RelatedCharacterBehaviourRef { get; protected set; }

		protected LocalActionBus _selfActionBusRef;


		[NonSerialized, ShowInInspector, LabelText("记录的攻击方向"), FoldoutGroup("运行时")]
		public Nullable<Vector3> RecordedAttackDirection;


		[NonSerialized, ShowInInspector, LabelText("记录的攻击位置"), FoldoutGroup("运行时")]
		public Nullable<Vector3> RecordedAttackPosition;

		[ShowInInspector, FoldoutGroup("运行时"), LabelText("上次有效普攻输入时间点")]
		public float LastValidNormalAttackInputTime { get; protected set; }

#endregion



#region 初始化 

		



		public virtual void InitializeOnInstantiate(
			PlayerARPGConcreteCharacterBehaviour behaviour,
			LocalActionBus lab,
			SOConfig_WeaponTemplate configRuntime,
			DamageTypeEnum damageType)
		{
			SelfConfigRuntimeInstance = configRuntime;
			RelatedCharacterBehaviourRef = behaviour;
			_runtimeSOConfigCopyList.Clear();
			
			_selfActionBusRef = lab;
			_playerControllerRef = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
			InputAction_ARPG inputRef = GameReferenceService_ARPG.Instance.InputActionInstance;


			
			foreach (SOConfig_PresetAnimationInfoBase perConfig in PresetAnimationInfoList_File)
			{
				var newObj = UnityEngine.Object.Instantiate(perConfig);
				_runtimeSOConfigCopyList.Add(newObj);
				foreach (var perSAI in newObj.SelfAllPresetAnimationInfoList)
				{
					SelfAllPresetAnimationInfoList_RuntimeAll.Add(perSAI);
				}
			}
			foreach (AnimationInfoBase perSAI in SelfAllPresetAnimationInfoList_RuntimeAll)
			{
				if (perSAI.OccupationInfo != null)
				{
					perSAI.OccupationInfo.RelatedInterface = this;
				}
				else
				{
					DBug.LogError($"动画信息{perSAI.ConfigName}没有配置占用信息，这可能会导致一些问题。");
				}
			}
			SelfPAECRuntimeConfig_All =  new List<SOConfig_PresetPlayerAnimationEventCallback>();
			
			
			foreach (SOConfig_PresetPlayerAnimationEventCallback perConfig in SelfAllAnimationEventCallbackList_File)
			{
				var newObj = UnityEngine.Object.Instantiate(perConfig);
				_runtimeSOConfigCopyList.Add(newObj);
				SelfPAECRuntimeConfig_All.Add(newObj);
				newObj.PACConfigInfo.BuildRuntimePAEC();

			}




			foreach (var perVFX in PresetVFXInfoList_File)
			{
				var newObj = UnityEngine.Object.Instantiate(perVFX);
				_runtimeSOConfigCopyList.Add(newObj);
				foreach (var perVFXInfo in newObj.PerVFXInfoList)
				{
					AllVFXInfoList_Runtime.Add(perVFXInfo);
				}
			}
			




			_Cache_ANInfo_BattleIdle = GetAnimationInfoByConfigName(_ANString_IdleInBattle);
			_Cache_ANInfo_BattleRun = GetAnimationInfoByConfigName(_ANString_RunInBattle);
			_Cache_ANInfo_BattleRunStop = GetAnimationInfoByConfigName(_ANString_RunStopInBattle);
			_Cache_ANInfo_SwitchEnter = GetAnimationInfoByConfigName(_ANString_SwitchEnter);
			_Cache_ANInfo_SwitchExit = GetAnimationInfoByConfigName(_ANString_SwitchExit);



			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationStart_动画通用开始,
				_ABC_GeneralAnimationStart);
			_selfActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件,
				_ABC_GeneralAnimationCustomEventCallback);
			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_CheckAnimationComplete_OnGeneralAnimationComplete);
			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画,
				_ABC_RestoreAnimationToIdle);
			




			inputRef.BattleGeneral.FireBase.started += _IC_OnStartNormalAttackInput;
			inputRef.BattleGeneral.FireBase.performed += _IC_OnNormalAttackPerformed;
			inputRef.BattleGeneral.FireBase.canceled += _IC_OnNormalAttackInputCanceled;



			CurrentAssignedDamageType = damageType;
			Buff_ChangeCommonDamageType changeType =
				RelatedCharacterBehaviourRef.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
			changeType.AssignAndBroadcastChangeDamageType(CurrentAssignedDamageType);
		}

		/// <summary>
		/// <para>根据传入的动画信息【配置名字】来获取那个配置；检视器上【浅绿色】的那个</para>
		/// </summary>
		public virtual AnimationInfoBase GetAnimationInfoByConfigName(string configName)
		{
			foreach (AnimationInfoBase perInfo in SelfAllPresetAnimationInfoList_RuntimeAll)
			{
				if (perInfo.ConfigName.Equals(configName, StringComparison.OrdinalIgnoreCase))
				{
					return perInfo;
				}
			}
			return null;
		}


		/// <summary>
		/// <para>把所有的PAM内容都重置掉</para>
		/// </summary>
		protected virtual void ResetAllPAMContent()
		{
		}


#endregion

#region 基本 动画 & 输入 响应



		protected virtual void _ABC_GeneralAnimationStart(DS_ActionBusArguGroup ds)
		{
			var configName = ds.ObjectArguStr as string;
			if (!IfContainsRelatedAnimationConfigName(configName))
			{
				return;
			}


			if (SelfPAECRuntimeConfig_All != null && SelfPAECRuntimeConfig_All.Count > 0)
			{
				foreach (var perConfig in SelfPAECRuntimeConfig_All)
				{
					foreach (var perEC in perConfig.PACConfigInfo.AllAECList_Runtime)
					{
						if (perEC.AnimationEventPreset == PlayerAnimationEventPresetEnumType.Start_开始 &&
						    perEC._AN_RelatedAnimationConfigName.Equals(ds.ObjectArguStr as string,
							    StringComparison.OrdinalIgnoreCase))
						{
							switch (perEC)
							{
								case PAEC_开始一段位移_StartDisplacement paec开始一段位移StartDisplacement:
									PAEC_StartDisplacement(paec开始一段位移StartDisplacement, RecordedAttackDirection.Value);
									break;
								case PAEC_生成版面_SpawnLayout paec生成版面SpawnLayout:
									PAEC_ExecuteSpawnLayout(paec生成版面SpawnLayout);
									break;
								case PAEC_生成游戏事件_SpawnGameplayEvent paec生成游戏事件SpawnGameplayEvent:
									PAEC_LaunchGameplayEvent(paec生成游戏事件SpawnGameplayEvent);
									break;
								case PAEC_停止特效配置_StopVFXFromConfig paec停止特效配置StopVFXFromConfig:
									paec停止特效配置StopVFXFromConfig.ExecuteByWeapon(RelatedCharacterBehaviourRef, this);
									break;
								case PAEC_生成特效配置_SpawnVFXFromConfig paec生成特效配置SpawnVFXFromConfig:
									PAEC_PlayVFXOnAnimationCallback(paec生成特效配置SpawnVFXFromConfig);
									break;
								case PAEC_播放Timeline特效_PlayTimelineVFX paec播放Timeline特效PlayTimelineVFX:
									PAEC_PlayTimelineVFX(paec播放Timeline特效PlayTimelineVFX);
									break;
								default:
									PAEC_DefaultProcessPAEC(perEC);
									break;
							}
						}
					}
				}
			}
		}


		protected virtual void _ABC_GeneralAnimationCustomEventCallback(DS_ActionBusArguGroup ds)
		{
			var configName = (ds.ObjectArguStr as AnimationInfoBase).ConfigName;
			if (!IfContainsRelatedAnimationConfigName(configName))
			{
				return;
			}
			foreach (var perConfig in SelfPAECRuntimeConfig_All)
			{
				foreach (var perEC in perConfig.PACConfigInfo.AllAECList_Runtime)
				{

					if (perEC.AnimationEventPreset == PlayerAnimationEventPresetEnumType.Custom_自定义 &&
					    perEC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArguStr as AnimationInfoBase).ConfigName,
						    StringComparison.OrdinalIgnoreCase) &&
					    perEC.CustomEventString.Equals(ds.ObjectArgu2 as string, StringComparison.OrdinalIgnoreCase))
					{
						switch (perEC)
						{
							case PAEC_开始一段位移_StartDisplacement paec开始一段位移StartDisplacement:

								PAEC_StartDisplacement(paec开始一段位移StartDisplacement, RecordedAttackDirection.Value);

								break;
							case PAEC_生成版面_SpawnLayout paec生成版面SpawnLayout:
								PAEC_ExecuteSpawnLayout(paec生成版面SpawnLayout);
								break;
							case PAEC_生成游戏事件_SpawnGameplayEvent paec生成游戏事件SpawnGameplayEvent:
								PAEC_LaunchGameplayEvent(paec生成游戏事件SpawnGameplayEvent);
								break;
							case PAEC_停止特效配置_StopVFXFromConfig paec停止特效配置StopVFXFromConfig:
								paec停止特效配置StopVFXFromConfig.ExecuteByWeapon(RelatedCharacterBehaviourRef, this);
								break;
							case PAEC_生成特效配置_SpawnVFXFromConfig paec生成特效配置SpawnVFXFromConfig:
								PAEC_PlayVFXOnAnimationCallback(paec生成特效配置SpawnVFXFromConfig);
								break;
							case PAEC_播放Timeline特效_PlayTimelineVFX paec播放Timeline特效PlayTimelineVFX:
								PAEC_PlayTimelineVFX(paec播放Timeline特效PlayTimelineVFX);
								break;
							default:
								PAEC_DefaultProcessPAEC(perEC);
								break;
						}
					}
				}
			}
		}



		/// <summary> objStr 是配置的名字(AnimationInfo 的 Config名字，【比对】)， obj1是关联的AnimationArtHelper,obj2是使用的AnimationInfo</summary>
		protected virtual void _ABC_CheckAnimationComplete_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
		{
			var configName = ds.ObjectArguStr as string;
			if (!IfContainsRelatedAnimationConfigName(configName))
			{
				return;
			}
			if (SelfPAECRuntimeConfig_All != null && SelfPAECRuntimeConfig_All.Count > 0)
			{
				foreach (var perConfig in SelfPAECRuntimeConfig_All)
				{
					foreach (var perEC in perConfig.PACConfigInfo.AllAECList_Runtime)
					{

						if (perEC.AnimationEventPreset == PlayerAnimationEventPresetEnumType.Complete_结束 &&
						    perEC._AN_RelatedAnimationConfigName.Equals(configName, StringComparison.OrdinalIgnoreCase))
						{
							switch (perEC)
							{
								case PAEC_开始一段位移_StartDisplacement paec开始一段位移StartDisplacement:
									PAEC_StartDisplacement(paec开始一段位移StartDisplacement, RecordedAttackDirection.Value);

									break;
								case PAEC_生成版面_SpawnLayout paec生成版面SpawnLayout:
									PAEC_ExecuteSpawnLayout(paec生成版面SpawnLayout);
									break;
								case PAEC_生成游戏事件_SpawnGameplayEvent paec生成游戏事件SpawnGameplayEvent:
									PAEC_LaunchGameplayEvent(paec生成游戏事件SpawnGameplayEvent);
									break;
								case PAEC_停止特效配置_StopVFXFromConfig paec停止特效配置StopVFXFromConfig:
									paec停止特效配置StopVFXFromConfig.ExecuteByWeapon(RelatedCharacterBehaviourRef, this);
									break;
								case PAEC_生成特效配置_SpawnVFXFromConfig paec生成特效配置SpawnVFXFromConfig:
									PAEC_PlayVFXOnAnimationCallback(paec生成特效配置SpawnVFXFromConfig);
									break;
								case PAEC_播放Timeline特效_PlayTimelineVFX paec播放Timeline特效PlayTimelineVFX:
									PAEC_PlayTimelineVFX(paec播放Timeline特效PlayTimelineVFX);
									break;
								default:
									PAEC_DefaultProcessPAEC(perEC);
									break;
							}
						}
					}
				}
			}
			


		}




		/// <summary>
		/// 放到很后面，会播放待机动画
		/// </summary>
		/// <param name="ds"></param>
		protected virtual void _ABC_RestoreAnimationToIdle(DS_ActionBusArguGroup ds)
		{
			var rr = ds.GetObj1AsT<RP_DS_AnimationRestoreResult>();
			if (rr != null && rr.RestoreSuccess)
			{
				var ds_idle = new DS_ActionBusArguGroup(_Cache_ANInfo_BattleIdle,
					AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
					RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
					false,
					FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);
				RelatedCharacterBehaviourRef.GetRelatedActionBus().TriggerActionByType(ds_idle);
			}
		}
		
		public virtual void _IC_OnStartNormalAttackInput(InputAction.CallbackContext context)
		{
		}

		/// <summary>
		/// <para>直接接收到来自ARPGBehaviour的调用，ARPGBehaviour并不是十分关心具体的普攻情况，在很多时候只是在某些时机传入响应的数据而已</para>
		/// </summary>
		public virtual void _IC_OnNormalAttackPerformed(InputAction.CallbackContext context)
		{
		}

		public virtual void _IC_OnNormalAttackInputCanceled(InputAction.CallbackContext context)
		{
		}



#endregion

#region 基本的逻辑通用函数


		/// <summary>
		/// <para>当</para>
		/// </summary>
		/// <returns></returns>
		public virtual bool GetCurrentReactToNewNormalAttackOnInputStart()
		{
			if (!ReferenceEquals(_playerControllerRef.CurrentControllingBehaviour, RelatedCharacterBehaviourRef))
			{
				return false;
			}
			return true;
		}

		public virtual PlayerWeaponAnimationMotion GetCurrentPlayerAnimationMotion() { return null; }


		/// <summary>
		/// <para>记录一下当前的输入方向和位置。不同的攻击逻辑会在不同的时机记录，所以导致了不同的攻击朝向等。</para>
		/// </summary>
		protected virtual void GetAndOffsetCurrentInputPositionAndDirection()
		{
			RecordedAttackDirection = _playerControllerRef.InputResult_AimDirectionOnCurrentGameplayDirection;
			RecordedAttackPosition = _playerControllerRef.InputResult_InputPositionOnFloor;
		}

		public virtual void FixedUpdateTick(float ct, int cf, float delta)
		{
		}

		public virtual void UpdateTick(float ct, int cf, float delta)
		{
			for (int i = 0; i < AllVFXInfoList_Runtime.Count; i++)
			{
				AllVFXInfoList_Runtime[i].UpdateTick(delta, cf, delta);
			}
		}

		/// <summary>
		/// <para>当前动画被外面打断。这是一个十分常见的情况，因为自己也会打断自己。</para>
		/// </summary>
		public virtual FixedOccupiedCancelTypeEnum OnOccupiedCanceledByOther(
			DS_OccupationInfo occupySourceInfo,
			FixedOccupiedCancelTypeEnum explicitType = FixedOccupiedCancelTypeEnum.None_未指定 ,bool invokeBreakResult = true)
		{



			//有显式的取消类型传入了，则使用传入的逻辑
			//某些时刻的【打断】并不是因为动画变动导致的打断，而是因为需要打断才导致动画变动，后者不会经由GetFixedOccupiedCancelType获得类型信息
			//        比如释放时被沉默。

			//所以如果指定了类型，就不需要再去根据OccupationInfo去获取类型了，直接使用并处理即可。


			//需要对 OtherBreak进行一点额外的检查，因为有可能其实就还是自己的动画，但是由于架构设置，所有的前摇都会被当做OtherBreak。
			//则如果这个OtherBreak就是自己的动画，则将类型转换为 ContinueBreak;
			if (explicitType == FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断)
			{
				if (IfContainsRelatedAnimationConfigName(occupySourceInfo.OccupationInfoConfigName))
				{
					explicitType = FixedOccupiedCancelTypeEnum.ContinueBreak_接续断;
				}
				if (occupySourceInfo.OccupationInfoConfigName.Contains("待机"))
				{
					explicitType = FixedOccupiedCancelTypeEnum.ContinueBreak_接续断;
				}
			}
			else if (explicitType == FixedOccupiedCancelTypeEnum.ContinueBreak_接续断)
			{
				if (!IfContainsRelatedAnimationConfigName(occupySourceInfo.OccupationInfoConfigName))
				{
					explicitType = FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断;
				}
			}

			if (invokeBreakResult)
			{
				ProcessBreakResultByOccupiedCancelType(explicitType, occupySourceInfo);
			}
			return explicitType;

		}



		protected virtual void ProcessBreakResultByOccupiedCancelType(
			FixedOccupiedCancelTypeEnum explicitType,
			DS_OccupationInfo occupySourceInfo)
		{
			switch (explicitType)
			{
				case FixedOccupiedCancelTypeEnum.None_未指定:
					DBug.LogWarning(
						$"武器{SelfConfigRuntimeInstance.name}处理了一个 未显示指定 打断类型的动画占用，来自{occupySourceInfo.OccupationInfoConfigName}。已按照机制断处理，可以检查一下");
					BreakResult_OtherBreak();
					break;
				case FixedOccupiedCancelTypeEnum.StiffnessBreak_硬直断:
					BreakResult_StiffnessBreak();
					break;
				case FixedOccupiedCancelTypeEnum.WeakBreak_弱断:
					BreakResult_WeakBreak();
					break;
				case FixedOccupiedCancelTypeEnum.StrongBreak_强断:
					BreakResult_StrongBreak();
					break;
				case FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断:
					//如果是多段脱手的，且被待机打断，那就不发生变动
					if (GetCurrentPlayerAnimationMotion()?.PlayerAnimationMotionProgressType ==
					    PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中)
					{
					}
					else
					{
						BreakResult_OtherBreak();
					}
					break;
				case FixedOccupiedCancelTypeEnum.ContinueBreak_接续断:
					BreakResult_ContinueBreak();
					break;
				case FixedOccupiedCancelTypeEnum.CancelBreak_自主取消断:
					BreakResult_CancelBreak();
					break;
				case FixedOccupiedCancelTypeEnum.Logic_SwitchCharacter_机制换人断:
					BreakResult_SwitchCharacter();
					break;
				case FixedOccupiedCancelTypeEnum.DeathBreak_死亡断:
					BreakResult_DeathBreak();
					break;
			}
		}

		

#endregion

#region 打断处理


		public abstract bool IfContainsRelatedAnimationConfigName(string configName);


		protected virtual void BR_ResetAllPAMContent()
		{
			var cpam = GetCurrentPlayerAnimationMotion();
			if (cpam != null)
			{

				cpam.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
				cpam._prepare_InChargeTime = 0f;
				cpam._middleAnimation_ChargeMarkedAsRelease = false;
				cpam._prepareAnimation_ChargeMarkedAsRelease = false;
				cpam._Middle_InChargeTime = 0f;
				cpam._Middle_EffectingTime = 0f;
				cpam._prepareAnimation_WaitingForTargetPoint_InWaitingTime = 0f;
			}
		}

		protected virtual void BR_StopDisplacement()
		{
			foreach (var perConfig in SelfPAECRuntimeConfig_All)
			{
				foreach (var perAEC in perConfig.PACConfigInfo.AllAECList_Runtime)
				{
					if (perAEC is PAEC_开始一段位移_StartDisplacement displacement)
					{
						displacement.DisplacementActive = false;
					}
				}
			}
		}

		/// <summary>
		/// 通用的退出技能效果。用于硬直断、弱断、抢断、机制其他打断、机制换人。
		/// 死亡断会调用这个，并由额外操作
		/// </summary>
		public virtual void BR_CommonExitEffect()
		{
			BR_ResetAllPAMContent();
			BR_StopDisplacement();
			VFX_GeneralClear();
		}

		public virtual void BreakResult_StiffnessBreak()
		{
			BR_CommonExitEffect();

		}


		public virtual void BreakResult_WeakBreak()
		{

			BR_CommonExitEffect();
		}


		public virtual void BreakResult_StrongBreak()
		{

			BR_CommonExitEffect();
		}


		public virtual void BreakResult_OtherBreak()
		{

			BR_CommonExitEffect();
		}


		public virtual void BreakResult_ContinueBreak()
		{
			
		}

		public virtual void BreakResult_CancelBreak()
		{

			BR_CommonExitEffect();
		}

		public virtual void BreakResult_SwitchCharacter()
		{

			BR_CommonExitEffect();
		}



		public virtual void BreakResult_DeathBreak()
		{
			BR_CommonExitEffect();
			VFX_GeneralClear(true);
			
			
		}
		
		
		

#endregion




#region VFX

		public PerVFXInfo _VFX_PS_GetAndSetBeforePlay(
			string uid,
			bool needApplyTransformOffset = true,
			I_RP_ContainVFXContainer container = null,
			bool withDamageTypeVariant = false,
			PerVFXInfo.GetDamageTypeDelegate getDamageType = null,
			string from = null)
		{
			PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList_Runtime,
				uid,
				withDamageTypeVariant,
				getDamageType,
				from);
			return selfVFXInfo._VFX_GetPSHandle(
				needApplyTransformOffset,
				RelatedCharacterBehaviourRef.GetRelatedVFXContainer());
		}
		
		public PerVFXInfo _VFX_PS_StopAllSameUid(string uid,bool stopImmediate)
		{
			PerVFXInfo t = null;
			for (int i = 0; i < AllVFXInfoList_Runtime.Count; i++)
			{
				if (uid == AllVFXInfoList_Runtime[i]._VFX_InfoID)
				{
					AllVFXInfoList_Runtime[i]?.VFX_StopThis(stopImmediate);
				}
			}
			return t;
		}

		public PerVFXInfo _VFX_PS_JustGet(string uid)
		{
			PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList_Runtime, uid, false);
			return selfVFXInfo;
		}

		public PerVFXInfo _VFX_PS_GetAndSetBeforePlay(
			PerVFXInfo info,
			bool needApplyTransformOffset = true)
		{
			return info._VFX_GetPSHandle(
				needApplyTransformOffset,
				RelatedCharacterBehaviourRef.GetRelatedVFXContainer());
		}

		public virtual void VFX_GeneralClear(bool immediate = false)
		{
			PerVFXInfo.VFX_GeneralClear(AllVFXInfoList_Runtime, immediate);
		}


		/// <summary>
		/// <para>获取当前表现出来的伤害类型</para>
		/// </summary>
		public virtual DamageTypeEnum GetCurrentDamageType()
		{
			if (RelatedCharacterBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
				.ChangeCommonDamageType_常规伤害类型更改) == BuffAvailableType.NotExist)
			{
				return DamageTypeEnum.NoType_无属性;
			}
			else
			{
				var targetType = RelatedCharacterBehaviourRef.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum
					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
				return targetType.CurrentDamageType;
			}
		}

#endregion

#region 处理PAEC 响应

protected virtual void PAEC_DefaultProcessPAEC(BasePlayerAnimationEventCallback paec)
{
	paec.ExecuteByWeapon(RelatedCharacterBehaviourRef, this);

}

		protected virtual void PAEC_ExecuteSpawnLayout(PAEC_生成版面_SpawnLayout spawnLayout)
		{
			var l = spawnLayout.RelatedConfig.SpawnLayout_NoAutoStart(RelatedCharacterBehaviourRef);
			l.LayoutHandlerFunction.PresetNeedUniformTimeStamp = spawnLayout.NeedUniformTimeStamp;

			//如果预设了需要统一时间戳，则在此处统一
			if (spawnLayout.NeedUniformTimeStamp)
			{
				l.LayoutHandlerFunction.UniformTimeStamp = Time.frameCount + GetHashCode();
			}



			Nullable<Vector3> targetDirection = null;
			Nullable<Vector3> targetPosition = null;

			switch (SelfConfigRuntimeInstance.WeaponFunction.PresetAttackDirectionType)
			{
				case WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向:
					targetDirection = RecordedAttackDirection;
					targetPosition = null;
					break;
				case WeaponAttackDirectionTypeEnum.PointerDirectionInstant_瞬时的输入方向:
					targetDirection = _playerControllerRef.InputResult_AimDirectionOnCurrentGameplayDirection;
					targetPosition = null;
					break;
				case WeaponAttackDirectionTypeEnum.RegisteredCharacterMovementDirection_记录的角色移动方向:
					targetDirection = (_playerControllerRef.transform.forward.normalized);
					break;
				case WeaponAttackDirectionTypeEnum.ControlledByHandler_由具体的Handler实现:
					break;
				case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainRegistered_记录的指针位置:
					targetDirection = null;
					targetPosition = RecordedAttackPosition;
					l.LayoutHandlerFunction.OverrideSpawnFromPosition = targetPosition;
					break;
				case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainInstant_瞬时的指针位置:
					targetDirection = null;
					targetPosition = _playerControllerRef.InputResult_InputPositionOnFloor;
					l.LayoutHandlerFunction.OverrideSpawnFromPosition = targetPosition;
					break;
				case WeaponAttackDirectionTypeEnum.CharacterPosition_角色位置:
					targetDirection = null;
					targetPosition = _playerControllerRef.CurrentControllingBehaviour.transform.position;
					break;
				case WeaponAttackDirectionTypeEnum.RegisteredCharacterMoveDirectionThenPointer_记录的角色移动方向后指针:
					targetDirection =
						_playerControllerRef.CurrentControllingBehaviour.GetRelatedArtHelper().CurrentFaceLeft
							? BaseGameReferenceService.CurrentBattleLogicLeftDirection
							: BaseGameReferenceService.CurrentBattleLogicRightDirection;
					targetPosition = _playerControllerRef.CurrentControllingBehaviour.transform.position;
					break;
			}



			if (!spawnLayout.NotAcceptDamageTypeChange)
			{
				l.LayoutContentInSO.DamageApplyInfo.DamageType = _damageTypeBuffRef.CurrentDamageType;
			}
			l.LayoutHandlerFunction.OverrideSpawnFromDirection = targetDirection;
			if (spawnLayout is PAEC_响应蓄力的版面生成_SpawnLayoutAffectByCharge paec_charge)
			{
				var pam = GetCurrentPlayerAnimationMotion();
				float chargedTime = 0f, maxTime = 0f, minTime = 0f;

				if (paec_charge.AffectByPrepare)
				{
					chargedTime = pam._prepare_InChargeTime;
					minTime = pam._Prepare_MinChargeTime;
					maxTime = pam._Prepare_MaxChargeTime;
				}
				else
				{
					chargedTime = pam._Middle_InChargeTime;
					minTime = pam._Middle_MinChargeTime;
					maxTime = pam._Middle_MaxChargeTime;

				}
				var mul = (chargedTime - minTime) / (maxTime - minTime);

				if (paec_charge.IncludeSizeMultiply)
				{
					var evaFromCurve = paec_charge.SizeMultiplyCurve.Evaluate(mul);
					var valueFromCharge = evaFromCurve * paec_charge.SizeMultiplyAtFullCharge;
					l.LayoutContentInSO.RelatedProjectileScale = valueFromCharge;
				}
				if (paec_charge.IncludeDamageMultiply)
				{
					var evaFromCurve = paec_charge.DamageMultiplyCurve.Evaluate(mul);
					var valueFromCharge = evaFromCurve * paec_charge.DamageMultiplyAtFullCharge;
					var i = l.LayoutContentInSO.DamageApplyInfo.RelatedDataEntryInfos.FindIndex((config =>
						config.RelatedDataEntryType == RP_DataEntry_EnumType.AttackPower_攻击力));
					if (i != -1)
					{
						var entry = l.LayoutContentInSO.DamageApplyInfo.RelatedDataEntryInfos[i];
						entry.Partial *= valueFromCharge;
					}
				}
				if (paec_charge.IncludeLifeTimeMultiply)
				{
					var evaFromCurve = paec_charge.LifeTimeMultiplyCurve.Evaluate(mul);
					var valueFromCharge = evaFromCurve * paec_charge.LifeTimeMultiplyAtFullCharge;
					foreach (var perLC in l.LayoutContentInSO.LayoutComponentList)	
					{
						switch (perLC)
						{
							case LC_NWay nway:
								nway.DefaultLifetime *= valueFromCharge;
								break;
							case LC_JustSpawn justSpawn:
								justSpawn.SetLifetime *= valueFromCharge;
								break;
						}
					}
				}
			
				
				 
				 

			}
			
			
			l.LayoutHandlerFunction.StartLayout();
		}


		protected virtual void PAEC_LaunchGameplayEvent(PAEC_生成游戏事件_SpawnGameplayEvent paec)
		{
			switch (paec)
			{
				case PAEC_响应蓄力的事件触发_LaunchEventAffectByCharge paec_eventWithCharge:
					float chargedTime = 0f, maxTime = 0f, mimTime = 0f;
					var pam = GetCurrentPlayerAnimationMotion();
					if (paec_eventWithCharge.AffectByPrepare)
					{ 
						chargedTime = pam._prepare_InChargeTime;
						mimTime = pam._Prepare_MinChargeTime;
						maxTime = pam._Prepare_MaxChargeTime;
					}
					else
					{
						chargedTime = pam._Middle_InChargeTime;
						mimTime = pam._Middle_MinChargeTime;
						maxTime = pam._Middle_MaxChargeTime;
					}
					var mul = (chargedTime - mimTime) / (maxTime - mimTime);
					var ds_eg_mul = new DS_GameplayEventArguGroup();
					ds_eg_mul.FloatArgu3 = paec_eventWithCharge.SizeMultiplyCurve_1.Evaluate(mul) *
					                       paec_eventWithCharge.MulAtFullCharge_1;
					ds_eg_mul.FloatArgu4 = paec_eventWithCharge.SizeMultiplyCurve_2.Evaluate(mul) *
					                       paec_eventWithCharge.MulAtFullCharge_2;

					if (paec_eventWithCharge.RelatedConfig)
					{
						GameplayEventManager.Instance.StartGameplayEvent(paec_eventWithCharge.RelatedConfig, ds_eg_mul);
					}
					break;
				default:
					paec.ExecuteByWeapon(RelatedCharacterBehaviourRef, this);
					break;
			}
		}

		protected virtual void PAEC_PlayVFXOnAnimationCallback(PAEC_生成特效配置_SpawnVFXFromConfig paec)
		{
			BasePlayerAnimationEventCallback t = paec.ExecuteByWeapon(RelatedCharacterBehaviourRef, this, false);
			PerVFXInfo vfxInfo = paec.RuntimeVFXInfoRef;
			
		}

		protected virtual void PAEC_PlayTimelineVFX(PAEC_播放Timeline特效_PlayTimelineVFX paec_timeline)
		{
		}

		protected virtual void PAEC_StartDisplacement(PAEC_开始一段位移_StartDisplacement paec , Vector3 direction)
		{
			paec.StartDisplacement(RelatedCharacterBehaviourRef.transform.position,
				RelatedCharacterBehaviourRef,
				direction);
		}

#endregion


#region 动画 & 逻辑的各个子部分


#region 前摇部分




		protected virtual void _InternalProgress_CompleteOn_Prepare(
			string configName,
			SheetAnimationInfo_帧动画配置 sheetAnimationInfo,
			BaseCharacterSheetAnimationHelper animationHelper)
		{
			var currentPAM = GetCurrentPlayerAnimationMotion();
			//这里是正常前摇，所以检查是不是需要进入等待施法
			//包含等待，则进入等待
			if (currentPAM._Prepare_CanWaitForTargetPoint)
			{
				_InternalProgress_P_C_WaitInput(currentPAM);
				return;
			}
			if (currentPAM._Prepare_IsCharging)
			{
				//检查前摇的蓄力是否需要更换动画
				if (currentPAM._prepareAnimation_ChangeAnimationAfterBasePrepare)
				{
					_InternalProgress_P_B2_ChargeWithChangeAnimation(currentPAM);
					return;
				}
				else
				{
					_InternalProgress_P_B1_ChargeWithoutChangeAnimation(currentPAM);
					return;
				}
			}
			//都不是，那就自动流转到下一阶段

			_InternalProgress_P_A1_SimplePrepare(configName, sheetAnimationInfo, animationHelper);
		}


		/// <summary>
		/// <para>简单前摇，自动流转到 中段 </para>
		/// </summary>
		protected virtual void _InternalProgress_P_A1_SimplePrepare(
			string configName,
			SheetAnimationInfo_帧动画配置 sheetAnimationInfo,
			BaseCharacterSheetAnimationHelper animationHelper)
		{
			_InternalProgress_ProgressToMiddle();
		}

		protected virtual void _InternalProgress_P_B1_ChargeWithoutChangeAnimation(PlayerWeaponAnimationMotion pam)
		{
			// var ds_aniToPrepareCharge = new DS_ActionBusArguGroup(
			// 	SelfPlayerSkillAnimationMotion._prepareAnimation_ChangeAnimationAfterChargeAnimationName,
			// 	AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
			// 	_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
			// 	true,
			// 	FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			// _characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToPrepareCharge);

			// if (ds_aniToPrepareCharge.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			// {
			// 	//被阻塞了。这是不合理的，应当能够进入
			// 	DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在“准备”动画结束后，根据配置应当进入前摇蓄力动画，但是播放它的动画被阻塞了，这不合理");
			// 	return;
			// }
			pam.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中;
		}

		protected virtual void _InternalProgress_P_B2_ChargeWithChangeAnimation(PlayerWeaponAnimationMotion pam)
		{
			var ds_aniToPrepareCharge = new DS_ActionBusArguGroup(pam._prepareAnimation_ChangeAnimationAfterChargeAnimationName,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToPrepareCharge);

			if (ds_aniToPrepareCharge.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{RelatedCharacterBehaviourRef}的武器{SelfConfigRuntimeInstance.name}，在“准备”动画结束后，根据配置应当进入前摇蓄力动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}
			pam.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中;
		}


		protected virtual void _InternalProgress_P_C_WaitInput(PlayerWeaponAnimationMotion pam)
		{
			var ds_aniToPrepareWait = new DS_ActionBusArguGroup(pam._prepareAnimation_WaitingForTargetPointAnimationName,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToPrepareWait);

			if (ds_aniToPrepareWait.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{RelatedCharacterBehaviourRef}的技能{SelfConfigRuntimeInstance.name}，在“准备”动画结束后，根据配置应当进入前摇等待动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}
			pam.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中;
			return;
		}


#endregion
		


#region 中段部分

		protected virtual void _InternalProgress_ProgressToMiddle()
		{
			var currentPAM = GetCurrentPlayerAnimationMotion();
			currentPAM._Middle_InChargeTime = 0f;

			//是个持续施法的
			if (currentPAM.ContinuousCasting)
			{
				//可能是可以蓄力的
				if (currentPAM._Middle_IsCharging)
				{
					_InternalProgress_M_A2_CastingWithChargeAnimation(currentPAM);
				}
				else
				{
					_InternalProgress_M_A1_CastingWithoutChargeAnimation(currentPAM);
				}
			}
			//是个作用中的
			else
			{
				_InternalProgress_M_B1_Effecting(currentPAM);
			}
		}



		protected virtual void _InternalProgress_M_A1_CastingWithoutChargeAnimation(PlayerWeaponAnimationMotion pam)
		{
			var ds_aniToMiddle = new DS_ActionBusArguGroup(pam._ancn_MiddlePartAnimationName,
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true , FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToMiddle);

			if (ds_aniToMiddle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{RelatedCharacterBehaviourRef}的普攻{pam._ancn_PrepareAnimationName}" +
				              $"，在“准备”动画结束后，根据配置应当进入中段施法动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}
			pam.PlayerAnimationMotionProgressType = PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法;
		}


		protected virtual void _InternalProgress_M_A2_CastingWithChargeAnimation(PlayerWeaponAnimationMotion pam)
		{
			var ds_aniToMiddle = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(pam._ancn_MiddlePartAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToMiddle);
			if (ds_aniToMiddle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{RelatedCharacterBehaviourRef}的武器{SelfConfigRuntimeInstance.name}，在“准备”动画结束后，根据配置应当进入中段施法蓄力动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}

			pam.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中;
		}


		protected virtual void _InternalProgress_M_B1_Effecting(PlayerWeaponAnimationMotion pam)
		{
			var ds_aniToMiddle = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(pam._ancn_MiddlePartAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				RelatedCharacterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			RelatedCharacterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToMiddle);
			if (ds_aniToMiddle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{RelatedCharacterBehaviourRef}的技能{SelfConfigRuntimeInstance.name}，在“准备”动画结束后，根据配置应当进入中段作用中动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}

			pam.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中;
		}
		
		
#endregion



#endregion



		public virtual void UnloadAndClear()
		{
			_selfActionBusRef.RemoveAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_CheckAnimationComplete_OnGeneralAnimationComplete);

			for (int i = 0; i < _runtimeSOConfigCopyList.Count; i++)
			{
				UnityEngine.Object.Destroy(_runtimeSOConfigCopyList[i]);
			}
			_runtimeSOConfigCopyList.Clear();


			var inputRef = GameReferenceService_ARPG.Instance.InputActionInstance;
			inputRef.BattleGeneral.FireBase.performed -= _IC_OnNormalAttackPerformed;
			inputRef.BattleGeneral.FireBase.started -= _IC_OnStartNormalAttackInput;
			inputRef.BattleGeneral.FireBase.canceled -= _IC_OnNormalAttackInputCanceled;
		}

	}
//
//
//     [Serializable]
//     public class NormalAttackContainsThreePeriodAnimationInfo
//     {
//         [SerializeField, LabelText("总的配置名——匹配逻辑查找"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
//         public string MainConfigName;
//         [SerializeReference, LabelText("三段配置，一定是 前摇|判定|后摇 的顺序。名字里必须要有【普攻】")]
//         [InfoBox("三段配置，第一段(前摇)占用2取消2；第二段(判定)占用2取消3；第三段(后摇)占用1取消3，结束调整0")]
//         public List<AnimationInfoBase> _ThreePeriodList = new List<AnimationInfoBase>();
//         
//         
//         
// #if UNITY_EDITOR
//         [Button("转换动画配置到 Spine 版 ")]
//         protected virtual void _ConvertToSpine()
//         {
//             if (UnityEditor.EditorUtility.DisplayDialog("操作确认",
//                 "这个操作会将动画配置转换为Spine版，是否继续？\n" + "先检查一下容器里面的都是帧动画配置。\n【该操作不能撤销！】",
//                 "继续",
//                 "取消"))
//             {
//
//                 foreach (AnimationInfoBase perANI in _ThreePeriodList)
//                 {
//                     if (perANI is SpineAnimationInfo_Spine动画配置)
//                     {
//                         UnityEditor.EditorUtility.DisplayDialog("？", "这个容器里已经有Spine版的动画信息了！", "OK");
//                         return;
//                     }
//                 }
//                 List<AnimationInfoBase> newList = new List<AnimationInfoBase>();
//                 foreach (AnimationInfoBase perANI in _ThreePeriodList)
//                 {
//                     if (perANI is SheetAnimationInfo_帧动画配置 perSheet)
//                     {
//                         SpineAnimationInfo_Spine动画配置 newSheet =
//                             SpineAnimationInfo_Spine动画配置.GetDeepCopyFromSpine(perSheet);
//                         newList.Add(newSheet);
//                     }
//                     else
//                     {
//
//                     }
//                 }
//                 _ThreePeriodList = newList;
//             }
//         }
//
//         [Button("转换动画配置到 Sheet 版 ")]
//         protected virtual void _ConvertToSheet()
//         {
//             if (UnityEditor.EditorUtility.DisplayDialog("操作确认",
//                 "这个操作会将动画配置转换为帧动画版，是否继续？\n" + "先检查一下容器里面的都是Spine动画配置。\n【该操作不能撤销！】",
//                 "继续",
//                 "取消"))
//             {
//
//                 foreach (AnimationInfoBase perANI in _ThreePeriodList)
//                 {
//                     if (perANI is SheetAnimationInfo_帧动画配置)
//                     {
//                         UnityEditor.EditorUtility.DisplayDialog("？", "这个容器里已经有Sheet版的动画信息了！", "OK");
//                         return;
//                     }
//                 }
//                 List<AnimationInfoBase> newList = new List<AnimationInfoBase>();
//                 foreach (AnimationInfoBase perANI in _ThreePeriodList)
//                 {
//                     if (perANI is SpineAnimationInfo_Spine动画配置 perSpine)
//                     {
//                         SheetAnimationInfo_帧动画配置 newSheet = SheetAnimationInfo_帧动画配置.GetDeepCopyFromSpine(perSpine);
//                         newList.Add(newSheet);
//                     }
//                     else
//                     {
//
//                     }
//                 }
//                 _ThreePeriodList = newList;
//             }
//
//         }
// #endif
//
//
// #region 艺术资产相关
//
//
//         [SerializeField, LabelText("默认使用武器普通攻击Icon"), FoldoutGroup("配置", true), TitleGroup("配置/艺术")]
//         public Sprite WeaponNormalAttackIcon;
//
//
//
// #endregion
//

// #if UNITY_EDITOR
//         [Button("转换动画配置到 Spine 版 "), FoldoutGroup("配置", true), TitleGroup("配置/动画"), HorizontalGroup("配置/动画/Button")]
//         protected virtual void _ConvertToSpine()
//         {
//             if (UnityEditor.EditorUtility.DisplayDialog("操作确认",
//                 "这个操作会将动画配置转换为Spine版，是否继续？\n" + "先检查一下容器里面的都是帧动画配置。\n【该操作不能撤销！】",
//                 "继续",
//                 "取消"))
//             {
//
//                 foreach (AnimationInfoBase perANI in SelfAllPresetAnimationInfoList)
//                 {
//                     if (perANI is SpineAnimationInfo_Spine动画配置)
//                     {
//                         UnityEditor.EditorUtility.DisplayDialog("？", "这个容器里已经有Spine版的动画信息了！", "OK");
//                         return;
//                     }
//                 }
//                 List<AnimationInfoBase> newList = new List<AnimationInfoBase>();
//                 foreach (AnimationInfoBase perANI in SelfAllPresetAnimationInfoList)
//                 {
//                     if (perANI is SheetAnimationInfo_帧动画配置 perSheet)
//                     {
//                         SpineAnimationInfo_Spine动画配置 newSheet =
//                             SpineAnimationInfo_Spine动画配置.GetDeepCopyFromSpine(perSheet);
//                         newList.Add(newSheet);
//                     }
//                     else
//                     {
//
//                     }
//                 }
//                 SelfAllPresetAnimationInfoList = newList;
//             }
//         }
//
//         [Button("转换动画配置到 Sheet 版 "), FoldoutGroup("配置", true), TitleGroup("配置/动画"), HorizontalGroup("配置/动画/Button")]
//         protected virtual void _ConvertToSheet()
//         {
//             if (UnityEditor.EditorUtility.DisplayDialog("操作确认",
//                 "这个操作会将动画配置转换为帧动画版，是否继续？\n" + "先检查一下容器里面的都是Spine动画配置。\n【该操作不能撤销！】",
//                 "继续",
//                 "取消"))
//             {
//
//                 foreach (AnimationInfoBase perANI in SelfAllPresetAnimationInfoList)
//                 {
//                     if (perANI is SheetAnimationInfo_帧动画配置)
//                     {
//                         UnityEditor.EditorUtility.DisplayDialog("？", "这个容器里已经有Sheet版的动画信息了！", "OK");
//                         return;
//                     }
//                 }
//                 List<AnimationInfoBase> newList = new List<AnimationInfoBase>();
//                 foreach (AnimationInfoBase perANI in SelfAllPresetAnimationInfoList)
//                 {
//                     if (perANI is SpineAnimationInfo_Spine动画配置 perSpine)
//                     {
//                         SheetAnimationInfo_帧动画配置 newSheet = SheetAnimationInfo_帧动画配置.GetDeepCopyFromSpine(perSpine);
//                         newList.Add(newSheet);
//                     }
//                     else
//                     {
//
//                     }
//                 }
//                 SelfAllPresetAnimationInfoList = newList;
//             }
//
//         }
// #endif

//     }
}