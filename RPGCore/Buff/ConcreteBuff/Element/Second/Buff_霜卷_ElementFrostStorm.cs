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
	[TypeInfoBox(" 奥术反应——霜卷【风+水】，")]
	[Serializable]
	public class Buff_霜卷_ElementFrostStorm : BaseRPBuff, I_BuffAsElementSecond, I_BuffAsElementArcane,
		I_BuffContainLoopVFX
	{
		[SerializeField, LabelText("附加的【风】属性伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		private float _windDamageBonusRatio = 50f;

		[SerializeField, LabelText("附加的【水】属性伤害相较于此次伤害的百分比")]
		[TitleGroup("===基本配置===")]
		private float _waterDamageBonusRatio = 50f;

		[SerializeField, LabelText("降低移速百分比")]
		[TitleGroup("===基本配置===")]
		private float _moveSpeedReducePartial = 50f;



		private Float_RPDataEntry _entry_MoveSpeed;
		private Float_ModifyEntry_RPDataEntry _modify_moveSpeed;



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
			_entry_MoveSpeed = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			_modify_moveSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(_moveSpeedReducePartial,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.RearMul,
				this);
			_entry_MoveSpeed.AddDataEntryModifier(_modify_moveSpeed);
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			(this as I_BuffContainLoopVFX).SpawnVFX();
			var damageBLP = blps.Find(x => x is BLP_FromDamage) as BLP_FromDamage;
			if (damageBLP == null)
			{
				Debug.LogError("Buff-霜卷 : OnBuffInitialized: 未找到伤害信息");
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
				DamageTypeEnum.AoNengShui_奥能水,
				damageBLP.RelatedDamageResultInfoRef.DamageResult_TakenOnHP * _waterDamageBonusRatio / 100f,
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
			if (_entry_MoveSpeed.ModifyListContains(_modify_moveSpeed))
			{
				_entry_MoveSpeed.RemoveEntryModifier(_modify_moveSpeed);
				_modify_moveSpeed.ReleaseToPool();
				;
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