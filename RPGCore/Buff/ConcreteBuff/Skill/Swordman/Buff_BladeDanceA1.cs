using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager;
using DG.Tweening;
using GameplayEvent;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using Global.Utility;
using RPGCore.AssistBusiness;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RPGCore.Buff.ConcreteBuff.Skill.Swordman
{
	[Serializable]
	public class Buff_BladeDanceA1 : BaseRPBuff
	{
		/*
		 * https://xxi1p77cfp.feishu.cn/wiki/M5gLwB07BiobC9k8NNyc9E1cnDg
		 * [剑舞buff]
每秒消耗少量SP。
      当SP少于可再次进行伤害追击的SP量时，[结束技能]
当玩家造成任意攻击，则 
  当此次攻击的施加者 等同于 此buff的持有者时，进行[伤害追加]。每次伤害追加，都会消耗一定SP。
  追加可以设定范围，当范围足够大时即为始终追加。
  如果该施加者有有效的[破绽标记]，此次追加不消耗SP。
  
  内部实现细节：
    每次试图追加伤害时会 消耗SP 并 生成一个 追加事务，，事务记录了需要使用的特效和延迟时间。
    当 当前游戏时间 大于 追加事务 记录的时间，生成特效、生成伤害、移除追加事务。
    （当buff被移除后，未触发的事务也会跟着被移除。也即当切换角色、关闭buff时有可能吞掉最后那几下伤害，SP也会被跟着吞掉）
		 */
		
		
		private FloatPresentValue_RPDataEntry _fpv_SPEntry;
		private Float_RPDataEntry _entry_MaxSP;

		[LabelText("每秒消耗SP量"), TitleGroup("===数值==="), SerializeField]
		public float SPConsumePerSecond = 2f;

		/// <summary>
		/// 下次消耗时间点
		/// </summary>
		private float _nextConsumeTime;

		[LabelText("每次追击消耗的SP量"), TitleGroup("===数值==="), SerializeField]
		public float SPConsumePerChase = 10f;
		
		[LabelText("开启后免伤百分比"), SuffixLabel("%"), SerializeField]
		private float _resistBonus = 10f;
		
		[LabelText("剑舞触发概率 "), TitleGroup("===数值==="), SerializeField]
		public float TriggerProbability = 0.6f;
		
		[LabelText("追击使用的伤害配置"), TitleGroup("===数值==="), SerializeField]
		public ConSer_DamageApplyInfo _damageApplyInfo_Chase = new ConSer_DamageApplyInfo
		{
			DamageType = DamageTypeEnum.None,
			DamageFromFlag = DamageFromTypeFlag.None_未指定,
			DamageTryTakenBase = 1,
			DamageTakenRelatedDataEntry = true,
			RelatedDataEntryInfos = new List<ConSer_DataEntryRelationConfig>()
			{
				new ConSer_DataEntryRelationConfig
				{
					RelatedToReceiver = false,
					RelatedDataEntryType = RP_DataEntry_EnumType.AttackSpeed_攻击速度,
					Partial = 1,
					CalculatePosition = (ModifyEntry_CalculatePosition)ModifyEntry_CalculatePosition.Original,
					CacheDataEntryValue = 0
				}
			},
			ContainBuffEffect = false,
		};

		[LabelText("vfxID_剑舞循环"), TitleGroup("===VFX==="), SerializeField, GUIColor(187f / 255f, 1f, 0f)]
		public string _vfxID_BuffLoop;

		private PerVFXInfo _vfxInfo_BuffLoop;


		[LabelText("vfxID_剑舞触发 _剑气发生"), TitleGroup("===VFX==="), SerializeField, GUIColor(187f / 255f, 1f, 0f)]
		public string _vfxID_Trigger;

		[LabelText("vfxID_剑舞触发 _追击剑气"), TitleGroup("===VFX==="), SerializeField, GUIColor(187f / 255f, 1f, 0f)]
		public string _vfxID_Chase;


		[LabelText("vfx_剑舞_触发高度 "), TitleGroup("===VFX==="), SerializeField]
		public float _vfx_TriggerHeight = 1.25f;


		[LabelText("vfx_剑舞触发半径 "), TitleGroup("===VFX==="), SerializeField]
		public float _vfx_TriggerRadius = 1.25f;
		
		[LabelText("vfx_飞行时长") , TitleGroup("===VFX==="), SerializeField]
		public float _vfx_FlyDuration = 0.5f;
		
		[LabelText("Buff触发追击时的事件们")]
		[TitleGroup("===事件&参数===")]
		[PropertyOrder(-10)]
		public List<SOConfig_PrefabEventConfig> OnBuffTriggerEventList;

		

		[LabelText("追击的伤害结算延迟_最小"), TitleGroup("===数值==="), SerializeField]
		public float _chaseDamageDelay = 0.2f;

		[LabelText("追击的伤害结算延迟_最大"), TitleGroup("===数值==="), SerializeField]
		public float _chaseDamageDelayMax = 0.5f;

		/// <summary>
		/// <para>追击事务信息</para>
		/// </summary>
		public class PerChaseTransactionInfo
		{
			public I_RP_Damage_ObjectCanReceiveDamage _receiver;
			public float WillTakeChaseTime;
		}
		
		
		private List<PerChaseTransactionInfo > _list_ChaseTransactionInfo = new List<PerChaseTransactionInfo>();
		
		

		private Buff_ChangeCommonDamageType _buff_ChangeCommonDamageType;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_nextConsumeTime = BaseGameReferenceService.CurrentFixedTime + 1f;
			_vfxInfo_BuffLoop = _VFX_GetAndSetBeforePlay(_vfxID_BuffLoop,true,true,GetCurrentDamageType)?._VFX__10_PlayThis();
			
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
				_ABC_TryAddNewChaseAttackTransaction);
			_buff_ChangeCommonDamageType =
				parent.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.ChangeCommonDamageType_常规伤害类型更改) as Buff_ChangeCommonDamageType;
			_fpv_SPEntry = parent.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentSP_当前SP);
			_entry_MaxSP = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.SPMax_最大SP);
			
			;
		}


		public override DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
		{
			return base.OnExistBuffRefreshed(caster, blps);
		}
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			// if (_vfxInfo_BuffLoop == null)
			// {
			// 	_vfxInfo_BuffLoop = _VFX_GetOrInstantiateNew(_vfxConfig_BuffLoop);
			// }
			// _vfxInfo_BuffLoop._VFX__10_PlayThis();

			return base.OnBuffInitialized(blps);
		}

		private void _ABC_CheckIfNeedReduceDamage(DS_ActionBusArguGroup ds)
		{
			var dar = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			if (dar == null)
			{
				return;
			}
			dar.CP_DamageAmount_DPart.MultiplyPart -= (_resistBonus / 100f);
		}
		
		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);

			//每秒消耗
			if (currentTime > _nextConsumeTime)
			{
				_nextConsumeTime = currentTime + 1f;

				//如果剩余量已经不能再追加了，自动移除这个buff'
				if (_fpv_SPEntry.CurrentValue < SPConsumePerChase)
				{
					MarkAsRemoved = true;
					return;
				}
				var modify = Float_ModifyEntry_RPDataEntry.GetNewFromPool(-SPConsumePerSecond,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontAdd,
					this);
				_fpv_SPEntry.AddDataEntryModifier(modify);



			}


			for (int i = _list_ChaseTransactionInfo.Count - 1; i >= 0; i--)
			{
				var tmpInfo = _list_ChaseTransactionInfo[i];
				if (tmpInfo.WillTakeChaseTime < currentTime)
				{
					continue;
				}
                
				//进行伤害追加
				//要检查Receiver是不是还存在并且有效
				if (tmpInfo._receiver == null || !tmpInfo._receiver.ReceiveDamage_IfDataValid())
				{
					continue;
				}
				_damageApplyInfo_Chase.DamageType = _buff_ChangeCommonDamageType.CurrentDamageType;
				
				var newDAI = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromFromConSer(_damageApplyInfo_Chase,
					tmpInfo._receiver,
					Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
					null);
				//触发事件
				var dd = 
				_damageAssistServiceRef.ApplyDamage(newDAI);
					dd.ReleaseToPool();
					
					if (OnBuffTriggerEventList  != null && OnBuffTriggerEventList.Count > 0)
					{
						foreach (var perEvent in OnBuffTriggerEventList)
						{
							GameplayEventManager.Instance.StartGameplayEvent(perEvent);
						}
					}
					
				GenericPool<PerChaseTransactionInfo>.Release(tmpInfo);
				_list_ChaseTransactionInfo.RemoveAt(i);
			}
		}


		/// <summary>
		/// 试图追加一次伤害
		/// </summary>
		/// <param name="ds"></param>
		protected virtual void _ABC_TryAddNewChaseAttackTransaction(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			//不是同一个caster，直接return
			
			
			if (!ReferenceEquals(dar.Caster, Parent_SelfBelongToObject) )
			{
				return;
			}
			//是任意一种玩家伤害，才能追加
			if (dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerNormalAttack_玩家普攻伤害) ||
			    dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerSkillAttack_玩家技能伤害) ||
			    dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerUltraAttack_玩家超杀伤害) ||
			    dar.DamageFromTypeFlags.HasFlag(DamageFromTypeFlag.PlayerCoopAttack_玩家连协伤害))
			{		
				var rd =  Random.Range(0f,1f);

				if (rd > TriggerProbability)
				{
					return;
				}
				
				PerChaseTransactionInfo newTransaction = GenericPool<PerChaseTransactionInfo>.Get();
				 newTransaction._receiver = dar.Receiver;
				 newTransaction.WillTakeChaseTime = BaseGameReferenceService.CurrentFixedTime +
				                                    UnityEngine.Random.Range(_chaseDamageDelay, _chaseDamageDelayMax);
				_list_ChaseTransactionInfo.Add(newTransaction);
				
				
				
				
				//生成特效
				
				

				var selfPos = Parent_SelfBelongToObject.GetBuffReceiverPosition();
				selfPos.y += _vfx_TriggerHeight;
				var randomR = MathExtend.Vector3RotateOnXOZ(Vector3.right, Random.Range(0, 360f));
				randomR *= _vfx_TriggerRadius;
				selfPos += randomR;


				var _triggerVFX = _VFX_GetAndSetBeforePlay(_vfxID_Trigger,  true, true, GetCurrentDamageType)._VFX__10_PlayThis()
					._VFX_2_SetPositionToGlobalPosition(selfPos);
				

				var bonePos = (dar.Receiver.GetRelatedArtHelper() as I_RP_ContainVFXContainer)
					.GetVFXHolderGlobalPosition("剑舞追击点","胸部").Item1.Value;
				var direction = (bonePos - selfPos).normalized;

				var pp = _VFX_GetAndSetBeforePlay(_vfxID_Chase, true,true,GetCurrentDamageType)._VFX__10_PlayThis()
					._VFX_2_SetPositionToGlobalPosition(selfPos)._VFX__3_SetDirectionOnForwardOnGlobalAll(direction);
				
				// pp.CurrentActiveRuntimePSPlayProxyRef.transform
				// 	.DOMove(dar.Receiver.ReceiveDamage_GetCurrentReceiverPosition(), _vfx_FlyDuration)
				// 	.SetEase(Ease.OutCubic).OnComplete((() => { pp.VFX_StopThis(false); }));
				
				
				
				//如果没有看破标记，就耗蓝
				if (dar.Receiver.ReceiveDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
					    .FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough) !=
				    BuffAvailableType.Available_TimeInAndMeetRequirement)
				{
					var dd = Float_ModifyEntry_RPDataEntry.GetNewFromPool(-SPConsumePerChase,
						RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
						ModifyEntry_CalculatePosition.FrontAdd,
						this);
					_fpv_SPEntry.AddDataEntryModifier(dd);
				}

			}
		}
		
		
		
		


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			_vfxInfo_BuffLoop?.VFX_StopThis();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_CheckIfNeedReduceDamage);
			
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
				_ABC_TryAddNewChaseAttackTransaction);
			for (int i = _list_ChaseTransactionInfo.Count - 1; i >= 0; i--)
			{
				GenericPool<PerChaseTransactionInfo>.Release(_list_ChaseTransactionInfo[i]);
			}
			_list_ChaseTransactionInfo.Clear();
			
			
			return base.OnBuffPreRemove();
		}
	}
}