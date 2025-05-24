//#pragma warning disable CS0162
//#pragma warning disable CS0414


using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.TimedTaskManager;
using RPGCore.Buff;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Pool;
using Random = UnityEngine.Random;


namespace RPGCore
{
	/// <summary>
	/// 基本的RPG数据模型。
	/// <para>该类抽象。如果需要实现，则必须继承！</para>
	/// </summary>
	public abstract class RolePlay_DataModelBase

	{
		protected float _currentTime;
		protected float _currentFrameCount;

		protected RolePlay_BaseBehaviour _selfRelatedRPBehaviour;

		public RolePlay_BaseBehaviour GetRelatedRPBehaviour
		{
			get { return _selfRelatedRPBehaviour; }
		}


#region DataEntry相关业务

		/// <summary>
		/// <para>数据模型是否已经初始化完成？</para>
		/// <para>如果没有初始化完成，则视作与该DM关联的Behaviour还不可用，会跳过一些业务</para>
		/// </summary>
		protected bool _dataModelInitialized = false;

		/// <summary>
		/// <para>数据模型是否已经初始化完成？</para>
		/// <para>如果没有初始化完成，则视作与该DM关联的Behaviour还不可用，会跳过一些业务</para>
		/// </summary>
		public bool DataModelInitialized => _dataModelInitialized;

		[ShowInInspector, FoldoutGroup("运行时", true), LabelText("数据项库")]
		public DataEntry_Database SelfDataEntry_Database { get; protected set; }

		public Float_RPDataEntry GetFloatDataEntry(RP_DataEntry_EnumType dataType, bool allowNotExist = false)
		{
			return SelfDataEntry_Database.GetFloatDataEntryByType(dataType, allowNotExist);
		}

		public FloatPresentValue_RPDataEntry GetFloatPresentValue(RP_DataEntry_EnumType type)
		{
			return SelfDataEntry_Database.GetFloatPresentValueEntryByType(type);
		}

#endregion

		/// <summary>
		/// <para>自身的回调列表</para>
		/// <para>根据RPDM ActionType 来触发某个类型的回调</para>
		/// </summary>
		public LocalActionBus SelfActionBusReference { get; protected set; }


		[ShowInInspector, FoldoutGroup("运行时", true), LabelText("BuffHolder")]
		public RPBuff_BuffHolder SelfBuffHolderInstance { get; protected set; }

		/// <summary>
		/// <para>自身的RB</para>
		/// </summary>
		protected Rigidbody _selfRigidBodyReference;


#region 初始化部分

		public RolePlay_DataModelBase(RolePlay_BaseBehaviour behaviourReference)
		{
			_selfRelatedRPBehaviour = behaviourReference;

			SelfDataEntry_Database = new DataEntry_Database(behaviourReference);
			SelfBuffHolderInstance = new RPBuff_BuffHolder(behaviourReference);
			SelfActionBusReference = behaviourReference.GetRelatedActionBus();
			SelfActionBusReference.RegisterAction(ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryInitialized_数据项被初始化,
				_ABC_DataEntryPostProcess_OnDataEntryDatabaseInitializeFromConfig);
		}


		/// <summary>
		/// <para>当某个DataEntry被添加后的一些后处理，包括但不限于  为其添加关联的Buff项 、 为[最大值]数据项添加[当前值]数据项</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_DataEntryPostProcess_OnDataEntryDatabaseInitializeFromConfig(DS_ActionBusArguGroup ds)
		{
			var initEntry = ds.ObjectArgu1 as Float_RPDataEntry;
			switch (initEntry.RP_DataEntryType)
			{
				case RP_DataEntry_EnumType.CharacterLevel:
					break;
				case RP_DataEntry_EnumType.HPMax_最大HP:

					var entry_chp =
						SelfDataEntry_Database.InitializeFloatPresentValueEntry(RP_DataEntry_EnumType.CurrentHP_当前HP,
							initEntry);
					_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.SRPG_SyncHPPartialOnMaxChanged_当HP最大值变动时同步当前,
						_selfRelatedRPBehaviour,
						_selfRelatedRPBehaviour);
					entry_chp.ResetDataToValue(initEntry.CurrentValue);

					break;
				case RP_DataEntry_EnumType.SPMax_最大SP:
					var entry_csp =
						SelfDataEntry_Database.InitializeFloatPresentValueEntry(RP_DataEntry_EnumType.CurrentSP_当前SP,
							initEntry);
					_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.SRPG_SyncSPPartialOnMaxChanged_当SP最大值变动时同步当前,
						_selfRelatedRPBehaviour,
						_selfRelatedRPBehaviour);
					entry_csp.ResetDataToValue(initEntry.CurrentValue);
					break;
				case RP_DataEntry_EnumType.M_Strength_主力量:
					_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.ARPG_2PUtil_STRConversion_ARPG力量转换,
						_selfRelatedRPBehaviour,
						_selfRelatedRPBehaviour);
					break;
				case RP_DataEntry_EnumType.M_Dexterity_主敏捷:
					_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.ARPG_2PUtil_DEXConversion_ARPG敏捷转换,
						_selfRelatedRPBehaviour,
						_selfRelatedRPBehaviour);
					break;
				case RP_DataEntry_EnumType.M_Vitality_主体质:
					_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.ARPG_2PUtil_VITConversion_ARPG体质转换,
						_selfRelatedRPBehaviour,
						_selfRelatedRPBehaviour);
					break;
				case RP_DataEntry_EnumType.M_Spirit_主精神:
					_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.ARPG_2PUtil_SPRConversion_ARPG精神转换,
						_selfRelatedRPBehaviour,
						_selfRelatedRPBehaviour);

					break;
				case RP_DataEntry_EnumType.M_Intellect_主智力:
					_selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.ARPG_2PUtil_INTConversion_ARPG智力转换,
						_selfRelatedRPBehaviour,
						_selfRelatedRPBehaviour);
					break;
                case RP_DataEntry_EnumType.M_Charm_主魅力:
                    _selfRelatedRPBehaviour.ReceiveBuff_TryApplyBuff(
                        RolePlay_BuffTypeEnum.ARPG_2PUtil_INTConversion_ARPG魅力转换,
                        _selfRelatedRPBehaviour,
                        _selfRelatedRPBehaviour);
                    break;
            }
		}


		// public void InitializeBySOCharacterConfigInfo(SOConfig_CharacterInfo soConfig)
		// {
		//     foreach (var perEntry in soConfig.CharacterRawInfo.DataEntryGroup.InitDataEntryList)
		//     {
		//      SelfDataEntry_Database.InitializeFromInitConfig(perEntry);   
		//     }
		//
		//     foreach (var perDataSOGroup in soConfig.CharacterRawInfo.DataEntryGroupTemplate)
		//     {
		//         foreach (var perEntry in perDataSOGroup.ConfigCollection.InitDataEntryList)
		//         {
		//             SelfDataEntry_Database.InitializeFromInitConfig(perEntry);   
		//         }
		//     }
		// }

#endregion

#region Tick部分

		/// <summary>
		/// <para>Update时序的Tick，经由自身关联的Behaviour调用</para>
		/// <para>默认实现包含：对BuffHolder的Tick</para>
		/// </summary>
		public virtual void UpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			_currentTime = currentTime;
			_currentFrameCount = currentFrameCount;
			SelfBuffHolderInstance.UpdateTick(currentTime, currentFrameCount, delta);
		}


		/// <summary>
		/// <para>FixedUpdate时序的Tick，经由自身关联的Behaviour调用</para>
		/// <para>默认实现为空</para>
		/// </summary>
		public virtual void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			SelfBuffHolderInstance.FixedUpdateTick(currentTime, currentFrame, delta);
		}

#endregion
#region 伤害流程

		private List<int> stampList_DamageInfo = new List<int>();

		/// <summary>
		/// 移除一个伤害时间戳。常见于穿透恢复
		/// </summary>
		/// <param name="toRemove"></param>
		public void RemoveDamageStamp(int toRemove)
		{
			if (toRemove == 0 || toRemove == -1)
			{
				return;
			}
			if (stampList_DamageInfo.Contains(toRemove))
			{
				stampList_DamageInfo.Remove(toRemove);
			}
		}




		/// <summary>
		/// <para>接收伤害：需要构造一个 RP_DS_DamageApplyInfo </para>
		/// <para>当EffectID为0时，表示不需要关注重复施加等问题</para>
		/// <para>伤害的很多步骤都会向内部广播，每个步骤也会更改将要返回的DamageAppyResult</para>
		/// </summary>
		/// <returns>返回内容是一个DamageApplyResult</returns>
		public virtual RP_DS_DamageApplyResult ReceiveDamage(RP_DS_DamageApplyInfo damageApplyInfo, int effectIDStamp)
		{
#if UNITY_EDITOR
			if (damageApplyInfo.DamageType == 0)
			{
				DBug.LogError($"{_selfRelatedRPBehaviour.name}接受伤害的时候出现了一个DamageType为0的伤害，这是不正确的，检查一下");
			}
#endif

#region 基本构建

			LocalActionBus lab_receiver = GetRelatedRPBehaviour.GetRelatedActionBus();
			LocalActionBus lab_caster = damageApplyInfo.DamageCaster?.ApplyDamage_GetLocalActionBus();

			//构建伤害施加结果
			RP_DS_DamageApplyResult damageApplyResult = GenericPool<RP_DS_DamageApplyResult>.Get();
			damageApplyResult.Reset();

			damageApplyResult.Receiver = damageApplyInfo.DamageReceiver;
			damageApplyResult.Caster = damageApplyInfo.DamageCaster;
			damageApplyResult.RelatedProjectileRuntimeRef = damageApplyInfo.RelatedProjectileBehaviourRuntime;
			damageApplyResult.ProcessOption = damageApplyInfo.StepOption;
			damageApplyResult.DamageTimestamp = damageApplyInfo.DamageTimestamp;
			
			damageApplyResult.DamageType = damageApplyInfo.DamageType;
			damageApplyResult.DamageFromTypeFlags = damageApplyInfo.DamageFromTypeFlag;
			damageApplyResult.ResultLogicalType = RP_DamageResultLogicalType.NormalResult;

			if (damageApplyInfo.DamageWorldPosition.HasValue)
			{
				damageApplyResult.DamageWorldPosition = damageApplyInfo.DamageWorldPosition;
			}
			else
			{
				damageApplyResult.DamageWorldPosition = _selfRelatedRPBehaviour.transform.position;
			}

			damageApplyResult.DamageType = damageApplyInfo.DamageType;
			damageApplyResult.DamageRawValueFromDamageApplyInfo = damageApplyInfo.DamageTakenBase;

#endregion


			FloatPresentValue_RPDataEntry currentHPPresent =
				SelfDataEntry_Database.GetFloatPresentValueEntryByType(RP_DataEntry_EnumType.CurrentHP_当前HP);
			float currentHP = currentHPPresent.GetCurrentValue();

			Float_RPDataEntry MaxHPPresent =
				SelfDataEntry_Database.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
			float MaxHp = MaxHPPresent.GetCurrentValue();

			//如果是治疗，则走治疗单独的流程
			if (damageApplyResult.DamageType == DamageTypeEnum.Heal_治疗)
			{
				ReceiveDamagePart_Heal();
			}
			//然后是伤害的流程
			else
			{
				//序言部分

				if (!damageApplyResult.Receiver.ReceiveDamage_IfDataValid())
				{
					damageApplyResult.ResultLogicalType = RP_DamageResultLogicalType.InvalidDamage;
				}
				
				
				if (!damageApplyResult.ProcessOption.IgnorePrelude)
				{
					ReceiveDamagePart_Prelude();
				}
				

				//序言结束后，如果不再是“常规结果”，则相当于此次伤害被某些因素无效化了。那么提前返回
				if (damageApplyResult.ResultLogicalType != RP_DamageResultLogicalType.NormalResult)
				{
					return damageApplyResult;
				}

				if (!damageApplyResult.ProcessOption.IgnoreDodge)
				{
					ReceiveDamagePart_Dodge();
				}
				
				
				if (damageApplyResult.ResultLogicalType == RP_DamageResultLogicalType.DodgedSoNothing)
				{
					SelfActionBusReference.TriggerActionByType(ActionBus_ActionTypeEnum
						.L_DamageResult_DamageDodged_伤害结果伤害被闪避);
					return damageApplyResult;
				}
				
				
				if (!damageApplyResult.ProcessOption.IgnoreFrontAddPart_AttackAndDefense)
				{
					ReceiveDamagePart_FrontAdd_AttackAndDefense();
				}
				else
				{
					damageApplyResult.DamageAmount_APart = damageApplyResult.DamageRawValueFromDamageApplyInfo;
				}
				damageApplyResult.PopupDamageNumber = damageApplyResult.DamageAmount_APart;
				
				
				//技能威力计算
				if (!damageApplyResult.ProcessOption.IgnoreSkillPower)
				{
					ReceiveDamagePart_SkillPower();
				}
				else
				{
					damageApplyResult.DamageAmount_SkillPowerPart = damageApplyResult.DamageAmount_APart;
				}
				damageApplyResult.PopupDamageNumber = damageApplyResult.DamageAmount_SkillPowerPart;



				if (!damageApplyResult.ProcessOption.IgnoreExtraDamage_ResistAndFragile)
				{
					ReceiveDamagePart_ExtraDamage();
				}
				else
				{
					damageApplyResult.DamageAmount_ExtraDamagePart = damageApplyResult.DamageAmount_SkillPowerPart;
				}
				damageApplyResult.PopupDamageNumber = damageApplyResult.DamageAmount_ExtraDamagePart;

				
				
				if (!damageApplyResult.ProcessOption.IgnoreCritical)
				{
					ReceiveDamagePart_CriticalPart();
				}
				else
				{
					damageApplyResult.DamageAmount_CriticalPart = damageApplyResult.DamageAmount_ExtraDamagePart;
				}
				damageApplyResult.PopupDamageNumber = damageApplyResult.DamageAmount_CriticalPart;


				if (!damageApplyResult.ProcessOption.IgnoreRearMulPart_FinalBonus)
				{
					ReceiveDamagePart_FinalDamage();
				}
				else
				{
					damageApplyResult.DamageAmount_FinalBonusPart = damageApplyResult.DamageAmount_CriticalPart;
				}
				ReceiveDamagePart_CheckFrontAndBack();
				

				if (!damageApplyResult.ProcessOption.IgnoreGuarantee)
				{
					ReceiveDamagePart_DamageGuarantee();
				}
				
				
				if (damageApplyResult.ProcessOption.IgnorePopup)
				{

					damageApplyResult.PopupDamageNumber = 0f;
				}
				else
				{
					damageApplyResult.PopupDamageNumber = damageApplyResult.DamageAmount_FinalBonusPart;
				}

				
				
				
				if (!damageApplyResult.ProcessOption.IgnoreShieldPassing)
				{
					ReceiveDamagePart_Shield();
				}
				else
				{
					damageApplyResult.DamageAmount_AfterShield = damageApplyResult.DamageAmount_FinalBonusPart;
				}
				damageApplyResult.PopupDamageNumber = damageApplyResult.DamageAmount_AfterShield;

				if (!damageApplyResult.ProcessOption.IgnoreHealth)
				{
					_ReceiveDamagePart_HPPart(ref damageApplyResult,
						ref damageApplyInfo,
						ref lab_receiver,
						ref lab_caster,
						ref currentHPPresent,
						ref effectIDStamp);
				}



			}



			ReceiveDamagePart_ProjectileFunctionComponentPart();


			if (!damageApplyResult.ProcessOption.IgnoreBuff)
			{
				ReceiveDamagePart_Buff();
			}




			return damageApplyResult;



			/*
			 *
			 *
			 *
			 *
			 *
			 */


			void ReceiveDamagePart_Prelude()
			{
				if (damageApplyResult.DamageTimestamp != 0)
				{
					//如果记录的StampList内已经有此次ID的Damage了，则不会造成重复伤害，跳过
					if (stampList_DamageInfo.Contains(damageApplyResult.DamageTimestamp))
					{
						damageApplyResult.ResultLogicalType = RP_DamageResultLogicalType.InvalidDamage;
						return;
					}
					else
					{
						stampList_DamageInfo.Add(damageApplyResult.DamageTimestamp);
					}
				}

				var ds_damagePrelude_receiver =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_DamagePrelude_对接收方伤害流程序言部分);
				ds_damagePrelude_receiver.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(ds_damagePrelude_receiver);
				
				var _ds_damagePrelude_Caster = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamagePrelude_对施加方伤害流程序言部分);
				_ds_damagePrelude_Caster.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(_ds_damagePrelude_Caster);
			}



			void ReceiveDamagePart_Heal()
			{
				damageApplyResult.ResultLogicalType = RP_DamageResultLogicalType.ActAsHeal;
				var 
					ds_healPre_Receiver=
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeHeal_对接收方将要进行治疗计算);
				ds_healPre_Receiver.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(ds_healPre_Receiver);
				
				var ds_healPre_Caster =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeHeal_对施加方将要进行治疗计算);
				ds_healPre_Caster.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(ds_healPre_Caster);


				damageApplyResult.HealAmount =
					damageApplyResult.CP_Heal.GetResult(damageApplyResult.DamageRawValueFromDamageApplyInfo);

				var newModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(damageApplyResult.HealAmount,
					RPDM_DataEntry_ModifyFrom.FromDamage_伤害流程,
					ModifyEntry_CalculatePosition.FrontAdd,
					damageApplyResult.Caster,
					damageApplyResult.DamageTimestamp);
				currentHPPresent.AddDataEntryModifier(newModify);
				damageApplyResult.PopupDamageNumber = damageApplyResult.HealAmount;

				damageApplyResult.DamageResult_TakenOnHP = -damageApplyResult.HealAmount;

				var ds_healTo = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Damage_OnHealTakenOnHP_治疗打到了HP上);
				ds_healTo.ObjectArgu1 = damageApplyResult.HealAmount;
				lab_receiver.TriggerActionByType(ds_healTo);
			}




			void ReceiveDamagePart_Dodge()
			{
				float baseHit = 100f;
				float casterHitBonus = _Internal_GetCasterData_Safe(RP_DataEntry_EnumType.Accuracy_命中率,
					ref damageApplyResult.Caster,
					ref damageApplyResult.RelatedProjectileRuntimeRef,
					0f);

				var receiverDodge = damageApplyInfo.DamageReceiver
					.ReceiveDamage_GetRelatedDataEntry(RP_DataEntry_EnumType.DodgeRate_闪避率).CurrentValue;


				var _ds_preDodge = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_BeforeDodge_对施加方将要进行闪避计算);
				_ds_preDodge.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(_ds_preDodge);

				var _ds_preDodge_receiver =
					new DS_ActionBusArguGroup(
						ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeDodge_对接收方将要进行闪避计算);
				_ds_preDodge_receiver.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(_ds_preDodge_receiver);


				casterHitBonus = damageApplyResult.CP_Accuracy.GetResult(casterHitBonus);
				receiverDodge = damageApplyResult.CP_DodgeRate.GetResult(receiverDodge);

				float hitRatio = baseHit + casterHitBonus - receiverDodge;
				hitRatio = Mathf.Clamp(hitRatio, 0, 100);
				// 例，当33命中率时，有67概率进入,即被闪避 
				if (hitRatio < Random.Range(0f, 100f))
				{
					//闪避成功
					damageApplyResult.ResultLogicalType = RP_DamageResultLogicalType.DodgedSoNothing;
				}
			}


			
			//前加算区，攻防部分
			void ReceiveDamagePart_FrontAdd_AttackAndDefense()
			{
				var defenseEntry =
					damageApplyInfo.DamageReceiver.ReceiveDamage_GetRelatedDataEntry(RP_DataEntry_EnumType.Defense_防御力);
				var currentDefenseValue = defenseEntry.CurrentValue;

				var ds_receiver = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Receiver_BeforeFrontAdd_对接收方将要进行前加攻防算区计算);
				ds_receiver.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(ds_receiver);

				var ds_caster = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Caster_BeforeFrontAdd_对施加方将要进行前加攻防算区计算);
				ds_caster.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(ds_caster);

				currentDefenseValue = damageApplyResult.CP_APart_Defense.GetResult(currentDefenseValue);

				float defenseIgnore = damageApplyResult.CP_APart_DefenseIgnore.GetResult(0f);

				float finalDefense = currentDefenseValue - defenseIgnore;
				if (finalDefense < 0f)
				{
					finalDefense = 0f;
				}

				damageApplyResult.DamageAmount_APart =
					damageApplyResult.DamageRawValueFromDamageApplyInfo - finalDefense;
				if (damageApplyResult.DamageAmount_APart < 0f)
				{
					damageApplyResult.DamageAmount_APart = 0f;
				}
			}

			void ReceiveDamagePart_SkillPower()
			{
				var ds_caster = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DamageProcess_SkillPower_技能威力计算);
				ds_caster.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(ds_caster);
				damageApplyResult.DamageAmount_SkillPowerPart = damageApplyResult.CP_SkillPower.GetResult(damageApplyResult.DamageAmount_APart);
				
			}

			
			void ReceiveDamagePart_ExtraDamage()
			{

				var ds_receiver = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Receiver_BeforeFrontMul_对接收方将要进行前乘额外算区计算);
				ds_receiver.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(ds_receiver);
				
				var ds_caster = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Caster_BeforeFrontMul_对施加方将要进行前乘额外算区计算);
				ds_caster.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(ds_caster);
				
				float extraDamage = damageApplyResult.CP_DamageAmount_BPart.GetResult(1f);
				damageApplyResult.DamageAmount_ExtraDamagePart =
					damageApplyResult.DamageAmount_SkillPowerPart * extraDamage;


			}

			void ReceiveDamagePart_CriticalPart()
			{

				var _ds_caster_PreCritical =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_DamageProcess_Caster_BeforeCritical_对施加方将要进行暴击计算);
				_ds_caster_PreCritical.ObjectArgu1 = damageApplyInfo;
				lab_caster?.TriggerActionByType(_ds_caster_PreCritical);


				var _ds_receiver_PreCritical =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_DamageProcess_Receiver_BeforeCritical_对接收方将要进行暴击计算);
				_ds_receiver_PreCritical.ObjectArgu1 = damageApplyInfo;
				lab_receiver.TriggerActionByType(_ds_receiver_PreCritical);

				float casterCriticalRate = _Internal_GetCasterData_Safe(RP_DataEntry_EnumType.CriticalRate_暴击率,
					ref damageApplyResult.Caster,
					ref damageApplyResult.RelatedProjectileRuntimeRef,
					0f);
				float criticalRatio = damageApplyResult.CP_CriticalRate.GetResult(casterCriticalRate);




				if (criticalRatio > Random.Range(0f, 100f))
				{
					//暴击成功
					damageApplyResult.IsDamageCauseCritical = true;
					float casterCriticalBonus = _Internal_GetCasterData_Safe(RP_DataEntry_EnumType.CriticalBonus_暴击伤害,
						ref damageApplyResult.Caster,
						ref damageApplyResult.RelatedProjectileRuntimeRef,
						0f);
					float extraCriticalBonus = damageApplyResult.CP_CriticalBonus.GetResult(casterCriticalBonus);


					//抗暴的计算时抗暴的Buff自行处理的，不在这里
					damageApplyResult.DamageAmount_CriticalPart = damageApplyResult.DamageAmount_ExtraDamagePart *
					                                              (1.5f + extraCriticalBonus);
				}
				else
				{
					damageApplyResult.DamageAmount_CriticalPart = damageApplyResult.DamageAmount_ExtraDamagePart;
				}
			}



			void ReceiveDamagePart_FinalDamage()
			{

				var ds_receiver = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算);
				ds_receiver.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(ds_receiver);

				var ds_caster = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_DamageProcess_Caster_BeforeRearMul_对施加方将要进行后乘最终算区计算);
				ds_caster.ObjectArgu1 = damageApplyResult;
				lab_caster?.TriggerActionByType(ds_caster);

				float finalDamage = damageApplyResult.CP_DamageAmount_DPart.GetResult(1f);
				damageApplyResult.DamageAmount_FinalBonusPart =
					damageApplyResult.DamageAmount_CriticalPart * finalDamage;
				
				
				
			}

			void ReceiveDamagePart_CheckFrontAndBack()
			{
				var projectile = damageApplyResult.RelatedProjectileRuntimeRef;
				if (projectile == null)
				{
					damageApplyResult.DamageIsFromBack = null;
					return;
				}
				Vector3 projectilePosition;
				projectilePosition = projectile.RelatedGORef.transform.position;
				if (damageApplyResult.Caster == null)
				{
					damageApplyResult.DamageIsFromBack = null;
					return;
				}

				Vector3 behaviourFaceDirection = damageApplyResult.Receiver.ReceiveDamage_GetCurrentReceiverFaceDirection();
				
				//如果子弹没有速度，则只判断位置 之间的连线
				//如果子弹有速度，则判断速度和受击方 连线
				if (projectile.DesiredMoveSpeed > float.Epsilon)
				{

					Vector3 dirFromBehaviourToProjectile = (projectilePosition -
					                                        damageApplyResult.Receiver
						                                        .ReceiveDamage_GetCurrentReceiverPosition());
					dirFromBehaviourToProjectile.y = 0f;
					 if(Vector3.Dot( dirFromBehaviourToProjectile, behaviourFaceDirection) > 0)
					 {
						 damageApplyResult.DamageIsFromBack = false;
					 }
					 else
					 {
						 damageApplyResult.DamageIsFromBack = true;
					 }
				}
				else
				{
					var dirOfProjectileVelocity = projectile.DesiredMoveDirection;
					dirOfProjectileVelocity.y = 0f;
					if (Vector3.Dot(dirOfProjectileVelocity, behaviourFaceDirection) > 0)
					{
						damageApplyResult.DamageIsFromBack = false;
					}
					else
					{
						damageApplyResult.DamageIsFromBack = true;
					}

				}
			}

			void ReceiveDamagePart_DamageGuarantee()
			{
				if(damageApplyResult.DamageAmount_FinalBonusPart < 1f)
				{
					damageApplyResult.DamageAmount_FinalBonusPart = 1f;
				}
			}


			void ReceiveDamagePart_Shield()
			{
				damageApplyResult.DamageResult_TakenOnShield = 0f;
				damageApplyResult.DamageAmount_AfterShield = damageApplyResult.DamageAmount_FinalBonusPart;
				var ds_shield =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_DamageProcess_BeforeTakeToShield_将要对甲盾进行伤害计算);
				ds_shield.ObjectArgu1 = damageApplyResult;
				lab_receiver.TriggerActionByType(ds_shield);
				
				



			}



			void ReceiveDamagePart_ProjectileFunctionComponentPart()
			{
				if (damageApplyInfo.RelatedProjectileBehaviourRuntime != null &&
				    damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedFunctionComponentList != null)
				{
					bool canEliminate = false;

					/*
					 * 结算斩杀
					 */
					var eliminateIndex =
						damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedFunctionComponentList.FindIndex(
							(component => component is PFC_斩杀_Eliminate));
					if (eliminateIndex != -1)
					{
						PFC_斩杀_Eliminate info = damageApplyInfo.RelatedProjectileBehaviourRuntime
							.RelatedFunctionComponentList[eliminateIndex] as PFC_斩杀_Eliminate;
						//根据标签类型判断能否斩杀
						var chpPartial =
							SelfDataEntry_Database.GetFloatPresentValueEntryByType(RP_DataEntry_EnumType.CurrentHP_当前HP)
								.CurrentValue / SelfDataEntry_Database
								.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).CurrentValue;
						//检查自身类型
						if (SelfBuffHolderInstance.CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_NormalEnemy_普通敌人) ==
						    BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							if (chpPartial < (info.EliminatePartial_ToNormal / 100f))
							{
								canEliminate = true;
							}
						}
						else if (SelfBuffHolderInstance.CheckTargetBuff(RolePlay_BuffTypeEnum
							._EnemyTag_EliteEnemy_精英敌人) == BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							if (info.AvailableToElite)
							{
								if (chpPartial < (info.EliminatePartial_ToElite / 100f))
								{
									canEliminate = true;
								}
							}
						}
						else if (
							SelfBuffHolderInstance.CheckTargetBuff(RolePlay_BuffTypeEnum._EnemyTag_BossEnemy_首领敌人) ==
							BuffAvailableType.Available_TimeInAndMeetRequirement)
						{
							if (info.AvailableToBoss)
							{
								if (chpPartial < (info.EliminatePartial_ToElite / 100f))
								{
									canEliminate = true;
								}
							}
						}

						if (canEliminate)
						{
							DS_ActionBusArguGroup ds_eliminateToCaster =
								new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
									.L_Damage_ToCaster_ResultToEliminateSlash_施加方_造成了斩杀效果);
							
							ds_eliminateToCaster.ObjectArgu1 = damageApplyResult;
							damageApplyResult.Caster?.ApplyDamage_GetLocalActionBus()
								.TriggerActionByType(ds_eliminateToCaster);

							int a = SelfDataEntry_Database.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP)
								.GetCeilIntValue();
							int count = (int)Math.Floor(Math.Log10(a) + 1); // 得到 位数
							int result = (int)Math.Pow(10, count) - 1; // 得到 全是9

							damageApplyResult.PopupDamageNumber = result;

							damageApplyResult.DamageResult_TakenOnHP = currentHP;
							damageApplyResult.CauseOverloadDamageEffect = true;

							DS_ActionBusArguGroup ds_TakenOnHp = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
								.L_DamageProcess_Receiver_DamageTakenOnHP_对接收方伤害打到了HP上);
							ds_TakenOnHp.ObjectArgu1 = damageApplyResult;
							GetRelatedRPBehaviour.GetRelatedActionBus().TriggerActionByType(ds_TakenOnHp);

							DS_ActionBusArguGroup ds_damageFatal = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
								.L_DamageResult_DamageResultToFatal_伤害结果导致了致命伤);
							ds_damageFatal.ObjectArgu1 = damageApplyResult;
							damageApplyResult.MayCauseDeath = true;
							GetRelatedRPBehaviour.GetRelatedActionBus().TriggerActionByType(ds_damageFatal);

							if (damageApplyResult.MayCauseDeath)
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

						}
					}

					/*
					 * 结算  动画调速
					 */
					if (!canEliminate && damageApplyInfo.DamageReceiver.ReceiveDamage_IfDataValid())
					{
						//没有处决的情况才会对受击方调速
						if (damageApplyInfo.RelatedProjectileBehaviourRuntime != null &&
						    damageApplyInfo.DamageReceiver != null)
						{
							var componentIndex_changeTimeScale =
								damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedFunctionComponentList
									.FindIndex((component =>
										component is PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver));
							if (componentIndex_changeTimeScale != -1)
							{
								PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver component_ToReceiver =
									damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedFunctionComponentList[
											componentIndex_changeTimeScale] as
										PFC_命中后动画调速接收者_HitModifyAnimationSpeedOfReceiver;
								DS_ActionBusArguGroup ds_aniSpeed = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
									.L_AnimationHelper_OnRequiredAnimationLogicSpeedMulToReceiver_动画要求逻辑速度倍率对接收者);
								ds_aniSpeed.ObjectArgu1 = component_ToReceiver;
								ds_aniSpeed.ObjectArgu2 = damageApplyInfo;
								damageApplyInfo.DamageReceiver.GetRelatedActionBus().TriggerActionByType(ds_aniSpeed);
							}
						}
					}
					if (damageApplyInfo.RelatedProjectileBehaviourRuntime != null &&
					    damageApplyInfo.RelatedProjectileBehaviourRuntime.SelfCaster != null)
					{
						var componentIndex_changeTimeScale =
							damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedFunctionComponentList.FindIndex(
								(component => component is PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster));
						if (componentIndex_changeTimeScale != -1)
						{
							PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster component_toCaster =
								damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedFunctionComponentList[
									componentIndex_changeTimeScale] as PFC_命中后动画调速发起者_HitModifyAnimationSpeedOfCaster;
							var ds_aniSpeed = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
								.L_AnimationHelper_OnRequiredAnimationLogicSpeedMulToCaster_动画要求逻辑速度倍率对发起者);
							ds_aniSpeed.ObjectArgu1 = component_toCaster;
							ds_aniSpeed.ObjectArgu2 = damageApplyInfo;
							damageApplyInfo.RelatedProjectileBehaviourRuntime.SelfCaster.GetRelatedActionBus()
								.TriggerActionByType(ds_aniSpeed);
						}
					}
				}
			}

			void ReceiveDamagePart_Buff()
			{
				
				if (damageApplyInfo.ContainApplyBuff &&
				    damageApplyInfo.BuffApplyInfoList_RuntimeCopyFromConfig != null &&
				    damageApplyInfo.BuffApplyInfoList_RuntimeCopyFromConfig.Count > 0)
				{
					foreach (ConSer_BuffApplyInfo perBuffApplyInfo in damageApplyInfo
						.BuffApplyInfoList_RuntimeCopyFromConfig)
					{
						List<BaseBuffLogicPassingComponent> blpList = perBuffApplyInfo.GetFullBLPList();
						AmendBLPInfo(blpList, damageApplyInfo.DamageReceiver);
					
						(BuffApplyResultEnum, BaseRPBuff) applyResultTuple = SelfBuffHolderInstance.TryApplyBuff(
							perBuffApplyInfo.BuffType,
							damageApplyInfo.DamageCaster as I_RP_Buff_ObjectCanApplyBuff,
							damageApplyInfo.DamageReceiver as I_RP_Buff_ObjectCanReceiveBuff,
							blpList);
						damageApplyResult.AddApplyResultInfo(perBuffApplyInfo.BuffType, applyResultTuple.Item1);
					}
				}
				if (damageApplyInfo.BuffApplyInfoList_RuntimeAdd != null &&
				    damageApplyInfo.BuffApplyInfoList_RuntimeAdd.Count > 0)
				{
					foreach (var perBuffApplyInfo in damageApplyInfo.BuffApplyInfoList_RuntimeAdd)
					{
						List<BaseBuffLogicPassingComponent> blpList = perBuffApplyInfo.BuffLogicPassingComponents;
						AmendBLPInfo(blpList, damageApplyInfo.DamageReceiver);

						(BuffApplyResultEnum, BaseRPBuff) applyResultTuple = SelfBuffHolderInstance.TryApplyBuff(
							perBuffApplyInfo.BuffType,
							damageApplyInfo.DamageCaster as I_RP_Buff_ObjectCanApplyBuff,
							damageApplyInfo.DamageReceiver as I_RP_Buff_ObjectCanReceiveBuff,
							blpList);
						damageApplyResult.AddApplyResultInfo(perBuffApplyInfo.BuffType, applyResultTuple.Item1);
					}
				}
				if (damageApplyResult.ApplyResult != null && damageApplyResult.ApplyResult.Count > 0)
				{
					damageApplyResult.ContainBuffEffect = true;
				}
			}



			//补偿BLP信息，有些BLP的数据只有运行时才有，或者说是需要运行时数据计算得到的。这时候就需要补偿这些数据
			void AmendBLPInfo(List<BaseBuffLogicPassingComponent> blpList , I_RP_Damage_ObjectCanReceiveDamage receiver)
			{
				if (blpList != null && blpList.Count > 0)
				{
					foreach (BaseBuffLogicPassingComponent perBLP in blpList)
					{
						switch (perBLP)
						{
							case Buff_失衡推拉_UnbalanceMovement.BLP_开始失衡推拉_StartUnbalanceMovementBLP blp_unbalance:
								blp_unbalance.UnbalanceDirection = Vector3.right;

								if (damageApplyInfo.RelatedProjectileBehaviourRuntime == null)
								{
									break;
								}
								if (!receiver.ReceiveDamage_IfDataValid())
								{
									break;
								}
								var receiverPos = receiver.ReceiveDamage_GetCurrentReceiverPosition();
								receiverPos.y = 0f;
								var projectilePosition = damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedGORef
									.transform.position;
								projectilePosition.y = 0f;
								Vector3 directionToReceiver = (receiverPos - projectilePosition).normalized;

								//blp_unbalance.UnbalanceDirection will be angular bisector of directionToReceiver and 	damageApplyInfo.RelatedProjectileBehaviourRuntime.DesiredMoveDirection

								blp_unbalance.UnbalanceDirection = (directionToReceiver +
								                                    damageApplyInfo.RelatedProjectileBehaviourRuntime
									                                    .DesiredMoveDirection).normalized;
								blp_unbalance.UnbalanceDirection.y = 0f;
								blp_unbalance.UnbalanceDirection = blp_unbalance.UnbalanceDirection.normalized;
								 

								break;
							case Buff_牵引推拉_DragMovement.BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP blp_drag:
								blp_drag.DragDirection = Vector3.right;

								if (damageApplyInfo.RelatedProjectileBehaviourRuntime == null)
								{
									break;
								}
								if (!receiver.ReceiveDamage_IfDataValid())
								{
									break;
								}
								var receiverPosition = receiver.ReceiveDamage_GetCurrentReceiverPosition();
								receiverPosition.y = 0f;
								var projectilePos = damageApplyInfo.RelatedProjectileBehaviourRuntime.RelatedGORef.transform.position;
								projectilePos.y = 0f;
								Vector3 dtr = (receiverPosition - projectilePos).normalized;
								//blp_unbalance.UnbalanceDirection will be angular bisector of directionToReceiver and 	damageApplyInfo.RelatedProjectileBehaviourRuntime.DesiredMoveDirection

								if (damageApplyInfo.RelatedProjectileBehaviourRuntime.DesiredMoveSpeed < 0.05f)
								{
									blp_drag.DragDirection = (dtr);
								}
								else
								{
									blp_drag.DragDirection = (dtr + damageApplyInfo.RelatedProjectileBehaviourRuntime
										.DesiredMoveDirection);
								}
								blp_drag.DragDirection.y = 0f;
								blp_drag.DragDirection = blp_drag.DragDirection.normalized;

								break;
						}
					}
				}

			}
		}


		/// <summary>
		/// <para>注意，敌人的这一部分有额外覆写。这是因为他们包含额外的死亡时强制硬直及等待</para>
		/// </summary>
		/// <param name="damageApplyResult"></param>
		/// <param name="damageApplyInfo"></param>
		/// <param name="lab_receiver"></param>
		/// <param name="lab_caster"></param>
		/// <param name="currentHPPresent"></param>
		/// <param name="effectIDStamp"></param>
		protected virtual void _ReceiveDamagePart_HPPart(
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

					if (damageApplyResult.DamageResult_TakenOnHP >= (SelfDataEntry_Database
						.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP).CurrentValue * 0.3f))
					{
						damageApplyResult.CauseOverloadDamageEffect = true;
					}

					var _ds_damageFatal = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_DamageResult_DamageResultToFatal_伤害结果导致了致命伤);
					ds_receiver.ObjectArgu1 = damageApplyResult;
					lab_receiver.TriggerActionByType(_ds_damageFatal);

					if (damageApplyResult.MayCauseDeath)
					{
						damageApplyResult.DamageResult_TakenOnHP = currentHPPresent.CurrentValue;

						currentHPPresent.ResetDataToValue(0f, true);
						var ds_death =
							new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Damage_OnFinallyDeath_伤害流程最终死亡);
						ds_death.ObjectArgu1 = _selfRelatedRPBehaviour;
						ds_death.ObjectArgu2 = damageApplyResult.Caster;
						ds_death.ObjectArguStr = damageApplyResult;
						lab_receiver.TriggerActionByType(ds_death);
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

		/// <summary>
		/// 获取Caster身上的某种数据。如果没有
		/// </summary>
		/// <returns></returns>
		protected float _Internal_GetCasterData_Safe(
			RP_DataEntry_EnumType entry,
			ref I_RP_Damage_ObjectCanApplyDamage caster,
			ref ProjectileBehaviour_Runtime projectile,
			float notExist = 0f)
		{
			if (projectile != null)
			{
				//如果空了，或者数据显式无效了
				if (caster == null || !caster.CasterDataEntryValid())
				{
					return projectile.SelfLayoutConfigReference.LayoutHandlerFunction.CasterDataEntryCache[entry];
				}
				else
				{
					return caster.ApplyDamage_GetRelatedDataEntry(entry).CurrentValue;
				}
			}
			else
			{
				//如果空了，或者数据显式无效了
				if (caster == null || !caster.CasterDataEntryValid())
				{
					return projectile.SelfLayoutConfigReference.LayoutHandlerFunction.CasterDataEntryCache[entry];
				}
				else
				{
					return notExist;
				}
			}
		}

#endregion

		public RP_DataEntry_Base GetDataEntryWhenWhenCalculatingDamageTakenThatRelatedToDataEntry(
			RP_DataEntry_EnumType type)
		{
			return SelfDataEntry_Database.GetFloatDataEntryByType(type);
		}


		public void ReceiveDataEntryEffectFromRPDS(RP_DS_DataEntryApplyInfo rpDSDataEntryApplyInfo)
		{
			//没有处理Stamp
			var newModify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(rpDSDataEntryApplyInfo.FinalModifyValue,
				rpDSDataEntryApplyInfo.ModifyFrom,
				rpDSDataEntryApplyInfo.CalculatePosition,
				rpDSDataEntryApplyInfo.Caster);
			SelfDataEntry_Database.GetFloatDataEntryByType(rpDSDataEntryApplyInfo.DataEntryTypeEnum)
				.AddDataEntryModifier(newModify);
		}

		public virtual void ClearBeforeDestroy()
		{
			//清理所有的数据项

			//clear all
			SelfBuffHolderInstance.ClearBuffHolder();
			SelfDataEntry_Database.ClearBeforeDestroy();
		}
	}
}