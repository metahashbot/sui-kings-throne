using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Buff;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[TypeInfoBox("如果移动时没有仇恨目标，则会使用同样配置数据但与仇恨目标不相关的方式移动")]
	[Serializable]
	public class DH_通用常规移动_CommonMovement : BaseDecisionHandler
	{


		[ValidateInput("@this._an_MoveAnimationName != null", "！没有配置动画名！")]
		[LabelText("使用的动画配置名"), SerializeField, FoldoutGroup("配置", true), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		protected string _an_MoveAnimationName;


		[Header(" === 转身 ===")]
		[SerializeField, LabelText("需要【试图转身】阶段"), FoldoutGroup("配置", true)]
		protected bool _requireTurnFaceAnimation = false;
		 
		[InfoBox("如果有【试图转身】，则自行结束必定都在转身过程结束之后才有可能进行")]
		[SerializeField,LabelText("  转身动画"),ShowIf(nameof(_requireTurnFaceAnimation)), FoldoutGroup("配置", true),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		protected string _an_TurnFaceAnimationName;



		protected enum MovementPhaseTypeEnum
		{
			None_未指定 = 0, TurningFace_正在转身 = 1, Moving_正在移动 = 2
		}

		protected MovementPhaseTypeEnum _currentMovementPhase = MovementPhaseTypeEnum.None_未指定;


		[Header(" === 速度 ===")]
		[SerializeField, LabelText("直接覆盖原始速度？"), FoldoutGroup("配置", true)]
		protected bool _isOverrideMoveSpeed;

		[SerializeField, LabelText("        覆盖的移动速度"), FoldoutGroup("配置", true), ShowIf(nameof(_isOverrideMoveSpeed))]
		protected float _overrideMoveSpeed;

		[SerializeField, LabelText("        移动速度倍率乘数"), FoldoutGroup("配置", true), HideIf(nameof(_isOverrideMoveSpeed))]
		protected float _moveSpeedMul = 1f;

		[Header(" === 位置 ===")]
		[SerializeField,LabelText("√：仅与玩家有关 || 口：可选仇恨目标或自身位置"),FoldoutGroup("配置",true)]
		protected bool _destOnlyRelatedToPlayer = false;


		[FormerlySerializedAs("_destNeedRelatedToPlayer")]
		[SerializeField, LabelText("目的地计算起始基于 √：仇恨目标位置 || 口：自身位置"), FoldoutGroup("配置", true),
		 HideIf(nameof(_destOnlyRelatedToPlayer))]
		protected bool _destNeedRelatedToHatredTarget = true;
		
		


		[SerializeReference, LabelText("此次移动最小路程"), FoldoutGroup("配置", true)]
		protected DecisionDistanceInfoComponent _distance_min = new DD_自身到仇恨目标距离_CurrentDistanceToHateTarget()
		{
			ValueMultiplier = 0.5f,
		};

		[SerializeReference, LabelText("此次移动最大路程"), FoldoutGroup("配置", true)]
		protected DecisionDistanceInfoComponent _distance_max = new DD_自身到仇恨目标距离_CurrentDistanceToHateTarget
		{
			ValueMultiplier = 1,
		};

		[FormerlySerializedAs("_angleRangeLimitToPlayer"),SerializeField, LabelText("        角度计算起始基于 √:仇恨到自身方向 || 口:自身到仇恨方向"), FoldoutGroup("配置", true)]
		protected bool _angleRangeLimitToHateTarget = true;


		[SerializeField, LabelText("        角度范围（顺逆时针各这么多）"), FoldoutGroup("配置", true)]
		protected float _angleRange = 45f;


		[Header(" === 结束 ===")]
		[SerializeField, LabelText("结束：最大持续时间限制"), FoldoutGroup("配置", true)]
		protected bool _stop_RelatedToMaxDuration = true;

		[SerializeField, LabelText("        移动最小时间"), FoldoutGroup("配置", true),
		 ShowIf(nameof(_stop_RelatedToMaxDuration))]
		protected float _minMovementDuration = 0.5f;

		[SerializeField, LabelText("        移动最大时间"), FoldoutGroup("配置", true),
		 ShowIf(nameof(_stop_RelatedToMaxDuration))]
		protected float _maxMovementDuration = 1.5f;


		[Space]
		[SerializeField, LabelText("结束：达到目的地"), FoldoutGroup("配置", true)]
		protected bool _stop_RelatedToDestination = true;

		[Space]
		[SerializeField, LabelText("结束：过于靠近玩家"), FoldoutGroup("配置", true)]
		protected bool _stop_RelatedToPlayerDistance = true;

		[SerializeReference, LabelText("        当距离玩家小于该值时，结束"), FoldoutGroup("配置", true),
		 ShowIf(nameof(_stop_RelatedToPlayerDistance))]
		protected DecisionDistanceInfoComponent _stop_RelatedDistanceValue = new DD_纯数值_PureValue
		{
			AbsoluteValue = 1f, ValueMultiplier = 1f
		};

		[Space]
		[SerializeReference, LabelText("结束：过于靠近仇恨目标"), FoldoutGroup("配置", true)]
		protected bool _stop_RelatedToHatredTarget = true;
		
		[SerializeReference, LabelText("        当距离仇恨目标小于该值时，结束"), FoldoutGroup("配置", true),
		 ShowIf(nameof(_stop_RelatedToHatredTarget))]
		protected DecisionDistanceInfoComponent _stop_RelatedDistanceValue_HatredTarget = new DD_纯数值_PureValue
		{
			AbsoluteValue = 1f, ValueMultiplier = 1f
		};



		[FormerlySerializedAs("_isMoveDirectionRelatedToPlayer"),Header(" === 朝向 ===")]
		[SerializeField, LabelText("移动时朝向 √与仇恨有关 || 口 只与速度有关"), FoldoutGroup("配置", true)]
		protected bool _isMoveDirectionRelatedToHatredTarget = true;

		[FormerlySerializedAs("_moveDirection_FacePlayer"),SerializeField, LabelText("        √：面对仇恨目标 ||  口：背对仇恨目标"), FoldoutGroup("配置", true),
		 ShowIf(nameof(_isMoveDirectionRelatedToHatredTarget)), FoldoutGroup("配置", true)]
		protected bool _moveDirection_FaceHatredTarget;



		[SerializeField, LabelText("        √：面向移动方向  ||  口：背对移动方向"), FoldoutGroup("配置", true),
		 HideIf(nameof(_isMoveDirectionRelatedToHatredTarget))]
		protected bool _faceMoveSpeed = true;

		
		[SerializeField,LabelText("    √:最小翻面时间 | 口:无需最小翻面时间"), FoldoutGroup("配置", true)]
		protected bool _faceNeedMinDuration = true;
		 
		[SerializeField,LabelText("        最小翻面时间"),ShowIf(nameof(_faceNeedMinDuration)), FoldoutGroup("配置", true)]
		protected float _faceMinDuration = 0.5f;

		//下一次能转身的时间
		protected float _nextTurnFaceChangeTime;


		protected BaseARPGArtHelper _selfArtHelperRef;
		protected Float_RPDataEntry _entry_MoveSpeed;


		protected float _movementEndTime;

		protected Vector3 _movementTargetPosition;


		public override void Initialize(SOConfig_AIBrain config)
		{
			base.Initialize(config);
			_selfRelatedBehaviourRef = config.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			_selfArtHelperRef = _selfRelatedBehaviourRef.GetSelfRolePlayArtHelper() as BaseARPGArtHelper;

			_entry_MoveSpeed = _selfRelatedBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速);
			_playerControllerRef = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
		}


		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds_start = base.OnDecisionBeforeStartExecution(withSideEffect);
			_Internal_BlockBrainAutoDeduce();
			_nextTurnFaceChangeTime = BaseGameReferenceService.CurrentFixedTime;
			
			PickPosition();
			if (_requireTurnFaceAnimation)
			{
				_currentMovementPhase = MovementPhaseTypeEnum.TurningFace_正在转身;
				//看看是不是需要【试图转身】
				var targetPos = _movementTargetPosition;
				var selfPos = _selfRelatedBehaviourRef.transform.position;
				var currentFaceLeft = _selfRelatedBehaviourRef.GetRelatedArtHelper().CurrentFaceLeft;
				var targetOnLeft = targetPos.x < selfPos.x;
				bool needToTurn = false;
				if ( (currentFaceLeft && !targetOnLeft) || (!currentFaceLeft && targetOnLeft))
				{
					needToTurn = true;
				}
				
				
				if (needToTurn)
				{
					//需要的，那先播转身动画  
					
					SelfRelatedBrainInstanceRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour
						.GetRelatedArtHelper().SetFaceLeft(targetOnLeft);
					_Internal_RequireAnimation(GetAnimationInfoFromBrain(_an_TurnFaceAnimationName)); 
					
				}
				else
				{
					//实际上并不需要试图转身，那就和以前一样

					_currentMovementPhase = MovementPhaseTypeEnum.Moving_正在移动;
					_Internal_RequireAnimation(GetAnimationInfoFromBrain(_an_MoveAnimationName), true, true);
					SetSelfFaceDirection();
					//结束：最大持续时间
					if (_stop_RelatedToMaxDuration)
					{
						_movementEndTime = BaseGameReferenceService.CurrentFixedTime +
						                   UnityEngine.Random.Range(_minMovementDuration, _maxMovementDuration);
					}

				}
				
			}
			else
			{
				_currentMovementPhase = MovementPhaseTypeEnum.Moving_正在移动;
				_Internal_RequireAnimation(GetAnimationInfoFromBrain(_an_MoveAnimationName), true, true);
				SetSelfFaceDirection();
				//结束：最大持续时间
				if (_stop_RelatedToMaxDuration)
				{
					_movementEndTime = BaseGameReferenceService.CurrentFixedTime +
					                   UnityEngine.Random.Range(_minMovementDuration, _maxMovementDuration);
				}


			}
			
			
			return ds_start;
		}



		/// <summary>
		/// 选择移动位置
		/// </summary>
		private void PickPosition()
		{

			Vector3 globalRight = BaseGameReferenceService.CurrentBattleLogicRightDirection;

			var playerPos = _playerControllerRef.CurrentControllingBehaviour.transform.position;
			var selfPos = _selfRelatedBehaviourRef.transform.position;
			var ht = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget();
			float distanceLength = UnityEngine.Random.Range(_distance_min.GetDistanceValue(SelfRelatedBrainInstanceRef),
				_distance_max.GetDistanceValue(SelfRelatedBrainInstanceRef));
			
			if (_destOnlyRelatedToPlayer)
			{
				float angle = 0f;

				angle = UnityEngine.Random.Range(-_angleRange, _angleRange);

				var fromPos = selfPos;
				Vector3 targetPoint = playerPos;
				targetPoint.y = 0f;
				fromPos.y = 0f;
				Vector3 direction_targetToFrom = Vector3.zero;
				if (_angleRangeLimitToHateTarget)
				{
					direction_targetToFrom = (fromPos - targetPoint).normalized;
				}
				else
				{
					direction_targetToFrom = (targetPoint - fromPos).normalized;
				}
				Vector3 rotateR = MathExtend.Vector3RotateOnXOZ(direction_targetToFrom, angle);
				Vector3 result = targetPoint + rotateR * distanceLength;

				_movementTargetPosition = result;
				
			}
			else
			{

			
				//在仇恨周围选点，
				if (_destNeedRelatedToHatredTarget && ht)
				{
					float angle = 0f;

					angle = UnityEngine.Random.Range(-_angleRange, _angleRange);

					var fromPos = selfPos;
					Vector3 targetPoint = ht.transform.position;
					targetPoint.y = 0f;
					fromPos.y = 0f;
					Vector3 direction_targetToFrom = Vector3.zero;
					if (_angleRangeLimitToHateTarget)
					{
						direction_targetToFrom = (fromPos - targetPoint).normalized;
					}
					else
					{
						direction_targetToFrom = (targetPoint - fromPos).normalized;
					}
					Vector3 rotateR = MathExtend.Vector3RotateOnXOZ(direction_targetToFrom, angle);
					Vector3 result = targetPoint + rotateR * distanceLength;

					_movementTargetPosition = result;
				}
				//在自己周围选点
				else
				{
					float angle = UnityEngine.Random.Range(-_angleRange, _angleRange);
					Vector3 direction_targetToFrom = Vector3.zero;

					if (ht)
					{

						Vector3 tmpHatredTargetPos = ht.transform.position;
						Vector3 tmpSelfPos = selfPos;
						tmpHatredTargetPos.y = 0f;
						tmpSelfPos.y = 0f;
						//是玩家到自己的方向 ： 0为远离玩家的方向
						if (_angleRangeLimitToHateTarget)
						{
							direction_targetToFrom = (tmpHatredTargetPos - tmpSelfPos).normalized;
						}
						//是自己到玩家的方向： 0 为靠近玩家的方向
						else
						{
							direction_targetToFrom = (tmpSelfPos - tmpHatredTargetPos).normalized;
						}
					}
					else
					{
						direction_targetToFrom =
							new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
					}

					Vector3 rotateR = MathExtend.Vector3RotateOnXOZ(direction_targetToFrom, angle);
					Vector3 result = selfPos + rotateR * distanceLength;
					_movementTargetPosition = result;
				}
			}
			
		}



		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);

			if (_currentMovementPhase != MovementPhaseTypeEnum.Moving_正在移动)
			{
				return;
			}
			
			
			Vector3 globalRight = BaseGameReferenceService.CurrentBattleLogicRightDirection;

			float moveSpeed = GetMovementSpeed();
			
			var movementTP = _movementTargetPosition;
			movementTP.y = 0f;
			var selfPos = _selfRelatedBehaviourRef.transform.position;
			selfPos.y = 0f;

			Vector3 movementDir = (movementTP - selfPos).normalized;
			Vector3 movementDelta = movementDir * (delta * moveSpeed);
			_selfRelatedBehaviourRef.TryMovePosition_OnlyXZ(movementDelta, true, 3f);



			if (_stop_RelatedToDestination)
			{
				if (Vector3.SqrMagnitude(selfPos - movementTP) < 0.01f)
				{
					OnDecisionNormalComplete();
					return;
				}
			}

			if (_stop_RelatedToPlayerDistance)
			{
				var playerPos = _playerControllerRef.CurrentControllingBehaviour.transform.position;
				playerPos.y = 0f;
				if (Vector3.SqrMagnitude(selfPos - playerPos) < 0.01f)
				{
					OnDecisionNormalComplete();
					return;
				}
			}

			if (_stop_RelatedToHatredTarget && SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget())
			{
				var hatredTargetPos =
					SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget().transform.position;
				hatredTargetPos.y = 0f;
				if (Vector3.SqrMagnitude(selfPos - hatredTargetPos) < 0.01f)
				{
					OnDecisionNormalComplete();
					return;
				}
			}
			
			if (_stop_RelatedToMaxDuration)
			{
				if (ct > _movementEndTime)
				{
					OnDecisionNormalComplete();
					return;
				}
			}

			if (!_faceNeedMinDuration)
			{
				SetSelfFaceDirection();
			}
			else
			{
				if (ct > _nextTurnFaceChangeTime)
				{
					_nextTurnFaceChangeTime = ct + _faceMinDuration;
					SetSelfFaceDirection();
				}
				
			}
		}
		protected virtual void SetSelfFaceDirection()
		{
			Vector3 globalRight = BaseGameReferenceService.CurrentBattleLogicRightDirection;
			var selfPos = _selfRelatedBehaviourRef.transform.position;
			selfPos.y = 0f;

			var movementTP = _movementTargetPosition;
			movementTP.y = 0f;
			Vector3 movementDir = (movementTP - selfPos).normalized;
			var ht = SelfRelatedBrainInstanceRef.BrainHandlerFunction.GetCurrentHatredTarget();
			//如果是朝着玩家，那实际上和移动方向没有关系，只需要看着玩家就行了
			if (_isMoveDirectionRelatedToHatredTarget && ht)
			{
				var hatredPos = ht.transform.position;
				hatredPos.y = 0f;
				Vector3 dir_selfToHatred = (hatredPos - selfPos).normalized;
				float dotResult = Vector3.Dot(dir_selfToHatred, globalRight);

				//和仇恨有关，并且需要朝着仇恨
				if (_moveDirection_FaceHatredTarget)
				{
					//点乘大于0，则和右侧同向，朝右；
					if (dotResult > 0f)
					{
						_selfArtHelperRef.SetFaceLeft(false);
					}
					else
					{
						_selfArtHelperRef.SetFaceLeft(true);
					}
				}
				//背对仇恨
				else
				{
					//点乘大于0，则和右侧同向，朝左；
					if (dotResult > 0f)
					{
						_selfArtHelperRef.SetFaceLeft(true);
					}
					else
					{
						_selfArtHelperRef.SetFaceLeft(false);
					}
				}
			}
			else
			{
				float dotResult = Vector3.Dot(globalRight, movementDir);
				if (_faceMoveSpeed)
				{
					//点乘大于0，朝右
					_selfArtHelperRef.SetFaceLeft(dotResult < 0f);
				}
				else
				{
					_selfArtHelperRef.SetFaceLeft(dotResult > 0f);
				}
			}
		}

		protected virtual float GetMovementSpeed()
		{
			if (_isOverrideMoveSpeed)
			{
				return _overrideMoveSpeed;
			}
			else
			{
				return _entry_MoveSpeed.CurrentValue * _moveSpeedMul;
			}
		}


		protected override void _ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(DS_ActionBusArguGroup ds)
		{
			base._ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(ds);
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(ds.ObjectArguStr as SOConfig_AIDecision))
			{
				return;
			}
			if (_currentMovementPhase == MovementPhaseTypeEnum.TurningFace_正在转身)
			{
				_currentMovementPhase = MovementPhaseTypeEnum.Moving_正在移动;
				_Internal_RequireAnimation(GetAnimationInfoFromBrain(_an_MoveAnimationName), true, false);
				if (_stop_RelatedToMaxDuration)
				{
					_movementEndTime = BaseGameReferenceService.CurrentFixedTime +
					                   UnityEngine.Random.Range(_minMovementDuration, _maxMovementDuration);
				}
				
			}
		}

#if UNITY_EDITOR

		// [Button("转换其他类似的行为")]
		// public void Convert()
		// {
		// 	//find all soconfig ai decision by asset database
		// 	var soConfigs = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
		// 	foreach (var perGUID in soConfigs)
		// 	{
		// 		var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
		// 		SOConfig_AIDecision perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perPath);
		// 		DH_通用常规移动_CommonMovement newCM = new DH_通用常规移动_CommonMovement();
		// 		switch (perSO.DecisionHandler)
		// 		{
		// 			case Approach_601001 approach601001:
		// 				newCM._distance_min = new DD_自身到玩家距离_CurrentDistanceToPlayer
		// 				{
		// 					ValueMultiplier = 0.1f
		// 				};
		// 				newCM._distance_max = new DD_自身到玩家距离_CurrentDistanceToPlayer
		// 				{
		// 					ValueMultiplier = 0.25f
		// 				};
		// 				newCM._destNeedRelatedToHatredTarget = true;
		// 				newCM._angleRange = approach601001._range_Angle;
		// 				newCM._moveSpeedMul = approach601001._speedMul;
		// 				newCM._minMovementDuration = approach601001._maxDuration;
		// 				newCM._maxMovementDuration = approach601001._maxDuration;
		// 				newCM._an_MoveAnimationName = approach601001._an_moveAnimationName;
		// 				perSO.DecisionHandler = newCM;
		// 				UnityEditor.EditorUtility.SetDirty(perSO);
		// 				UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 				break;
		// 			case CommonWander_DecisionHandler commonWanderDecisionHandler:
		// 				newCM._an_MoveAnimationName = commonWanderDecisionHandler._an_MoveAnimationName;
		// 				newCM._destNeedRelatedToHatredTarget = commonWanderDecisionHandler._needRelatedToPlayer;
		// 				newCM._distance_min = new DD_纯数值_PureValue
		// 					{ ValueMultiplier = 1f };
		// 				newCM._distance_max = new DD_纯数值_PureValue
		// 					{ ValueMultiplier = commonWanderDecisionHandler._wanderRange };
		// 				newCM._angleRangeLimitToHateTarget = commonWanderDecisionHandler._needAngleRangeLimit;
		// 				newCM._angleRange = commonWanderDecisionHandler._angleRange;
		// 				newCM._moveSpeedMul = commonWanderDecisionHandler._moveSpeedMul;
		// 				newCM._stop_RelatedToMaxDuration = true;
		// 				newCM._minMovementDuration = commonWanderDecisionHandler._minWanderDuration;
		// 				newCM._maxMovementDuration = commonWanderDecisionHandler._maxWanderDuration;
		// 				newCM._moveDirection_FaceHatredTarget = commonWanderDecisionHandler._facePlayer;
		// 				perSO.DecisionHandler = newCM;
		// 				UnityEditor.EditorUtility.SetDirty(perSO);
		// 				UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 				break;
		// 			case DecisionHandler_CommonApproachDirect decisionHandlerCommonApproachDirect:
		// 				newCM._stop_RelatedToPlayerDistance = true;
		// 				newCM._isOverrideMoveSpeed = decisionHandlerCommonApproachDirect.OverrideChaseSpeed;
		// 				newCM._overrideMoveSpeed = decisionHandlerCommonApproachDirect.ChaseSpeed;
		// 				newCM._stop_RelatedToMaxDuration = true;
		// 				newCM._minMovementDuration = decisionHandlerCommonApproachDirect.ChaseMinDuration;
		// 				newCM._maxMovementDuration = decisionHandlerCommonApproachDirect.ChaseMaxDuration;
		// 				newCM._an_MoveAnimationName = decisionHandlerCommonApproachDirect._AN_RunAnimationName;
		//
		// 				perSO.DecisionHandler = newCM;
		// 				UnityEditor.EditorUtility.SetDirty(perSO);
		// 				UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 				break;
		// 			case DecisionHandler_PaceFromCurrent decisionHandlerPaceFromCurrent:
		// 				newCM._distance_min = decisionHandlerPaceFromCurrent._minDistance.DeepCopyAsNew(
		// 					decisionHandlerPaceFromCurrent._minDistance);
		// 				newCM._distance_max = decisionHandlerPaceFromCurrent._maxDistance.DeepCopyAsNew(
		// 					decisionHandlerPaceFromCurrent._maxDistance);
		// 				newCM._an_MoveAnimationName = decisionHandlerPaceFromCurrent._an_paceAnimationInfo;
		// 				newCM._moveSpeedMul = decisionHandlerPaceFromCurrent._speedMultiplier;
		// 				newCM._destNeedRelatedToHatredTarget = false;
		// 				newCM._angleRange = decisionHandlerPaceFromCurrent._angleRange_FromPlayer;
		// 				newCM._stop_RelatedToMaxDuration = true;
		// 				newCM._maxMovementDuration = decisionHandlerPaceFromCurrent._maxDuration;
		// 				newCM._minMovementDuration = decisionHandlerPaceFromCurrent._minDuration;
		// 				
		// 				perSO.DecisionHandler = newCM;
		// 				UnityEditor.EditorUtility.SetDirty(perSO);
		// 				UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 				break;
		// 			case DecisionHandler_Pace decisionHandlerPace:
		// 				newCM._an_MoveAnimationName = decisionHandlerPace._an_paceAnimationInfo;
		// 				newCM._moveSpeedMul = decisionHandlerPace._speedMultiplier;
		// 				newCM._angleRange = decisionHandlerPace._angleRange_FromPlayer;
		// 				newCM._stop_RelatedToMaxDuration = true;
		// 				perSO.DecisionHandler = newCM;
		// 				UnityEditor.EditorUtility.SetDirty(perSO);
		// 				UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 				break;
		// 			case StepBack_601001 stepBack:
		// 				newCM._moveSpeedMul = stepBack._stepBackSpeedMul;
		// 				newCM._an_MoveAnimationName = stepBack._an_StepBackAnimation;
		// 				newCM._stop_RelatedToMaxDuration = true;
		// 				newCM._maxMovementDuration = stepBack._stepBackMaxDuration;
		// 				newCM._minMovementDuration = stepBack._stepBackMinDuration;
		// 				newCM._destNeedRelatedToHatredTarget = true;
		// 				newCM._distance_min = new DD_自身到玩家距离_CurrentDistanceToPlayer
		// 				{
		// 					ValueMultiplier = 1f
		// 				};
		// 				newCM._distance_max = new DD_匹配距离监听_DistanceByPlayerDistanceListen
		// 				{
		// 					ValueMultiplier = 1,
		// 					ListenComponentIndex = 2
		// 				};
		// 				newCM._angleRange = stepBack._stepBackAngleRange;
		// 				newCM._moveDirection_FaceHatredTarget = false;
		// 				newCM._faceMoveSpeed = stepBack.FaceMovementDirection;
		// 				
		// 				perSO.DecisionHandler = newCM;
		// 				UnityEditor.EditorUtility.SetDirty(perSO);
		// 				UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 				break;
		// 		}
		// 	}
		// }

#endif



	}
}