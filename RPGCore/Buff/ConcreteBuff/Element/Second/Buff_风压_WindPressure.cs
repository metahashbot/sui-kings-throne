using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Element.Second
{
	[TypeInfoBox("增幅反应。【风压】 = 灵 + 风，增幅风")]
	[Serializable]
	public class Buff_风压_WindPressure : BaseRPBuff, I_BuffAsElementSecond, I_BuffAsElementAmplification,
		I_BuffContainLoopVFX
	{

#region 配置

		[LabelText("增加【风】伤害的比率(50表示增加50%的伤害)")]
		[TitleGroup("===基本配置===")]
		public float WindDamageBonusRatio = 50f;

		[LabelText("每次刷新时持续时长")]
		[TitleGroup("===基本配置===")]
		public float DurationPerRefresh = 8f;

		[LabelText("当前层数")]
		[TitleGroup("===基本配置===")]
		public int CurrentStackCount = 0;

		[LabelText("最大层数")]
		[TitleGroup("===基本配置===")]
		public int MaxStackCount = 3;

#endregion

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			CurrentStackCount = 1;
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeFrontMul_对接收方将要进行前乘额外算区计算,
				_ABC_CheckIfBonusElementDamage);
			(this as I_BuffAsElementAmplification).RefreshAmplificationTime();
			(this as I_BuffContainLoopVFX).SpawnVFX();
		}



		private void _ABC_CheckIfBonusElementDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar.DamageType != DamageTypeEnum.AoNengFeng_奥能风)
			{
				return;
			}
			dar.CP_DamageAmount_BPart.MultiplyPart += WindDamageBonusRatio * CurrentStackCount / 100f;
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeFrontMul_对接收方将要进行前乘额外算区计算,
				_ABC_CheckIfBonusElementDamage);
			(this as I_BuffContainLoopVFX).StopVFX();
			return base.OnBuffPreRemove();
		}
		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			CurrentStackCount += 1;
			CurrentStackCount = Mathf.Clamp(CurrentStackCount, 0, MaxStackCount);
			(this as I_BuffAsElementAmplification).RefreshAmplificationTime();
			(this as I_BuffContainLoopVFX).SpawnVFX();
			var ds = base.OnExistBuffRefreshed(caster, blps);
			return ds;
		}
		public override string ToString()
		{
			return $"当前层数{CurrentStackCount},已增伤{WindDamageBonusRatio * CurrentStackCount}";
		}

		[SerializeField, LabelText("循环特效UID"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===基本配置===")]
		private string _vfxUid;
		private PerVFXInfo _relatedVFXInfo;
		[SerializeField, LabelText("特效停止时是立刻的吗？")]
		[TitleGroup("===基本配置===")]
		private bool _stopImmediate;

		public float GetAmplificationResetDuration()
		{
			return DurationPerRefresh;
		}
		public int GetCurrentStack()
		{
			return CurrentStackCount;
		}

		public float GetRelatedBuffRemainingPartial()
		{
			return BuffRemainingAvailableTime / GetAmplificationResetDuration();
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