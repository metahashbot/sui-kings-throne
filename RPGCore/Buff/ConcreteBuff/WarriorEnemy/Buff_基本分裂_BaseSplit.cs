using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager.Component;
using ARPG.Manager.Config;
using GameplayEvent;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.WarriorEnemy
{
	[Serializable]
	public class Buff_基本分裂_BaseSplit : BaseRPBuff
	{

		[LabelText("警示时间"), FoldoutGroup("配置", true), SuffixLabel("秒")]
		public float WarningTime;


		[LabelText("间隔_上次效果结束到下次警示开始"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
		public float IntervalBetweenEffect;



		[LabelText("分身上限"), FoldoutGroup("配置", true)]
		public int MaxSplitCount = 3;

		[LabelText("分身最大生命值比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float MaxSplitHPPercent = 50f;

		[LabelText("分身攻击力比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float MaxSplitAttackDamagePercent = 50f;

		[LabelText(" 分身防御比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float MaxSplitDefensePercent = 50f;

		[LabelText("分身死亡后给本体回血最大生命值比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float MaxSplitDeadHealPercent = 10f;

		[SerializeField, LabelText("常驻特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _Vfx_SplitLoop;

		protected PerVFXInfo _vfxInfo_SplitLoop;


		[SerializeField, LabelText("能力被破坏时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_AbilityBreak;

		protected PerVFXInfo _vfxInfo_AbilityBreak;
		
		[SerializeField,LabelText("能力被破坏时的事件们") ,FoldoutGroup("配置",true),TitleGroup("配置")]
		protected List<SOConfig_PrefabEventConfig> _abilityBreakEvents;

		[SerializeField, LabelText("充能时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_charge;

		protected bool charge_flag = true;
		protected PerVFXInfo _vfxInfo_Charge;

		[SerializeField, LabelText("警示时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_warning;

		protected PerVFXInfo _vfxInfo_Warning;


		[SerializeField, LabelText("释放时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_release;

		protected PerVFXInfo _vfxInfo_Release;


		/// <summary>
		/// 由自身分裂产出的行为容器
		/// </summary>
		[ShowInInspector, FoldoutGroup("运行时")]
		protected List<EnemyARPGCharacterBehaviour> _selfRelatedSplitBehaviourList;
		/// <summary>
		///  当前活跃的分身数量
		/// </summary>
		public int CurrentActiveSplitCount => _selfRelatedSplitBehaviourList.Count;


		protected enum BattleSplitStateTypeEnum
		{
			None_无事发生 = 0, Interval_间隔中 = 1, Warning_警示中 = 2,
		}


		protected BattleSplitStateTypeEnum CurrentState = BattleSplitStateTypeEnum.None_无事发生;


		protected float _nextChangeStateTime;

		private EnemyARPGCharacterBehaviour _selfEnemyBehaviourRef;




		[NonSerialized, LabelText("当前能力可用吗")]
		public bool CurrentAbilityAvailable = true;





		[SerializeField, LabelText("重新恢复效果的时长"), FoldoutGroup("配置", true), TitleGroup("配置/数值"), SuffixLabel("秒")]
		protected float _recoveryDuration = 10f;
		protected float _willRecoverTime;




		
		
		
		
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_DisableAbility_OnThunderBoom);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
				_ABCRefreshSplitBehaviour_OnTurnToCorpse);
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
				_ABCRefreshSplitBehaviour_OnTurnToCorpse);
			_selfRelatedSplitBehaviourList =
				CollectionPool<List<EnemyARPGCharacterBehaviour>, EnemyARPGCharacterBehaviour>.Get();
			_selfRelatedSplitBehaviourList.Clear();
			_selfEnemyBehaviourRef = parent as EnemyARPGCharacterBehaviour;
		}


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			EnableAbility();
			return base.OnBuffInitialized(blps);
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_基本分裂_BattleWildLogicPassing blp_BaseSplit:
					WarningTime = blp_BaseSplit.WarningTime;
					IntervalBetweenEffect = blp_BaseSplit.IntervalBetweenEffect;
					MaxSplitCount = blp_BaseSplit.MaxSplitCount;
					MaxSplitHPPercent = blp_BaseSplit.MaxSplitHPPercent;
					MaxSplitAttackDamagePercent = blp_BaseSplit.MaxSplitAttackDamagePercent;
					MaxSplitDefensePercent = blp_BaseSplit.MaxSplitDefensePercent;
					MaxSplitDeadHealPercent = blp_BaseSplit.MaxSplitDeadHealPercent;

					break;
			}
		}



		protected virtual void _ABC_DisableAbility_OnThunderBoom(DS_ActionBusArguGroup ds)
		{
			var targetBuffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			if (targetBuffType == RolePlay_BuffTypeEnum.ElementSecondThunderBoom_LeiBao_二级雷暴)
			{
				DisableAbility();
			}
		}



		protected virtual void EnableAbility()
		{
			CurrentAbilityAvailable = true;

			CurrentState = BattleSplitStateTypeEnum.Interval_间隔中;
			_vfxInfo_SplitLoop = _VFX_GetAndSetBeforePlay(_Vfx_SplitLoop)._VFX__10_PlayThis();


		}


		/// <summary>
		/// <para>具体的禁用能力，通常包括停止bool以不再tick，并播放相关特效</para>
		/// </summary>
		protected virtual void DisableAbility()
		{
			_willRecoverTime = BaseGameReferenceService.CurrentFixedTime + _recoveryDuration;
			
			if (CurrentAbilityAvailable)
			{
				CurrentAbilityAvailable = false;
				_vfxInfo_SplitLoop.VFX_StopThis(true);
				_vfxInfo_Warning.VFX_StopThis(true);
				_vfxInfo_Charge.VFX_StopThis(true);
				_vfxInfo_AbilityBreak = _VFX_GetAndSetBeforePlay(_vfx_AbilityBreak)._VFX__10_PlayThis();
				foreach (var eventConfig in _abilityBreakEvents)
				{
					if (eventConfig == null)
					{
						continue;
					}
					GameplayEventManager.Instance.StartGameplayEvent(eventConfig);
				}
			}
		}



		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			if (!CurrentAbilityAvailable && currentTime > _willRecoverTime)
			{
				EnableAbility();
			}
			
			
			if (!CurrentAbilityAvailable)
			{
				return;
			}
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			switch (CurrentState)
			{
				case BattleSplitStateTypeEnum.Interval_间隔中:

					if (charge_flag)
					{
						_vfxInfo_Charge = _VFX_GetAndSetBeforePlay(_vfx_charge)?._VFX__10_PlayThis();
						float mulSimSpeed = 10 / IntervalBetweenEffect;
						_vfxInfo_Charge._VFX__6_SetSimulationSpeed(mulSimSpeed);
						charge_flag = false;
					}

					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = BattleSplitStateTypeEnum.Warning_警示中;
						_vfxInfo_Charge.VFX_StopThis(true);
						charge_flag = true;
						_nextChangeStateTime = currentTime + WarningTime;
						_vfxInfo_Warning = _VFX_GetAndSetBeforePlay(_vfx_warning)?._VFX__10_PlayThis();
					}
					break;
				case BattleSplitStateTypeEnum.Warning_警示中:
					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = BattleSplitStateTypeEnum.Interval_间隔中;
						_VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();
						if (_vfx_warning != null)
						{
							_vfxInfo_Warning.VFX_StopThis(true);
						}
						_nextChangeStateTime = currentTime + IntervalBetweenEffect;
						ActivateSplitEffect();
					}
					break;
			}
		}





		protected virtual void ActivateSplitEffect()
		{
			//执行一次分裂
			if (!CurrentAbilityAvailable)
			{
				return;
			}

			if (CurrentActiveSplitCount >= MaxSplitCount)
			{
				return;
			}


			EnemySpawnService_SubActivityService.SingleSpawnInfo newSingleSpawnInfo =
				_selfEnemyBehaviourRef.RelatedSpawnConfigInstance_SingleSpawnInfo;
			List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo> relatedCollection =
				_selfEnemyBehaviourRef.RelatedSpawnConfigInstance.EnemySpawnConfigTypeHandler.EnemySpawnCollection_RuntimeFinal;

			// _selfEnemyBehaviourRef.RelatedSpawnConfigInstance.EnemySpawnConfigTypeHandler
			// 	.AddNewSingleSpawnInfoToListByPerEnemyTypeInfo(newSingleSpawnInfo.RawSpawnInfo, 1);

			;
			EnemyARPGCharacterBehaviour newBehaviour =
				_characterOnMapManagerRef.SpawnNewEnemyBySingleConfigEntryInRuntime(_selfEnemyBehaviourRef,
					_selfEnemyBehaviourRef.RelatedSpawnConfigInstance_SingleSpawnInfo,
					_selfEnemyBehaviourRef.transform.position, _selfEnemyBehaviourRef.SelfCharacterVariantScale,
					true);
			newBehaviour.GetAIBrainRuntimeInstance().BrainHandlerFunction.PickDefaultBehaviourPattern();

			var hpMax = newBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			hpMax.ReplaceOriginalValue(hpMax.CurrentValue * (MaxSplitHPPercent / 100f));

			var attackDamage = newBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackPower_攻击力);
			attackDamage.ReplaceOriginalValue(attackDamage.CurrentValue * (MaxSplitAttackDamagePercent / 100f));

			var defense = newBehaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.Defense_防御力);
			defense.ReplaceOriginalValue(defense.CurrentValue * (MaxSplitDefensePercent / 100f));
			
			
			


			if (!_selfRelatedSplitBehaviourList.Contains(newBehaviour))
			{
				_selfRelatedSplitBehaviourList.Add(newBehaviour);
			}
			else
			{
				DBug.LogError($"分身在添加时出现了重复的角色");
			}
		}






		protected virtual void _ABCRefreshSplitBehaviour_OnTurnToCorpse(DS_ActionBusArguGroup ds)
		{
			if (!CurrentAbilityAvailable)
			{
				return;
			}
			var behaviour = ds.ObjectArgu1 as EnemyARPGCharacterBehaviour;
			//
			if (_selfRelatedSplitBehaviourList.Contains(behaviour))
			{
				_selfRelatedSplitBehaviourList.Remove(behaviour);
			}


			if (MaxSplitDeadHealPercent > float.Epsilon)
			{
				var maxHP = _selfEnemyBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);

				var damage_healSelf = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(_selfEnemyBehaviourRef,
					_selfEnemyBehaviourRef,
					DamageTypeEnum.Heal_治疗,
					maxHP.CurrentValue * (MaxSplitDeadHealPercent / 100f),
					DamageProcessStepOption.HealDPS());
				damage_healSelf.DamageWorldPosition = behaviour.transform.position;
				_damageAssistServiceRef.ApplyDamage(damage_healSelf);
			}
		}


		protected override void ClearAndUnload()
		{
			GlobalActionBus.GetGlobalActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieToCorpse_敌人死亡到尸体,
				_ABCRefreshSplitBehaviour_OnTurnToCorpse);
			GlobalActionBus.GetGlobalActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.G_EnemyBehaviour_OnEnemyDieDirect_敌人直接死亡没有尸体,
				_ABCRefreshSplitBehaviour_OnTurnToCorpse);

			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
				_ABC_DisableAbility_OnThunderBoom);




			_selfRelatedSplitBehaviourList.Clear();
			CollectionPool<List<EnemyARPGCharacterBehaviour>, EnemyARPGCharacterBehaviour>.Release(
				_selfRelatedSplitBehaviourList);
			base.ClearAndUnload();
		}

		[Serializable]
		public class BLP_基本分裂_BattleWildLogicPassing : BaseBuffLogicPassingComponent
		{



			[LabelText("警示时间"), FoldoutGroup("配置", true), SuffixLabel("秒")]
			public float WarningTime;


			[LabelText("间隔_上次效果结束到下次警示开始"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
			public float IntervalBetweenEffect;



			[LabelText("分身上限"), FoldoutGroup("配置", true)]
			public int MaxSplitCount = 3;

			[LabelText("分身最大生命值比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
			public float MaxSplitHPPercent = 50f;

			[LabelText("分身攻击力比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
			public float MaxSplitAttackDamagePercent = 50f;

			[LabelText(" 分身防御比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
			public float MaxSplitDefensePercent = 50f;

			[LabelText("分身死亡后给本体回血最大生命值比例"), FoldoutGroup("配置", true), SuffixLabel("%")]
			public float MaxSplitDeadHealPercent = 10f;


			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_基本分裂_BattleWildLogicPassing>.Release(this);
			}
		}

	}
}