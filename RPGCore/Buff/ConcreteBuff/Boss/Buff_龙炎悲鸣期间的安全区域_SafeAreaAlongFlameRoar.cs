using System;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
namespace RPGCore.Buff.ConcreteBuff.Boss
{
	[Serializable]
	public class Buff_龙炎悲鸣期间的安全区域_SafeAreaAlongFlameRoar : BaseRPBuff , I_BuffTransferWithinPlayer
	{
		private I_RP_Buff_ObjectCanReceiveBuff _originalParent;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_BlockFireAreaEffect_OnBuffPreAdd);
		}



		private void _ABC_BlockFireAreaEffect_OnBuffPreAdd(DS_ActionBusArguGroup ds)
		{
			RolePlay_BuffTypeEnum newBuffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			if (newBuffType == RolePlay_BuffTypeEnum.FireAreaEffectToPlayerByFlameDragon_烈鸟龙的火圈对玩家效果 ||
			    newBuffType == RolePlay_BuffTypeEnum.FireCoreInstantDeathEffectByFlameDragon_烈鸟龙火核的秒杀效果)
			{
				var buffApplyResult = ds.GetObj2AsT<RP_DS_BuffApplyResult>();
				buffApplyResult.BlockByOtherBuff = true;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_BlockFireAreaEffect_OnBuffPreAdd);
			return base.OnBuffPreRemove();
		}



		I_RP_Buff_ObjectCanReceiveBuff I_BuffTransferWithinPlayer.OriginalParent
		{
			get => _originalParent;
			set => _originalParent = value;
		}
		public void ProcessTransfer(I_BuffTransferWithinPlayer transferFrom, PlayerARPGConcreteCharacterBehaviour newPlayer)
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_BlockFireAreaEffect_OnBuffPreAdd);
			Parent_SelfBelongToObject = newPlayer;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_BlockFireAreaEffect_OnBuffPreAdd);
		}
	}
}