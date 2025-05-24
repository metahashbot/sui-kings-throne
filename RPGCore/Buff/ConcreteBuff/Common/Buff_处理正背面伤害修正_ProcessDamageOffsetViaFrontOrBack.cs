using System;
using ARPG.Character.Base;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.AssistBusiness;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class Buff_处理正背面伤害修正_ProcessDamageOffsetViaFrontOrBack : BaseRPBuff
	{


		[SerializeField, LabelText("包含正面伤害修正")]
		[TitleGroup("===具体配置===")]
		private bool _containOffsetOnFront;
		
		[SerializeField,LabelText("    正面伤害调整百分比") ,ShowIf("_containOffsetOnFront"),TitleGroup("===具体配置==="),SuffixLabel("%")]
		private float _offsetPercentOnFront = 0f;
		 
		 
		[SerializeField, LabelText("包含背面伤害修正")]
		[TitleGroup("===具体配置===")]
		private bool _containOffsetOnBack;
		
		[SerializeField,LabelText("    背面伤害调整百分比") ,ShowIf("_containOffsetOnBack"),TitleGroup("===具体配置==="),SuffixLabel("%")]
		private float _offsetPercentOnBack = 50f;
		
		[SerializeField,LabelText("    背击包含额外跳字")]
		[ShowIf("_containOffsetOnBack"),TitleGroup("===具体配置===")]
		private bool _containExtraPopupOnBack = true;



		private static DamageHintManager _dhmRef;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_dhmRef = SubGameplayLogicManager_ARPG.Instance.DamageHintManagerReference;



			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckAppendDamage_OnDamageTakenOnRM);
		}


		private void _ABC_CheckAppendDamage_OnDamageTakenOnRM(DS_ActionBusArguGroup ds)
		{
			var rpds_dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();

			if (rpds_dar == null)
			{
				return;
			}

			var projectile = rpds_dar.RelatedProjectileRuntimeRef;
			if (projectile == null)
			{
				return;
			}

			var projectilePosition = projectile.RelatedGORef.transform.position;
			var receiverBehaviour = Parent_SelfBelongToObject as BaseARPGCharacterBehaviour;
			if (!receiverBehaviour)
			{
				return;
			}

			var receiverFaceDir = receiverBehaviour.GetRelatedArtHelper().CurrentFaceLeft
				? BaseGameReferenceService.CurrentBattleLogicLeftDirection
				: BaseGameReferenceService.CurrentBattleLogicRightDirection;
			var dirFromReceiverToProjectile = (projectilePosition - receiverBehaviour.GetCollisionCenter());
			 
			if( _containOffsetOnBack || _containOffsetOnFront)
			{
				//check if same direction
				var dot = Vector3.Dot(dirFromReceiverToProjectile.normalized, receiverFaceDir);
				//点积大于0为同向
				if (dot > 0)
				{
					if (_containOffsetOnFront)
					{
						rpds_dar.CP_DamageAmount_DPart.MultiplyPart += (_offsetPercentOnFront / 100f);
					}
				}
				else
				{
					//back
					if (_containOffsetOnBack)
					{
						rpds_dar.CP_DamageAmount_DPart.MultiplyPart += (_offsetPercentOnBack / 100f);

						if (_containExtraPopupOnBack)
						{
							_dhmRef.PopupBackAttackHint(receiverBehaviour.GetCollisionCenter());
						}
					}
				}
			}


		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp )
			{
				case BLP_正背面伤害修正覆写_OverrideOffsetOnBackOrFront overrideOnOffset:
					_containOffsetOnFront = overrideOnOffset._containOffsetOnFront;
					_offsetPercentOnFront = overrideOnOffset._offsetPercentOnFront;
					_containOffsetOnBack = overrideOnOffset._containOffsetOnBack;
					_offsetPercentOnBack = overrideOnOffset._offsetPercentOnBack;
					_containExtraPopupOnBack = overrideOnOffset._containExtraPopupOnBack;
					
					 
					break;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算,
				_ABC_CheckAppendDamage_OnDamageTakenOnRM);
			return base.OnBuffPreRemove();
		}



		
		[Serializable]
		public class BLP_正背面伤害修正覆写_OverrideOffsetOnBackOrFront : BaseBuffLogicPassingComponent
		{

			[SerializeField, LabelText("包含正面伤害修正")]
			public bool _containOffsetOnFront;

			[SerializeField, LabelText("    正面伤害调整百分比"), ShowIf("_containOffsetOnFront"), SuffixLabel("%")]
			public float _offsetPercentOnFront = 0f;


			[SerializeField, LabelText("包含背面伤害修正")]
			public bool _containOffsetOnBack;

			[SerializeField, LabelText("    背面伤害调整百分比"), ShowIf("_containOffsetOnBack"), SuffixLabel("%")]
			public float _offsetPercentOnBack = 50f;

			[SerializeField, LabelText("    背击包含额外跳字")]
			[ShowIf("_containOffsetOnBack")]
			public bool _containExtraPopupOnBack = true;




			 

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_正背面伤害修正覆写_OverrideOffsetOnBackOrFront>.Release(this);
			}
		}


	}
}