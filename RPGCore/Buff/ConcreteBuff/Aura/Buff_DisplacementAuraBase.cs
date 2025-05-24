using System;
using System.Collections.Generic;
using ARPG.Character.Base;
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
namespace RPGCore.Buff.ConcreteBuff.Aura
{
	[Serializable]
	public class Buff_DisplacementAuraBase : BaseRPBuff
	{
		[SerializeField, LabelText("光环特效"), FoldoutGroup("配置", true), GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_Aura;

		protected PerVFXInfo _vfxInfo_Aura;

		[SerializeField, LabelText("推拉等效每秒距离-对重量1，正数推，负数拉"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		public float PullDistancePerSecond = 1.8f;

		[SerializeField, LabelText("每重量降低推拉速度百分比-累乘"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		public float PullDistancePerWeight = 0.20f;


		[SerializeField, LabelText("推拉最大力度"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		public float MaxPullForce = 5f;


		[SerializeField, LabelText("作用范围"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		public float CurrentAffectRange = 5f;

		[SerializeField, LabelText("作用周期 - 间隔"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		public float CurrentAffectInterval = 5f;

		[SerializeField, LabelText("作用生效时长"), FoldoutGroup("配置", true),
		 TitleGroup("配置/数值", Alignment = TitleAlignments.Centered)]
		public float CurrentAffectDuration = 0.5f;


		[ShowInInspector, LabelText("当前作用的阵营"), FoldoutGroup("运行时", true)]
		public FactionTypeEnumFlag CurrentAffectFaction;



		/// <summary>
		/// true为正在作用，false为正在间隔周期
		/// </summary>
		public bool BuffAffecting { get; private set; }

		/// <summary>
		/// 下次更换状态的时间点
		/// </summary>
		public float NextChangeTypeTime { get; private set; }


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
		}


		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			NextChangeTypeTime = BaseGameReferenceService.CurrentFixedTime + CurrentAffectInterval;
			_vfxInfo_Aura = _VFX_GetAndSetBeforePlay(PerVFXInfo._VFX_GetByUID(AllVFXInfoList, _vfx_Aura, false))
				?.VFX_4_SetLocalScale(CurrentAffectRange)?._VFX__10_PlayThis();
			return ds;
		}




		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);


			if (currentTime > NextChangeTypeTime)
			{
				if (BuffAffecting)
				{
					BuffAffecting = false;
					NextChangeTypeTime = currentTime + CurrentAffectInterval;
				}
				else
				{
					BuffAffecting = true;
					NextChangeTypeTime = currentTime + CurrentAffectDuration;
				}
			}


			if (BuffAffecting)
			{
				var hitList = _characterOnMapManagerRef.GetBehaviourListInRange(
					Parent_SelfBelongToObject.GetBuffReceiverPosition(),
					CurrentAffectRange,
					CurrentAffectFaction);
				foreach (BaseARPGCharacterBehaviour perB in hitList)
				{
					float behaviourWeight = perB.GetFloatDataEntryByType(RP_DataEntry_EnumType.MovementMass_重量)
						.CurrentValue;
					float abs = Mathf.Abs(PullDistancePerSecond);
					if (behaviourWeight > MaxPullForce)
					{
						continue;
					}
					var direction = perB.transform.position - Parent_SelfBelongToObject.GetBuffReceiverPosition();
					direction.Normalize();

					float pow = behaviourWeight - 1f;
					float t = Mathf.Pow((1f - PullDistancePerWeight), pow);
					float distance = PullDistancePerSecond * t * delta;

					// if (PullDistancePerSecond < 0f)
					// {
					// 	direction = -direction;
					// }

					perB.TryMovePosition_OnlyXZ(distance * direction);
				}
			}
		}



		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_位移光环信息_DisplacementAura blp_displacementAura:

					PullDistancePerSecond = blp_displacementAura.PullDistancePerSecond;
					PullDistancePerWeight = blp_displacementAura.PullDistancePerWeight;
					MaxPullForce = blp_displacementAura.MaxPullForce;
					CurrentAffectInterval = blp_displacementAura.CurrentAffectInterval;
					CurrentAffectDuration = blp_displacementAura.CurrentAffectDuration;
					CurrentAffectRange = blp_displacementAura.CurrentAffectRange;
					CurrentAffectFaction = blp_displacementAura.CurrentAffectFaction;

					break;
			}
		}










		[Serializable]
		public class BLP_位移光环信息_DisplacementAura : BaseBuffLogicPassingComponent
		{
			[SerializeField, LabelText("推拉等效每秒距离-对重量1，正数推，负数拉")]
			public float PullDistancePerSecond;
			[SerializeField, LabelText("每重量降低推拉速度百分比-累乘")]
			public float PullDistancePerWeight;
			[SerializeField, LabelText("推拉最大力度")]
			public float MaxPullForce;
			[SerializeField, LabelText("作用周期 - 间隔")]
			public float CurrentAffectInterval;
			[SerializeField, LabelText("作用生效时长")]
			public float CurrentAffectDuration;
			[SerializeField, LabelText("作用范围")]
			public float CurrentAffectRange;
			[ShowInInspector, LabelText("当前作用的阵营")]
			public FactionTypeEnumFlag CurrentAffectFaction;


			public static BLP_位移光环信息_DisplacementAura GetNew(BLP_位移光环信息_DisplacementAura copyFrom = null)
			{
				var tmpNew = new BLP_位移光环信息_DisplacementAura();
				if (copyFrom != null)
				{
					tmpNew.PullDistancePerSecond = copyFrom.PullDistancePerSecond;
					tmpNew.PullDistancePerWeight = copyFrom.PullDistancePerWeight;
					tmpNew.MaxPullForce = copyFrom.MaxPullForce;
					tmpNew.CurrentAffectInterval = copyFrom.CurrentAffectInterval;
					tmpNew.CurrentAffectDuration = copyFrom.CurrentAffectDuration;
					tmpNew.CurrentAffectRange = copyFrom.CurrentAffectRange;
					tmpNew.CurrentAffectFaction = copyFrom.CurrentAffectFaction;
				}
				return tmpNew;
			}

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_位移光环信息_DisplacementAura>.Release(this);
				return;
			}
		}

	}
}