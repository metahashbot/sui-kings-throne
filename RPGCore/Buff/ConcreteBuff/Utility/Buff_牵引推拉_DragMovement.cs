using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using Global;
using Global.ActionBus;
using Global.Setting;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Utility
{
	[Serializable]
	public class Buff_牵引推拉_DragMovement : BaseRPBuff
	{

		public class PerDragInfo
		{
			public string UID;
			/// <summary>
			/// 剩余牵引时长
			/// </summary>
			public float RemainingDuration;
			/// <summary>
			/// <para>牵引方向</para>
			/// </summary>
			public Vector3 DragDirection;
			/// <summary>
			/// <para>牵引力度</para>
			/// </summary>
			public float DragVelocity;
		}


		[NonSerialized, ShowInInspector, LabelText("当前还活跃的牵引信息列表")]
		public List<PerDragInfo> CurrentActiveDragInfoList = new List<PerDragInfo>();


		[SerializeField]
		public float SpeedMultiplierToCalculation = 1f;

		private static ConSer_MiscSettingInSO _miscSettingRef;

		private BaseARPGCharacterBehaviour _behaviourRef;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_miscSettingRef = GlobalConfigurationAssetHolderHelper.GetGCAHH().MiscSetting_Runtime.SettingContent;
			_behaviourRef = Parent_SelfBelongToObject as BaseARPGCharacterBehaviour;
		}

		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);

			//从所有的List中得出最终的方向
			Vector3 finalDir = Vector3.zero;
			for (int i = CurrentActiveDragInfoList.Count - 1; i >= 0; i--)
			{
				CurrentActiveDragInfoList[i].RemainingDuration -= delta;
				if (CurrentActiveDragInfoList[i].RemainingDuration <= 0f)
				{
					GenericPool<PerDragInfo>.Release(CurrentActiveDragInfoList[i]);
					CurrentActiveDragInfoList.RemoveAt(i);
					continue;
				}
			}

			foreach (PerDragInfo perInfo in CurrentActiveDragInfoList)
			{
				finalDir += (perInfo.DragDirection * perInfo.DragVelocity);
			}

			//最终方向乘以速度倍率
			var movement = finalDir * (delta * SpeedMultiplierToCalculation);
			_behaviourRef.TryMovePosition_XYZ(movement, true);
		}



		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP :
					if (!Parent_SelfBelongToObject.CurrentDataValid())
					{
						return;
					}
					var blp_drag = blp as BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP;
					var selfMass = Parent_SelfBelongToObject
						.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MovementMass_重量).CurrentValue;
					if (Mathf.Abs(blp_drag.DragPower) < selfMass)
					{
						return;
					}
					PerDragInfo processDragInfo = null;
					if (!string.IsNullOrEmpty(blp_drag.UID))
					{
						var findI = CurrentActiveDragInfoList.FindIndex((info => info.UID.Equals(blp_drag.UID)));
						if (findI != -1)
						{
							CurrentActiveDragInfoList[findI].RemainingDuration = blp_drag.RemainingDuration;
							processDragInfo = CurrentActiveDragInfoList[findI];
						}
						else
						{
							processDragInfo = GenericPool<PerDragInfo>.Get();
							CurrentActiveDragInfoList.Add(processDragInfo);
						}
					}
					else
					{
						processDragInfo = GenericPool<PerDragInfo>.Get();
						CurrentActiveDragInfoList.Add(processDragInfo);
					}



					processDragInfo.UID = blp_drag.UID;
					processDragInfo.RemainingDuration = blp_drag.RemainingDuration;

					processDragInfo.DragDirection = Vector3.zero;
					if (blp_drag.UseXAixs)
					{
						processDragInfo.DragDirection.x = blp_drag.DragDirection.x;
					}
					if (blp_drag.UseZAixs)
					{
						processDragInfo.DragDirection.z = blp_drag.DragDirection.z;
					}
					
					float purePower = Mathf.Abs(blp_drag.DragPower);

					float dis_cal = purePower /
					                (selfMass * _miscSettingRef.ForcedDisplacement_MassMultiplier) *
					                _miscSettingRef.ForcedDisplacement_DistanceMultiplier;
					float duration_cal =
						Mathf.Sqrt(
							_miscSettingRef.ForcedDisplacement_DurationOperator * (purePower / selfMass)) /
						10f * _miscSettingRef.ForcedDisplacement_DurationMultiplier;
					float velocity = dis_cal / duration_cal;

					processDragInfo.DragVelocity = velocity * (blp_drag.DragPower > 0f ? 1f : -1f);

					ResetTimeToMax();
					break;
				case BLP_停止牵引推拉_StopDragMovement blp_stop:
					for (int i = CurrentActiveDragInfoList.Count - 1; i >= 0; i--)
					{
						if (!string.IsNullOrEmpty(CurrentActiveDragInfoList[i].UID) &&
						    CurrentActiveDragInfoList[i].UID.Equals(blp_stop.StopUID))
						{
							GenericPool<PerDragInfo>.Release(CurrentActiveDragInfoList[i]);
							CurrentActiveDragInfoList.RemoveAt(i);
							break;
						}
					}
					ResetTimeToMax();
					break;
			}
		}



		private void ResetTimeToMax()
		{
			float max = 0f;
			foreach (PerDragInfo perInfo in CurrentActiveDragInfoList)
			{
				if (perInfo.RemainingDuration > max)
				{
					max = perInfo.RemainingDuration;
				}
			}
			ResetExistDurationAs(max);
			ResetAvailableTimeAs(max);
		}


		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds = base.OnBuffPreRemove();

			if (CurrentActiveDragInfoList != null && CurrentActiveDragInfoList.Count > 0)
			{
				foreach (PerDragInfo perDragInfo in CurrentActiveDragInfoList)
				{
					GenericPool<PerDragInfo>.Release(perDragInfo);
				}
				CurrentActiveDragInfoList.Clear();
			}


			return ds;
		}



		[Serializable]
		public class BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP : BaseBuffLogicPassingComponent
		{
			[SerializeField, LabelText("牵引效果UID")]
			public string UID = string.Empty;
			[SerializeField, LabelText("牵引时长")]
			public float RemainingDuration = 1;
			[SerializeField, LabelText("牵引方向")]
			public Vector3 DragDirection = Vector3.zero;
			[SerializeField, LabelText("牵引力度")]
			public float DragPower = 1;
			[SerializeField, LabelText("使用X轴")]
			public bool UseXAixs = true;
			[SerializeField, LabelText("使用Z轴")]
			public bool UseZAixs = true;


			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_开始牵引推拉等效失衡参数_StartDragMovementAsUnbalanceBLP>.Release(this);
			}
		}
		[Serializable]
		public class BLP_停止牵引推拉_StopDragMovement : BaseBuffLogicPassingComponent
		{
			[SerializeField, LabelText("停止牵引的UID")]
			public string StopUID = String.Empty;

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_停止牵引推拉_StopDragMovement>.Release(this);
			}
		}
	}
}