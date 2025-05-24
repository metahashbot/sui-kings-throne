using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using DG.Tweening;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Utility
{

	[Serializable]
	public class Buff_强霸体_StrongStoic : BaseRPBuff , I_RPLogicCanApplyStoic
	{
		/// <summary>
		/// <para>记录了霸体的施加来源信息。</para>
		/// </summary>
		[ShowInInspector]
		protected List<RPDS_StoicApplyInfo> _stoicApplyInfoList = new List<RPDS_StoicApplyInfo>();

		protected PerVFXInfo _vfxInfo_Stoic;

		[SerializeField,LabelText("使用的描边预设名字")]
		private string _outlinePresetName_Stoic ="强霸体描边";
		

		// private Tweener _tween_OutlineBlink;

		private BaseARPGCharacterBehaviour _behaviourRef;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_ProcessWeakStoicEffect_OnOtherBuffTryAdd);
			_buffCollection = GlobalConfigurationAssetHolderHelper.GetGCAHH().Collection_RPBuff;
			_behaviourRef = parent as BaseARPGCharacterBehaviour;
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_霸体施加信息_StoicApplyInfoBLP stoicApplyInfoBlp:
					//先检查重复
					//如果新的时间更短
					var fi = _stoicApplyInfoList.FindIndex((info =>
						ReferenceEquals(info.Applier, stoicApplyInfoBlp.Applier)));
					if (fi == -1)
					{
						RPDS_StoicApplyInfo stoicApplyInfo = GenericPool<RPDS_StoicApplyInfo>.Get();
						stoicApplyInfo.Applier = stoicApplyInfoBlp.Applier;
						stoicApplyInfo.RemainingDuration = stoicApplyInfoBlp.RemainingDuration;
						_stoicApplyInfoList.Add(stoicApplyInfo);
					}
					else
					{
						var oldInfo = _stoicApplyInfoList[fi];
						oldInfo.RemainingDuration = stoicApplyInfoBlp.RemainingDuration;
					}
					// _vfxInfo_Stoic = _VFX_GetAndSetBeforePlay(_VFX_Stoic, false);
					ProcessStoicBlinkOutline();
					UpdateAvailableTimeByInfoList();
					break;
				case BLP_霸体移除信息_StoicRemoveInfoBLP stoicRemoveInfoBlp:
					var fi2 = _stoicApplyInfoList.FindIndex((info =>
						ReferenceEquals(info.Applier, stoicRemoveInfoBlp.Applier)));
					if (fi2 != -1)
					{
						GenericPool<RPDS_StoicApplyInfo>.Release(_stoicApplyInfoList[fi2]);
						_stoicApplyInfoList.RemoveAt(fi2);
					}
					else
					{
						// DBug.LogWarning($"在移除霸体时，找不到对应的施加信息，这是不应该的");
					}
					UpdateAvailableTimeByInfoList();

					break;
				case BLP_ApplyFromConfig_由表中直接施加 fromConfig:
					RPDS_StoicApplyInfo stoicApplyInfo_2 = GenericPool<RPDS_StoicApplyInfo>.Get();
					stoicApplyInfo_2.Applier = this;
					stoicApplyInfo_2.RemainingDuration = -1f;
					_stoicApplyInfoList.Add(stoicApplyInfo_2);
					break;
			}
		}



		private void ProcessStoicBlinkOutline()
		{
			var ds_outline =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AnimationHelper_OnOutlineTaskRequired_当要求新的描边任务);
			ds_outline.ObjectArgu1 = _characterOnMapManagerRef.GetOutlinePresetTaskByPresetID(_outlinePresetName_Stoic);
			ds_outline.ObjectArgu2 = Parent_SelfBelongToObject;
			ds_outline.FloatArgu1 = 99999f;
			ds_outline.ObjectArguStr = _outlinePresetName_Stoic;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ds_outline);

		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			for (int i = _stoicApplyInfoList.Count - 1; i >= 0; i--)
			{
				if (Mathf.Approximately(_stoicApplyInfoList[i].RemainingDuration, -1f))
				{
					continue;
				}
				_stoicApplyInfoList[i].RemainingDuration -= delta;
				if (_stoicApplyInfoList[i].RemainingDuration <= 0f)
				{
					GenericPool<RPDS_StoicApplyInfo>.Release(_stoicApplyInfoList[i]);
					_stoicApplyInfoList.RemoveAt(i);
					if (_stoicApplyInfoList.Count == 0)
					{
						_vfxInfo_Stoic?.VFX_StopThis();
					}
					continue;
				}
			}
			UpdateAvailableTimeByInfoList();
		}

		private void UpdateAvailableTimeByInfoList()
		{
			float maxDuration = -0.01f;

			for (int i = _stoicApplyInfoList.Count - 1; i >= 0; i--)
			{
				if (Mathf.Approximately(_stoicApplyInfoList[i].RemainingDuration, -1f))
				{
					maxDuration = -1f;
					ResetDurationAndAvailableTimeAs(-1f, -1f, false);
					return;
				}
				else
				{
					if (_stoicApplyInfoList[i].RemainingDuration > maxDuration)
					{
						maxDuration = _stoicApplyInfoList[i].RemainingDuration;
					}
				}
			}

			if (maxDuration < 0f)
			{
				Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.StrongStoic_强霸体);
				return;
			}
			else
			{
				ResetDurationAndAvailableTimeAs(maxDuration, maxDuration, false);

			}
		}

		
		

		protected override void _Internal_OnAvailableTimeUseUp()
		{
			
			base._Internal_OnAvailableTimeUseUp();

		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var _ds_removeOutline =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AnimationHelper_OnRemoveOutlineTask_当要求移除描边任务);
			_ds_removeOutline.ObjectArgu1 = _outlinePresetName_Stoic;
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(_ds_removeOutline);
			
			for (int i = _stoicApplyInfoList.Count - 1; i >= 0; i--)
			{
				GenericPool<RPDS_StoicApplyInfo>.Release(_stoicApplyInfoList[i]);
				_stoicApplyInfoList.RemoveAt(i);
			}
			// _tween_OutlineBlink?.Kill();
			_stoicApplyInfoList.Clear();
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffPreAdd_一个Buff将要被添加,
				_ABC_ProcessWeakStoicEffect_OnOtherBuffTryAdd);
			
			return base.OnBuffPreRemove();
		}

		private SOCollection_RPBuff _buffCollection;
		private void _ABC_ProcessWeakStoicEffect_OnOtherBuffTryAdd(DS_ActionBusArguGroup ds)
		{
			RolePlay_BuffTypeEnum newBuffType = (RolePlay_BuffTypeEnum)ds.IntArgu1.Value;
			if (GetBuffCurrentAvailableType() != BuffAvailableType.Available_TimeInAndMeetRequirement)
			{
				return;
			}
			var config = _buffCollection.GetRPBuffByTypeAndLevel(newBuffType);
			if (config.ConcreteBuffFunction.InternalFunctionFlagType.HasFlag(RP_BuffInternalFunctionFlagTypeEnum
				.BlockByStrongStoic_被强霸体屏蔽))
			{
				var result = ds.ObjectArgu2 as RP_DS_BuffApplyResult;
				result.BlockByOtherBuff = true;
			}
		}





	}
}