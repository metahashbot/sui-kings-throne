using System;
using System.Collections.Generic;
using ARPG.BattleActivity.Config.EnemySpawnAddon;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Config;
using ARPG.Character.Enemy.AI;
using ARPG.Character.Enemy.AI.Decision;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Config;
using ARPG.Manager;
using ARPG.Manager.Component;
using ARPG.Manager.Config;
using ARPG.UI.Panel;
using DG.Tweening;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.AreaFunctionHandler;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ARPG.Character.Enemy
{
	public class EnemyARPGCharacterBehaviour : BaseARPGCharacterBehaviour
	{
#if UNITY_EDITOR

		// redraw constantly
		[OnInspectorGUI]
		private void RedrawConstantly()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
		}

#endif

		[SerializeField, LabelText("配置_敌人头像"), FoldoutGroup("配置/艺术表现"), Required]
		public Sprite _Icon;
		

		[SerializeField, Required, LabelText("ArtHelper"), FoldoutGroup("配置/艺术表现")]
		protected EnemyARPGArtHelper _selfArtHelper;

		[ShowInInspector, LabelText("数据模型"), FoldoutGroup("运行时", true)]
		protected ARPGEnemyDataModel _selfDataModelInstance;


		[ShowInInspector, LabelText("正在使用的AIBrain运行时实例"), FoldoutGroup("运行时", true)]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		protected SOConfig_AIBrain SelfAIBrainRuntimeInstance;

		public SOConfig_AIBrain GetAIBrainRuntimeInstance()
		{
			return SelfAIBrainRuntimeInstance;
		}

#region 接口和基类实现



		protected override RolePlay_DataModelBase GetSelfRolePlayDataModel()
		{
			return _selfDataModelInstance;
		}

		public override RolePlay_ArtHelperBase GetSelfRolePlayArtHelper()
		{
			return _selfArtHelper;
		}


#endregion

#region 生成  |  初始化  | 设置

		public override void InitializeOnInstantiate()
		{
			base.InitializeOnInstantiate();
			_selfDataModelInstance = new ARPGEnemyDataModel(this);
			_selfArtHelper.InitializeOnInstantiate(_selfActionBusInstance);
			_selfArtHelper.InjectBaseRPBehaviourRef(this);
			
			ApplySpawnAddonConfig();



			_selfActionBusInstance.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_OnFinallyDeath_伤害流程最终死亡,
				_ABC_OnFinallyDead_OnHpReducedTo0);
			_selfActionBusInstance.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_RequireDirectSelfExplosion_要求直接自爆,
				_ABC_OnDirectSelfExplosion_OnRequireSelfExplosion);
		}


		private string _databaseID;
		
		public override void InitializeByConfig(
			SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE entry,
			SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry gameplayDataEntry,
			SOFE_CharacterResourceInfo.PerTypeInfo resourceEntry,
			Vector3 spawnPos)
		{
			base.InitializeByConfig(entry, gameplayDataEntry, resourceEntry, spawnPos);
			_databaseID = entry.Name;

            //设置掉落经验
            _selfDataModelInstance.SelfDataEntry_Database.InitializeFloatDataEntry(
				RP_DataEntry_EnumType.DropExp, gameplayDataEntry.DropExp);

            //初始化AIBrain
            string aiBrainUID = entry.AIBrainID;
			SOConfig_AIBrain relatedRawAIBrain =
				GlobalConfigurationAssetHolderHelper.Instance.Collection_AIBrainCollection.GetConfigByID(aiBrainUID);
			if (relatedRawAIBrain == null)
			{
				if ((int)_selfBehaviourNamedType < 601000)
				{
					//这东西就是没AI，不管
					return;
				}
#if UNITY_EDITOR
				DBug.LogError(
					$"游戏角色{gameObject.name} | 配置{entry.Name}在初始化时，试图使用ID为{entry.AIBrainID}的AIBrain配置，但是并没有找到，所以它没有AI被激活");
#endif
				return;
			}
			else
			{
				SelfAIBrainRuntimeInstance = UnityEngine.Object.Instantiate(relatedRawAIBrain);
				SelfAIBrainRuntimeInstance.BrainHandlerFunction.Initialize(this,
					relatedRawAIBrain,
					SelfAIBrainRuntimeInstance);
			}



			//为敌人增加通用的伤害微调
			if (_selfBehaviourNamedType != CharacterNamedTypeEnum.Utility_VoidEntity_空实体)
			{
				if (ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人) !=
				    BuffAvailableType.NotExist)

				{
					ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.DamageTweak_NormalEnemy_普通敌人伤害微调, this, this);

				}
				else
				{
					ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.DamageTweak_EliteEnemy_精英敌人伤害微调, this, this);

				}
			}


		}




		public override void ApplyAddonAfterSpawn(List<BaseEnemySpawnAddon> addons, Vector3 spawnPos)
		{
			base.ApplyAddonAfterSpawn(addons, spawnPos);
			for (int i = 0; i < addons.Count; i++)
			{
				var perAddon = addons[i];
				switch (perAddon)
				{
					case ESA_调整AI行为模式_SwitchAIBehaviourPattern switchAIBehaviourPattern:
						if (string.IsNullOrEmpty(switchAIBehaviourPattern.TargetBehaviourPatternID))
						{
							break;
						}
						if (SelfAIBrainRuntimeInstance.BrainHandlerFunction.CurrentActiveBehaviourPattern == null)
						{
							SelfAIBrainRuntimeInstance.BrainHandlerFunction.SwitchBehaviourPattern(
								switchAIBehaviourPattern.TargetBehaviourPatternID);
							break;
						}
						else if (!switchAIBehaviourPattern.SwitchOnSame &&
						         SelfAIBrainRuntimeInstance.BrainHandlerFunction.CurrentActiveBehaviourPattern
							         .BehaviourPatternTypeID.Equals(switchAIBehaviourPattern.TargetBehaviourPatternID,
								         StringComparison.OrdinalIgnoreCase))
						{
							break;
						}
						SelfAIBrainRuntimeInstance.BrainHandlerFunction.SwitchBehaviourPattern(switchAIBehaviourPattern
							.TargetBehaviourPatternID);
						break;
					case EAS_增加全局监听_AddAIListenConfigToBrain addListen:
						if (addListen.ToAddAIListenConfig == null)
						{
							break;
						}

						if (SelfAIBrainRuntimeInstance == null ||
						    !SelfAIBrainRuntimeInstance.BrainHandlerFunction.CurrentBrainActive)
						{
							break;
						}

						SelfAIBrainRuntimeInstance.BrainHandlerFunction.AddNewSingleAIListen(addListen
							.ToAddAIListenConfig);
						break;
					case ESA_调整HUD内容_ModifySelfHUD hud:
						if (SelfRelatedHUD == null)
						{
							DBug.LogWarning($" {name} 没有HUD，无法调整HUD内容");
							break;
						}
						if (hud._EnableText)
						{
							SelfRelatedHUD.ShowTextAs(hud);
						}


						break;
					case ESA_进行巡逻_EnterAsPatrol patrol:
						if (patrol.HasBehaviourPatternSwitch)
						{
							if (SelfAIBrainRuntimeInstance == null ||
							    !SelfAIBrainRuntimeInstance.BrainHandlerFunction.CurrentBrainActive)
							{
								break;
							}

							SelfAIBrainRuntimeInstance.BrainHandlerFunction.SwitchBehaviourPattern(
								patrol.TargetBehaviourPatternID);
						}
						if (SelfAIBrainRuntimeInstance.BrainHandlerFunction
							.TryGetDecisionRuntimeInstanceByDecisionHandlerInsideCurrentBehaviourPattern(
								out DH_在点位间巡逻_Patrol patrolDecision))
						{
							patrolDecision.ReceivePatrolInfo(patrol.PatrolInfoPairsList);
						}



						break;
				}
			}
		}



		[ShowInInspector, LabelText("关联的生成配置，为空就表明不是正常生成的"), FoldoutGroup("运行时", true)]
		public SOConfig_SpawnEnemyConfig RelatedSpawnConfigInstance { get; protected set; } = null;

		[ShowInInspector, LabelText("关联的生成配置中具体的那份实例"), FoldoutGroup("运行时", true), ReadOnly]
		public EnemySpawnService_SubActivityService.SingleSpawnInfo RelatedSpawnConfigInstance_SingleSpawnInfo
		{
			get;
			protected set;
		} = null;

		[ShowInInspector, LabelText("关联的生成配置中具体的那份实例的生成点信息"), FoldoutGroup("运行时", true), ReadOnly]
		public EnemySpawnPositionRuntimeInfo
			RelatedSpawnConfigInstance_SpawnPositionInfo { get; protected set; } = null;

		public string RelatedAreaID { get; set; }

		public void SetSpawnConfigRef(
			SOConfig_SpawnEnemyConfig config,
			EnemySpawnService_SubActivityService.SingleSpawnInfo singleInfo,
			EnemySpawnPositionRuntimeInfo spawnPositionInfo)
		{
			RelatedSpawnConfigInstance = config;
			RelatedSpawnConfigInstance_SingleSpawnInfo = singleInfo;
			RelatedSpawnConfigInstance_SpawnPositionInfo = spawnPositionInfo;
			RelatedAreaID = config.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID;
        }



#endregion

#region Tick

		public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.UpdateTick(currentTime, currentFrameCount, delta);
			if (SelfAIBrainRuntimeInstance)
			{
				SelfAIBrainRuntimeInstance.BrainHandlerFunction.UpdateTick(currentTime,
					currentFrameCount,
					delta * _currentAnimationSpeedMul);
			}
		}


		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			_selfArtHelper.FixedUpdateTick(currentTime, currentFrameCount, delta * _currentAnimationSpeedMul);
			if (SelfAIBrainRuntimeInstance)
			{
				SelfAIBrainRuntimeInstance.BrainHandlerFunction.FixedUpdateTick(currentTime,
					currentFrameCount,
					delta * _currentAnimationSpeedMul);
			}

		}



#endregion


#region 死亡 | 销毁 |  数据无效

#region 自爆



		/// <summary>
		/// <para>直接要求自爆。会跳过常规伤害流程中的一些步骤</para>
		/// </summary>
		protected virtual void _ABC_OnDirectSelfExplosion_OnRequireSelfExplosion(DS_ActionBusArguGroup ds)
		{
			_selfDataModelInstance.GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP).ResetDataToValue(-1f);


			CommonDeath();
			var ds_deathDirect =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体);
			ds_deathDirect.ObjectArgu1 = this;

			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_deathDirect);


		}

#endregion



#region 前序死亡流程

/*
		//
		// 已记录死亡时，会锁1血。并且硬直。在此期间依然接受伤害(并改写跳字)，仍然会额外硬直(递减)，每次受击都会再次硬直，</para>
		//直到一定时间没有再次受击，此时内部改写血量并FinallyDeath</para>
		
		*/

		
#endregion
		
		

		/// <summary>
		/// <para>伤害流程死亡 并不等于 销毁。这里只是向其他地方广播死亡。通常还会有一个变成尸体的过程，</para>
		/// <para>并且还会消弹、清弹等等。尸体消失了才是真的完全销毁。</para>
		/// </summary>
		protected override void _ABC_OnFinallyDead_OnHpReducedTo0(DS_ActionBusArguGroup rpds)
		{
			//死亡功能
			//死亡动画
			//一般来说死亡动画的配置文件都在AIBrain里面有，如果没有再另说
			//直接要求执行决策 通用死亡，决策名一般都是"通用普通死亡"，如果没有再手动从Brain查找死亡动画，如果还没有再报错
			bool exeInBrain = false, exeInAni = false;
			
			if (SelfAIBrainRuntimeInstance != null)
			{
				//拿一下ObjStr上的RPDS_DAR
				//如果是空的，则表明是正常死亡
				if (rpds.ObjectArguStr is RP_DS_DamageApplyResult rpds_dar && rpds_dar.CauseOverloadDamageEffect)
				{
					SOConfig_AIDecision decision_overload = SelfAIBrainRuntimeInstance.BrainHandlerFunction
						.FindSelfDecisionByString("通用过伤死亡");
					//并且试图播放过伤特效
					_PlayOverloadDamageVFX(transform.position, _selfArtHelper._VFXScaleRadius, rpds_dar);
					if (decision_overload != null)
					{
						SelfAIBrainRuntimeInstance.BrainHandlerFunction.AddDecisionToQueue(decision_overload,
							BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首,
							true);
						exeInBrain = true;
						_selfDataModelInstance.GetFloatDataEntry(RP_DataEntry_EnumType.CorpseDuration_尸体存在时间)
							.ResetDataToValue(
								(SelfAIBrainRuntimeInstance.BrainHandlerFunction.GetRelatedAnimationInfo(
										(decision_overload.DecisionHandler as DH_通用常规死亡_CommonDeath)._an_Death) as
									SheetAnimationInfo_帧动画配置).ClipTransitionRef.Clip.length);
					}
					if (!exeInBrain)
					{
						var decision =
							SelfAIBrainRuntimeInstance.BrainHandlerFunction.FindSelfDecisionByString("通用普通死亡");
						//如果此处检查到配置中，不需要覆盖过伤时长覆盖（常见于Boss等情况），则不会覆盖实体存在时间。
						if (_selfDataModelInstance.GetFloatDataEntry(RP_DataEntry_EnumType.OverDamageOverride_过伤覆盖时长).GetCurrentValue() > float.Epsilon)
                        {
                            if (decision != null)
                            {
                                SelfAIBrainRuntimeInstance.BrainHandlerFunction.AddDecisionToQueue(decision,
                                    BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首,
                                    true);
                                exeInBrain = true;
                                _selfDataModelInstance.GetFloatDataEntry(RP_DataEntry_EnumType.CorpseDuration_尸体存在时间)
                                    .ResetDataToValue(
                                    (SelfAIBrainRuntimeInstance.BrainHandlerFunction.GetRelatedAnimationInfo(
                                            (decision.DecisionHandler as DH_通用常规死亡_CommonDeath)._an_Death) as
                                        SheetAnimationInfo_帧动画配置).ClipTransitionRef.Clip.length);
                            }
                        }
					
					}
				}
				else
				{
					var decision = SelfAIBrainRuntimeInstance.BrainHandlerFunction.FindSelfDecisionByString("通用普通死亡");
					if (decision != null)
					{
						SelfAIBrainRuntimeInstance.BrainHandlerFunction.AddDecisionToQueue(decision,
							BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首,true);
						exeInBrain = true;
						if (_selfDataModelInstance.GetFloatDataEntry(RP_DataEntry_EnumType.OverDamageOverride_过伤覆盖时长).GetCurrentValue() > float.Epsilon)
						{
                            _selfDataModelInstance.GetFloatDataEntry(RP_DataEntry_EnumType.CorpseDuration_尸体存在时间)
                            .ResetDataToValue(
                                (SelfAIBrainRuntimeInstance.BrainHandlerFunction.GetRelatedAnimationInfo(
                                        (decision.DecisionHandler as DH_通用常规死亡_CommonDeath)._an_Death) as
                                    SheetAnimationInfo_帧动画配置).ClipTransitionRef.Clip.length);
                        }
                            
					}
				}
			
				if (!exeInBrain)
				{
					DBug.LogWarning($"敌人角色{name}在死亡时，AI决策中并没有\"通用普通死亡\"的决策，正在试图直接播放[普通死亡]的动画。");
					var _sai_death = SelfAIBrainRuntimeInstance.BrainHandlerFunction.GetRelatedAnimationInfo("普通死亡");
					if (_sai_death != null)
					{
						var ds_ani = new DS_ActionBusArguGroup(_sai_death,
							AnimationPlayOptionsFlagTypeEnum.Default_缺省状态,
							_selfArtHelper.SelfAnimationPlayResult,
							false);
						_selfActionBusInstance.TriggerActionByType(ds_ani);
						exeInAni = true;
					}
				}
			}

			if (_selfArtHelper.MainCharacterAnimationHelperRef != null)
			{
				var dropDuration = Mathf.Clamp(1.5f,
					0f,
					_selfDataModelInstance.GetFloatDataEntry(RP_DataEntry_EnumType.CorpseDuration_尸体存在时间)
						.GetCurrentValue());
				StartBodyFallToGroundProgress(_selfArtHelper.MainCharacterAnimationHelperRef.gameObject,
					0f,
					dropDuration,
					Ease.OutExpo);
			}
			
			if ( (int)_selfBehaviourNamedType > 600999 &&  !exeInAni && !exeInBrain)
			{
				DBug.LogError($"{name}在试图播放死亡动画时，AIBrain中既没有通用死亡决策，也没有记录通用死亡动画，这不合理");
			}

			//死亡音效
			//死亡特效
			
			CommonDeath();

			//全局广播，如果可以变成尸体，自己变成尸体了
			var dsDie = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体);
			dsDie.ObjectArgu1 = this;
			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(dsDie);

			//全局广播，使玩家角色获得经验值
			var dsGainExp = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_CharacterBehaviour_OnEnemyDieGainExp_敌人死亡获得经验);
			dsGainExp.IntArgu1 = _selfDataModelInstance.SelfDataEntry_Database
				.GetFloatDataEntryByType(RP_DataEntry_EnumType.DropExp).GetRoundIntValue();
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(dsGainExp);

			var dsEnemyKilled = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_CharacterBehaviour_OnEnemyKilledWithUID_击杀特定ID的敌人);
			dsEnemyKilled.ObjectArguStr = this._databaseID;
			dsEnemyKilled.IntArgu1 = 1;
            GlobalActionBus.GetGlobalActionBus().TriggerActionByType(dsEnemyKilled);
        }



        protected void _PlayOverloadDamageVFX(Vector3 pos, float vfxRadius , RP_DS_DamageApplyResult dar)
		{
			GameObject _prefab = null;
			switch (dar.DamageType)
			{
				case DamageTypeEnum.YuanNengGuang_源能光:
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Light_Guang;
					break;
				case DamageTypeEnum.YuanNengDian_源能电:
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Electric_Dian;
					break;
				case DamageTypeEnum.AoNengTu_奥能土:
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Earth_Tu;
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Water_Shui;
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Fire_Huo;
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Wind_Feng;
					break;
				case DamageTypeEnum.LingNengLing_灵能灵:
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Spirit_Ling;
					break;
				case DamageTypeEnum.YouNengXingHong_幽能猩红:　
					_prefab = _glmRef.CharacterOnMapManagerReference.OverloadDamageVFXPrefab_Crimson_Xinghong;
					break;
				case DamageTypeEnum.YouNengAnYing_幽能暗影:
					break;
				case DamageTypeEnum.YouNengHunDun_幽能混沌:
					break;
				case DamageTypeEnum.TrueDamage_真伤:
					break;
			}
			if (_prefab)
			{ 
				var vfx = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(_prefab);
				vfx.transform.position = pos;
				vfx.transform.localScale = Vector3.one * vfxRadius;
				vfx.Play();
				 
			}
		}

		/// <summary>
		/// <para>无论是常规死亡还是自爆死亡，都要进行的通用死亡步骤</para>
		/// </summary>
		protected virtual void CommonDeath()
		{
			CharacterDataValid = false;
			var ds_dataInvalid =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_ARPGBehaviour_BehaviourDataInvalid_角色行为数据不再有效);
			ds_dataInvalid.ObjectArgu1 = this;
			_selfActionBusInstance.TriggerActionByType(ds_dataInvalid);

			//清理Buff，比如清理特效
			foreach (SOConfig_RPBuff perBuff in _selfDataModelInstance.SelfBuffHolderInstance.SelfActiveBuffCollection
				.Values)
			{
				perBuff.ConcreteBuffFunction.VFX_GeneralClear(true);
			}

			//检查自己的VFXContainer，如果有层级上的关系，直接拿出去
			foreach (var perPS in GetRelatedVFXContainer().SelfRegisteredPSPPInfoRuntimeList)
			{
				if (perPS == null)
				{
					continue;
				}
				if (perPS.transform == null)
				{
					continue;
				}
				if (perPS.transform.IsChildOf(transform))
				{
					perPS.transform.SetParent(VFXPoolManager.Instance.transform);
				}
				perPS.StopImmediately();
			}
		}

		

#endregion

#region 动画


		protected override void _ABC_ProcessAnimatorRequirement_OnRequireAnimator(DS_ActionBusArguGroup ds)
		{
			var currentAnimationInfoRef = ds.ObjectArgu1 as AnimationInfoBase;
			ds.ObjectArgu2 = _selfAnimationPlayerResult;
			_selfAnimationPlayerResult.Reset();
			float mul = ds.FloatArgu2 ?? 1f;
			//obj1为空，则表示传入的是str，那自己查一下
			if (currentAnimationInfoRef == null)
			{
				var configName = ds.ObjectArguStr as string;
				var findI =
					SelfAIBrainRuntimeInstance.BrainHandlerFunction.SelfAllPresetAnimationInfoList_RuntimeAll.FindIndex(
						info => info.ConfigName.Equals(configName, StringComparison.OrdinalIgnoreCase));
				if (findI == -1)
				{
					DBug.LogError($" 玩家{name}在处理动画需求时，在没有按照配置查找而是在名字查找时，没有找到名字【{configName}】这不合理，检查一下");
					return;
				}
				_selfAnimationPlayerResult.RelatedAnimationInfoRef = 
					currentAnimationInfoRef =
						SelfAIBrainRuntimeInstance.BrainHandlerFunction.SelfAllPresetAnimationInfoList_RuntimeAll[findI];
			}

			AnimationPlayOptionsFlagTypeEnum playOptions = (AnimationPlayOptionsFlagTypeEnum)ds.IntArgu1.Value;


			//需要检测占用
			if (ds.IntArgu2.HasValue && ds.IntArgu2.Value == 1)
			{
				Debug.LogError($"敌人的动画不需要检测占用，这是在干什么");
			}

			_selfArtHelper.SetAnimation(currentAnimationInfoRef, playOptions, mul);


		}

#endregion
		
		

#region HUD

		public UIRW_EnemyAboveHUDItem SelfRelatedHUD { get; protected set; }
		
		
		/// <summary>
		/// <para>为这个敌人注入其HUD的UIRW的引用</para>
		/// <para>主要作用是把它挂到自己的HUD挂点上</para>
		/// </summary>
		public void InjectHUDUIRWRef(UIRW_EnemyAboveHUDItem hudItem)
		{
			SelfRelatedHUD = hudItem;
			hudItem.transform.SetParent(_HUDHeightMatchGameObject.transform);
			// hudItem.transform.localPosition = Vector3.zero;
			hudItem._rect_SelfRectTransform.localPosition = Vector3.zero;
			hudItem._rect_SelfRectTransform.anchoredPosition = Vector2.zero;
			hudItem.transform.localScale = Vector3.one * 0.01f;
			hudItem.transform.localRotation = Quaternion.identity;
			
		}
		
#endregion
		
		
		
		protected void StartBodyFallToGroundProgress(GameObject body, float fellAnimationForce, float fellAnimationDuration, Ease easeType)
		{
			
			body.transform
				.DOLocalJump(
					new Vector3(body.transform.localPosition.x, DeathHeight, body.transform.localPosition.z),
					fellAnimationForce,
					0,
					fellAnimationDuration).SetEase(easeType);
			// body.transform.DOLocalMove(new Vector3(body.transform.localPosition.x, DeathHeight, body.transform.localPosition.z),
			// 	fellAnimationDuration).SetEase(easeType);
		}

		public override void ClearBeforeDestroy()
		{
			//清理工作
			//特效的清理

			//把VFXHolder底下的所有东西都拿走
			_selfArtHelper.ClearBeforeDestroy();


			//DataModel的清理
			//ArtHelper的清理
			_selfDataModelInstance.ClearBeforeDestroy();
			_selfDataModelInstance = null;
			//Ai的清理
			if (SelfAIBrainRuntimeInstance != null)
			{
				SelfAIBrainRuntimeInstance.BrainHandlerFunction.ClearBeforeDestroy();
				Object.Destroy(SelfAIBrainRuntimeInstance);
				SelfAIBrainRuntimeInstance = null;
			}
		}
	}
}