using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.EquipmentPerk
{
	[Serializable]
	public class Buff_EP_钢牙撕裂_SteelFangTear : BaseRPBuff, I_BuffCanAsEquipmentPerk
	{

		[Serializable]
		public struct BuffData
		{
			[SerializeField, LabelText("Perk等级")]
			public int Level;
			[SerializeField, LabelText("降低防御百分比"), SuffixLabel("%")]
			public float DefenseReducePartial;
			
			[SerializeField,LabelText("叠层")]
			public int StackCount;
			 [SerializeField,LabelText("持续时长")]
			public float Duration;
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
		public float EffectDuration { get; private set; }
		public int StackCountOverride { get; private set; }



		private void _ABC_CheckIfAppendDamage(DS_ActionBusArguGroup ds)
		{
			var darGlobal = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			//是不是自己？
			if (darGlobal.Caster == null || (darGlobal.Caster != Parent_SelfBelongToObject))
			{
				return;
			}

			BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify blp_modifyOnDefense =
				new BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify();
			blp_modifyOnDefense.TargetUID = Parent_SelfBelongToObject.ToString() + "钢牙撕裂";
			blp_modifyOnDefense.TargetEntry = RP_DataEntry_EnumType.Defense_防御力;
			blp_modifyOnDefense.ModifyDuration = EffectDuration;
			blp_modifyOnDefense.ModifyValue = AppendPartial;
			blp_modifyOnDefense.CalculatePosition = ModifyEntry_CalculatePosition.FrontAdd;
			blp_modifyOnDefense.OverrideStack = true;
			blp_modifyOnDefense.OverrideStackAs = StackCountOverride;

			(darGlobal.Receiver as I_RP_Buff_ObjectCanReceiveBuff).ReceiveBuff_TryApplyBuff(
				RolePlay_BuffTypeEnum.CommonDataEntryModifyStackable_通用数据项修正可叠层,
				darGlobal.Caster as I_RP_Buff_ObjectCanApplyBuff,
				darGlobal.Receiver as I_RP_Buff_ObjectCanReceiveBuff,
				blp_modifyOnDefense);
				
			
		}



		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_作为Perk的等级_ActAsPerkLevel perkLevel:
					//find by level
					BuffData buffData = BuffDataList.Find(x => x.Level == perkLevel.Level);
					AppendPartial = buffData.DefenseReducePartial;
					EffectDuration = buffData.Duration;
					StackCountOverride = buffData.StackCount;
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