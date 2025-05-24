using System;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Skill.Swordman
{
	/// <summary>
	/// 来自技能的剑心效果。对应技能——看破
	/// </summary>
	[Serializable]
	public class Buff_FromSkill_来自技能剑心_HeartOfSword : BaseRPBuff
	{
		[SerializeField, LabelText("降低技能CD的百分比"), TitleGroup("===数值==="), SuffixLabel("%")]
		private float _reduceCDPercent = 20f;

		[SerializeField, LabelText("提升的闪避率百分比"), TitleGroup("===数值==="), SuffixLabel("%")]
		private float _increaseDodgePercent = 20f;

		[SerializeField, LabelText("提升的攻速百分比"), TitleGroup("===数值==="), SuffixLabel("%")]
		private float _increaseAttackSpeedPercent = 20f;
		
		[SerializeField,LabelText("vfxid-常驻VFX") ,TitleGroup("===VFX==="), GUIColor(187f / 255f, 1f, 0f)]
		private string _VFX_Permanent;


		private Float_ModifyEntry_RPDataEntry _modify_Dodge;
		private Float_RPDataEntry _entry_DodgeRef;

		private Float_ModifyEntry_RPDataEntry _modify_AttackSpeed;
		private Float_RPDataEntry _entry_AttackSpeedRef;




		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_Skill_OnSkillCoolDownReset_技能重新开始计CD了, _ABC_ReduceCDPartial);

			_entry_DodgeRef = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.DodgeRate_闪避率);
			_modify_Dodge = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_increaseDodgePercent,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul,
				this);
			_entry_DodgeRef.AddDataEntryModifier(_modify_Dodge);
			_entry_AttackSpeedRef = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			_modify_AttackSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_increaseAttackSpeedPercent,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul,
				this);

			_VFX_GetAndSetBeforePlay(_VFX_Permanent,  true, false)?._VFX__10_PlayThis();

		}



		protected void _ABC_ReduceCDPartial(DS_ActionBusArguGroup ds)
		{
			var skillRef = ds.GetObj1AsT<BaseRPSkill>();
			skillRef.CurrentCoolDownDuration = skillRef.CoolDownTime * (1f - _reduceCDPercent / 100f);
		}




		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			
			_entry_DodgeRef.RemoveEntryModifier(_modify_Dodge);
			_modify_Dodge.ReleaseToPool();
			_entry_AttackSpeedRef.RemoveEntryModifier(_modify_AttackSpeed);
			_modify_AttackSpeed.ReleaseToPool();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.L_Skill_OnSkillCoolDownReset_技能重新开始计CD了, _ABC_ReduceCDPartial);
			_VFX_JustGet(_VFX_Permanent)?.VFX_StopThis();

			return base.OnBuffPreRemove();
		}

	}
}