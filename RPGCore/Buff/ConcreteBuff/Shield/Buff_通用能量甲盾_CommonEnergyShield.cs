using System;
using System.Collections.Generic;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Shield
{
	[Serializable]
	public class Buff_通用能量甲盾_CommonEnergyShield : BaseRPBuff
	{
		[SerializeField,LabelText("甲盾UID")]
		[TitleGroup("===基本配置===")]
		public string ShieldUid;
		
		[SerializeField,LabelText("破裂后刷新需要的时间")]
		[TitleGroup("===基本配置===")]
		public float RefreshTime = 10f;
		
		
		[SerializeField,LabelText("护盾HP")]
		[TitleGroup("===基本配置===")]
		public float InitShieldHP = 1000f;


		[ShowInInspector,LabelText("当前护盾HP")]
		[NonSerialized]
		[FoldoutGroup("运行时")]
		public float CurrentShieldHP;


		[SerializeField, LabelText("关联属性")]
		[TitleGroup("===基本配置===")]
		public DamageTypeEnum RelatedDamageType;

		[SerializeField,LabelText("同属性伤害百分比")] 
		[TitleGroup("===基本配置===")]
		public float DamageBonusRatio = 400f;
		
		[SerializeField,LabelText("异属性伤害百分比")]
		[TitleGroup("===基本配置===")]
		public float DamageRatio_Other = 100f;
		 



		private PerVFXInfo _vfxInfo_LoopShield;
		




		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_BeforeTakeToShield_将要对甲盾进行伤害计算,
				_ABC_ProcessDamageTakeOnShield);
		}
		
		
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			return base.OnBuffInitialized(blps);
		}


		private void _ABC_ProcessDamageTakeOnShield(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			
			//看一看伤害类型，如果匹配则计算时会更多一些
			float damageRaw = dar.DamageAmount_FinalBonusPart;
			float damageRemaining = dar.DamageAmount_FinalBonusPart;
			bool match = false;
			if (dar.DamageType == RelatedDamageType)
			{
				damageRemaining *= (DamageBonusRatio / 100f);
				match = true;
			}
			else
			{
				damageRemaining *= (DamageRatio_Other / 100f);
			}
			if (damageRemaining > CurrentShieldHP)
			{
				float OnHP = 0f;
				//如果匹配伤害，则剩余的将要到HP上的量为 护盾量 / 匹配伤害百分比  
				if (match)
				{
					OnHP = damageRaw - (CurrentShieldHP / (DamageBonusRatio / 100f));
				}
				else
				{
					OnHP = damageRaw - (CurrentShieldHP / (DamageRatio_Other / 100f));
				}
				OnHP = Mathf.Max(0f, OnHP);
				dar.DamageAmount_AfterShield = OnHP;
				dar.DamageResult_TakenOnShield = CurrentShieldHP;


				CurrentShieldHP = 0;
				MarkAsRemoved = true;
				return;
			}
			else
			{
				CurrentShieldHP -= damageRemaining;
				dar.DamageAmount_AfterShield = 0f;
				dar.DamageResult_TakenOnShield = damageRemaining;
				
			}
			
			
		}


		private void _VFX_PlayBreakVFX()
		{
			
		}


		private void _VFX_PlayAbsorbVFX()
		{
			switch (RelatedDamageType)
			{
				case DamageTypeEnum.YuanNengGuang_源能光:
					_VFX_GetAndSetBeforePlay(_vfxUid_Light_Absorb)._VFX__10_PlayThis();
					break;
				case DamageTypeEnum.YuanNengDian_源能电:
					 
					break;
				case DamageTypeEnum.AoNengTu_奥能土:
					break;
				case DamageTypeEnum.AoNengShui_奥能水:
					break;
				case DamageTypeEnum.AoNengHuo_奥能火:
					break;
				case DamageTypeEnum.AoNengFeng_奥能风:
					break;
				case DamageTypeEnum.LingNengLing_灵能灵:
					break;
			}
		}



		public class BLP_能量甲盾初始化参数_CommonEnergyShieldBLP : BaseBuffLogicPassingComponent
		{

			[SerializeField, LabelText("甲盾UID")]
			public string ShieldUid;
			[SerializeField, LabelText("破裂后刷新需要的时间")]
			public float RefreshTime = 10f;
			[SerializeField, LabelText("护盾HP")]
			public float InitShieldHP = 1000;
			[SerializeField, LabelText("关联属性")]
			public DamageTypeEnum RelatedDamageType = DamageTypeEnum.AoNengHuo_奥能火;
			[SerializeField, LabelText("同属性伤害百分比")]
			public float DamageBonusRatio = 400f;
			[SerializeField, LabelText("异属性伤害百分比")]
			public float DamageRatio_Other = 100f;
			 
			
			 

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_能量甲盾初始化参数_CommonEnergyShieldBLP>.Release(this);
			}
		}
		
		
		
		[SerializeField, LabelText("护盾常驻特效-电"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Electric;

		[SerializeField, LabelText("护盾击破特效-电"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Electric_Break;
		
		[SerializeField,LabelText("护盾吸收特效-电"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Electric_Absorb;

		[SerializeField, LabelText("护盾常驻特效-光"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Light;

		[SerializeField, LabelText("护盾击破特效-光"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Light_Break;
		
		[SerializeField,LabelText("护盾吸收特效-光"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Light_Absorb;

		[SerializeField, LabelText("护盾常驻特效-火"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Fire;

		[SerializeField, LabelText("护盾击破特效-火"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Fire_Break;
		 
		[SerializeField,LabelText("护盾吸收特效-火"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		 protected string _vfxUid_Fire_Absorb;

		[SerializeField, LabelText("护盾常驻特效-水"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Water;

		[SerializeField, LabelText("护盾击破特效-水"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Water_Break;
		
		[SerializeField,LabelText("护盾吸收特效-水"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		 protected string _vfxUid_Water_Absorb;

		[SerializeField, LabelText("护盾常驻特效-风"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Wind;

		[SerializeField, LabelText("护盾击破特效-风"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Wind_Break;
		
		[SerializeField,LabelText("护盾吸收特效-风"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		 protected string _vfxUid_Wind_Absorb;

		[SerializeField, LabelText("护盾常驻特效-土"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Earth;

		[SerializeField, LabelText("护盾击破特效-土"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Earth_Break;
		
		[SerializeField,LabelText("护盾吸收特效-土"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		 protected string _vfxUid_Earth_Absorb;

		[SerializeField, LabelText("护盾常驻特效-灵"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Soul;

		[SerializeField, LabelText("护盾击破特效-灵"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		protected string _vfxUid_Soul_Break;
		
		[SerializeField,LabelText("护盾吸收特效-灵"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===特效配置===")]
		 protected string _vfxUid_Soul_Absorb;


	}
}