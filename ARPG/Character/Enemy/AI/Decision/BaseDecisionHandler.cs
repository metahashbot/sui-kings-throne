using System;
using System.Collections.Generic;
using ARPG.Camera;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using ARPG.Character.Player;
using ARPG.Character.Player.Ally;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Character.Enemy.AI.Decision
{

	[Serializable]
	public abstract class BaseDecisionHandler
	{
        
		[NonSerialized]
		public SOConfig_AIDecision SelfRelatedDecisionRuntimeInstance;

		[NonSerialized]
		public SOConfig_AIBrain SelfRelatedBrainInstanceRef;

		[ShowInInspector, LabelText("决策正常运行？"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public bool DecisionRunningNormal { get; protected set; } = false;
		
		

		protected LocalActionBus _selfLocalActionBusRef;

		protected BaseARPGCharacterBehaviour _selfRelatedBehaviourRef;


		protected static PlayerCharacterBehaviourController _playerControllerRef;
		protected static CharacterOnMapManager _characterOnMapManagerRef;
		protected static MainCameraBehaviour_ARPG _cameraBehaviourRef;
		protected static SubGameplayLogicManager_ARPG _glmArpgRef;
		protected static GameReferenceService_ARPG _grsRef;

		
		[Space(20)]
		[SerializeField, FoldoutGroup("配置")]
		[LabelText("【作用】于【自行结束】时")]
		[PropertyOrder(19)]
		public DCCConfigInfo DCCConfig_OnStop = new DCCConfigInfo();
		
		[LabelText("【作用】于【打断】时"), FoldoutGroup("配置", true)]
		[SerializeField] [PropertyOrder(20)]
		public DCCConfigInfo DCCConfig_OnBreak = new DCCConfigInfo();

		[SerializeField, FoldoutGroup("配置")]
		[LabelText("【作用】于【决策退出】时")]
		[PropertyOrder(21)]
		public DCCConfigInfo DCCConfig_OnExit = new DCCConfigInfo();


		

		protected bool _willDecisionImmediateEnd = false;


		[LabelText("进入决策时仇恨的位置"), NonSerialized, FoldoutGroup("运行时",
			 false,
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public Vector3 HatredPositionOnEnterDecision;
		
		[LabelText("上次结束决策时间点"),NonSerialized,FoldoutGroup("运行时",
			 false,
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public float LastDecisionEndTime = float.MinValue;


		public static void StaticInitialize()
		{
			_grsRef = GameReferenceService_ARPG.Instance;
			_glmArpgRef = SubGameplayLogicManager_ARPG.Instance;
			_cameraBehaviourRef = _grsRef.CameraBehaviourRef;
			_playerControllerRef = _glmArpgRef.PlayerCharacterBehaviourControllerReference;
			_characterOnMapManagerRef = _glmArpgRef.CharacterOnMapManagerReference;

		}


		#region 初始化

		public void InjectRawConfigReference(SOConfig_AIDecision rawTemplate, SOConfig_AIDecision runtimeInstance)
		{	
			SelfRelatedDecisionRuntimeInstance = runtimeInstance;

			SelfRelatedDecisionRuntimeInstance.OriginalSOAssetTemplate = rawTemplate;
		}

		public virtual void Initialize(SOConfig_AIBrain config)
		{
			LastDecisionEndTime = float.MinValue;
			SelfRelatedBrainInstanceRef = config;
			_selfRelatedBehaviourRef = config.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			_selfLocalActionBusRef = config.BrainHandlerFunction.SelfLocalActionBusRef;
			
			_selfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_PLC_Spawn_OnOneSpawnOperationAddToList_当一次生成活动已经将数据加入了列表,
				_ABC_CheckIfNeedUnifyDamageStamp_OnOneLayoutSpawnOperation);


			_selfLocalActionBusRef.RegisterAction(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationStart_动画通用开始,
				_ABC_ProcessGeneralAnimationStart_OnAnimationStart);
			_selfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束,
				_ABC_ProcessGeneralAnimationComplete_OnAnimationComplete);
			_selfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件,
				_ABC_ProcessGeneralAnimationEvent_OnAnimationCustomEvent);

			DCCConfig_OnStop.CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			DCCConfig_OnStop.BuildRuntimeDCC(ref DCCConfig_OnStop.CommonComponents_RuntimeAll);

			DCCConfig_OnBreak.CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			DCCConfig_OnBreak.BuildRuntimeDCC(ref DCCConfig_OnBreak.CommonComponents_RuntimeAll);

			DCCConfig_OnExit.ProcessRuntimeBuild();

			if (_includeWallHitCheck)
			{
				DCCConfig_OnWallHit.ProcessRuntimeBuild();
			}
			
			
			SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo.CommonComponents_RuntimeAll =
				new List<BaseDecisionCommonComponent>();
			SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo.BuildRuntimeDCC( ref
				SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo.CommonComponents_RuntimeAll);
		
			
			foreach (var perDD in SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo.CommonComponents_RuntimeAll)
			{
				if (perDD.RequireAnimationMatching &&
				    perDD.AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义)
				{
					perDD.CustomEventString = perDD.CustomEventString.Trim();
				}
			}
			foreach (var perDD in DCCConfig_OnBreak.CommonComponents_RuntimeAll)
			{
				if (perDD.RequireAnimationMatching &&
				    perDD.AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义)
				{
					perDD.CustomEventString = perDD.CustomEventString.Trim();
				}
			}
			foreach (var perDD in DCCConfig_OnStop.CommonComponents_RuntimeAll)
			{
				if (perDD.RequireAnimationMatching &&
				    perDD.AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义)
				{
					perDD.CustomEventString = perDD.CustomEventString.Trim();
				}
			}

			if (_includeWallHitCheck)
			{
				_selfLocalActionBusRef.RegisterAction(
					ActionBus_ActionTypeEnum.L_Utility_MovementCollideWithAirWall_此次移动碰撞到了空气墙,
					_ABC_AccumulateHitWallFrameCount_OnCollideWithWall,
					0,
					true);
			}


		}

#endregion
#region 选取

		/// <summary>
		/// <para>开始执行决策前的回调。在此之后就正常执行了</para>
		/// <para>通常是在Brain的 ExecuteFirstDecision 中进行</para>
		/// </summary>
		/// <returns></returns>
		public virtual DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			DecisionRunningNormal = true;
			if (SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget() == null)
			{

				HatredPositionOnEnterDecision = SelfRelatedBrainInstanceRef.BrainHandlerFunction
					.SelfRelatedARPGCharacterBehaviour.transform.position;
			}
			else
			{


				HatredPositionOnEnterDecision = SelfRelatedBrainInstanceRef.BrainHandlerFunction
					.GetCurrentHatredTarget().GetCollisionCenter();

			}

			_collideWithWallAccumulateFrameCount = 0;
			
            _Internal_BlockBrainAutoDeduce();
			var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIDecision_OnDecisionBeginExecute_当决策开始执行);
			ds.ObjectArgu1 = this;
			_selfLocalActionBusRef.TriggerActionByType(ds);
			if (withSideEffect)
			{
				ProcessCommonSideComponents();
			}
			return ds;
		}


		/// <summary>
		/// <para>在自主推演的时候获得选取权重</para>
		/// </summary>
		/// <returns></returns>
		public virtual float GetPickWeightAtAutoDeduce()
		{
			return SelfRelatedDecisionRuntimeInstance.ConfigContent.OriginalPickWeight;
		}


		/// <summary>
		/// <para>在被Brain选取到队列之前做的事情，做完这个就是自己被加到队列里了</para>
		/// </summary>
		public virtual void BeforePickedToQueueByBrain()
		{
		}


		/// <summary>
		/// <para>如果需要阻塞自主推演，则调用这个</para>
		/// </summary>
		protected virtual DS_ActionBusArguGroup _Internal_BlockBrainAutoDeduce(bool autoLaunch = true)
		{
			var ds_block = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_AIDecision_DecisionRequireBrainBlockAutoDeduce_由决策要求阻塞Brain的自动推演);
			ds_block.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_block);
			}
			return ds_block;
		}


		protected virtual DS_ActionBusArguGroup _Internal_ReleaseBrainAutoDeduce(bool autoLaunch = true)
		{
			var ds_release = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_AIDecision_DecisionRequireBrainReleaseAutoDeduce_由决策要求释放Brain的自动推演);
			ds_release.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_release);
			}
			return ds_release;
		}

		protected virtual DS_ActionBusArguGroup _Internal_RequireAutoDeduce(bool forceDeduce, bool autoLaunch = true)
		{
			var ds_require = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_AIDecision_DecisionRequireAutoDeduce_由决策要求立刻进行自行推演);
			ds_require.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			ds_require.IntArgu1 = forceDeduce ? 1 : 0;
			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_require);
			}
			return ds_require;
		}



		protected virtual void ProcessCommonSideComponents()
		{
			foreach (BaseDecisionCommonComponent perComponent in SelfRelatedDecisionRuntimeInstance.ConfigContent
				.DCCConfigInfo.CommonComponents_RuntimeAll)
			{
				if (perComponent.RequireAnimationMatching)
				{
					continue;
				}
				perComponent.EnterComponent(SelfRelatedBrainInstanceRef);
			}
		}

#endregion

#region 运行中



		public virtual void FixedUpdateTick(float ct, int cf, float delta)
		{
			foreach (BaseDecisionCommonComponent perDCC in SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo
				.CommonComponents_RuntimeAll)
			{
				perDCC.FixedUpdateTick(ct, cf, delta);
			}
			if( _includeWallHitCheck &&  (cf  - _lastCollideWithWallFrameIndex) > _wallHitCheckReleaseInterval)
			{
				_collideWithWallAccumulateFrameCount = 0;
			}

		}

		/// <summary>
		/// <para>该决策正在执行的时候被打断了</para>
		/// <para>传入的是打断源</para>
		/// <para>通常来说【打断】是包括【停止】的，它会先处理打断的内容再调用常规【停止】</para>
		/// </summary>
		public virtual void OnDecisionRunningBreak(SOConfig_AIDecision breakSource)
		{
			var ds_Break = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIDecision_OnDecisionBreak_当决策被打断);
			ds_Break.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			ds_Break.ObjectArgu2 = breakSource;
			_selfLocalActionBusRef.TriggerActionByType(ds_Break);

			if (DCCConfig_OnBreak.CommonComponents_RuntimeAll != null &&
			    DCCConfig_OnBreak.CommonComponents_RuntimeAll.Count > 0)
			{
				foreach (BaseDecisionCommonComponent perDCC in DCCConfig_OnBreak.CommonComponents_RuntimeAll)
				{
					perDCC.EnterComponent(SelfRelatedBrainInstanceRef);
				}
			}

			OnDecisionNormalComplete(false, false);
		}

#endregion


#region 撞墙

		[SerializeField, FoldoutGroup("配置")]
		[LabelText("包含[持续撞墙]检测")]
		[PropertyOrder(22)]
		public bool _includeWallHitCheck = false;

		[SerializeField, FoldoutGroup("配置")]
		[ShowIf("@this._includeWallHitCheck")]
		[LabelText("触发[持续撞墙]的持续帧数")]
		[PropertyOrder(23)]
		public int _wallHitCheckInterval = 10;

		[SerializeField, FoldoutGroup("配置")]
		[ShowIf("@this._includeWallHitCheck")]
		[LabelText("释放撞墙检测的帧数间隔")]
		[PropertyOrder(23)]
		public int _wallHitCheckReleaseInterval = 10;

		[SerializeField, FoldoutGroup("配置")]
		[ShowIf("@this._includeWallHitCheck")]
		[LabelText("【作用】于触发[持续撞墙]（无Tick无动画）")]
		[PropertyOrder(23)]
		public DCCConfigInfo DCCConfig_OnWallHit = new DCCConfigInfo();


		protected int _lastCollideWithWallFrameIndex;

		protected int _collideWithWallAccumulateFrameCount;

		private void _ABC_AccumulateHitWallFrameCount_OnCollideWithWall(DS_ActionBusArguGroup ds)
		{
			_lastCollideWithWallFrameIndex = BaseGameReferenceService.CurrentFixedFrame;
			if (CheckIfCurrentBrainValidAndCurrentDecisionIsThis(SelfRelatedDecisionRuntimeInstance))
			{
				_collideWithWallAccumulateFrameCount += 1;
				if (_collideWithWallAccumulateFrameCount >= _wallHitCheckInterval)
				{
					_collideWithWallAccumulateFrameCount = 0;
					foreach (BaseDecisionCommonComponent perDCC in DCCConfig_OnWallHit.CommonComponents_RuntimeAll)
					{
						perDCC.EnterComponent(SelfRelatedBrainInstanceRef);
					}
					_collideWithWallAccumulateFrameCount = 0;
				}
			}
		}
		
		 

#endregion
		
		protected virtual DS_ActionBusArguGroup _Internal_RequireAnimation(
			AnimationInfoBase animationInfo,
			bool autoLaunch = true,
			Nullable<bool> lookHateTarget = null , AnimationPlayOptionsFlagTypeEnum options = AnimationPlayOptionsFlagTypeEnum.Default_缺省状态)
		{
			var ds_ani = new DS_ActionBusArguGroup(animationInfo,
				options,
				_selfRelatedBehaviourRef.GetRelatedArtHelper().SelfAnimationPlayResult,
				false);

			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_ani);
			}
			if (lookHateTarget != null)
			{
				QuickFaceHateTargetDirection(lookHateTarget.Value);
			}

			return ds_ani;
		}



		protected virtual DS_ActionBusArguGroup _Internal_RequirePostponeAutoDeduceTime(float postpone,
			bool autoLaunch = true)
		{
			var ds_postpone = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_AIDecision_DecisionRequireBrainAutoDeducePostpone_由决策要求推迟Brain的自动推演时间点);
			ds_postpone.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			ds_postpone.FloatArgu1 = postpone;
			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_postpone);
			}
			return ds_postpone;
		}



		protected virtual DS_ActionBusArguGroup _Internal_RequirePreemptDecision(SOConfig_AIDecision targetDecision, bool autoLaunch = true)
		{
			var ds_preempt =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AIDecision_DecisionRequirePreemptDecision_由决策要求抢占式执行决策);
			ds_preempt.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			ds_preempt.ObjectArgu2 = targetDecision;
			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_preempt);
			}
			return ds_preempt;
		}

		protected virtual DS_ActionBusArguGroup _Internal_RequireEnqueueDecision(SOConfig_AIDecision targetDecision,
			bool autoLaunch = true)
		{
			DS_ActionBusArguGroup ds_enqueue =
				new DS_ActionBusArguGroup(
					ActionBus_ActionTypeEnum.L_AIDecision_DecisionRequireQueueDecision_由决策要求排队一个决策);
			ds_enqueue.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			ds_enqueue.ObjectArgu2 = targetDecision;
			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_enqueue);
			}
			return ds_enqueue;
		}
		
		/// <summary>
		/// <para>当决策结束的清理工作。结束：仅仅是正常自行结束。不包括被打断、被抢占</para>
		/// <para>NormalStop表示这是否由决策自己发出的停止，[自己停止]则会应用正常的AfterStop操作，</para>
		/// </summary>
		public virtual DS_ActionBusArguGroup OnDecisionNormalComplete(bool autoLaunch = true, bool normalStop = true)
		{
			DecisionRunningNormal = false;
			LastDecisionEndTime = BaseGameReferenceService.CurrentFixedTime;
			foreach (var perDD in SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo
				.CommonComponents_RuntimeAll)
			{
				perDD.OnDecisionExit(SelfRelatedBrainInstanceRef);
			}
			foreach (BaseDecisionCommonComponent perDD in DCCConfig_OnStop.CommonComponents_RuntimeAll)
			{
				perDD.OnDecisionExit(SelfRelatedBrainInstanceRef);
			}

			if (DCCConfig_OnStop.CommonComponents_RuntimeAll != null && normalStop)
			{
				foreach (var component in DCCConfig_OnStop.CommonComponents_RuntimeAll)
				{
					component.EnterComponent(SelfRelatedBrainInstanceRef);
				}
			}

			if (DCCConfig_OnExit.CommonComponents_RuntimeAll != null)
			{
				foreach (var component in DCCConfig_OnExit.CommonComponents_RuntimeAll)
				{
					component.EnterComponent(SelfRelatedBrainInstanceRef);
				}
			}


			_collideWithWallAccumulateFrameCount = 0;
			
			if (normalStop)
			{
				_Internal_ReleaseBrainAutoDeduce();
				_Internal_RequireAutoDeduce(false);
			}
			var ds_stop = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AIDecision_DecisionComplete_决策自行结束);
			ds_stop.ObjectArgu1 = SelfRelatedDecisionRuntimeInstance;
			if (autoLaunch)
			{
				_selfLocalActionBusRef.TriggerActionByType(ds_stop);
			}
			return ds_stop;
		}


		/// <summary> objStr 是配置的名字(AnimationInfo 的 Config名字)，obj1是关联的AnimationArtHelper，obj2是使用的AnimationInfo</summary>
		protected virtual void _ABC_ProcessGeneralAnimationStart_OnAnimationStart(DS_ActionBusArguGroup ds)
		{
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(ds.ObjectArguStr as SOConfig_AIDecision))
			{
				return;
			}
			foreach (BaseDecisionCommonComponent perDCC in SelfRelatedDecisionRuntimeInstance.ConfigContent
				.DCCConfigInfo.CommonComponents_RuntimeAll)
			{
				if (perDCC.RequireAnimationMatching &&
				    perDCC.AnimationEventPreset == AnimationEventPresetEnumType.Start_开始 &&
				    perDCC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArgu2 as AnimationInfoBase).ConfigName,
					    StringComparison.OrdinalIgnoreCase))
				{
					perDCC.EnterComponent(SelfRelatedBrainInstanceRef);
				}
			}
		}

		/// <summary> objStr 是配置的名字(AnimationInfo 的 Config名字，【比对】)， obj1是关联的AnimationArtHelper,obj2是使用的AnimationInfo</summary>
		protected virtual void _ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(DS_ActionBusArguGroup ds)
		{
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(ds.ObjectArguStr as SOConfig_AIDecision))
			{
				return;
			}
			foreach (BaseDecisionCommonComponent perDCC in SelfRelatedDecisionRuntimeInstance.ConfigContent
				.DCCConfigInfo.CommonComponents_RuntimeAll)
			{
				if (perDCC.RequireAnimationMatching && perDCC.AnimationEventPreset == AnimationEventPresetEnumType.Complete_结束 &&
				    perDCC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArgu2 as AnimationInfoBase).ConfigName,
					    StringComparison.OrdinalIgnoreCase))
				{
					perDCC.EnterComponent(SelfRelatedBrainInstanceRef);
				}
			}
			
		}
		/// <summary>objStr是AnimationInfoBase， obj1是发起的AnimationHelper，obj2是自定义动画事件名</summary>
		protected virtual void _ABC_ProcessGeneralAnimationEvent_OnAnimationCustomEvent(DS_ActionBusArguGroup ds)
		{
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(ds.ObjectArgu1 as SOConfig_AIDecision))
			{
				return;
			}
			bool isFirst = ds.IntArgu1.HasValue && ds.IntArgu1.Value == 0;
			foreach (BaseDecisionCommonComponent perDCC in SelfRelatedDecisionRuntimeInstance.ConfigContent
				.DCCConfigInfo.CommonComponents_RuntimeAll)
			{
				if (perDCC.RequireAnimationMatching && perDCC.AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义 &&
				    perDCC._AN_RelatedAnimationConfigName.Equals((ds.ObjectArguStr as AnimationInfoBase).ConfigName, StringComparison.OrdinalIgnoreCase) &&
				    perDCC.CustomEventString.Trim().Equals(ds.ObjectArgu2 as string,StringComparison.OrdinalIgnoreCase))
				{
					perDCC.EnterComponent(SelfRelatedBrainInstanceRef);
				}
			}
		}

		// protected virtual SOConfig_AIDecision FindSelfDecisionByString(string str)
		// {
		// 	foreach (SOConfig_AIDecision perDecision in SelfRelatedBrainInstanceRef.ConfigContent.DecisionList)
		// 	{
		// 		if (string.Equals(str, perDecision.ConfigContent.DecisionID, StringComparison.OrdinalIgnoreCase))
		// 		{
		// 			return perDecision;
		// 		}
		// 	}
		// 	DBug.LogError(
		// 		$"丢失决策:{str}，来自脑{SelfRelatedBrainInstanceRef.name}，归属于角色{_selfRelatedBehaviourRef}");
		// 	return null;
		// }


		public virtual void QuickFaceHateTargetDirection(bool invert = false)
		{
			var fromPosition = _selfRelatedBehaviourRef.transform.position;
			if (!SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget())
			{
				return;
			}
			var hateTargetPos = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget().transform.position;
			hateTargetPos.y = 0f;
			fromPosition.y = 0f;
			Vector3 targetDire = (hateTargetPos - fromPosition).normalized;
			float dotResult = Vector3.Dot(BaseGameReferenceService.CurrentBattleLogicRightDirection, targetDire);
			//点乘大于0，则和右侧同向，朝右；
			var artHelperRef = (_selfRelatedBehaviourRef.GetSelfRolePlayArtHelper() as BaseARPGArtHelper);
			if (dotResult > 0f)
			{
				if (invert)
				{
					artHelperRef.SetFaceLeft(true);
				}
				else
				{
					artHelperRef.SetFaceLeft(false);
				}
			}
			else
			{
				if (invert)
				{
					artHelperRef.SetFaceLeft(false);
				}
				else
				{
					artHelperRef.SetFaceLeft(true);
				}
			}
		}
		
		//背向玩家
		public virtual void QuickBackHateTargetDirection(bool invert = false)
		{
			var fromPosition = _selfRelatedBehaviourRef.transform.position;
			if (!SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget())
			{
				return;
			}
			var hateTargetPos = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget().transform.position;
			hateTargetPos.y = 0f;
			fromPosition.y = 0f;
			Vector3 targetDire = (hateTargetPos - fromPosition).normalized;
			float dotResult = Vector3.Dot(BaseGameReferenceService.CurrentBattleLogicRightDirection, targetDire);
			//点乘大于0，则和右侧同向，朝左；
			var artHelperRef = (_selfRelatedBehaviourRef.GetSelfRolePlayArtHelper() as BaseARPGArtHelper);
			if (dotResult > 0f)
			{
				if (invert)
				{
					artHelperRef.SetFaceLeft(false);
				}
				else
				{
					artHelperRef.SetFaceLeft(true);
				}
			}
			else
			{
				if (invert)
				{
					artHelperRef.SetFaceLeft(true);
				}
				else
				{
					artHelperRef.SetFaceLeft(false);
				}
			}
		}
			
		public virtual void ClearOnUnload()
		{
			SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo.ClearOnUnload();
			DCCConfig_OnStop.ClearOnUnload();
			DCCConfig_OnBreak.ClearOnUnload();
			DCCConfig_OnExit.ClearOnUnload();


			if (_includeWallHitCheck)
			{
				_selfLocalActionBusRef.RemoveAction(
					ActionBus_ActionTypeEnum.L_Utility_MovementCollideWithAirWall_此次移动碰撞到了空气墙,
					_ABC_AccumulateHitWallFrameCount_OnCollideWithWall);
			}



		}

		protected AnimationInfoBase GetAnimationInfoFromBrain(string name)
		{
			return SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetRelatedAnimationInfo(name);
		}
		
		


		public Vector3 GetDirectionToHatredTarget()
		{
			//这里处理一下仇恨突然消失的情况。
			//通常来说这都不会出现，因为玩家换角色会触发事件来刷新所有敌人的仇恨。如果敌人的仇恨是玩家，会立刻刷新成新的角色。哪怕敌人的仇恨时召唤物，然后召唤物突然没了，也一定会刷新到玩家上
			//问题会出现在召唤物刷仇恨的时候。它会出现场上没有敌人的情况.这时候就要为它提供fallback数据
			//TODO : 不过目前版本（23.12）不会出现友军搜索敌人的情况，因为当前版本的召唤物并不会使用攻击决策或带朝向的响应
			var currentHatredTarget = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget();
			Vector3 targetPoint = currentHatredTarget.transform.position;
			Vector3 fromPos = _selfRelatedBehaviourRef.transform.position;
			fromPos.y = 0f;
			
			Vector3 direction_targetToFrom = (targetPoint - fromPos).normalized;

			
			return direction_targetToFrom;
			
		}
		protected Vector3 GetDirectionToPlayer()
		{
			Vector3 targetPoint = _playerControllerRef.CurrentControllingBehaviour.transform.position;
			targetPoint.y = 0f;
			Vector3 fromPos = _selfRelatedBehaviourRef.transform.position;
			fromPos.y = 0f;
			Vector3 direction_targetToFrom = (targetPoint - fromPos).normalized;
			return direction_targetToFrom;
		}

		/// <summary>
		/// <para>获取当前角色具体面片(Spine或Sheet)的面朝朝向，（其实就是世界左/世界右)</para>
		/// </summary>
		/// <returns></returns>
		public Vector3 GetSelfCharacterFaceDirection()
		{
			return _selfRelatedBehaviourRef.GetRelatedArtHelper().CurrentFaceLeft
				? BaseGameReferenceService.CurrentBattleLogicLeftDirection
				: BaseGameReferenceService.CurrentBattleLogicRightDirection;
		}


		
		
		
		/// <summary>
		/// <para>检查当前的脑、当前的角色是否还都有效，以及当前正在运行的决策是不是自己</para>
		/// </summary>
		protected virtual bool CheckIfCurrentBrainValidAndCurrentDecisionIsThis(SOConfig_AIDecision compareDecision)
		{
			if (SelfRelatedBrainInstanceRef == null)
			{
				return false;
			}
			if (!_selfRelatedBehaviourRef.CharacterDataValid)
			{
				return false;
			}
			if (_selfRelatedBehaviourRef is AllyARPGCharacterBehaviour ally)
			{
				if (ally.GetAIBrainRuntimeInstance() == null)
				{
					return false;
				}
			}
			else if (_selfRelatedBehaviourRef is EnemyARPGCharacterBehaviour enemy)
			{
				if (enemy.GetAIBrainRuntimeInstance() == null)
				{
					return false;
				}
			}
			if (compareDecision == null)
			{
				return false;
			}
			if (compareDecision != this.SelfRelatedDecisionRuntimeInstance)
			{
				return false;
			}
			if (compareDecision != SelfRelatedBrainInstanceRef.BrainHandlerFunction.CurrentRunningDecision)
			{
				return false;
			}
			return true;
		}

		protected float GetSelfToPlayerDistanceSQ()
		{
			var selfPos = _selfRelatedBehaviourRef.transform.position;
			var playerPos = _playerControllerRef.CurrentControllingBehaviour.transform.position;
			selfPos.y = 0f;
			playerPos.y = 0f;
			return Vector3.SqrMagnitude(selfPos - playerPos);
		}

		protected float GetSelfToHatredDistanceSQ()
		{
			
			var selfPos = _selfRelatedBehaviourRef.transform.position;
			var ht = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget();
			if(ht)
			{
				var hatredTarget = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget().transform
					.position;
				selfPos.y = 0f;
				hatredTarget.y = 0f;
				return Vector3.SqrMagnitude(selfPos - hatredTarget);
			}
			else
			{
				return 0f;
			}
		}

		protected virtual void _ABC_CheckIfNeedUnifyDamageStamp_OnOneLayoutSpawnOperation(DS_ActionBusArguGroup ds)
		{
			var relatedHandler = ds.ObjectArgu2 as BaseLayoutHandler;

			if (SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo.CommonComponents_RuntimeAll == null ||
			    SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo.CommonComponents_RuntimeAll.Count == 0)
			{
				return;
			}
			foreach (var perAEC in SelfRelatedDecisionRuntimeInstance.ConfigContent.DCCConfigInfo
				.CommonComponents_RuntimeAll)
			{
				if (perAEC is not DCC_生成版面_SpawnLayout spawnLayout)
				{
					continue;
				}
				if(!ReferenceEquals( relatedHandler.RelatedProjectileLayoutConfigRef.OriginalSOAssetTemplate, spawnLayout.RelatedConfig))
				{
					continue;
				}
				if (!spawnLayout._needSyncDamageTimeStamp)
				{
					continue;
				}
				if (!(spawnLayout.RelatedConfig.LayoutContentInSO.LayoutUID.Equals(relatedHandler
						.RelatedProjectileLayoutConfigRef.LayoutContentInSO.LayoutUID)))
				{
					continue;
				}

				var waitToSpawnList = ds.ObjectArgu1 as List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo>;
				foreach (var perWait in waitToSpawnList)
				{
					perWait.ProjectileBehaviour.OverrideDamageStamp = Time.frameCount + this.GetHashCode();
				}
			}
		}



//
// #if UNITY_EDITOR
// 		[Button("CopyAll")]
// 		public void CopyAll()
// 		{
// 		
// 			var allFileAEC =  UnityEditor.AssetDatabase.FindAssets("t:SOConfig_PresetEnemyAnimationEventCallback");
// 			
// 			foreach (var fileAEC in allFileAEC)
// 			{
// 				var fileAECPath = UnityEditor.AssetDatabase.GUIDToAssetPath(fileAEC);
// 				var fileAECObj = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PresetEnemyAnimationEventCallback>(
// 					fileAECPath);
// 				//创建一个新的SOConfig_PresetSideEffects
// 				var newSOConfig = ScriptableObject.CreateInstance<SOConfig_PresetSideEffects>();
// 				newSOConfig.name = fileAECObj.name + "_SideEffects";
// 				var newSOPath = fileAECPath;
// 				newSOPath = newSOPath.Replace(fileAECObj.name, newSOConfig.name);
//
// 				newSOConfig.DCCList = new List<BaseDecisionCommonComponent>();
//
// 				for (int i = 0; i < fileAECObj.AnimationEventCallbackList_Serialize.Count; i++)
// 				{
// 					BaseAnimationEventCallback perAEC = fileAECObj.AnimationEventCallbackList_Serialize[i];
// 					var newDCC = perAEC.DeepCopyTo(ref newSOConfig.DCCList);
// 					if (newDCC == null)
// 					{
// 						Debug.Log($"有一个NewDCC是空的，它是文件{fileAECObj.name}的第{i}个AEC，他可能是个触发DCC的AEC，手动迁移一下");
// 					}
// 					else
// 					{
// 						newDCC.RequireAnimationMatching = true;
// 						newDCC.AnimationEventPreset = perAEC.AnimationEventPreset;
// 						newDCC._AN_RelatedAnimationConfigName = perAEC._AN_RelatedAnimationConfigName;
// 						newDCC.CustomEventString = perAEC.CustomEventString;
// 					}
// 				}
//
// 				UnityEditor.AssetDatabase.CreateAsset(newSOConfig, newSOPath);
// 				
// 				 
// 			}
//
// 			//从AssetDatabase扫所有的 DecisionConfig
// 			var allDecisions = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
// 			var allFileDCC = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_PresetSideEffects");
// 			foreach (var decision in allDecisions)
// 			{
// 				var decisionPath = UnityEditor.AssetDatabase.GUIDToAssetPath(decision);
// 				var decisionObj = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(decisionPath);
//
// 				if (decisionObj.DecisionHandler == null)
// 				{
// 					continue;
// 				}
// 				if (decisionObj.DecisionHandler.AnimationEventCallbackList_Serialize != null)
// 				{
// 					if (decisionObj.ConfigContent.CommonComponents == null)
// 					{
// 						decisionObj.ConfigContent.CommonComponents = new List<BaseDecisionCommonComponent>();
// 					}
// 					for (int i = 0; i < decisionObj.DecisionHandler.AnimationEventCallbackList_Serialize.Count; i++)
// 					{
// 						BaseAnimationEventCallback perAEC =
// 							decisionObj.DecisionHandler.AnimationEventCallbackList_Serialize[i];
// 						var newDCC = perAEC.DeepCopyTo(ref decisionObj.ConfigContent.CommonComponents);
// 						if (newDCC == null)
// 						{
// 							Debug.Log($"有一个NewDCC是空的，它是文件{decisionObj.name}的第{i}个AEC，他可能是个触发DCC的AEC，手动迁移一下");
// 						}
// 						else
// 						{
// 							newDCC.RequireAnimationMatching = true;
// 							newDCC.AnimationEventPreset = perAEC.AnimationEventPreset;
// 							newDCC._AN_RelatedAnimationConfigName = perAEC._AN_RelatedAnimationConfigName;
// 							newDCC.CustomEventString = perAEC.CustomEventString;
// 						}
// 					}
// 				}
//
// 				if (decisionObj.DecisionHandler.AnimationEventCallbackList_File != null)
// 				{
// 					if (decisionObj.ConfigContent.CommonComponents_File == null)
// 					{
// 						decisionObj.ConfigContent.CommonComponents_File = new List<SOConfig_PresetSideEffects>();
// 					}
//
// 					for (int i = 0; i < decisionObj.DecisionHandler.AnimationEventCallbackList_File.Count; i++)
// 					{
// 						var perFile = decisionObj.DecisionHandler.AnimationEventCallbackList_File[i];
//
// 						foreach (string fileAEC in allFileDCC)
// 						{
// 							var fileAECPath = UnityEditor.AssetDatabase.GUIDToAssetPath(fileAEC);
// 							SOConfig_PresetSideEffects fileAECObj =
// 								UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PresetSideEffects>(fileAECPath);
// 							var originalFileName = perFile.name;
// 							var newFile = fileAECObj.name;
// 							if (originalFileName.Contains(newFile))
// 							{
// 								decisionObj.ConfigContent.CommonComponents_File.Add(fileAECObj);
// 							}
// 						}
// 					}
// 				}
//
// 				UnityEditor.EditorUtility.SetDirty(decisionObj);
// 			}
// 			 
// 			
//
// 			 
// 		}
// #endif
		
		 
		 
	}


}