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
	public class Buff_EP_护甲穿透_ArmorPiercing : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{

		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("防御穿透量,40就是无视40%的防御"), SuffixLabel("%")]
			public float DefenseReducePartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public float CurrentReducePartial { get; private set; }
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeFrontAdd_对施加方将要进行前加攻防算区计算,
				_ABC_CheckAndAppendDefenseIgnore);
			return base.OnBuffInitialized(blps);
		}


		private void _ABC_CheckAndAppendDefenseIgnore(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			dar.CP_APart_DefenseIgnore.AddonPart += 1;
			dar.CP_APart_DefenseIgnore.MultiplyPart += CurrentReducePartial;
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);

					CurrentReducePartial = buffData.DefenseReducePartial;
					break;
			}
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeFrontAdd_对施加方将要进行前加攻防算区计算,
				_ABC_CheckAndAppendDefenseIgnore);
			return base.OnBuffPreRemove();
		}



	}
}