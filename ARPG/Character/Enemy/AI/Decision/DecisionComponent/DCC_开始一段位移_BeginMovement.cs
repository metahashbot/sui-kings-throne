using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Player.Ally;
using ARPG.Config.BattleLevelConfig;
using ARPG.Manager;
using Global;
using Global.AreaOnMap.AreaFunctionHandler;
using Global.Utility;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_开始一段位移_BeginMovement : BaseDecisionCommonComponent
	{
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 开始一段位移";
		}

		[LabelText("位移无视空气墙吗?")]
		public bool IgnoreAirWall = false;

		[LabelText("位移方向：  √:使用决策开始时方向  || 口:在采样曲线时重新校准")]
		public bool UseDirectionInfoFromDecision = true;

		[LabelText("    每帧校准朝向?")]
		[HideIf(nameof(UseDirectionInfoFromDecision))]
		public bool UpdateDirectionInMovement = false;


		[LabelText("√:目的地使用一个固定距离数值 | 口:目的地使用一个配置位置")]
		public bool UseFixedTargetPosition = true;



		[LabelText("需要选定的位置类型")] [HideIf(nameof(UseFixedTargetPosition))]
		public EnemySpawnPoint_PresetTypeEnum TargetPositionType;

		[LabelText("需要选定的位置的UID")] [HideIf(nameof(UseFixedTargetPosition))]
		public string TargetPositionUID;

		[NonSerialized]
		public Vector3 movementTargetPosition;

		[LabelText("    位移距离，作为曲线的Y范围")]
		[ShowIf(nameof(UseFixedTargetPosition))]
		public float DisplacementDistance = 0.15f;

		[LabelText("位移过程时长，作为曲线的X范围")]
		public float DisplacementTimeMax = 0.2f;

		[InfoBox("相对值曲线，XY轴的范围都是[0,1]之间")]
		[LabelText("位移完成度曲线")]
		public AnimationCurve DisplacementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));


		[LabelText("目的地来点特效？")]
		public bool ContainVFX = false;
		[InfoBox("将会在位移目的地播放这个特效，所以这个特效应当是一次性的。它依然使用配置好的特效。只是在播放后会再次最终增加一个将其世界位置置于位移目的地的步骤")]
		[LabelText("    特效配置名"), ShowIf(nameof(ContainVFX)), GUIColor(187f / 255f, 1f, 0f)]
		public string _VFXConfigName;





		[NonSerialized, LabelText("位移起始时间"), ReadOnly, ShowInInspector]
		public float _movementStartTime;

		[NonSerialized, LabelText("位移开始了吗"), ReadOnly, ShowInInspector]
		public bool _movementHasStart = false;

		[NonSerialized, LabelText("位移开始时所记录的方向"), ReadOnly, ShowInInspector]
		public Vector3 _movementDirectionOnStart;

		[NonSerialized, LabelText("位移开始时的位置"), ReadOnly, ShowInInspector]
		public Vector3 _movementPositionOnStart;


		/// <summary>
		/// 由计算得出的位移终点，用于播放特效
		/// </summary>
		protected Vector3 _vfxPosCalculated;


		protected PerVFXInfo _relatedVFXInfo;


		protected bool _recordFirstEvaluate = false;
		protected float _lastEvaluateTime = 0f;

		protected SOConfig_AIBrain _relatedBrainRef;


		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			_recordFirstEvaluate = false;
			_movementHasStart = true;
			_relatedBrainRef = relatedBrain;

			_movementStartTime = BaseGameReferenceService.CurrentFixedTime;
			var dh = relatedBrain.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler;

			if (UseDirectionInfoFromDecision)
			{
				if (dh is DH_通用常规攻击_CommonAttackWithAnimation decisionAttack)
				{
					_movementDirectionOnStart = decisionAttack.ToHatredTargetDirectionOnEnterDecision;
				}
				else
				{
					DBug.LogWarning($" 位移动画事件响应：使用决策开始时方向，但是决策不是通用常规攻击，" +
					                $"所以使用了决策的自身朝向。来源角色{relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}，" +
					                $"决策{dh.SelfRelatedDecisionRuntimeInstance.name}");
				}
			}
			else
			{
				_movementDirectionOnStart = dh.GetDirectionToHatredTarget();
			}
			if (!UseFixedTargetPosition)
			{
				List<EnemySpawnPositionRuntimeInfo> pos = null;
				switch (_relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour)
				{
					case EnemyARPGCharacterBehaviour enemyArpgCharacterBehaviour:
						pos = GameReferenceService_ARPG.Instance.SubGameplayLogicManagerRef.ActivityManagerArpgInstance
							.EnemySpawnServiceSubActivityServiceComponentRef.GetTargetSpawnPoint(
								enemyArpgCharacterBehaviour.RelatedAreaID,
								TargetPositionType,
								TargetPositionUID);
						break;
					case AllyARPGCharacterBehaviour allyArpgCharacterBehaviour:
						GameReferenceService_ARPG.Instance.SubGameplayLogicManagerRef.ActivityManagerArpgInstance
							.EnemySpawnServiceSubActivityServiceComponentRef.GetTargetSpawnPoint(null,
								TargetPositionType,
								TargetPositionUID);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				if (pos.Count == 0)
				{
					DBug.LogError(
						$"敌人AI动画响应：位移到指定位置，但类型:{TargetPositionType}于ID:{TargetPositionUID}。来源角色{relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}，没有获取到任何有效的具体位置");
				}

				pos.Shuffle();
				movementTargetPosition = pos[0].GetCurrentSpawnPosition();
			}

			_movementPositionOnStart =
				relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position;

			if (ContainVFX)
			{
				_relatedVFXInfo = dh.SelfRelatedBrainInstanceRef.BrainHandlerFunction
					._VFX_GetAndSetBeforePlay(_VFXConfigName,
						true,
						relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetRelatedArtHelper())
					?._VFX__10_PlayThis();
				if (UseFixedTargetPosition)
				{
					_relatedVFXInfo._VFX_2_SetPositionToGlobalPosition(_movementPositionOnStart +
					                                                   _movementDirectionOnStart *
					                                                   DisplacementDistance);
				}
				else
				{
					_relatedVFXInfo._VFX_2_SetPositionToGlobalPosition(movementTargetPosition + Vector3.up * 0.05f);
				}
			}
		}

		
		

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			if (!_movementHasStart)
			{
				return;
			}
			switch (_relatedBrainRef.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler)
			{
				case DH_通用常规攻击_CommonAttackWithAnimation dh通用常规攻击CommonAttackWithAnimation:
				case DH_通用常规发呆_CommonIdle dh通用常规发呆CommonIdle:


					if (!_recordFirstEvaluate)
					{
						_recordFirstEvaluate = true;
						return;
					}


					float x_ct = (ct - _movementStartTime) / DisplacementTimeMax;


					if (x_ct > 1f)
					{
						_movementHasStart = false;
						return;
					}

					float lastEvaX = ct - delta;
					float lastEvaResult =
						DisplacementCurve.Evaluate((lastEvaX - _movementStartTime) / DisplacementTimeMax) *
						DisplacementDistance;
					float currentEvaResult =
						DisplacementCurve.Evaluate((ct - _movementStartTime) / DisplacementTimeMax) *
						DisplacementDistance;

					Vector3 movementDirection = _movementDirectionOnStart;

					if (!UseDirectionInfoFromDecision && UpdateDirectionInMovement)
					{
						movementDirection = _relatedBrainRef.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler
							.GetDirectionToHatredTarget();
					}
					Vector3 movementDelta;

					if (UseFixedTargetPosition)
					{
						movementDelta = (currentEvaResult - lastEvaResult) * movementDirection;
					}
					else
					{
						//move by curve


						var dest = Vector3.Lerp(_movementPositionOnStart,
							movementTargetPosition,
							DisplacementCurve.Evaluate((ct - _movementStartTime) / DisplacementTimeMax));
						movementDelta = dest - _relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour
							.transform.position;
					}



					// float currentEva = DisplacementCurve.Evaluate(x_ct);
					// float movementInCurve = currentEva * _Config_DisplacementDistance;
					//
					// Vector3 targetMovement = movementDirection.normalized * (movementInCurve * _Config_DisplacementDistance);
					// Vector3 targetPosition = _movementPositionOnStart + targetMovement;
					// Vector3 movementDelta = targetPosition - behaviour.transform.position;
					// // if (ContainVFX)
					// // {
					// // 	_relatedVFXInfo._VFX_2_SetPositionToGlobalPosition(targetPosition);
					// // }

					_relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.TryMovePosition_XYZ(
						movementDelta,
						true,
						3f,
						IgnoreAirWall);
					break;

				default:
					DBug.LogError(
						$"敌人AI动画响应：生成版面在执行时，没有可用的具体Handler类型，检查一下。来源角色{_relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}，" +
						$"决策{_relatedBrainRef.BrainHandlerFunction.CurrentRunningDecision.name}");
					_movementHasStart = false;
					return;
			}
		}
	}
}