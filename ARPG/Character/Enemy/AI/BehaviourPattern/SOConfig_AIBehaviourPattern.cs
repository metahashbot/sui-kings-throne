using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision;
using ARPG.Character.Enemy.AI.Listen;
using Sirenix.OdinInspector;
using UnityEngine;
// using UnityEngine.Pool;
namespace ARPG.Character.Enemy.AI.BehaviourPattern
{
	[Serializable]
	[CreateAssetMenu(fileName = "_【行为模式】", menuName = "#SO Assets#/#敌人AI#/_【行为模式】配置", order = 161)]
	public class SOConfig_AIBehaviourPattern : ScriptableObject
	{
		[SerializeField, LabelText("【行为模式】的类型ID"), FoldoutGroup("配置", true),
		 GUIColor(250f / 255f, 113f / 255f, 15f / 255f)]
		public string BehaviourPatternTypeID;
		
		[Space(10)]
		[SerializeField,LabelText("行为模式目前可用", SdfIconType.Alarm),FoldoutGroup("配置", true)]
		public bool IsAvailable = true;


		[Space(10)]
		[SerializeField, LabelText("该行为模式所具有的标签"), FoldoutGroup("配置", true)]
		public AIBehaviourPatternCommonFlag CommonFlag;

		[Space(20)]
		[LabelText("预制决策队列"), FoldoutGroup("配置", true), SerializeField, GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		[ListDrawerSettings(DefaultExpandedState = true,ShowFoldout = true, NumberOfItemsPerPage = 50)]
		public List<string> PresetDecisionUIDList = new List<string>();



		[Space(30)]
		[LabelText("【决策】：该行为模式所包含的决策"), FoldoutGroup("配置", true), SerializeField]
		[ListDrawerSettings(DefaultExpandedState = true, ShowFoldout = true, NumberOfItemsPerPage = 50)]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public List<SOConfig_AIDecision> DecisionList = new List<SOConfig_AIDecision>();


#if UNITY_EDITOR

		[Button("【监听】删除重复的")]
		[FoldoutGroup("配置/操作", false)]
		private void RemoveDuplicateListen()
		{
			var listenList = new List<SOConfig_AIListen>();
			foreach (var listen in ListenList)
			{
				if (!listenList.Contains(listen))
				{
					listenList.Add(listen);
				}
			}
			ListenList = listenList;
			//set dirty
			UnityEditor.EditorUtility.SetDirty(this);
		}


		[Button("【决策】删除重复的")]
		[FoldoutGroup("配置/操作", false)]
		private void RemoveDuplicateDecision()
		{
			var decisionList = new List<SOConfig_AIDecision>();
			foreach (var decision in DecisionList)
			{
				if (!decisionList.Contains(decision))
				{
					decisionList.Add(decision);
				}
			}
			DecisionList = decisionList;
			//set dirty
			UnityEditor.EditorUtility.SetDirty(this);
		}

#endif
		

		[Space(20)]
		[LabelText("【监听】：该行为模式所包含的监听"), FoldoutGroup("配置", true), SerializeField]
		[ListDrawerSettings(DefaultExpandedState = true, ShowFoldout = true, NumberOfItemsPerPage = 50)]
		public List<SOConfig_AIListen> ListenList = new List<SOConfig_AIListen>();



#region 运行时


		[NonSerialized, ShowInInspector, ReadOnly, LabelText("关联的[脑]引用"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public SOConfig_AIBrain RelatedBrainRef;


		[NonSerialized, ShowInInspector, ReadOnly, LabelText("当前激活的是这个吗？"),
		 FoldoutGroup("运行时", false, VisibleIf = "@CommonEditorUtility.VisibleIfApplicationPlaying()")]
		public bool IsCurrentActivePattern = false;



#endregion

		//关联的编辑期Brain原始SOConfig引用，用来当各种下拉菜单的查找源的
		[HideInInspector, SerializeField]
		public SOConfig_AIBrain RelatedBrainSOConfig;



	}
}