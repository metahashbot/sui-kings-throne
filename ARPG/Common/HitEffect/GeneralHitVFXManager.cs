using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common.HitEffect
{

	[TypeInfoBox("ARPG场景中，来自投射物的通用特效管理\n" + "不是投射物的特效，通常由发起的地方自行处理（比如Buff）。或者这里只提供VFX Prefab的查找功能")]
	public class GeneralHitVFXManager : MonoBehaviour
	{
		public static GeneralHitVFXManager Instance;



		private SOCollection_HitVFXGroupConfig _hitVFXGroupConfigCollectionRef;
		private SOCollection_HitColorGroupConfig _hitColorGroupConfigCollectionRef;

		private SOConfig_HitVFXGroupConfig _cache_commonDefaultConfigRef;

		private SOConfig_HitVFXGroupConfig _cache_playerDefaultConfigRef;

		private SOConfig_HitColorGroupConfig _cache_commonDefaultColorConfigRef;


		public List<SOConfig_HitVFXGroupConfig> CurrentAllActiveHitVFXGroupConfig =
			new List<SOConfig_HitVFXGroupConfig>();


		[FoldoutGroup("通用参数", true)]
		[SerializeField, LabelText("开启 屏幕空间 的纵深高度修正")]
		private bool _enableScreenSpaceZDepthFix = true;

		/// <summary>
		/// 每在Z轴大于本体1单位，跳字高度就提高这么多
		/// </summary>
		[SerializeField, LabelText("屏幕空间的纵深高度修正的乘数")]
		[ShowIf(nameof(_enableScreenSpaceZDepthFix))] [FoldoutGroup("通用参数", true)]
		private float _screenSpaceZDepthFixValue = 0.4f;


		private UnityEngine.Camera _mainCameraRef;

		private class ThisHitVFXInfo
		{

			public List<PerDamageVFXInfoPair> AllOverrideGroup = new List<PerDamageVFXInfoPair>();

			public List<PerDamageVFXInfoPair> PriorityGroup = new List<PerDamageVFXInfoPair>();

			public List<PerDamageVFXInfoPair> AddonGroup = new List<PerDamageVFXInfoPair>();

			public void Clear()
			{
				AllOverrideGroup.Clear();
				PriorityGroup.Clear();
				AddonGroup.Clear();
			}


		}



		[Title("=== 生成位置 ===", TitleAlignment = TitleAlignments.Centered)]
		[SerializeField, LabelText("在[世界上]方向上偏移范围曲线")]
		public AnimationCurve PopupPositionOffset_Up = new AnimationCurve(new Keyframe(0f, 0.35f),
			new Keyframe(0.5f, -0.5f),
			new Keyframe(1f, 0.35f));

		[NonSerialized]
		public float CurrentStep_Up;

		[SerializeField, LabelText("↑单次步进范围")]
		public Vector2 PopupPositionOffset_Up_Step = new Vector2(0.1f, 0.2f);


		public float GetCurrentStop_Up()
		{
			CurrentStep_Up += UnityEngine.Random.Range(PopupPositionOffset_Up_Step.x, PopupPositionOffset_Up_Step.y);
			if (CurrentStep_Up > 1f)
			{
				CurrentStep_Up -= 1f;
			}
			return PopupPositionOffset_Up.Evaluate(CurrentStep_Up);
		}




		[SerializeField, LabelText("在[世界右] 方向上偏移范围曲线")]
		public AnimationCurve PopupPositionOffsetRange_Right = new AnimationCurve(new Keyframe(0f, 0.35f),
			new Keyframe(0.5f, -0.5f),
			new Keyframe(1f, 0.35f));

		[NonSerialized]
		public float CurrentStep_Right;

		[SerializeField, LabelText("↑单次步进范围")]
		public Vector2 PopupPositionOffset_Right_Step = new Vector2(0.1f, 0.2f);


		public float GetCurrentStop_Right()
		{
			CurrentStep_Right += UnityEngine.Random.Range(PopupPositionOffset_Right_Step.x,
				PopupPositionOffset_Right_Step.y);
			if (CurrentStep_Right > 1f)
			{
				CurrentStep_Right -= 1f;
			}
			return PopupPositionOffsetRange_Right.Evaluate(CurrentStep_Right);
		}


		[SerializeField, LabelText("起始高度 —— 高于伤害发生位置这么多")]
		public float PopYInitialOffset = 1f;

		[SerializeField, LabelText("到摄像机距离的修正——负得越多则越靠近屏幕")]
		public float PopZInitialOffset = -0.5f;





		private ThisHitVFXInfo CurrentHitVFXInfo;


		public void AwakeInitialize()
		{
			Instance = this;
			CurrentHitVFXInfo = new ThisHitVFXInfo();
			_hitVFXGroupConfigCollectionRef = GlobalConfigurationAssetHolderHelper.Instance.Collection_HitVFXGroup;
			_hitColorGroupConfigCollectionRef = GlobalConfigurationAssetHolderHelper.Instance.Collection_HitColorGroup;

			_cache_commonDefaultConfigRef =
				_hitVFXGroupConfigCollectionRef.Collection.Find(x => x.ConfigName.Equals("标准默认"));
			_cache_playerDefaultConfigRef =
				_hitVFXGroupConfigCollectionRef.Collection.Find(x => x.ConfigName.Equals("标准默认"));
			_cache_commonDefaultColorConfigRef =
				_hitColorGroupConfigCollectionRef.Collection.Find(x => x.ConfigName.Equals("标准默认"));
			CurrentAllActiveHitVFXGroupConfig.Add(_cache_commonDefaultConfigRef);
			CurrentAllActiveHitVFXGroupConfig.Add(_cache_playerDefaultConfigRef);
			CurrentAllActiveHitVFXGroupConfig.Add(_cache_commonDefaultConfigRef);
		}












		public void StartInitialize()
		{
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
				_ABC_GenerateVFX_OnDamageResultEntryGenerated);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
				_ABC_ProcessHitColor_OnDamageResultEntryGenerated);
			_mainCameraRef = GameReferenceService_ARPG.Instance.CameraBehaviourRef.MainCamera;
		}


		/// <summary>
		/// <para>处理受击颜色效果</para>
		/// </summary>
		private void _ABC_ProcessHitColor_OnDamageResultEntryGenerated(DS_ActionBusArguGroup ds)
		{
			RP_DS_DamageApplyResult damageApplyResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;


			//跳字小于0.1时不播放特效
			if (damageApplyResult.PopupDamageNumber < 0.1f)
			{
				return;
			}
			if (damageApplyResult.ProcessOption.IgnoreTint)
			{
				return;
			}

			PerHitColorInfoPair requiredInfo = null;

			if (damageApplyResult.RelatedProjectileRuntimeRef != null &&
			    damageApplyResult.RelatedProjectileRuntimeRef.SelfLayoutConfigReference != null)
			{
				if (damageApplyResult.ResultLogicalType != RP_DamageResultLogicalType.NormalResult)
				{
					return;
				}

				if (!damageApplyResult.RelatedProjectileRuntimeRef.SelfLayoutConfigReference.LayoutContentInSO
					.ContainHitColorEffect)
				{
					return;
				}
				var s = damageApplyResult.RelatedProjectileRuntimeRef.SelfLayoutConfigReference.LayoutContentInSO
					.HitCharacterColorConfigNameID;
				if (s.Equals(_cache_commonDefaultColorConfigRef.ConfigName, StringComparison.OrdinalIgnoreCase))
				{
					requiredInfo =
						HitColor_GetRequiredHitColorInfoFromGroupConfigByRPDS(_cache_commonDefaultColorConfigRef,
							damageApplyResult);
				}
				else
				{
					var findIndex = _hitColorGroupConfigCollectionRef.Collection.FindIndex(x =>
						x.ConfigName.Equals(s, StringComparison.OrdinalIgnoreCase));
					if (findIndex == -1)
					{
						DBug.LogWarning($"没有找到名为{s}的受击颜色配置");
						return;
					}
					SOConfig_HitColorGroupConfig findConfig = _hitColorGroupConfigCollectionRef.Collection[findIndex];
					requiredInfo = HitColor_GetRequiredHitColorInfoFromGroupConfigByRPDS(findConfig, damageApplyResult);
				}
			}
			else
			{
				switch (damageApplyResult.ResultLogicalType)
				{
					case RP_DamageResultLogicalType.NormalResult:
						requiredInfo =
							HitColor_GetRequiredHitColorInfoFromGroupConfigByRPDS(_cache_commonDefaultColorConfigRef,
								damageApplyResult);
						break;
					case RP_DamageResultLogicalType.InvincibleAllSoNothing:
						break;
					case RP_DamageResultLogicalType.DodgedSoNothing:
						break;
					case RP_DamageResultLogicalType.ParriedAndOnlyEffect:
						break;
					case RP_DamageResultLogicalType.InvalidDamage:
						break;
					case RP_DamageResultLogicalType.ActAsHeal:
						break;
				}
			}



			if (requiredInfo != null)
			{
				HitColorEffectInfo content = requiredInfo.HitColorEffectInfoContent;
				var ds_hit =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Damage_RequiredHitColorEffect_要求命中颜色效果);
				ds_hit.ObjectArgu1 = content;
				ds_hit.ObjectArgu2 = damageApplyResult;
				ds_hit.ObjectArguStr = requiredInfo;
				damageApplyResult.Receiver.GetRelatedActionBus().TriggerActionByType(ds_hit);
			}
			else
			{
				// DBug.LogWarning( $"没有 在 配置组:{s}内找到合适的受击颜色配置");
			}
		}

		private PerHitColorInfoPair HitColor_GetRequiredHitColorInfoFromGroupConfigByRPDS(
			SOConfig_HitColorGroupConfig hit,
			RP_DS_DamageApplyResult dar)
		{
			foreach (PerHitColorInfoPair perInfo in hit.HitColorConfigList)
			{
				switch (perInfo.Requirement)
				{
					case VFXInfo_CommonHit vfxInfoCommonHit:
						if (vfxInfoCommonHit.DamageType == DamageTypeEnum.None &&
						    vfxInfoCommonHit.RelatedBuff == RolePlay_BuffTypeEnum.None)
						{
							return perInfo;
						}
						else if (vfxInfoCommonHit.DamageType == DamageTypeEnum.None &&
						         vfxInfoCommonHit.RelatedBuff != RolePlay_BuffTypeEnum.None &&
						         dar.Receiver.ReceiveDamage_CheckTargetBuff(vfxInfoCommonHit.RelatedBuff) ==
						         BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							return perInfo;
						}
						else if (vfxInfoCommonHit.DamageType != DamageTypeEnum.None &&
						         dar.DamageType == vfxInfoCommonHit.DamageType &&
						         vfxInfoCommonHit.RelatedBuff == RolePlay_BuffTypeEnum.None)
						{
							return perInfo;
						}
						else if (vfxInfoCommonHit.DamageType != DamageTypeEnum.None &&
						         dar.DamageType == vfxInfoCommonHit.DamageType &&
						         dar.Receiver.ReceiveDamage_CheckTargetBuff(vfxInfoCommonHit.RelatedBuff) ==
						         BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							return perInfo;
						}
						break;
					case VFXInfo_OnlyBuff vfxInfoOnlyBuff:
						if (vfxInfoOnlyBuff.RelatedBuff == RolePlay_BuffTypeEnum.None)
						{
							return perInfo;
						}
						else if (dar.Receiver.ReceiveDamage_CheckTargetBuff(vfxInfoOnlyBuff.RelatedBuff) ==
						         BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							return perInfo;
						}
						break;
				}
			}
			return null;
		}



		private void _ABC_GenerateVFX_OnDamageResultEntryGenerated(DS_ActionBusArguGroup ds)
		{
			CurrentHitVFXInfo.Clear();
			var damageApplyResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			//如果不是Projectile造成的就直接无视
			if (damageApplyResult.RelatedProjectileRuntimeRef == null ||
			    !damageApplyResult.RelatedProjectileRuntimeRef.SelfLayoutConfigReference)
			{
				return;
			}
			//Temp ： 临时：治疗伤害不弹
			if (damageApplyResult.DamageType == DamageTypeEnum.Heal_治疗)
			{
				return;
			}
			//如果伤害过低，就不弹了
			if (damageApplyResult.ResultLogicalType == RP_DamageResultLogicalType.NormalResult &&
			    damageApplyResult.PopupDamageNumber < 0.1f)
			{
				return;
			}


			if (damageApplyResult.ResultLogicalType == RP_DamageResultLogicalType.NormalResult)
			{
				HitConfigInSource hitConfig = damageApplyResult.RelatedProjectileRuntimeRef.SelfLayoutConfigReference
					.LayoutContentInSO.HitVFXConfig;
				//为空或者是 allDefault 那就是默认情况，按完全默认处理
				if (hitConfig == null || hitConfig.AllDefault)
				{
					SpawnHitVFX_AllDefault(damageApplyResult);
				}

				if (hitConfig != null && hitConfig.RelatedConfigName != null && hitConfig.RelatedConfigName.Length > 0)
				{
					foreach (var perConfigName in hitConfig.RelatedConfigName)
					{
						SpawnHitVFX_ByConfigName(damageApplyResult, perConfigName);
					}
				}


				PlayThisVFXGroup(damageApplyResult);
			}
		}

		private void SpawnHitVFX_AllDefault(RP_DS_DamageApplyResult damageApplyResult)
		{
			Vector3 pos = damageApplyResult.DamageWorldPosition ?? Vector3.zero;


			DamageTypeEnum damageType = damageApplyResult.DamageType;



			if (damageApplyResult.Receiver is EnemyARPGCharacterBehaviour enemy)
			{
				if (enemy.ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人) !=
				    BuffAvailableType.NotExist)
				{
					PerDamageVFXInfoPair findPair = _cache_commonDefaultConfigRef.AllDamageVFXInfoList.Find((pair =>
						pair.VFXInfoContent as VFXInfo_CommonHit != null &&
						(pair.VFXInfoContent as VFXInfo_CommonHit).DamageType == damageType &&
						(pair.VFXInfoContent as VFXInfo_CommonHit).RelatedBuff ==
						RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人));

					CurrentHitVFXInfo.PriorityGroup.Add(findPair);
				}
				else if (enemy.ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人) !=
				         BuffAvailableType.NotExist)
				{
					PerDamageVFXInfoPair findPair = _cache_commonDefaultConfigRef.AllDamageVFXInfoList.Find((pair =>
						pair.VFXInfoContent as VFXInfo_CommonHit != null &&
						(pair.VFXInfoContent as VFXInfo_CommonHit).DamageType == damageType &&
						(pair.VFXInfoContent as VFXInfo_CommonHit).RelatedBuff ==
						RolePlay_BuffTypeEnum._EnemyTag_EliteEnemy_精英敌人));

					CurrentHitVFXInfo.PriorityGroup.Add(findPair);
				}
				else if (enemy.ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) !=
				         BuffAvailableType.NotExist)
				{
					PerDamageVFXInfoPair findPair = _cache_commonDefaultConfigRef.AllDamageVFXInfoList.Find((pair =>
						pair.VFXInfoContent as VFXInfo_CommonHit != null &&
						(pair.VFXInfoContent as VFXInfo_CommonHit).DamageType == damageType &&
						(pair.VFXInfoContent as VFXInfo_CommonHit).RelatedBuff ==
						RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人));

					CurrentHitVFXInfo.PriorityGroup.Add(findPair);
				}
			}
			else if (damageApplyResult.Receiver is PlayerARPGConcreteCharacterBehaviour player)
			{
				PerDamageVFXInfoPair findPair = _cache_playerDefaultConfigRef.AllDamageVFXInfoList.Find((pair =>
					pair.VFXInfoContent as VFXInfo_CommonHit != null &&
					(pair.VFXInfoContent as VFXInfo_CommonHit).DamageType == damageType));

				CurrentHitVFXInfo.PriorityGroup.Add(findPair);
			}
		}


		//get position projectile on plane 


		private void SpawnHitVFX_ByConfigName(RP_DS_DamageApplyResult damageApplyResult, string configName)
		{
			if (damageApplyResult.Receiver is EnemyARPGCharacterBehaviour enemy)
			{
				SOConfig_HitVFXGroupConfig findConfig =
					_hitVFXGroupConfigCollectionRef.Collection.Find(config => config.ConfigName == configName);
				if (findConfig != null)
				{
					foreach (PerDamageVFXInfoPair perInfo in findConfig.AllDamageVFXInfoList)
					{
						switch (perInfo.VFXInfoContent)
						{
							case VFXInfo_CommonHit vfxInfoCommonHit:
								if (vfxInfoCommonHit.DamageType == DamageTypeEnum.None &&
								    vfxInfoCommonHit.RelatedBuff == RolePlay_BuffTypeEnum.None)
								{
									CurrentHitVFXInfo.PriorityGroup.Add(perInfo);
								}
								else if (vfxInfoCommonHit.DamageType != DamageTypeEnum.None &&
								         damageApplyResult.DamageType == vfxInfoCommonHit.DamageType &&
								         vfxInfoCommonHit.RelatedBuff == RolePlay_BuffTypeEnum.None)
								{
									CurrentHitVFXInfo.PriorityGroup.Add(perInfo);
								}
								else if (vfxInfoCommonHit.DamageType == DamageTypeEnum.None &&
								         vfxInfoCommonHit.RelatedBuff != RolePlay_BuffTypeEnum.None &&
								         enemy.ReceiveBuff_CheckTargetBuff(vfxInfoCommonHit.RelatedBuff) !=
								         BuffAvailableType.NotExist)
								{
									CurrentHitVFXInfo.PriorityGroup.Add(perInfo);
								}
								else if (vfxInfoCommonHit.DamageType != DamageTypeEnum.None &&
								         damageApplyResult.DamageType == vfxInfoCommonHit.DamageType &&
								         vfxInfoCommonHit.RelatedBuff != RolePlay_BuffTypeEnum.None &&
								         enemy.ReceiveBuff_CheckTargetBuff(vfxInfoCommonHit.RelatedBuff) !=
								         BuffAvailableType.NotExist)
								{
									CurrentHitVFXInfo.PriorityGroup.Add(perInfo);
								}
								break;
							case VFXInfo_OnlyBuff vfxInfoOnlyBuff:
								if (enemy.ReceiveDamage_CheckTargetBuff(vfxInfoOnlyBuff.RelatedBuff) !=
								    BuffAvailableType.NotExist)
								{
									CurrentHitVFXInfo.PriorityGroup.Add(perInfo);
								}
								break;
						}
					}
				}
			}
			else if (damageApplyResult.Receiver is PlayerARPGConcreteCharacterBehaviour player)
			{
				SOConfig_HitVFXGroupConfig findConfig =
					_hitVFXGroupConfigCollectionRef.Collection.Find(config => config.ConfigName == configName);
				if (findConfig != null)
				{
					foreach (PerDamageVFXInfoPair perInfo in findConfig.AllDamageVFXInfoList)
					{
						switch (perInfo.VFXInfoContent)
						{
							case VFXInfo_CommonHit vfxInfoCommonHit:
								CurrentHitVFXInfo.PriorityGroup.Add(perInfo);
								break;
							case VFXInfo_OnlyBuff vfxInfoOnlyBuff:
								if (player.ReceiveDamage_CheckTargetBuff(vfxInfoOnlyBuff.RelatedBuff) !=
								    BuffAvailableType.NotExist)
								{
									CurrentHitVFXInfo.PriorityGroup.Add(perInfo);
								}
								break;
						}
					}
				}
			}
		}

		public void PlayThisVFXGroup(RP_DS_DamageApplyResult damageApplyResult)
		{
			//如果有[AllOverride]组，则不播放 Priority组和Addon组
			if (CurrentHitVFXInfo.AllOverrideGroup.Count > 0)
			{
			}
			//可以播放 优先级组 和 Addon组
			else
			{
				if (CurrentHitVFXInfo.PriorityGroup.Count > 0)
				{
					CurrentHitVFXInfo.PriorityGroup.Sort(((pair, infoPair) =>
						(pair.VFXSpawnSource as VFXSpawn_Priority).Priority.CompareTo(
							(infoPair.VFXSpawnSource as VFXSpawn_Priority).Priority)));
					if (CurrentHitVFXInfo.PriorityGroup[0].VFXSpawnSource == null)
					{
						return;
					}
					int firstPriority = (CurrentHitVFXInfo.PriorityGroup[0].VFXSpawnSource as VFXSpawn_Priority)
						.Priority;


					foreach (PerDamageVFXInfoPair perPair in CurrentHitVFXInfo.PriorityGroup)
					{
						var vfxSpawn_Priority = (perPair.VFXSpawnSource as VFXSpawn_Priority);

						if (vfxSpawn_Priority.Priority < firstPriority)
						{
							break;
						}
						var getPos = GetSpawnFromPosition(perPair, damageApplyResult);
						PlaySingleVFX(perPair, getPos, GetSpawnScale(damageApplyResult));
					}
				}
			}
		}

		private Vector3 GetSpawnScale(RP_DS_DamageApplyResult dar)
		{
			var targetScale = Vector3.one;
			if (dar.Receiver == null || dar.Receiver is not BaseARPGCharacterBehaviour behaviour)
			{
				return targetScale;
			}
			targetScale *= (behaviour.GetRelatedArtHelper()._VFXScaleRadius * behaviour.SelfCharacterVariantScale);
			if (behaviour.GetIfFlipped())
			{
				targetScale.x *= -1f;
			}

			return targetScale;
		}

		private Vector3 GetSpawnFromPosition(PerDamageVFXInfoPair vfxInfoPair, RP_DS_DamageApplyResult rpds_dar)
		{
			Vector3 popupFromPosition = Vector3.zero;
			if (rpds_dar.DamageWorldPosition != null)
			{
				popupFromPosition = rpds_dar.DamageWorldPosition.Value;
				var receiver = rpds_dar.Receiver;
				var receiverPos = receiver.ReceiveDamage_GetCurrentReceiverPosition();
				var dirFromReceiverToDamage = (popupFromPosition - receiverPos).normalized;
				if (dirFromReceiverToDamage.z > 0f)
				{
					dirFromReceiverToDamage.z = 0f;
				}

				//需要先计算出来，经过高度修正后的位置，然后投射到目标平面上
				// 本体艺术半径 乘这个方向，然后投射；
				//然后统一加上默认的高度修正
				//然后再计算深度，如果更靠内了，则需要 基于屏幕空间的差距，来计算这个修正

				var vfxRadiusOfReceiver = receiver.GetRelatedArtHelper()._VFXScaleRadius;
				if (Vector3.SqrMagnitude(receiverPos - popupFromPosition) < vfxRadiusOfReceiver * vfxRadiusOfReceiver)
				{
					//在内部，不需要修正
				}
				else
				{
					//在外部，需要修正
					popupFromPosition = receiverPos + dirFromReceiverToDamage * vfxRadiusOfReceiver;
				}

				popupFromPosition.x += GetCurrentStop_Right();
				popupFromPosition.y += PopYInitialOffset;
				popupFromPosition.y += GetCurrentStop_Up();
				popupFromPosition.z += PopZInitialOffset;




				//会计算方向。如果在后
				// popupFromPosition = Vector3.ProjectOnPlane(popupFromPosition,
				// 	receiver.GetRelatedArtHelper().MainCharacterAnimationHelperRef.transform.forward);


				//是否开启了屏幕空间修正
				if (_enableScreenSpaceZDepthFix)
				{
					var receiverPosition = receiver.ReceiveDamage_GetCurrentReceiverPosition();
					if (popupFromPosition.z > receiverPosition.z)
					{
						var vpOfBehaviour = _mainCameraRef.WorldToViewportPoint(receiverPosition);
						var vpOfDamage = _mainCameraRef.WorldToViewportPoint(popupFromPosition);
						var zDiff = Mathf.Abs(vpOfDamage.y - vpOfBehaviour.y);
						popupFromPosition += Vector3.up * (zDiff * _screenSpaceZDepthFixValue);
					}
				}
			}



			return popupFromPosition;
		}

		public void PlaySingleVFX(PerDamageVFXInfoPair perPair, Vector3 pos, Vector3 targetScale)
		{
			var vfx = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(perPair.VFXPrefab);
			vfx.transform.position = pos;
			vfx.transform.localScale = targetScale;
			vfx.Play();

			//
			//
			//
			//
			//
			//
			//
			// var p = perPair.VFXInfoConfig._VFX_GetPSHandle(true, receiver.GetRelatedVFXContainer());
			//
			//
			// p?._VFX__10_PlayThis(true, true);

			// if (perPair.RelatedVFXPrefabArray == null || perPair.RelatedVFXPrefabArray.Length < 1)
			// {
			// 	return;
			// 	// DBug.LogError($"特效信息{perPair.ConfigName}的Prefab数组为空，没有选取到任何特效");
			// }
			//
			//
			//
			//
			// var pickPrefab = perPair.RelatedVFXPrefabArray[UnityEngine.Random.Range(0,
			// 	perPair.RelatedVFXPrefabArray.Length)];
			// var vfx_Obj = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(pickPrefab);
			//
			// switch (perPair.VFXTransformContent)
			// {
			// 	case VFXTransform_JustScaleAnchor vfxTransformJustScaleAnchor:
			// 		(Transform, float) anchor_onlyScale =
			// 			(receiver.GetRelatedVFXContainer()).GetVFXHolderTransformAndRegisterVFX(
			// 				ConSer_VFXHolderInfo._VFXAnchorName_OnlyScale,
			// 				vfx_Obj,
			// 				false);
			// 		vfx_Obj.transform.SetParent(anchor_onlyScale.Item1, false);
			// 		vfx_Obj.transform.localScale = anchor_onlyScale.Item2 * Vector3.one;
			// 		vfx_Obj.Play();
			// 		break;
			// 	case VFXTransform_JustScalePosition:
			// 		(Vector3?, float) pos_onlyScale =
			// 			(receiver.GetRelatedVFXContainer()).GetVFXHolderGlobalPosition(ConSer_VFXHolderInfo
			// 				._VFXAnchorName_OnlyScale);
			// 		vfx_Obj.transform.position = pos_onlyScale.Item1.Value;
			// 		vfx_Obj.transform.localScale = pos_onlyScale.Item2 * Vector3.one;
			// 		vfx_Obj.Play();
			// 		break;
			// 	case VFXTransform_Bone vfxTransformBone:
			//
			//
			// 		foreach (string boneConfigName in vfxTransformBone.BoneConfigNames)
			// 		{
			// 			(Transform, float) bone =
			// 				(receiver.GetRelatedVFXContainer()).GetVFXHolderTransformAndRegisterVFX(boneConfigName,
			// 					vfx_Obj);
			// 			if (bone.Item1 != null)
			// 			{
			// 				vfx_Obj.transform.localScale = bone.Item2 * Vector3.one;
			// 				vfx_Obj.transform.SetParent(bone.Item1, false);
			// 				vfx_Obj.transform.rotation = Quaternion.identity;
			// 				vfx_Obj.Play();
			//
			// 				break;
			// 			}
			// 		}
			// 		break;
			// 	case VFXTransform_BonePosition vfxBonePosition:
			//
			//
			// 		foreach (string boneConfigName in vfxBonePosition.BoneConfigNames)
			// 		{
			// 			(Vector3?, float) pos =
			// 				(receiver.GetRelatedVFXContainer()).GetVFXHolderGlobalPosition(boneConfigName,true);
			// 			if (pos.Item1 != null)
			// 			{
			// 				vfx_Obj.transform.localScale = pos.Item2 * Vector3.one;
			// 				vfx_Obj.transform.position = pos.Item1.Value;
			// 				vfx_Obj.transform.rotation = Quaternion.identity;
			// 				vfx_Obj.Play();
			//
			// 				break;
			// 			}
			// 		}
			// 		break;
			// }
			// if (perPair.PlayInWorldSpace)
			// {
			// 	vfx_Obj.transform.SetParent(VFXPoolManager.Instance.transform, true);
			// }
		}



// #if UNITY_EDITOR
// 		[Button("转移特效配置")]
// 		private void _Convert()
// 		{
// 			var all = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_HitVFXGroupConfig");
// 			foreach (var per in all)
// 			{
// 				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(per);
// 				var so = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_HitVFXGroupConfig>(path);
// 				foreach (var perPair in so.AllDamageVFXInfoList)
// 				{
// 					if (perPair.VFXInfoConfig == null)
// 					{
// 						continue;
// 					}
// 					perPair.VFXID = perPair.VFXInfoConfig._VFX_InfoID;
// 					perPair.VFXPrefab = perPair.VFXInfoConfig.Prefab;
// 				}
// 				UnityEditor.EditorUtility.SetDirty(so);
// 			}
// 		}
// #endif



	}
}