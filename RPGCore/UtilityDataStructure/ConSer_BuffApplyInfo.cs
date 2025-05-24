using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Manager.Config;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Aura;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Level;
using RPGCore.Buff.ConcreteBuff.Shield;
using RPGCore.Buff.ConcreteBuff.SpecificCharacter;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.Buff.Config;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RPGCore.UtilityDataStructure
{
	[Serializable]
	public class ConSer_BuffApplyInfo
	{

		public override string ToString()
		{
			return $"{BuffType} 和{ (BuffLogicPassingComponents != null && BuffLogicPassingComponents.Count > 0 ? BuffLogicPassingComponents.Count : 0)}个参数组";
		}
#if UNITY_EDITOR
		[OnValueChanged(nameof(_OVC_CheckIfAddRelatedBLP))]
#endif
		[LabelText("Buff类型标签")]
		public RolePlay_BuffTypeEnum BuffType;

		[ShowInInspector, SerializeReference, LabelText("Buff传递参数组-直属")]
		protected List<BaseBuffLogicPassingComponent> BuffLogicPassingComponents;
		
		[SerializeField, LabelText("Buff传递参数组-文件")]
		protected List<SOConfig_BLPConfig> BuffEffectArray_File;


		
		protected static List<BaseBuffLogicPassingComponent> _internalBLPList = new List<BaseBuffLogicPassingComponent>();


		/// <summary>
		/// <para>获取所有的传递参数组。使用的是一个static的List，所以每次使用前都会Clear，不要试图cache它</para>
		/// </summary>
		/// <returns></returns>
		public List<BaseBuffLogicPassingComponent> GetFullBLPList()
		{
			_internalBLPList.Clear();
			if (this.BuffLogicPassingComponents != null && this.BuffLogicPassingComponents.Count > 0)
			{
				_internalBLPList.AddRange(this.BuffLogicPassingComponents);
			}
			if (this.BuffEffectArray_File != null && this.BuffEffectArray_File.Count > 0)
			{
				foreach (SOConfig_BLPConfig perConfig in this.BuffEffectArray_File)
				{
					if (perConfig.BLPList_Serialize != null && perConfig.BLPList_Serialize.Length > 0)
					{
						_internalBLPList.AddRange(perConfig.BLPList_Serialize);
					}
				}
			}
			return _internalBLPList;
		}
		

#if UNITY_EDITOR

		/// <summary>
		/// <para>检查一下是不是需要添加关联的那个BLP</para>
		/// </summary>
		private void _OVC_CheckIfAddRelatedBLP()
		{
			if (BuffType != RolePlay_BuffTypeEnum.None)
			{
				if (BuffLogicPassingComponents == null)
				{
					BuffLogicPassingComponents = new List<BaseBuffLogicPassingComponent>();
				}
			}
			switch (BuffType)
			{
				case RolePlay_BuffTypeEnum.StiffnessOnHit_受击硬直:
					if (!Check<Buff_受击硬直_StiffnessOnHit.BLP_受击硬直通用_StiffnessOnHitBLP>())
					{
						BuffLogicPassingComponents.Add(new Buff_受击硬直_StiffnessOnHit.BLP_受击硬直通用_StiffnessOnHitBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.DragMovement_牵引推拉:
					if (!Check<Buff_牵引推拉_DragMovement.BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP>())
					{
						BuffLogicPassingComponents.Add(new Buff_牵引推拉_DragMovement.
							BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.UnbalanceMovement_失衡推拉 : 
					if (!Check<Buff_失衡推拉_UnbalanceMovement.BLP_开始失衡推拉_StartUnbalanceMovementBLP>())
					{
						BuffLogicPassingComponents.Add(new Buff_失衡推拉_UnbalanceMovement.
							BLP_开始失衡推拉_StartUnbalanceMovementBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.Aura_CommonHealAura_通用治疗光环:
					case RolePlay_BuffTypeEnum.Aura_SimpleFireAura_简单灼烧光环:
					if (!Check<Buff_CommonAuraBase.BLP_基本光环构建信息_BaseAuraBuild>())
					{
						BuffLogicPassingComponents.Add(new Buff_CommonAuraBase.BLP_基本光环构建信息_BaseAuraBuild());
					}
					break;
				case RolePlay_BuffTypeEnum.Shield_Energy_属性能量甲盾 :
					if (!Check<Buff_通用能量甲盾_CommonEnergyShield.BLP_能量甲盾初始化参数_CommonEnergyShieldBLP>())
					{
						BuffLogicPassingComponents.Add(
							new Buff_通用能量甲盾_CommonEnergyShield.BLP_能量甲盾初始化参数_CommonEnergyShieldBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.Level_图腾免伤_ResistTotem :
					if (!Check<Buff_Totem_图腾免伤_ResistTotem.BLP_免伤图腾的配置信息_ResistTotemConfig>())
					{
						BuffLogicPassingComponents.Add(
							new Buff_Totem_图腾免伤_ResistTotem.BLP_免伤图腾的配置信息_ResistTotemConfig());
					}
					break;
				case RolePlay_BuffTypeEnum.Level_图腾召唤_SummonTotem:
					if (!Check<Buff_Totem_图腾召唤_SummonTotem.BLP_召唤图腾的配置信息_SummonTotemConfig>())
					{
						BuffLogicPassingComponents.Add(
							new Buff_Totem_图腾召唤_SummonTotem.BLP_召唤图腾的配置信息_SummonTotemConfig());
					}
					break;
				case RolePlay_BuffTypeEnum.Level_图腾回血_HealTotem : 
					if (!Check<Buff_Totem_图腾回血_HealTotem.BLP_回血图腾配置信息_HealTotemConfig>())
					{
						BuffLogicPassingComponents.Add(
							new Buff_Totem_图腾回血_HealTotem.BLP_回血图腾配置信息_HealTotemConfig());
					}
					break;
				case RolePlay_BuffTypeEnum.RemoveBuff_移除Buff:
					if(! Check<Buff_移除其他Buff_UtilityRemoveOtherBuff.BLP_移除其他Buff_UtilityRemoveOtherBuff>())
					{
						BuffLogicPassingComponents.Add(
							new Buff_移除其他Buff_UtilityRemoveOtherBuff.BLP_移除其他Buff_UtilityRemoveOtherBuff());
					}
					break;
				case RolePlay_BuffTypeEnum.CommonEnemyWeakness_通用敌人弱点:
					if (!Check<BLP_弱点规则修饰_ModifyWeaknessRule>())
					{
						  var newRule = new BLP_弱点规则修饰_ModifyWeaknessRule();
						  newRule.RelatedRule = new WAR_初始化规则条目_InitRuleEntry
						  {
							  RelatedWeaknessUID = "弱点规则名",
							  InitActive = false,
							  CountAmount = 100,
						  };
						  BuffLogicPassingComponents.Add(newRule);
					}
					break;
				case RolePlay_BuffTypeEnum.ProcessDamageOffsetViaFrontOrBack_背后或正面伤害修正:
					if (!Check<Buff_处理正背面伤害修正_ProcessDamageOffsetViaFrontOrBack.BLP_正背面伤害修正覆写_OverrideOffsetOnBackOrFront>())
					{
						BuffLogicPassingComponents.Add(new Buff_处理正背面伤害修正_ProcessDamageOffsetViaFrontOrBack.BLP_正背面伤害修正覆写_OverrideOffsetOnBackOrFront());
					}
					break;
				
					
				
			}
		}

		private bool Check<T>() where T : BaseBuffLogicPassingComponent
		{
			if (BuffLogicPassingComponents != null && BuffLogicPassingComponents.Count > 0)
			{
				if (BuffLogicPassingComponents.Exists((component => component is T)))
				{
					return true;
				}
			}
			if(BuffEffectArray_File!= null && BuffEffectArray_File.Count > 0)
			{
				foreach (SOConfig_BLPConfig perConfig in BuffEffectArray_File)
				{
					if (perConfig.BLPList_Serialize != null && perConfig.BLPList_Serialize.Length > 0)
					{
						foreach (BaseBuffLogicPassingComponent perBLP in perConfig.BLPList_Serialize)
						{
							if (perBLP is T)
							{
								return true;
							}
						}
					}
				}
			}
			BuffLogicPassingComponents = new List<BaseBuffLogicPassingComponent>();
			return false;
		}

#endif

	}


	public class BuffApplyInfo_Runtime
	{
		[LabelText("Buff类型标签")]
		public RolePlay_BuffTypeEnum BuffType;
		[ShowInInspector, SerializeReference, LabelText("Buff传递参数组-直属")]
		public List<BaseBuffLogicPassingComponent> BuffLogicPassingComponents;

		public static BuffApplyInfo_Runtime GetFromPool()
		{
			var newApplyInfo = GenericPool<BuffApplyInfo_Runtime>.Get();
			newApplyInfo.BuffType = RolePlay_BuffTypeEnum.None;
			newApplyInfo.BuffLogicPassingComponents =
				CollectionPool<List<BaseBuffLogicPassingComponent>, BaseBuffLogicPassingComponent>.Get();
			newApplyInfo.BuffLogicPassingComponents.Clear();
			return newApplyInfo;
		}


		/// <summary>
		/// <para>在退回池前将内容释放回池。</para>
		/// <para>只有运行时生成的非配置数据才会这样做</para>
		/// </summary>
		public void ReleaseBeforeReturnToPool()
		{
			if (BuffLogicPassingComponents != null && BuffLogicPassingComponents.Count > 0)
			{
				foreach (var perI in BuffLogicPassingComponents)
				{
					perI.ReleaseOnReturnToPool();
				}
			}
			BuffLogicPassingComponents.Clear();

			BuffLogicPassingComponents = null;
			CollectionPool<List<BaseBuffLogicPassingComponent>, BaseBuffLogicPassingComponent>.Release(
				BuffLogicPassingComponents);
			GenericPool<BuffApplyInfo_Runtime>.Release(this);
		}

		
	}


	/// <summary>
	/// <para>BLP仅作为参数传递的时候的一种载体。【禁止】修改里面的数据。具体的Buff在使用里面的数据的时候，应当自行拷贝出去</para>
	/// </summary>
	[Serializable]
	public abstract class BaseBuffLogicPassingComponent
	{


		/// <summary>
		/// <para>释放只是用于运行时构建的BLP，而且是懒释放（只在下一次从池中拿出的时候释放，而不是退回池中的时候）</para>
		/// <para>那种直接序列化的，不需要释放它。</para>
		/// </summary>
		public abstract void ReleaseOnReturnToPool();

	}

	public static class BuffLogicPassingUtility
	{
		public static T GetTargetComponent<T>(this BaseBuffLogicPassingComponent[] array)
			where T : BaseBuffLogicPassingComponent
		{
			foreach (var perI in array)
			{
				if (perI is T perT)
				{
					return perT;
				}
			}

			return default;
		}
		
	}



}