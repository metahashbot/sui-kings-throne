using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Equipment.Weapon.Weapon_Dagger
{
	[Serializable]
	public class WH_Dagger_ButcherKnife : WeaponHandler_MultiAttack
	{

		/// <summary>
		/// 累积的暴击次数
		/// </summary>
		private int _criticalAccumulate;
		
		[SerializeField,LabelText("减少技能CD的累积次数")]
		private int _reduceSkillCDCount =12;

		[SerializeField, LabelText("可以累积的版面UID列表")]
		private List<string> layoutUID_CanAccumulateList = new List<string>();
		 
		[SerializeField,LabelText("减少CD的技能列表")]
		 private List<RPSkill_SkillTypeEnum> _reduceCDSkillList = new List<RPSkill_SkillTypeEnum>();

		[SerializeField, LabelText("减少的CD百分比"), SuffixLabel("%")]
		private float _cdToReducePartial = 50;

		public override void InitializeOnInstantiate(
			PlayerARPGConcreteCharacterBehaviour behaviour,
			LocalActionBus lab,
			SOConfig_WeaponTemplate configRuntime,
			DamageTypeEnum damageType)
		{
			base.InitializeOnInstantiate(behaviour, lab, configRuntime, damageType);
			behaviour.GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
					_ABC_RecordCritical);
		}


		private void _ABC_RecordCritical(DS_ActionBusArguGroup ds)
		{
			var rpds_dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
		
			//包含了那个UID
			if (rpds_dar.RelatedProjectileRuntimeRef != null && layoutUID_CanAccumulateList.Contains(
				rpds_dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference.LayoutContentInSO.LayoutUID))
			{
				_criticalAccumulate += 1;
				CheckAndReduceCD(); 
				
				//暴击额外加1；
				if (rpds_dar.IsDamageCauseCritical)
				{
					_criticalAccumulate += 1;
					CheckAndReduceCD();
				}
			}

		}

		private List<SOConfig_RPSkill> _findSkillList = new List<SOConfig_RPSkill>();


		private void CheckAndReduceCD()
		{
			if(_criticalAccumulate > _reduceSkillCDCount)
			{
				_criticalAccumulate = 0;
				RelatedCharacterBehaviourRef.GetRelatedSkillHolder().GetCurrentSkillList(_findSkillList);
				foreach (SOConfig_RPSkill perSkill in _findSkillList)
				{
					if (_reduceCDSkillList.Contains(perSkill.ConcreteSkillFunction.SkillType))
					{
						perSkill.ConcreteSkillFunction.ModifyRemainingCD(true,
							(_cdToReducePartial / 100f) * perSkill.ConcreteSkillFunction.CurrentCoolDownDuration);
					}
				}
			}
		}



		public override void UnloadAndClear()
		{
			base.UnloadAndClear();
			RelatedCharacterBehaviourRef.GetRelatedActionBus().RemoveAction( ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
				_ABC_RecordCritical);
		}
	}
	
	
	
}