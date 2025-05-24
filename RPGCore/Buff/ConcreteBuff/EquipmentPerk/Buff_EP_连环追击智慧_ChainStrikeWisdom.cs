using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_连环追击智慧_ChainStrikeWisdom : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{

		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("追加智慧百分比,40就是智慧的40%"), SuffixLabel("%")]
			public float TrueDamageAppendPartial;
		}

		[SerializeField, LabelText("逐等级数据"), ListDrawerSettings(ShowFoldout = true, AddCopiesLastElement = true),
		 FoldoutGroup("配置", true)]
		public List<BuffData> BuffDataList = new List<BuffData>();



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_CheckIfAppendDamage);
			_entry_INT = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.M_Intellect_主智力);
			return base.OnBuffInitialized(blps);
		}
		public float AppendPartial { get; private set; }

		private Float_RPDataEntry _entry_INT;



		private void _ABC_CheckIfAppendDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			float value = _entry_INT.CurrentValue * (AppendPartial / 100f);
			var getNewDAI = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(dar.Receiver,
				dar.Caster,
				DamageTypeEnum.TrueDamage_真伤,
				value,
				DamageProcessStepOption.TrueDamageDPS());
			dar.ProcessOption = DamageProcessStepOption.TrueDamageDirectValueDPS();
			dar.ProcessOption.IgnoreRearMulPart_FinalBonus = true;
			getNewDAI.DamageTimestamp = BaseGameReferenceService.CurrentFixedFrame + GetHashCode();
			getNewDAI.DamageWorldPosition = dar.DamageWorldPosition;
			_damageAssistServiceRef.ApplyDamage(getNewDAI).ReleaseToPool();
		}



		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					AppendPartial = buffData.TrueDamageAppendPartial;
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