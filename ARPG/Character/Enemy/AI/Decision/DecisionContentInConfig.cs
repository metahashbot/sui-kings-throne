using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision
{
	[Serializable]
	public class DecisionContentInConfig
	{


		[LabelText("决策UID"), SerializeField, GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		public string DecisionID;

		[LabelText("初始选取权重"),  SerializeField] [HideInInspector]
		public float OriginalPickWeight = 50;

		[LabelText("作为活跃决策时依然可以被手动决策选取？"), SerializeField] [HideInInspector]
		public bool CanBePickedWhenActive = true;

		[SerializeField]
		[LabelText("【作用】配置信息")]
		public DCCConfigInfo DCCConfigInfo;

		[LabelText("自主推演可用？"), SerializeField]
		public bool CanAutoDeduce = true;
		
		[LabelText("选取此决策时其代表的占用级，通常高于运行时占用级"), FoldoutGroup("配置/【占用级】", false), SerializeField]
		[HideInInspector]
		public float OccupationLevel_Pick = 25;

		[LabelText("此决策运行时的占用级，通常低于选取占用"), FoldoutGroup("配置/【占用级】", false), SerializeField] [HideInInspector]

		public float OccupationLevel_Running = 50;

		[LabelText("此决策报告内容结束后的占用级，通常很低，以被打断"), FoldoutGroup("配置/【占用级】", false), SerializeField] [HideInInspector]

		public float OccupationLevel_End = 10;


		// [InfoBox("如果某个副作用没有指定【匹配动画】，那么此处的副作用都将会在进入决策时被执行")]
		// [SerializeReference, LabelText("【副作用-直属】决策关联的副作用组件们"), FoldoutGroup("配置/【副作用】", true)]
		// [ListDrawerSettings(ListElementLabelName = "@GetElementNameInList()")]
		// public List<BaseDecisionCommonComponent> CommonComponents;
		
		
		
		// [Space(30)]
		// [SerializeField, LabelText("【副作用-文件】文件配置的副作用组件们】"), FoldoutGroup("配置/【副作用】", true),
		//  ListDrawerSettings(ShowFoldout = true), InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		// public List<SOConfig_PresetSideEffects> CommonComponents_File;
		//
		//
		// [NonSerialized, LabelText("决策运行时副作用组件们"), FoldoutGroup("运行时")]
		// public List<BaseDecisionCommonComponent> CommonComponents_RuntimeAll;




	}
}