using System;
using System.Collections.Generic;
using System.Diagnostics;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Player;
using ARPG.Equipment;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Buff.Requirement;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.PlayerAnimationMotion;
using RPGCore.Projectile.Layout;
using RPGCore.Skill.Config;
using RPGCore.Skill.Config.Requirement;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Playables;
using UnityEngine.Pool;
using WorldMapScene.Character;
using Object = UnityEngine.Object;
namespace RPGCore.Skill
{
	/// <summary>
	/// <para>一个基础的抽象技能。相当于Handler的基类。</para>
	/// </summary>
	[Serializable]
	public abstract partial class BaseRPSkill : I_RP_NeedReactToOccupy, I_RPLogicCanApplyStoic,
		I_RPLogicCanApplyInvincible
	{
		
		[LabelText("技能标识符枚举")]
		public RPSkill_SkillTypeEnum SkillType;
		
		[LabelText("技能等级")]
		public int SkillLevel = 1;

		[LabelText("初始冷却时间"), SuffixLabel("秒")]
		public float CoolDownTime = 10f;
		[LabelText("初始技能威力"), SuffixLabel("%")]
		public float SkillPowerInitPercent = 500f;

		[LabelText("    ↓→所有受影响的版面UID"), SerializeField]
		protected List<string> SkillPowerAffectLayoutUID = new List<string>();




		[LabelText("初始释放消耗的SP点")]
		public float SPConsume;
		
		[LabelText("校准朝向时使用的类型")]
		public WeaponAttackDirectionTypeEnum AttackDirectionType = WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向;



		[SerializeField, LabelText("【VFX】 关联的特效信息们")]
		[ListDrawerSettings(ShowPaging = true,
			ShowItemCount = true,
			ShowIndexLabels = true,
			ListElementLabelName = "_VFX_InfoID")]
		public List<PerVFXInfo> AllVFXInfoList;

		[LabelText("【动画】 动画配置信息组们"), SerializeReference] [ListDrawerSettings(ShowPaging = true,
			ShowItemCount = true,
			ShowIndexLabels = true,
			ListElementLabelName = "ConfigName")]
		public List<AnimationInfoBase> SelfAnimationInfoList = new List<AnimationInfoBase>
		{
			new SheetAnimationInfo_帧动画配置
			{
				ConfigName = "基本前摇",
			},
			new SheetAnimationInfo_帧动画配置
			{
				ConfigName = "施法中|作用中 二选一"
			},
			new SheetAnimationInfo_帧动画配置
			{
				ConfigName = "基本后摇",
			}
		};

		[LabelText("【PAM】  PAM-动画动作配置"), SerializeField]
		public PlayerSkillAnimationMotion SelfPlayerSkillAnimationMotion;


		[Space(20)]
		[LabelText("【作用】—默认作用，常用于匹配动画")]
		public PACConfigInfo PACConfigInfo = new PACConfigInfo();


		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetExitName()")]
#endif
		public PACConfigInfo PACConfigInfo_ExitSkill;


		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetToPrepareName()")]
#endif
		public PACConfigInfo PACConfigInfo_ProgressToPrepare;
		
		

		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetToMiddleName()")]
#endif
		public PACConfigInfo PACConfigInfo_ProgressToMiddle;


		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetToPostName()")]
#endif
		public PACConfigInfo PACConfigInfo_ProgressToPost;

		
		
        
		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetLogicOtherName()")]
#endif
		public PACConfigInfo PACConfigInfo_LogicOtherBreak;

		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetStiffnessName()")]
#endif
		public PACConfigInfo PACConfigInfo_StiffnessBreak;


		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetWeakName()")]
#endif
		public PACConfigInfo PACConfigInfo_WeakBreak;

		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetStrongName()")]
#endif

		public PACConfigInfo PACConfigInfo_StrongBreak;


		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetContinueName()")]
#endif
		public PACConfigInfo PACConfigInfo_ContinueBreak;

		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetCancelName()")]
#endif
		public PACConfigInfo PACConfigInfo_CancelBreak;

		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetSwitchName()")]
#endif
		public PACConfigInfo PACConfigInfo_SwitchCharacterBreak;


		[FoldoutGroup("各节点上的【作用】（无Tick无匹配）")]
		[SerializeField]
#if UNITY_EDITOR
		[LabelText("@this.GetDeathName()")]
#endif

		public PACConfigInfo PACConfigInfo_DeathBreak;

#if UNITY_EDITOR

		private string GetExitName()
		{
			return $"作用——退出时 {PACConfigInfo_ExitSkill?._EDITOR_GetElementCount()}";
		}

		private string GetToPrepareName()
		{
			return $"作用——进入准备时 {PACConfigInfo_ProgressToPrepare?._EDITOR_GetElementCount()}";
		}

		private string GetToMiddleName()
		{
			return $"作用——进入中段时 {PACConfigInfo_ProgressToPrepare?._EDITOR_GetElementCount()}";
		}
		private string GetToPostName()
		{
			return $"作用——进入后摇时 {PACConfigInfo_ProgressToPost?._EDITOR_GetElementCount()}";
		}

		private string GetLogicOtherName()
		{
			return $"作用——逻辑其他时 {PACConfigInfo_LogicOtherBreak?._EDITOR_GetElementCount()}";
		}

		private string GetStiffnessName()
		{
			return $"作用——硬直时 {PACConfigInfo_StiffnessBreak?._EDITOR_GetElementCount()}";
		}

		private string GetWeakName()
		{
			return $"作用——弱断时 {PACConfigInfo_WeakBreak?._EDITOR_GetElementCount()}";
		}

		private string GetStrongName()
		{
			return $"作用——强断时 {PACConfigInfo_StrongBreak?._EDITOR_GetElementCount()}";
		}

		private string GetContinueName()
		{
			return $"作用——接续断时 {PACConfigInfo_ContinueBreak?._EDITOR_GetElementCount()}";
		}

		private string GetCancelName()
		{
			return $"作用——取消断时 {PACConfigInfo_CancelBreak?._EDITOR_GetElementCount()}";
		}

		private string GetSwitchName()
		{
			return $"作用——换人断时 {PACConfigInfo_SwitchCharacterBreak?._EDITOR_GetElementCount()}";
		}

		private string GetDeathName()
		{
			return $"作用——死亡断时 {PACConfigInfo_DeathBreak?._EDITOR_GetElementCount()}";
		}
		
#endif
		
		 
		 
		 
		 
		
		
		 

		[ShowInInspector, LabelText("预输入？")] [FoldoutGroup("运行时",
			false,
			VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]

		public bool _InputHolding { get; protected set; } = false;

#region 外部引用

		protected PlayerARPGConcreteCharacterBehaviour _characterBehaviourRef;
		protected PlayerARPGArtHelper _characterArtHelperRef;
		protected PlayerCharacterBehaviourController _playerControllerRef;

#endregion
#region 运行时关联信息

		[ShowInInspector, LabelText("技能归属于"), FoldoutGroup("运行时",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public I_RP_ObjectCanReleaseSkill RelatedRPSkillCaster { get; protected set; }


		[ShowInInspector, LabelText("技能归属于的技能Holder"), FoldoutGroup("运行时/架构",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public RPSkill_SkillHolder RelatedSkillHolderRef { get; protected set; }



		[ShowInInspector, LabelText("运行时技能SO实例"), FoldoutGroup("运行时/架构",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public SOConfig_RPSkill SelfSkillConfigRuntimeInstance;



		[ShowInInspector, LabelText("关联的输入槽位"), FoldoutGroup("运行时/架构",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public SkillSlotTypeEnum SkillSlot;

		protected LocalActionBus _selfActionBusRef;


		// [InfoBox("基类技能并不提供类似于“是否在释放”的相关信息，因为不同的技能对于“释放”的定义会不一样，甚至产生额外的子状态\n" +
		//          "所以只会提供“激活”这个通用的信息")]
		// [NonSerialized,ShowInInspector, LabelText("当前技能活跃吗"), FoldoutGroup("运行时/功能", true)]
		// public bool SkillIsActive = false;


		[ShowInInspector, LabelText("当前计CD时长"), FoldoutGroup("运行时/功能",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		[NonSerialized]
		public float CurrentCoolDownDuration;

		[ShowInInspector, ReadOnly, LabelText("剩余CD"), FoldoutGroup("运行时/功能",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public float RemainingCoolDownDuration { get; protected set; }

		public virtual void ModifyRemainingCD(bool modifyOrReset, float modify)
		{
			if (modifyOrReset)
			{
				RemainingCoolDownDuration += modify;
			}
			else
			{
				RemainingCoolDownDuration = modify;
			}
		}

		[ShowInInspector, LabelText("当前SP消耗量"), FoldoutGroup("运行时/功能",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public float CurrentSPConsume;

		protected FloatPresentValue_RPDataEntry _spEntry;


		protected Float_RPDataEntry _entry_attackSpeed;
		protected Float_RPDataEntry _entry_castAccelerate;

		protected Buff_ChangeCommonDamageType _ccdt;
		protected Buff_ChangeCommonDamageType _Buff_DamageTypeBuffRef
		{
			get
			{
				if (_ccdt == null)
				{
					_ccdt = RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
						.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
				}
				return _ccdt;
			}
		}



		protected Float_RPDataEntry _cdaEntry;
		protected float _selfSkillCDAccelerate
		{
			get
			{
				if (_cdaEntry == null)
				{
					_cdaEntry = RelatedRPSkillCaster.ReleaseSkill_GetRelatedFloatDataEntry(RP_DataEntry_EnumType
						.SkillAccelerate_技能加速);
				}
				if (_cdaEntry == null)
				{
					return 0f;
				}
				else
				{
					return _cdaEntry.GetCurrentValue();
				}
			}
		}
		[NonSerialized, ShowInInspector, LabelText("记录的攻击方向"), FoldoutGroup("运行时",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public Nullable<Vector3> RecordedAttackDirection;


		[NonSerialized, ShowInInspector, LabelText("记录的攻击位置"), FoldoutGroup("运行时", VisibleIf =
			 "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public Nullable<Vector3> RecordedAttackPosition;

		[NonSerialized, ShowInInspector, LabelText("自身发出的Layout们"), FoldoutGroup("运行时",
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		protected List<SOConfig_ProjectileLayout> _layoutList = new List<SOConfig_ProjectileLayout>();

#endregion

#region Tick

		public virtual void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			if (PACConfigInfo != null && PACConfigInfo.AllAECList_Runtime.Count > 0)
			{
				foreach (var perEC in PACConfigInfo.AllAECList_Runtime)
				{
					switch (perEC)
					{
						case PAEC_生成选点指示器_SpawnPositionIndicator indicator:
							if (indicator._isIndicatorActive)
							{
								indicator.UpdateTick_ProcessIndicatorPosition(_characterBehaviourRef.transform.position,
									_playerControllerRef.InputResult_InputPositionOnFloor ??
									_characterBehaviourRef.transform.position,
									RecordedAttackDirection ?? _characterBehaviourRef.GetCurrentPlayerFaceDirection());
							}
							break;
					}
				}
			}
			for (int i = 0; i < AllVFXInfoList.Count; i++)
			{
				AllVFXInfoList[i].UpdateTick(currentTime, currentFrameCount, delta);
			}
		}


		public virtual void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			SkillCDTick(delta);

			if (PACConfigInfo != null && PACConfigInfo.AllAECList_Runtime.Count > 0)
			{
				foreach (var perEC in PACConfigInfo.AllAECList_Runtime)
				{
					switch (perEC)
					{
						case PAEC_开始一段位移_StartDisplacement paec开始一段位移StartDisplacement:
							paec开始一段位移StartDisplacement.FixedUpdateTick_ProcessDisplacement(currentTime,
								delta,
								_characterBehaviourRef);
							break;
					}
				}
			}

			_Internal_TryInputToPrepareTick();

			switch (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType)
			{
				case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中:
					SelfPlayerSkillAnimationMotion._prepare_InChargeTime += delta;
					if (SelfPlayerSkillAnimationMotion._prepare_InChargeTime >
					    SelfPlayerSkillAnimationMotion._Prepare_MinChargeTime)
					{
						//已经取消蓄力了，满足了最小时间。那就直接释放掉
						if (SelfPlayerSkillAnimationMotion._prepareAnimation_ChargeMarkedAsRelease)
						{
							_InternalProgress_ProgressToMiddle();
							return;
						}
						//还没有满足，那就接着续
					}
					//已经超出最大蓄力时间了，那就直接释放掉
					if (SelfPlayerSkillAnimationMotion._prepare_InChargeTime >
					    SelfPlayerSkillAnimationMotion._Prepare_MaxChargeTime)
					{
						SelfPlayerSkillAnimationMotion._prepareAnimation_ChargeMarkedAsRelease = true;
						_InternalProgress_ProgressToMiddle();
					}
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
					SelfPlayerSkillAnimationMotion._prepareAnimation_WaitingForTargetPoint_InWaitingTime += delta;
					if (SelfPlayerSkillAnimationMotion._prepareAnimation_WaitingForTargetPoint_IncludeMaxWaitingTime &&
					    SelfPlayerSkillAnimationMotion._prepareAnimation_WaitingForTargetPoint_InWaitingTime >
					    SelfPlayerSkillAnimationMotion._prepareAnimation_WaitingForTargetPoint_MaxWaitingTime)
					{
						_InternalProgress_ProgressToMiddle();
					}
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
					SelfPlayerSkillAnimationMotion._Middle_EffectingTime += delta;
					//持续时间已经超过最大时间了，向后流转
					if (SelfPlayerSkillAnimationMotion._Middle_EffectingTime >
					    SelfPlayerSkillAnimationMotion._MiddleEffecting_MaxDuration)
					{
						_InternalProgress_ProgressToPost();
					}
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
					SelfPlayerSkillAnimationMotion._Middle_InChargeTime += delta;
					if (SelfPlayerSkillAnimationMotion._Middle_InChargeTime >
					    SelfPlayerSkillAnimationMotion._Middle_MinChargeTime)
					{
						//已经取消蓄力了，满足了最小时间。那就直接释放掉
						if (SelfPlayerSkillAnimationMotion._middleAnimation_ChargeMarkedAsRelease)
						{
							//视作技能生效
							_InternalSkillEffect_SkillDefaultTakeEffect();
							//向后流转
							_InternalProgress_ProgressToPost();
							return;
						}
						//还没有满足，那就接着续
					}
					break;
				case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇:
					break;
			}
		}


		protected virtual void _FixedUpdateTick_ProcessDisplacement(
			PAEC_开始一段位移_StartDisplacement paec开始一段位移StartDisplacement,
			float currentTime,
			float delta)
		{
			paec开始一段位移StartDisplacement.FixedUpdateTick_ProcessDisplacement(currentTime, delta, _characterBehaviourRef);
		}
		
		
		
		protected virtual void _Internal_TryInputToPrepareTick()
		{
			if (_InputHolding)
			{
				switch (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType)
				{
					case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
						//未指定，那就是不在自己这。有可能是待使用，也有可能是被断出去了。

						//进行更详细的判断，是需要开始准备了，还是在干别的

						if (!IfReactToInput())
						{
							return;
						}
						if (!_Internal_TryPrepareSkill())
						{
							return;
						}

						break;
					case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
						//处理输入确定
						_InternalProgress_ProgressToMiddle();

						break;
					case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
						break;
					case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中:

						if (!IfReactToInput_OffhandState())
						{
							return;
						}
						_SkillProgress_Offhand_PrepareSkill();
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段:
						break;
					case PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇:
						break;
				}
			}
		}


		/// <summary>
		/// <para>当前技能的CD是否能Tick</para>
		/// </summary>
		/// <returns></returns>
		protected virtual bool IfSkillCanCDTick()
		{
			if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
			    PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己)
			{
				return true;
			}
			return false;
		}


		/// <summary>
		/// <para>默认的CD Tick中，技能正在活跃和技能正在释放都不会减CD</para>
		/// </summary>
		/// <param name="delta"></param>
		protected virtual void SkillCDTick(float delta)
		{
			if (!IfSkillCanCDTick())
			{
				return;
			}
			float useCD = delta * (1f + _selfSkillCDAccelerate / 100f);
			RemainingCoolDownDuration -= useCD;
		}

#endregion


#region 初始化和添加

		/// <summary>
		/// <para>获得时初始化</para>
		/// <para>获取关联的引用。获取相关数据项；绑定输入；注入占用接口</para>
		/// <para>绑定事件：</para>
		/// </summary>
		public virtual void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			_playerControllerRef = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
			_characterBehaviourRef = parent as PlayerARPGConcreteCharacterBehaviour;
			_characterArtHelperRef = _characterBehaviourRef.GetSelfRolePlayArtHelper() as PlayerARPGArtHelper;
			_selfActionBusRef = parent.ReleaseSkill_GetActionBus();
			RelatedSkillHolderRef = skillHolderRef;
			RelatedRPSkillCaster = parent;
			SelfSkillConfigRuntimeInstance = configRuntimeInstance;



			CurrentSPConsume = configRuntimeInstance.ConcreteSkillFunction.SPConsume;
			CurrentCoolDownDuration = configRuntimeInstance.ConcreteSkillFunction.CoolDownTime;
			SkillSlot = slot;
			_spEntry = RelatedRPSkillCaster.ReleaseSkill_GetPresentDataEntry(RP_DataEntry_EnumType.CurrentSP_当前SP);
			_entry_attackSpeed =
				RelatedRPSkillCaster.ReleaseSkill_GetRelatedFloatDataEntry(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			_entry_castAccelerate =
				RelatedRPSkillCaster.ReleaseSkill_GetRelatedFloatDataEntry(RP_DataEntry_EnumType
					.SkillCastingAccelerate_技能施法额外加速);

			PACConfigInfo.BuildRuntimePAEC();
			PACConfigInfo_ExitSkill?.BuildRuntimePAEC();
			;
			BindingInput();


			foreach (var perSAI in configRuntimeInstance.ConcreteSkillFunction.SelfAnimationInfoList)
			{
				if (perSAI.OccupationInfo != null)
				{
					perSAI.OccupationInfo.RelatedInterface = this;
				}
			}

			_selfActionBusRef?.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationStart_动画通用开始,
				_ABC_OnGeneralAnimationStart);
			_selfActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件,
				_ABC_OnSpineGeneralAnimationEventWithString);
			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_OnGeneralAnimationComplete);


			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_SyncRemainingCDPartial_OnCDReduceChanged);
			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_SkillPower_技能威力计算,
				_ABC_SkillPowerBonus_);
			_selfActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_Internal_CheckIfCauseBreakAsSkillWeakBreak_OnOtherBuffInitialized);






			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
				_ABC_ProcessActiveCharacterChanged_OnCurrentCharacterChanged);
		}



		/// <summary>
		/// <para>对输入进行绑定。默认在InitOnObtain时进行</para>
		/// </summary>
		protected virtual void BindingInput()
		{
			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
			{
				return;
			}
			var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
			InputAction ia = GetTargetInputActionRef(SkillSlot);
			ia.started += _IC_ReceiveSkillInput_Start;
			ia.performed += _IC_ReceiveSkillInput_Performed;
			ia.canceled += _IC_ReceiveSkillInput_Cancel;
		}

		protected virtual InputAction GetTargetInputActionRef(SkillSlotTypeEnum slot)
		{
			InputAction_ARPG inputActionInstanceRef = GameReferenceService_ARPG.Instance.InputActionInstance;
			switch (slot)
			{
				case SkillSlotTypeEnum.SlotNormal1_常规槽位1:
					return inputActionInstanceRef.BattleGeneral.SkillNormal1;
				case SkillSlotTypeEnum.SlotNormal2_常规槽位2:
					return inputActionInstanceRef.BattleGeneral.SkillNormal2;
				case SkillSlotTypeEnum.DisplacementSkill_位移槽位:
					return inputActionInstanceRef.BattleGeneral.SkillDisplacement;
				case SkillSlotTypeEnum.UltraSkill_超杀槽位:
					return inputActionInstanceRef.BattleGeneral.SkillUltra;
			}
			return null;
		}

#endregion


#region 移除和清理

		/// <summary>
		/// 在移除前的清理工作
		/// </summary>
		public virtual DS_ActionBusArguGroup ClearBeforeRemove()
		{
			VFX_GeneralClear(true);
			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnSkillPreRemove_技能将要从Holder中移除);
			ds.ObjectArgu1 = this;
			_selfActionBusRef?.TriggerActionByType(ds);

			UnbindInput();



			_selfActionBusRef?.RemoveAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationStart_动画通用开始,
				_ABC_OnGeneralAnimationStart);
			_selfActionBusRef?.RemoveAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件,
				_ABC_OnSpineGeneralAnimationEventWithString);
			_selfActionBusRef?.RemoveAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_OnGeneralAnimationComplete);
			_selfActionBusRef?.RemoveAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_SyncRemainingCDPartial_OnCDReduceChanged);
			GlobalActionBus.GetGlobalActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.G_PC_OnCurrentUsingCharacterChanged_当前使用的角色更换,
					_ABC_ProcessActiveCharacterChanged_OnCurrentCharacterChanged);

			return ds;
		}


		protected virtual void UnbindInput()
		{
			if (SkillSlot == SkillSlotTypeEnum.None_未装备)
			{
				return;
			}
			var iar = GameReferenceService_ARPG.Instance.InputActionInstance;
			InputAction ia = GetTargetInputActionRef(SkillSlot);
			ia.started -= _IC_ReceiveSkillInput_Start;
			ia.performed -= _IC_ReceiveSkillInput_Performed;
			ia.canceled -= _IC_ReceiveSkillInput_Cancel;
		}

#endregion
#region 查询

		/// <summary>
		/// 默认的技能释放前的检查，可能的结果有：就绪、被CD阻挡、被buff阻挡、被数据项阻挡。通常还需要子类继续检查是否正在使用。
		/// </summary>
		public virtual SkillReadyTypeEnum GetSkillReadyType()
		{
			if (RemainingCoolDownDuration > 0f)
			{
				return SkillReadyTypeEnum.BlockByCD;
			}

			//////////////////////
			//首先判断沉默
			////////////////
			if (RelatedRPSkillCaster.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum.Silence_DisableAllSkill) ==
			    BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				return SkillReadyTypeEnum.BlockByBuff;
			}


			return SkillReadyTypeEnum.Ready;
		}

#endregion

#region 技能过程

#region 输入响应

		protected virtual void _IC_ReceiveSkillInput_Start(InputAction.CallbackContext context)
		{
			_InputHolding = true;
		}



		protected virtual void _IC_ReceiveSkillInput_Performed(InputAction.CallbackContext context)
		{
		}



		protected virtual void _IC_ReceiveSkillInput_Cancel(InputAction.CallbackContext context)
		{
			_InputHolding = false;
			switch (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType)
			{
				case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
					break;
				case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
					SelfPlayerSkillAnimationMotion._prepareAnimation_ChargeMarkedAsRelease = true;
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中:
					SelfPlayerSkillAnimationMotion._prepareAnimation_ChargeMarkedAsRelease = true;
					//如果已经此时已经释放掉，检查一下是不是立刻触发
					if (SelfPlayerSkillAnimationMotion._prepare_InChargeTime >
					    SelfPlayerSkillAnimationMotion._Prepare_MinChargeTime)
					{
						_InternalProgress_ProgressToMiddle();
						return;
					}
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
					break;
				case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段:
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇:
					break;
			}
		}


		/// <summary>
		/// <para>记录一下当前的输入方向和位置。不同的攻击逻辑会在不同的时机记录，所以导致了不同的攻击朝向等。</para>
		/// <para>位置是 在耗蓝CD之前，在OnSkillBeginPrepare之前，在动画成功播放之后</para>
		/// </summary>
		protected virtual void GetAndOffsetCurrentInputPositionAndDirection()
		{
			var selfPosition = _characterBehaviourRef.transform.position;
			switch (AttackDirectionType)
			{
				case WeaponAttackDirectionTypeEnum.None_未指定:
					break;
				case WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向:
					RecordedAttackDirection = _playerControllerRef.Input_InputMoveRawDirectionV3Normalized;
					RecordedAttackPosition = selfPosition + RecordedAttackDirection.Value;
					QuickFaceToRecordDirection();
					break;
				case WeaponAttackDirectionTypeEnum.PointerDirectionInstant_瞬时的输入方向:
					break;
				case WeaponAttackDirectionTypeEnum.RegisteredCharacterMovementDirection_记录的角色移动方向:
					RecordedAttackDirection = _playerControllerRef.InputResultAimDirectionRawV3;
					RecordedAttackPosition = selfPosition + RecordedAttackDirection.Value.normalized;
					break;
				case WeaponAttackDirectionTypeEnum.ControlledByHandler_由具体的Handler实现:
					break;
				case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainRegistered_记录的指针位置:
					RecordedAttackDirection = _playerControllerRef.InputResult_AimDirectionOnCurrentGameplayDirection;
					RecordedAttackPosition = _playerControllerRef.InputResult_InputPositionOnFloor;
					break;
				case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainInstant_瞬时的指针位置:
					RecordedAttackDirection = _playerControllerRef.InputResult_AimDirectionOnCurrentGameplayDirection;
					RecordedAttackPosition = _playerControllerRef.InputResult_InputPositionOnFloor;
					break;
				case WeaponAttackDirectionTypeEnum.CharacterPosition_角色位置:
					RecordedAttackPosition = selfPosition;
					RecordedAttackDirection = _characterBehaviourRef.GetSelfRolePlayArtHelper().CurrentFaceLeft
						? BaseGameReferenceService.CurrentBattleLogicLeftDirection
						: BaseGameReferenceService.CurrentBattleLogicRightDirection;
					break;
				case WeaponAttackDirectionTypeEnum.RegisteredCharacterMoveDirectionThenPointer_记录的角色移动方向后指针:
					if (_playerControllerRef.InputDirect_InputMoveRaw.sqrMagnitude > 0.1f)
					{
						RecordedAttackDirection = _playerControllerRef.Input_InputMoveRawDirectionV3Normalized;
					}
					else
					{
						RecordedAttackDirection = _playerControllerRef
							.InputResult_AimDirectionOnCurrentGameplayDirectionNormalized;
					}
					RecordedAttackPosition = selfPosition + RecordedAttackDirection.Value;
					break;
			}
			QuickFaceToRecordDirection();
		}


		protected Vector3 GetValidPositionInRange(float maxRange)
		{
			var currentInputPosition = _playerControllerRef.InputResult_InputLogicPosition;
			var distance = Vector3.Distance(currentInputPosition, _characterBehaviourRef.transform.position);
			Vector3 indicatorPos = currentInputPosition;
			if (distance > maxRange)
			{
				var currentSelfPosY0 = _characterBehaviourRef.transform.position;
				currentSelfPosY0.y = 0f;
				var input_y0 = _playerControllerRef.InputResult_InputLogicPosition;
				input_y0.y = 0f;
				var dir = (input_y0 - currentSelfPosY0).normalized;
				var clampPos = _characterBehaviourRef.transform.position + dir * maxRange;
				indicatorPos = clampPos;
			}
			indicatorPos = SubGameplayLogicManager_ARPG.Instance.GetAlignedTerrainPosition(indicatorPos) ??
			               indicatorPos;
			return indicatorPos;
		}

#endregion



#region 使用   ——   前摇部分

		/// <summary>
		/// <para>检查需要消耗的数据是否足够。 默认检查的是SP消耗量</para>
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckIfDataEntryEnough()
		{
			return _spEntry.GetCurrentValue() >= CurrentSPConsume;
		}


		/// <summary>
		/// <para>当前是否应该响应输入的？</para>
		/// <para>包含：当前操作的角色是自己   |  技能已经准备就绪  |   消耗足够   |   游戏状态正常运行</para>
		/// </summary>
		/// <returns></returns>
		protected virtual bool IfReactToInput(
			bool checkCurrentActivePlayer = true,
			bool checkDataEntryEnough = true,
			bool checkReadyType = true,
			bool checkRunningState = true)
		{
			if (checkCurrentActivePlayer)
			{
				if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
				{
					return false;
				}
			}
			if (checkReadyType)
			{
				if (GetSkillReadyType() != SkillReadyTypeEnum.Ready)
				{
					return false;
				}
			}
			if (checkDataEntryEnough)
			{
				if (!CheckIfDataEntryEnough())
				{
					return false;
				}
			}
			if (checkRunningState)
			{
				switch (BaseGameReferenceService.GameRunningState)
				{
					case BaseGameReferenceService.GameRunningStateTypeEnum.None_未指定:
					case BaseGameReferenceService.GameRunningStateTypeEnum.Loading_加载中:
					case BaseGameReferenceService.GameRunningStateTypeEnum.Paused_暂停:
						return false;
				}
			}

			return true;
		}


		/// <summary>
		/// <para>技能开始准备。</para>
		/// </summary>
		/// <returns></returns>
		protected virtual DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
		{
			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnSkillBeginPrepare_技能开始进行准备);
			ds.ObjectArgu1 = this;
			if (autoLaunch)
			{
				_selfActionBusRef.TriggerActionByType(ds);
			}
			return ds;
		}

		/// <summary>
		/// <para>主动停止一个技能。对于施法中|作用中的技能来说，如果不做额外要求则都可以被停止。</para>
		/// </summary>
		protected virtual DS_ActionBusArguGroup Internal_OnSkillManualCancel(bool autoLaunch = true)
		{
			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnSkillCancelHolding_技能主动取消);
			ds.ObjectArgu1 = this;
			if (autoLaunch)
			{
				_selfActionBusRef.TriggerActionByType(ds);
			}
			return ds;
		}

		/// <summary>
		/// <para>试图准备技能。 </para>
		/// </summary>
		/// <returns></returns>
		public virtual bool _Internal_TryPrepareSkill()
		{
			 
			
			//试图占用
			_InputHolding = false;
			var ani = GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_PrepareAnimationName);
			var ds_ani = new DS_ActionBusArguGroup(ani,
				SelfPlayerSkillAnimationMotion._prepareAnimation_PlayOptionsFlagTypeEnum,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);

			//乘算速度
			ds_ani.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByCastingAccelerate
					? SelfPlayerSkillAnimationMotion._prepareAnimation_CastingAccelerateMultiplier : 0f,
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByAttackSpeed ? SelfPlayerSkillAnimationMotion
					._prepareAnimation_AttackSpeedAccelerateMultiplier : 0f);
			RelatedRPSkillCaster.ReleaseSkill_GetActionBus().TriggerActionByType(ds_ani);
			//ds2是动画播放结果
			var ds_result = ds_ani.GetObj2AsT<RP_DS_AnimationPlayResult>();
			//被阻塞，那并不能进行这个动画
			if (ds_result.PlayBlockedByOccupation)
			{
				return false;
			}
			GetAndOffsetCurrentInputPositionAndDirection();

			OnSkillConsumeSP();
			OnSkillSetCoolDown();

			OnSkillBeginPrepare();

			var ds_prepare = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnSkillBeginPrepare_技能开始进行准备);
			ds_prepare.ObjectArgu1 = this;
			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_prepare);




			SelfPlayerSkillAnimationMotion._prepare_InChargeTime = 0f;
			SelfPlayerSkillAnimationMotion._Middle_EffectingTime = 0f;
			SelfPlayerSkillAnimationMotion._prepareAnimation_WaitingForTargetPoint_InWaitingTime = 0f;
			SelfPlayerSkillAnimationMotion._prepareAnimation_ChargeMarkedAsRelease = false;


			if (SelfPlayerSkillAnimationMotion._Prepare_IsCharging)
			{
				SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
					PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中;
			}
			else
			{
				SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
					PlayerAnimationMotionProgressTypeEnum.Prepare_前摇;
			}

			PACConfigInfo_ProgressToPrepare?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);



			//占用成功播放了
			return true;
		}









		protected virtual float GetLogicOffsetAnimationPlaySpeed(float castMul, float attackMul)
		{
			float result = 1f;
			float bonus = 0f;
			bonus += castMul * _entry_castAccelerate.GetCurrentValue();
			bonus += attackMul * _entry_attackSpeed.GetCurrentValue();

			result += bonus / 100f;
			return result;
		}

#endregion


#region 使用  ——  使用中

		protected virtual DS_ActionBusArguGroup OnSkillConsumeSP()
		{
			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnSKillReadyToConsumeSP_技能将要消耗SP);

			CurrentSPConsume = SPConsume;
			ds.ObjectArgu1 = this;
			_selfActionBusRef.TriggerActionByType(ds);

			_spEntry.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(-CurrentSPConsume,
				RPDM_DataEntry_ModifyFrom.FromSkill_技能,
				ModifyEntry_CalculatePosition.FrontAdd,
				RelatedRPSkillCaster));


			return ds;
		}



		/// <summary>
		/// <para>设置技能的冷却CD。</para>
		/// </summary>
		/// <returns></returns>
		protected virtual DS_ActionBusArguGroup OnSkillSetCoolDown()
		{
			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnSkillCoolDownReset_技能重新开始计CD了);

			CurrentCoolDownDuration = CoolDownTime;
			ds.ObjectArgu1 = this;
			_selfActionBusRef.TriggerActionByType(ds);

			RemainingCoolDownDuration = CurrentCoolDownDuration;
			return ds;
		}

#endregion

#region 使用   ——  结算

		/// <summary>
		/// <para>在技能结束后回到战斗待机</para>
		/// </summary>
		protected virtual void ReturnToIdleAfterSkill(bool checkOccupy = false)
		{
			var ds_returnIdleAni = new DS_ActionBusArguGroup("战斗待机",
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterArtHelperRef.SelfAnimationPlayResult,
				checkOccupy,
				FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);
			_selfActionBusRef.TriggerActionByType(ds_returnIdleAni);
			if (ds_returnIdleAni.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				DBug.LogError($" 技能{SkillType}在结束时回到待机时被占用了，这并不合理，检查一下");
				return;
			}
		}

#endregion

#endregion



#region 动画事件监听响应

		protected virtual void ExecuteAnimationEventCallback(BasePlayerAnimationEventCallback eac)
		{
			switch (eac)
			{
				case PAEC_播放Timeline特效_PlayTimelineVFX paec播放Timeline特效PlayTimelineVFX:
					PAEC_SpawnTimeline(paec播放Timeline特效PlayTimelineVFX);
					break;
				case PAEC_开始一段位移_StartDisplacement paec开始一段位移StartDisplacement:
					PAEC_StartDisplacement(paec开始一段位移StartDisplacement, RecordedAttackDirection.Value);
					break;
				case PAEC_生成版面_SpawnLayout paec生成版面SpawnLayout:
					PAEC_SpawnLayout(paec生成版面SpawnLayout);
					break;
				case PAEC_生成特效配置_SpawnVFXFromConfig paec生成特效配置SpawnVFXFromConfig:
					PAEC_SpawnVFXFromConfig(paec生成特效配置SpawnVFXFromConfig);
					break;
				case PAEC_生成游戏事件_SpawnGameplayEvent paec生成游戏事件SpawnGameplayEvent:
					eac.ExecuteBySkill(RelatedRPSkillCaster as BaseARPGCharacterBehaviour, this);
					break;
				case PAEC_停止特效配置_StopVFXFromConfig paec停止特效配置StopVFXFromConfig:
					eac.ExecuteBySkill(RelatedRPSkillCaster as BaseARPGCharacterBehaviour, this);
					break;
				case PAEC_生成选点指示器_SpawnPositionIndicator indicator:
					PAEC_SpawnIndicator(indicator);
					break;
				case PAEC_关闭所有指示器_DisableIndicator disable:
					foreach (BasePlayerAnimationEventCallback perAEC in PACConfigInfo.AllAECList_Runtime)
					{
						if (perAEC is not PAEC_生成选点指示器_SpawnPositionIndicator indicator)
						{
							continue;
						}
						indicator.DisableIndicator();
					}
					break;
			}
		}

		public virtual void PAEC_StartDisplacement(PAEC_开始一段位移_StartDisplacement paec , Vector3 direction)
		{
			paec.StartDisplacement(_characterBehaviourRef.transform.position, _characterBehaviourRef, direction);
		}

		protected virtual void PAEC_SpawnTimeline(PAEC_播放Timeline特效_PlayTimelineVFX paec)
		{
			var tt = paec.RelatedTimeline;
			DBug.Log($"来自技能{SelfSkillConfigRuntimeInstance.name}正在试图播放Timeline:{paec.RelatedTimeline.name}");
			//匹配伤害类型
			if (paec.MatchDamageType)
			{
				if (!paec.DamageTypeList.Contains(GetCurrentDamageType()))
				{
					return;
				}
			}
			var pd = _characterBehaviourRef.GetRelatedPlayableDirector();

			if (pd.playableAsset != null)
			{
				pd.Stop();
				pd.time = 0f;
			}
			if (!pd.gameObject.activeInHierarchy)
			{
				pd.gameObject.SetActive(true);
			}
			if (paec.UseAimDirection)
			{
				Vector3 attackDirection =
					RecordedAttackDirection ?? _characterBehaviourRef.GetCurrentPlayerFaceDirection();

				pd.transform.rotation = Quaternion.LookRotation(attackDirection, Vector3.up);
			}
			else
			{
				pd.transform.localRotation = Quaternion.identity;
			}



			pd.playableAsset = tt;
			pd.Play();
		}

		protected virtual void PAEC_SpawnLayout(PAEC_生成版面_SpawnLayout paec, bool autoLaunch = true)
		{
			var layoutRuntime = paec.RelatedConfig.SpawnLayout_NoAutoStart(_characterBehaviourRef);
			_layoutList.Add(layoutRuntime);
			paec._RuntimeLayoutRef = layoutRuntime;
			if (paec.NeedUniformTimeStamp)
			{
				layoutRuntime.LayoutHandlerFunction.PresetNeedUniformTimeStamp = true;
				layoutRuntime.LayoutHandlerFunction.UniformTimeStamp = Time.frameCount + GetHashCode();
			}

			if (!paec.NotAcceptDamageTypeChange)
			{
				layoutRuntime.LayoutContentInSO.DamageApplyInfo.DamageType = _Buff_DamageTypeBuffRef.CurrentDamageType;
			}
			layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromPosition = _characterBehaviourRef.transform.position;
			layoutRuntime.LayoutHandlerFunction.OverrideSpawnFromDirection = RecordedAttackDirection ??
			                                                                 _characterBehaviourRef
				                                                                 .GetCurrentPlayerFaceDirection();
			if (autoLaunch)
			{
				layoutRuntime.LayoutHandlerFunction.StartLayout();
			}
		}


		protected virtual void PAEC_SpawnVFXFromConfig(PAEC_生成特效配置_SpawnVFXFromConfig paec)
		{
			var vfxInfo = _VFX_GetAndSetBeforePlay(paec._vfx_UIDToPlay,
				true,
				paec._vfx_IncludeDamageVariant,
				GetCurrentDamageType);
			if (vfxInfo == null)
			{
				return;
			}
			paec.RuntimeVFXInfoRef = vfxInfo;

			if (paec._vfx_IncludeExtraAlign)
			{
				var direction = RecordedAttackDirection ?? _characterBehaviourRef.GetCasterForwardDirection();
				if (paec._vfx_AlignByForwardOrRight)
				{
					if (paec._vfx_AlignToWorld)
					{
						vfxInfo._VFX__3_SetDirectionOnForwardOnGlobalY0(direction);
					}
					else
					{
						vfxInfo._VFX__3_SetDirectionOnForwardOnLocal(direction);
					}
				}
				else
				{
					if (paec._vfx_AlignToWorld)
					{
						vfxInfo._VFX__3_SetDirectionOnRightOnGlobalY0(-direction);
					}
					else
					{
						vfxInfo._VFX__3_SetDirectionOnRightOnLocalY0(-direction);
					}
				}
			}

			vfxInfo._VFX__10_PlayThis(true, false);
		}

		/// <summary>
		/// <para>基本基类没有任何作用，需要使用Active那个函数</para>
		/// </summary>
		/// <param name="indicator"></param>
		protected virtual void PAEC_SpawnIndicator(PAEC_生成选点指示器_SpawnPositionIndicator indicator)
		{
			indicator.ExecuteBySkill(_characterBehaviourRef, this, false);
		}

#endregion

#region 通用事件

		/// <summary>
		/// <para>obj1是ArtHelper，obj2是AnimationInfo，objStr是动画配置名</para>
		/// </summary>
		/// <param name="ds"></param>
		protected virtual void _ABC_OnGeneralAnimationStart(DS_ActionBusArguGroup ds)
		{
			var configName = ds.GetObj2AsT<AnimationInfoBase>().ConfigName;
			if (!IfAnimationConfigBelongSelf(configName))
			{
				return;
			}
			if (PACConfigInfo != null && PACConfigInfo.AllAECList_Runtime.Count > 0)
			{
				foreach (var perEC in PACConfigInfo.AllAECList_Runtime)
				{
					if (perEC.AnimationEventPreset == PlayerAnimationEventPresetEnumType.Start_开始 &&
					    perEC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArgu2 as AnimationInfoBase).ConfigName,
						    StringComparison.OrdinalIgnoreCase))
					{
						ExecuteAnimationEventCallback(perEC);
					}
				}
			}
		}

		/// <summary>
		/// <para>发出了通用的 带字符串的动画事件</para>
		/// <para>objStr 是Spine动画事件名，  obj1是SpineHelper， obj2是发出事件的动画的名字</para>
		/// </summary>
		protected virtual void _ABC_OnSpineGeneralAnimationEventWithString(DS_ActionBusArguGroup ds)
		{
			var configName = (ds.ObjectArguStr as AnimationInfoBase).ConfigName;
			if (!IfAnimationConfigBelongSelf(configName))
			{
				return;
			}
			if (PACConfigInfo != null && PACConfigInfo.AllAECList_Runtime.Count > 0)
			{
				foreach (var perEC in PACConfigInfo.AllAECList_Runtime)
				{
					var configNameInEC = perEC._AN_RelatedAnimationConfigName;
					var eventNameInEC = perEC.CustomEventString;

					if (perEC.AnimationEventPreset == PlayerAnimationEventPresetEnumType.Custom_自定义)
					{
						var eventNameInAnimation = ds.ObjectArgu2 as string;
						if (configName.Equals(configNameInEC, StringComparison.OrdinalIgnoreCase) &&
						    eventNameInAnimation.Equals(eventNameInEC, StringComparison.OrdinalIgnoreCase))
						{
							ExecuteAnimationEventCallback(perEC);
						}
					}
					else
					{
						continue;
					}
				}
			}
		}



		/// <summary>
		/// <para>通用的技能动画结束Spine事件。Spine所有动画结束都是这个事件，所以要内部判定是不是自己</para>
		/// <para> objStr 是配置的名字(AnimationInfo 的 Config名字，【比对】)， obj1是关联的AnimationArtHelper,obj2是使用的AnimationInfo</para>
		/// </summary>
		protected virtual void _ABC_OnGeneralAnimationComplete(DS_ActionBusArguGroup ds)
		{
			var configName = ds.GetObj2AsT<AnimationInfoBase>().ConfigName;
			if (!IfAnimationConfigBelongSelf(configName))
			{
				return;
			}

			if (PACConfigInfo.AllAECList_Runtime != null && PACConfigInfo.AllAECList_Runtime.Count > 0)
			{
				foreach (var perEC in PACConfigInfo.AllAECList_Runtime)
				{
					if (perEC.AnimationEventPreset == PlayerAnimationEventPresetEnumType.Complete_结束 &&
					    perEC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArgu2 as AnimationInfoBase).ConfigName,
						    StringComparison.OrdinalIgnoreCase))
					{
						ExecuteAnimationEventCallback(perEC);
					}
				}
			}

			switch (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType)
			{
				case PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己:
					break;
				case PlayerAnimationMotionProgressTypeEnum.Prepare_前摇:
					_InternalProgress_CompleteOn_Prepare(ds.ObjectArguStr as string,
						ds.GetObj2AsT<SheetAnimationInfo_帧动画配置>(),
						ds.GetObj1AsT<BaseCharacterSheetAnimationHelper>());
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中:
					//前摇蓄力是个循环动画，它在重复时的Complete没有任何意义，这里的业务 应当去找Tick那里，计时的。
					break;
				case PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中:
					//前摇等待，这时候应当是等待输入的，它在重复时的Complete没有意义。这里的业务去等Input就行了
					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法:
					//中段持续施法的动画结束了，则此时技能流转到下一阶段。并且算作“技能生效”
					_InternalSkillEffect_SkillDefaultTakeEffect();

					_InternalProgress_ProgressToPost();

					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中:
					// 中段持续施法 是个循环动画，它在重复时的Complete没有意义，等Input就行了

					break;
				case PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中:
					//中段作用中 的动画循环没有意义，它是一个计时的事情

					break;
				case PlayerAnimationMotionProgressTypeEnum.Post_后摇:
					//后摇结束的时候
					_InternalProgress_Post_CompleteOn_Post();
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇:
					_InternalProgress_Offhand_CompleteOnPrepare();
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段:
					_InternalProgress_Offhand_CompleteOnMiddle();
					break;
				case PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇:
					_InternalProgress_Offhand_CompleteOnPost();
					break;
			}
		}


		/// <summary>
		/// <para>当【CD减免】数据项发生变动时，同步当前的剩余CD到合适比例</para>
		/// </summary>
		/// <param name="ds"></param>
		protected virtual void _ABC_SyncRemainingCDPartial_OnCDReduceChanged(DS_ActionBusArguGroup ds)
		{
			if (!ds.IntArgu1.HasValue ||
			    (RP_DataEntry_EnumType)ds.IntArgu1.Value != RP_DataEntry_EnumType.SkillCDReduce_技能CD缩减)
			{
				return;
			}
			var targetEntry = ds.ObjectArgu1 as Float_RPDataEntry;

			float currentReduce = targetEntry.GetCurrentValue();
			currentReduce = Mathf.Clamp(currentReduce, 0f, 0.95f);
			float remainPartial = 1f - currentReduce;
			//e.g.，如果减免百分之25，则首先计CD的时候使用的CD值就是原本75%的
			CurrentCoolDownDuration = SelfSkillConfigRuntimeInstance.ConcreteSkillFunction.CoolDownTime * remainPartial;
			//如果当前剩余CD大于0，则表明技能正在CD中，需要同步一下
			if (RemainingCoolDownDuration > 0f)
			{
				RemainingCoolDownDuration *= remainPartial;
			}
		}



		/// <summary>
		/// <para>当 当前操作的角色 发生变动时， 处理一些情况。通常会清理特效。</para>
		/// <para> 此处并不会检查是否真的换人了之类的，这是 【确定】换人了之后响应的那个回调，所以肯定是换人了。</para>
		/// </summary>
		protected virtual void _ABC_ProcessActiveCharacterChanged_OnCurrentCharacterChanged(DS_ActionBusArguGroup ds)
		{
			var outBehaviour = ds.ObjectArgu2 as PlayerARPGConcreteCharacterBehaviour;
			if (ReferenceEquals(outBehaviour, _characterBehaviourRef))
			{
				// ();
			}
		}

#endregion


#region 动画结束的流转业务

#region 前摇部分

		protected virtual void _InternalProgress_CompleteOn_Prepare(
			string configName,
			SheetAnimationInfo_帧动画配置 sheetAnimationInfo,
			BaseCharacterSheetAnimationHelper animationHelper)
		{
			//这里是正常前摇，所以检查是不是需要进入等待施法
			//包含等待，则进入等待
			if (SelfPlayerSkillAnimationMotion._Prepare_CanWaitForTargetPoint)
			{
				_InternalProgress_P_C_WaitInput();
				return;
			}
			if (SelfPlayerSkillAnimationMotion._Prepare_IsCharging)
			{
				//检查前摇的蓄力是否需要更换动画
				if (SelfPlayerSkillAnimationMotion._prepareAnimation_ChangeAnimationAfterBasePrepare)
				{
					_InternalProgress_P_B2_ChargeWithChangeAnimation();
					return;
				}
				else
				{
					_InternalProgress_P_B1_ChargeWithoutChangeAnimation();
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

		protected virtual void _InternalProgress_P_B1_ChargeWithoutChangeAnimation()
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
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中;
		}

		protected virtual void _InternalProgress_P_B2_ChargeWithChangeAnimation()
		{
			var ds_aniToPrepareCharge = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion
					._prepareAnimation_ChangeAnimationAfterChargeAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);

			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToPrepareCharge);

			ds_aniToPrepareCharge.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByCastingAccelerate
					? SelfPlayerSkillAnimationMotion._prepareAnimation_CastingAccelerateMultiplier : 0f,
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByAttackSpeed ? SelfPlayerSkillAnimationMotion
					._prepareAnimation_AttackSpeedAccelerateMultiplier : 0f);

			if (ds_aniToPrepareCharge.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在“准备”动画结束后，根据配置应当进入前摇蓄力动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.PrepareCharging_前摇蓄力中;
		}


		protected virtual void _InternalProgress_P_C_WaitInput()
		{
			var ds_aniToPrepareWait = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion
					._prepareAnimation_WaitingForTargetPointAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToPrepareWait);


			ds_aniToPrepareWait.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByCastingAccelerate
					? SelfPlayerSkillAnimationMotion._prepareAnimation_CastingAccelerateMultiplier : 0f,
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByAttackSpeed ? SelfPlayerSkillAnimationMotion
					._prepareAnimation_AttackSpeedAccelerateMultiplier : 0f);


			if (ds_aniToPrepareWait.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在“准备”动画结束后，根据配置应当进入前摇等待动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.PrepareWaiting_前摇等待中;
			return;
		}

#endregion
#region 中段部分

		protected virtual void _InternalProgress_ProgressToMiddle()
		{
			SelfPlayerSkillAnimationMotion._Middle_InChargeTime = 0f;
			 PACConfigInfo_ProgressToMiddle?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);

			//是个持续施法的
			if (SelfPlayerSkillAnimationMotion.ContinuousCasting)
			{
				//可能是可以蓄力的
				if (SelfPlayerSkillAnimationMotion._Middle_IsCharging)
				{
					_InternalProgress_M_A2_CastingWithChargeAnimation();
				}
				else
				{
					_InternalProgress_M_A1_CastingWithoutChargeAnimation();
				}
			}
			//是个作用中的
			else
			{
				_InternalProgress_M_B1_Effecting();
			}
		}


		protected virtual void _InternalProgress_M_A1_CastingWithoutChargeAnimation()
		{
			var ds_aniToMiddle = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToMiddle);

			ds_aniToMiddle.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				SelfPlayerSkillAnimationMotion._Middle_AccelerateByCastingAccelerate
					? SelfPlayerSkillAnimationMotion._middleAnimation_CastingAccelerateMultiplier : 0f,
				SelfPlayerSkillAnimationMotion._Middle_AccelerateByAttackSpeed ? SelfPlayerSkillAnimationMotion
					._middleAnimation_AttackSpeedAccelerateMultiplier : 0f);


			if (ds_aniToMiddle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在“准备”动画结束后，根据配置应当进入中段施法动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MiddleCasting_中段持续施法;
		}

		protected virtual void _InternalProgress_M_A2_CastingWithChargeAnimation()
		{
			var ds_aniToMiddle = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);

			ds_aniToMiddle.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				SelfPlayerSkillAnimationMotion._Middle_AccelerateByCastingAccelerate
					? SelfPlayerSkillAnimationMotion._middleAnimation_CastingAccelerateMultiplier : 0f,
				SelfPlayerSkillAnimationMotion._Middle_AccelerateByAttackSpeed ? SelfPlayerSkillAnimationMotion
					._middleAnimation_AttackSpeedAccelerateMultiplier : 0f);

			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToMiddle);
			if (ds_aniToMiddle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在“准备”动画结束后，根据配置应当进入中段施法蓄力动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}

			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MiddleCastingCharging_中段持续施法蓄力中;
		}


		protected virtual void _InternalProgress_M_B1_Effecting()
		{
			var ds_aniToMiddle = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_MiddlePartAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			ds_aniToMiddle.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				SelfPlayerSkillAnimationMotion._Middle_AccelerateByCastingAccelerate
					? SelfPlayerSkillAnimationMotion._middleAnimation_CastingAccelerateMultiplier : 0f,
				SelfPlayerSkillAnimationMotion._Middle_AccelerateByAttackSpeed ? SelfPlayerSkillAnimationMotion
					._middleAnimation_AttackSpeedAccelerateMultiplier : 0f);

			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToMiddle);
			if (ds_aniToMiddle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在“准备”动画结束后，根据配置应当进入中段作用中动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}


			_InternalSkillEffect_SkillDefaultTakeEffect();
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MiddleEffecting_中段作用中;
		}

#endregion

#region 后段部分

		/// <summary>
		/// <para>试图流转到 “后摇”部分</para>
		/// </summary>
		protected virtual void _InternalProgress_ProgressToPost()
		{
			 PACConfigInfo_ProgressToPost?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
			//看看有没有完全脱手
			//不包含完全脱手
			if (!SelfPlayerSkillAnimationMotion._Post_IncludeFullyOffHandPart)
			{
				//没有完全脱手时，当试图进入后摇，就视作【技能释放结束】
				_InternalSkillEffect_SkillDefaultFinishEffect();
			}
			//有完全脱手的部分。那还就没结束。
			else
			{
			}
			_InternalProgress_Post_();
		}


		/// <summary>
		/// <para>流转到后摇。</para>
		/// <para>如果还有完全脱手的部分，则需要再去处理_InternalProgress_Post_CompleteOn_Post。这里是一般情形都会流转到后摇的 </para>
		/// </summary>
		protected virtual void _InternalProgress_Post_()
		{
			var ds_aniToPost = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_PostPartAnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);

			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToPost);
			if (ds_aniToPost.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在中段动画结束后，根据配置应当进入后摇动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}

			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.Post_后摇;
		}



		/// <summary>
		/// <para>后摇结束了</para>
		/// <para>如果有多段脱手部分， 则状态将变为后摇脱手等待，否则就回到 nonSelf并idle</para>
		/// </summary>
		protected virtual void _InternalProgress_Post_CompleteOn_Post()
		{
			//包含多段脱手
			if (SelfPlayerSkillAnimationMotion._Post_IncludeFullyOffHandPart)
			{
				ReturnToIdleAfterSkill();
				SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
					PlayerAnimationMotionProgressTypeEnum.MultiPostOffhandWaiting_后摇脱手等待中;
			}
			//不包含多段脱手，则返回idle
			else
			{
				ReturnToIdleAfterSkill();
				SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
					PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
			}
		}

#endregion
#region 技能效果

		/// <summary>
		/// <para>技能默认的“产生效果”</para>
		/// <para>对于【施法中】的技能，会在施法阶段结束后“产生效果”</para>
		/// <para>对于【作用中】的技能，在进入作用中时就“产生效果”</para>
		/// </summary>
		protected virtual void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
		}

		/// <summary>
		/// <para>技能在【多段脱手】状态下的产生效果。</para>
		/// </summary>
		protected virtual void _InternalSkillEffect_SkillTakeEffect_OnMultiOffhandPart()
		{
		}



		/// <summary>
		/// <para>技能默认的“结束”</para>
		/// <para></para>
		/// </summary>
		protected virtual void _InternalSkillEffect_SkillDefaultFinishEffect()
		{
			var ds_finish = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Skill_OnSkillDefaultFinish_技能默认结束了);
			ds_finish.ObjectArgu1 = this;
			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_finish);
		}

#endregion
#region 多段脱手

		/// <summary>
		/// <para>在脱手状态下，当前是否响应输入</para>
		/// <para>不总是会响应，比如内置CD、没到可以响应的时间点、layoutHandler还没到某种状态</para>
		/// </summary>
		/// <returns></returns>
		protected virtual bool IfReactToInput_OffhandState()
		{
			return IfReactToInput();
		}



		/// <summary>
		/// <para>多段脱手中，试图流转到前摇。</para>
		/// </summary>
		/// <returns></returns>
		protected virtual bool _SkillProgress_Offhand_PrepareSkill()
		{
			//试图占用
			var ds_ani = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_MultiPart_Prepare_AnimationName),
				SelfPlayerSkillAnimationMotion._prepareAnimation_PlayOptionsFlagTypeEnum,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断);

			//乘算速度
			ds_ani.FloatArgu2 = GetLogicOffsetAnimationPlaySpeed(
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByCastingAccelerate
					? SelfPlayerSkillAnimationMotion._prepareAnimation_CastingAccelerateMultiplier : 0f,
				SelfPlayerSkillAnimationMotion._Prepare_AccelerateByAttackSpeed ? SelfPlayerSkillAnimationMotion
					._prepareAnimation_AttackSpeedAccelerateMultiplier : 0f);
			RelatedRPSkillCaster.ReleaseSkill_GetActionBus().TriggerActionByType(ds_ani);
			//ds2是动画播放结果
			var ds_result = ds_ani.GetObj2AsT<RP_DS_AnimationPlayResult>();
			//被阻塞，那并不能进行这个动画
			if (ds_result.PlayBlockedByOccupation)
			{
				return false;
			}
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MultiPrepare_多段前摇;
			//占用成功播放了
			return true;
		}


		/// <summary>
		/// 多段脱手部分，的前摇结束了，将会自动流转到中段
		/// </summary>
		/// <returns></returns>
		protected virtual void _InternalProgress_Offhand_CompleteOnPrepare()
		{
			var ds_aniToMiddle = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_MultiPart_Middle_AnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToMiddle);
			if (ds_aniToMiddle.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在“准备”动画结束后，根据配置应当进入中段作用中动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}


			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MultiMiddle_多段中段;
		}



		/// <summary>
		/// <para>多短脱手。在多段脱手的中段动画【必定是施法中】结束时，默认操作是技能再次生效，并且流转到多段后摇</para>
		/// </summary>
		protected virtual void _InternalProgress_Offhand_CompleteOnMiddle()
		{
			var ds_aniToPost = new DS_ActionBusArguGroup(
				GetAnimationInfoByConfigName(SelfPlayerSkillAnimationMotion._ancn_MultiPart_Post_AnimationName),
				AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SelfAnimationPlayResult,
				true,
				FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
			_characterBehaviourRef.ReleaseSkill_GetActionBus().TriggerActionByType(ds_aniToPost);
			if (ds_aniToPost.GetObj2AsT<RP_DS_AnimationPlayResult>().PlayBlockedByOccupation)
			{
				//被阻塞了。这是不合理的，应当能够进入
				DBug.LogError($"{_characterBehaviourRef}的技能{SkillType}，在中段动画结束后，根据配置应当进入后摇动画，但是播放它的动画被阻塞了，这不合理");
				return;
			}
			_InternalSkillEffect_SkillTakeEffect_OnMultiOffhandPart();
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.MultiAfter_多段后摇;
		}


		/// <summary>
		/// <para>多段脱手部分。后摇动画结束了。</para>
		/// <para>根据具体情况的不同，它可能是技能结束，也可能单纯就是多段中的一段</para>
		/// </summary>
		protected virtual void _InternalProgress_Offhand_CompleteOnPost()
		{
			ReturnToIdleAfterSkill();
		}

#endregion

#endregion

#region 特效相关

		public PerVFXInfo _VFX_GetAndSetBeforePlay(
			string uid,
			bool needApplyTransformOffset = true,
			bool withDamageTypeVariant = false,
			PerVFXInfo.GetDamageTypeDelegate getDamageType = null,
			string from = null)
		{
			PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList,
				uid,
				withDamageTypeVariant,
				getDamageType,
				from);
			return selfVFXInfo?._VFX_GetPSHandle(needApplyTransformOffset,
				_characterBehaviourRef.GetRelatedVFXContainer());
		}

		public PerVFXInfo _VFX_StopAllSameUid(string uid, bool stopImmediate)
		{
			PerVFXInfo t = null;
			for (int i = 0; i < AllVFXInfoList.Count; i++)
			{
				if (uid == AllVFXInfoList[i]._VFX_InfoID)
				{
					AllVFXInfoList[i]?.VFX_StopThis(stopImmediate);
				}
			}
			return t;
		}

		public PerVFXInfo _VFX_JustGet(string uid)
		{
			PerVFXInfo selfVFXInfo = PerVFXInfo._VFX_GetByUID(AllVFXInfoList, uid, false);
			return selfVFXInfo;
		}

		public PerVFXInfo _VFX_GetAndSetBeforePlay(PerVFXInfo info, bool needApplyTransformOffset = true)
		{
			return info._VFX_GetPSHandle(needApplyTransformOffset, _characterBehaviourRef.GetRelatedVFXContainer());
		}

		public virtual void VFX_GeneralClear(bool immediate = false)
		{
			PerVFXInfo.VFX_GeneralClear(AllVFXInfoList, immediate);
		}

		public virtual void VFX_GeneralStop()
		{
			PerVFXInfo.VFX_StopAll(AllVFXInfoList);
		}


		/// <summary>
		/// <para>获取当前表现出来的伤害类型</para>
		/// </summary>
		public virtual DamageTypeEnum GetCurrentDamageType()
		{
			if (_Buff_DamageTypeBuffRef == null)
			{
				return DamageTypeEnum.NoType_无属性;
			}

			return _Buff_DamageTypeBuffRef.CurrentDamageType;
		}

#endregion

#region 动画相关

		/// <summary>
		/// <para>通过动画配置名来查找Spine动画信息</para>
		/// </summary>
		public AnimationInfoBase GetAnimationInfoByConfigName(string configName)
		{
			var index = SelfSkillConfigRuntimeInstance.ConcreteSkillFunction.SelfAnimationInfoList.FindIndex(x =>
				string.Equals(configName, x.ConfigName, StringComparison.OrdinalIgnoreCase));
			;
			if (index == -1)
			{
				DBug.LogError($"技能{SelfSkillConfigRuntimeInstance.name}内部查找动画配置:{configName} 时，并没有查找到。" +
				              $"匹配的是【动画配置名】，不是【动画名】。是检视面板中浅绿色的那个，在【动画配置信息组们】那里");
				return null;
			}


			return SelfSkillConfigRuntimeInstance.ConcreteSkillFunction.SelfAnimationInfoList[index];
		}


		/// <summary>
		/// <para>检查该动画配置是否归属于自己。三个动画响应不会响应不归属于自己的动画信息</para>
		/// </summary>
		/// <returns></returns>
		protected virtual bool IfAnimationConfigBelongSelf(string configName)
		{
			return SelfPlayerSkillAnimationMotion.ContainsAnimationConfig(configName);
		}

#endregion


#region 占用、取消、打断

		/// <summary>
		/// <para>一个正常的占用被打断。</para>
		/// <para>占用被打断是很正常的情况，动画自然接续都是占用被打断，异常状态、硬直之类的都是如此。</para>
		/// <para>调用源是常规Behaviour在播放动画前的检查</para>
		/// </summary>
		public virtual FixedOccupiedCancelTypeEnum OnOccupiedCanceledByOther(
			DS_OccupationInfo occupySourceInfo,
			FixedOccupiedCancelTypeEnum explicitType = FixedOccupiedCancelTypeEnum.None_未指定,
			bool invokeBreakResult = true)
		{
			//有显式的取消类型传入了，则使用传入的逻辑
			//某些时刻的【打断】并不是因为动画变动导致的打断，而是因为需要打断才导致动画变动，后者不会经由GetFixedOccupiedCancelType获得类型信息
			//        比如释放时被沉默。

			//所以如果指定了类型，就不需要再去根据OccupationInfo去获取类型了，直接使用并处理即可。


			//需要对 OtherBreak进行一点额外的检查，因为有可能其实就还是自己的动画，但是由于架构设置，所有的前摇都会被当做OtherBreak。
			//则如果这个OtherBreak就是自己的动画，则将类型转换为 ContinueBreak;
			if (explicitType == FixedOccupiedCancelTypeEnum.Logic_OtherBreak_机制其他动画打断)
			{
				if (IfAnimationConfigBelongSelf(occupySourceInfo.OccupationInfoConfigName))
				{
					explicitType = FixedOccupiedCancelTypeEnum.ContinueBreak_接续断;
				}
				else if (SelfPlayerSkillAnimationMotion._Post_IncludeFullyOffHandPart)
				{
					if (occupySourceInfo.OccupationInfoConfigName.Contains("待机"))
					{
						explicitType = FixedOccupiedCancelTypeEnum.ContinueBreak_接续断;
					}
				}
			}
			else if (explicitType == FixedOccupiedCancelTypeEnum.ContinueBreak_接续断)
			{
				if (!IfAnimationConfigBelongSelf(occupySourceInfo.OccupationInfoConfigName))
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


		protected virtual void ProcessBreakResultByOccupiedCancelType(FixedOccupiedCancelTypeEnum explicitType ,
			DS_OccupationInfo  occupySourceInfo)
		{

			switch (explicitType)
			{
				case FixedOccupiedCancelTypeEnum.None_未指定:
					DBug.LogWarning(
						$"技能{SelfSkillConfigRuntimeInstance.name}处理了一个 未显示指定 打断类型的动画占用，来自{occupySourceInfo.OccupationInfoConfigName}。已按照机制断处理，可以检查一下");
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
					if (SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType ==
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

		/// <summary>
		/// <para>用于检查刚刚添加的buff是否造成了[技能弱断]的效果</para>
		/// <para>由于 沉默|缴械 这种弱霸体抵抗的效果并不会有对应的动画，所以实际的逻辑是由于沉默|缴械被技能监听，而立刻由技能内部转换为了idle状态而处理，并非由于沉默|缴械变成idle而进行了“断”的业务</para>
		/// <para></para>
		/// </summary>
		protected virtual void _ABC_Internal_CheckIfCauseBreakAsSkillWeakBreak_OnOtherBuffInitialized(
			DS_ActionBusArguGroup ds)
		{
			RolePlay_BuffTypeEnum newBuffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			switch (newBuffType)
			{
				case RolePlay_BuffTypeEnum.Silence_沉默_CM:
					OnOccupiedCanceledByOther(
						_characterBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
							._Cache_ANInfo_BattleIdle.OccupationInfo,
						FixedOccupiedCancelTypeEnum.WeakBreak_弱断);
					ReturnToIdleAfterSkill();
					break;
			}
		}


		/// <summary>
		/// <para>检查打断源是不是技能自己。</para>
		/// <para>在检查时，会在“取消”之前检查，所以虽然取消也是自己的动画，但是分类时会被分到 取消断，其他的动画会被处理为 “接续断”</para>
		/// <para>检查的是自己的动画列表。</para>
		/// </summary>
		protected virtual bool _Internal_CheckIfBreakAnimationIsSelf(DS_OccupationInfo oInfo)
		{
			foreach (AnimationInfoBase perANI in SelfAnimationInfoList)
			{
				if (perANI.OccupationInfo.OccupationInfoConfigName.Equals(oInfo.OccupationInfoConfigName,
					StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

#region 打断结果

		protected virtual void BR_ResetAllPAMContent()
		{
			SelfPlayerSkillAnimationMotion.PlayerAnimationMotionProgressType =
				PlayerAnimationMotionProgressTypeEnum.NoneSelf_不是自己;
			SelfPlayerSkillAnimationMotion._prepare_InChargeTime = 0f;
			SelfPlayerSkillAnimationMotion._middleAnimation_ChargeMarkedAsRelease = false;
			SelfPlayerSkillAnimationMotion._prepareAnimation_ChargeMarkedAsRelease = false;
			SelfPlayerSkillAnimationMotion._Middle_InChargeTime = 0f;
			SelfPlayerSkillAnimationMotion._Middle_EffectingTime = 0f;
			SelfPlayerSkillAnimationMotion._prepareAnimation_WaitingForTargetPoint_InWaitingTime = 0f;
		}

		protected virtual void BR_DisableAllIndicator()
		{
			if (PACConfigInfo != null && PACConfigInfo.AllAECList_Runtime.Count > 0)
			{
				foreach (var per in PACConfigInfo.AllAECList_Runtime)
				{
					if (per is not PAEC_生成选点指示器_SpawnPositionIndicator indicator)
					{
						continue;
					}
					indicator.DisableIndicator();
				}
			}
		}

		protected virtual void BR_StopAllRelatedLayout()
		{
			for (int i = _layoutList.Count - 1; i >= 0; i--)
			{
				if (_layoutList[i] == null)
				{
					_layoutList.RemoveAt(i);
				}
			}
			for (int i = _layoutList.Count - 1; i >= 0; i--)
			{
				_layoutList[i].StopLayout();
			}
		}

		protected virtual void BR_ClearAllRelatedLayout()
		{
			for (int i = _layoutList.Count - 1; i >= 0; i--)
			{
				if (_layoutList[i] == null)
				{
					_layoutList.RemoveAt(i);
				}
			}
			for (int i = _layoutList.Count - 1; i >= 0; i--)
			{
				_layoutList[i].LayoutHandlerFunction.ClearLayout(true);
			}
		}


		/// <summary>
		/// 通用的退出技能效果。用于硬直断、弱断、抢断、机制其他打断、机制换人。
		/// 死亡断会调用这个，并由额外操作
		/// 通用操作包含：重设所有PAM动画行为、禁用所有指示器、停止所有相关layout、清除所有特效
		/// </summary>
		public virtual void BR_CommonExitEffect()
		{
			BR_ResetAllPAMContent();
			BR_DisableAllIndicator();
			BR_StopAllRelatedLayout();
			VFX_GeneralClear();
			PACConfigInfo_ExitSkill?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}

		public virtual void BreakResult_StiffnessBreak()
		{
			BR_CommonExitEffect();
			PACConfigInfo_StiffnessBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}


		public virtual void BreakResult_WeakBreak()
		{

			BR_CommonExitEffect();
			 PACConfigInfo_WeakBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}


		public virtual void BreakResult_StrongBreak()
		{

			BR_CommonExitEffect();
			 PACConfigInfo_StrongBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}


		public virtual void BreakResult_OtherBreak()
		{

			BR_CommonExitEffect();
			 PACConfigInfo_LogicOtherBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}


		public virtual void BreakResult_ContinueBreak()
		{
			PACConfigInfo_ContinueBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}

		public virtual void BreakResult_CancelBreak()
		{

			BR_CommonExitEffect();
			 PACConfigInfo_CancelBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}

		public virtual void BreakResult_SwitchCharacter()
		{

			BR_CommonExitEffect();
			PACConfigInfo_SwitchCharacterBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
		}



		/// <summary>
		/// <para>死亡段会在BR基础断之上，包含清理所有Layout，并立刻停止所有特效</para>
		/// </summary>
		public virtual void BreakResult_DeathBreak()
		{
			BR_CommonExitEffect();
			BR_ClearAllRelatedLayout();
			VFX_GeneralClear(true);
			PACConfigInfo_DeathBreak?.JustExecuteAllEffectBySkill(_characterBehaviourRef, this);
			
			
		}
		
		
		
		
#endregion

#endregion


		[InlineButton("FunctionA")]
		[Conditional("DEBUG")]
		protected void C_Debug_LogOccupyNotImplementInfo(DS_OccupationInfo occupationInfo)
		{
			DBug.LogError($"技能:{SkillType}出现了未处理的占用打断，打断来源是:{occupationInfo.OccupationInfoConfigName}");
		}


#if UNITY_EDITOR

		protected void PeekBuffConfig(RolePlay_BuffTypeEnum type)
		{
			var va = GlobalConfigurationAssetHolderHelper.GetGCAHH().Collection_RPBuff.GetRPBuffByTypeAndLevel(type);
			if (va != null)
			{
				Sirenix.OdinInspector.Editor.OdinEditorWindow.InspectObject(va);
			}
		}
#endif



		/// <summary>
		/// <para>技能自己的威力影响</para>
		/// </summary>
		protected virtual void _ABC_SkillPowerBonus_(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar == null)
			{
				return;
			}
			if (!dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerSkillAttack_玩家技能伤害))
			{
				return;
			}

			if (dar.RelatedProjectileRuntimeRef == null)
			{
				return;
			}

			var layoutUID = dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference.LayoutContentInSO.LayoutUID;

			if (!SelfSkillConfigRuntimeInstance.ConcreteSkillFunction.SkillPowerAffectLayoutUID.Contains(layoutUID))
			{
				return;
			}


			dar.CP_SkillPower.MultiplyPart += SkillPowerInitPercent / 100f;
		}


		/// <summary>
		/// <para>快速地校准朝向到当前指针方向</para>
		/// </summary>
		protected virtual void QuickFaceToPointer()
		{
			var d = _playerControllerRef.InputResult_AimDirectionOnCurrentGameplayDirection;
			if (d.x > 0f)
			{
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(false);
			}
			else
			{
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(true);
			}
		}

		protected virtual void QuickFaceToRecordDirection()
		{
			if (RecordedAttackDirection == null)
			{
				return;
			}
			if (RecordedAttackDirection.Value.x > 0f)
			{
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(false);
			}
			else
			{
				_characterBehaviourRef.GetSelfRolePlayArtHelper().SetFaceLeft(true);
			}
		
		}


#region 快速霸体操作

		protected virtual void ApplyWeakStoic(float duration = -1f)
		{
			var blp_stoic = GenericPool<BLP_霸体施加信息_StoicApplyInfoBLP>.Get();
			blp_stoic.Applier = this;
			blp_stoic.RemainingDuration = duration;
			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体,
				_characterBehaviourRef,
				_characterBehaviourRef,
				blp_stoic);
			blp_stoic.ReleaseOnReturnToPool();
		}

		protected virtual void RemoveWeakStoic()
		{
			if (_characterBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体) !=
			    BuffAvailableType.NotExist)
			{
				var blp_StoicRemove = GenericPool<BLP_霸体移除信息_StoicRemoveInfoBLP>.Get();
				blp_StoicRemove.Applier = this;
				_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.WeakStoic_弱霸体,
					_characterBehaviourRef,
					_characterBehaviourRef,
					blp_StoicRemove);
				blp_StoicRemove.ReleaseOnReturnToPool();
			}
		}


		protected virtual void ApplyStrongStoic(float duration = -1f)
		{
			var blp_stoic = GenericPool<BLP_霸体施加信息_StoicApplyInfoBLP>.Get();
			blp_stoic.Applier = this;
			blp_stoic.RemainingDuration = duration;
			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.StrongStoic_强霸体,
				_characterBehaviourRef,
				_characterBehaviourRef,
				blp_stoic);
			blp_stoic.ReleaseOnReturnToPool();
		}
		protected virtual void RemoveStrongStoic()
		{
			if (_characterBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.StrongStoic_强霸体) !=
			    BuffAvailableType.NotExist)
			{
				var blp_StoicRemove = GenericPool<BLP_霸体移除信息_StoicRemoveInfoBLP>.Get();
				blp_StoicRemove.Applier = this;
				_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.StrongStoic_强霸体,
					_characterBehaviourRef,
					_characterBehaviourRef,
					blp_StoicRemove);
				blp_StoicRemove.ReleaseOnReturnToPool();
			}
		}


		protected virtual void ApplyDirectorInvincible(float duration = -1)
		{
			var blp_Invincible = GenericPool<Buff_InvincibleAllByDirector.BLP_机制无敌施加信息_DirectorInvincibleApplyInfoBLP>
				.Get();
			blp_Invincible.Applier = this;
			blp_Invincible.RemainingDuration = duration;
			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.InvincibleByDirector_All_WD_来自机制的完全无敌,
				_characterBehaviourRef,
				_characterBehaviourRef,
				blp_Invincible);
			blp_Invincible.ReleaseOnReturnToPool();
		}

		protected virtual void RemoveDirectorInvincible()
		{
			if (_characterBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
				.InvincibleByDirector_All_WD_来自机制的完全无敌) != BuffAvailableType.NotExist)
			{
				var blp_Invincible =
					GenericPool<Buff_InvincibleAllByDirector.BLP_机制无敌移除信息_DirectorInvincibleRemoveInfoBLP>.Get();
				blp_Invincible.Applier = this;
				_characterBehaviourRef.ReceiveBuff_TryApplyBuff(
					RolePlay_BuffTypeEnum.InvincibleByDirector_All_WD_来自机制的完全无敌,
					_characterBehaviourRef,
					_characterBehaviourRef,
					blp_Invincible);
				blp_Invincible.ReleaseOnReturnToPool();
			}
		}

#endregion

#region 其他buff查询

		protected bool _Internal_CheckIfContainsFlagBuff(RP_BuffInternalFunctionFlagTypeEnum flag)
		{
			return _characterBehaviourRef.GetRelatedBuffHolder().ReceiveBuff_CheckExistValidBuffWithTag(flag);
		}

#endregion

#region 图标部分

		[Serializable]
		public struct SpritePair
		{
			public string Desc;
			[PreviewField(Height = 50)]
			public Sprite SpriteAsset;
		}

		[SerializeField, LabelText("技能图标信息们"), ListDrawerSettings(AddCopiesLastElement = true)]
		public List<SpritePair> SpritePairs = new List<SpritePair>
		{
			new SpritePair
			{
				Desc = "默认",
				SpriteAsset = null
			}
		};

		public virtual Sprite GetCurrentSprite(DamageTypeEnum @override = DamageTypeEnum.None)
		{
			if (SpritePairs.Count == 1)
			{
				return SpritePairs[0].SpriteAsset;
			}

			DamageTypeEnum fd = DamageTypeEnum.None;

			if (@override != DamageTypeEnum.None)
			{
				fd = @override;
			}
			else
			{
				fd = (RelatedRPSkillCaster.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
					.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType).CurrentDamageType;
			}
			switch (fd)
			{
				case DamageTypeEnum.YuanNengGuang_源能光:
					return SpritePairs.Find((pair => pair.Desc.Contains("光"))).SpriteAsset;
				case DamageTypeEnum.YuanNengDian_源能电:
					return SpritePairs.Find((pair => pair.Desc.Contains("电"))).SpriteAsset;
				case DamageTypeEnum.AoNengTu_奥能土:
					return SpritePairs.Find((pair => pair.Desc.Contains("土"))).SpriteAsset;
				case DamageTypeEnum.AoNengShui_奥能水:
					return SpritePairs.Find((pair => pair.Desc.Contains("水"))).SpriteAsset;
				case DamageTypeEnum.AoNengHuo_奥能火:
					return SpritePairs.Find((pair => pair.Desc.Contains("火"))).SpriteAsset;
				case DamageTypeEnum.AoNengFeng_奥能风:
					return SpritePairs.Find((pair => pair.Desc.Contains("风"))).SpriteAsset;
				case DamageTypeEnum.LingNengLing_灵能灵:
					return SpritePairs.Find((pair => pair.Desc.Contains("灵"))).SpriteAsset;
				case DamageTypeEnum.YouNengXingHong_幽能猩红:
					return SpritePairs.Find((pair => pair.Desc.Contains("猩红"))).SpriteAsset;
			}
			return null;
		}

#endregion



	}
}