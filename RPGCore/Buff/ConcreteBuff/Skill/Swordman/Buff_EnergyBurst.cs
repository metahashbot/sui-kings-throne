using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGCore.Buff.ConcreteBuff.Skill.Swordman
{
	[Serializable]
	[TypeInfoBox("监听一个 L_Skill_OnSKillReadyToConsumeSP_技能将要消耗SP")]
	public class Buff_EnergyBurst : BaseRPBuff
	{
		[LabelText("攻击力提高的比率，25表示提升25%")] [ShowInInspector, FoldoutGroup("运行时/来自技能", true)]
		public float AttackPowerBonus = 25;

		[LabelText("攻击力提高的算区，默认前乘")] [ShowInInspector, FoldoutGroup("运行时/来自技能", true)]
		public ModifyEntry_CalculatePosition AttackPowerBonusCalculatePosition = ModifyEntry_CalculatePosition.FrontMul;


		[LabelText("技能CD减少量")] [ShowInInspector, FoldoutGroup("运行时/来自技能", true),SuffixLabel("%")]
		public float SkillCDReducePartial = 80f;

		[SerializeField, LabelText("VFXConfig_跟随特效"), FoldoutGroup("配置", true)]
		protected string _VFXConfig_Follow;

		[SerializeField, LabelText("VFXConfig_眼睛上的"), FoldoutGroup("配置", true)]
		protected string _VFXConfig_EyeFollow;
		

		[SerializeField, LabelText("特效挂的骨骼配置名"), GUIColor(206f / 255f, 177f / 255f, 227f / 255f),
		 FoldoutGroup("配置", true)]
		protected string _bfName_EyeBone;
		

		private Float_RPDataEntry _attackPowerEntry;

		private Float_ModifyEntry_RPDataEntry _modify_AttackPower;

		private Float_RPDataEntry _cdReduceEntry;

		private Float_ModifyEntry_RPDataEntry _modify_CDReduce;



		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_attackPowerEntry = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
			_modify_AttackPower = Float_ModifyEntry_RPDataEntry.GetNewFromPool(AttackPowerBonus,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				AttackPowerBonusCalculatePosition);
			_attackPowerEntry.AddDataEntryModifier(_modify_AttackPower);

			_cdReduceEntry = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.SkillCDReduce_技能CD缩减);
			_modify_CDReduce = Float_ModifyEntry_RPDataEntry.GetNewFromPool(SkillCDReducePartial / 100f,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontAdd,
				this);
			_cdReduceEntry.AddDataEntryModifier(_modify_CDReduce);

		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			// var vfx_follow = _VFX_GetOrInstantiateNew(_VFXConfig_Follow)._VFX_2_SetAnchorToOnlyScaleVFXHolder(this)
			// 	._VFX__10_PlayThis();
			// PerVFXInfo vfx_eye = _VFX_GetOrInstantiateNew(_VFXConfig_EyeFollow)._VFX__10_PlayThis();
			// var vfxHolder = Parent_SelfBelongToObject.GetRelatedVFXContainer()
			// 	.GetVFXHolderTransformAndRegisterVFX(_bfName_EyeBone, vfx_eye.CurrentActiveRuntimePSPlayProxyRef.gameObject);
			// vfx_eye.CurrentActiveRuntimePSPlayProxyRef.transform.SetParent(vfxHolder.Item1);
			// vfx_eye.CurrentActiveRuntimePSPlayProxyRef.transform.localPosition = Vector3.back * 0.45f;
			// vfx_eye.CurrentActiveRuntimePSPlayProxyRef.transform.localScale = Vector3.one * 1.75f;
			
		

			return base.OnBuffInitialized(blps);
		}


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			_modify_CDReduce.ModifyValue = SkillCDReducePartial / 100f;
			_cdReduceEntry.Recalculate();
			return base.OnExistBuffRefreshed(caster, blps);
		}
		public override void GeneralActionBusCallback(DS_ActionBusArguGroup ds)
		{
			if (ds.ActionType == ActionBus_ActionTypeEnum.L_Skill_OnSKillReadyToConsumeSP_技能将要消耗SP)
			{
				var targetSkill = ds.ObjectArgu1 as BaseRPSkill;
				targetSkill.CurrentSPConsume = 0f;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			_attackPowerEntry.RemoveEntryModifier(_modify_AttackPower);
			_modify_AttackPower.ReleaseToPool();
			_cdReduceEntry.RemoveEntryModifier(_modify_CDReduce);
			_modify_CDReduce.ReleaseToPool()
			;
			return base.OnBuffPreRemove();
		}
	}
}