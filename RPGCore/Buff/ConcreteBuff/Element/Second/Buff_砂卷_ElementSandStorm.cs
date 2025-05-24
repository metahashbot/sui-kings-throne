using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Buff.BuffComponent;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Element.Second
{
	[TypeInfoBox("奥术反应——【砂卷】 = 风 + 土")]
	[Serializable]
	public class Buff_砂卷_ElementSandStorm : BaseRPBuff, I_BuffAsElementSecond, I_BuffAsElementArcane,
		I_BuffContainLoopVFX
	{
		[SerializeField, LabelText("附加的【风】属性伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		private float _windDamageBonusRatio = 50f;

		[SerializeField, LabelText("附加的【土】属性伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		private float _earthDamageBonusRatio = 50f;

		[SerializeField, LabelText("降低攻速百分比")]
		[TitleGroup("===基本配置===")]
		private float _attackSpeedReduceRatio = 50f;

		[SerializeField, LabelText("降低投射物飞行速度百分比")]
		[TitleGroup("===基本配置===")]
		private float _projectileSpeedReducePartial;



		private Float_RPDataEntry _entry_AttackSpeed;
		private Float_ModifyEntry_RPDataEntry _modify_AttackSpeed;



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
			_entry_AttackSpeed = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			_modify_AttackSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_attackSpeedReduceRatio,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.RearMul,
				this);

			_entry_AttackSpeed.AddDataEntryModifier(_modify_AttackSpeed);
		}


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
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
				DamageTypeEnum.AoNengTu_奥能土,
				damageBLP.RelatedDamageResultInfoRef.DamageResult_TakenOnHP * _earthDamageBonusRatio / 100f,
				DamageProcessStepOption.AppendDamageDPS());
			_damageAssistServiceRef.ApplyDamage(newDamage_huo).ReleaseToPool();
			return ds;
		}



		public override DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
		{
			ResetDurationAndAvailableTimeAs(SelfConfigInstance.OriginalBuffConfigTemplate.ConcreteBuffFunction
				.BuffInitAvailableTime ,
				SelfConfigInstance.OriginalBuffConfigTemplate.ConcreteBuffFunction.BuffInitAvailableTime);
			return base.OnExistBuffRefreshed(caster, blps);
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();
			if (_entry_AttackSpeed.ModifyListContains(_modify_AttackSpeed))
			{
				_entry_AttackSpeed.RemoveEntryModifier(_modify_AttackSpeed);
				_modify_AttackSpeed.ReleaseToPool();
				
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