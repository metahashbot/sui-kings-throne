using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Level
{
	[Serializable]
	public class Buff_Totem_图腾回血_HealTotem : BaseRPBuff
	{

		private float _healAmount_Count;

		private float _healAmount_HPPartial;

		protected float _triggerInterval;
		protected float _nextTriggerTime;
		protected string AreaUid;

		[LabelText("忽略的角色枚举们"), SerializeField, TitleGroup("===配置===")]
		protected List<CharacterNamedTypeEnum> _ignoreCharacterList = new List<CharacterNamedTypeEnum>();

		[SerializeField, LabelText("释放时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_release;
		
		[SerializeField, LabelText("回蓝特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_ReplySP;

		protected PerVFXInfo _vfxInfo_ReplySP;
		
		
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_nextTriggerTime = BaseGameReferenceService.CurrentFixedTime + _triggerInterval;
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
				_ABC_DamageTruncate,
				999);
		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (GetBuffCurrentAvailableType() == BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				if (currentTime > _nextTriggerTime)
				{
					_nextTriggerTime = currentTime + _triggerInterval;
					_Internal_ProcessHeal();
				}
			}
		}


		private void _Internal_ProcessHeal()
		{
			foreach (BaseARPGCharacterBehaviour perBehaviour in _characterOnMapManagerRef
				.CurrentAllActiveARPGCharacterBehaviourCollection)
			{
				if (perBehaviour is not EnemyARPGCharacterBehaviour enemy)
				{
					continue;
				}

				if (_ignoreCharacterList.Contains(perBehaviour.SelfBehaviourNamedType))
				{
					continue;
				}
                
				if (!enemy.RelatedSpawnConfigInstance.RelatedSpawnEnemyGameplayEventHandlerRef.AreaUID.Equals(AreaUid))
				{
					continue;
				}
				
				if (_healAmount_Count > 0f)
				{
                    
					var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(enemy,
						Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
						DamageTypeEnum.Heal_治疗,
						_healAmount_Count,
						DamageProcessStepOption.HealDPS());
					_VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();
					_damageAssistServiceRef.ApplyDamage(dai).ReleaseToPool();
				}
				if (_healAmount_HPPartial > 0f)
				{
					var hpEntry = enemy.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
					var dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(enemy,
						Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
						DamageTypeEnum.Heal_治疗,
						hpEntry.CurrentValue * _healAmount_HPPartial / 100f,
						DamageProcessStepOption.HealDPS());
					_VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();
					_damageAssistServiceRef.ApplyDamage(dai).ReleaseToPool();
				}
			}
		}
		/// <summary>
		/// <para>图腾的伤害截断效果</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_DamageTruncate(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();

			dar.DamageAmount_AfterShield = 0f;
			if (dar.Caster is not PlayerARPGConcreteCharacterBehaviour player)
			{
				return;
			}
			else
			{
				//查查玩家成分
				var checkR =
					player.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Level_攻击反回血图腾_AttackBonusAntiHeal);
				if (checkR == BuffAvailableType.NotExist)
				{
					return;
				}
				var buff =
					player.ApplyBuff_GetRelatedBuff(RolePlay_BuffTypeEnum.Level_攻击反回血图腾_AttackBonusAntiHeal) as
						Buff_AntiTotem_攻击反回血图腾_AttackBonus;
				dar.DamageAmount_AfterShield = buff.CurrentStackCount;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算,
				_ABC_DamageTruncate);
			return base.OnBuffPreRemove();
		}
		

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_回血图腾配置信息_HealTotemConfig blp_heal:
					_healAmount_Count = blp_heal._healAmount_Count;
					_healAmount_HPPartial = blp_heal._healAmount_HPPartial;
					_triggerInterval = blp_heal._triggerInterval;
					AreaUid = blp_heal.AreaUid;
					break;
			}
		}


		[Serializable]
		public class BLP_回血图腾配置信息_HealTotemConfig : BaseBuffLogicPassingComponent
		{
			[SerializeField, LabelText("每次回血量"), SuffixLabel("点"), TitleGroup("===数值===")]
			public float _healAmount_Count;
			[SerializeField, LabelText("每次回血量(百分比)"), SuffixLabel("%"), TitleGroup("===数值===")]
			public float _healAmount_HPPartial;
			[SerializeField, LabelText("触发的间隔时长"), SuffixLabel("秒"), TitleGroup("===数值===")]
			public float _triggerInterval;
			[SerializeField, LabelText("区域UID"), TitleGroup("===数值===")]
			public string AreaUid;
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_回血图腾配置信息_HealTotemConfig>.Release(this);
			}
		}

	}
}