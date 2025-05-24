using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using Global.ActionBus;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using WorldMapScene.Character;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[TypeInfoBox("透明|隐身")]
	[Serializable]
	public class Buff_TransparentAndCloaking : BaseRPBuff
	{

		[SerializeField, LabelText("目标不透明度")]
		public float TargetOpacity = 0.25f;
		[SerializeField, LabelText("目标隐身后颜色")]
		public Color TargetColor = new Color(61f / 255f, 61f / 255f, 61f / 255f);




		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamagePrelude_对接收方伤害流程序言部分,
				_ABC_ResetDamageResultToInvalid_OnDamagePreCalculate,
				100);
			BaseARPGArtHelper artH = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedArtHelper() as BaseARPGArtHelper;
			// artH.SetTransparent_All(TargetOpacity, TargetColor);
			return ds;
		}


		private void _ABC_ResetDamageResultToInvalid_OnDamagePreCalculate(DS_ActionBusArguGroup ds)
		{
			var damageResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			damageResult.ResultLogicalType = RP_DamageResultLogicalType.InvalidDamage;
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds_remove = base.OnBuffPreRemove();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamagePrelude_对接收方伤害流程序言部分,
				_ABC_ResetDamageResultToInvalid_OnDamagePreCalculate);
			BaseARPGArtHelper artH = Parent_SelfBelongToObject.ReceiveBuff_GetRelatedArtHelper() as BaseARPGArtHelper;
			// artH.SetTransparent_All(1f, Color.white);
			;
			return ds_remove;
		}


	}
}