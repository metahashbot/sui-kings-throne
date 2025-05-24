using System;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using ARPG.Config.BattleLevelConfig;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.AreaFunctionHandler;
using Global.Utility;
using RPGCore.Buff;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[Serializable]
	public class DH_位移到指定位置_MovementSpecificPosition : BaseDecisionHandler
	{


		[ValidateInput("@this._an_MoveAnimationName != null", "！没有配置动画名！")]
		[LabelText("使用的动画配置名"), SerializeField, FoldoutGroup("配置", true), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		protected string _an_MoveAnimationName;


		[SerializeField, LabelText("该决策内包含无敌？"), FoldoutGroup("配置", true)]
		protected bool _containInvincible = true;




		[Header(" === 速度 ===")]
		[SerializeField, LabelText("√:使用曲线移动？ | 口:自然移动"), FoldoutGroup("配置", true)]
		protected bool _isCurveMovement = false;




		[HideIf(nameof(_isCurveMovement))]
		[SerializeField, LabelText("直接覆盖原始速度？"), FoldoutGroup("配置", true)]
		protected bool _isOverrideMoveSpeed;

		[HideIf(nameof(_isCurveMovement))]
		[SerializeField, LabelText("        覆盖的移动速度"), FoldoutGroup("配置", true), ShowIf(nameof(_isOverrideMoveSpeed))]
		protected float _overrideMoveSpeed;

		[HideIf(nameof(_isCurveMovement))]
		[SerializeField, LabelText("        移动速度倍率乘数"), FoldoutGroup("配置", true), HideIf(nameof(_isOverrideMoveSpeed))]
		protected float _moveSpeedMul = 1f;


		[ShowInInspector, NonSerialized, LabelText("当前目标点"), FoldoutGroup("运行时", true), ReadOnly]
		public Vector3 _currentTargetPoint;



		[SerializeField, LabelText("移动时无视空气墙？"), FoldoutGroup("配置", true)]
		protected bool _ignoreAirWall = true;

		protected BaseARPGArtHelper _selfArtHelperRef;
		protected Float_RPDataEntry _entry_MoveSpeed;


		public Nullable<Vector3> _movementTargetPosition;




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
			_movementTargetPosition = null;
			//从生成的singleInfo那拿数据
			EnemySpawnPositionRuntimeInfo spawnPointInfo =
				(_selfRelatedBehaviourRef as EnemyARPGCharacterBehaviour).RelatedSpawnConfigInstance_SpawnPositionInfo;
			
			
			if (_containInvincible)
			{
				_selfRelatedBehaviourRef.ReceiveBuff_TryApplyBuff(
					RolePlay_BuffTypeEnum.InvincibleByDirector_All_WD_来自机制的完全无敌,
					_selfRelatedBehaviourRef,
					_selfRelatedBehaviourRef);
			}


			_Internal_BlockBrainAutoDeduce();
			_Internal_RequireAnimation(GetAnimationInfoFromBrain(_an_MoveAnimationName));





			return ds_start;
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




		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			if (_movementTargetPosition == null)
			{
				return;
			}
			if (!_isCurveMovement)
			{

				
				
				
				Vector3 globalRight = BaseGameReferenceService.CurrentBattleLogicRightDirection;

				float moveSpeed = GetMovementSpeed();


				var movementTP = _movementTargetPosition.Value;
				movementTP.y = 0f;
				var selfPos = _selfRelatedBehaviourRef.transform.position;
				selfPos.y = 0f;

				Vector3 movementDir = (movementTP - selfPos).normalized;
				Vector3 movementDelta = movementDir * (delta * moveSpeed);
				_selfRelatedBehaviourRef.TryMovePosition_OnlyXZ(movementDelta, true, 10f, !_ignoreAirWall);
				if (Vector3.SqrMagnitude(selfPos - movementTP) < 0.01f)
				{
					OnDecisionNormalComplete();
					return;
				}


			}

		}

		public override DS_ActionBusArguGroup OnDecisionNormalComplete(bool autoLaunch = true, bool normalStop = true)
		{
			if (_containInvincible)
			{
				_selfRelatedBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum
					.InvincibleByDirector_All_WD_来自机制的完全无敌);
			}

			return base.OnDecisionNormalComplete(autoLaunch, normalStop);
		}


		
		
	}
}