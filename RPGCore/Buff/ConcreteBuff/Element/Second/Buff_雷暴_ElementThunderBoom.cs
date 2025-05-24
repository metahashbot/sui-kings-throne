using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Element.Second
{
	[TypeInfoBox("聚变反应——【雷暴】 = 电 + 火")]
	[Serializable]
	public class Buff_雷暴_ElementThunderBoom : BaseRPBuff , I_BuffAsElementSecond , I_BuffAsElementFusion,
		I_BuffContainOnetimeVFXOnInitializeOrRefreshed
	{

		[SerializeField, LabelText("追加的【电】伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		protected float _electricDamageBonusRatio = 50f;

		[SerializeField, LabelText("追加的【火】伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		protected float _fireDamageBonusRatio = 50f;



		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			(this as I_BuffContainOnetimeVFXOnInitializeOrRefreshed).SpawnVFX();
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			MarkAsRemoved = true;
			(this as I_BuffContainOnetimeVFXOnInitializeOrRefreshed).SpawnVFX();
			var damageBLP = blps.Find(x => x is BLP_FromDamage) as BLP_FromDamage;
			if (damageBLP == null)
			{
				Debug.LogError("Buff_雷暴_ElementThunderBoom: OnBuffInitialized: 未找到伤害信息");
				return ds;
			}

			var newDamage_dian = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(
				Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
				damageBLP.RelatedDamageResultInfoRef.Caster,
				DamageTypeEnum.YuanNengDian_源能电,
				damageBLP.RelatedDamageResultInfoRef.DamageResult_TakenOnHP * _electricDamageBonusRatio / 100f,
				DamageProcessStepOption.AppendDamageDPS());
			_damageAssistServiceRef.ApplyDamage(newDamage_dian).ReleaseToPool();
			var newDamage_huo = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(
				Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
				damageBLP.RelatedDamageResultInfoRef.Caster,
				DamageTypeEnum.AoNengHuo_奥能火,
				damageBLP.RelatedDamageResultInfoRef.DamageResult_TakenOnHP * _fireDamageBonusRatio / 100f,
				DamageProcessStepOption.AppendDamageDPS());
			_damageAssistServiceRef.ApplyDamage(newDamage_huo).ReleaseToPool();
			return ds;
		}



#region 接口

		private GameObject _vfxObject;
		[SerializeField, LabelText("单次特效UID"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===基本配置===")]
		private string _vfxUid;

#endregion
		string I_BuffContainOnetimeVFXOnInitializeOrRefreshed.vfxUID
		{
			get => _vfxUid;
			set => _vfxUid = value;
		}
	}
}