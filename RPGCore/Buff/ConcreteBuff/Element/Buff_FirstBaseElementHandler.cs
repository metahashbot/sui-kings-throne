using System;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
namespace RPGCore.Buff.ConcreteBuff.Element
{
	/// <summary>
	/// <para>对于RPBehaviour和RPDispatcher，用于处理带有元素的伤害时增加元素反应相关的buff(Tag)</para>
	/// </summary>
	[Serializable]
	[TypeInfoBox("需要监听:\n" + " L_Buff_OnBuffPreAdd_一个Buff将要被添加 \n" + " L_Damage_OnDamageBeforeShield_伤害开始与护盾计算之前 \n")]
	public class Buff_FirstBaseElementHandler : BaseRPBuff
	{
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
				_ABC_ProcessOnDamageBeforeTaken);
		}




		private void _ABC_ProcessOnDamageBeforeTaken(DS_ActionBusArguGroup ds)
		{
			BLP_FromDamage currentBLP = GenericPool<BLP_FromDamage>.Get();
			RP_DS_DamageApplyResult damageApplyResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			I_RP_Buff_ObjectCanReceiveBuff receiver = damageApplyResult.Receiver as I_RP_Buff_ObjectCanReceiveBuff;
			I_RP_Buff_ObjectCanApplyBuff caster = damageApplyResult.Caster as I_RP_Buff_ObjectCanApplyBuff;
			;
			currentBLP.RelatedDamageResultInfoRef = damageApplyResult;
			switch (damageApplyResult.DamageType)
			{
				case DamageTypeEnum.YuanNengGuang_源能光:
					receiver.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ElementFirstLightTag_Guang_一级光标签,
						caster,
						receiver,
						currentBLP);
					break;
				case DamageTypeEnum.YuanNengDian_源能电:
					receiver.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ElementFirstElectricTag_Dian_一级电标签,
						caster,
						receiver,
						currentBLP);
					break;
				case DamageTypeEnum.AoNengTu_奥能土:
					receiver.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ElementFirstEarthTag_Tu_一级土标签,
						caster,
						receiver,
						currentBLP);

					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					receiver.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ElementFirstWaterTag_Shui_一级水标签,
						caster,
						receiver,
						currentBLP);
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					receiver.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ElementFirstFireTag_Huo_一级火标签,
						caster,
						receiver,
						currentBLP);
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					receiver.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ElementFirstWindTag_Feng_一级风标签,
						caster,
						receiver,
						currentBLP);
					break;
				case DamageTypeEnum.LingNengLing_灵能灵:
					receiver.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ElementFirstSoulTag_Ling_一级灵标签,
						caster,
						receiver,
						currentBLP);
					break;
			}

			GenericPool<BLP_FromDamage>.Release(currentBLP);

		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeFrontAdd_对接收方将要进行前加攻防算区计算,
				_ABC_ProcessOnDamageBeforeTaken);
			return base.OnBuffPreRemove();
		}
	}
}