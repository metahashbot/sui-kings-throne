using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Manager;
using GameplayEvent;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Aura
{
	[Serializable]
	[TypeInfoBox("通用光环")]
	public class Buff_CommonAuraBase : BaseRPBuff
	{
		
		[SerializeField, LabelText("光环特效"), FoldoutGroup("配置", true), GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_Aura;

		protected PerVFXInfo _vfxInfo_Aura;


		[SerializeField, LabelText("伤害信息"), FoldoutGroup("配置", true)]
		public ConSer_DamageApplyInfo DamageApplyInfo;

		[SerializeField, LabelText("当前范围"), FoldoutGroup("运行时", true)]
		public float CurrentAffectRange = 5f;

		[SerializeField, LabelText("当前作用间隔"), FoldoutGroup("运行时", true)]
		public float CurrentAffectInterval = 1f;

		[ShowInInspector, LabelText("当前作用的阵营"), FoldoutGroup("运行时", true)]
		public FactionTypeEnumFlag CurrentAffctFaction;


		protected float _nextAffectTime;


		[SerializeField, LabelText("光环生效时需要触发的事件们"), FoldoutGroup("配置", true)]
		public List<SOConfig_PrefabEventConfig> OnAuraAffectEventList = new List<SOConfig_PrefabEventConfig>();
		
		
		


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
		}
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps )
		{
			var ds = base.OnBuffInitialized(blps);
			
			_nextAffectTime = BaseGameReferenceService.CurrentFixedTime;
			_vfxInfo_Aura = _VFX_GetAndSetBeforePlay(PerVFXInfo._VFX_GetByUID(AllVFXInfoList, _vfx_Aura, false))
				?.VFX_4_SetLocalScale(CurrentAffectRange)?._VFX__10_PlayThis();
			return ds;
		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);


			if (currentTime > _nextAffectTime)
			{
				if(OnAuraAffectEventList != null && OnAuraAffectEventList.Count > 0)
				{
					foreach (var perEvent in OnAuraAffectEventList)
					{
						GameplayEventManager.Instance.StartGameplayEvent(perEvent);
					}
				}
				
				
				var hitList = _characterOnMapManagerRef.GetBehaviourListInRange(
					Parent_SelfBelongToObject.GetBuffReceiverPosition(),
					CurrentAffectRange,
					CurrentAffctFaction);
				foreach (BaseARPGCharacterBehaviour perTarget in hitList)
				{
					RP_DS_DamageApplyInfo dai_heal = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromFromConSer(
						DamageApplyInfo,
						perTarget,
						Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
						null);

					dai_heal.DamageWorldPosition = perTarget.transform.position;
					
					
					SubGameplayLogicManager_ARPG.Instance.DamageAssistServiceInstance.ApplyDamage(dai_heal, 0);
				}
				_nextAffectTime += CurrentAffectInterval;
			}
		}



		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_基本光环构建信息_BaseAuraBuild auraBuild:

					DamageApplyInfo = auraBuild.DamageApplyInfo;
					CurrentAffectRange = auraBuild.CurrentAffectRange;
					CurrentAffectInterval = auraBuild.CurrentAffectInterval;
					CurrentAffctFaction = auraBuild.CurrentAffctFaction;
					 
					break;
			}
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			_vfxInfo_Aura?.VFX_StopThis();
			return base.OnBuffPreRemove();
		}


		[Serializable]
		public class BLP_基本光环构建信息_BaseAuraBuild : BaseBuffLogicPassingComponent
		{
			[SerializeField,LabelText("使用的伤害信息")]
			public ConSer_DamageApplyInfo DamageApplyInfo;

			public float CurrentAffectRange;
			public float CurrentAffectInterval;
			
			public FactionTypeEnumFlag CurrentAffctFaction;

			public static BLP_基本光环构建信息_BaseAuraBuild GetFromCopy(BLP_基本光环构建信息_BaseAuraBuild copyFrom = null)
			{
				var tmpNew = new BLP_基本光环构建信息_BaseAuraBuild();
				if (copyFrom != null)
				{
					tmpNew.DamageApplyInfo = copyFrom.DamageApplyInfo;
					tmpNew.CurrentAffectRange = copyFrom.CurrentAffectRange;
					tmpNew.CurrentAffectInterval = copyFrom.CurrentAffectInterval;
					tmpNew.CurrentAffctFaction = copyFrom.CurrentAffctFaction;
				}
				return tmpNew;
			}


			public override void ReleaseOnReturnToPool()
			{
				DamageApplyInfo = null;
				GenericPool<BLP_基本光环构建信息_BaseAuraBuild>.Release(this);

			}
		}
	}
}