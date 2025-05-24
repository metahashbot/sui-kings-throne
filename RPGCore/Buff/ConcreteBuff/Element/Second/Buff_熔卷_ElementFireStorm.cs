using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Buff.BuffComponent;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Element.Second
{
	[TypeInfoBox("二级反应——熔卷【风+火】，奥术反应")]
	[Serializable]
	public class Buff_熔卷_ElementFireStorm : BaseRPBuff, I_BuffAsElementSecond, I_BuffAsElementArcane, I_BuffContainLoopVFX
	{
		[SerializeField,LabelText("附加的风属性伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		private float _windDamageBonusRatio = 50f;
		
		[SerializeField,LabelText("附加的火属性伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		private float _fireDamageBonusRatio = 50f;
		
		[SerializeField,LabelText("降低防御力百分比")]
		[TitleGroup("===基本配置===")]
		private float _defenseReduceRatio = 50f;
		
		

		private Float_RPDataEntry _entry_defense;
		private Float_ModifyEntry_RPDataEntry _modify_defense;
		


		[SerializeField, LabelText("循环特效UID"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===基本配置===")]
		private string _vfxUid;
		private PerVFXInfo _relatedVFXInfo;
		[SerializeField, LabelText("特效停止时是立刻的吗？")]
		[TitleGroup("===基本配置===")]
		private bool _stopImmediate;

		
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_entry_defense = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.Defense_防御力);
			_modify_defense = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_defenseReduceRatio,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.RearMul,
				this);
			_entry_defense.AddDataEntryModifier(_modify_defense);
			
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds= base.OnBuffInitialized(blps);
			(this as I_BuffContainLoopVFX).SpawnVFX();
			var damageBLP = blps.Find(x => x is BLP_FromDamage) as BLP_FromDamage;
			if (damageBLP == null)
			{
				Debug.LogError("Buff-熔卷 : OnBuffInitialized: 未找到伤害信息");
				return ds;
			}
			var newDamage_dian = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(
				Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
				damageBLP.RelatedDamageResultInfoRef.Caster,
				DamageTypeEnum.AoNengFeng_奥能风,
				damageBLP.RelatedDamageResultInfoRef.DamageResult_TakenOnHP * _windDamageBonusRatio / 100f,
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


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			ResetDurationAndAvailableTimeAs(
				SelfConfigInstance.OriginalBuffConfigTemplate.ConcreteBuffFunction.BuffInitAvailableTime,
				SelfConfigInstance.OriginalBuffConfigTemplate.ConcreteBuffFunction.BuffInitAvailableTime);
			return base.OnExistBuffRefreshed(caster, blps);
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();
			if (_entry_defense.ModifyListContains(_modify_defense))
			{ 
				_entry_defense.RemoveEntryModifier(_modify_defense);
				_modify_defense.ReleaseToPool();
				_modify_defense = null;
			}
			 
			return ds;
		}


		public override void VFX_GeneralClear(bool StopAndClear = false)
		{
			(this as I_BuffContainLoopVFX).StopVFX();
		}
		string I_BuffContainLoopVFX.vfxUID
		{
			get => _vfxUid;
			set => _vfxUid = value;
		}
		PerVFXInfo I_BuffContainLoopVFX.relatedVFXInfo
		{
			get => _relatedVFXInfo;
			set => _relatedVFXInfo = value;
		}
		bool I_BuffContainLoopVFX.stopImmediate
		{
			get => _stopImmediate;
			set => _stopImmediate = value;
		}
	}
}