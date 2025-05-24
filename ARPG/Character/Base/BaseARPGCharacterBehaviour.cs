using System;
using System.Collections.Generic;
using ARPG.BattleActivity.Config.EnemySpawnAddon;
using ARPG.Character.Config;
using ARPG.Character.Enemy;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Config;
using ARPG.Manager;
using ARPG.Manager.Config;
using Global;
using Global.ActionBus;
using Global.Setting;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;
using UnityEngine.Serialization;
namespace ARPG.Character.Base
{
	/// <summary>
	/// <para>一个ARPG中的基本角色Behaviour。抽象，玩家和敌人需要另行继承</para>
	/// </summary>
	[TypeInfoBox("BaseARPGBehaviour部分：需要如下结构：\n" + "本体,挂载，作为世界位置，以及【缩放】\n" + "--RotationAnchor：【旋转】锚点\n" +
	             "----FlipAnchor：【翻转】锚点\n" + "------ArtHelper：通用的玩家或敌人ArtHelper，在此之下再挂本体和残影的Spine\n" +
	             "========================\n" +
	             "颜色：\n" +
	             "黄色：投射物碰撞\n" +
	             "红色：吸附范围\n" +
	             "绿色：角色体积挤压")]
	public abstract class BaseARPGCharacterBehaviour : RolePlay_BaseBehaviour, I_RP_Damage_ObjectCanApplyDamage,
		I_RP_Damage_ObjectCanReceiveDamage, I_RP_Projectile_ObjectCanReleaseProjectile
	{
		/// <summary>
		/// <para>碰撞信息</para>
		/// <para>x小于1表示为圆形，小于2表示为等腰梯形，小于3表示为扇形</para>
		/// <para>圆形：y：半径</para>
		/// <para>等腰梯形：y：水平方向长度，z：竖直方向长度，w：竖直倾斜程度</para>
		/// </summary>
		

#region 碰撞信息

		[ShowInInspector,NonSerialized,LabelText("当前预览碰撞信息索引", SdfIconType.Activity),FoldoutGroup("配置/碰撞",true)]
#if UNITY_EDITOR
		[OnValueChanged("ChangePreviewCollisionInfoIndex")]
#endif
		public int PreviewCollisionInfoIndex = 0;


#if UNITY_EDITOR
		private void ChangePreviewCollisionInfoIndex()
		{
			if (PreviewCollisionInfoIndex < 0)
			{
				PreviewCollisionInfoIndex = 0;
			}
			if (PreviewCollisionInfoIndex > 0)
			{
				if (PreviewCollisionInfoIndex > MultiCollisionInfoList.Count)
				{
					PreviewCollisionInfoIndex = MultiCollisionInfoList.Count;
				}
			}
			CurrentActiveCollisionInfoIndex = PreviewCollisionInfoIndex;

		}
		
#endif
		[SerializeField, LabelText("碰撞检测信息"), FoldoutGroup("配置/碰撞", true)]
		public ConSer_CollisionInfo SelfCollisionInfo;

		[SerializeField, LabelText("包含多组碰撞信息"), FoldoutGroup("配置/碰撞", true)]
		public bool ContainMultiCollisionInfo = false;

		[SerializeField, LabelText("多组碰撞信息"), FoldoutGroup("配置/碰撞", true), ShowIf("ContainMultiCollisionInfo")]
		public List<ConSer_CollisionInfo> MultiCollisionInfoList;



		public ConSer_CollisionInfo GetCollisionInfoByIndex(int i = 0)
		{
			if (i == 0)
			{
				return SelfCollisionInfo;
			}
			else
			{
				if (i > MultiCollisionInfoList.Count)
				{
					return null;
				}
				return MultiCollisionInfoList[i - 1];
			}
		}

#endregion


		public void FillCollisionInfoToList(ref List<CollisionCheckInfo_RPBehaviourFull> infoList)
		{
			var currentCollisionInfo = GetCurrentActiveCollisionInfo();
			for (int i = 0; i < currentCollisionInfo.CollisionInfoList.Count; i++)
			{
				SingleCollisionInfo currentInfo = currentCollisionInfo.CollisionInfoList[i];
				CollisionCheckInfo_RPBehaviourFull info = new CollisionCheckInfo_RPBehaviourFull();
				info.RPBehaviourRef = this;
				info.LayerMask = currentCollisionInfo.CollisionLayerMask;
				info.ColliderInfo =
					new float4(currentInfo.ColliderType == ColliderTypeInCollisionInfo.Circle_圆形 ? 0.5f : 1.5f,
						currentInfo.Radius,
						currentInfo.Length,
						currentInfo.InclineRatio);
				info.ColliderOffsetPos = currentInfo.OffsetPos;
				info.ColliderOffsetPos.x = GetIfFlipped() ? -info.ColliderOffsetPos.x : info.ColliderOffsetPos.x;
				var position = transform.position;
				info.FromPos = new float2(position.x, position.z);
				infoList.Add(info);
			}
		}
		
		
		

		public int CurrentActiveCollisionInfoIndex { get; protected set; }

		public void SetCurrentCollisionInfoIndex(int index)
		{
			CurrentActiveCollisionInfoIndex = index;
			if (index > 0)
			{
				if (index > MultiCollisionInfoList.Count)
				{
					CurrentActiveCollisionInfoIndex = 0;
				}
			}
		}

		public ConSer_CollisionInfo GetCurrentActiveCollisionInfo()
		{
			return GetCollisionInfoByIndex(CurrentActiveCollisionInfoIndex);
		}

		
		
		public Vector3 GetCollisionCenter()
		{
			var currentCI = GetCurrentActiveCollisionInfo();
			var offsetPos = currentCI.CollisionInfoList[0].OffsetPos;
			offsetPos.x = GetIfFlipped() ? -offsetPos.x : offsetPos.x;
			var posV3 = transform.position;
			posV3.x += offsetPos.x;
			posV3.z += offsetPos.y;
			return posV3;
		}

		protected static ConSer_MiscSettingInSO _miscSettingRef;

		[ShowInInspector, LabelText("自身碰撞半径_将用于空气墙检测"), FoldoutGroup("运行时/运行参数", false,
			 VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		protected float _selfRBColliderRadius;

		[SerializeField, LabelText("开启对玩家的角色挤压的碰撞等效计算？"), FoldoutGroup("配置")]
		public bool EnableCharacterMovementColliderEquivalent = false;

		[SerializeField, LabelText("挤压等效吸附圆半径乘数")] [FoldoutGroup("配置")]
		[ShowIf( "EnableCharacterMovementColliderEquivalent" )]
		public float EquivalentAbsorbRadiusMul = 1.5f;

		[SerializeField, LabelText("绿色框:HUD高度匹配游戏对象"), FoldoutGroup("配置/HUD内容匹配")]
		public GameObject _HUDHeightMatchGameObject;
		[SerializeField, LabelText("绿色球:元素组匹配位置"), FoldoutGroup("配置/HUD内容匹配")]
		public GameObject _HUDElementGroupMatchTransform;

		[SerializeField, LabelText("弱点条匹配游戏对象"), FoldoutGroup("配置/HUD内容匹配")]
		public GameObject _WeaknessMatchingGameObject;

		[SerializeField, LabelText("生成附加项"), FoldoutGroup("配置", true)]
		private List<SOConfig_EnemySpawnAddonPresetConfig> _spawnAddonPresetConfigList =
			new List<SOConfig_EnemySpawnAddonPresetConfig>();

		[NonSerialized]
		public List<SOConfig_EnemySpawnAddonPresetConfig> RuntimeSpawnAddonPresetConfigList =
			new List<SOConfig_EnemySpawnAddonPresetConfig>();



		[FormerlySerializedAs("DeathHigh"), SerializeField, LabelText("死亡后的高度"), FoldoutGroup("配置")]
		public float DeathHeight = 0f;




		protected static SubGameplayLogicManager_ARPG _glmRef;
		public static void StaticInitialize(SubGameplayLogicManager_ARPG glmRef)
		{
			_glmRef = glmRef;
		}
		/// <summary>
		/// <para>角色的数据是否仍然有效</para>
		/// <para>正在等待清理的角色的数据、初始化未完成的角色的数据  就都是无效的</para>
		/// </summary>
		public bool CharacterDataValid { get; protected set; } = false;

		/// <summary>
		/// <para>参见文档   https://xxi1p77cfp.feishu.cn/wiki/W7fFwVKP2iJnKvk6a7hcbmxcn9f</para>
		/// </summary>
		public class RP_DS_ForceMovementRuntimeInfo
		{
			[ShowInInspector, LabelText("强制位移活跃中？")]
			public bool MovementActive;

			[ShowInInspector, LabelText("作为失衡位移？")]
			public bool AsUnbalanceMovement;

			[ShowInInspector, LabelText("已经过时长")]
			public float ElapsedTime;

			[ShowInInspector, LabelText("剩余时长")]
			public float RemainingTime => MovementTargetDuration - ElapsedTime;


			[ShowInInspector, LabelText("位移起始位置")]
			public Vector3 DisplacementFromPosition;

			[ShowInInspector, LabelText("位移目的地位置")]
			public Vector3 DisplacementTargetPosition;


			[ShowInInspector, LabelText("位移整体向量")]
			public Vector3 DisplacementAllVector;


			[ShowInInspector, LabelText("当前完成度")]
			public float CurrentFinishPartial => Mathf.Clamp01(ElapsedTime / MovementTargetDuration);

			[ShowInInspector, LabelText("本次位移将要进行的时长")]
			public float MovementTargetDuration;

			//reset
			public void Reset()
			{
				MovementActive = false;
				ElapsedTime = 0;
				DisplacementFromPosition = Vector3.zero;
				DisplacementTargetPosition = Vector3.zero;
				MovementTargetDuration = 0;
			}


		}

		/// <summary>
		/// <para>当两帧之间位置差距有多少的时候，才算作有新的朝向被计算出来</para>
		/// </summary>
		protected static readonly float _DirectionChangedPositionThresholdSqr = 0.01f;


#region 动画响应

		/// <summary>
		/// <para>被要求动画。</para>
		/// </summary>
		protected abstract void _ABC_ProcessAnimatorRequirement_OnRequireAnimator(DS_ActionBusArguGroup ds);

#endregion

#region 动画调速

#region 调速

		private List<int> _damageStampRelated = new List<int>();
		/// <summary> 
		/// <para>将要恢复常速的时间点</para>
		/// </summary>
		[ShowInInspector, FoldoutGroup("运行时"), LabelText("将要恢复常速的时间点？")]
		private float _nextRestoreTime;

		[ShowInInspector, FoldoutGroup("运行时"), LabelText("当前记录了速度乘数修正？")]
		private bool _registeredAnimationSpeedMul = false;

		[ShowInInspector, FoldoutGroup("运行时"), LabelText("当前记录的速度乘数修正")]
		protected float _currentAnimationSpeedMul = 1f;



		protected virtual void _ABC_ProcessAnimationSpeedModify_OnCasterModify(DS_ActionBusArguGroup ds)
		{
			PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster pfcComponent =
				ds.ObjectArgu1 as PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster;
			RP_DS_DamageApplyInfo dai = ds.ObjectArgu2 as RP_DS_DamageApplyInfo;
			//如果时间戳有效，且已经存在，则不处理这次调速
			if (dai.DamageTimestamp != 0 && _damageStampRelated.Contains(dai.DamageTimestamp))
			{
				return;
			}
			_damageStampRelated.Add(dai.DamageTimestamp);
			AddAnimationStuckSpeedTime(pfcComponent.SpeedMultiplier, pfcComponent.SpeedModifyDuration);
		}


		protected virtual void _ABC_ProcessAnimationSpeedModify_OnReceiverModify(DS_ActionBusArguGroup ds)
		{
			PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver pfcComponent =
				ds.ObjectArgu1 as PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver;
			RP_DS_DamageApplyInfo dai = ds.ObjectArgu2 as RP_DS_DamageApplyInfo;
			//如果时间戳有效，且已经存在，则不处理这次调速
			if (dai.DamageTimestamp != 0 && _damageStampRelated.Contains(dai.DamageTimestamp))
			{
				return;
			}
			_damageStampRelated.Add(dai.DamageTimestamp);

			float selfMass = GetFloatDataEntryByType(RP_DataEntry_EnumType.MovementMass_重量).CurrentValue;
			if (selfMass > pfcComponent.MaxAvailableMass)
			{
				return;
			}
			float duration = pfcComponent.SpeedModifyDuration * Mathf.Pow((1f - pfcComponent.PerExtraMassPower),
				Mathf.Clamp(selfMass - 1f, 1f, pfcComponent.MaxAvailableMass));
			AddAnimationStuckSpeedTime(pfcComponent.SpeedMultiplier, duration);
		}


		/// <summary>
		/// <para>增加动画“卡帧”的时长。添加是[不短于]的，也就是说添加的时间只会更长不会变短：</para>
		/// <para>e.g.剩余0.3s，试图添加0.1s，则还是0.3s。剩余0.2s,试图添加0.4s，则剩余0.4s</para>
		/// </summary>
		/// <param name="logicSpeedMul"></param>
		/// <param name="duration"></param>
		public void AddAnimationStuckSpeedTime(float logicSpeedMul, float duration)
		{
			float remain = _nextRestoreTime - BaseGameReferenceService.CurrentFixedTime;
			if (remain > duration)
			{
				return;
			}
			_currentAnimationSpeedMul = logicSpeedMul;
			_nextRestoreTime = BaseGameReferenceService.CurrentFixedTime + duration;
			_registeredAnimationSpeedMul = true;
			GetRelatedArtHelper().SetAllAnimationLogicSpeedMul(logicSpeedMul);
		}


		/// <summary>
		/// <para>直接设置,是不注册的。</para>
		/// </summary>
		public void SetAnimationStuckSpeed(float logicSpeedMul)
		{
			AddAnimationStuckSpeedTime(logicSpeedMul, float.MaxValue);

			_registeredAnimationSpeedMul = false;
			_currentAnimationSpeedMul = logicSpeedMul;
		}

		/// <summary>
		/// <para>当死亡时，重置速度乘数</para>
		/// </summary>
		/// <param name="ds"></param>
		protected virtual void _ABC_ResetAnimationSpeedMulOnDie(DS_ActionBusArguGroup ds)
		{
			_registeredAnimationSpeedMul = false;
			_currentAnimationSpeedMul = 1f;
		}

#endregion

#endregion




		public override void InitializeOnInstantiate()
		{
			base.InitializeOnInstantiate();
			CurrentActiveCollisionInfoIndex = 0;
			var selfCollider = GetComponent<Collider>();
			if (selfCollider is SphereCollider sc)
			{
				_selfRBColliderRadius = sc.radius;
			}
			else if (selfCollider is CapsuleCollider cc)
			{
				_selfRBColliderRadius = cc.radius;
			}


			_selfActionBusInstance.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnRequiredAnimationLogicSpeedMulToCaster_动画要求逻辑速度倍率对发起者,
				_ABC_ProcessAnimationSpeedModify_OnCasterModify);
			_selfActionBusInstance.RegisterAction(
				ActionBus_ActionTypeEnum.L_AnimationHelper_OnRequiredAnimationLogicSpeedMulToReceiver_动画要求逻辑速度倍率对接收者,
				_ABC_ProcessAnimationSpeedModify_OnReceiverModify);
			_selfActionBusInstance.RegisterAction(ActionBus_ActionTypeEnum.L_ARPGCharacterRequireAnimation_ARPG角色要求动画,
				_ABC_ProcessAnimatorRequirement_OnRequireAnimator,
				-999);
			_selfActionBusInstance.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_OnFinallyDeath_伤害流程最终死亡,
				_ABC_ResetAnimationSpeedMulOnDie);
			//注册所有的




			if (this is PlayerARPGConcreteCharacterBehaviour)
			{
				SelfCollisionInfo.CollisionLayerMask = 1 << 1;
				if (MultiCollisionInfoList != null)
				{
					foreach (var perInfo in MultiCollisionInfoList)
					{
						perInfo.CollisionLayerMask = 1 << 1;
					}
				}
			}
			else if (this is EnemyARPGCharacterBehaviour)
			{
				SelfCollisionInfo.CollisionLayerMask = 1;
				if (MultiCollisionInfoList != null)
				{
					foreach (var perInfo in MultiCollisionInfoList)
					{
						perInfo.CollisionLayerMask = 1;
					}
				}
			}
		}

		/// <summary>
		/// <para>引用生成附加项。需要在初始化具体的DataModel之后。所以需要子类自行调用</para>
		/// </summary>
		protected virtual void ApplySpawnAddonConfig()
		{
			for (int i = 0; i < _spawnAddonPresetConfigList.Count; i++)
			{
				SOConfig_EnemySpawnAddonPresetConfig perPreset = _spawnAddonPresetConfigList[i];
				if (perPreset == null)
				{
					continue;
				}
				var configRuntime = UnityEngine.Object.Instantiate(perPreset);
				RuntimeSpawnAddonPresetConfigList.Add(configRuntime);
				ApplyAddonAfterSpawn(configRuntime.AddonList_Serialize, transform.position);
			}
		}


		/// <summary>
		/// <para>在生成结束后，应用附加业务。这通常来源于EnemySpawnService，会在CharacterOnMap生成角色之后调用。</para>
		/// </summary>
		public virtual void ApplyAddonAfterSpawn(List<BaseEnemySpawnAddon> addons, Vector3 spawnPos)
		{
			foreach (BaseEnemySpawnAddon perAddon in addons)
			{
				switch (perAddon)
				{
					case ESA_附加Buff_AddBuff esa附加BuffAddBuff:
						foreach (ESA_附加Buff_AddBuff.PerAddonInfo perAddonInfo in esa附加BuffAddBuff.GetAddBuffInfoList())
						{
							BaseRPBuff targetBuff = null;
							if (perAddonInfo.ApplyAsDirectMode)
							{
								targetBuff = GetSelfRolePlayDataModel().SelfBuffHolderInstance.TryApplyBuff_DirectMode(
									perAddonInfo.BuffID,
									perAddonInfo.ContainBLP ? perAddonInfo.GetBLPList_RuntimeFinal() : null,
									this);
							}
							else
							{
								var appR = GetSelfRolePlayDataModel().SelfBuffHolderInstance.TryApplyBuff(
									perAddonInfo.BuffID,
									this,
									this,
									perAddonInfo.ContainBLP ? perAddonInfo.GetBLPList_RuntimeFinal() : null);
								if (appR.Item1 == BuffApplyResultEnum.Success ||
								    appR.Item1 == BuffApplyResultEnum.AlreadyExistsAndRefresh)
								{
									targetBuff = appR.Item2;
								}
							}
							if (perAddonInfo.ContainDurationOverride)
							{
								targetBuff.ResetExistDurationAs(perAddonInfo.OverrideExistDuration);
							}
							if (perAddonInfo.ContainAvailableTimeOverride)
							{
								targetBuff.ResetAvailableTimeAs(perAddonInfo.OverrideAvailableTime);
							}

						}
						break;
					case ESA_移除Buff_RemoveBuff esa移除BuffRemoveBuff:
						foreach (ESA_移除Buff_RemoveBuff.PerRemoveInfo perRemoveInfo in esa移除BuffRemoveBuff
							.AddBuffInfoList)
						{
							GetSelfRolePlayDataModel().SelfBuffHolderInstance.TryRemoveBuff(perRemoveInfo.BuffID);
						}
						break;
					case EAS_修改数据项_ModifyDataEntryValue modifyDataEntryValue:
						foreach (EAS_修改数据项_ModifyDataEntryValue.DataEntryModifyInfo perModifyInfo in
							modifyDataEntryValue.ModifyInfosList)
						{
							Float_RPDataEntry targetEntry =
								GetSelfRolePlayDataModel().GetFloatDataEntry(perModifyInfo.Type);
							if (targetEntry is FloatPresentValue_RPDataEntry fpv)
							{
								if (perModifyInfo.OverrideOrModify)
								{
									fpv.ResetDataToValue(perModifyInfo.ModifyValue);
								}
								else
								{
									fpv.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(
										perModifyInfo.ModifyValue,
										RPDM_DataEntry_ModifyFrom.Initialize_LoadWithTemplate_加载,
										perModifyInfo.CalculatePosition,
										this));
								}
							}
							else
							{

								if (perModifyInfo.OverrideOrModify)
								{
									targetEntry.ReplaceOriginalValue(perModifyInfo.ModifyValue);
								}
								else
								{
									targetEntry.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(
										perModifyInfo.ModifyValue,
										RPDM_DataEntry_ModifyFrom.Initialize_LoadWithTemplate_加载,
										perModifyInfo.CalculatePosition,
										this));
								}
							}
						}
						break;

					case ESA_生成一个特效_GenerateVFXOnSpawn spawnVFX:
						if (spawnVFX._prefab_VFXPrefab == null)
						{
							break;
						}
						var pp = UnityEngine.Object.Instantiate(spawnVFX._prefab_VFXPrefab, transform);
						pp.transform.localPosition = Vector3.up * spawnVFX._SpawnHeight;
						pp.transform.localScale = Vector3.one * spawnVFX._SpawnScale;
						break;

				}
			}

		}




		/// <summary>
		/// <para>根据SOFE的数据内容来初始化自身</para>
		/// <para>·：初始化BaseRPGEntry数据</para>
		/// </summary>
		public virtual void InitializeByConfig(
			SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE entry,
			SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry gameplayDataEntry,
			SOFE_CharacterResourceInfo.PerTypeInfo resourceEntry,
			Vector3 spawnPos)
		{
			GetSelfRolePlayArtHelper().SetRelatedSheetGroupIndex(resourceEntry.CharacterIndex);

			SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry baseRPGEntryConfig =
				gameplayDataEntry;


			var _selfDataModelInstance = GetSelfRolePlayDataModel();
            //初始化BaseRPGEntry数据
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.CharacterLevel, baseRPGEntryConfig.Level);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.HPMax_最大HP, baseRPGEntryConfig.HPMax);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.SPMax_最大SP, baseRPGEntryConfig.SPMax);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.HPSRegen_每秒HP回复, 0);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.SPSRegen_每秒SP回复, 0);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.AttackPower_攻击力, baseRPGEntryConfig.AttackPower);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.Defense_防御力, baseRPGEntryConfig.Defense);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.AttackSpeed_攻击速度, baseRPGEntryConfig.AttackSpeed);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.SkillAccelerate_技能加速, baseRPGEntryConfig.SkillAccelerate);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.Toughness_韧性, baseRPGEntryConfig.Toughness);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.CriticalRate_暴击率, baseRPGEntryConfig.CriticalRate);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.CriticalBonus_暴击伤害, baseRPGEntryConfig.CriticalBonus);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.Accuracy_命中率, baseRPGEntryConfig.Accuracy);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.DodgeRate_闪避率, baseRPGEntryConfig.Dodge);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.M_Strength_主力量, baseRPGEntryConfig.M_Strength);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.M_Dexterity_主敏捷, baseRPGEntryConfig.M_Dexterity);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.M_Vitality_主体质, baseRPGEntryConfig.M_Vitality);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.M_Spirit_主精神, baseRPGEntryConfig.M_Spirit);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.M_Intellect_主智力, baseRPGEntryConfig.M_Intelligence);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.M_Charm_主魅力, baseRPGEntryConfig.M_Charming);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.MoveSpeed_移速, baseRPGEntryConfig.MoveSpeed).SetLowerBound(0f);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.MovementMass_重量, baseRPGEntryConfig.MassWeight);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.SkillCDReduce_技能CD缩减, 0f);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.CorpseDuration_尸体存在时间, baseRPGEntryConfig.CorpseDuration);
            InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType.OverDamageOverride_过伤覆盖时长, baseRPGEntryConfig.OverdamageOverride);
            _selfDataModelInstance.SelfBuffHolderInstance.TryApplyBuff_DirectMode(
				RolePlay_BuffTypeEnum.ChangeCommonDamageType_常规伤害类型更改, this);

			if (baseRPGEntryConfig.BuffTypeList != null)
			{
				foreach (RolePlay_BuffTypeEnum perBuff in baseRPGEntryConfig.BuffTypeList)
				{
					var blp_applyFromExcel = GenericPool<BLP_ApplyFromConfig_由表中直接施加>.Get();
					_selfDataModelInstance.SelfBuffHolderInstance.TryApplyBuff(perBuff, this, this, blp_applyFromExcel);
					blp_applyFromExcel.ReleaseOnReturnToPool();
				}
			}


			_miscSettingRef = GlobalConfigurationAssetHolderHelper.Instance.MiscSetting_Runtime.SettingContent;
			CharacterDataValid = true;
		}


		public override void SetCharacterVariantScale(float scale)
		{
			base.SetCharacterVariantScale(scale);
			GetRelatedArtHelper().SetCharacterLocalScale(SelfCharacterVariantScale *
			                                             _glmRef.CharacterOnMapManagerReference
				                                             .CurrentCharacterScaleInScene);

		}




#region Tick

		public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.UpdateTick(currentTime, currentFrameCount, delta);

			UpdateRecordPosition_Instant(transform.position);
		}


		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (_registeredAnimationSpeedMul && currentTime > _nextRestoreTime)
			{
				_registeredAnimationSpeedMul = false;
				_currentAnimationSpeedMul = 1f;
				GetRelatedArtHelper().SetAllAnimationLogicSpeedMul(1f);

			}
		}

#endregion
		public PlayableDirector GetRelatedPlayableDirector()
		{
			return (GetSelfRolePlayArtHelper() as BaseARPGArtHelper).SelfPlayableDirector;
		}





		public virtual void ClearBeforeDestroy()
		{
		}

		public FloatPresentValue_RPDataEntry ApplyDamage_GetRelatedPresentValue(RP_DataEntry_EnumType type)
		{
			return GetSelfRolePlayDataModel().GetFloatPresentValue(type);
		}
		public LocalActionBus ApplyDamage_GetLocalActionBus()
		{
			return _selfActionBusInstance;
		}
		public bool CasterDataEntryValid()
		{
			return CharacterDataValid;
		}
		public Vector3? GetDamageCasterPosition()
		{
			return GetCollisionCenter();
		}

		public override Vector3 GetCasterForwardDirection()
		{
			if (GetRelatedArtHelper().CurrentFaceLeft)
			{
				return BaseGameReferenceService.CurrentBattleLogicLeftDirection;
			}
			else
			{
				return BaseGameReferenceService.CurrentBattleLogicRightDirection;
			}
		}

		public bool GetIfFlipped()
		{
			if (_selfBaseArtHelperRef)
			{
				return _selfBaseArtHelperRef.GetIfFlipped();
			}
			else
			{
				return GetRelatedArtHelper().GetIfFlipped();
			}
		}

		public override Vector3 ReceiveDamage_GetCurrentReceiverPosition()
		{
			return GetCollisionCenter();
		}

		public Vector3 ReceiveDamage_GetCurrentReceiverFaceDirection()
		{
			return _selfBaseArtHelperRef.CurrentFaceLeft ? BaseGameReferenceService.CurrentBattleLogicLeftDirection :
				BaseGameReferenceService.CurrentBattleLogicRightDirection;
		}
		public RP_DS_DamageApplyResult ReceiveDamage_ReceiveFromRPDS(RP_DS_DamageApplyInfo applyInfo, int effectIDStamp)
		{
			return GetSelfRolePlayDataModel().ReceiveDamage(applyInfo, effectIDStamp);
		}



		public FloatPresentValue_RPDataEntry ReceiveDamage_GetRelatedPresentValue(RP_DataEntry_EnumType type)
		{
			return GetSelfRolePlayDataModel().GetFloatPresentValue(type);
		}
		public bool ReceiveDamage_IfDataValid()
		{
			return CharacterDataValid;
		}
		public BaseARPGArtHelper GetRelatedArtHelper()
		{
			return GetSelfRolePlayArtHelper() as BaseARPGArtHelper;
		}

		protected override Vector3? GetAlignedPosition(Vector3 fromPosition, float alignHeight)
		{
			return _glmRef.GetAlignedTerrainPosition(fromPosition, alignHeight);
		}

		protected override float GetMovementLogicRadius()
		{
			return _selfRBColliderRadius;
		}

#if UNITY_EDITOR
		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();

			if (_HUDHeightMatchGameObject == null)
			{
				return;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(_HUDHeightMatchGameObject.transform.position + Vector3.up * 0.5f,
				new Vector3(4f, 1f, 0.5f));
			if (GetCurrentActiveCollisionInfo() != null)
			{
				var currentActiveCollisionInfo = GetCurrentActiveCollisionInfo();
				foreach (SingleCollisionInfo perSingleInfo in currentActiveCollisionInfo.CollisionInfoList)
				{

					Draw(perSingleInfo);

				}

				void Draw(SingleCollisionInfo perSingleInfo)
				{
					var offsetPos = perSingleInfo.OffsetPos;
					offsetPos.x = GetIfFlipped() ? -offsetPos.x : offsetPos.x;
					Vector3 drawPos = transform.position;
					drawPos.x += offsetPos.x;
					drawPos.z += offsetPos.y;
					Gizmos.color = Color.red;
					float absorbRadius = perSingleInfo.ColliderType == ColliderTypeInCollisionInfo.Box_等腰梯形
						? perSingleInfo.Radius / 2f : perSingleInfo.Radius;
					Gizmos.DrawWireSphere(drawPos, absorbRadius);

					if (EnableCharacterMovementColliderEquivalent)
					{
						var equivalentAbsorbRadius = absorbRadius * EquivalentAbsorbRadiusMul;
						Gizmos.color = Color.green;
						Gizmos.DrawWireSphere(drawPos, equivalentAbsorbRadius);
					}

					Gizmos.color = Color.yellow;
					if (perSingleInfo.ColliderType == ColliderTypeInCollisionInfo.Circle_圆形)
					{
						Gizmos.DrawWireSphere(drawPos, perSingleInfo.Radius);
					}
					else if (perSingleInfo.ColliderType == ColliderTypeInCollisionInfo.Box_等腰梯形)
					{
						Vector3 centerPos = transform.position;
						centerPos.x += offsetPos.x;
						centerPos.z += offsetPos.y;
						Vector3 point0 = GetInclinedPoint(0,
							centerPos,
							new Vector2(perSingleInfo.Radius, perSingleInfo.Length),
							perSingleInfo.InclineRatio);
						Vector3 point1 = GetInclinedPoint(1,
							centerPos,
							new Vector2(perSingleInfo.Radius, perSingleInfo.Length),
							perSingleInfo.InclineRatio);
						Vector3 point2 = GetInclinedPoint(2,
							centerPos,
							new Vector2(perSingleInfo.Radius, perSingleInfo.Length),
							perSingleInfo.InclineRatio);
						Vector3 point3 = GetInclinedPoint(3,
							centerPos,
							new Vector2(perSingleInfo.Radius, perSingleInfo.Length),
							perSingleInfo.InclineRatio);
						Gizmos.color = Color.yellow;
						Gizmos.DrawLine(point0, point1);

						Gizmos.DrawLine(point1, point2);
						Gizmos.DrawLine(point2, point3);
						Gizmos.DrawLine(point3, point0);
					}

				}

				Vector3 GetInclinedPoint(int index, Vector3 center, Vector2 size, float ratio)
				{
					switch (index)
					{
						//左上
						case 0:
							return new Vector3(center.x - size.x / 2f + size.y * ratio,
								center.y,
								center.z + size.y / 2f);
						//右上
						case 1:
							return new Vector3(center.x + size.x / 2f - size.y * ratio,
								center.y,
								center.z + size.y / 2f);
						//右下
						case 2:
							return new Vector3(center.x + size.x / 2f, center.y, center.z - size.y / 2f);
						//左下
						case 3:
							return new Vector3(center.x - size.x / 2f, center.y, center.z - size.y / 2f);
					}
					return Vector3.back;
				}
			}
	
	
			if (GetRelatedArtHelper() != null)
			{

				if (_HUDElementGroupMatchTransform != null)
				{
					Gizmos.DrawWireSphere(_HUDElementGroupMatchTransform.transform.position, 0.77f);
				}
				else
				{
					var size = GetRelatedArtHelper()._VFXScaleRadius;
					float h = size / 2f;
					var v3 = new Vector3(-h * 1.5f, -h * 1.5f, 0f);
					Gizmos.DrawWireSphere(_HUDHeightMatchGameObject.transform.position+v3, 0.77f);
				}
			}
		}
#endif
		public virtual I_RP_ContainVFXContainer GetRelatedVFXContainer()
		{
			return GetRelatedArtHelper() as I_RP_ContainVFXContainer;
		}

		public override bool CurrentDataValid()
		{
			return CharacterDataValid;
		}



#if UNITY_EDITOR

#if UNITY_EDITOR
		[Button("本地化：角色名字", ButtonSizes.Large, Icon = SdfIconType.Translate), PropertyOrder(100),]
		[HorizontalGroup("1")]
		public void Translate_CharacterName()
		{
			var table = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Localization.LocalizationTableCollection>(
				"Assets/Localization/StringTables/CharacterName_角色名字.asset");
			UnityEditor.Localization.UI.LocalizationTablesWindow.ShowWindow(table);
		}
		[Button("表格：角色资源表", ButtonSizes.Large, Icon = SdfIconType.FileExcel), PropertyOrder(100),]
		[HorizontalGroup("2")]
		public void Excel_CharacterResource()
		{
			UnityEditor.AssetDatabase.OpenAsset(
				UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(
					"Assets/ExcelData/RPG/角色资源索引.xlsx"));
		}
		[Button("转表：角色资源", ButtonSizes.Large, Icon = SdfIconType.Circle), PropertyOrder(100),]
		[HorizontalGroup("2")]
		public void Convert_CharacterResource()
		{
			var asset  = UnityEditor.AssetDatabase.LoadAssetAtPath<SO_Conversion>("Assets/ExcelData/角色_1_1_角色资源.asset");
			asset.ProcessConvert();
		}

		[Button("表格：RPG初始化配置", ButtonSizes.Large, Icon = SdfIconType.FileExcel), PropertyOrder(100),]
		[HorizontalGroup("3")]
		public void Excel_RPGData()
		{
			UnityEditor.AssetDatabase.OpenAsset(
				UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(
					"Assets/ExcelData/RPG/RPG项初始化配置.xlsx"));
		}
		
		
		[Button("转表：角色基本配置", ButtonSizes.Large, Icon = SdfIconType.Circle), PropertyOrder(100),]
		[HorizontalGroup("3")]
		public void Convert_CharacterBase()
		{
			var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SO_Conversion>("Assets/ExcelData/角色_0_0_角色基本配置项.asset");
			asset.ProcessConvert();
		}

		[Button("转表：RPG初始化配置", ButtonSizes.Large, Icon = SdfIconType.Circle), PropertyOrder(100),]
		[HorizontalGroup("3")]
		public void Convert_CharacterRPG()
		{
			var asset =
				UnityEditor.AssetDatabase.LoadAssetAtPath<SO_Conversion>("Assets/ExcelData/角色_0_1_角色RPG数值.asset");
			asset.ProcessConvert();
		}
#endif
		
#endif




// #if UNITY_EDITOR
// 		[Button("转移碰撞数据")]
// 		private void _ConvertAllCollisionInfo()
// 		{
// 			//load all prefab contains BaseARPGCharacterBehaviour
// 			var guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
// 			foreach (var guid in guids)
// 			{
// 				 string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
// 				 var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
// 				 var b = prefab.GetComponent<BaseARPGCharacterBehaviour>();
// 				 if (b == null)
// 				 {
// 					 continue;
// 				 }
// 				  
// 				 b.SelfCollisionInfo.CollisionInfoList = new List<SingleCollisionInfo>();
// 				 b.SelfCollisionInfo.CollisionInfoList.Add(new SingleCollisionInfo
// 				 {
// 					 ColliderType = b.SelfCollisionInfo.ColliderType,
// 					 Radius = b.SelfCollisionInfo.Radius,
// 					 InclineRatio = b.SelfCollisionInfo.InclineRatio,
// 					 Length = b.SelfCollisionInfo.Length,
// 					 OffsetPos = b.SelfCollisionInfo.OffsetPos
// 				 });
// 				 if (b.ContainMultiCollisionInfo)
// 				 {
// 					 foreach (var perInfo in b.MultiCollisionInfoList)
// 					 {
// 						 perInfo.CollisionInfoList = new List<SingleCollisionInfo>();
// 						 perInfo.CollisionInfoList.Add(new SingleCollisionInfo
// 						 {
// 							 ColliderType = perInfo.ColliderType,
// 							 Radius = perInfo.Radius,
// 							 InclineRatio = perInfo.InclineRatio,
// 							 Length = perInfo.Length,
// 							 OffsetPos = perInfo.OffsetPos
// 						 });
// 					 }
// 				 }
// 				 UnityEditor.EditorUtility.SetDirty(prefab);
//
// 			}
// 			 //set dirty
//
//
// 		}
// #endif
	}
}