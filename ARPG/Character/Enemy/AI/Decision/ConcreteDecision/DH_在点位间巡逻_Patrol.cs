using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Config.BattleLevelConfig;
using ARPG.Manager.Component;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[Serializable]
	public class DH_在点位间巡逻_Patrol : BaseDecisionHandler
	{

		[ValidateInput("@this._an_MoveAnimationName != null", "！没有配置动画名！")]
		[LabelText("移动时动画名"), SerializeField, FoldoutGroup("配置", true), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		private string _an_MoveAnimationName;

		private AnimationInfoBase _ani_MoveAnimationInfo;
		[LabelText("等待时动画名"), SerializeField, FoldoutGroup("配置", true), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		private string _an_StayAnimationName;

		private AnimationInfoBase _ani_StayAnimationInfo;





		[Serializable]
		public class PatrolInfoPair
		{
			[LabelText("位置ID-生成点-不限点位类型"), GUIColor(133f / 255f, 209f / 255f, 173f / 255f)]
			public string PositionID;

			[SerializeField, LabelText("直接覆盖原始速度？")]
			public bool _isOverrideMoveSpeed;

			[SerializeField, LabelText("        覆盖的移动速度"),
			 ShowIf(nameof(_isOverrideMoveSpeed))]
			public float _overrideMoveSpeed;

			[SerializeField, LabelText("        移动速度倍率乘数"),
			 HideIf(nameof(_isOverrideMoveSpeed))]
			public float _moveSpeedMul = 1f;


			[LabelText("等待时长")]
			public float StayDuration = 5f;


			[NonSerialized]
			public float RemainingDuration;
			[NonSerialized]
			public Vector3 CachePosition;


		}
		[InfoBox("巡逻的点位是自动匹配生成它所使用的区域的，必须需要从ESA传入指定的配置才可以开始巡逻")]
		[NonSerialized, LabelText("巡逻信息组们"),ShowInInspector,ReadOnly]
		public List<PatrolInfoPair> PatrolInfoPairs = new List<PatrolInfoPair>();

		//当前正在活跃的巡逻信息索引，超过了会倒回来，把sequence翻转一下
		private int _currentActivePatrolInfoIndex;

		//正向的？
		private bool _forwardSequence;

		private static EnemySpawnService_SubActivityService _essRef;

		protected BaseARPGArtHelper _selfArtHelperRef;

		protected Vector3 _movementTargetPosition;
		private enum PatrolStateTypeEnum
		{
			None_未开始 = 0, Staying_等待中 = 1, Walking_移动中 = 2,
		}

		private PatrolStateTypeEnum _currentPatrolState;
		public override void Initialize(SOConfig_AIBrain config)
		{
			base.Initialize(config);
			_selfArtHelperRef = _selfRelatedBehaviourRef.GetRelatedArtHelper();
			_essRef = _glmArpgRef.ActivityManagerArpgInstance.EnemySpawnServiceSubActivityServiceComponentRef;

			_entry_MoveSpeed = _selfRelatedBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速);
			_ani_MoveAnimationInfo = GetAnimationInfoFromBrain(_an_MoveAnimationName);
			;
			_ani_StayAnimationInfo = GetAnimationInfoFromBrain(_an_StayAnimationName);
		}


		private Float_RPDataEntry _entry_MoveSpeed;

		private void _SetPatrolInfo()
		{
			if (PatrolInfoPairs == null || PatrolInfoPairs.Count == 0)
			{
				DBug.LogError($"角色{_selfRelatedBehaviourRef.name}的决策{SelfRelatedDecisionRuntimeInstance.name}的 ！巡逻信息组数量为0，这是不允许的！");
				return;
			}
			foreach (PatrolInfoPair perPair in PatrolInfoPairs)
			{
				if (string.IsNullOrEmpty(perPair.PositionID))
				{
					DBug.LogError($"巡逻信息组中有一个没有配置位置ID的信息组，这是不允许的，位置ID：{perPair.PositionID}");
					continue;
				}

				string areaID = null;
				switch (SelfRelatedBrainInstanceRef.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour)
				{
					case EnemyARPGCharacterBehaviour enemy:
						areaID = enemy.RelatedSpawnConfigInstance.EnemySpawnConfigTypeHandler.RelatedAreaID;
						break;
				}
				var findList = _essRef.GetTargetSpawnPoint(areaID ,perPair.PositionID);
				if (findList == null || findList.Count == 0)
				{
					continue;
				}
				var find = findList[0];
				perPair.CachePosition = find.GetCurrentSpawnPosition();
				perPair.CachePosition.y = 0f;
				perPair.RemainingDuration = perPair.StayDuration;
			}
			if (PatrolInfoPairs.Count < 2)
			{
				DBug.LogError(
					$"角色{_selfRelatedBehaviourRef.name}的决策{SelfRelatedDecisionRuntimeInstance.name}的 ！巡逻信息组数量不足2个，这是不允许的！");
				return;
			}
		}

		public void ReceivePatrolInfo(List<PatrolInfoPair> outInfo)
		{
			PatrolInfoPairs.Clear();
			PatrolInfoPairs.AddRange(outInfo);
			
			_SetPatrolInfo();
			ProcessToNextPatrol();
		}
		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);


			if (PatrolInfoPairs.Count == 0)
			{
				return;
			}

			var currentInfo = PatrolInfoPairs[_currentActivePatrolInfoIndex];

			var selfPos = _selfRelatedBehaviourRef.transform.position;
			selfPos.y = 0f;
			var speed = currentInfo._isOverrideMoveSpeed ? currentInfo._overrideMoveSpeed
				: _entry_MoveSpeed.GetCurrentValue() * currentInfo._moveSpeedMul;

			switch (_currentPatrolState)
			{
				case PatrolStateTypeEnum.None_未开始:
					ProcessToNextPatrol();
					break;
				case PatrolStateTypeEnum.Staying_等待中:
					currentInfo.RemainingDuration -= delta;
					if (currentInfo.RemainingDuration <= 0f)
					{
						currentInfo.RemainingDuration = currentInfo.StayDuration;
						ProcessToNextPatrol();
					}
					break;
				case PatrolStateTypeEnum.Walking_移动中:

					Vector3 movementDir = (currentInfo.CachePosition - selfPos).normalized;
					Vector3 movementDelta = movementDir * (delta * speed);
					_selfRelatedBehaviourRef.TryMovePosition_OnlyXZ(movementDelta, true, 3f);

					_selfArtHelperRef.SetFaceLeft(movementDelta.x < 0f);



					if (Vector3.Distance(selfPos, currentInfo.CachePosition) < (speed * delta * 4f))
					{
						ProcessToNextPatrol();
					}
					break;
			}
		}










		private void ProcessToNextPatrol()
		{
			switch (_currentPatrolState)
			{
				case PatrolStateTypeEnum.None_未开始:
					_currentActivePatrolInfoIndex = 0;
					_currentPatrolState = PatrolStateTypeEnum.Walking_移动中;
					_Internal_RequireAnimation(_ani_MoveAnimationInfo);
					break;
				case PatrolStateTypeEnum.Staying_等待中:
					if (_forwardSequence)
					{
						_currentActivePatrolInfoIndex += 1;
						if (_currentActivePatrolInfoIndex >= PatrolInfoPairs.Count)
						{
							_currentActivePatrolInfoIndex = PatrolInfoPairs.Count - 2;
							_forwardSequence = false;
						}
					}
					else
					{
						_currentActivePatrolInfoIndex -= 1;
						if (_currentActivePatrolInfoIndex < 0)
						{
							_currentActivePatrolInfoIndex = 1;
							_forwardSequence = true;
						}
					}
					_currentPatrolState = PatrolStateTypeEnum.Walking_移动中;
					_Internal_RequireAnimation(_ani_MoveAnimationInfo);


					break;
				case PatrolStateTypeEnum.Walking_移动中:
					
					_currentPatrolState = PatrolStateTypeEnum.Staying_等待中;
					_Internal_RequireAnimation(_ani_StayAnimationInfo);

					break;
			}
		}







	}
}