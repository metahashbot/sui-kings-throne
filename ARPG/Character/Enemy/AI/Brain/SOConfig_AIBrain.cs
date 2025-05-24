using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Config;
using ARPG.Character.Enemy.AI.BehaviourPattern;
using ARPG.Character.Enemy.AI.Decision;
using Global;
using RPGCore;
using RPGCore.UtilityDataStructure.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
namespace ARPG.Character.Enemy.AI
{
	[TypeInfoBox("一个AIBrain的配置。\n" +
	             "它记录了这个AIBrain都包含什么决策，都有什么样的监听\n" +
	             "决策和监听也是同样结构的SOConfig\n")]
	[Serializable]
	[CreateAssetMenu(fileName = "____AI Brain配置", menuName = "#SO Assets#/#敌人AI#/AI Brain配置", order = 161)]
	public class SOConfig_AIBrain : ScriptableObject
	{

		// [Button]
		// public void DDDD()
		// {
		// 	var currentAllBuffSO =
		// 		UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIBrain", null);
		// 	foreach (string path in currentAllBuffSO)
		// 	{
		// 		var selfPath = UnityEditor.AssetDatabase.GUIDToAssetPath(path);
		// 		var targetSoconfig =
		// 			UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIBrain>(selfPath);
		// 		if (targetSoconfig.BrainHandlerFunction == null)
		// 		{
		// 			Debug.LogError($"AIB里面没有FUnction，是{targetSoconfig.name},它位于{selfPath}");
		// 			continue;
		// 		}
		// 		if (targetSoconfig.ConfigContent.BehaviourPatternList == null)
		// 		{
		// 			Debug.LogError($"AIB里面没有行为模式，是{targetSoconfig.name},它位于{selfPath}");
		// 			continue;
		// 		}
		// 		foreach (SOConfig_AIBehaviourPattern perBP in targetSoconfig.ConfigContent.BehaviourPatternList)
		// 		{
		// 			if (perBP == null)
		// 			{
		// 				Debug.LogError($"AIB里面有行为模式，但是行为模式是空的，是{targetSoconfig.name},它位于{selfPath}");
		// 				continue;
		// 			}
		// 			if (perBP.DecisionList == null)
		// 			{
		// 				Debug.LogError($"AIB里面有行为模式，但是行为模式里面没有决策，是{targetSoconfig.name},它位于{selfPath}");
		// 				continue;
		// 			}
		// 			foreach (var perD in perBP.DecisionList)
		// 			{
		// 				if (perD.DecisionHandler == null)
		// 				{
		// 					Debug.LogError(
		// 						$"AIB里面有行为模式，行为模式里面有决策，但是决策里面没有DecisionHandler，是{targetSoconfig.name},它位于{selfPath}");
		// 					continue;
		// 				}
		// 				if (perD.DecisionHandler.AnimationEventCallbackList_Serialize != null &&
		// 				    perD.DecisionHandler.AnimationEventCallbackList_Serialize.Exists(c =>
		// 					    c.AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义 &&
		// 					    string.IsNullOrEmpty(c._AN_RelatedAnimationConfigName)))
		// 				{
		// 					Debug.LogError(
		// 						$"AIB里面有行为模式，行为模式里面有决策，决策里面有AnimationEventCallbackList_Serialize，但是有自定义事件，但是没有关联动画，是{targetSoconfig.name},它位于{selfPath}");
		// 					continue;
		// 				}
		// 			}
		// 		}
		// 	}
		// }

#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif
		public ContentInAIBrainConfig ConfigContent;
		
		
		
		
#if UNITY_EDITOR
        [Button("把SO文件的名字改成Type的名字"), GUIColor(1f, 1f, 0f)]
        private void Rename()
        {
            
            //rename so file name to RolePlay_BuffTypeEnum
            var soPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var soName = ConfigContent.BrainTypeID.ToString();
            UnityEditor.AssetDatabase.RenameAsset(soPath, soName);
            
            
            
        }
        [Button("刷新一下AIBrain的SO库"), GUIColor(1f, 1f, 0f)]
        private void RefreshCollection()
        {
	        //call SOCollection_RPBuff Refresh
	        var gcahh = Addressables.LoadAssetAsync<GameObject>("GCAHH").WaitForCompletion()
		        .GetComponent<GlobalConfigurationAssetHolderHelper>();
	        gcahh.Collection_AIBrainCollection.Refresh();
        }
        
        [Button("打开以编辑↓这个script↓"),GUIColor(1f,1f,0f)]
        private void _BUTTON_OpenScript()
        {
            if (BrainHandlerFunction == null)
            {
                return;
            }
            //find concrete class name
            var concreteClassName = BrainHandlerFunction.GetType().Name;
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





#if UNITY_EDITOR
#endif


		[Space(30)]
		[SerializeReference, LabelText("Brain的具体Handler")]
		public BaseAIBrainHandler BrainHandlerFunction;

		[NonSerialized, ShowInInspector, LabelText("原始AIBrainConfig的模板"), ReadOnly,
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public SOConfig_AIBrain RawAIBrainAssetTemplate;



#if UNITY_EDITOR

		// public List<string> GetAIDecisionNameList()
		// {
		// 	var newList = new List<string>();
		// 	foreach (var perD in ConfigContent.DecisionList)
		// 	{
		// 		newList.Add(perD.ConfigContent.DecisionID);
		// 	}
		// 	return newList;
		// }
		//
		// public List<string> GetAnimationNameList()
		// {
		// 	var newList = new List<string>();
		// 	foreach (var perN in BrainHandlerFunction.SelfAllPresetAnimationInfoList)
		// 	{
		// 		newList.Add(perN.ConfigName);
		//
		// 	}
		// 	return newList;
		//
		// }
		//
		// public List<string> GetVFXNameList()
		// {
		// 	var newList = new List<string>();
		// 	foreach (var perVFX in BrainHandlerFunction.AllVFXInfoList)
		// 	{
		// 		newList.Add(perVFX._VFX_InfoID);
		// 	}
		// 	return newList;
		// }


		// [Button("转移所有动画配置")]
		// private void _TransferAllAnimationConfig()
		// {
		// 	 //load all SOConfig_AIBrain
		// 	var allSOConfigAIBrain = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIBrain");
		// 	foreach (var perSO in allSOConfigAIBrain)
		// 	{
		// 		var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
		// 		SOConfig_AIBrain perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIBrain>(perSOPath);
		// 		if (perSOAsset.BrainHandlerFunction == null)
		// 		{
		// 			continue;
		// 		}
		// 		if (perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoList == null)
		// 		{
		// 			continue;
		// 		}
		// 		if (perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoList.Count == 0)
		// 		{
		// 			continue;
		// 		}
		// 		// create new SOConfig_PresetAnimationInfoGroup
		// 		SOConfig_PresetAnimationInfoBase newAnimationConfig =
		// 			ScriptableObject.CreateInstance<SOConfig_PresetAnimationInfoBase>();
		// 		newAnimationConfig.name = $"{name}_基本动画配置组";
		// 		//copy all 
		// 		newAnimationConfig.SelfAllPresetAnimationInfoList = new List<AnimationInfoBase>();
		// 		for (int i = 0; i < perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoList.Count; i++)
		// 		{
		// 			AnimationInfoBase perAnimation = perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoList[i];
		// 			switch (perAnimation)
		// 			{
		// 				case SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle a1:
		// 					newAnimationConfig.SelfAllPresetAnimationInfoList.Add(
		// 						SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle.GetDeepCopy(a1));
		// 					break;
		// 				case SheetAnimationMove_可移动的帧动画配置 sheetAnimationMove可移动的帧动画配置:
		// 					newAnimationConfig.SelfAllPresetAnimationInfoList.Add(
		// 						SheetAnimationMove_可移动的帧动画配置.GetDeepCopy(sheetAnimationMove可移动的帧动画配置));
		// 					break;
		// 				case SheetAnimationInfo_帧动画配置 sheetAnimationInfo帧动画配置:
		// 					newAnimationConfig.SelfAllPresetAnimationInfoList.Add(
		// 						SheetAnimationInfo_帧动画配置.GetDeepCopy(sheetAnimationInfo帧动画配置));
		// 					break;
		// 			}
		// 		}
		//
		//
		// 		//save file aside perSOAsset
		// 		var newAnimationConfigPath = $"{perSOPath.Replace(".asset", "")}_基本动画配置组.asset";
		// 		UnityEditor.AssetDatabase.CreateAsset(newAnimationConfig, newAnimationConfigPath);
		// 		//load it 
		// 		var newConfigConfig =
		// 			UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PresetAnimationInfoBase>(newAnimationConfigPath);
		// 		//assign to perSOAsset
		// 		if (perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoListFile == null)
		// 		{
		// 			perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoListFile =
		// 				new List<SOConfig_PresetAnimationInfoBase>();
		// 		}
		// 		perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoListFile.Add(newConfigConfig);
		// 		//set dirty
		// 		UnityEditor.EditorUtility.SetDirty(newAnimationConfig);
		// 		UnityEditor.EditorUtility.SetDirty(perSOAsset);
		// 	}
		// }
		
				// [Button("转移所有特效配置")]
		// private void _TransferAllVFXConfig()
		// {
		// 	//load all SOConfig_AIBrain
		// 	var allSOConfigAIBrain = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIBrain");
		// 	foreach (var perSO in allSOConfigAIBrain)
		// 	{
		// 		var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
		// 		SOConfig_AIBrain perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIBrain>(perSOPath);
		// 		if (perSOAsset.BrainHandlerFunction == null)
		// 		{
		// 			continue;
		// 		}
		// 		if (perSOAsset.BrainHandlerFunction.AllVFXInfoList == null)
		// 		{
		// 			continue;
		// 		}
		// 		if (perSOAsset.BrainHandlerFunction.AllVFXInfoList.Count == 0)
		// 		{
		// 			continue;
		// 		}
		// 		// create new SOConfig_PresetVFXInfoGroup
		// 		var newVFXConfig = ScriptableObject.CreateInstance<SOConfig_PresetVFXInfoGroup>();
		// 		newVFXConfig.name = $"{name}_基本特效配置组";
		// 		//copy all 
		// 		newVFXConfig.PerVFXInfoList = new List<PerVFXInfo>();
		// 		for (int i = 0; i < perSOAsset.BrainHandlerFunction.AllVFXInfoList.Count; i++)
		// 		{
		// 			var perVFX = perSOAsset.BrainHandlerFunction.AllVFXInfoList[i];
		// 			var newPerVFX = PerVFXInfo.GetDeepCopy(perVFX);
		// 			newVFXConfig.PerVFXInfoList.Add(newPerVFX);
		// 		}
		// 		
		// 		
		// 		//save file aside perSOAsset
		// 		var newVFXConfigPath = $"{perSOPath.Replace(".asset", "")}_基本特效配置组.asset";
		// 		UnityEditor.AssetDatabase.CreateAsset(newVFXConfig, newVFXConfigPath);
		// 		//load it 
		// 		var newConfigConfig =
		// 			UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PresetVFXInfoGroup>(newVFXConfigPath);
		// 		//assign to perSOAsset
		// 		if (perSOAsset.BrainHandlerFunction.AllVFXInfoList_File == null)
		// 		{
		// 			perSOAsset.BrainHandlerFunction.AllVFXInfoList_File = new List<SOConfig_PresetVFXInfoGroup>();
		// 		}
		// 		perSOAsset.BrainHandlerFunction.AllVFXInfoList_File.Add(newConfigConfig);
		// 		 //set dirty
		// 		 UnityEditor.EditorUtility.SetDirty(newVFXConfig);
		// 		UnityEditor.EditorUtility.SetDirty(perSOAsset);
		//
		// 	}
		// }

		[Button("为所有的Brain关联的配置文件注入依赖")]
		private void _InjectAllDep()
		{
			//load all SOConfig_AIBrain
			var allSOConfigAIBrain = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIBrain");
			foreach (var perSO in allSOConfigAIBrain)
			{
				var perSOPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perSO);
				SOConfig_AIBrain perSOAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIBrain>(perSOPath);

				if (perSOAsset.ConfigContent != null)
				{
					if (perSOAsset.ConfigContent.BehaviourPatternList != null)
					{
						foreach (SOConfig_AIBehaviourPattern perBP in perSOAsset.ConfigContent.BehaviourPatternList)
						{
							if (perBP == null)
							{
								continue;
							}
							perBP.RelatedBrainSOConfig = perSOAsset;
							
							if (perBP.DecisionList != null)
							{
								foreach (SOConfig_AIDecision perD in perBP.DecisionList)
								{
									if (perD == null)
									{
										continue;
									}
									perD.RelatedBrainSOConfig = perSOAsset;
									if (perD.DecisionHandler == null)
									{
										continue;
									}
									if (perD.DecisionHandler != null)
									{
										
										
									}
								}
							}
						}
					}
				}
				
				if (perSOAsset.BrainHandlerFunction == null)
				{
					continue;
				}
				if (perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoListFile == null)
				{
					continue;
				}
				if (perSOAsset.BrainHandlerFunction.SelfAllPresetAnimationInfoListFile.Count == 0)
				{
					continue;
				}
				 
			}
			
		}

#endif

	}
}