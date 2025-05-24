using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	[TypeInfoBox("目前兼容：WAR_所有伤害的百分比调整")]
	public class Buff_EP_苦痛打击_PainStrike : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{


		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("弱点积累增加比率,40就是比之前多40%"), SuffixLabel("%")]
			public float WeaknessAppend;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Damage_WeaknessEffectOnTakenAccumulate_弱点将要积累弱点槽时,
				_ABC_CheckIfNeedAppendWAR);
			return base.OnBuffInitialized(blps);
		}


		public float CurrentWeaknessAppend { get; private set; }

		private void _ABC_CheckIfNeedAppendWAR(DS_ActionBusArguGroup ds)
		{
			if (ds.ObjectArgu1 is WRListen_所有伤害的百分比调整_ModifyByAllDamageByPartial war)
			{
				war.Cache_CurrentAccumulate = 1f * (CurrentWeaknessAppend / 100f);
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
					CurrentWeaknessAppend = buffData.WeaknessAppend;
					break;
			}
		}




		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Damage_WeaknessEffectOnTakenAccumulate_弱点将要积累弱点槽时,
				_ABC_CheckIfNeedAppendWAR);
			return base.OnBuffPreRemove();
		}

	}
}