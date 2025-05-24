using System;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using Global;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_由预设移动控制的移动_MovementByPresetMovement : DCC_开始一段位移_BeginMovement
	{


		private Vector3 movementTargetPosition;

		private BaseDecisionHandler _decisionHandlerRef;
		private BaseARPGCharacterBehaviour _behaviourRef;
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			_recordFirstEvaluate = false;
			_movementHasStart = true;

			_movementStartTime = BaseGameReferenceService.CurrentFixedTime;

			_decisionHandlerRef = relatedBrain.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler;
			_behaviourRef = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			DH_预设移动_PresetMovement dhPresetMovement = _decisionHandlerRef as DH_预设移动_PresetMovement;
			;


			_movementDirectionOnStart =
				(dhPresetMovement._movementTargetPosition - _behaviourRef.transform.position).normalized;
			movementTargetPosition = dhPresetMovement._movementTargetPosition;

			_movementPositionOnStart = _behaviourRef.transform.position;

			if (ContainVFX)
			{
				_relatedVFXInfo = _decisionHandlerRef.SelfRelatedBrainInstanceRef.BrainHandlerFunction
					._VFX_GetAndSetBeforePlay(_VFXConfigName, false)?._VFX__10_PlayThis();
				_relatedVFXInfo._VFX_2_SetPositionToGlobalPosition(movementTargetPosition);
			}
		}

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			if (!_movementHasStart)
			{
				return;
			}
			switch (_decisionHandlerRef)
			{
				case DH_预设移动_PresetMovement dhPresetMovement:

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
					var lastEvaPos = Vector3.Lerp(_movementPositionOnStart,
						movementTargetPosition,
						DisplacementCurve.Evaluate((lastEvaX - _movementStartTime) / DisplacementTimeMax));

					var currentEvaPos = Vector3.Lerp(_movementPositionOnStart,
						movementTargetPosition,
						DisplacementCurve.Evaluate((ct - _movementStartTime) / DisplacementTimeMax));
					var movementDelta = currentEvaPos - lastEvaPos;

					// float currentEva = DisplacementCurve.Evaluate(x_ct);
					// float movementInCurve = currentEva * _Config_DisplacementDistance;
					//
					// Vector3 targetMovement = movementDirection.normalized * (movementInCurve * _Config_DisplacementDistance);
					// Vector3 targetPosition = _movementPositionOnStart + targetMovement;
					// Vector3 movementDelta = targetPosition - _behaviourRef.transform.position;
					// // if (ContainVFX)
					// // {
					// // 	_relatedVFXInfo._VFX_2_SetPositionToGlobalPosition(targetPosition);
					// // }

					_behaviourRef.TryMovePosition_XYZ(movementDelta, true, 20f, false);
					break;
				default:
					DBug.LogError($"敌人AI动画响应：生成版面在执行时，没有可用的具体Handler类型，检查一下。来源角色{_behaviourRef.name}，" +
					              $"决策{_decisionHandlerRef.SelfRelatedDecisionRuntimeInstance.name}");
					_movementHasStart = false;
					return;
			}
		}
	}
}