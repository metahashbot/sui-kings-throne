using System;
using ARPG.Character.Player.Ally;
using ARPG.Manager;
using Global;
using Global.Utility;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_进行一段位移操作_BeginMovementOperation : BaseDecisionCommonComponent
	{

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 位移：_{DisplacementUID}_";
		}

		[SerializeField, LabelText("位移过程UID，用于其它DCC停止")]
		public string DisplacementUID;
		
		[SerializeField,LabelText("离开作用时停止位移")]
		public bool StopOnLeaveDecision = true;

		[NonSerialized]
		public bool DisplacementCurrentActive = false;


		public enum PositionSelectTypeEnum
		{
			PlayerPositionOnEnter_开始该作用时玩家位置 = 1,
			OnPresetSpawnRegister_刷怪生成点位置 = 2,
			OnDirectParsePosition_直接传入位置 = 3,
			AlwaysOnPlayerPosition_始终为玩家位置 = 4,
			SelfFaceDirection_自身朝向 = 5,
			OnDirectParseDirection_直接传入方向 = 6,
			PlayerPositionOnEnterDecision_开始决策时仇恨位置 =7,
		}

		[SerializeField, LabelText(" = 位移目标位置类型 = ")]
		public PositionSelectTypeEnum TargetPositionType = PositionSelectTypeEnum.PlayerPositionOnEnter_开始该作用时玩家位置;

		[Header("↓====↓")]
		[SerializeField, LabelText("√:达到位置时停止 | 口:按方向持续直到最大时长")]
		public bool StopWhenArrive = true;
		[SerializeField, ShowIf("@(this.TargetPositionType == PositionSelectTypeEnum.OnPresetSpawnRegister_刷怪生成点位置)")]
		public string TargetPositionUID;
		
		[SerializeField,LabelText("	   位置所属区域，不填就是当前区域"),ShowIf("@this.TargetPositionType == PositionSelectTypeEnum.OnPresetSpawnRegister_刷怪生成点位置")]
		public string TargetPositionAreaUID;
		
		

		[SerializeField, LabelText("    位移目标位置")]
		[ShowIf("@this.TargetPositionType == PositionSelectTypeEnum.OnDirectParsePosition_直接传入位置")]
		public Vector3 DirectParsePosition;

		//仅限 SelfFaceDirection_自身朝向 & OnDirectParseDirection_直接传入方向
		[SerializeField, LabelText("    位移距离")]
		[ShowIf(
			"@(this.TargetPositionType == PositionSelectTypeEnum.SelfFaceDirection_自身朝向 ||this.TargetPositionType == PositionSelectTypeEnum.OnDirectParseDirection_直接传入方向)")]
		public float DirectParseDistance = 1f;
		
		//仅限 OnDirectParseDirection_直接传入方向
		[SerializeField,LabelText("    传入方向")]
		[ShowIf("@this.TargetPositionType == PositionSelectTypeEnum.OnDirectParseDirection_直接传入方向")]
		public Vector3 DirectParseDirection;


		[SerializeField, LabelText("校准朝向"), ToggleButtons("开始位移时校准朝向", "不校准")]
		public bool FaceHatredTargetOnStart = false;

		[SerializeField, LabelText("校准为"), ToggleButtons("校准为面朝", "校准为背向")]
		[ShowIf("FaceHatredTargetOnStart")]
		public bool OffsetFaceTo = true;
		 

		[SerializeField, LabelText("位移无视空气墙吗？")]
		public bool IgnoreAirWall;

		[SerializeField, LabelText("√:匀速运动 | 口:按曲线完成")]
		public bool UseUniformSpeed = true;

		[SerializeField, LabelText("    速度使用 √:移速属性 | 口:固定数值")]
		[ShowIf("@this.UseUniformSpeed")]
		public bool UseMoveSpeedDataEntry = true;

		[ShowIf("@this.UseUniformSpeed && !this.UseMoveSpeedDataEntry")]
		[SerializeField, LabelText("    固定移速")]
		public float FixedMoveSpeed = 3f;
		
		[SerializeField, LabelText("    曲线")]
		[InfoBox("当“到达位置时停止”则为完成度曲线；当“按方向持续”时为速度曲线")]
		[HideIf("@this.UseUniformSpeed")]
		public AnimationCurve DisplacementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

		[SerializeField, LabelText("最大允许时长 | 曲线X轴长度 ")]
		public float MaxDuration = 5f;

		[SerializeField, LabelText("速度乘数_作为曲线Y轴缩放")]
		[HideIf("@this.StopWhenArrive")]
		public float CurveSpeedMultiplier = 1f;
		
		[SerializeField,LabelText("在X方向上限制角度")]
		public bool LimitAngleOnX = false;
		
		[SerializeField,LabelText("角度限制_顺逆各这么多")]
		[ShowIf("@this.LimitAngleOnX")]
		public float LimitAngleValueOnX = 30f;
		
		
		
		[LabelText("目的地生成特效")]
		public bool ContainVFXOnDest = false;

		[InfoBox("将会在位移目的地播放这个特效，所以这个特效应当是一次性的。它依然使用配置好的特效。只是在播放后会再次最终增加一个将其世界位置置于位移目的地的步骤")]
		[LabelText("    特效配置名"), ShowIf(nameof(ContainVFXOnDest)), GUIColor(187f / 255f, 1f, 0f)]
		public string _VFXConfigNameOnDest;




		[NonSerialized]
		public float _startTime;

		[NonSerialized]
		public Vector3 _targetPosition;

		/// <summary>
		/// <para>计算时的移动方向。如果是跟踪玩家位置，那这个值会时刻变动，否则就只是在Enter时变动一次</para>
		/// </summary>
		[NonSerialized]
		public Vector3 _runtimeMovementDirection;


		private Vector3 _startPosition;

		private Float_RPDataEntry _entry_moveSpeed;

		private SOConfig_AIBrain _relatedBrainRef;


		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			if (DisplacementCurrentActive)
			{
				DBug.LogWarning(
					$"来自角色：{relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}   的决策：" +
					$"{relatedBrain.BrainHandlerFunction.CurrentRunningDecision.name}中的位移已经开始，不应该再次开始位移");
				return;
			}
			
			
			_relatedBrainRef = relatedBrain;
			DisplacementCurrentActive = true;
			_startTime = BaseGameReferenceService.CurrentFixedTime;
			_entry_moveSpeed =
				relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetFloatDataEntryByType(
					RP_DataEntry_EnumType.MoveSpeed_移速);
			_startPosition = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position;
			switch (TargetPositionType)
			{
				case PositionSelectTypeEnum.PlayerPositionOnEnter_开始该作用时玩家位置:
					_targetPosition = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
						.CurrentControllingBehaviour.transform.position;
					break;
				case PositionSelectTypeEnum.OnPresetSpawnRegister_刷怪生成点位置:
					var activity = SubGameplayLogicManager_ARPG.Instance.ActivityManagerArpgInstance
						.EnemySpawnServiceSubActivityServiceComponentRef;
					string areaID = null;

					if (string.IsNullOrEmpty(TargetPositionAreaUID))
					{
						switch (relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour)
						{
							case EnemyARPGCharacterBehaviour enemy:
								areaID = enemy.RelatedSpawnConfigInstance.EnemySpawnConfigTypeHandler.RelatedAreaID;
								break;
						}
					}
					else
					{
						areaID = TargetPositionAreaUID;
					}
					var getList = activity.GetTargetSpawnPoint(areaID, TargetPositionUID);
					_targetPosition = getList[0].GetCurrentSpawnPosition();
					break;
				case PositionSelectTypeEnum.OnDirectParsePosition_直接传入位置:
					_targetPosition = DirectParsePosition;
					break;
				case PositionSelectTypeEnum.AlwaysOnPlayerPosition_始终为玩家位置:
					_targetPosition = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
						.CurrentControllingBehaviour.transform.position;
					break;
				case PositionSelectTypeEnum.SelfFaceDirection_自身朝向 :
					var faceLeft = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour
						.GetSelfRolePlayArtHelper().CurrentFaceLeft;
					if (faceLeft)
					{
						_targetPosition =
							relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position +
							BaseGameReferenceService.CurrentBattleLogicLeftDirection * DirectParseDistance;
					}
					else
					{
						_targetPosition =
							relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position +
							BaseGameReferenceService.CurrentBattleLogicRightDirection * DirectParseDistance;
					}
					break;
				case PositionSelectTypeEnum.OnDirectParseDirection_直接传入方向:
					_targetPosition =
						relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position +
						DirectParseDirection * DirectParseDistance;
					break;
				case PositionSelectTypeEnum.PlayerPositionOnEnterDecision_开始决策时仇恨位置:
					_targetPosition = relatedBrain.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler.HatredPositionOnEnterDecision ;
					_targetPosition.y = 0f;

                    break;
			}

			if (FaceHatredTargetOnStart)
			{
				relatedBrain.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler
					.QuickFaceHateTargetDirection(!OffsetFaceTo);
					
			}

			_runtimeMovementDirection =
				(_targetPosition - relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform
					.position).normalized;
		}


		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);

			if (!DisplacementCurrentActive)
			{
				return;
			}
			//如果使用速度位移，那达到目的地的条件是：距离目的地位置 的 距离 小于 速度*delta *2，既计算两帧的偏移
			if (ct > (_startTime + MaxDuration))
			{
				DisplacementCurrentActive = false;
				DBug.Log($" 来自{_relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour}的决策" +
				         $"{_relatedBrainRef.BrainHandlerFunction.CurrentRunningDecision.name}的位移超时，超时时长{MaxDuration}");
				return;
			}

			switch (TargetPositionType)
			{
				case PositionSelectTypeEnum.OnDirectParsePosition_直接传入位置:
					_targetPosition = DirectParsePosition;
                    _runtimeMovementDirection =
                        (_targetPosition - _relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform
                            .position).normalized;
                    break;
				case PositionSelectTypeEnum.AlwaysOnPlayerPosition_始终为玩家位置:
					_targetPosition = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
						.CurrentControllingBehaviour.transform.position;
                    _runtimeMovementDirection =
                        (_targetPosition - _relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform
                            .position).normalized;
                    break;

			}

			//不使用曲线
			if (UseUniformSpeed)
			{
				var moveSpeed = FixedMoveSpeed;	
				if (UseMoveSpeedDataEntry)
				{
					moveSpeed = _entry_moveSpeed.CurrentValue;
				}
				var moveDelta = moveSpeed * delta;
				_runtimeMovementDirection.y = 0f;
				_runtimeMovementDirection.Normalize();
				var moveDeltaVector = _runtimeMovementDirection * moveDelta;
				
				_LimitAngle( ref moveDeltaVector);
				
				_relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.TryMovePosition_OnlyXZ(
					moveDeltaVector,
					true,
					3f,
					!IgnoreAirWall);
				
				 //检查是否达到目的地
				 if (Vector3.Distance(_relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour
					 .transform.position, _targetPosition) < (moveDelta * 2f))
				 {
					 if (ContainVFXOnDest)
					 {
						 var vfx = _relatedBrainRef.BrainHandlerFunction._VFX_GetAndSetBeforePlay(_VFXConfigNameOnDest,
							 false);
						 vfx._VFX_2_SetPositionToGlobalPosition(_targetPosition);
						 vfx._VFX__10_PlayThis();
					 }
					 DisplacementCurrentActive = false;
				 }
			}
			//使用完成度曲线
			else
			{
				if (StopWhenArrive)
				{
					var x_ct = (ct - _startTime) / MaxDuration;
					var currentFinishPartial = DisplacementCurve.Evaluate(x_ct);

					var lerpPosition = Vector3.Lerp(_startPosition,
						_targetPosition,
						currentFinishPartial);
					var moveDeltaVector = lerpPosition - _relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour
						.transform.position;

					_LimitAngle(ref moveDeltaVector);
					_relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.TryMovePosition_OnlyXZ(
						moveDeltaVector,
						true,
						3f,
						!IgnoreAirWall);
				}
				else
				{
					//现在作为速度曲线
					var x_ct = (ct - _startTime) / MaxDuration;
					var currentFinishPartial = DisplacementCurve.Evaluate(x_ct);
					var speed = CurveSpeedMultiplier * _entry_moveSpeed.CurrentValue;
                    _runtimeMovementDirection.y = 0f;
                    _runtimeMovementDirection.Normalize();
                    var moveDeltaVector = _runtimeMovementDirection * (speed * delta * currentFinishPartial);

                    _LimitAngle(ref moveDeltaVector);
                    _relatedBrainRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.TryMovePosition_OnlyXZ(
                        moveDeltaVector,
                        true,
                        3f,
                        !IgnoreAirWall);
				}
			}
		}

		private void _LimitAngle(ref Vector3 delta)
		{
			//检查角度限制
			if (LimitAngleOnX)
			{
				var currentMag = delta.magnitude;
				var currentDirNor = delta.normalized;
				var currentMoveAngle = Vector3.SignedAngle(currentDirNor, Vector3.right, Vector3.up);
				float targetMoveAngle = 0f;
				//朝左边
				if (Mathf.Abs(currentMoveAngle) > 90f)
				{
					var diff = 180f - Mathf.Abs(currentMoveAngle);
					//允许的范围是 180 - limit
					//正的，在上半区。那么限制到 180-limit ~ 180
					if (currentMoveAngle > 0)
					{
						targetMoveAngle = Mathf.Clamp(currentMoveAngle, 180f - LimitAngleValueOnX, 180f);
					}
					else
					{
						targetMoveAngle = Mathf.Clamp(currentMoveAngle, -180f, -180f + LimitAngleValueOnX);
					}
				}
				else
				{
					//允许的范围是 0 ~ limit
					//正的，在右半区
					if (currentMoveAngle > 0)
					{
						targetMoveAngle = Mathf.Clamp(currentMoveAngle, 0f, LimitAngleValueOnX);
					}
					else
					{
						targetMoveAngle = Mathf.Clamp(currentMoveAngle, -LimitAngleValueOnX, 0f);
					}
				}
				var targetDir = MathExtend.Vector3RotateOnXOZ(Vector3.right, targetMoveAngle);
				delta = targetDir * currentMag;

			}
		}
		

		public override void OnDecisionExit(SOConfig_AIBrain relatedBrain)
		{
			base.OnDecisionExit(relatedBrain);
			 if(StopOnLeaveDecision)
			 {
				 DisplacementCurrentActive = false;
			 }
		}
	}
}