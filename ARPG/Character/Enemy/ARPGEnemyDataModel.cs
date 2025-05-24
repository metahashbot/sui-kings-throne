using ARPG.Character.Base;
using Global.ActionBus;
using RPGCore;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using UnityEngine.Events;
using UnityEngine.Pool;
namespace ARPG.Character.Enemy
{
	public class ARPGEnemyDataModel : BaseARPGDataModel
	{

		public bool DataModelTickActive { get; private set; } = true;


		private void _ToggleDataModelTickActive(bool to)
		{
			DataModelTickActive = to;
		}
		private UnityAction<bool> _ua_Toggle;
		
		public ARPGEnemyDataModel(RolePlay_BaseBehaviour behaviourReference) : base(behaviourReference)
		{
			_ua_Toggle = _ToggleDataModelTickActive;
		}


		protected override void _ReceiveDamagePart_HPPart(
			ref RP_DS_DamageApplyResult damageApplyResult,
			ref RP_DS_DamageApplyInfo damageApplyInfo,
			ref LocalActionBus lab_receiver,
			ref LocalActionBus lab_caster,
			ref FloatPresentValue_RPDataEntry currentHPPresent,
			ref int effectIDStamp)
		{
			if (damageApplyResult.DamageAmount_AfterShield > float.Epsilon)
			{
				var ds_receiver = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Receiver_BeforeTakeToHP_对接收方将要对HP进行伤害计算);
				ds_receiver.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(ds_receiver);


				var ds_caster = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Caster_BeforeTakeToHP_对施加方将要对HP进行伤害计算);
				ds_caster.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(ds_caster);


				//剩余伤害比血量多，给打死了
				if (damageApplyResult.DamageAmount_AfterShield >= currentHPPresent.CurrentValue)
				{
				
					damageApplyResult.MayCauseDeath = true;


					var _ds_damageFatal = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_DamageResult_DamageResultToFatal_伤害结果导致了致命伤);
					ds_receiver.ObjectArgu1 = damageApplyResult;
					lab_receiver.TriggerActionByType(_ds_damageFatal);
					//此处可能被锁血之类的机制改写，下方才是真正的死亡

					if (damageApplyResult.MayCauseDeath)
					{
						//Boss不适用过伤、记录死亡 的机制
						if (SelfBuffHolderInstance.CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							currentHPPresent.ResetDataToValue(0f, true);
							var ds_death =
								new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
									.L_Damage_OnFinallyDeath_伤害流程最终死亡);
							ds_death.ObjectArgu1 = _selfRelatedRPBehaviour;
							ds_death.ObjectArgu2 = damageApplyResult.Caster;
							ds_death.ObjectArguStr = damageApplyResult;
							lab_receiver.TriggerActionByType(ds_death);
						}
						else
						{
							bool noRegisterDeath_不会记录死亡 = false;
							if (SelfBuffHolderInstance.CheckTargetBuff(RolePlay_BuffTypeEnum.DeathNoRegister_死亡时不会记录) ==
							    BuffAvailableType.Available_TimeInAndMeetRequirement)
							{
								noRegisterDeath_不会记录死亡 = true;
							}

							bool alreadyRegistered_已记录死亡 =
								SelfBuffHolderInstance.CheckTargetBuff(RolePlay_BuffTypeEnum.RegisterToDeath_已记录死亡) ==
								BuffAvailableType.Available_TimeInAndMeetRequirement;
							damageApplyResult.DamageResult_TakenOnHP = currentHPPresent.CurrentValue;

							//过量伤害会直接使用碎裂的死亡决策，而不是普通的死亡决策。
							//如果没有过量伤害，那么就会进行普通的伤害等待
							//超伤会无视掉死亡记录的等待，即时已经等待，也会立刻死亡
							if (damageApplyResult.DamageAmount_AfterShield >= (SelfDataEntry_Database
								.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).CurrentValue * 0.3f))
							{
								damageApplyResult.CauseOverloadDamageEffect = true;

								currentHPPresent.ResetDataToValue(0f, true);
								var ds_death =
									new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
										.L_Damage_OnFinallyDeath_伤害流程最终死亡);
								ds_death.ObjectArgu1 = _selfRelatedRPBehaviour;
								ds_death.ObjectArgu2 = damageApplyResult.Caster;
								ds_death.ObjectArguStr = damageApplyResult;
								lab_receiver.TriggerActionByType(ds_death);
							}
							else
							{
								damageApplyResult.PopupDamageNumber = damageApplyResult.DamageResult_TakenOnHP;
								if (alreadyRegistered_已记录死亡)
								{
									damageApplyResult.DamageResult_TakenOnHP = 0f;
									damageApplyResult.PopupDamageNumber = damageApplyResult.DamageAmount_AfterShield;
									_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
										RolePlay_BuffTypeEnum.RegisterToDeath_已记录死亡,
										_selfRelatedRPBehaviour,
										_selfRelatedRPBehaviour);
								}
								//那就记录一下
								else
								{
									//但是不会记录，那就直接死了
									if (noRegisterDeath_不会记录死亡)
									{
										currentHPPresent.ResetDataToValue(0f, true);
										var ds_death = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
											.L_Damage_OnFinallyDeath_伤害流程最终死亡);
										ds_death.ObjectArgu1 = _selfRelatedRPBehaviour;
										ds_death.ObjectArgu2 = damageApplyResult.Caster;
										lab_receiver.TriggerActionByType(ds_death);
									}
									//会记录死亡，那就记录
									else
									{
										damageApplyResult.DamageResult_TakenOnHP = currentHPPresent.CurrentValue;
										damageApplyResult.PopupDamageNumber =
											damageApplyResult.DamageAmount_AfterShield;
										_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
											RolePlay_BuffTypeEnum.RegisterToDeath_已记录死亡,
											_selfRelatedRPBehaviour,
											_selfRelatedRPBehaviour);
									}
								}
							}
						}
					}
					else
					{
						damageApplyResult.DamageResult_TakenOnHP = currentHPPresent.CurrentValue - 1f;
						currentHPPresent.ResetDataToValue(1f, true);
					}
				}
				//正常打没打死
				else
				{
					damageApplyResult.DamageResult_TakenOnHP = damageApplyResult.DamageAmount_AfterShield;
					damageApplyResult.PopupDamageNumber = damageApplyResult.DamageResult_TakenOnHP;



					var newModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(
						-damageApplyResult.DamageResult_TakenOnHP,
						RPDM_DataEntry_ModifyFrom.FromDamage_伤害流程,
						ModifyEntry_CalculatePosition.FrontAdd,
						damageApplyInfo.DamageCaster,
						effectIDStamp);
					currentHPPresent.AddDataEntryModifier(newModify);

					DS_ActionBusArguGroup ds_takenOnHP_Caster =
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
							.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上);
					ds_takenOnHP_Caster.ObjectArgu1 = damageApplyResult;
					lab_caster?.TriggerActionByType(ds_takenOnHP_Caster);

					DS_ActionBusArguGroup ds_takenOnHp = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上);
					ds_takenOnHp.ObjectArgu1 = damageApplyResult;
					lab_receiver.TriggerActionByType(ds_takenOnHp);
				}
			}
		}
	}
}