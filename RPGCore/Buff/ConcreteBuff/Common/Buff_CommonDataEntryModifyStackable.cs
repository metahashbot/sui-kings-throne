using System;
using System.Collections.Generic;
using System.Text;
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
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	[TypeInfoBox("Buff：通用的数据项修正可叠层buff")]
	public class Buff_CommonDataEntryModifyStackable : BaseRPBuff, I_BuffContainTimedStack , I_BuffCanAsEquipmentPerk
	{
		// [ShowInInspector,LabelText("当前所有修饰效果"),FoldoutGroup("运行时",true)]
		// private List<PerBuffTimedStack> _buffStackCollection;

		private I_BuffContainTimedStack _stackInterface;
		
		
		[ShowInInspector, LabelText("当前所有叠层的修饰效果，UID-字典"), FoldoutGroup("运行时", true)]
		private Dictionary<string, BuffData_PerTypeStackInfoGroup> _selfBuffStackDictionary;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_selfBuffStackDictionary =
				CollectionPool<Dictionary<string, BuffData_PerTypeStackInfoGroup>,
					KeyValuePair<string, BuffData_PerTypeStackInfoGroup>>.Get();
			_stackInterface = this;
			_selfBuffStackDictionary.Clear();

			ReceiveAddStackByBCLP(BuffLogicPassingComponentsOnInit);
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			ReceiveAddStackByBCLP(blps);
			return ds;

		}



		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			ReceiveAddStackByBCLP(blps);
			return base.OnExistBuffRefreshed(caster, blps);
		}
		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			_stackInterface.FixedUpdateTick(currentTime, currentFrameCount, delta);
		}


		Dictionary<string, BuffData_PerTypeStackInfoGroup> I_BuffContainTimedStack.SelfBuffContentDict
		{
			get => _selfBuffStackDictionary;
			set => _selfBuffStackDictionary = value;
		}
		public BuffData_PerTypeStackInfoGroup GetBuffStackByInfo(string uid)
		{
			return _selfBuffStackDictionary[uid];
		}
		/// <summary>
		/// <para>通过BCLP(BuffLogicPassingComponent)来为buff应用具体的业务</para>
		/// </summary>
		/// <param name="blps"></param>
		public void ReceiveAddStackByBCLP(List<BaseBuffLogicPassingComponent> blps)
		{
			if (blps == null || blps.Count == 0)
			{
				return;
			}
			foreach (BaseBuffLogicPassingComponent perBLP in blps)
			{
				switch (perBLP)
				{
					case BuffLogicPassing_通用数据项修饰_GeneralDataEntryModify blp_dataEntry:
						
						//添加可能因为霸体的存在而不成功，此处将要进行相关业务
						var applyResult = GenericPool<RP_DS_StackApplyResult>.Get();
						applyResult.Reset();
						applyResult.DataType = blp_dataEntry.TargetEntry;
						applyResult.RelatedBLP = blp_dataEntry;
						applyResult.UID = blp_dataEntry.TargetUID;
						 applyResult.InternalFunctionFlagType = blp_dataEntry.InternalFunctionFlagType;
						var ds_tryApply =
							new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnStackPreAdd_一个数据叠层将要被添加);
						ds_tryApply.ObjectArgu1 = blp_dataEntry;
						ds_tryApply.ObjectArgu2 = applyResult;
						ds_tryApply.IntArgu1 = (int)blp_dataEntry.TargetEntry;
						Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ds_tryApply);

						if (applyResult.BlockByStoic)
						{
							GenericPool<RP_DS_StackApplyResult>.Release(applyResult);
							return;
						}
						GenericPool<RP_DS_StackApplyResult>.Release(applyResult);	
						

						BuffData_PerTypeStackInfoGroup targetGroupInfo = _CheckAndCreate(blp_dataEntry.TargetUID);

						//如果包含层数上限覆写的操作，则在此进行。会吧超过最大层数的层移除掉。
						if (blp_dataEntry.OverrideStack)
						{
							targetGroupInfo.CurrentMaxStack = blp_dataEntry.OverrideStackAs;
							if (targetGroupInfo.StackContentList.Count > targetGroupInfo.CurrentMaxStack)
							{
								for (int i = targetGroupInfo.StackContentList.Count - 1;
									i >= (targetGroupInfo.CurrentMaxStack) && i >= 0;
									i--)
								{
									targetGroupInfo.StackContentList[i].OnStackRemove();
									targetGroupInfo.StackContentList.RemoveAt(i);
								}
							}
						}


						//调整层数。调整层数和爆层是两个独立的业务。一次操作可能调整层数，调整完了依然可能因为层数满了而施加不上。
						
						//数量满了，那就爆了
						if (targetGroupInfo.CurrentMaxStack != 0 && targetGroupInfo.StackContentList.Count == targetGroupInfo.CurrentMaxStack)
						{
							/*
							 * 根据BLP内的配置，可能包含 满了之后触发的业务，通常有施加额外buff、清除当前组的所有层 
							 */
							if (blp_dataEntry.ContainStackFlowOperation)
							{
								if (blp_dataEntry.StackFlowOperationList != null &&
								    blp_dataEntry.StackFlowOperationList.Count > 0)
								{
									foreach (var one in blp_dataEntry.StackFlowOperationList)
									{
										Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(one.BuffType,
											Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
											Parent_SelfBelongToObject,
											one.GetFullBLPList());
									}
								}
								
								if (blp_dataEntry.ClearCurrentGroupOnStackFull)
								{
									targetGroupInfo.ClearStack();
								}

							}
							return;
						}

						SingleBuffTimedStack_WithDataModifier
							timedStackWithDm = SingleBuffTimedStack_WithDataModifier.GetFromPool();
						timedStackWithDm.RelatedBuffRef = this;
						timedStackWithDm.RelatedStackInfoGroupRef = targetGroupInfo;
						timedStackWithDm.FromTime = BaseGameReferenceService.CurrentFixedTime;
						timedStackWithDm.EntryRef =
							Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(blp_dataEntry.TargetEntry);
						timedStackWithDm.InitDuration = blp_dataEntry.ModifyDuration;
						timedStackWithDm.RelatedBuffRef = this;
						timedStackWithDm.ModifyEntryRef = Float_ModifyEntry_RPDataEntry.GetNewFromPool(
							blp_dataEntry.ModifyValue,
							RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
							blp_dataEntry.CalculatePosition);


						timedStackWithDm.OnStackAdd();
						targetGroupInfo.StackContentList.Add(timedStackWithDm);

						//如果需要显示在UI，则处理这个
						if (blp_dataEntry.NeedDisplayOnUI)
						{
							var ds_displayOnUI =
								new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
									.G_UI_RequireBuffDisplayContent_要求Buff显示内容);
							ds_displayOnUI.ObjectArgu1 = timedStackWithDm;
							ds_displayOnUI.ObjectArgu2 = blp_dataEntry.DisplayContent;
							timedStackWithDm.SelfBuffDisplayInfoField = blp_dataEntry.DisplayContent;
							GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_displayOnUI);
						}

						
						
						break;
					case BuffLogicPassing_有间隔的对表现值修饰_ModifyPresentValueWithInterval blp_modifyPV:

						//添加可能因为霸体的存在而不成功，此处将要进行相关业务
						var applyResult_pv = GenericPool<RP_DS_StackApplyResult>.Get();
						applyResult_pv.Reset();
						applyResult_pv.DataType = blp_modifyPV.TargetEntry;
						applyResult_pv.RelatedBLP = blp_modifyPV;
						applyResult_pv.UID = blp_modifyPV.TargetUID;
						applyResult_pv.InternalFunctionFlagType = blp_modifyPV.InternalFunctionFlagType;
						var ds_tryApply_pv =
							new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Buff_OnStackPreAdd_一个数据叠层将要被添加);
						ds_tryApply_pv.ObjectArgu1 = blp_modifyPV;
						ds_tryApply_pv.ObjectArgu2 = applyResult_pv;
						ds_tryApply_pv.IntArgu1 = (int)blp_modifyPV.TargetEntry;
						Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().TriggerActionByType(ds_tryApply_pv);

						if (applyResult_pv.BlockByStoic)
						{
							GenericPool<RP_DS_StackApplyResult>.Release(applyResult_pv);
							return;
						}
						GenericPool<RP_DS_StackApplyResult>.Release(applyResult_pv);	

						BuffData_PerTypeStackInfoGroup targetGroupInfo_PV = _CheckAndCreate(blp_modifyPV.TargetUID);
						targetGroupInfo_PV.StackUID = blp_modifyPV.TargetUID;
						if (targetGroupInfo_PV.StackUID == null || targetGroupInfo_PV.StackUID.Length < 1)
						{
							targetGroupInfo_PV.StackUID = SelfBuffType.ToString();
						}
					

						//如果包含层数上限覆写的操作，则在此进行。会吧超过最大层数的层移除掉。
						if (blp_modifyPV.OverrideStack)
						{
							targetGroupInfo_PV.CurrentMaxStack = blp_modifyPV.OverrideStackAs;
							if (targetGroupInfo_PV.StackContentList.Count > targetGroupInfo_PV.CurrentMaxStack)
							{
								for (int i = targetGroupInfo_PV.StackContentList.Count - 1;
									i >= (targetGroupInfo_PV.CurrentMaxStack) && i >= 0;
									i--)
								{
									targetGroupInfo_PV.StackContentList[i].OnStackRemove();
									targetGroupInfo_PV.StackContentList.RemoveAt(i);
								}
							}
						}
						
						//数量满了，那就爆了
						if (targetGroupInfo_PV.CurrentMaxStack != 0 && targetGroupInfo_PV.StackContentList.Count == targetGroupInfo_PV.CurrentMaxStack)
						{
							return;
						}
						
						
						BuffTimedStack_WithPresentValueModifyAndInterval timedStackWitchPvInterval =
							BuffTimedStack_WithPresentValueModifyAndInterval.GetFromPool();
						timedStackWitchPvInterval.RelatedBuffRef = this;
						timedStackWitchPvInterval.RelatedStackInfoGroupRef = targetGroupInfo_PV;
						timedStackWitchPvInterval.FromTime = BaseGameReferenceService.CurrentFixedTime;
						if (blp_modifyPV.MayResistByToughness)
						{
							var toughnessEntry =
								Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(
									RP_DataEntry_EnumType.Toughness_韧性);
							var toughnessValue = toughnessEntry.CurrentValue;
							toughnessValue = Mathf.Clamp(toughnessValue, 0f, 100f);
							float v  = blp_modifyPV.ModifyDuration * (1f - toughnessValue / 100f);
							timedStackWitchPvInterval.InitDuration = v;
						}
						else
						{
							timedStackWitchPvInterval.InitDuration = blp_modifyPV.ModifyDuration;
						}
						timedStackWitchPvInterval.RelatedBuffRef = this;
						timedStackWitchPvInterval.ModifyValue = blp_modifyPV.ModifyValue;
						timedStackWitchPvInterval.ModifyPosition = blp_modifyPV.CalculatePosition;
						timedStackWitchPvInterval.ApplyInterval = blp_modifyPV.Interval;
						timedStackWitchPvInterval.ModifyPresentValue =
							Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(blp_modifyPV.TargetEntry);
						timedStackWitchPvInterval.OnStackAdd();
						targetGroupInfo_PV.StackContentList.Add(timedStackWitchPvInterval);
						
						//如果需要显示在UI，则处理这个
						if (blp_modifyPV.NeedDisplayOnUI)
						{
							var ds_displayOnUI =
								new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
									.G_UI_RequireBuffDisplayContent_要求Buff显示内容);
							ds_displayOnUI.ObjectArgu1 = timedStackWitchPvInterval;
							ds_displayOnUI.ObjectArgu2 = blp_modifyPV.DisplayContent;
							timedStackWitchPvInterval.SelfBuffDisplayInfoField = blp_modifyPV.DisplayContent;
							GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_displayOnUI);
						}
						
						
						break;
				}
			}
		}


		private BuffData_PerTypeStackInfoGroup _CheckAndCreate(string s)
		{
			if (s == null || s.Length < 1)
			{
				throw new IndexOutOfRangeException($"在创建可叠层Buff时，传入的UID为空，这不合理，检查一下");
			}
			if (!_selfBuffStackDictionary.ContainsKey(s))
			{
				BuffData_PerTypeStackInfoGroup newList = GenericPool<BuffData_PerTypeStackInfoGroup>.Get();
				newList.StackUID = s;
				newList.StackContentList = CollectionPool<List<PerBuffTimedStack>, PerBuffTimedStack>.Get();
				_selfBuffStackDictionary.Add(s, newList);
			}
			return _selfBuffStackDictionary[s];
		}
		protected override void ClearAndUnload()
		{
			foreach (BuffData_PerTypeStackInfoGroup perGroup in _selfBuffStackDictionary.Values)
			{
				foreach (PerBuffTimedStack perStack in perGroup.StackContentList)
				{
					perStack.OnStackRemove();
				}
				perGroup.StackContentList.Clear();
				CollectionPool<List<PerBuffTimedStack>, PerBuffTimedStack>.Release(perGroup.StackContentList);
				GenericPool<BuffData_PerTypeStackInfoGroup>.Release(perGroup);
			}
			_selfBuffStackDictionary.Clear();
			CollectionPool<Dictionary<string, BuffData_PerTypeStackInfoGroup>, KeyValuePair<string,
				BuffData_PerTypeStackInfoGroup>>.Release(_selfBuffStackDictionary);
			base.ClearAndUnload();
		}


		public override string ToString()
		{
			StringBuilder selfSB = new StringBuilder();
			foreach (BuffData_PerTypeStackInfoGroup perTypeStackGroup in _selfBuffStackDictionary.Values)
			{
				selfSB.AppendLine(perTypeStackGroup.ToString());
				
				
				selfSB.AppendLine("        ======");
			}



			return selfSB.ToString();

		}
		
		
		
		
	}
}