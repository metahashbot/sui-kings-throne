using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_风暴卫_StormDefense : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{

		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("风伤害减少百分比，于抵抗计算后,过盾计算前"), SuffixLabel("%")]
			public float DamageReducePartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();


		public float CurrentDamageReduce { get; private set; }


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			return base.OnBuffInitialized(blps);
		}
		private void _ABC_CheckIfNeedReduceDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar == null)
			{
				return;
			}
			if (dar.DamageType != DamageTypeEnum.AoNengFeng_奥能风)
			{
				return;
			}
			dar.CP_DamageAmount_DPart.MultiplyPart -= (CurrentDamageReduce / 100f);
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentDamageReduce = buffData.DamageReducePartial;
					break;
			}
		}
		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			return base.OnBuffPreRemove();
		}
	}
}