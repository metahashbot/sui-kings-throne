using System;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Config;
using ARPG.Character.Enemy.AI;
using ARPG.Config;
using ARPG.Manager;
using DG.Tweening;
using Global;
using Global.ActionBus;
using RPGCore;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using WorldMapScene.Character;
using Object = UnityEngine.Object;
namespace ARPG.Character.Player.Ally
{
	[TypeInfoBox("一个友军")]
	public class AllyARPGCharacterBehaviour : BaseARPGCharacterBehaviour
	{


#if UNITY_EDITOR

		// redraw constantly
		[OnInspectorGUI]
		private void RedrawConstantly()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
		}

#endif

		[SerializeField, Required, LabelText("ArtHelper"), FoldoutGroup("配置", true)]
		protected AllyARPGArtHelper _selfArtHelper;



		[ShowInInspector, LabelText("数据模型"), FoldoutGroup("运行时", true)]
		protected ARPGAllyDataModel _selfDataModelInstance;

		[ShowInInspector, LabelText("正在使用的AIBrain运行时实例"), FoldoutGroup("运行时", true)]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		protected SOConfig_AIBrain SelfAIBrainRuntimeInstance;

		public SOConfig_AIBrain GetAIBrainRuntimeInstance()
		{
			return SelfAIBrainRuntimeInstance;
		}

		

		public override void InitializeOnInstantiate()
		{
			base.InitializeOnInstantiate();
			_selfDataModelInstance = new ARPGAllyDataModel(this);
			_selfArtHelper.InitializeOnInstantiate(_selfActionBusInstance);
			_selfArtHelper.InjectBaseRPBehaviourRef(this);

			ApplySpawnAddonConfig();



			_selfActionBusInstance.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_OnFinallyDeath_伤害流程最终死亡,
				_ABC_OnFinallyDead_OnHpReducedTo0);
			_selfActionBusInstance.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_RequireDirectSelfExplosion_要求直接自爆,
				_ABC_OnDirectSelfExplosion_OnRequireSelfExplosion);
		}




		public override void InitializeByConfig(
			SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE entry,
			SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry gameplayDataEntry,
			SOFE_CharacterResourceInfo.PerTypeInfo resourceEntry,
			Vector3 spawnPos)
		{
			base.InitializeByConfig(entry, gameplayDataEntry, resourceEntry, spawnPos);
			//初始化AIBrain
			string aiBrainUID = entry.AIBrainID;
			SOConfig_AIBrain relatedRawAIBrain =
				GlobalConfigurationAssetHolderHelper.Instance.Collection_AIBrainCollection.GetConfigByID(aiBrainUID);
			if (relatedRawAIBrain == null)
			{
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
		}

		public override void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.UpdateTick(currentTime, currentFrameCount, delta);
			if (SelfAIBrainRuntimeInstance)
			{
				SelfAIBrainRuntimeInstance.BrainHandlerFunction.UpdateTick(currentTime, currentFrameCount, delta);
			}
		}


		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			_selfArtHelper.FixedUpdateTick( currentTime , currentFrameCount , delta);
			if (SelfAIBrainRuntimeInstance)
			{
				SelfAIBrainRuntimeInstance.BrainHandlerFunction.FixedUpdateTick(currentTime, currentFrameCount, delta);
			}
		}

		/// <summary>
		/// <para>直接要求自爆。会跳过常规伤害流程中的一些步骤</para>
		/// </summary>
		protected virtual void _ABC_OnDirectSelfExplosion_OnRequireSelfExplosion(DS_ActionBusArguGroup ds)
		{
			_selfDataModelInstance.GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP).ResetDataToValue(-1f);


			CommonDeath();
			var ds_deathDirect =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieDirect_友军直接死亡没有尸体);
			ds_deathDirect.ObjectArgu1 = this;

			GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_deathDirect);


		}

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
				var decision =
					SelfAIBrainRuntimeInstance.BrainHandlerFunction.FindSelfDecisionByString("通用普通死亡");
				if (decision != null)
				{
					SelfAIBrainRuntimeInstance.BrainHandlerFunction.AddDecisionToQueue(decision,
						BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首);
					exeInBrain = true;
				}

				if (!exeInBrain)
				{
					DBug.LogWarning($"友军角色{name}在死亡时，AI决策中并没有\"通用普通死亡\"的决策，正在试图直接播放[普通死亡]的动画。");
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

			if ( (int)_selfBehaviourNamedType > 600999 &&  !exeInAni && !exeInBrain)
			{
				DBug.LogError($"{name}在试图播放死亡动画时，AIBrain中既没有通用死亡决策，也没有记录通用死亡动画，这不合理");
			}

			//死亡音效
			//死亡特效
			
			CommonDeath();

			if (_selfArtHelper.MainCharacterAnimationHelperRef != null)
			{
				StartBodyFallToGroundProgress(_selfArtHelper.MainCharacterAnimationHelperRef.gameObject, 0f, 1.5f, Ease.OutExpo);
			}
			

			//全局广播，如果可以变成尸体，自己变成尸体了
			var dsDie = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieToCorpse_友军死亡到尸体);
			dsDie.ObjectArgu1 = this;
		
			GlobalActionBus.GetGlobalActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.G_AllyBehaviour_OnAllyDieToCorpse_友军死亡到尸体, dsDie);


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
		protected void StartBodyFallToGroundProgress(
			GameObject body,
			float fellAnimationForce,
			float fellAnimationDuration,
			Ease easeType)
		{
			//牢大的直升机死了从天上落地
			body.transform
				.DOLocalJump(new Vector3(body.transform.localPosition.x, DeathHeight, body.transform.localPosition.z),
					fellAnimationForce,
					0,
					fellAnimationDuration).SetEase(easeType);
			// body.transform.DOLocalMove(new Vector3(body.transform.localPosition.x, DeathHeight, body.transform.localPosition.z),
			// 	fellAnimationDuration).SetEase(easeType);
		}

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
				_selfAnimationPlayerResult.RelatedAnimationInfoRef = currentAnimationInfoRef = SelfAIBrainRuntimeInstance
					.BrainHandlerFunction.SelfAllPresetAnimationInfoList_RuntimeAll[findI];
			}

			AnimationPlayOptionsFlagTypeEnum playOptions = (AnimationPlayOptionsFlagTypeEnum)ds.IntArgu1.Value;


			//需要检测占用
			if (ds.IntArgu2.HasValue && ds.IntArgu2.Value == 1)
			{
				Debug.LogError($"召唤物的动画不需要检测占用，这是在干什么");
			}

			_selfArtHelper.SetAnimation(currentAnimationInfoRef, playOptions, mul);

		}


#endregion
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


		protected override RolePlay_DataModelBase GetSelfRolePlayDataModel()
		{
			return _selfDataModelInstance;
		}
		public override RolePlay_ArtHelperBase GetSelfRolePlayArtHelper()
		{
			return _selfArtHelper;
		}
	}
}