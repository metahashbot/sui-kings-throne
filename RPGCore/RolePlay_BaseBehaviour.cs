using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Equipment;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Character.Config;
using Global.RuntimeRecord;
using Global.Utility;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore
{
	public abstract class RolePlay_BaseBehaviour : MonoBehaviour, I_RP_Buff_ObjectCanApplyBuff,
		I_RP_Buff_ObjectCanReceiveBuff, I_RP_DataEntry_ObjectCanApplyDataEntryEffect,
		I_RP_DataEntry_ObjectCanReceiveDataEntryEffect, I_RP_Database_ObjectContainDataEntryDatabase,
		I_RP_Projectile_ObjectCanReceiveProjectileCollision
	{
#region 内部字段

		protected RaycastHit[] _selfRaycastHitResults = new RaycastHit[2];
		[SerializeField, LabelText("角色类型指定"), FoldoutGroup("配置", true)]
		protected CharacterNamedTypeEnum _selfBehaviourNamedType;
		public CharacterNamedTypeEnum SelfBehaviourNamedType
		{
			get => _selfBehaviourNamedType;
			protected set => _selfBehaviourNamedType = value;
		}

		[ShowInInspector, FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()"),
		 LabelText("本地事件线")]
		protected LocalActionBus _selfActionBusInstance;



		[ShowInInspector, LabelText("于运行时Record中的角色信息记录-引用自GCAHH的Record")]
		[FoldoutGroup("运行时")]
		public ConcreteRuntimeRecord_Character SelfCharacterRuntimeRecordRef { get; protected set; }


		protected RaycastHit[] rbMoveCheckResults = new RaycastHit[2];
		protected Collider[] rbCheckResults = new Collider[2];



		/// <summary>
		/// <para>单个角色的尺寸变体乘数。会应用到ScaleAnchor上。这个是每个个体区分的，它与当前环境下的</para>
		/// </summary>
		public float SelfCharacterVariantScale { get; protected set; } = 1f;

		public virtual void SetCharacterVariantScale(float scale)
		{
			SelfCharacterVariantScale = scale;
		}

#endregion
#region 基本时序 加载 初始化

		public virtual void InitializeOnInstantiate()
		{
			SelfCharacterRuntimeRecordRef = GlobalConfigurationAssetHolderHelper.Instance
				.RuntimeRecordHelperCharacter.GetCharacterRecord((int)SelfBehaviourNamedType);
			// SelfCharacterInfoConfigRef = GlobalConfigurationAssetHolderHelper.Instance.Collection_CharacterRawInfo
			// 	.GetCharacterRawInfoByType(behaviourType);

			_selfBaseArtHelperRef = GetComponentInChildren<RolePlay_ArtHelperBase>(true);
			_selfActionBusInstance = GenericPool<LocalActionBus>.Get();
			_selfActionBusInstance.Init();
			BindingEvents();

#if UNITY_EDITOR
			gameObject.AddComponent<RPInspector>();
#endif
			// if (this is ConcretePlayerCharacterBehaviour_BattleGame)
			// {
			// 	SelfCollisionInfo.CollisionLayerMask = 0b10;
			// }
			// else if (this is BaseEnemyBehaviour_BattleGame)
			// {
			// 	SelfCollisionInfo.CollisionLayerMask = 0b01;
			// }
		}

		/// <summary>
		/// <para>从GCAHH中加载逻辑配置和艺术资产。加载完成后会触发全局广播</para>
		/// </summary>
		protected virtual void LoadLogicConfigAndArtAsset()
		{
			// CharacterAssetEntry artAssetEntry =
			// 	GlobalConfigurationAssetHolderHelper.Instance.RAH_CharacterAssetEntry.GetTargetCharacterAssetEntry(SelfBehaviourNamedType);
			// var prefabRef = artAssetEntry.SelfGOWithArtHelperPrefab;
			// var newArtPartGO = Instantiate(prefabRef, transform);
			// RolePlay_ArtHelperBase artHelperBaseRef = newArtPartGO.GetComponent<RolePlay_ArtHelperBase>();
			// artHelperBaseRef.InitializeOnInstantiate(_selfActionBusInstance);

			// InjectArtHelperOnInstantiate(artHelperBaseRef);
		}

		protected virtual void BindingEvents() { }

#endregion

		protected RP_DS_AnimationPlayResult _selfAnimationPlayerResult = new RP_DS_AnimationPlayResult();

		protected abstract RolePlay_DataModelBase GetSelfRolePlayDataModel();

		protected RolePlay_ArtHelperBase _selfBaseArtHelperRef;
		public abstract RolePlay_ArtHelperBase GetSelfRolePlayArtHelper();



		protected abstract void _ABC_OnFinallyDead_OnHpReducedTo0(DS_ActionBusArguGroup rpds);



#region Tick

		/// <summary>
		/// <para>Update时序的Tick，经由自身关联的Behaviour调用</para>
		/// </summary>
		public virtual void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			GetSelfRolePlayDataModel()?.UpdateTick(currentTime, currentFrameCount, delta);
			GetSelfRolePlayArtHelper()?.UpdateTick(currentTime, currentFrameCount, delta);
		}

		protected virtual void OnDestroy()
		{
		}

		/// <summary>
		/// <para>FixedUpdate时序的Tick，经由自身关联的Behaviour调用</para>
		/// <para>基类只是记录</para>
		/// </summary>
		public virtual void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			GetSelfRolePlayDataModel()?.FixedUpdateTick(currentTime, currentFrameCount, delta);
			_positionRecordList_Instant.RemoveAt(_positionRecordList_Instant.Count - 1);
			_positionRecordList_Instant.Insert(0, transform.position);
		}

#endregion

#region 移动尝试  ——  Movement

		private static List<(int, SingleCollisionInfo)> _characterMovementCheckRelatedCollisionInfos =
			new List<(int, SingleCollisionInfo)>();

		/// <summary>
		///  获取移动时用作逻辑检测的半径数值
		/// </summary>
		protected abstract float GetMovementLogicRadius();

		protected abstract Vector3? GetAlignedPosition(Vector3 fromPosition, float alignHeight);
		/// <summary>
		/// <para>在XYZ上移动位置</para>
		/// </summary>
		public virtual Vector3 TryMovePosition_XYZ(
			Vector3 deltaMovement,
			bool alignOnTerrain = true,
			float alignHeight = 3f,
			bool checkWithAirWall = true,
			bool checkWithCharacterVolume = false)
		{
			var fromPos = transform.position;

			bool collided = false;


			//在世界前上的投影检测
			Vector3 movementDeltaOnForward = Vector3.Project(deltaMovement,
				BaseGameReferenceService.CurrentBattleLogicalForwardDirection);

			if (checkWithAirWall)
			{
				int checkR_AtForward = Physics.RaycastNonAlloc(fromPos,
					movementDeltaOnForward.normalized,
					rbMoveCheckResults,
					movementDeltaOnForward.magnitude + GetMovementLogicRadius() + 0.01f,
					1 << 16);
				//发生了碰撞，则截断
				if (checkR_AtForward > 0)
				{
					collided = true;
					var hitPoint = rbMoveCheckResults[0].point;
					var lengthAtForward = Vector3.Distance(fromPos, hitPoint);
					movementDeltaOnForward = movementDeltaOnForward.normalized *
					                         (lengthAtForward - GetMovementLogicRadius() - 0.01f);
				}
			}

			Vector3 movementDeltaOnRight = Vector3.Project(deltaMovement,
				BaseGameReferenceService.CurrentBattleLogicRightDirection);

			if (checkWithAirWall)
			{
				int checkR_AtRight = Physics.RaycastNonAlloc(fromPos,
					movementDeltaOnRight.normalized,
					rbMoveCheckResults,
					movementDeltaOnRight.magnitude + GetMovementLogicRadius(),
					1 << 16);
				if (checkR_AtRight > 0)
				{
					collided = true;
					var hitPoint = rbMoveCheckResults[0].point;
					var lengthAtRight = Vector3.Distance(fromPos, hitPoint);
					movementDeltaOnRight = movementDeltaOnRight.normalized *
					                       (lengthAtRight - GetMovementLogicRadius() - 0.01f);
				}
			}


			Vector3 movementDeltaOnUp = Vector3.Project(deltaMovement, Vector3.up);

			//前方有空气墙
			if (collided && checkWithAirWall)
			{
				var ds_airWall = new DS_ActionBusArguGroup(
					ActionBus_ActionTypeEnum.L_Utility_MovementCollideWithAirWall_此次移动碰撞到了空气墙);
				_selfActionBusInstance.TriggerActionByType(ds_airWall);
			}


			//包含了角色体积检测，通常只有Boss才会带角色体积检测
			if (checkWithCharacterVolume)
			{
				_characterMovementCheckRelatedCollisionInfos.Clear();
				var allCharacterInCom = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
					.CurrentAllActiveARPGCharacterBehaviourCollection;
				
				foreach (BaseARPGCharacterBehaviour perBehaviour in allCharacterInCom)
				{
					if (!perBehaviour.EnableCharacterMovementColliderEquivalent)
					{
						continue;
					}


					ConSer_CollisionInfo currentActiveCollisionInfo = perBehaviour.GetCurrentActiveCollisionInfo();
					//在处理任意一个图形的时候，如果出现了 ：
					int currentState = 0;
					//State0 : 当前不在，目的地不在
					//State1 : 当前不在，目的地在
					//State2 : 当前在

					Vector3 currentPos = fromPos;
					Vector3 targetPos = fromPos + movementDeltaOnForward + movementDeltaOnRight;
					Vector3 targetPosOnRight = fromPos + movementDeltaOnRight;
					Vector3 targetPosOnForward = fromPos + movementDeltaOnForward;

					Vector3 behaviourPos = perBehaviour.transform.position;
					behaviourPos.y = 0f;

					var flipped = perBehaviour.GetIfFlipped();

					foreach (SingleCollisionInfo perSingleCollisionInfo in currentActiveCollisionInfo.CollisionInfoList)
					{
						Vector3 equivalentCenterOfThisInfo = behaviourPos;
						equivalentCenterOfThisInfo.x += flipped ? -perSingleCollisionInfo.OffsetPos.x
							: perSingleCollisionInfo.OffsetPos.x;
						equivalentCenterOfThisInfo.z += perSingleCollisionInfo.OffsetPos.y;
						float equivalentRadius =
							perSingleCollisionInfo.ColliderType == ColliderTypeInCollisionInfo.Box_等腰梯形
								? perSingleCollisionInfo.Radius / 2f : perSingleCollisionInfo.Radius;

						if (MathExtend.ContainsPointInCircleY0(currentPos,
							equivalentCenterOfThisInfo,
							equivalentRadius))
						{
							_characterMovementCheckRelatedCollisionInfos.Add((2,perSingleCollisionInfo));
							currentState = 2;
						}
						if (MathExtend.IntersectSegmentCircleV3Y0(currentPos,
							targetPos,
							equivalentCenterOfThisInfo,
							equivalentRadius))
						{
							_characterMovementCheckRelatedCollisionInfos.Add((1,perSingleCollisionInfo));
							if (currentState == 0)
							{
								currentState = 1;
							}
						}
					}

					switch (currentState)
					{
						case 0:
							 //当前不在。目的地不在。不对delta做任何修改。无事发生
							break;
						case 1:
							//当前不在，目的地在。需要截断进入的部分 。 并且处于1的前提是 当前 对于所有图形来说，都没有在任何图形之内。
							//			即，当前不需要处理挤出
							for (int i = 0; i < _characterMovementCheckRelatedCollisionInfos.Count; i++)
							{
								Vector3 equivalentCenterOfThisInfo = behaviourPos;
								var info = _characterMovementCheckRelatedCollisionInfos[i].Item2;
								equivalentCenterOfThisInfo.x += flipped ? info.OffsetPos
									.x : -info.OffsetPos.x;
								equivalentCenterOfThisInfo.z += info.OffsetPos.y;
								float equivalentRadius = info.ColliderType ==
								                         ColliderTypeInCollisionInfo.Box_等腰梯形
										? info.Radius / 2f
										: info.Radius ;
								equivalentRadius *= perBehaviour.EquivalentAbsorbRadiusMul;
								//逐个图形截断
								if (MathExtend.IntersectSegmentCircleV3Y0(fromPos,
									movementDeltaOnForward,
									equivalentCenterOfThisInfo,
									equivalentRadius))
								{
									movementDeltaOnRight = Vector3.zero;
								}
								if (MathExtend.IntersectSegmentCircleV3Y0(fromPos,
									movementDeltaOnRight,
									equivalentCenterOfThisInfo,
									equivalentRadius))
								{
									movementDeltaOnForward = Vector3.zero;
								}
							}
							break;
						case 2:
							//出现当前在的时候，依然可能出现对于 其他若干图形来说处于当前不在目的地在的情况。
							//只处理state为2的
							var minRadius = 9999f;
							var minDisToCenter = 9999f;
							for (int i = 0; i < _characterMovementCheckRelatedCollisionInfos.Count; i++)
							{
								if (_characterMovementCheckRelatedCollisionInfos[i].Item1 != 2)
								{
									continue;
								}
								var info = _characterMovementCheckRelatedCollisionInfos[i].Item2;
								Vector3 equivalentCenterOfThisInfo = behaviourPos;
								equivalentCenterOfThisInfo.x += flipped ? -_characterMovementCheckRelatedCollisionInfos[i].Item2.OffsetPos.x
									: _characterMovementCheckRelatedCollisionInfos[i].Item2.OffsetPos.x;
								equivalentCenterOfThisInfo.z += _characterMovementCheckRelatedCollisionInfos[i].Item2.OffsetPos.y;
								
								float currentDisToCenter = Vector3.Distance(fromPos, equivalentCenterOfThisInfo);
								currentDisToCenter *= perBehaviour.EquivalentAbsorbRadiusMul;
								if(minRadius > currentDisToCenter)
								{
									minRadius = currentDisToCenter;
								}
								
							}
							//把这个
							
							 movementDeltaOnRight *= -minRadius;
							 movementDeltaOnRight.x = Mathf.Clamp( movementDeltaOnRight.x , -minRadius , minRadius);
							 movementDeltaOnRight.z = Mathf.Clamp( movementDeltaOnRight.z , -minRadius , minRadius);
							 movementDeltaOnForward *= -minRadius;
							 movementDeltaOnForward.x = Mathf.Clamp( movementDeltaOnForward.x , -minRadius , minRadius);
							 movementDeltaOnForward.z = Mathf.Clamp( movementDeltaOnForward.z , -minRadius , minRadius);
							 
							 
							 
							 
							break;
					}
				}
			}





			Vector3 finalDelta = movementDeltaOnForward + movementDeltaOnRight + movementDeltaOnUp;



			Vector3 finalPos;
			if (alignOnTerrain)
			{
				finalPos = GetAlignedPosition(fromPos + finalDelta, alignHeight) ?? fromPos + finalDelta;
			}
			else
			{
				finalPos = fromPos + finalDelta;
			}



			if (checkWithAirWall)
			{
				//23.9.7增加了计算完成后的再次检测。因为在8月一段时间的测试中，偶尔会出现直接进入空气墙的问题。
				//所以又增加了一个overlap检测，如果被检测到了，此次移动全部截断
				var tt = Physics.OverlapSphereNonAlloc(finalPos, GetMovementLogicRadius(), rbCheckResults, 1 << 16);
				if (tt > 0)
				{
					transform.position = fromPos;
					return fromPos;
				}
			}

			transform.position = finalPos;
			return finalPos;




			// void Sync(BaseARPGCharacterBehaviour self,Vector3 pos)
			// {
			// 	if (self is not PlayerARPGConcreteCharacterBehaviour)
			// 	{
			// 		return;
			// 	}
			// 	foreach (var perBehaviour in _glmRef.PlayerCharacterBehaviourControllerReference.CurrentAllCharacterBehaviourList)
			// 	{
			// 		if (self == perBehaviour)
			// 		{
			// 			continue;
			// 		}
			// 		perBehaviour.SelfRigidbody.position = pos;
			// 	}
			// }
		}

		/// <summary>
		/// <para>试图移动自身位置。常见于强制位移 </para>
		/// <para>会检测空气墙，射线检测，尽量避免穿模</para>
		/// </summary>
		public virtual Vector3 TryMovePosition_OnlyXZ(
			Vector3 deltaMovement,
			bool alignWithTerrain = true,
			float alignHeight = 3f,
			bool checkWithAirWall = true)
		{
			//检测目的方向是否有空气墙
			//需要在世界前和世界右两个方向上检测，分别检测，如果有空气墙则分别截断

			var originalMag = deltaMovement.magnitude;
			deltaMovement.y = 0f;
			deltaMovement = deltaMovement.normalized * originalMag;
			return TryMovePosition_XYZ(deltaMovement, alignWithTerrain, alignHeight, checkWithAirWall);
		}

#endregion

#region 位置记录

		protected static int _positionRecordCapacity = 120;


		/// <summary>
		/// <para>位置记录_实时的。这意味着如果一段时间不动，所有记录都会同步为当前位置。</para>
		/// <para>如果需要那种不动就不记的，用OnlyMovement版</para>
		/// <para>这个常用于 残影 之类的效果</para>
		/// </summary>
		private List<Vector3> _positionRecordList_Instant = new List<Vector3>();



		/// <summary>
		///  <para>位置记录最小阈值。如果平方小于这个数值，则视作没有发生移动。</para>
		/// </summary>
		private static float _positionRecordInterval = 0.0025f;
		/// <summary>
		/// <para>位置记录：位移时才记录</para>
		/// </summary>
		private List<Vector3> _positionRecordList_OnlyMovement = new List<Vector3>();


		/// <summary>
		/// 更新一次位置记录。这是始终记录的，无论是否真的发生位移
		/// </summary>
		/// <param name="pos"></param>
		protected virtual void UpdateRecordPosition_Instant(Vector3 pos)
		{
			_positionRecordList_Instant.RemoveAt(_positionRecordList_Instant.Count - 1);
			_positionRecordList_Instant.Insert(0, pos);
		}


		/// <summary>
		///  更新一次位置记录。这是只有发生位移时才记录的
		///  </summary>
		protected virtual void UpdateRecordPosition_MovementOnly(Vector3 position)
		{
			Vector3 currentPos = transform.position;
			if (Vector3.SqrMagnitude(currentPos -
			                         _positionRecordList_OnlyMovement[_positionRecordList_OnlyMovement.Count - 1]) >
			    _positionRecordInterval)
			{
				_positionRecordList_OnlyMovement.RemoveAt(_positionRecordList_OnlyMovement.Count - 1);
				_positionRecordList_OnlyMovement.Insert(0, currentPos);
			}
		}


		/// <summary>
		/// <para>重新填充位置记录信息。常见于加载、过场等情景，需要清空之前记录的各种信息，全部重置为当前位置</para>
		/// </summary>
		public void RefillPositionRecord(Vector3 pos, float offset)
		{
			_positionRecordList_Instant.Clear();
			_positionRecordList_OnlyMovement.Clear();

			var targetPos = pos + Vector3.forward * offset;

			for (int i = 0; i < _positionRecordCapacity; i++)
			{
				_positionRecordList_Instant.Add(targetPos);
				_positionRecordList_OnlyMovement.Add(targetPos);
			}
		}


		/// <summary>
		/// <para>获取 这么多 逻辑帧之前的位置</para>
		/// </summary>
		public Vector3 GetPositionRecordByFrameInterval(int offsetsFromCurrent)
		{
			offsetsFromCurrent = Mathf.Clamp(offsetsFromCurrent, 0, _positionRecordCapacity - 1);
			return _positionRecordList_Instant[offsetsFromCurrent];
		}

		/// <summary>
		/// <para>获取这么多 逻辑帧之前的 包含有效移动的位置</para>
		/// </summary>
		public Vector3 GetPositionRecordByFrameInterval_OnlyMovement(int offsetsFromCurrent)
		{
			offsetsFromCurrent = Mathf.Clamp(offsetsFromCurrent, 0, _positionRecordCapacity - 1);
			return _positionRecordList_OnlyMovement[offsetsFromCurrent];
		}

#endregion

#region I_RP_接口实现

		public string ApplyBuff_GetRelatedCasterName()
		{
			return name;
		}

		public RP_DataEntry_Base ApplyBuff_GetRelatedDataEntry(RP_DataEntry_EnumType type, bool allowNotExist = false)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.GetTargetDataEntry(type, allowNotExist);
		}
		public BaseRPBuff ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum buffType)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.GetTargetBuff(buffType);
		}


		public BuffApplyResultEnum ReceiveBuff_TryApplyBuff(
			RolePlay_BuffTypeEnum buffType,
			I_RP_Buff_ObjectCanApplyBuff caster,
			I_RP_Buff_ObjectCanReceiveBuff receiver,
			params BaseBuffLogicPassingComponent[] logicPassingComponents)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance
				.TryApplyBuff(buffType, caster, receiver, logicPassingComponents).Item1;
		}
		public BuffApplyResultEnum ReceiveBuff_TryApplyBuff(
			RolePlay_BuffTypeEnum buffType,
			I_RP_Buff_ObjectCanApplyBuff caster,
			I_RP_Buff_ObjectCanReceiveBuff receiver,
			List<BaseBuffLogicPassingComponent> logicPassingComponents)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance
				.TryApplyBuff(buffType, caster, receiver, logicPassingComponents).Item1;
		}
		public virtual bool CurrentDataValid()
		{
			return true;
		}
		public Vector3 GetBuffReceiverPosition()
		{
			return transform.position;
		}
		public RolePlay_ArtHelperBase ReceiveBuff_GetRelatedArtHelper()
		{
			return GetSelfRolePlayArtHelper();
		}

		public BuffRemoveResultEnum ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.TryRemoveBuff(type);
		}

		public LocalActionBus ReceiveBuff_GetRelatedActionBus()
		{
			return GetRelatedActionBus();
		}

		public BuffAvailableType ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.CheckTargetBuff(type);
		}

		public BaseRPBuff ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum type, bool createWhenNotExist = false)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.GetTargetBuff(type, createWhenNotExist);
		}
		public bool ReceiveBuff_CheckExistValidBuffWithTag(RP_BuffInternalFunctionFlagTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.ReceiveBuff_CheckExistValidBuffWithTag(type);
		}

		public Float_RPDataEntry ReceiveBuff_GetFloatDataEntry(
			RP_DataEntry_EnumType entryType,
			bool allowNotExist = false)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.GetTargetDataEntry(entryType, allowNotExist) as
				Float_RPDataEntry;
		}
		public FloatPresentValue_RPDataEntry ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType type)
		{
			return GetSelfRolePlayDataModel().GetFloatPresentValue(type);
		}

		public RP_DataEntry_Base ReceiveBuff_GetDataEntry(RP_DataEntry_EnumType entryType)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.GetTargetDataEntry(entryType);
		}

		public string ApplyDamage_GetRelatedCasterName()
		{
			return name;
		}

		public Float_RPDataEntry ApplyDamage_GetRelatedDataEntry(RP_DataEntry_EnumType type, bool allowNotExist = false)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.GetFloatDataEntryByType(type, allowNotExist);
		}

		public BuffAvailableType ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.CheckTargetBuff(type);
		}

		public BaseRPBuff ApplyDamage_GetTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.GetTargetBuff(type);
		}


		// public virtual RP_DS_DamageApplyResult ReceiveDamage_ReceiveFromRPDS(RP_DS_DamageApplyInfo applyInfo,
		// 	int effectIDStamp)
		// {
		// 	return GetSelfRolePlayDataModel().ReceiveDamage(applyInfo, effectIDStamp);
		// }

		public virtual Vector3 ReceiveDamage_GetCurrentReceiverPosition()
		{
			return transform.position;
		}

		public Float_RPDataEntry ReceiveDamage_GetRelatedDataEntry(RP_DataEntry_EnumType dataEntryType)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.GetFloatDataEntryByType(dataEntryType);
		}

		public BuffAvailableType ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.CheckTargetBuff(type);
		}

		public BaseRPBuff ReceiveDamage_GetTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.GetTargetBuff(type);
		}

		public Vector2 ReceiveDamage_GetCurrentReceiverDirection()
		{
			Vector3 dir = GetSelfRolePlayArtHelper().transform.forward;
			return new Vector2(dir.x, dir.z);
		}

		public void ReceiveDataEntry_ReceiveFromRPDS(RP_DS_DataEntryApplyInfo rpds)
		{
			GetSelfRolePlayDataModel().ReceiveDataEntryEffectFromRPDS(rpds);
		}

		public RP_DataEntry_Base ReceiveDataEntry_GetRelatedDataEntry(RP_DataEntry_EnumType type)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.GetTargetDataEntry(type);
		}

		public BuffAvailableType ReceiveDataEntry_CheckTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.CheckTargetBuff(type);
		}

		public BaseRPBuff ReceiveDataEntry_GetTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.GetTargetBuff(type);
		}

		public RP_DataEntry_Base ApplyDataEntry_GetRelatedDataEntry(RP_DataEntry_EnumType type)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.GetTargetDataEntry(type);
		}

		public BuffAvailableType ApplyDataEntry_CheckTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.CheckTargetBuff(type);
		}

		public BaseRPBuff ApplyDataEntry_GetTargetBuff(RolePlay_BuffTypeEnum type)
		{
			return GetSelfRolePlayDataModel().SelfBuffHolderInstance.GetTargetBuff(type);
		}

		public string GetObjectName_ObjectContainDataEntryDatabase()
		{
			return gameObject.name;
		}
		public LocalActionBus GetRelatedActionBus()
		{
			return _selfActionBusInstance;
		}
		public Float_RPDataEntry GetFloatDataEntryByType(RP_DataEntry_EnumType type, bool allowNotExist = false)
		{
			return GetSelfRolePlayDataModel().GetFloatDataEntry(type, allowNotExist);
		}

		public Float_RPDataEntry InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType type, float initValue)
		{
			return GetSelfRolePlayDataModel().SelfDataEntry_Database.InitializeFloatDataEntry(type, initValue);
		}
		public FloatPresentValue_RPDataEntry GetFloatPresentValueByType(RP_DataEntry_EnumType type)
		{
			return GetSelfRolePlayDataModel().GetFloatPresentValue(type);
		}
		public Vector3 GetCasterPosition()
		{
			return transform.position;
		}
		public virtual Vector3 GetCasterForwardDirection()
		{
			return GetSelfRolePlayArtHelper().transform.forward;
		}

		public Vector3 GetCasterY0Position()
		{
			return new Vector3(transform.position.x, 0f, transform.position.z);
		}
		public void FillDataCache(Dictionary<RP_DataEntry_EnumType, float> dataEntryCache)
		{
			GetSelfRolePlayDataModel().SelfDataEntry_Database.FillDataEntryCache(dataEntryCache);
		}

#endregion


#if UNITY_EDITOR
		public RolePlay_DataModelBase _EditorOnly_GetDataModel() { return GetSelfRolePlayDataModel(); }
		protected virtual void OnDrawGizmos()
		{
		}

#endif
		//
		//
		// public void AlignWithTerrain(float heightOffset = 0f)
		// {
		// 	Vector3 originalPos = transform.position;
		// 	Vector3 checkFromPos = new Vector3(originalPos.x, originalPos.y + 10f, originalPos.z);
		// 	int c = Physics.RaycastNonAlloc(checkFromPos, Vector3.down, _selfRaycastHitResults, 999f, (1 << 11));
		//
		// 	if (c == 0)
		// 	{
		// 		return;
		// 	}
		// 	else
		// 	{
		// 		Vector3 targetPos = _selfRaycastHitResults[0].point;
		// 		targetPos.y += heightOffset;
		// 		SetPosition(targetPos);
		// 	}
		// }
		public void RemoveProjectileStamp(int id)
		{
			GetSelfRolePlayDataModel().RemoveDamageStamp(id);
		}
	}
}