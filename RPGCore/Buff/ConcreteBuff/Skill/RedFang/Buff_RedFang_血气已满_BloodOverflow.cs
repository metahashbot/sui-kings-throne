using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Skill.RedFang
{
	[Serializable]
	public class Buff_RedFang_血气已满_BloodOverflow : BaseRPBuff
	{


		[SerializeField, LabelText("血气已满的特效"), TitleGroup("===具体配置==="), GUIColor(187f / 255f, 1f, 0f)]
		private string _vfxid;
		
		






		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSkillBeginPrepare_技能开始进行准备,
				_ABC_TryRemoveBuff_OnReleaseSkill);
		}

		private void _ABC_TryRemoveBuff_OnReleaseSkill(DS_ActionBusArguGroup ds)
		{
			var targetSkill = ds.GetObj1AsT<BaseRPSkill>();

			if (targetSkill.SkillType == RPSkill_SkillTypeEnum.RedFang_BloodSacrifice_A1_红牙猩红血池 ||
			    targetSkill.SkillType == RPSkill_SkillTypeEnum.RedFang_SickleSlash_A1_红牙猩红镰刃)
			{
				//如果在此期间，大招开着的，就不移除自己
				if (Parent_SelfBelongToObject.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
					.FromSkill_RedFang_ScarletDarkness_猩月黯影) == BuffAvailableType.Available_TimeInAndMeetRequirement)
				{
				}
				else
				{
					MarkAsRemoved = true;
					Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.BloodGasValue_血气值,
						Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
						Parent_SelfBelongToObject);
				}
			}
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Skill_OnSkillBeginPrepare_技能开始进行准备,
				_ABC_TryRemoveBuff_OnReleaseSkill);
			return base.OnBuffPreRemove();
		}


	}
}