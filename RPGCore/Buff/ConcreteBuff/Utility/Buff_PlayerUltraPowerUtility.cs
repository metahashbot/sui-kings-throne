using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Enemy;
using Global;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Utility
{
	[Serializable]
	[TypeInfoBox("玩家通用的UP处理，包括同步不同角色之间的UP值、在UP满的时候触发事件")]
	public class Buff_PlayerUltraPowerUtility : BaseRPBuff
	{
		
		
		[SerializeField,LabelText("配置-每次造成伤害恢复这么多s的CD"),TitleGroup("===配置===")]
		public float  _Template_CoolDownPerHit = 0.1f;
		
		[NonSerialized,ShowInInspector,LabelText("当前-每次造成伤害恢复这么多s的CD"),TitleGroup("===运行时===")]
		protected float _Current_CoolDownPerHit;
		
		[SerializeField,LabelText("超能拾取恢复倍率"),TitleGroup("===配置===")]
		public float _Template_PickUpRecoverRate = 1f;
		
		[NonSerialized,ShowInInspector,LabelText("当前-超能拾取恢复倍率"),TitleGroup("===运行时===")]
		protected float _Current_PickUpRecoverRate;
		
		
		
		

		[NonSerialized]
		protected PlayerARPGConcreteCharacterBehaviour _selfBehaviourRef;
		[NonSerialized]
		protected BaseRPSkill _selfUPSkill;

		protected BaseRPSkill GetSelfUPSkill()
		{
			if (_selfUPSkill!=null)
			{
				return _selfUPSkill;
			}
			_selfUPSkill = (Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour).GetRelatedSkillHolder()
				.GetSkillOnSlot(SkillSlotTypeEnum.UltraSkill_超杀槽位).ConcreteSkillFunction;
			return _selfUPSkill;
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_Current_PickUpRecoverRate = _Template_PickUpRecoverRate;
			_Current_CoolDownPerHit = _Template_CoolDownPerHit;

			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
					_ABC_ReduceOnHit_OnDamageTakenOnHP);


			_selfBehaviourRef = Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour;
			
			return ds;
		}


		private void _ABC_ReduceOnHit_OnDamageTakenOnHP(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar == null)
			{
				return;
			}
			if (dar.Receiver is not EnemyARPGCharacterBehaviour enemy)
			{
				return;
			}
			GetSelfUPSkill().ModifyRemainingCD(true, -_Current_CoolDownPerHit); ;
		}


		/// <summary>
		/// partial	是个0~1的小数
		/// </summary>
		/// <param name="partial"></param>
		public void RestoreUltraPowerCDByPartial(float partial)
		{
			float oCD = GetSelfUPSkill().CurrentCoolDownDuration;
			GetSelfUPSkill().ModifyRemainingCD(true,-oCD * partial * _Current_PickUpRecoverRate); ;
		}

		public void RestoreUltraPowerCDByCount(float amount)
		{

			GetSelfUPSkill().ModifyRemainingCD(true,-amount * _Current_PickUpRecoverRate);
		}


		protected override void ClearAndUnload()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
				_ABC_ReduceOnHit_OnDamageTakenOnHP);

			base.ClearAndUnload();
		}
	}
}