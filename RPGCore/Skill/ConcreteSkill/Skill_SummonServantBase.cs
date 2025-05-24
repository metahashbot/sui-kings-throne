// using System;
// using System.Collections.Generic;
// using ARPG.Character.Base;
// using ARPG.Character.Base.CustomSpineData;
// using ARPG.Character.Enemy.AI;
// using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
// using ARPG.Character.Player.Ally;
// using ARPG.Manager;
// using Global;
// using Global.ActionBus;
//
// using Global.Utility;
// using RPGCore.DataEntry;
// using RPGCore.Interface;
// using RPGCore.Projectile.Layout;
// using RPGCore.Skill.Config;
// using RPGCore.UtilityDataStructure;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.Pool;
// namespace RPGCore.Skill.ConcreteSkill
// {
// 	[Serializable]
// 	[TypeInfoBox("召唤类技能的技能基类")]
// 	public abstract class Skill_SummonServantBase : BaseRPSkill , I_SkillNeedShowProgress
// 	{
//
// 		[SerializeField, LabelText("VFX_召唤施法特效"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _vfx_release;
//
// 		[SerializeField, LabelText("VFX_召唤生成特效"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/VFX", Alignment = TitleAlignments.Centered), GUIColor(187f / 255f, 1f, 0f)]
// 		protected string _vfx_summonOn;
//
//
// 		protected AnimationInfoBase _sai_Release;
// 		[SerializeField, LabelText("目标召唤物的角色Type"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public CharacterNamedTypeEnum TargetSummonCharacterType;
//
// 		[SerializeField, LabelText("召唤范围"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float SummonRange = 2f;
// 		
// 		[SerializeField,LabelText("召唤数量上限"),FoldoutGroup("配置",true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public int InitialSummonLimit = 1;
// 		
// 		[SerializeField,LabelText("召唤持续时长"),FoldoutGroup("配置",true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		public float SummonDuration = 15f;
// 		[SerializeField, LabelText("召唤解除AI决策名"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/内部机制", Alignment = TitleAlignments.Centered)]
// 		public string _aidc_ReleaseDecisionName = "召唤解除";
//
// 		[SerializeField, LabelText("召唤物生成延迟(在动画TakeEffect之后)"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered)]
// 		protected float _summonAppearDelay = 1.5f;
//
// 		protected bool _registeredSummonTask = false;
// 		protected float _willSummonAppearTime;
// 		protected Vector3 _registeredSummonPosition;
//
//
// 		[ShowInInspector, LabelText("召唤物列表"), FoldoutGroup("运行时", true)]
// 		public List<SummonedBehaviourInfo> SummonedAllyInfoList { get; protected set; }= new List<SummonedBehaviourInfo>();
//
//
//
// 		[SerializeField, LabelText("包含 解除 时的自爆"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/内部机制", Alignment = TitleAlignments.Centered)]
// 		protected bool _containSelfExplosion_OnRelease = false;
//
// 		[SerializeField, LabelText("    AIDC_召唤解除"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/内部机制", Alignment = TitleAlignments.Centered), GUIColor(200f / 255f, 48f / 255f, 107f / 255f),
// 		 ShowIf(nameof(_containSelfExplosion_OnRelease))]
// 		protected string _aidc_SummonRelease;
//
// 		[SerializeField, LabelText("    解除后自爆的版面"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/内部机制", Alignment = TitleAlignments.Centered), ShowIf(nameof(_containSelfExplosion_OnRelease)),
// 		InlineEditor(InlineEditorObjectFieldModes.Boxed)]
// 		protected SOConfig_ProjectileLayout _selfExplosionLayout_OnRelease;
//
// 		
// 		
// 		
// 		[SerializeField, LabelText("包含 死亡 时的自爆"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/内部机制", Alignment = TitleAlignments.Centered)]
// 		protected bool _containSelfExplosion_OnDeath = false;
// 		
// 		[SerializeField, LabelText("AIDC_死亡"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/内部机制", Alignment = TitleAlignments.Centered), GUIColor(200f / 255f, 48f / 255f, 107f / 255f),
// 		 ShowIf(nameof(_containSelfExplosion_OnDeath))]
// 		protected string _aidc_death;
// 		
// 		[SerializeField, LabelText("    死亡后自爆的版面"), FoldoutGroup("配置", true),
// 		 TitleGroup("配置/配置", Alignment = TitleAlignments.Centered), ShowIf(nameof(_containSelfExplosion_OnDeath))
// 		,InlineEditor(InlineEditorObjectFieldModes.Boxed)]
// 		protected SOConfig_ProjectileLayout _selfExplosionLayout_OnDeath;
// 		
// 		
//
// 		public class SummonedBehaviourInfo
// 		{
// 			public AllyARPGCharacterBehaviour BehaviourRef;
// 			public float StartTime;
// 			public float RemainingDuration;
// 		}
// 		
// 		public override void InitOnObtain(
// 			RPSkill_SkillHolder skillHolderRef,
// 			SOConfig_RPSkill configRuntimeInstance,
// 			I_RP_ObjectCanReleaseSkill parent,
// 			SkillSlotTypeEnum slot)
// 		{
// 			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
// 			// _sai_Release =
// 			// 	GetAnimationInfoByConfigName(configRuntimeInstance.ContentInSO._AN_SkillReleaseSpineAnimationName);
//
// 			_selfActionBusRef.RegisterAction(
// 				ActionBus_ActionTypeEnum.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了,
// 				_ABC_CheckAndRemoveSummonedInfo_OnAllyCorpseDestroyed);
// 		}
//
// 		protected override DS_ActionBusArguGroup OnSkillBeginPrepare(bool autoLaunch = true)
// 		{
// 			var ds = base.OnSkillBeginPrepare(autoLaunch);
//
//
// 			OnSkillResetCoolDown();
// 			OnSkillConsumeSP();
//
// 			_VFX_GetOrInstantiateNew(_vfx_release)._VFX__10_PlayThis(true,true);
//
//
// 			// _Internal_GeneralRequireAnimationEvent(SelfSkillConfigRuntimeInstance.ContentInSO._AN_SkillReleaseSpineAnimationName,
// 			// 	true);
// 			
// 			return ds;
// 		}
// 		
// 		
// 		
// 		/// <summary>
// 		/// <para>获取正常的召唤位置，不包括元素亲和那种</para>
// 		/// <para>默认的召唤位置是玩家当前朝向，如果不能用就每30度转一下尝试，还不能就和玩家到同一个位置</para>
// 		/// </summary>
// 		/// <returns></returns>
// 		protected Vector3 GetBaseSummonPosition(float radius)
// 		{
// 			var glmRef = SubGameplayLogicManager_ARPG.Instance;
// 			var faceLeft = _characterBehaviourRef.GetSelfRolePlayArtHelper().CurrentFaceLeft;
// 			Vector3 fromPosition = _characterBehaviourRef.transform.position;
// 			Vector3 fromOffset = faceLeft ? Vector3.left : Vector3.right;
// 		
// 			for (float angle = 0f; angle < 360f; angle += 30f)
// 			{
// 				Vector3 targetPosition = fromPosition + MathExtend.Vector3RotateOnXOZ(fromOffset * radius, angle);
// 				var r = glmRef.CheckTargetPositionFromPositionValid(fromPosition, targetPosition);
// 				if (r.HasValue)
// 				{
// 					return r.Value;
// 				}
// 			}
// 			return fromPosition;
// 		}
//
//
// 		/// <summary>
// 		/// <para>召唤物最终生成，因为前面还会有一个延迟</para>
// 		/// </summary>
// 		protected virtual SummonedBehaviourInfo SummonedBehaviourAppear()
// 		{
//
// 			CharacterOnMapManager comRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
// 			AllyARPGCharacterBehaviour summoned =
// 				comRef.SpawnNewAllyByTypeAndPosition(TargetSummonCharacterType, _registeredSummonPosition, 1);
// 			
// 			
// 			
// 			
// 			SummonedBehaviourInfo newInfo = GenericPool<SummonedBehaviourInfo>.Get();
// 			newInfo.BehaviourRef = summoned;
// 			newInfo.RemainingDuration = SummonDuration;
// 			newInfo.StartTime = BaseGameReferenceService.CurrentFixedTime;
// 			SummonedAllyInfoList.Add(newInfo);
// 			
// 			newInfo.BehaviourRef.GetAIBrainRuntimeInstance().BrainHandlerFunction.PickDefaultBehaviourPattern();
// 			if (_containSelfExplosion_OnDeath)
// 			{
// 				var dh_selfExplosion =
// 					(summoned.GetAIBrainRuntimeInstance().BrainHandlerFunction.FindSelfDecisionByString(_aidc_death)
// 						.DecisionHandler as DH_通用常规死亡_CommonDeath);
// 				dh_selfExplosion._containDeathLayout = true;
// 				dh_selfExplosion._relatedLayout = _selfExplosionLayout_OnDeath;
// 			
// 			}
// 			
// 			if (_containSelfExplosion_OnRelease)
// 			{
// 				var dh_summonRelease =
// 					(summoned.GetAIBrainRuntimeInstance().BrainHandlerFunction.FindSelfDecisionByString(_aidc_SummonRelease)
// 						.DecisionHandler as DH_通用常规自爆_CommonSelfExplosion);
// 				dh_summonRelease._containLayout = true;
// 				dh_summonRelease._layout_SelfExplosion = _selfExplosionLayout_OnRelease;
// 			}
// 			return newInfo;
//
//
// 		}
//
// 		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
// 		{
// 			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
// 			TickToSummonRelease(currentTime, currentFrameCount, delta);
// 			
// 			if (_registeredSummonTask)
// 			{
// 				if (currentTime > _willSummonAppearTime)
// 				{
// 					_registeredSummonTask = false;
// 					SummonedBehaviourAppear();
// 				}
// 			}
// 		}
// 		/// <summary>
// 		/// <para>同步一下属性</para>
// 		/// </summary>
// 		/// <param name="ally"></param>
// 		protected void SyncWithSelfData(AllyARPGCharacterBehaviour ally)
// 		{
// 			var selfHPMax = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).ReplaceOriginalValue(selfHPMax.CurrentValue);
// 			
// 			var selfHP = _characterBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
// 			ally.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP).ReplaceOriginalValue(selfHP.CurrentValue);
//
// 			var selfSPMax = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP).ReplaceOriginalValue(selfSPMax.CurrentValue);
// 			
// 			var selfSP = _characterBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);
// 			ally.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP).ReplaceOriginalValue(selfSP.CurrentValue);
//
// 			var selfATK = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackPower_攻击力);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackPower_攻击力).ReplaceOriginalValue(selfATK.CurrentValue);
//
// 			var selfDEF = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.Defense_防御力);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.Defense_防御力).ReplaceOriginalValue(selfDEF.CurrentValue);
//
// 			var selfMoveSpeed = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速).ReplaceOriginalValue(selfMoveSpeed.CurrentValue);
//
// 			var selfAttackSpeed =
// 				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
// 			
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackSpeed_攻击速度).ReplaceOriginalValue(selfAttackSpeed.CurrentValue);
// 			
// 			var selfCriticalRate =
// 				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalRate_暴击率);
// 			
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalRate_暴击率).ReplaceOriginalValue(selfCriticalRate.CurrentValue);
//
// 			var selfCriticalDamage =
// 				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalBonus_暴击伤害);
// 			
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalBonus_暴击伤害).ReplaceOriginalValue(selfCriticalDamage.CurrentValue);
// 			
// 			var selfToughness =
// 				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.Toughness_韧性);
// 			
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.Toughness_韧性).ReplaceOriginalValue(selfToughness.CurrentValue);
//
// 			var selfAccuracy = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.Accuracy_命中率);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.Accuracy_命中率).ReplaceOriginalValue(selfAccuracy.CurrentValue);
//
// 			var selfDodge = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.DodgeRate_闪避率);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.DodgeRate_闪避率).ReplaceOriginalValue(selfDodge.CurrentValue);
//
// 			var selfStr = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Strength_主力量);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Strength_主力量).ReplaceOriginalValue(selfStr.CurrentValue);
//
// 			var selfInt = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Intellect_主智力);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Intellect_主智力).ReplaceOriginalValue(selfInt.CurrentValue);
//
// 			var selfDex = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Dexterity_主敏捷);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Dexterity_主敏捷).ReplaceOriginalValue(selfDex.CurrentValue);
// 			
// 			var selfVit = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Vitality_主体质);
// 			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Vitality_主体质).ReplaceOriginalValue(selfVit.CurrentValue);
// 			
//
//
//
//
//
// 		}
//
//
// 		protected virtual void RegisterSummonPositionAndTask()
// 		{
// 			_registeredSummonTask = true;
// 			_willSummonAppearTime = BaseGameReferenceService.CurrentFixedTime + _summonAppearDelay;
//
// 			// _Internal_BroadcastSkillReleaseFinish();
//
// 			//到这里了，开始具体的召唤
// 			_registeredSummonPosition = GetBaseSummonPosition(SummonRange);
//
// 			_VFX_GetOrInstantiateNew(_vfx_summonOn)._VFX_2_SetPositionToGlobalPosition(_registeredSummonPosition)._VFX__10_PlayThis();
// 			
// 		}
// 		
//
//
// 		/// <summary>
// 		/// <para>当有角色的Behaviour被销毁时，检查是不是自己的召唤物，是的话就移除掉</para>
// 		/// </summary>
// 		protected virtual void _ABC_CheckAndRemoveSummonedInfo_OnAllyCorpseDestroyed(DS_ActionBusArguGroup ds)
// 		{
// 			var targetBehaviour = ds.ObjectArgu1 as BaseARPGCharacterBehaviour;
// 			if (targetBehaviour == null)
// 			{
// 				return;
// 			}
//
// 			int findIndex = SummonedAllyInfoList.FindIndex((info => info.BehaviourRef.Equals(targetBehaviour)));
// 			if (findIndex != -1)
// 			{
// 				SummonedAllyInfoList.RemoveAt(findIndex);
// 			}
// 		}
//
// 		
// 		
// 		/// <summary>
// 		/// <para>tick，检查是否超时解除召唤</para>
// 		/// </summary>
// 		protected virtual void TickToSummonRelease(float ct, int cf, float delta)
// 		{
// 			for (int i = SummonedAllyInfoList.Count - 1; i >= 0; i--)
// 			{
// 				if(SummonedAllyInfoList[i].BehaviourRef == null)
// 				{
// 					SummonedAllyInfoList.RemoveAt(i);
// 					continue;
// 				}
// 				var perSummon = SummonedAllyInfoList[i];
// 				perSummon.RemainingDuration -= delta;
// 				if (perSummon.RemainingDuration < 0f)
// 				{
// 					perSummon.BehaviourRef.GetAIBrainRuntimeInstance().BrainHandlerFunction.AddDecisionToQueue(
// 						_aidc_ReleaseDecisionName,
// 						BaseAIBrainHandler.DecisionEnqueueType.EnqueueToSecond_加入排队但是清空后续);
// 					SummonedAllyInfoList.RemoveAt(i);
// 				}
//
// 			}
// 		}
// 		public override DS_ActionBusArguGroup ClearBeforeRemove()
// 		{
// 			_selfActionBusRef.RemoveAction(
// 				ActionBus_ActionTypeEnum.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了,
// 				_ABC_CheckAndRemoveSummonedInfo_OnAllyCorpseDestroyed);
// 			return base.ClearBeforeRemove();
// 			
// 		}
//
// 		public AnimationInfoBase ActAsSheetAnimationInfo => _sai_Release;
// 		BaseRPSkill I_SkillNeedShowProgress._selfSkillRef_Interface => this;
// 	}
// }