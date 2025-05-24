using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff
{
	[Serializable]
	[TypeInfoBox("免伤将作用于 [L_Damage_OnDamageBeforeShield_伤害开始与护盾计算之前]，")]

	public class Buff_DamageReduceOverall : BaseRPBuff
	{

		// [LabelText("免伤|百分比减免部分"), FoldoutGroup("运行时", true), NonSerialized]
		// public float CurrentReducePartial = 1f;
		//
		// [LabelText("免伤|数值减免部分"), FoldoutGroup("运行时", true), NonSerialized]
		// public float CurrentReduceAddon = 0f;
		//
		//
		// public class PerStackData
		// {
		// 	public string StackUID;
		// 	public float StartTime;
		// 	public float RemainingDuration;
		// 	public float DamageReducePartial;
		// 	public float DamageReduceAddon;
		// }
		//
		//
		// public List<PerStackData> PerStackDataList;
		//
		//
		//
		//
		// public override void Init(
		// 	RPBuff_BuffHolder buffHolderRef,
		// 	SOConfig_RPBuff configRuntimeInstance,
		// 	SOConfig_RPBuff configRawTemplate,
		// 	I_RP_Buff_ObjectCanReceiveBuff parent,
		// 	I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		// {
		// 	PerStackDataList = CollectionPool<List<PerStackData>, PerStackData>.Get();
		//
		// 	base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
		// 	var lab = parent.ReceiveBuff_GetRelatedActionBus();
		// 	lab.RegisterAction(ActionBus_ActionTypeEnum.L_Damage_OnDamagePreTakenOnHP_伤害将要计算到HP上之前,
		// 		_ABC_ReduceDamageOverall_OnDamagePreTakenOnHP,
		// 		49);
		// }
		//
		//
		//
		// public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		// {
		// 	if (blps != null && blps.Length > 0)
		// 	{
		// 		foreach (var blp in blps)
		// 		{
		// 			ProcessPerBLP(blp);
		// 		}
		// 	}
		// 	return base.OnBuffInitialized(blps);
		// }
		//
		//
		// public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		// {
		// 	base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
		//
		// 	for (int i = PerStackDataList.Count - 1; i >= 0; i--)
		// 	{
		// 		PerStackData currentStack = PerStackDataList[i];
		//
		//
		//
		// 		if (Mathf.Approximately(currentStack.RemainingDuration, -1f))
		// 		{
		// 			continue;
		// 		}
		// 		currentStack.RemainingDuration -= delta;
		//
		//
		// 		if (currentStack.RemainingDuration <= 0f)
		// 		{
		// 			UnityEngine.Pool.GenericPool<PerStackData>.Release(currentStack);
		// 			PerStackDataList.RemoveAt(i);
		// 		}
		// 	}
		// }
		//
		// private void _ABC_ReduceDamageOverall_OnDamagePreTakenOnHP(DS_ActionBusArguGroup ds)
		// {
		// 	var daResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
		// 	if (daResult.DamageOriginalGenre ==
		// 	    DamageOriginalGenreTypeEnum. FromInternalDirect_内部直接对HP运算 ||
		// 	    daResult.DamageOriginalGenre == DamageOriginalGenreTypeEnum.FromSceneItem_来自场景物件)
		// 	{
		// 		return;
		// 	}
		// 	float currentAllPartial = 0f;
		// 	float currentAllAddon = 0f;
		// 	foreach (PerStackData perStack in PerStackDataList)
		// 	{
		// 		currentAllPartial += perStack.DamageReducePartial;
		// 		currentAllAddon += perStack.DamageReduceAddon;
		// 	}
		//
		// 	if (currentAllPartial > 99f)
		// 	{
		// 		currentAllPartial = 99f;
		// 	}
		// 	
		// 	daResult.TakenOnHealthPower =
		// 		daResult.TakenOnHealthPower * (1f - currentAllPartial / 100f) - currentAllAddon;
		// 	if(daResult.TakenOnHealthPower < 1f)
		// 	{
		// 		daResult.TakenOnHealthPower = 1f;
		// 	}
		// 	daResult.PopupDamageNumber = daResult.TakenOnHealthPower;
		//
		//
		//
		// }
		//
		//
		// public override DS_ActionBusArguGroup OnExistBuffRefreshed(
		// 	I_RP_Buff_ObjectCanApplyBuff caster,
		// 	List<BaseBuffLogicPassingComponent> blps)
		// {
		// 	if (blps != null && blps.Length > 0)
		// 	{
		// 		foreach (var blp in blps)
		// 		{
		// 			ProcessPerBLP(blp);
		// 		}
		// 	}
		// 	return base.OnExistBuffRefreshed(caster, blps);
		// }
		//
		// protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		// {
		// 	switch (blp)
		// 	{
		// 		case BLP_免伤传递参数_DamageReduceOverallBLP fragileOverallBLP:
		// 		{
		// 			PerStackData newStack = null;
		// 			if (fragileOverallBLP.IgnoreStack)
		// 			{
		// 				newStack = UnityEngine.Pool.GenericPool<PerStackData>.Get();
		// 				PerStackDataList.Add(newStack);
		// 			}
		// 			else
		// 			{
		// 				var tryFindIndex = PerStackDataList.FindIndex(x => x.StackUID == fragileOverallBLP.FromUID);
		// 				if (tryFindIndex != -1)
		// 				{
		// 					newStack = PerStackDataList[tryFindIndex];
		// 				}
		// 				else
		// 				{
		// 					newStack = UnityEngine.Pool.GenericPool<PerStackData>.Get();
		// 					PerStackDataList.Add(newStack);
		// 				}
		// 			}
		//
		// 			newStack.StartTime = BaseGameReferenceService.CurrentFixedTime;
		// 			newStack.RemainingDuration = fragileOverallBLP.Duration;
		// 			newStack.DamageReduceAddon = fragileOverallBLP.DamageReduceAddon;
		// 			newStack.DamageReducePartial = fragileOverallBLP.DamageReducePartial;
		// 			newStack.StackUID = fragileOverallBLP.FromUID;
		// 			break;
		// 		}
		// 	}
		// }
		//
		// protected override void ClearAndUnload()
		// {
		// 	base.ClearAndUnload();
		// 	foreach (var perStack in PerStackDataList)
		// 	{
		// 		UnityEngine.Pool.GenericPool<PerStackData>.Release(perStack);
		// 	}
		// 	PerStackDataList.Clear();
		// 	CollectionPool<List<PerStackData>, PerStackData>.Release(PerStackDataList);
		//
		//
		// 	Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
		// 		ActionBus_ActionTypeEnum.L_Damage_OnDamagePreTakenOnHP_伤害将要计算到HP上之前,
		// 		_ABC_ReduceDamageOverall_OnDamagePreTakenOnHP);
		// }
		//
		//
		//
		//
		// [Serializable]
		// public class BLP_免伤传递参数_DamageReduceOverallBLP : BaseBuffLogicPassingComponent
		// {
		//
		// 	[NonSerialized, ShowInInspector, ReadOnly]
		// 	public string FromUID;
		//
		// 	[SerializeField, LabelText("√：同来源是叠层？ | 口: 刷新")]
		// 	public bool IgnoreStack = false;
		//
		// 	[LabelText("百分比减少部分"), SuffixLabel("%")]
		// 	public float DamageReducePartial = 0f;
		//
		//
		// 	[LabelText("数值减少部分")]
		// 	public float DamageReduceAddon = 0f;
		//
		//
		// 	[LabelText("持续时长，-1就是无限")]
		// 	public float Duration = -1f;
		//
		//
		// 	public static BLP_免伤传递参数_DamageReduceOverallBLP GetFromPool(BLP_免伤传递参数_DamageReduceOverallBLP copyFrom)
		// 	{
		// 		var ret = new BLP_免伤传递参数_DamageReduceOverallBLP();
		// 		// var ret = UnityEngine.Pool.GenericPool<BLP_易伤传递参数_FragileOverallBLP>.Get();
		// 		ret.FromUID = copyFrom.FromUID;
		// 		ret.IgnoreStack = copyFrom.IgnoreStack;
		// 		ret.DamageReducePartial = copyFrom.DamageReducePartial;
		// 		;
		// 		ret.DamageReduceAddon = copyFrom.DamageReduceAddon;
		// 		ret.Duration = copyFrom.Duration;
		//
		// 		return ret;
		// 	}
		//
		//
		// 	public override void DeepCopyToRuntimeList(
		// 		BaseBuffLogicPassingComponent copyFrom,
		// 		List<BaseBuffLogicPassingComponent> targetList)
		// 	{
		// 		BLP_免伤传递参数_DamageReduceOverallBLP copyFromBLP = copyFrom as BLP_免伤传递参数_DamageReduceOverallBLP;
		// 		var newOne = GetFromPool(copyFromBLP);
		// 		targetList.Add(newOne);
		//
		//
		//
		// 	}
		// 	public override void DeepCopyToRuntimeArray(
		// 		BaseBuffLogicPassingComponent copyFrom,
		// 		BaseBuffLogicPassingComponent[] targetArray,
		// 		int index)
		// 	{
		// 		BLP_免伤传递参数_DamageReduceOverallBLP copyFromBLP = copyFrom as BLP_免伤传递参数_DamageReduceOverallBLP;
		// 		var newOne = GetFromPool(copyFromBLP);
		// 		targetArray[index] = newOne;
		// 	}
		// 	public override void Release()
		// 	{
		// 		return;
		// 		// UnityEngine.Pool.GenericPool<BLP_易伤传递参数_FragileOverallBLP>.Release(this);
		// 	}
		// }


		

	}
}