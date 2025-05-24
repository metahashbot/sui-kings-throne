using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Global.ActionBus;
using RPGCore.Buff.Requirement;
using RPGCore.DataEntry;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	[TypeInfoBox("HP、SP、UP会自动换算为PresentValue并与其最大值比对\n")]
	public class AIListen_数据项变动监听_DataEntryChangedListen : BaseAIListenComponent
	{
		[Serializable]
		public class PerListenInfo
		{
			[NonSerialized]
			public bool Executed = false;

			[LabelText("一次性的？")]
			public bool Once = true;

			[LabelText("比对数据 —— 左侧")]
			public RP_DataEntry_EnumType DataEntryType;

			[LabelText("比对数据 —— 右侧是一个数据项吗")]
			public bool CompareToOtherDataEntry;

			[HideIf(nameof(CompareToOtherDataEntry)), LabelText("比对数据 —— 右侧")]
			public float CompareAbsoluteValue;

			[SerializeField, LabelText("相当于右侧数据项的百分之多少？"), SuffixLabel("%")]
			[ShowIf(nameof(CompareToOtherDataEntry))]
			public float ComparePercentValue = 100;

			[LabelText("比较符号")]
			public CompareMethodEnum CompareMethod;

			[ShowIf(nameof(CompareToOtherDataEntry)), LabelText("比对数据 —— 右侧")]
			public RP_DataEntry_EnumType OtherDataEntryType;

			// [SerializeReference, LabelText("直属=副作用——当上述比对成立时执行")]
			// public List<BaseDecisionCommonComponent> SideEffectOnCompareSuccess =
			// 	new List<BaseDecisionCommonComponent>();
			//
			// [SerializeField, LabelText("文件=副作用——当上述比对成立时执行"), ListDrawerSettings(ShowFoldout = true),
			// 	 			 InlineEditor(InlineEditorObjectFieldModes.Boxed)]
			// public List<SOConfig_PresetSideEffects> SideEffectOnCompareSuccess_File = new List<SOConfig_PresetSideEffects>();

			[SerializeField]
			public DCCConfigInfo SideEffectOnCompareSuccess_DCCInfo;

		}

		[SerializeField, LabelText("比对内容们")]
		public List<PerListenInfo> PerListenInfos = new List<PerListenInfo>();

		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);
			brainRef.BrainHandlerFunction.SelfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_ProcessDataValue_OnDataValueChanged);

			foreach (var perListenInfoVARIABLE in PerListenInfos)
			{
				perListenInfoVARIABLE.SideEffectOnCompareSuccess_DCCInfo.CommonComponents_RuntimeAll =
					new List<BaseDecisionCommonComponent>();
				perListenInfoVARIABLE.SideEffectOnCompareSuccess_DCCInfo.BuildRuntimeDCC(ref perListenInfoVARIABLE
					.SideEffectOnCompareSuccess_DCCInfo.CommonComponents_RuntimeAll);
			}
		}




		protected void _ABC_ProcessDataValue_OnDataValueChanged(DS_ActionBusArguGroup ds)
		{
			var type = ds.ObjectArgu1 as Float_RPDataEntry;
			if (!PerListenInfos.Exists((info => info.DataEntryType == type.RP_DataEntryType)))
			{
				return;
			}

			switch (ds.ObjectArgu1)
			{
				case FloatPresentValue_RPDataEntry floatPresentValueRPDataEntry:
					foreach (PerListenInfo perInfo in PerListenInfos)
					{
						if (perInfo.DataEntryType == floatPresentValueRPDataEntry.RP_DataEntryType)
						{
							switch (perInfo.DataEntryType)
							{
								case RP_DataEntry_EnumType.CurrentHP_当前HP:

									if (!perInfo.CompareToOtherDataEntry)
									{
										if (Check(floatPresentValueRPDataEntry.CurrentValue,
											perInfo.CompareMethod,
											perInfo.CompareAbsoluteValue))
										{
											Process(perInfo);
										}
									}
									else
									{
										var maxHP = (ds.ObjectArgu2 as I_RP_Database_ObjectContainDataEntryDatabase)
											.GetFloatDataEntryByType(RP_DataEntry_EnumType.HPMax_最大HP);
										var percent = (floatPresentValueRPDataEntry.CurrentValue / maxHP.CurrentValue) *
										              100f;
										if (Check(percent, perInfo.CompareMethod, perInfo.ComparePercentValue))
										{
											Process(perInfo);
										}
									}
									break;
								case RP_DataEntry_EnumType.CurrentSP_当前SP:
									if (!perInfo.CompareToOtherDataEntry)
									{
										if (Check(floatPresentValueRPDataEntry.CurrentValue,
											perInfo.CompareMethod,
											perInfo.CompareAbsoluteValue))
										{
											Process(perInfo);
										}
									}
									else
									{
										var maxSP = (ds.ObjectArgu2 as I_RP_Database_ObjectContainDataEntryDatabase)
											.GetFloatDataEntryByType(RP_DataEntry_EnumType.SPMax_最大SP);
										var percentSP = floatPresentValueRPDataEntry.CurrentValue / maxSP.CurrentValue;
										if (Check(percentSP, perInfo.CompareMethod, perInfo.ComparePercentValue))
										{
											Process(perInfo);
										}
									}
									break;
								// case RP_DataEntry_EnumType.CurrentUP_当前UP:
								// 	if (!perInfo.CompareToOtherDataEntry)
								// 	{
								// 		if (Check(floatPresentValueRPDataEntry.CurrentValue,
								// 			perInfo.CompareMethod,
								// 			perInfo.CompareAbsoluteValue))
								// 		{
								// 			Process(perInfo);
								// 		}
								// 	}
								// 	else
								// 	{
								// 		var max = 100f;
								// 		var currentP = floatPresentValueRPDataEntry.CurrentValue / max * 100f;
								// 		if (Check(currentP, perInfo.CompareMethod, perInfo.ComparePercentValue))
								// 		{
								// 			Process(perInfo);
								// 		}
								// 	}
								// 	break;
							}
						}
					}

					break;
				case Float_RPDataEntry floatRPDataEntry:
					foreach (PerListenInfo perInfo in PerListenInfos)
					{
						if (perInfo.DataEntryType == floatRPDataEntry.RP_DataEntryType)
						{
							float currentValue = floatRPDataEntry.CurrentValue;
							//不是数据项，那就是和一个固定数值比对
							if (!perInfo.CompareToOtherDataEntry)
							{
								if (Check(currentValue, perInfo.CompareMethod, perInfo.CompareAbsoluteValue))
								{
									Process(perInfo);
								}
							}
							else
							{
								//是数据项，那就是和一个数据项比对
								var otherDataEntry = (ds.ObjectArgu2 as I_RP_Database_ObjectContainDataEntryDatabase)
									.GetFloatDataEntryByType(perInfo.OtherDataEntryType);
								if (otherDataEntry == null)
								{
									DBug.LogError("数据项比对时，找不到对应的数据项");
									return;
								}
								var percent = (currentValue / otherDataEntry.CurrentValue) * 100f;
								if (Check(percent, perInfo.CompareMethod, perInfo.ComparePercentValue))
								{
									Process(perInfo);
								}
							}
						}
					}
					break;
			}
		}

		private bool Check(float value1, CompareMethodEnum methodEnum, float value2)
		{
			switch (methodEnum)
			{
				case CompareMethodEnum.None:
					DBug.LogError("比对数据项不应该出现None");
					break;
				case CompareMethodEnum.Less_小于:
					return value1 < value2;
				case CompareMethodEnum.LessOrEqual_小于等于:
					return value1 <= value2;
				case CompareMethodEnum.Equal_等于:
					return Mathf.Approximately(value1, value2);
				case CompareMethodEnum.LargerOrEqual_大于等于:
					return value1 >= value2;
				case CompareMethodEnum.Larger_大于:
					return value1 > value2;
				case CompareMethodEnum.NotEqual_不等于:
					return !Mathf.Approximately(value1, value2);
				default:
					throw new ArgumentOutOfRangeException(nameof(methodEnum), methodEnum, null);
			}
			return false;
		}

		protected void Process(PerListenInfo perInfo)
		{
			if (!perInfo.Once || !perInfo.Executed)
			{
				perInfo.Executed = true;
				foreach (var commonComponent in perInfo.SideEffectOnCompareSuccess_DCCInfo.CommonComponents_RuntimeAll)
				{
					commonComponent.EnterComponent(RelatedAIBrainRuntimeInstance);
				}
			}
		}

		public override void UnRegisterListenInActionBus()
		{
			base.UnRegisterListenInActionBus();
			RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef.RegisterAction(
				ActionBus_ActionTypeEnum.L_DataEntry_OnDataEntryValueChanged_数据项的值被改变,
				_ABC_ProcessDataValue_OnDataValueChanged);

			foreach (var perListenInfoVARIABLE in PerListenInfos)
			{
				perListenInfoVARIABLE.SideEffectOnCompareSuccess_DCCInfo.ClearOnUnload();
			}
		}









	}

}