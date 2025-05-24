using System;
using System.Collections;
using System.Collections.Generic;
using RPGCore.Buff.ConcreteBuff.Aura;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Level;
using RPGCore.Buff.ConcreteBuff.Shield;
using RPGCore.Buff.ConcreteBuff.Utility;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class ESA_附加Buff_AddBuff : BaseEnemySpawnAddon
	{
		/// <summary>
		/// 这是序列化内容。不能在运行时动它。运行时想添加新的话，须要使用专门的方法
		/// </summary>
		[Serializable]
		public class PerAddonInfo
		{
#if UNITY_EDITOR
			[OnValueChanged(nameof(_OVC_CheckIfAddRelatedBLP))]
#endif
			public RolePlay_BuffTypeEnum BuffID;

			[LabelText("√：直接添加  || 口：正常流程添加")]
			public bool ApplyAsDirectMode;

			[LabelText("包含对存在时长的覆写？")]
			public bool ContainDurationOverride;

			[LabelText("包含对有效时长的覆写？")]
			public bool ContainAvailableTimeOverride;

			[LabelText("覆写存在时长"), ShowIf(nameof(ContainDurationOverride))]
			public float OverrideExistDuration;

			[LabelText("覆写有效时长"), ShowIf(nameof(ContainAvailableTimeOverride))]
			public float OverrideAvailableTime;

			[LabelText("包含其他逻辑传递组件(BLP-BuffLogicPassingComponent)")]
			public bool ContainBLP = false;

			[LabelText("直属==逻辑传递组件(BLP-BuffLogicPassingComponent)"), ShowIf(nameof(ContainBLP)), SerializeReference]
			[FormerlySerializedAs("BLPList")]
			private BaseBuffLogicPassingComponent[] BLPList_Serialize;

			[LabelText("文件==逻辑传递组件(BLP-BuffLogicPassingComponent)"), ShowIf(nameof(ContainBLP)), SerializeField]
			[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
			private List<SOConfig_BLPConfig> BLPList_File = new List<SOConfig_BLPConfig>();


			[NonSerialized]
			private List<BaseBuffLogicPassingComponent> BLPList_RuntimeFinal;

			public List<BaseBuffLogicPassingComponent> GetBLPList_RuntimeFinal()
			{
				BLPList_RuntimeFinal = new List<BaseBuffLogicPassingComponent>();
				for (int i = 0; i < BLPList_Serialize.Length; i++)
				{
					BLPList_RuntimeFinal.Add(BLPList_Serialize[i]);
				}
				for (int i = 0; i < BLPList_File.Count; i++)
				{
					var newConfig = UnityEngine.Object.Instantiate(BLPList_File[i]);
					for (int j = 0; j < newConfig.BLPList_Serialize.Length; j++)
					{
						BLPList_RuntimeFinal.Add(newConfig.BLPList_Serialize[j]);
					}
				}
				return BLPList_RuntimeFinal;
			}


#if UNITY_EDITOR

		/// <summary>
		/// <para>检查一下是不是需要添加关联的那个BLP</para>
		/// </summary>
		private void _OVC_CheckIfAddRelatedBLP()
		{
			if (BuffID != RolePlay_BuffTypeEnum.None)
			{
				if (BLPList_Serialize == null)
				{
					BLPList_Serialize = new BaseBuffLogicPassingComponent[] { };
				}
			}
			switch (BuffID)
			{
				case RolePlay_BuffTypeEnum.StiffnessOnHit_受击硬直:
					if (!Check<Buff_受击硬直_StiffnessOnHit.BLP_受击硬直通用_StiffnessOnHitBLP>())
					{
						((IList)BLPList_Serialize).Add(new Buff_受击硬直_StiffnessOnHit.BLP_受击硬直通用_StiffnessOnHitBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.DragMovement_牵引推拉:
					if (!Check<Buff_牵引推拉_DragMovement.BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP>())
					{
						((IList)BLPList_Serialize).Add(new Buff_牵引推拉_DragMovement.
							BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.UnbalanceMovement_失衡推拉 : 
					if (!Check<Buff_失衡推拉_UnbalanceMovement.BLP_开始失衡推拉_StartUnbalanceMovementBLP>())
					{
						((IList)BLPList_Serialize).Add(new Buff_失衡推拉_UnbalanceMovement.
							BLP_开始失衡推拉_StartUnbalanceMovementBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.Aura_CommonHealAura_通用治疗光环:
					case RolePlay_BuffTypeEnum.Aura_SimpleFireAura_简单灼烧光环:
					if (!Check<Buff_CommonAuraBase.BLP_基本光环构建信息_BaseAuraBuild>())
					{
						((IList)BLPList_Serialize).Add(new Buff_CommonAuraBase.BLP_基本光环构建信息_BaseAuraBuild());
					}
					break;
				case RolePlay_BuffTypeEnum.Shield_Energy_属性能量甲盾 :
					if (!Check<Buff_通用能量甲盾_CommonEnergyShield.BLP_能量甲盾初始化参数_CommonEnergyShieldBLP>())
					{
						((IList)BLPList_Serialize).Add(
							new Buff_通用能量甲盾_CommonEnergyShield.BLP_能量甲盾初始化参数_CommonEnergyShieldBLP());
					}
					break;
				case RolePlay_BuffTypeEnum.Level_图腾免伤_ResistTotem :
					if (!Check<Buff_Totem_图腾免伤_ResistTotem.BLP_免伤图腾的配置信息_ResistTotemConfig>())
					{
						((IList)BLPList_Serialize).Add(
							new Buff_Totem_图腾免伤_ResistTotem.BLP_免伤图腾的配置信息_ResistTotemConfig());
					}
					break;
				case RolePlay_BuffTypeEnum.Level_图腾召唤_SummonTotem:
					if (!Check<Buff_Totem_图腾召唤_SummonTotem.BLP_召唤图腾的配置信息_SummonTotemConfig>())
					{
						((IList)BLPList_Serialize).Add(
							new Buff_Totem_图腾召唤_SummonTotem.BLP_召唤图腾的配置信息_SummonTotemConfig());
					}
					break;
				case RolePlay_BuffTypeEnum.Level_图腾回血_HealTotem : 
					if (!Check<Buff_Totem_图腾回血_HealTotem.BLP_回血图腾配置信息_HealTotemConfig>())
					{
						((IList)BLPList_Serialize).Add(
							new Buff_Totem_图腾回血_HealTotem.BLP_回血图腾配置信息_HealTotemConfig());
					}
					break;
				case RolePlay_BuffTypeEnum.RemoveBuff_移除Buff:
					if(! Check<Buff_移除其他Buff_UtilityRemoveOtherBuff.BLP_移除其他Buff_UtilityRemoveOtherBuff>())
					{
						((IList)BLPList_Serialize).Add(
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
						  ((IList)BLPList_Serialize).Add(newRule);
					}
					break;
				
					
				
			}
		}

		private bool Check<T>() where T : BaseBuffLogicPassingComponent
		{
			if (BLPList_Serialize != null && BLPList_Serialize.Length > 0)
			{
				foreach (BaseBuffLogicPassingComponent perBLP in BLPList_Serialize)
				{
					if(perBLP is T)
					{
						return true;
					}
				}
			}
			// if(BuffEffectArray_File!= null && BuffEffectArray_File.Count > 0)
			// {
			// 	foreach (SOConfig_BLPConfig perConfig in BuffEffectArray_File)
			// 	{
			// 		if (perConfig.BLPList_Serialize != null && perConfig.BLPList_Serialize.Length > 0)
			// 		{
			// 			foreach (BaseBuffLogicPassingComponent perBLP in perConfig.BLPList_Serialize)
			// 			{
			// 				if (perBLP is T)
			// 				{
			// 					return true;
			// 				}
			// 			}
			// 		}
			// 	}
			// }
			BLPList_Serialize = new BaseBuffLogicPassingComponent[] { };
			return false;
		}

#endif

		}

		[LabelText("直属==需要附加的Buff信息们"), SerializeField]
		[FormerlySerializedAs("AddBuffInfoList")]
		private List<PerAddonInfo> AddBuffInfoList_Serialize = new List<PerAddonInfo>();

		[LabelText("文件==需要附加的Buff信息们"), SerializeField]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		private List<SOConfig_BuffAddonInfo> AddBuffInfoList_File = new List<SOConfig_BuffAddonInfo>();

		[NonSerialized, LabelText("运行时具体附加buff信息们")]
		private List<PerAddonInfo> AddBuffInfoList_RuntimeFinal = new List<PerAddonInfo>();
		public List<PerAddonInfo> GetAddBuffInfoList()
		{
			AddBuffInfoList_RuntimeFinal.Clear();
			foreach (var perInfo in AddBuffInfoList_Serialize)
			{
				AddBuffInfoList_RuntimeFinal.Add(perInfo);
			}
			foreach (var perInfo in AddBuffInfoList_File)
			{
				foreach (PerAddonInfo perAddon in perInfo.AddonInfoList)
				{
					AddBuffInfoList_RuntimeFinal.Add(perAddon);
				}
			}
			return AddBuffInfoList_RuntimeFinal;
		}


		public override void ResetOnReturn()
		{
			return;
			// foreach (var perD in AddBuffInfoList)
			// {
			// 	GenericPool<PerAddonInfo>.Release(perD);
			// }
			// CollectionPool<List<PerAddonInfo>, PerAddonInfo>.Release(AddBuffInfoList);
			// GenericPool<ESA_附加Buff_AddBuff>.Release(this);
		}


	}
}