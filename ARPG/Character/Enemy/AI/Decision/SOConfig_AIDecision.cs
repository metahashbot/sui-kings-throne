using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
namespace ARPG.Character.Enemy.AI.Decision
{
	[Serializable]
	[CreateAssetMenu(fileName = "一个AI决策", menuName = "#SO Assets#/#敌人AI#/一个AI决策", order = 161)]
	public class SOConfig_AIDecision : ScriptableObject
	{
#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif
		[SerializeField,LabelText("固定配置部分")]
		public DecisionContentInConfig ConfigContent;


#if UNITY_EDITOR
		[Button("把SO文件的名字改成决策的名字"), GUIColor(1f, 1f, 0f)]
		private void Rename()
		{

			//rename so file name to RolePlay_BuffTypeEnum
			var soPath = UnityEditor.AssetDatabase.GetAssetPath(this);
			var soName = ConfigContent.DecisionID.ToString();
			UnityEditor.AssetDatabase.RenameAsset(soPath, soName);

		}
		[Button("打开以编辑↓这个script↓") ,GUIColor(1f, 1f, 0f)]
		private void _BUTTON_OpenScript()
		{
			if (DecisionHandler == null)
			{
				return;
			}
			//find concrete class name
			var concreteClassName = DecisionHandler.GetType().Name;
			//find all .cs file by assetdatabse
			var allScriptFiles = UnityEditor.AssetDatabase.FindAssets("t:Script");
			foreach (var scriptFile in allScriptFiles)
			{
				var scriptFilePath = UnityEditor.AssetDatabase.GUIDToAssetPath(scriptFile);
				if (scriptFilePath.Contains(concreteClassName))
				{
					UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(scriptFilePath, 0);
					break;
				}
			}
		}
#endif

		[SerializeReference, LabelText("决策Handler")]
		public BaseDecisionHandler DecisionHandler;


		[NonSerialized, LabelText("原始Decision模板"),ShowInInspector,ReadOnly]
		public SOConfig_AIDecision OriginalSOAssetTemplate;



#if UNITY_EDITOR
		
		// [Button("查一下所有带位移的副作用")]
		// private void _Button_FindAllMoveSideEffect()
		// {
		// 	var allSOConfigDecision = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
		// 	foreach (var perSO in allSOConfigDecision)
		// 	{
		// 		var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
		// 		var perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perSOPath);
		// 		foreach (var perDCC in perSOAsset.ConfigContent.CommonComponents)
		// 		{
		// 			if (perDCC is DCC_开始一段位移_BeginMovement)
		// 			{
		// 				Debug.Log($"于{perSOAsset.name}中找到了一个带位移的副作用：{perSOAsset.ConfigContent.DecisionID}");
		// 			}
		// 		}
		// 		foreach (SOConfig_PresetSideEffects perPreset in perSOAsset.ConfigContent.CommonComponents_File)
		// 		{
		// 			foreach (var perDCC in perPreset.DCCList)
		// 			{
		// 				if (perDCC is DCC_开始一段位移_BeginMovement)
		// 				{
		// 					Debug.Log($"于{perSOAsset.name}中找到了一个带位移的副作用：{perSOAsset.ConfigContent.DecisionID}");
		// 				}
		// 			}
		// 			
		// 		}
		// 	}
		// }
#endif
		//关联的编辑期Brain原始SOConfig引用，用来当各种下拉菜单的查找源的
		[HideInInspector, SerializeField]
		public SOConfig_AIBrain RelatedBrainSOConfig;




#if UNITY_EDITOR
		
		// [Button("转换所有副作用到DCCConfig")]
		// private void _Button_ConvertAllSideEffectToDCCConfig()
		// {
		// 	//load all SOConfig_AIDecision
		// 	var allSOConfigDecision = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
		// 	foreach (var perSO in allSOConfigDecision)
		// 	{
		// 		var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
		// 		var perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perSOPath);
		// 		//convert all CommonComponents to DCCConfigInfo
		// 		var newConfigInfo = perSOAsset.ConfigContent.DCCConfigInfo;
		// 		for (int i = 0; i < perSOAsset.ConfigContent.CommonComponents.Count; i++)
		// 		{
		// 			var tmp = perSOAsset.ConfigContent.CommonComponents[i];
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
		// 		if (perSOAsset.ConfigContent.CommonComponents_File != null &&
		// 		    perSOAsset.ConfigContent.CommonComponents_File.Count > 0)
		// 		{
		// 			newConfigInfo.CommonComponents_File = new List<SOConfig_PresetSideEffects>();
		// 			for (int i = 0; i < perSOAsset.ConfigContent.CommonComponents_File.Count; i++)
		// 			{
		// 				var tmp = perSOAsset.ConfigContent.CommonComponents_File[i];
		// 				newConfigInfo.CommonComponents_File.Add(tmp);
		// 			}
		// 			perSOAsset.ConfigContent.CommonComponents_File.Clear();
		// 		}
		// 		perSOAsset.ConfigContent.CommonComponents.Clear();
		// 		UnityEditor.EditorUtility.SetDirty(perSOAsset);
		// 	}
		// }
		 
		// [Button("转换所有副作用到DCCConfig")]
		// private void _Button_ConvertAllSideEffectToDCCConfig()
		// {
		// 	//load all SOConfig_AIDecision
		// 	var allSOConfigDecision = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
		// 	foreach (var perSO in allSOConfigDecision)
		// 	{
		// 		var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
		// 		var perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perSOPath);
		// 		//convert all CommonComponents to DCCConfigInfo
		// 		var newConfigInfo = perSOAsset.ConfigContent.DCCConfigInfo;
		// 		if (perSOAsset.DecisionHandler == null)
		// 		{
		// 			continue;
		// 		}
		// 		for (int i = 0; i < perSOAsset.DecisionHandler._sideEffectOnStopList.Count; i++)
		// 		{
		// 			var tmp = perSOAsset.DecisionHandler._sideEffectOnStopList[i];
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
		// 		perSOAsset.DecisionHandler._sideEffectOnStopList.Clear();
		// 		if (perSOAsset.DecisionHandler._sideEffectOnStopList_File != null &&
		// 		    perSOAsset.DecisionHandler._sideEffectOnStopList_File.Count > 0)
		// 		{
		// 			newConfigInfo.CommonComponents_File = new List<SOConfig_PresetSideEffects>();
		// 			for (int i = 0; i < perSOAsset.DecisionHandler._sideEffectOnStopList_File.Count; i++)
		// 			{
		// 				var tmp = perSOAsset.DecisionHandler._sideEffectOnStopList_File[i];
		// 				newConfigInfo.CommonComponents_File.Add(tmp);
		// 			}
		// 			perSOAsset.DecisionHandler._sideEffectOnStopList_File.Clear();
		// 		}
		// 		UnityEditor.EditorUtility.SetDirty(perSOAsset);
		// 	}
		// }
		//
		//
		// [Button("转换所Break有副作用到DCCConfig")]
		// private void _Button_ConvertAllSideEffectToDCCConfig_Break()
		// {
		// 	//load all SOConfig_AIDecision
		// 	var allSOConfigDecision = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
		// 	foreach (var perSO in allSOConfigDecision)
		// 	{
		// 		var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
		// 		var perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perSOPath);
		// 		//convert all CommonComponents to DCCConfigInfo
		// 		var newConfigInfo = perSOAsset.ConfigContent.DCCConfigInfo;
		// 		if (perSOAsset.DecisionHandler == null)
		// 		{
		// 			continue;
		// 		}
		// 		for (int i = 0; i < perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak.Count; i++)
		// 		{
		// 			var tmp = perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak[i];
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
		// 		perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak.Clear();
		// 		if (perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak_File != null &&
		// 		    perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak_File.Count > 0)
		// 		{
		// 			newConfigInfo.CommonComponents_File = new List<SOConfig_PresetSideEffects>();
		// 			for (int i = 0; i < perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak_File.Count; i++)
		// 			{
		// 				var tmp = perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak_File[i];
		// 				newConfigInfo.CommonComponents_File.Add(tmp);
		// 			}
		// 			perSOAsset.DecisionHandler._sideEffectOnStopList_OnBreak_File.Clear();
		// 		}
		// 		UnityEditor.EditorUtility.SetDirty(perSOAsset);
		// 	}
		// }
#endif
	}
}