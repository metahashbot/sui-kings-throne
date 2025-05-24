using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using Global;
using Global.ActionBus;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{


	[TypeInfoBox("如果选定这个决策但是仇恨目标为空，会立刻停止这个决策，但是作用依然会生效")]
	[Serializable]
	public class DH_通用常规攻击_CommonAttackWithAnimation : BaseDecisionHandler
	{
		[Header(" === 朝向 ===")]
		[FormerlySerializedAs("_facePlayerOnEnterDecision"), LabelText("    在进入决策时校准朝向到仇恨吗？"), FoldoutGroup("配置", true),
		 SerializeField, HideIf(nameof(_alwaysFaceHatredTarget))]
		protected bool _faceHatredTargetOnEnterDecision;

		[FormerlySerializedAs("_backPlayerOnEnterDecision"), LabelText("    在进入决策时校准背向到仇恨吗？"), FoldoutGroup("配置", true),
		 SerializeField, HideIf(nameof(_alwaysFaceHatredTarget))]
		protected bool _backHatredTargetOnEnterDecision;


		[FormerlySerializedAs("_alwaysFacePlayer"),]
		[LabelText("始终校准朝向到仇恨目标吗"), FoldoutGroup("配置", true), SerializeField]
		protected bool _alwaysFaceHatredTarget = true;


		[LabelText("校准时有最短间隔吗?"), FoldoutGroup("配置", true), SerializeField ,ShowIf(nameof(_alwaysFaceHatredTarget))]
		protected bool _faceHatredTargetContainInterval = true;

		[LabelText("    校准最短间隔"), FoldoutGroup("配置", true), SerializeField,
		 ShowIf("@(this._alwaysFaceHatredTarget && this._faceHatredTargetContainInterval)")]
		protected float _faceHatredTargetInterval = 0.5f;

		protected float _nextFaceTime;
		
		

		protected enum _AttackDHStateType
		{
			None = 0,
			Prepare_前摇 = 10,
			MiddleSingle_单次中段 = 21,
			MiddleLoop_循环中段 = 22,
			Post_后摇 = 30,
			OffsetLoop_垫动画循环的 =41,
			OffsetSingle_垫动画单次的 = 42,
		}

		protected _AttackDHStateType _attackStateType;


		//将要流转到下一个状态的时间点
		protected float _willProcessToNextStateTime;

		


		[Header(" === 攻击 ===")]
		[LabelText("包含攻击前摇?"), SerializeField, FoldoutGroup("配置",true)]
		protected bool _containAttackPrepareAnimation = false;
		

		[LabelText("    攻击前摇动画 配置名"), SerializeField, FoldoutGroup("配置", true),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf(nameof(_containAttackPrepareAnimation))]
		protected string _AN_AttackPrepare;

		[LabelText("[攻击动画]  配置名"), SerializeField, FoldoutGroup("配置", true),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		protected string _AN_AttackSpineAnimation;

		[LabelText("    √:按时间停止[攻击动画] | 口:按动画结束停止"),SerializeField, FoldoutGroup("配置", true)]
		protected bool _attackAnimationLoopAndStopByTime = false;

		[LabelText("    [攻击动画]动画持续时长"), ShowIf(nameof(_attackAnimationLoopAndStopByTime)),SerializeField,
		 FoldoutGroup("配置", true)]
		protected float _attackAnimationLoopDuration = 5f;

		
		[LabelText("包含攻击后摇"), SerializeField, FoldoutGroup("配置", true)]
		protected bool _containAttackEndAnimation = false;
        
		[LabelText("    攻击后摇动画 配置名"), SerializeField, FoldoutGroup("配置", true),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf(nameof(_containAttackEndAnimation))]
		protected string _AN_AttackEnd;
		
		
		
		[Header(" === 结束后动画 ===")]
		[InfoBox("[后续动画]不是后摇！这个是用来给攻击后垫时长的，比如攻击后发会儿呆。这段动画通常会循环播放，它将按照[时长]来判定完成" +
		         "，播放这个动画时依然视作攻击决策")]
		[LabelText("攻击结束后是否[垫]其他动画"), FoldoutGroup("配置", true), SerializeField]
		protected bool _addOtherAnimationAfterAttack;

		[LabelText("    其他动画 配置名"), SerializeField, FoldoutGroup("配置", true),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f), ShowIf(nameof(_addOtherAnimationAfterAttack))]
		protected string _an_otherSpineAnimation;

		[LabelText("√:按时间垫 | 口:按动画结束垫"), SerializeField, FoldoutGroup("配置", true),
		 ShowIf("@(this._addOtherAnimationAfterAttack)")]
		protected bool _offsetAnimationAsTimeDuration;
		
		[LabelText("    其他动画最小持续时长"), SerializeField, FoldoutGroup("配置", true),
		 ShowIf("@(this._addOtherAnimationAfterAttack && this._offsetAnimationAsTimeDuration)")]
		protected float _otherAnimationMinDuration = 0.5f;

		[LabelText("    其他动画最大持续时长"), SerializeField, FoldoutGroup("配置", true),
		 ShowIf("@(this._addOtherAnimationAfterAttack && this._offsetAnimationAsTimeDuration)")]
		protected float _otherAnimationMaxDuration = 1.5f;
		


		protected AnimationInfoBase _sai_attackTakeEffect;
		protected AnimationInfoBase _sai_attackPrepare;
		protected AnimationInfoBase _sai_attackEnd;


		[ShowInInspector, LabelText("进入决策时获得的仇恨方向"), FoldoutGroup("运行时", true),NonSerialized]
		public Vector3 ToHatredTargetDirectionOnEnterDecision;


		public override void Initialize(SOConfig_AIBrain config)
		{
			base.Initialize(config);
			_sai_attackTakeEffect = GetAnimationInfoFromBrain(_AN_AttackSpineAnimation);
			if (_containAttackPrepareAnimation)
			{
				_sai_attackPrepare = GetAnimationInfoFromBrain(_AN_AttackPrepare);
			}
			if (_containAttackEndAnimation)
			{
				_sai_attackEnd = GetAnimationInfoFromBrain(_AN_AttackEnd);
			}
			_attackStateType = _AttackDHStateType.None;


		}


		public override DS_ActionBusArguGroup OnDecisionNormalComplete(bool autoLaunch = true, bool normalStop = true)
		{
			SelfRelatedBrainInstanceRef.BrainHandlerFunction.RefreshHatredTarget();
			_attackStateType = _AttackDHStateType.None;
			return base.OnDecisionNormalComplete(autoLaunch, normalStop);
			
		}



		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds_start = base.OnDecisionBeforeStartExecution(false);


			_Internal_BlockBrainAutoDeduce();

			BaseARPGCharacterBehaviour currentHatredTarget =
				SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget();
			if (currentHatredTarget)
			{
				ToHatredTargetDirectionOnEnterDecision =
					(_playerControllerRef.CurrentControllingBehaviour.transform.position -
					 _selfRelatedBehaviourRef.transform.position).normalized;
				if (_faceHatredTargetOnEnterDecision)
				{
					QuickFaceHateTargetDirection();
				}
				else if (_backHatredTargetOnEnterDecision)
				{
					QuickBackHateTargetDirection();
				}
				
				ProcessCommonSideComponents();


				if (_containAttackPrepareAnimation)
				{
					_attackStateType = _AttackDHStateType.Prepare_前摇;
					_Internal_RequireAnimation(_sai_attackPrepare);
				}
				else
				{
					_Internal_RequireAnimation(_sai_attackTakeEffect);
					if (_attackAnimationLoopAndStopByTime)
					{
						_attackStateType = _AttackDHStateType.MiddleLoop_循环中段;
						_willProcessToNextStateTime =
							BaseGameReferenceService.CurrentFixedTime + _attackAnimationLoopDuration;
					}
					else
					{
						_attackStateType = _AttackDHStateType.MiddleSingle_单次中段;
						
					}
				}


				return ds_start;
			}
			else
			{
				ProcessCommonSideComponents();
				DBug.Log(
					$"因为没有仇恨目标。【决策立刻终止！】来自角色{_selfRelatedBehaviourRef.name}的决策{SelfRelatedDecisionRuntimeInstance.name},");
				_willDecisionImmediateEnd = true;

				return ds_start;
			}
		}

		
		



		protected override void _ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(DS_ActionBusArguGroup ds)
		{
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(ds.ObjectArguStr as SOConfig_AIDecision))
			{
				return;
			}
			base._ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(ds);
			switch (_attackStateType)
			{
				case _AttackDHStateType.Prepare_前摇:
					_Internal_RequireAnimation(_sai_attackTakeEffect);
					if (_attackAnimationLoopAndStopByTime)
					{
						_attackStateType = _AttackDHStateType.MiddleLoop_循环中段;
						_willProcessToNextStateTime =
							BaseGameReferenceService.CurrentFixedTime + _attackAnimationLoopDuration;
					}
					else
					{
						_attackStateType = _AttackDHStateType.MiddleSingle_单次中段;
						
					}
					break;
				case _AttackDHStateType.MiddleSingle_单次中段:
					if (_containAttackEndAnimation)
					{
						_Internal_RequireAnimation(_sai_attackEnd);
						_attackStateType = _AttackDHStateType.Post_后摇;
					}
					else if (_addOtherAnimationAfterAttack)
					{
						PlayOffsetAnimation();
					}
					else
					{
						OnDecisionNormalComplete();
					}
					break;
				case _AttackDHStateType.MiddleLoop_循环中段:
					break;
				case _AttackDHStateType.Post_后摇:
					//后摇结束，如果不垫动画，那就直接结束
					
						OnDecisionNormalComplete();
					
					break;
				case _AttackDHStateType.OffsetLoop_垫动画循环的:
					break;
				case _AttackDHStateType.OffsetSingle_垫动画单次的:
					OnDecisionNormalComplete();
					break;
			}
		}

		
		/// <summary>
		/// 垫动画
		/// </summary>
		void PlayOffsetAnimation()
		{
			//垫动画
			//按时间垫
			_Internal_RequireAnimation(GetAnimationInfoFromBrain(_an_otherSpineAnimation));
			if (_offsetAnimationAsTimeDuration)
			{
				_willProcessToNextStateTime = BaseGameReferenceService.CurrentFixedTime +
				                              UnityEngine.Random.Range(_otherAnimationMinDuration,
					                              _otherAnimationMaxDuration);
				_attackStateType = _AttackDHStateType.OffsetLoop_垫动画循环的;
			}
			else
			{
				_attackStateType = _AttackDHStateType.OffsetSingle_垫动画单次的;
			}
		}

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);

			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(SelfRelatedDecisionRuntimeInstance))
			{
				return;
			}
			
			if (_willDecisionImmediateEnd)
			{
				_willDecisionImmediateEnd = false;
				OnDecisionNormalComplete();
			}

			if (_alwaysFaceHatredTarget)
			{
				if (_faceHatredTargetContainInterval)
				{
					if (ct > _nextFaceTime)
					{
						QuickFaceHateTargetDirection();
						_nextFaceTime = ct + _faceHatredTargetInterval;
					}
				}
				else
				{
					QuickFaceHateTargetDirection();
				}
			}

			switch (_attackStateType)
			{
				case _AttackDHStateType.MiddleLoop_循环中段:
					if (ct > _willProcessToNextStateTime)
					{
						if (_addOtherAnimationAfterAttack)
						{
							PlayOffsetAnimation();
						}
						else
						{
							OnDecisionNormalComplete();
						}
					}
					break;
				case _AttackDHStateType.OffsetLoop_垫动画循环的:
					if (ct > _willProcessToNextStateTime)
					{
						OnDecisionNormalComplete();
						_willProcessToNextStateTime = float.MaxValue;
					}
					break;
			}
		}




		public override void ClearOnUnload()
		{
			base.ClearOnUnload();
		}
#if UNITY_EDITOR
		

		
		// [Button("转换攻击决策实现")]
		// public void Convert()
		// {
		//
		// 	var soConfigs = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
		//
		// 	foreach (var perGUID in soConfigs)
		// 	{
		// 		var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
		// 		SOConfig_AIDecision perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perPath);
		// 		if (perSO.DecisionHandler is not DH_通用常规攻击_CommonAttackWithAnimation attack)
		// 		{
		// 			continue;
		// 		}
		// 		List<BaseAnimationEventCallback> list =
		// 			attack.AnimationEventCallbackList = new List<BaseAnimationEventCallback>();
		// 		foreach (PerAttackInfo perAttackInfo in attack._attackInfoList)
		// 		{
		// 			if (perAttackInfo.NeedLayoutConfig)
		// 			{
		// 				list.Add(new AEC_生成版面_SpawnLayout
		// 				{
		// 					_AN_RelatedAnimationConfigName = attack._AN_AttackSpineAnimation,
		// 					AnimationEventPreset = AnimationEventPresetEnumType.Custom_自定义,
		// 					CustomEventString = "TakeEffect",
		// 					RelatedConfig = perAttackInfo._selfRelatedConfig,
		// 					_overrideDirectionFromLayout = perAttackInfo._overrideDirectionFromLayout,
		// 					_attackDirectionRelatedToHatredTarget = perAttackInfo._attackDirectionRelatedToHatredTarget,
		// 					UseDirectionOnEnter = perAttackInfo.UseDirectionOnEnter
		// 				});
		// 			}
		// 		}
		// 		UnityEditor.EditorUtility.SetDirty(perSO);
		// 				UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 	}
		// 	UnityEditor.AssetDatabase.Refresh();
		// }

#endif


// [Button("转换其他类似的行为")]
// 		public void Convert()
// 		{
// 			//find all soconfig ai decision by asset database
// 			var soConfigs = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
// 			foreach (var perGUID in soConfigs)
// 			{
// 				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
// 				SOConfig_AIDecision perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perPath);
// 				DH_通用常规攻击_CommonAttackWithAnimation newCM = new DH_通用常规攻击_CommonAttackWithAnimation();
// 				switch (perSO.DecisionHandler)
// 				{
// 					case MeleeAttack_601001 meleeAttack601001:
// 						newCM._attackInfoList[0]._selfRelatedConfig = meleeAttack601001.RelatedConfig;
// 						newCM._attackInfoList[0]._attackDirectionRelatedToHatredTarget =
// 							meleeAttack601001.FacePlayerOnSpawnLayout;
// 						newCM._AN_AttackSpineAnimation = meleeAttack601001._AN_AttackSpineAnimation;
// 						if (meleeAttack601001._did_QuickStepBack != null)
// 						{
// 							newCM._addOtherAnimationAfterAttack = false;
// 							newCM._hasExtraActionAfterStop = true;
// 							newCM._sideEffectOnStopList.Add(new DCC_要求排队决策_RequireQueueDecision
// 							{
// 								TargetDecisionNameList = new List<string> { meleeAttack601001._did_QuickStepBack }
// 							});
// 						}
// 						else if (meleeAttack601001._AN_IdleSpineAnimation != null)
// 						{
// 							newCM._addOtherAnimationAfterAttack = true;
// 							newCM._an_otherSpineAnimation = meleeAttack601001._AN_IdleSpineAnimation;
// 							newCM._otherAnimationMinDuration = meleeAttack601001._idleAfterAttackMinDuration;
// 							newCM._otherAnimationMaxDuration = meleeAttack601001._idleAfterAttackMaxDuration;
// 						}
// 						perSO.DecisionHandler = newCM;
//
//
// 						UnityEditor.EditorUtility.SetDirty(perSO);
// 						UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
// 						break;
// 					case DecisionHandler_CommonAttackWithLayoutAndDelay ca:
// 						newCM._attackInfoList[0]._selfRelatedConfig = ca.RelatedConfig;
// 						newCM._attackInfoList[0]._attackDirectionRelatedToHatredTarget = ca.FacePlayerOnSpawnLayout;
// 						newCM._AN_AttackSpineAnimation = ca._AN_AttackSpineAnimation;
// 						if (ca._AN_IdleSpineAnimation != null)
// 						{
// 							newCM._addOtherAnimationAfterAttack = true;
// 							newCM._an_otherSpineAnimation = ca._AN_IdleSpineAnimation;
// 							newCM._otherAnimationMinDuration = ca._idleAfterAttackMinDuration;
// 							newCM._otherAnimationMaxDuration = ca._idleAfterAttackMaxDuration;
// 						}
// 						perSO.DecisionHandler = newCM;
//
//
// 						UnityEditor.EditorUtility.SetDirty(perSO);
// 						UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
// 						break;
// 				}
// 			}
// 		}

	}
}