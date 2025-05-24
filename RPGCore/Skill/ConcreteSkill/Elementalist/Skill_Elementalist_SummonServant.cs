using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Enemy.AI.Listen;
using ARPG.Character.Player;
using ARPG.Character.Player.Ally;
using ARPG.Manager;
using Global;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	[Serializable]
	public class Skill_Elementalist_SummonServant : BaseRPSkill
	{
		
		
		[SerializeField,LabelText("自毁监听") ,TitleGroup("===数值===")]
		protected SOConfig_AIListen _selfDestroyListen;
		
		[SerializeField,LabelText("召唤物存在时长"),TitleGroup("===数值===")]
		protected float _summonedExistTime = 10f;
		
		
		[ShowInInspector,LabelText("已召唤角色"),FoldoutGroup("运行时")]
		protected List<AllyARPGCharacterBehaviour > SummonedAllyInfoList = new List<AllyARPGCharacterBehaviour>();
		
		
		protected CharacterNamedTypeEnum _currentActiveSummonCharacterType = CharacterNamedTypeEnum.None;
		
		protected Vector3 _registeredSummonPosition = Vector3.zero; 
		
		
		
		/// <summary>
		/// <para></para>
		/// </summary>
		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			_currentActiveSummonCharacterType = CharacterNamedTypeEnum.ElementalServant_元素傀儡;
			_registeredSummonPosition = _characterBehaviourRef.transform.position;
			SummonedBehaviourAppear();
			base._InternalSkillEffect_SkillDefaultTakeEffect();
		}


		/// <summary>
		/// <para>召唤物最终生成，因为前面还会有一个延迟</para>
		/// </summary>
		protected virtual void SummonedBehaviourAppear()
		{

			CharacterOnMapManager comRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
			AllyARPGCharacterBehaviour summoned =
				comRef.SpawnNewAllyByTypeAndPosition(_currentActiveSummonCharacterType, _registeredSummonPosition, 1);
			SyncWithSelfData(summoned);

			SummonedAllyInfoList.Add(summoned);


			switch (GetCurrentDamageType())
			{
				case DamageTypeEnum.AoNengTu_奥能土:
					summoned.GetAIBrainRuntimeInstance().BrainHandlerFunction.SwitchBehaviourPattern("土傀儡");
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					summoned.GetAIBrainRuntimeInstance().BrainHandlerFunction.SwitchBehaviourPattern("水傀儡");
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					summoned.GetAIBrainRuntimeInstance().BrainHandlerFunction.SwitchBehaviourPattern("火傀儡");
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					summoned.GetAIBrainRuntimeInstance().BrainHandlerFunction.SwitchBehaviourPattern("风傀儡");
					break;
			}
			var dd = (_selfDestroyListen.ListenComponent as AIListen_计时副作用_TimedSideEffect);
			dd.FixedDelay = true;
			dd.FixedDelayTime = _summonedExistTime;
			summoned.GetAIBrainRuntimeInstance().BrainHandlerFunction.AddNewSingleAIListen(_selfDestroyListen);
		}
		/// <summary>
		/// <para>同步一下属性</para>
		/// </summary>
		/// <param name="ally"></param>
		protected void SyncWithSelfData(AllyARPGCharacterBehaviour ally)
		{
			var selfHPMax = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).ReplaceOriginalValue(selfHPMax.CurrentValue);
			
			var selfHP = _characterBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			ally.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP).ReplaceOriginalValue(selfHP.CurrentValue);

			var selfSPMax = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP).ReplaceOriginalValue(selfSPMax.CurrentValue);
			
			var selfSP = _characterBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP);
			ally.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentSP_当前SP).ReplaceOriginalValue(selfSP.CurrentValue);

			var selfATK = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackPower_攻击力);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackPower_攻击力).ReplaceOriginalValue(selfATK.CurrentValue);

			var selfDEF = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.Defense_防御力);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.Defense_防御力).ReplaceOriginalValue(selfDEF.CurrentValue);

			var selfMoveSpeed = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速).ReplaceOriginalValue(selfMoveSpeed.CurrentValue);

			var selfAttackSpeed =
				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.AttackSpeed_攻击速度).ReplaceOriginalValue(selfAttackSpeed.CurrentValue);
			
			var selfCriticalRate =
				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalRate_暴击率);
			
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalRate_暴击率).ReplaceOriginalValue(selfCriticalRate.CurrentValue);

			var selfCriticalDamage =
				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalBonus_暴击伤害);
			
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.CriticalBonus_暴击伤害).ReplaceOriginalValue(selfCriticalDamage.CurrentValue);
			
			var selfToughness =
				_characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.Toughness_韧性);
			
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.Toughness_韧性).ReplaceOriginalValue(selfToughness.CurrentValue);

			var selfAccuracy = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.Accuracy_命中率);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.Accuracy_命中率).ReplaceOriginalValue(selfAccuracy.CurrentValue);

			var selfDodge = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.DodgeRate_闪避率);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.DodgeRate_闪避率).ReplaceOriginalValue(selfDodge.CurrentValue);

			var selfStr = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Strength_主力量);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Strength_主力量).ReplaceOriginalValue(selfStr.CurrentValue);

			var selfInt = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Intellect_主智力);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Intellect_主智力).ReplaceOriginalValue(selfInt.CurrentValue);

			var selfDex = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Dexterity_主敏捷);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Dexterity_主敏捷).ReplaceOriginalValue(selfDex.CurrentValue);
			
			var selfVit = _characterBehaviourRef.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Vitality_主体质);
			ally.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Vitality_主体质).ReplaceOriginalValue(selfVit.CurrentValue);
			
		}

	}
}