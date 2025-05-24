//#pragma warning disable CS0162
//#pragma warning disable CS0414

using System;
using System.Collections.Generic;
using System.Dynamic;
using ARPG.Character.Base;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace RPGCore.UtilityDataStructure
{

	/// <summary>
	/// <para>伤害类型的枚举。</para>
	/// </summary>
	public enum DamageTypeEnum
	{
		None,
		/// <summary>
		/// <para>无属性，比如反弹、物理普攻</para>
		/// </summary>
		NoType_无属性 = 1,
		YuanNengGuang_源能光 = 101,
		YuanNengDian_源能电 = 102,
		AoNengTu_奥能土 = 201,
		AoNengShui_奥能水 = 202,
		AoNengHuo_奥能火 = 203,
		AoNengFeng_奥能风 = 204,
		LingNengLing_灵能灵 = 301,
		YouNengXingHong_幽能猩红 = 401,
		YouNengAnYing_幽能暗影 = 402,
		YouNengHunDun_幽能混沌 = 403,
		Heal_治疗 = 900,
		TrueDamage_真伤 = 1000,

	}

	
	
	
	[Flags]
	public enum DamageFromTypeFlag
	{
		None_未指定 = 0,
		PlayerNormalAttack_玩家普攻伤害 = 1,
		PlayerSkillAttack_玩家技能伤害 = 2,
		PlayerUltraAttack_玩家超杀伤害 = 4,
		PlayerCoopAttack_玩家连协伤害 = 8,
		
	}



	[Serializable]
	
	public struct DamageProcessStepOption
	{

#if UNITY_EDITOR
		[Button("默认"),HorizontalGroup("1"),PropertyOrder(-1)]
		public void _button_SetAsDefault()
		{
			this = DamageProcessStepOption.DefaultDPSO_NormalAttack();
		}
		[Button("治疗"),HorizontalGroup("1"), PropertyOrder(-1)]
		public void _button_SetAsHeal()
		{
			this = DamageProcessStepOption.HealDPS();
		}
		
		[Button("真伤"),HorizontalGroup("1"), PropertyOrder(-1)]
		public void _button_SetAsTrueDamage()
		{
			this = DamageProcessStepOption.TrueDamageDPS();
		}
#endif
		[LabelText("无视【开始序言】部分？")]
		public bool IgnorePrelude;
		[LabelText("无视【闪避】部分？")]
		public bool IgnoreDodge;
		[LabelText("无视【前加算区】(防御)部分？")]
		public bool IgnoreFrontAddPart_AttackAndDefense;
		[LabelText("无视【技能威力】部分？")]
		public bool IgnoreSkillPower;
		[LabelText("无视【前乘算区】(额外伤害)部分？")]
		public bool IgnoreExtraDamage_ResistAndFragile;
		[LabelText("无视【暴击】部分？")]
		public bool IgnoreCritical;
		[LabelText("无视【后乘算区】(最终增伤)部分？")]
		public bool IgnoreRearMulPart_FinalBonus;
		[LabelText("不进行伤害保底？")]
		public bool IgnoreGuarantee;
		[LabelText("无视【过盾】部分？")]
		public bool IgnoreShieldPassing;
		[LabelText("无视【血量】部分？")]
		public bool IgnoreHealth;

		[LabelText("不进行【跳字】？")]
		public bool IgnorePopup;

		[LabelText("不进行【染色】？")]
		public bool IgnoreTint;

		[LabelText("无视【buff】部分？")]
		public bool IgnoreBuff;
		
		
		/// <summary>
		/// <para>一个正常的伤害流程步骤参数，所有步骤都会参与</para>
		/// </summary>
		/// <returns></returns>
		public static DamageProcessStepOption DefaultDPSO_NormalAttack()
		{
			DamageProcessStepOption newOption = new DamageProcessStepOption();
			newOption.IgnorePrelude = false;
			newOption.IgnoreDodge = false;
			newOption.IgnoreFrontAddPart_AttackAndDefense = false;
			newOption.IgnoreSkillPower = false;
			newOption.IgnoreExtraDamage_ResistAndFragile = false;
			newOption.IgnoreCritical = false;
			newOption.IgnoreRearMulPart_FinalBonus = false;
			newOption.IgnoreShieldPassing = false;
			newOption.IgnoreGuarantee = false;
			newOption.IgnorePopup = false;
			newOption.IgnoreHealth = false;
			newOption.IgnoreBuff = false;
			newOption.IgnoreTint = false;
			return newOption;
		}


		/// <summary>
		/// <para>常用于【治疗】的DPSO。包括序言。由于治疗本身的伤害流程就和常规的不一样。所以实际有意义的只需要考虑里面的 跳字、血量、buff</para>
		/// </summary>
		/// <returns></returns>
		public static DamageProcessStepOption HealDPS()
		{
			DamageProcessStepOption newOption = new DamageProcessStepOption();
			newOption.IgnorePrelude = false;
			newOption.IgnoreDodge = true;
			newOption.IgnoreFrontAddPart_AttackAndDefense = true;
			newOption.IgnoreSkillPower = false;
			newOption.IgnoreExtraDamage_ResistAndFragile = true;
			newOption.IgnoreCritical = true;
			newOption.IgnoreRearMulPart_FinalBonus = false;
			newOption.IgnoreShieldPassing = true;
			newOption.IgnoreGuarantee = false;
			newOption.IgnorePopup = false;
			newOption.IgnoreHealth = false;
			newOption.IgnoreBuff = false;
			newOption.IgnoreTint = false;
			return newOption;
		}
		
		public static DamageProcessStepOption TrueDamageDPS()
		{
			DamageProcessStepOption newOption = new DamageProcessStepOption();
			newOption.IgnorePrelude = false;
			newOption.IgnoreDodge = true;
			newOption.IgnoreFrontAddPart_AttackAndDefense = true;
			newOption.IgnoreSkillPower = false;
			newOption.IgnoreExtraDamage_ResistAndFragile = true;
			newOption.IgnoreCritical = true;
			newOption.IgnoreRearMulPart_FinalBonus = false;
			newOption.IgnoreShieldPassing = true;
			newOption.IgnoreGuarantee = false;
			newOption.IgnorePopup = false;
			newOption.IgnoreHealth = false;
			newOption.IgnoreBuff = false;
			newOption.IgnoreTint = false;
			return newOption;
			 
		}

		/// <summary>
		/// <para>直接造成纯数值的伤害，忽略了所有可能影响数值的步骤</para>
		/// </summary>
		public static DamageProcessStepOption TrueDamageDirectValueDPS()
		{
			DamageProcessStepOption newOption = new DamageProcessStepOption();
			newOption.IgnorePrelude = false;
			newOption.IgnoreDodge = true;
			newOption.IgnoreFrontAddPart_AttackAndDefense = true;
			newOption.IgnoreSkillPower = true;
			newOption.IgnoreExtraDamage_ResistAndFragile = true;
			newOption.IgnoreCritical = true;
			newOption.IgnoreRearMulPart_FinalBonus = true;
			newOption.IgnoreShieldPassing = true;
			newOption.IgnoreGuarantee = false;
			newOption.IgnorePopup = false;
			newOption.IgnoreHealth = false;
			newOption.IgnoreBuff = false;
			newOption.IgnoreTint = true;
			return newOption;
		}

		
		/// <summary>
		/// <para>【追加伤害】用的DPSO。追加伤害仅参与后乘区的计算，即不会暴击不计算防御不会闪避</para>
		/// </summary>
		/// <returns></returns>
		public static DamageProcessStepOption AppendDamageDPS()
		{
			DamageProcessStepOption newOption = new DamageProcessStepOption();
			newOption.IgnorePrelude = false;
			newOption.IgnoreDodge = true;
			newOption.IgnoreFrontAddPart_AttackAndDefense = true;
			newOption.IgnoreSkillPower = true;
			newOption.IgnoreExtraDamage_ResistAndFragile = true;
			newOption.IgnoreCritical = true;
			newOption.IgnoreRearMulPart_FinalBonus = false;
			newOption.IgnoreShieldPassing = false;
			newOption.IgnorePopup = false;
			newOption.IgnoreGuarantee = false;
			newOption.IgnoreHealth = false;
			newOption.IgnoreBuff = false;
			newOption.IgnoreTint = true;
			return newOption;
		}


		/// <summary>
		/// <para>【自爆】用的DPSO。只结算生命值，其他啥都没有</para>
		/// </summary>
		/// <returns></returns>
		public static DamageProcessStepOption SelfExplosionDPS()
		{
			DamageProcessStepOption newOption = new DamageProcessStepOption();
			newOption.IgnorePrelude = true;
			newOption.IgnoreDodge = true;
			newOption.IgnoreFrontAddPart_AttackAndDefense = true;
			newOption.IgnoreSkillPower = true;
			newOption.IgnoreExtraDamage_ResistAndFragile = true;
			newOption.IgnoreCritical = true;
			newOption.IgnoreRearMulPart_FinalBonus = true;
			newOption.IgnoreShieldPassing = true;
			newOption.IgnorePopup = true;
			newOption.IgnoreGuarantee = true;
			newOption.IgnoreHealth = false;
			newOption.IgnoreBuff = true;
			newOption.IgnoreTint = true;
			return newOption;
		}
	}

	/// <summary>
	/// <para>伤害流程中，用于提供伤害各种信息的数据结构</para>
	/// <para>总是会在进入伤害流程前被(从池中)构建。</para>
	/// <para>必定有一个caster和receiver。Projectile是可选项</para>
	/// <para></para>
	/// </summary>
	public class RP_DS_DamageApplyInfo
	{
		[ShowInInspector]
		public I_RP_Damage_ObjectCanReceiveDamage DamageReceiver;
		[ShowInInspector]
		public I_RP_Damage_ObjectCanApplyDamage DamageCaster;

		[ShowInInspector]
		public ProjectileBehaviour_Runtime RelatedProjectileBehaviourRuntime;

		[ShowInInspector, LabelText("伤害时间戳")]
		public int DamageTimestamp;
		
		[ShowInInspector]
		public DamageFromTypeFlag DamageFromTypeFlag;

		[ShowInInspector]
		public DamageTypeEnum DamageType;

		[ShowInInspector, LabelText("DamageApplyInfo被设置时的伤害量。")]
		public float DamageTakenBase;
		
		public DamageProcessStepOption StepOption = DamageProcessStepOption.DefaultDPSO_NormalAttack();
		
		/// <summary>
		/// 表现为空则通常表示这不是一个来自Projectile的伤害，这时候伤害位置直接算作接收者自身位置
		/// </summary>
		public Nullable<Vector3> DamageWorldPosition;
		/// <summary>
		/// 表现为空的意思就是没有设置，将会进行一个报错。伤害方向指的是 伤害发生点 到 伤害接收点 的方向
		/// </summary>
		[ShowInInspector, LabelText("伤害方向V3")]
		public Nullable<Vector3> DamageDirectionV3;
		
		/// <summary>
		/// <para>力度并不等同于伤害方向。有些时候为了看上去更河里，力度的方向和伤害的方向并不一致。</para>
		/// </summary>
		[ShowInInspector,LabelText("力度方向V3")]
		public Nullable<Vector3> ForceDirectionV3;
		
		[ShowInInspector, LabelText("包含Buff施加信息")]
		public bool ContainApplyBuff;

		/// <summary>
		/// <para>仅仅用于运行时的临时储存。它的内容都是来自各方面的拷贝，但一定都是配置性质的。</para>
		/// <para>所以这里面的内容在释放时，不会释放里面的内容，只是会清除容器</para>
		/// </summary>
		[ShowInInspector, LabelText("Buff施加信息-运行时从配置中拷贝")]
		public List<ConSer_BuffApplyInfo> BuffApplyInfoList_RuntimeCopyFromConfig = new List<ConSer_BuffApplyInfo>();
		
		
		/// <summary>
		/// <para>用于运行时的临时储存。它的内容来自非配置性质的数据，所以在这里面的内容在释放时，连带里面的具体内容会一并被释放掉</para>
		/// </summary>
		[ShowInInspector,LabelText("Buff施加信息-运行时添加")]
		public List<BuffApplyInfo_Runtime> BuffApplyInfoList_RuntimeAdd = new List<BuffApplyInfo_Runtime>();

		public object DropItemInfoRef;


		public void ReleaseBeforeToPool()
		{
			GenericPool<RP_DS_DamageApplyInfo>.Release(this);
		}
		//reset all content
		public void ResetAfterGetFromPool()
		{
			DamageReceiver = null;
			DamageCaster = null;
			RelatedProjectileBehaviourRuntime = null;
			DamageTimestamp = 0;
			DamageFromTypeFlag = DamageFromTypeFlag.None_未指定;
			DamageType = DamageTypeEnum.None;
			DamageTakenBase = 0f;
			StepOption = DamageProcessStepOption.DefaultDPSO_NormalAttack();
			DamageWorldPosition = null;
			DamageDirectionV3 = null;
			ContainApplyBuff = false;
			BuffApplyInfoList_RuntimeCopyFromConfig.Clear();
			DropItemInfoRef = null;
			
			
			
			if (BuffApplyInfoList_RuntimeAdd != null && BuffApplyInfoList_RuntimeAdd.Count > 0)
			{
				foreach (BuffApplyInfo_Runtime perAI in BuffApplyInfoList_RuntimeAdd)
				{
					perAI.ReleaseBeforeReturnToPool();
				}
			}
			BuffApplyInfoList_RuntimeAdd.Clear();

		}



		/// <summary>
		/// <para>在运行时构建一个DamageApplyInfo</para>
		/// <para>！！！注意，获取的RPDS是对象池中的，用完记得还回去！！！</para>
		/// </summary>
		public static RP_DS_DamageApplyInfo BuildDamageApplyInfoFromRuntime(
			I_RP_Damage_ObjectCanReceiveDamage receiver,
			I_RP_Damage_ObjectCanApplyDamage caster,
			DamageTypeEnum damageType,
			float damage,
			DamageProcessStepOption option = default,
			DamageFromTypeFlag flags = DamageFromTypeFlag.None_未指定)
		{
			RP_DS_DamageApplyInfo newRPDS = GenericPool<RP_DS_DamageApplyInfo>.Get();
			newRPDS.ResetAfterGetFromPool();
			newRPDS.DamageReceiver = receiver;
			newRPDS.DamageCaster = caster;
			newRPDS.DamageFromTypeFlag = flags;
			newRPDS.DamageType = damageType;
			newRPDS.DamageTakenBase = damage;
			newRPDS.StepOption = option;

			return newRPDS;
		}



		/// <summary>
		/// <para>根据ConSer内配置的DataEntry来计算伤害。伤害流程本身是只接受RPDS_DAI的，所以这总是用于由配置中的伤害变成运行时伤害信息的</para>
		/// <para>！！！注意，获取的RPDS是对象池中的，用完记得还回去！！！【如果是通过DamageAssist的ApplyDamage，那最后已经释放了】</para>
		/// </summary>
		public static RP_DS_DamageApplyInfo BuildDamageApplyInfoFromFromConSer(
			ConSer_DamageApplyInfo conserApplyInfo,
			I_RP_Damage_ObjectCanReceiveDamage receiver,
			I_RP_Damage_ObjectCanApplyDamage caster, ProjectileBehaviour_Runtime pr)
		{
			RP_DS_DamageApplyInfo newRPDS = GenericPool<RP_DS_DamageApplyInfo>.Get();
			newRPDS.ResetAfterGetFromPool();
			
			newRPDS.DamageReceiver = receiver;
			newRPDS.DamageCaster = caster;
			newRPDS.RelatedProjectileBehaviourRuntime = pr;
			
			newRPDS.DamageFromTypeFlag = conserApplyInfo.DamageFromFlag;
			newRPDS.DamageType = conserApplyInfo.DamageType;
			
			
			newRPDS.DamageTakenBase = GetDamageTakenValue(conserApplyInfo, receiver, caster);
			newRPDS.StepOption = conserApplyInfo.ProcessOption;
			newRPDS.ContainApplyBuff = conserApplyInfo.ContainBuffEffect;
			
			
			if (conserApplyInfo.ContainBuffEffect )
			{
				if(conserApplyInfo.BuffEffectArray != null && conserApplyInfo.BuffEffectArray.Length > 0)
				{
					foreach (var perBAI in conserApplyInfo.BuffEffectArray)
					{
						newRPDS.BuffApplyInfoList_RuntimeCopyFromConfig.Add(perBAI);
					}
				}
				if(conserApplyInfo.BuffEffectArray_File != null && conserApplyInfo.BuffEffectArray_File.Length > 0)
				{
					foreach (var perConfig in conserApplyInfo.BuffEffectArray_File)
					{
						if(perConfig.BuffEffectList == null || perConfig.BuffEffectList.Count == 0)
						{
							continue;
						}
						foreach (var perBAI in perConfig.BuffEffectList)
						{
							newRPDS.BuffApplyInfoList_RuntimeCopyFromConfig.Add(perBAI);
						}
					}
				}
			}
			
			return newRPDS;
		}


		public static float GetDamageTakenValue(
			ConSer_DamageApplyInfo conserApplyInfo,
			I_RP_Damage_ObjectCanReceiveDamage receiver,
			I_RP_Damage_ObjectCanApplyDamage caster)
		{
			float baseValue = conserApplyInfo.DamageTryTakenBase;
			if (conserApplyInfo.DamageTakenRelatedDataEntry)
			{
				Span<_CalculatePositionPair> currentCP =
					stackalloc _CalculatePositionPair[conserApplyInfo.RelatedDataEntryInfos.Count];

				for (int i = 0; i < conserApplyInfo.RelatedDataEntryInfos.Count; i++)
				{
					ConSer_DataEntryRelationConfig dataEntryInfo = conserApplyInfo.RelatedDataEntryInfos[i];
					///////////////////////////////
					//使用接受者的数据项
					//////////////////////////
					if (dataEntryInfo.RelatedToReceiver)
					{
						Float_RPDataEntry de =
							receiver.ReceiveDamage_GetRelatedDataEntry(dataEntryInfo.RelatedDataEntryType) as
								Float_RPDataEntry;

						currentCP[i].Value = (de).GetCurrentValue() * dataEntryInfo.Partial;
						currentCP[i].Position = dataEntryInfo.CalculatePosition;
					}
					////////////////////////////////
					//默认情况，关联施加者的 DataEntry
					/////////////////////////////
					else
					{
						float value = 0f;
						//caster已经无了，那就用cache值
						if (caster == null || !(caster as BaseARPGCharacterBehaviour).CharacterDataValid)
						{
							value = dataEntryInfo.CacheDataEntryValue;
						}
						else
						{
							RP_DataEntry_Base de =
								caster.ApplyDamage_GetRelatedDataEntry(dataEntryInfo.RelatedDataEntryType);
							value = (de as Float_RPDataEntry).GetCurrentValue();
							dataEntryInfo.CacheDataEntryValue = value;
						}

						//采用上界计算

						currentCP[i].Value = value * dataEntryInfo.Partial;
						currentCP[i].Position = dataEntryInfo.CalculatePosition;
					}
				}
				float o = 0f, fa = 0f, ra = 0f, fm = 1f, rm = 1f;
				foreach (_CalculatePositionPair positionPair in currentCP)
				{
					switch (positionPair.Position)
					{
						case ModifyEntry_CalculatePosition.Original:
							o += positionPair.Value;
							break;
						case ModifyEntry_CalculatePosition.FrontAdd:
							fa += positionPair.Value;
							break;
						case ModifyEntry_CalculatePosition.FrontMul:
							fm += positionPair.Value / 100f;
							break;
						case ModifyEntry_CalculatePosition.RearAdd:
							ra += positionPair.Value;
							break;
						case ModifyEntry_CalculatePosition.RearMul:
							rm += positionPair.Value / 100f;
							break;
					}
				}

				baseValue = (((o + fa) * fm) + ra) * rm + baseValue;
			}
			else
			{
				baseValue = conserApplyInfo.DamageTryTakenBase;
			}
			return baseValue;
		}

		private struct _CalculatePositionPair
		{
			public float Value;
			public ModifyEntry_CalculatePosition Position;
		}

		
		
		
		
	}

	public static class DamageApplyInfoExtend
	{
		
		
		
		
	}
}