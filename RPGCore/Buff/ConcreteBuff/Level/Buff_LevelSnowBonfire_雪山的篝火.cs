using System;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Level
{
	[Serializable]
	public class Buff_LevelSnowBonfire_雪山的篝火: BaseRPBuff
	{

		[SerializeField, LabelText("初始的火星范围"), TitleGroup("===具体配置===")]
		public float InitLayoutRadius = 3f;
		
		[SerializeField,LabelText("每次攻击增加的火星范围"), TitleGroup("===具体配置===")]
		public float LayoutRadiusIncreasePerAttack = 0.5f;
		
		[SerializeField,LabelText("最大火星范围"), TitleGroup("===具体配置===")]
		public float MaxLayoutRadius = 8f;

		public float CurrentLayoutRadius { get; private set; }
		
		[SerializeField,LabelText("重置火星范围的时间间隔"), TitleGroup("===具体配置===")]
		public float ResetLayoutRadiusInterval = 5f;

		[SerializeField, LabelText("最短触发间隔"), TitleGroup("===具体配置===")]
		public float MinTriggerInterval = 0.99f;

		private float _next_AbleToTriggerTime;
		private float _lastHitTime;
		
		
		[SerializeField,LabelText("关联的版面"),InlineEditor(InlineEditorObjectFieldModes.Boxed),TitleGroup("===具体配置===")]
		public SOConfig_ProjectileLayout RelatedLayout;



		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
				_ABC_ProcessLayoutRadiusAndSpawnLayout_OnTakenDamage,
				100);
		}



		private void _ABC_ProcessLayoutRadiusAndSpawnLayout_OnTakenDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			dar.DamageAmount_AfterShield = 0f;
			dar.PopupDamageNumber = 0f;


			if (BaseGameReferenceService.CurrentTimeInSecond <_next_AbleToTriggerTime)
			{
				return;
			
			}
			_next_AbleToTriggerTime = BaseGameReferenceService.CurrentTimeInSecond + MinTriggerInterval;
			

			if (BaseGameReferenceService.CurrentFixedTime > (_lastHitTime + ResetLayoutRadiusInterval))
			{
				CurrentLayoutRadius = InitLayoutRadius;
			}
			else
			{
				CurrentLayoutRadius = Mathf.Clamp(CurrentLayoutRadius + LayoutRadiusIncreasePerAttack,
					InitLayoutRadius,
					MaxLayoutRadius);
			}



			var tmpLayout = RelatedLayout.SpawnLayout_NoAutoStart(
				Parent_SelfBelongToObject as I_RP_Projectile_ObjectCanReleaseProjectile,
				false,
				Parent_SelfBelongToObject as RolePlay_BaseBehaviour);
			tmpLayout.LayoutContentInSO.RelatedProjectileScale = CurrentLayoutRadius;
			tmpLayout.LayoutHandlerFunction.StartLayout();
			
			
			_lastHitTime = BaseGameReferenceService.CurrentFixedTime;
		}

	}
}