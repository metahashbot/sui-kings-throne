using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_薄暮_Twilight : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{
		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("减少CD比率,填50就是少于50%时生效"), SuffixLabel("%")]
			public float CDReducePercent;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		private List<SOConfig_RPSkill> _skillList = new List<SOConfig_RPSkill>();
		public float CurrentRestorePartial { get; private set; }


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			RPSkill_SkillHolder ss = (Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour)
				.GetRelatedSkillHolder();
			ss.GetCurrentSkillList(_skillList);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSkillDefaultFinish_技能默认结束了,
				_ABC_TryReduceSkillCD_OnSKillNormalReleaseEnd);
			return base.OnBuffInitialized(blps);
		}

		private void _ABC_TryReduceSkillCD_OnSKillNormalReleaseEnd(DS_ActionBusArguGroup ds)
		{
			var dsSkill = ds.ObjectArgu1 as BaseRPSkill;
			RPSkill_SkillTypeEnum skillType = dsSkill.SelfSkillConfigRuntimeInstance.ConcreteSkillFunction.SkillType;

			foreach (SOConfig_RPSkill perSkillRuntime in _skillList)
			{
				if (perSkillRuntime.ConcreteSkillFunction.SkillType == skillType)
				{
					continue;
				}
				float m = perSkillRuntime.ConcreteSkillFunction.CurrentCoolDownDuration;
				float partial = m * (CurrentRestorePartial / 100f);

				perSkillRuntime.ConcreteSkillFunction.ModifyRemainingCD(true, -partial); 

			}
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentRestorePartial = buffData.CDReducePercent;
					break;
			}
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSkillDefaultFinish_技能默认结束了,
				_ABC_TryReduceSkillCD_OnSKillNormalReleaseEnd);

			return base.OnBuffPreRemove();
		}





	}
}