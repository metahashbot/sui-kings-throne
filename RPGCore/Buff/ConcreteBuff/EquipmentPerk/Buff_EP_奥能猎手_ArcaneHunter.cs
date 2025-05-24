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
	public class Buff_EP_奥能猎手_ArcaneHunter : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{

		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("水火风土伤害增加百分比，于伤害信息构建时"), SuffixLabel("%")]
			public float DamageBonusPartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedAddDamage);
			return base.OnBuffInitialized(blps);
		}


		public float CurrentDamageBonus { get; private set; }



		private void _ABC_CheckIfNeedAddDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar == null)
			{
				return;
			}
			if (dar.DamageType != DamageTypeEnum.AoNengFeng_奥能风 && dar.DamageType != DamageTypeEnum.AoNengHuo_奥能火 &&
			    dar.DamageType != DamageTypeEnum.AoNengShui_奥能水 && dar.DamageType != DamageTypeEnum.AoNengTu_奥能土)
			{
				return;
			}
			dar.CP_DamageAmount_DPart.MultiplyPart += (CurrentDamageBonus / 100f);
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					CurrentDamageBonus = buffData.DamageBonusPartial;
					break;
			}
		}
		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedAddDamage);
			return base.OnBuffPreRemove();
		}

	}
}