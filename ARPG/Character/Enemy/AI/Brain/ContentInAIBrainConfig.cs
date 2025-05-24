using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.BehaviourPattern;
using ARPG.Character.Enemy.AI.Decision;
using ARPG.Character.Enemy.AI.Listen;
using RPGCore;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI
{
	[Serializable]
	public class ContentInAIBrainConfig
	{
		[LabelText("Brain类型ID"), FoldoutGroup("配置", true), SerializeField]
		public string BrainTypeID;


#if UNITY_EDITOR
		// [Button("为所有【行为模式】补充 循环异常")]
		// private void AddAbnormalDecision()
		// {
		// 	var dd_loop =
		// 		UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(
		// 			"Assets/Character_角色/ARPG_NPC非玩家角色/Common_通用/AID_通用循环异常.asset");
		// 	foreach (SOConfig_AIBehaviourPattern perBP in BehaviourPatternList)
		// 	{
		// 		if (!perBP.DecisionList.Contains(dd_loop))
		// 		{
		// 			perBP.DecisionList.Add(dd_loop);
		// 		}
		// 	}
		//
		// }
		// [Button("为所有【行为模式】补充 不循环异常")]
		// private void AddAbnormalSingleDecision()
		// {
		// 	var dd_single =
		// 		UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(
		// 			"Assets/Character_角色/ARPG_NPC非玩家角色/Common_通用/AID_通用不循环异常.asset");
		// 	foreach (SOConfig_AIBehaviourPattern perBP in BehaviourPatternList)
		// 	{
		// 		if (!perBP.DecisionList.Contains(dd_single))
		// 		{
		// 			perBP.DecisionList.Add(dd_single);
		// 		}
		// 	}
		//
		// }
		//
		// [Button("为所有【行为模式】补充 通用死亡")]
		// private void AddAbnormalDeathDecision()
		// {
		// 	var dd_death =
		// 		UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(
		// 			"Assets/Character_角色/ARPG_NPC非玩家角色/Common_通用/AID_通用普通死亡.asset");
		// 	foreach (SOConfig_AIBehaviourPattern perBP in BehaviourPatternList)
		// 	{
		// 		if (!perBP.DecisionList.Contains(dd_death))
		// 		{
		// 			perBP.DecisionList.Add(dd_death);
		// 		}
		// 	}

		// }
		 
		

#endif

		[Space]
		[LabelText("【行为模式】 ：该Brain所包含的行为模式们"), FoldoutGroup("配置", true), SerializeField]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		[ListDrawerSettings(ShowFoldout = true,DefaultExpandedState = true)]
		public List<SOConfig_AIBehaviourPattern> BehaviourPatternList = new List<SOConfig_AIBehaviourPattern>();


		[Space(30)]
		[LabelText("【监听】：该Brain的__常驻__监听们"), FoldoutGroup("配置", true), SerializeField, ShowInInspector]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		[ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
		public List<SOConfig_AIListen> AlwaysListenList = new List<SOConfig_AIListen>();



		[Space(30)]
		[LabelText("【公共决策】：===共用决策容器们==="), FoldoutGroup("配置", true), SerializeField]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		[ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
		public List<SOConfig_AIDecision> InternalPublicDecisionList = new List<SOConfig_AIDecision>();


		

#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif

	}
}