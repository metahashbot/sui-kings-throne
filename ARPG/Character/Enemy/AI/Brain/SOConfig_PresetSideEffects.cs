using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI
{
	[Serializable]
	[CreateAssetMenu(fileName = "(P)预设的副作用配置", menuName = "#SO Assets#/#敌人AI#/(P)预设的副作用配置", order = 161)]
	public class SOConfig_PresetSideEffects : ScriptableObject
	{

		// [SerializeReference, LabelText("副作用配置们"), ListDrawerSettings(ShowFoldout = true)]
		// public List<BaseDecisionCommonComponent> DCCList = new List<BaseDecisionCommonComponent>();

		[SerializeField, LabelText("配置内容")]
		public DCCConfigInfo DCCConfigInfo;
		
		 
		// [Button("转换所有副作用到DCCConfig")]
		// private void _Button_ConvertAllSideEffectToDCCConfig()
		// {
		// 	//load all SOConfig_AIDecision
		// 	var allSOConfigDecision = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_PresetSideEffects");
		// 	foreach (var perSO in allSOConfigDecision)
		// 	{
		// 		var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
		// 		var perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PresetSideEffects>(perSOPath);
		// 		//convert all CommonComponents to DCCConfigInfo
		// 		DCCConfigInfo newConfigInfo = perSOAsset.DCCConfigInfo;
		// 		for (int i = 0; i < perSOAsset.DCCList.Count; i++)
		// 		{
		// 			var tmp = perSOAsset.DCCList[i];
		// 			if (tmp == null)
		// 			{
		// 				continue;
		// 			}
		// 			switch (tmp)
		// 			{
		// 				case DCC_停止特效配置_StopVFX:
		// 				case DCC_开始特效配置_StartVFX:
		// 				case DCC_停止特效配置_StopVFXFromConfig:
		// 				case DCC_生成特效配置_SpawnVFXFromConfig:
		// 					newConfigInfo.ContainVFX = true;
		// 					newConfigInfo.CCList_VFX.Add(tmp);
		// 					break;
		// 				case DCC_要求排队决策_RequireQueueDecision:
		// 				case DCC_要求抢占决策_RequirePreemptDecision:
		// 				case DCC_调整决策权重_ModifyDecisionWeight:
		// 				case DCC_调整决策自主推演可用性_ToggleDecisionAutoDeduce:
		// 				case DCC_按组加入决策至队列_AddDecisionGroupToQueue:
		// 				case DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce:
		// 				case DCC_要求立刻自主推演_RequireAutoDeduce:
		// 					newConfigInfo.ContainDecision = true;
		// 					newConfigInfo.CCList_Decision.Add(tmp);
		// 					break;
		// 				case DCC_开始一段位移_BeginMovement:
		// 				case DCC_进行一段位移操作_BeginMovementOperation:
		// 					newConfigInfo.ContainDis = true;
		// 					newConfigInfo.DCCList_Dis.Add(tmp);
		// 					break;
		// 				case DCC_停止版面_StopLayout:
		// 				case DCC_生成版面_SpawnLayout:
		// 				case DCC_停止UID版面_StopLayoutByUID:
		// 					newConfigInfo.ContainLayout = true;
		// 					newConfigInfo.DCC_LayoutList.Add(tmp);
		// 					break;
		// 				case DCC_停止游戏性事件_StopGameplayEvent:
		// 				case DCC_触发游戏性事件_LaunchGameplayEvent:
		// 					newConfigInfo.ContainEvent = true;
		// 					newConfigInfo.AbsorbList_PAEC.Add(tmp);
		// 					break;
		// 				case DCC_施加Buff_ApplyBuffWithArguments:
		// 				case DCC_移除Buff_RemoveBuffWithArguments:
		// 					newConfigInfo.ContainBuff = true;
		// 					newConfigInfo.BuffList_DCC.Add(tmp);
		// 					break;
		// 				case DCC_关闭描边预设任务_StopOutlinePresetTask:
		// 				case DCC_开启描边预设任务_StartOutlinePresetTask:
		// 					newConfigInfo.ContainOutline = true;
		// 					newConfigInfo.OutlineList_DCC.Add(tmp);
		// 					break;
		// 				case DCC_要求切换行为模式_RequireSwitchBehaviourPattern:
		// 				case DCC_调整行为模式可用性_ToggleBehaviourPatternAvailability:
		// 				case DCC_按类型切换行为模式_RequireSwitchBehaviourPatternByType:
		// 					newConfigInfo.ContainBP = true;
		// 					newConfigInfo.BPList_DCC.Add(tmp);
		// 					break;
		// 				default:
		// 					newConfigInfo.CommonComponents.Add(tmp);
		// 					break;
		// 			}
		// 		}
		// 		if (perSOAsset.DCCConfigInfo.CommonComponents_File != null &&
		// 		    perSOAsset.DCCConfigInfo.CommonComponents_File.Count > 0)
		// 		{
		// 			newConfigInfo.CommonComponents_File = new List<SOConfig_PresetSideEffects>();
		// 			for (int i = 0; i < perSOAsset.DCCConfigInfo.CommonComponents_File.Count; i++)
		// 			{
		// 				var tmp = perSOAsset.DCCConfigInfo.CommonComponents_File[i];
		// 				newConfigInfo.CommonComponents_File.Add(tmp);
		// 			}
		// 			perSOAsset.DCCConfigInfo.CommonComponents_File.Clear();
		// 		}
		// 		perSOAsset.DCCList.Clear();
		// 		UnityEditor.EditorUtility.SetDirty(perSOAsset);
		// 	}
		// }
		 
	}
}