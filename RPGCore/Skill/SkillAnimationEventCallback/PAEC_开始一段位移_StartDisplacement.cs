using System;
using ARPG.Character.Base;
using ARPG.Equipment;
using ARPG.Manager;
using Global;
using RPGCore;
using RPGCore.DataEntry;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Character.Player
{
	[Serializable]
	public class PAEC_开始一段位移_StartDisplacement : BasePlayerAnimationEventCallback
	{
		
		/// <summary>
		/// <para> 位移活跃吗</para>
		/// </summary>
		[NonSerialized]
		public bool DisplacementActive;
		
		
		[LabelText("位移方式")]
		public WeaponAttackDirectionTypeEnum DisplacementType =
			WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向;


		/// <summary>
		/// 记录的位移方向。通常
		/// </summary>
		[NonSerialized]
		public Vector3 RegisteredAttackDirection;

		
		/// <summary>
		/// 起始时间点
		/// </summary>
		[NonSerialized]
		public float DisplacementStartTime;
		[NonSerialized]
		public Vector3 DisplacementStartPosition;
		
		[SerializeField,LabelText("位移参与角色碰撞阻挡吗")]
		public bool DisplacementBlockByCharacter = true;
		
		
		[FormerlySerializedAs("DisplacementDistance"),LabelText("位移距离，作为曲线的Y范围"),SerializeField]
		protected float _Config_DisplacementDistance = 0.15f;

		public float GetConfigDisplacementDistance => _Config_DisplacementDistance;
		[InfoBox("影响乘数：若基准值是5，当前移速7，则最终距离 会加上 原始位移距离 * (7/5 - 1) * 这个值。如果这个值小于0，就相当于不受影响了")]
		[LabelText("位移距离受移速的影响乘数")]
		public float DisplacementDistanceSpeedFactor = 0.2f;

		[LabelText("位移距离的移速基准值")]
		public float DisplacementDistanceSpeedBase = 5f;
		
		 
		

		

		[NonSerialized]
		public float RuntimeDisplacementDistance;

		[FormerlySerializedAs("DisplacementTimeMax"),LabelText("位移过程时长，作为曲线的X范围"),SerializeField]
		protected float _Config_DisplacementTimeMax = 0.2f;
		
		public float GetConfigDisplacementTimeMax => _Config_DisplacementTimeMax;
		
		[NonSerialized]
		public float RuntimeDisplacementTimeMax;
		
		

		[InfoBox("【相对值】曲线，XY轴的范围都是[0,1]之间")]
		[LabelText("位移完成度曲线")]
		public AnimationCurve DisplacementCurve = new AnimationCurve( new Keyframe(0, 0), new Keyframe(1, 1));


		[InfoBox("【绝对值】曲线！曲线Y的值就直接是乘数")]
		[SerializeField,LabelText(" 位移时，输入方向对位移距离的影响曲线。")]
		 public AnimationCurve DisplacementDistanceFactorCurve = new AnimationCurve(new  Keyframe(-1,0.5f), new Keyframe(0,1),new Keyframe(1,1.5f));
		 
		
		

		public void StartDisplacement(Vector3 fromPosition, PlayerARPGConcreteCharacterBehaviour player , Vector3 direction)
		{
			DisplacementActive = true;
			DisplacementStartTime = BaseGameReferenceService.CurrentFixedTime;
			DisplacementStartPosition = fromPosition;

			if (DisplacementDistanceSpeedFactor < 0f)
			{
				RuntimeDisplacementDistance = GetConfigDisplacementDistance;
			}
			else
			{
				RuntimeDisplacementDistance = GetConfigDisplacementDistance + GetConfigDisplacementDistance *
					(player.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速).CurrentValue /
						DisplacementDistanceSpeedBase - 1) * DisplacementDistanceSpeedFactor;
			}

			RuntimeDisplacementTimeMax = GetConfigDisplacementTimeMax;
			RegisteredAttackDirection = direction;
		}

		public void FixedUpdateTick_ProcessDisplacement(
			float ct,
			float delta,
			BaseARPGCharacterBehaviour behaviour)
		{
	
			if (DisplacementActive)
			{
			    Vector3 targetDirection = RegisteredAttackDirection;
				switch (DisplacementType)
				{
					case WeaponAttackDirectionTypeEnum.RegisteredCharacterMovementDirection_记录的角色移动方向:
						targetDirection = behaviour.GetRelatedArtHelper().CurrentFaceLeft ? 
							Vector3.left : Vector3.right;
						break;
					case WeaponAttackDirectionTypeEnum.PointerDirectionRegistered_记录的输入方向:
                    case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainRegistered_记录的指针位置:
						targetDirection = RegisteredAttackDirection;
						break;
					case WeaponAttackDirectionTypeEnum.PointerDirectionInstant_瞬时的输入方向:
						targetDirection = _pcbRef.InputResultAimDirectionRawV3;
						break;
					case WeaponAttackDirectionTypeEnum.ControlledByHandler_由具体的Handler实现:
						break;
					case WeaponAttackDirectionTypeEnum.PointerPositionOnTerrainInstant_瞬时的指针位置:
						targetDirection = _pcbRef.InputResult_CurrentPlayerToPointerOnFloorDirection ??
						                  RegisteredAttackDirection;
						break;
					case WeaponAttackDirectionTypeEnum.CharacterPosition_角色位置:
						targetDirection = behaviour.transform.position;
						break;
					case WeaponAttackDirectionTypeEnum.RegisteredCharacterMoveDirectionThenPointer_记录的角色移动方向后指针:

						targetDirection = RegisteredAttackDirection;
						break;
				}
				//根据当前的ct，减去delta得到位移的具体长度
				float lastEvaTime = ct - delta;
				float lastEva = DisplacementCurve.Evaluate((lastEvaTime - DisplacementStartTime) / RuntimeDisplacementTimeMax);
				float x_currentTimeInCurve = (ct - DisplacementStartTime) / RuntimeDisplacementTimeMax;
				float curveEva = DisplacementCurve.Evaluate(x_currentTimeInCurve);
				float currentMovementInCurve = curveEva * RuntimeDisplacementDistance;
				float lastMovementInCurve = lastEva * RuntimeDisplacementDistance;
				var deltaMovement = targetDirection.normalized * (currentMovementInCurve - lastMovementInCurve);
				
				//进行一下输入的方向对距离的影响

				var behaviourFaceLeft = behaviour.GetRelatedArtHelper().CurrentFaceLeft;
				var currentInputOnX = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
					.InputResult_inputMoveOnCurrentGameplayDirectionNormalized.x;
				//如果当前朝左，则输入方向取反
				if (behaviourFaceLeft)
				{
					currentInputOnX *= -1f;
				}
				
				var evaFromInputCurve = DisplacementDistanceFactorCurve.Evaluate(currentInputOnX);
				deltaMovement *= evaFromInputCurve;

				behaviour.TryMovePosition_XYZ(deltaMovement, true, 10f, true, DisplacementBlockByCharacter);
				if (x_currentTimeInCurve > 1f)
				{
					DisplacementActive = false;
				}
			}
		}


		public override BasePlayerAnimationEventCallback ExecuteBySkill(
			BaseARPGCharacterBehaviour behaviour,
			BaseRPSkill skillRef,
			bool createNewWhenExist = false)
		{
			return this;
		}
		public override BasePlayerAnimationEventCallback ExecuteByWeapon(
			BaseARPGCharacterBehaviour behaviour,
			BaseWeaponHandler weaponHandler,
			bool createNewWhenExist = false)
		{
			return this;
		}
		public override string GetElementNameInList()
		{ 
			return $"{GetBaseCustomName()} 开始一段位移";
		}
	}
}