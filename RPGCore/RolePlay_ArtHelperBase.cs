using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Common.HitEffect;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Character;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.Playables;

namespace RPGCore
{
	/// <summary>
	/// <para>所有RolePlay对象都会使用的ArtHelper，这是基本抽象类</para>
	/// </summary>
	public abstract class RolePlay_ArtHelperBase : MonoBehaviour
	{
		
		
		
#region 锚点设置

		[SerializeField, Required, LabelText("翻转锚点")] [TitleGroup("===基本===")]
		public Transform _flipAnchor;

		[SerializeField, Required, LabelText("旋转锚点")] [TitleGroup("===基本===")]
		public Transform _rotateAnchor;

		[SerializeField, Required, LabelText("缩放锚点")] [TitleGroup("===基本===")]
		public Transform _scaleAnchor;



		[InfoBox("通用的挂点名是：【仅自身缩放】，【缩放并旋转】，【缩放旋转翻转】"), SerializeField, LabelText("挂点配置们"),
		 GUIColor(206f / 255f, 177f / 255f, 227f / 255f)] [TitleGroup("===基本===")]

#if UNITY_EDITOR
		[InlineButton(nameof(_Button_FillBase), "填充基础配置", ShowIf = "@this._selfVFXHolderInfoList.Count <= 2")]
#endif
		protected List<ConSer_VFXHolderInfo> _selfVFXHolderInfoList = new List<ConSer_VFXHolderInfo>
		{
			new ConSer_VFXHolderInfo
			{
				FollowConfigName = "仅自身缩放"
			},
			new ConSer_VFXHolderInfo
			{
				FollowConfigName = "缩放并旋转"
			},
			new ConSer_VFXHolderInfo
			{
				FollowConfigName = "缩放旋转翻转"
			},
			new ConSer_VFXHolderInfo
			{
				FollowConfigName = "胸部"
			}
		};

#if UNITY_EDITOR
		private void _Button_FillBase()
		{
			if (_selfVFXHolderInfoList.Count > 2)
			{
				return;
			}
			_selfVFXHolderInfoList.Add(new ConSer_VFXHolderInfo
			{
				FollowConfigName = "仅自身缩放"
			});
			_selfVFXHolderInfoList.Add(new ConSer_VFXHolderInfo
			{
				FollowConfigName = "缩放并旋转"
			});
			_selfVFXHolderInfoList.Add(new ConSer_VFXHolderInfo
			{
				FollowConfigName = "缩放旋转翻转"
			});
			_selfVFXHolderInfoList.Add(new ConSer_VFXHolderInfo
			{
				FollowConfigName = "胸部"
			});
		}

		[Button("把自己塞进去")]
		private void _Button_S()
		{
			_selfVFXHolderInfoList.Add(new ConSer_VFXHolderInfo
			{
				FollowConfigName = "仅自身"
			});
		}
#endif

		protected static readonly Vector3 _flipAnchorLocalScale = new Vector3(-1f, 1f, 1f);

#endregion


#region 【图组】相关

		[SerializeField, LabelText("将要使用【图组】索引")] [TitleGroup("===动画===")]
		public int RelatedCharacterSheetGroupIndex = 0;

		/// <summary>
		/// 将自身下方的所有具体的AnimationHelper设置为指定图组
		/// </summary>
		/// <param name="index"></param>
		public virtual void SetRelatedSheetGroupIndex(int index)
		{
			RelatedCharacterSheetGroupIndex = index;
			foreach (var perAHB in CharacterAnimationHelperObjectsDict.Values)
			{
				if (perAHB is BaseCharacterSheetAnimationHelper baseCharacterSheetAnimationHelper)
				{
					baseCharacterSheetAnimationHelper.CurrentSpriteGroupIndex = index;
				}
			}
		}

#endregion



#region 受击染色

		// [SerializeField, LabelText("【覆盖】受击染色配置组们")] [TitleGroup("===游戏逻辑相关视效===")]
		// public List<HitColorOverrideOnConcreteArtHelper> HitColorConfigOverride =
		// 	new List<HitColorOverrideOnConcreteArtHelper>();

#endregion



#region 内部字段

		[SerializeField, LabelText("视效半径 —— 与VFX有关")] [TitleGroup("===游戏逻辑相关视效===")]
		public float _VFXScaleRadius = 1f;



		[SerializeField, Required, LabelText("起始朝左？")] [TitleGroup("===游戏逻辑相关视效===")]
		protected bool _initFaceLeft = false;


		/// <summary>
		/// <para>当前翻转了吗？朝左是不翻转，朝右是翻转</para>
		/// </summary>
		[ShowInInspector, LabelText("当前朝左吗？"), FoldoutGroup("运行时", true)]
		public bool CurrentFaceLeft
		{
			get;
			protected set;
		}



		[ShowInInspector, LabelText("关联的Timeline Director"), SerializeField] [TitleGroup("===基本===")]
		public PlayableDirector SelfPlayableDirector;

#endregion
		protected LocalActionBus _localActionBusRef;
		public RolePlay_BaseBehaviour SelfRelatedBaseRPBaseBehaviour { get; protected set; }



#region 动画与其Helper

		[ShowInInspector, LabelText("角色动画Helper对象们的容器")] [TitleGroup("===基本===")]
		public Dictionary<int, CharacterAnimationHelperBase> CharacterAnimationHelperObjectsDict
		{
			get;
			protected set;
		} = new Dictionary<int, CharacterAnimationHelperBase>();



		[NonSerialized]
		[ShowInInspector, LabelText("当前的主动画Helper"), FoldoutGroup("运行时", true)]
		public CharacterAnimationHelperBase MainCharacterAnimationHelperRef;

		/// <summary>
		/// 默认获取的是主动画Helper上的信息。通常情况下，一个角色只会播放一个动画
		/// 架构上允许同时播放不同的动画，但是在非主动画Helper上的动画信息不提供主动画逻辑
		/// </summary>
		[ShowInInspector, LabelText("当前活跃的主动画信息"), FoldoutGroup("运行时", true)]
		public AnimationInfoBase CurrentMainAnimationInfoRuntime
		{
			get
			{
				if (MainCharacterAnimationHelperRef == null)
				{
					return null;
				}
				return MainCharacterAnimationHelperRef.CurrentActiveAnimationInfo;
			}
		}



		protected AnimationPlayOptionsFlagTypeEnum _currentAnimationPlayOption;

		protected RP_DS_AnimationPlayResult _selfAnimationPlayResult = new RP_DS_AnimationPlayResult();
		public RP_DS_AnimationPlayResult SelfAnimationPlayResult => _selfAnimationPlayResult;

		protected RP_DS_AnimationRestoreResult _selfAnimationRestoreResult = new RP_DS_AnimationRestoreResult();

		public RP_DS_AnimationRestoreResult GetInitRestoreResult()
		{
			_selfAnimationRestoreResult.RestoreSuccess = true;
			return _selfAnimationRestoreResult;
		}




		/// <summary>
		/// 运行时实际使用的动画片段信息们。从各个来源收集动画信息，
		/// 然后为运行时的动画片段信息再额外添加
		/// key是config的
		/// </summary>
		[ShowInInspector]
		protected List< ClipTransition> _animationRuntimeClipList =
			new List<ClipTransition>();
		protected delegate void AnimationCustomCallbackDelegate(string name);
		public void RegisterRuntimeClipTransition(ClipTransition clipTransition)
		{
			if (clipTransition == null || !clipTransition.Clip)
			{
				return;
			}
			if (!_animationRuntimeClipList.Contains(clipTransition))
			{
				//进行运行时注册，
				//为其绑定Start事件、Complete事件于0.999，如果Names大于0则还有Custom
				_animationRuntimeClipList.Add(clipTransition);
				clipTransition.Events.Add(new AnimancerEvent(0f, _SheetCallback_OnAnimationStart));
				clipTransition.Events.Add(new AnimancerEvent(0.95f, _SheetCallback_OnAnimationComplete));
				// clipTransition.Events.Add(new AnimancerEvent(0.95f, _SheetCallback_OnAnimationComplete));
				if (clipTransition.SerializedEvents.Names.Length > 0)
				{
					for (int i = 0; i < clipTransition.SerializedEvents.Names.Length; i++)
					{
						int i1 = i;
						clipTransition.Events.Add(clipTransition.SerializedEvents.NormalizedTimes[i],
							() =>
							{
								_SheetCallback_CustomEvent(clipTransition.SerializedEvents.Names[i1]);
							});

						// Delegate c = new AnimationCustomCallbackDelegate(_SheetCallback_CustomEvent);
						//
						// clipTransition.SerializedEvents.Callbacks[i].AddPersistentCall(c);
					}
				}

			}
		}

		public void RegisterRuntimeClipTransition(SheetAnimationInfo_帧动画配置 sai)
		{
			RegisterRuntimeClipTransition(sai.ClipTransitionRef);
			RegisterRuntimeClipTransition(sai.ClipTransitionRef_DirectionToBack);
			RegisterRuntimeClipTransition(sai.ClipTransitionRef_DirectionToForward);
			switch (sai)
			{
				case SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle sheetAnimationInfoMultipleIdle帧动画多形态Idle配置Idle:
					RegisterRuntimeClipTransition(sheetAnimationInfoMultipleIdle帧动画多形态Idle配置Idle
						.ClipTransitionRef_TransitionClip);
					break;
				case SheetAnimationMove_可移动的帧动画配置 sheetAnimationMove可移动的帧动画配置:
					RegisterRuntimeClipTransition((sheetAnimationMove可移动的帧动画配置.ClipTransitionRef_MovingTowardsAttack));
					RegisterRuntimeClipTransition(sheetAnimationMove可移动的帧动画配置.ClipTransitionRef_MovingAwayFromAttack);
					break;
			}
		}
		
		 
#endregion



		protected virtual void Awake()
		{
		}

		public virtual void InjectBaseRPBehaviourRef(RolePlay_BaseBehaviour behaviourRef)
		{
			SelfRelatedBaseRPBaseBehaviour = behaviourRef;
		}

		public virtual void InitializeOnInstantiate(LocalActionBus lab)
		{
			CharacterAnimationHelperObjectsDict = new Dictionary<int, CharacterAnimationHelperBase>();

			_localActionBusRef = lab;

			//是Spine还是Sheet，根据Get到的结果来算
			foreach (CharacterAnimationHelperBase perAnimationHelper in
				GetComponentsInChildren<CharacterAnimationHelperBase>(true))
			{
// 				if (CharacterAnimationHelperObjectsDict.ContainsKey(perAnimationHelper.SelfHelperIndex))
// 				{
// // #if UNITY_EDITOR
// // 					// DBug.LogError($"{behaviour.name}在构建动画对象容器时，有重复索引{perAnimationHelper}，这不合理，已跳过这个动画Helper");
// // 					continue;
// // #endif
// 				}
				CharacterAnimationHelperObjectsDict.Add(perAnimationHelper.SelfHelperIndex, perAnimationHelper);
				if (perAnimationHelper.SelfHelperIndex == 0)
				{
					MainCharacterAnimationHelperRef = perAnimationHelper;
				}
				perAnimationHelper.InstantiateOnInitialize(lab, this);
				switch (perAnimationHelper)
				{
					case BaseCharacterSheetAnimationHelper baseCharacterSheetAnimationHelper:
						break;
				}
			}

			CurrentFaceLeft = true;
			SetFaceLeft(false);
			ClipInfoWithoutEventsDict = new Dictionary<ClipTransition, ClipTransition>();
		}

		public virtual LocalActionBus GetSelfLocalActionBus()
		{
			return SelfRelatedBaseRPBaseBehaviour.GetRelatedActionBus();
		}

#region 朝向 & 旋转

		public bool GetIfFlipped()
		{
			if (_flipAnchor)
			{
				return _flipAnchor.transform.localScale.x < 0;
			}
			else
			{
				return false;
			}
		}

		public virtual void SetFaceLeft(bool faceLeft)
		{
			if (CurrentFaceLeft == faceLeft)
			{
				return;
			}
			else
			{
				CurrentFaceLeft = faceLeft;
				//需要调整朝向
				//默认朝左，现在朝左
				if (faceLeft)
				{
					if (_initFaceLeft)
					{
						if (SelfPlayableDirector != null)
						{
							SelfPlayableDirector.transform.localScale = Vector3.one;
						}
						_flipAnchor.transform.localScale = Vector3.one;
					}
					else
					{
						if (SelfPlayableDirector != null)
						{
							SelfPlayableDirector.transform.localScale = _flipAnchorLocalScale;
						}
						_flipAnchor.transform.localScale = _flipAnchorLocalScale;
					}
				}
				else
				{
					//默认朝左，现在朝右
					if (_initFaceLeft)
					{
						if (SelfPlayableDirector != null)
						{
							SelfPlayableDirector.transform.localScale = _flipAnchorLocalScale;
						}
						_flipAnchor.transform.localScale = _flipAnchorLocalScale;
					}
					else
					{
						if (SelfPlayableDirector != null)
						{
							SelfPlayableDirector.transform.localScale = Vector3.one;
						}
						_flipAnchor.transform.localScale = Vector3.one;
					}
				}
			}
		}

		/// <summary>
		/// <para>俯仰角</para>
		/// <para>EulerX，用来调整面对摄像机的角度的</para>
		/// </summary>
		public void SetRotation_EulerX(float rotX)
		{
			Quaternion rotation = transform.rotation;
			Quaternion newRot = Quaternion.Euler(rotX, rotation.eulerAngles.y, rotation.eulerAngles.z);
			_rotateAnchor.rotation = newRot;
		}

		public void SetRotation_EulerXY(float rotX, float rotY)
		{
			Quaternion rotation = transform.rotation;
			Quaternion newRot = Quaternion.Euler(rotX, rotY, rotation.eulerAngles.z);
			_rotateAnchor.rotation = newRot;
		}

		/// <summary>
		/// <para>设置旋转锚点的世界rotation到指定quaterion</para>
		/// </summary>
		/// <param name="q"></param>
		public void SetCharacterRotation_Quaternion(Quaternion q)
		{
			_rotateAnchor.rotation = q;
		}

		public void SetCharacterLocalScale(float finalScale)
		{
			if (_scaleAnchor == null)
			{
#if UNITY_EDITOR
				 DBug.LogError($"角色{SelfRelatedBaseRPBaseBehaviour.name}没有设置缩放锚点！");
#endif
                _scaleAnchor = SelfRelatedBaseRPBaseBehaviour.transform;
            }
			_scaleAnchor.localScale = Vector3.one * finalScale;
		}

		/// <summary>
		/// <para>偏转角</para>
		/// <para>EulerY，用来调整在场景中的旋转角度</para>
		/// </summary>
		/// <param name="rotY"></param>
		public void SetRotation_EulerY(float rotY)
		{
			Vector3 currentRotation = transform.rotation.eulerAngles;
			Quaternion newRot = Quaternion.Euler(currentRotation.x, rotY, currentRotation.z);
			_rotateAnchor.rotation = newRot;
		}

#endregion

#region Tick

		public virtual void UpdateTick(float currentTime, int currentFrame, float deltaTime)
		{
			foreach (var perHelper in CharacterAnimationHelperObjectsDict.Values)
			{
				perHelper.UpdateTick(currentTime, currentFrame, deltaTime);
			}

		}




		public virtual void FixedUpdateTick(float ct, int cf, float delta)
		{

			foreach (var perHelper in CharacterAnimationHelperObjectsDict.Values)
			{
				perHelper.FixedUpdateTick(ct, cf, delta);
			}
			if (CurrentMainAnimationInfoRuntime != null)
			{
				if (CurrentMainAnimationInfoRuntime is SheetAnimationMove_可移动的帧动画配置 move)
				{
					//直接读输入吧
					var lastPosition = SelfRelatedBaseRPBaseBehaviour.GetPositionRecordByFrameInterval(1);
					var currentPosition = SelfRelatedBaseRPBaseBehaviour.GetPositionRecordByFrameInterval(0);
					lastPosition.y = 0f;
					currentPosition.y = 0f;
					float dis = Vector3.Distance(currentPosition, lastPosition);
					//没有移动
					if (dis < 0.01f)
					{
						// DBug.Log($"播放原地变体，当前dis:{dis}");
						SheetAnimation_SetMainAnimation(move,
							AnimationPlayOptionsFlagTypeEnum.StableVariant_停止中的动画片段变体);
					}
					else
					{
						var faceDirection = CurrentFaceLeft ? BaseGameReferenceService.CurrentBattleLogicLeftDirection
							: BaseGameReferenceService.CurrentBattleLogicRightDirection;
						Vector3 deltaVector = currentPosition - lastPosition;
						deltaVector.y = 0f;
						deltaVector.Normalize();
						float dot = Vector3.Dot(deltaVector, faceDirection);
						//大于0，同向
						if (dot > 0f)
						{
							SheetAnimation_SetMainAnimation(move,
								AnimationPlayOptionsFlagTypeEnum.MovingForwardVariant_移动中朝正向的动画片段变体);
						}
						else
						{
							SheetAnimation_SetMainAnimation(move,
								AnimationPlayOptionsFlagTypeEnum.MovingBackVariant_移动中朝背向的动画片段变体);
						}



						// DBug.Log($"播放移动变体，因为Current为空，当前dis:{dis}");
					}
				}
				if (_currentGhostEffectActive)
				{
					SetGhostPosition(5, 5);
				}
			}
		}

#endregion



#region 外部设置   透明度 & 自发光

		public virtual void SetEmission_All(DS_CharacterEmissionInfo emissionInfo)
		{
			foreach (CharacterAnimationHelperBase perSH in CharacterAnimationHelperObjectsDict.Values)
			{
				switch (perSH)
				{
					case BaseCharacterSheetAnimationHelper baseCharacterSheetAnimationHelper:
						if (baseCharacterSheetAnimationHelper.SelfMeshRendererRef == null)
						{
							continue;
						}
						emissionInfo.SetMeshRendererByDS_Sheet(baseCharacterSheetAnimationHelper.SelfMeshRendererRef);
						break;
				}
			}
		}

#endregion


		public virtual void ClearBeforeDestroy()
		{
		}
		public virtual void SetAnimation(
			AnimationInfoBase info,
			AnimationPlayOptionsFlagTypeEnum option,
			float mulSpeed)
		{
			switch (info)
			{
				case SheetAnimationInfo_帧动画配置 sheetAnimationInfo帧动画配置:
					SheetAnimation_SetMainAnimation(sheetAnimationInfo帧动画配置, option, mulSpeed);
					break;
			}
		}

#region Sheet部分

		public virtual void _SheetCallback_OnAnimationStart()
		{
			// DBug.Log("动画开始，当前为"+CurrentMainAnimationInfoRuntime.ConfigName);
			var ds_general =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationStart_动画通用开始);
			ds_general.ObjectArguStr = CurrentMainAnimationInfoRuntime.ConfigName;
			ds_general.ObjectArgu1 = MainCharacterAnimationHelperRef;
			ds_general.ObjectArgu2 = CurrentMainAnimationInfoRuntime;
			_localActionBusRef?.TriggerActionByType(ds_general);
		}


		/// <summary> objStr 是配置的名字(AnimationInfo 的 Config名字，【比对】)， obj1是关联的AnimationArtHelper,obj2是使用的AnimationInfo</summary>
		public virtual void _SheetCallback_OnAnimationComplete()
		{
			var ds_general = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_AnimationHelper_OnAnimationComplete_动画通用结束);
			ds_general.ObjectArguStr = CurrentMainAnimationInfoRuntime.ConfigName;
			ds_general.ObjectArgu1 = MainCharacterAnimationHelperRef;
			ds_general.ObjectArgu2 = CurrentMainAnimationInfoRuntime;
			_localActionBusRef?.TriggerActionByType(ds_general);

			if (CurrentMainAnimationInfoRuntime is SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle idle)
			{
				if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum.PlayIdleReplace_播放长Idle动画))
				{
					idle.ResetAccumulateDuration();
				}
				SheetAnimation_SetMainAnimation(idle, AnimationPlayOptionsFlagTypeEnum.Default_缺省状态);
			}
		}
		public virtual void _SheetCallback_CustomEventByEventInfo()
		{
			Debug.Log($"An {nameof(AnimancerEvent)} was triggered:" +
			          $"\n- Event: {AnimancerEvent.CurrentEvent}" +
			          $"\n- State: {AnimancerEvent.CurrentState}");
			// var ds_general = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
			// 	.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件);
			// ds_general.ObjectArgu1 = MainCharacterAnimationHelperRef;
			// ds_general.ObjectArgu2 = eventName;
			// ds_general.ObjectArguStr = CurrentMainAnimationInfoRuntime;
			//
			// _localActionBusRef?.TriggerActionByType(ds_general);
		}

		public virtual void _SheetCallback_CustomEvent(string eventName)
		{
			var ds_general = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件);
			ds_general.ObjectArgu1 = MainCharacterAnimationHelperRef;
			ds_general.ObjectArgu2 = eventName;
			ds_general.ObjectArguStr = CurrentMainAnimationInfoRuntime;

			_localActionBusRef?.TriggerActionByType(ds_general);
		}

		public virtual void SheetAnimation_SetMainAnimation(
			SheetAnimationInfo_帧动画配置 sheetAnimationInfo,
			AnimationPlayOptionsFlagTypeEnum options,
			float mulSpeed = 1f)
		{
			RegisterRuntimeClipTransition(sheetAnimationInfo);
			
			// DBug.Log($"动画试图播放{sheetAnimationInfo.ConfigName}");
			int sheetIndex = sheetAnimationInfo._targetHelperIndex;

			_currentAnimationPlayOption = options;
			BaseCharacterSheetAnimationHelper mainHelperRef =
				MainCharacterAnimationHelperRef as BaseCharacterSheetAnimationHelper;
			if (CurrentMainAnimationInfoRuntime != null)
			{
				CurrentMainAnimationInfoRuntime.OccupationInfo.CurrentActiveOccupationLevel =
					sheetAnimationInfo.OccupationInfo.OccupationLevel;
			}
			//如果当前播放的动画信息和将要播放的不一致，则重置累积时长
			if (CurrentMainAnimationInfoRuntime != null && CurrentMainAnimationInfoRuntime != sheetAnimationInfo)
			{
				CurrentMainAnimationInfoRuntime.ResetAccumulateDuration();
			}

			float willPlaySpeed = sheetAnimationInfo.AnimationPlaySpeed * mulSpeed;
			if (sheetAnimationInfo.ClipTransitionRef.Clip == null)
			{
				throw new ArgumentOutOfRangeException(
					$"动画配置{sheetAnimationInfo.ConfigName}的动画片段丢失了！来自角色{SelfRelatedBaseRPBaseBehaviour.name}");
			}
			ClipTransition willPlayClipTransitionRef = sheetAnimationInfo.ClipTransitionRef;
			int holdOrRest = 0;

			
			if(_currentAnimationPlayOption.HasFlag( AnimationPlayOptionsFlagTypeEnum.DirectionToForward_方向向前))
			{
				if (sheetAnimationInfo.ContainDirectionVariation)
				{
					willPlayClipTransitionRef = sheetAnimationInfo.ClipTransitionRef_DirectionToForward;
				}
			}
			if(_currentAnimationPlayOption.HasFlag( AnimationPlayOptionsFlagTypeEnum.DirectionToBack_方向向后))
			{
				if (sheetAnimationInfo.ContainDirectionVariation)
				{
					willPlayClipTransitionRef = sheetAnimationInfo.ClipTransitionRef_DirectionToBack;
				}
			}
			if( _currentAnimationPlayOption.HasFlag( AnimationPlayOptionsFlagTypeEnum.MainTransparentVersion_主体透明版本))
			{
				if (sheetAnimationInfo._targetHelperIndex != 1)
				{
					DBug.LogWarning(
						$"来自ArtHelper{name}(归属于角色{SelfRelatedBaseRPBaseBehaviour.name}在播放动画时，" +
						$"要求播放了主体透明版本，但传入的动画索引并不是1，这不合理，检查一下");
				}
			}
			if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum.PlayIdleReplace_播放长Idle动画))
			{
				if (sheetAnimationInfo is not SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle longIdle)
				{
					DBug.LogError($"来自ArtHelper{name}(归属于角色{SelfRelatedBaseRPBaseBehaviour.name}在播放动画时" +
					              $"要求播放了长Idle动画，但传入的数据并不是长Idle动画配置，这不合理，检查一下");
				}
				else
				{
					longIdle.ResetAccumulateDuration();
					willPlayClipTransitionRef = longIdle.ClipTransitionRef;
				}
			}

			if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum.DisableExceptExplicitIndex_除了指定的索引外全部禁用))
			{
				foreach (CharacterAnimationHelperBase perHelper in CharacterAnimationHelperObjectsDict.Values)
				{
					if (perHelper.SelfHelperIndex != sheetIndex)
					{
						perHelper.gameObject.SetActive(false);
					}
				}
			}
			if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum
				.MovingForwardVariant_移动中朝正向的动画片段变体))
			{
				if (sheetAnimationInfo is not SheetAnimationMove_可移动的帧动画配置 moveVariant)
				{
					DBug.LogError($"来自ArtHelper{name}(归属于角色{SelfRelatedBaseRPBaseBehaviour.name}在播放动画时" +
					              $"要求播放了移动中的动画片段变体，但传入的数据并不是移动中的动画片段变体，这不合理，检查一下");
				}
				else
				{
					willPlayClipTransitionRef = moveVariant.ClipTransitionRef_MovingTowardsAttack;
					holdOrRest = 2;
				}
			}
			if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum
				.StableVariant_停止中的动画片段变体))
			{
				if (sheetAnimationInfo is not SheetAnimationMove_可移动的帧动画配置 moveVariant2)
				{
					DBug.LogError($"来自ArtHelper{name}(归属于角色{SelfRelatedBaseRPBaseBehaviour.name}在播放动画时" +
					              $"要求播放了移动中的动画片段变体，但传入的数据并不是移动中的动画片段变体，这不合理，检查一下");
				}
				else
				{
					willPlayClipTransitionRef = moveVariant2.ClipTransitionRef;
					holdOrRest = 2;
				}
			}
			if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum
				.HoldAnimationProcess_维持动画进度))
			{
				holdOrRest = 2;
			}
			if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum.MovingBackVariant_移动中朝背向的动画片段变体))
			{
				if (sheetAnimationInfo is not SheetAnimationMove_可移动的帧动画配置 moveV2)
				{
					DBug.LogError($"来自ArtHelper{name}(归属于角色{SelfRelatedBaseRPBaseBehaviour.name}在播放动画时" +
					              $"要求播放了移动中的动画片段变体，但传入的数据并不是移动中的动画片段变体，这不合理，检查一下");
				}
				else
				{
					willPlayClipTransitionRef = moveV2.ClipTransitionRef_MovingAwayFromAttack;
				}
			}
			if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum
				.ForceReplay_必定重播))
			{
				holdOrRest = 1;
			}
			 

			_SetCurrentAnimationInfo(sheetAnimationInfo)?.PlayWithSpeed(willPlayClipTransitionRef, willPlaySpeed, holdOrRest);
			SyncAnimationWithGhostEffect(willPlayClipTransitionRef, willPlaySpeed);




			BaseCharacterSheetAnimationHelper _SetCurrentAnimationInfo(SheetAnimationInfo_帧动画配置 sheetInfo)
			{
				if (CharacterAnimationHelperObjectsDict.TryGetValue(sheetInfo._targetHelperIndex,
					out CharacterAnimationHelperBase value))
				{
					sheetInfo._targetHelperRef = value as BaseCharacterSheetAnimationHelper;

					BaseCharacterSheetAnimationHelper targetSheet = value as BaseCharacterSheetAnimationHelper;
					targetSheet.CurrentActiveAnimationInfo = sheetAnimationInfo;
					targetSheet.CurrentActiveAnimationInfo.OccupationInfo.CurrentActiveOccupationLevel =
						sheetInfo.OccupationInfo.OccupationLevel;

					if (!targetSheet.gameObject.activeInHierarchy)
					{
						targetSheet.gameObject.SetActive(true);
					}
					return targetSheet;
				}
				DBug.LogError($"角色ArtHelper{name}试图用第{sheetIndex}个Sheet播放动画，但它并没有这个索引的Sheet，怎么回事？");
				return null;
			}

			void SyncAnimationWithGhostEffect(ClipTransition clipTransitionRef, float speed)
			{
				if (_currentGhostEffectActive)
				{
					foreach (SyncPlayAnimationHelperInfo perInfo in _ghostEffectAnimationHelperList)
					{
						var sheetAnimation = perInfo.RelatedHelper as BaseCharacterSheetAnimationHelper;
						if (!sheetAnimation.gameObject.activeSelf)
						{
							continue;
						}
						if (!ClipInfoWithoutEventsDict.ContainsKey(clipTransitionRef))
						{
							var newClip = new ClipTransition();
							newClip.Clip = clipTransitionRef.Clip;
							ClipInfoWithoutEventsDict.Add(clipTransitionRef, newClip);
						}
						
						sheetAnimation.SelfAnimancerComponent.Playable.Speed = sheetAnimationInfo.AnimationPlaySpeed;
						sheetAnimation.PlayWithSpeed(ClipInfoWithoutEventsDict[clipTransitionRef], speed);

						sheetAnimation.SetAlphaOverride(perInfo.TargetAlpha);
					}
				}
			}
		}

#endregion




		public virtual void SetAllAnimationLogicSpeedMul(float speed)
		{
			foreach (var perHelper in CharacterAnimationHelperObjectsDict.Values)
			{
				perHelper.AnimationLogicSpeedMul = speed;
			}
		}





#region 残影效果

		[Serializable]
		public class SyncPlayAnimationHelperInfo
		{
			public CharacterAnimationHelperBase RelatedHelper;
			public float TargetAlpha;
		}

		protected Dictionary<ClipTransition, ClipTransition> ClipInfoWithoutEventsDict =
        			new Dictionary<ClipTransition, ClipTransition>();
		[SerializeField, LabelText("作为残影效果的Helper信息们"), TitleGroup("===残影===")]
		protected List<SyncPlayAnimationHelperInfo> _ghostEffectAnimationHelperList =
			new List<SyncPlayAnimationHelperInfo>();

		protected bool _currentGhostEffectActive;

		public void ActivateGhostEffect()
		{
			_currentGhostEffectActive = true;
			foreach (SyncPlayAnimationHelperInfo perInfo in _ghostEffectAnimationHelperList)
			{
				if (!perInfo.RelatedHelper.gameObject.activeSelf)
				{
					perInfo.RelatedHelper.gameObject.SetActive(true);
				}
			}
		}

		public void DeactivateGhostEffect()
		{
			_currentGhostEffectActive = false;
			foreach (SyncPlayAnimationHelperInfo perInfo in _ghostEffectAnimationHelperList)
			{
				if (perInfo.RelatedHelper.gameObject.activeSelf)
				{
					perInfo.RelatedHelper.gameObject.SetActive(false);
				}
			}
		}

		public void SetGhostPosition(int capacity, int interval)
		{
			for (int i = 0; i < _ghostEffectAnimationHelperList.Count && i < capacity; i++)
			{
				var t = _ghostEffectAnimationHelperList[i].RelatedHelper.transform;
				t.position = SelfRelatedBaseRPBaseBehaviour.GetPositionRecordByFrameInterval((interval + 1) * i);
				t.localPosition += BaseGameReferenceService.CurrentBattleLogicalForwardDirection * (0.01f * i);
			}
		}

#endregion


	}
}