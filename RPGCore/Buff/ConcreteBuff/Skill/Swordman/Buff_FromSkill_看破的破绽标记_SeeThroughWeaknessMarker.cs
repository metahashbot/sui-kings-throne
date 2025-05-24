using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Skill.Swordman
{

	[Serializable]
	public class Buff_FromSkill_看破的破绽标记_SeeThroughWeaknessMarker : BaseRPBuff
	{


		[NonSerialized, LabelText("当前层数")]
		public int CurrentStackCount;

		[SerializeField, LabelText("每次延长的时长"), TitleGroup("===配置===")]
		public float _extendDuration = 3f;
		
        
		[SerializeField, LabelText("追加的伤害信息配置"), TitleGroup("===配置===")]
		public ConSer_DamageApplyInfo DamageApplyConfig = new ConSer_DamageApplyInfo
		{
			DamageType = DamageTypeEnum.TrueDamage_真伤,
			ProcessOption = DamageProcessStepOption.TrueDamageDPS(),
			DamageFromFlag = DamageFromTypeFlag.None_未指定,
			DamageTryTakenBase = 1,
			DamageTakenRelatedDataEntry = true,
			RelatedDataEntryInfos = new List<ConSer_DataEntryRelationConfig>()
			{
				new ConSer_DataEntryRelationConfig
				{
					RelatedToReceiver = true,
					RelatedDataEntryType = RP_DataEntry_EnumType.AttackPower_攻击力,
					Partial = 1,
					CalculatePosition = (ModifyEntry_CalculatePosition)ModifyEntry_CalculatePosition.Original,
					CacheDataEntryValue = 0
				}
			},
		};

		[SerializeField, LabelText("破绽特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_weakness;

		private PerVFXInfo _vfxInfo_Weakness;
		
		
		
		
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
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
				_ABC_TryAppendDamage_OnTakenOnHP);
		}


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds= base.OnBuffInitialized(blps);

			_vfxInfo_Weakness?.VFX_StopThis(true);
			_vfxInfo_Weakness = _VFX_GetAndSetBeforePlay(_vfx_weakness)?._VFX__10_PlayThis();

			return ds;
		}


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);

			_vfxInfo_Weakness?.VFX_StopThis(true);
			_vfxInfo_Weakness = _VFX_GetAndSetBeforePlay(_vfx_weakness )?._VFX__10_PlayThis();

			CurrentStackCount += 1;
			return ds;
		}

		private void _ABC_TryAppendDamage_OnTakenOnHP(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (GetBuffCurrentAvailableType() 
				!= BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				return;
			}
			//需要相同来源
			if (!ReferenceEquals(dar.Caster, Source_SelfReceiveFromObjectRef))
			{
				return;
				
			}
			if (!(dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerNormalAttack_玩家普攻伤害) ||
			      dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerSkillAttack_玩家技能伤害) ||
			      dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerUltraAttack_玩家超杀伤害) ||
			      dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerCoopAttack_玩家连协伤害)))
			{
				return;
			}


			//剑意
			var willOfSword =
				(dar.Caster as I_RP_Buff_ObjectCanApplyBuff).ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum
					.Buff_FromBuff_来自破绽剑意_WillOfSword)
				as Buff_FromBuff_来自破绽剑意_WillOfSword;
			willOfSword.AddWillOfSword(1);
			
			
			//追加伤害
			var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromFromConSer(DamageApplyConfig,
				Parent_SelfBelongToObject as I_RP_Damage_ObjectCanReceiveDamage,
				Source_SelfReceiveFromObjectRef as I_RP_Damage_ObjectCanApplyDamage,
				null);
			var dd = _damageAssistServiceRef.ApplyDamage(dai);
			
			
			
			dd.ReleaseToPool();

			//延长其他破绽标记的持续时间
			//从COM中扫
			var enemyList = _characterOnMapManagerRef
				.GetEnemyListInRange(Parent_SelfBelongToObject.GetBuffReceiverPosition(), 99f)
				.ClipEnemyListOnDefaultType().ClipEnemyListOnInvincibleBuff();
			foreach (var perBehaviour in enemyList)
			{
				// if (ReferenceEquals(perBehaviour, Parent_SelfBelongToObject))
				// {
				// 	continue;
				// }
				if (perBehaviour.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum
					    .FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough) ==
				    BuffAvailableType.Available_TimeInAndMeetRequirement)
				{
					var buff = perBehaviour.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum
							.FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough) as
						Buff_FromSkill_看破的破绽标记_SeeThroughWeaknessMarker;
					buff.ExtendDuration();	
				}
			}


			//自己减一层
			CurrentStackCount -= 1;
			if (CurrentStackCount <= 0)
			{
				Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(SelfBuffType);
			}
			
			
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_一个用作UID的标记_OneUIDTag blp一个用作uid的标记OneUidTag:
					_vfx_weakness = blp一个用作uid的标记OneUidTag.UID;
					break;
			}
		}


		public void ExtendDuration()
		{
			ResetDurationAndAvailableTimeAs(BuffRemainingExistDuration + _extendDuration,
				BuffRemainingAvailableTime + _extendDuration,
				true);
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			_vfxInfo_Weakness?.VFX_StopThis(true);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上,
				_ABC_TryAppendDamage_OnTakenOnHP);
			return base.OnBuffPreRemove();
		}


	}
}