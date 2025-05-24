using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_蝮蛇吐息_ViperBreath : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{


		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("每秒造成的当前生命值百分比"), SuffixLabel("%")]
			public float PerSecondDamagePartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
				_ABC_CheckIfAppendDamage);
			return base.OnBuffInitialized(blps);
		}
		public float AppendPartial { get; private set; }


		private void _ABC_CheckIfAppendDamage(DS_ActionBusArguGroup ds)
		{
			var darGlobal = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			//是不是自己？
			if (darGlobal.Caster == null || (darGlobal.Caster != Parent_SelfBelongToObject))
			{
				return;
			}
			var poisoning = darGlobal.Receiver.ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum.Poisoning_中毒);
			Buff_中毒_poisoning BuffPoisoning = null;
			Buff_中毒_poisoning.BLP_Poisoning blp_poi = new Buff_中毒_poisoning.BLP_Poisoning();
			blp_poi.NotLessDuration = 5f;
			blp_poi.CurrentHPPartial = AppendPartial;
			var t = (darGlobal.Receiver as I_RP_Buff_ObjectCanReceiveBuff).ReceiveBuff_TryApplyBuff(
				RolePlay_BuffTypeEnum.Poisoning_中毒,
				darGlobal.Caster as I_RP_Buff_ObjectCanApplyBuff,
				darGlobal.Receiver as I_RP_Buff_ObjectCanReceiveBuff,
				blp_poi);
		}




		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					AppendPartial = buffData.PerSecondDamagePartial;
					break;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			GlobalActionBus.GetGlobalActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
				_ABC_CheckIfAppendDamage);
			return base.OnBuffPreRemove();
		}

		

	}
}