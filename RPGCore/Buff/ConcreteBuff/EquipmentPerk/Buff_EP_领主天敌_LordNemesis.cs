using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_领主天敌_LordNemesis : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{

		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("对领主伤害的修正百分比"), SuffixLabel("%")]
			public float DamageBonusPartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedAddDamage);
			return base.OnBuffInitialized(blps);
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentDamageOffset = buffData.DamageBonusPartial;
					break;
			}
		}

		[ShowInInspector, LabelText("当前伤害修正")]
		public float CurrentDamageOffset { get; private set; }

		private void _ABC_CheckIfNeedAddDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar == null)
			{
				return;
			}
			if (dar.Receiver.ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) !=
			    BuffAvailableType.NotExist)
			{
				dar.CP_DamageAmount_DPart.MultiplyPart += CurrentDamageOffset / 100f;
			}
		}


		private void _ABC_CheckIfNeedReduceDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar == null)
			{
				return;
			}
			if (dar.Caster?.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) !=
			    BuffAvailableType.NotExist)
			{
				dar.CP_DamageAmount_DPart.MultiplyPart -= CurrentDamageOffset / 100f;
			}
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedAddDamage);
			return base.OnBuffPreRemove();
		}


	}
}