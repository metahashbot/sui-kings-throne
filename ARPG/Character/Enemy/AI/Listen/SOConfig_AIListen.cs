using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	[CreateAssetMenu(fileName = "一个AI监听配置",menuName = "#SO Assets#/#敌人AI#/一个AI监听配置",order = 161)]
	public class SOConfig_AIListen : ScriptableObject
	{
		[SerializeReference,LabelText("具体AI监听组件内容")]
		public BaseAIListenComponent ListenComponent;



		[NonSerialized,ShowInInspector]
		public SOConfig_AIListen RawListenTemplateRef;

		//关联的编辑期Brain原始SOConfig引用，用来当各种下拉菜单的查找源的
		[HideInInspector, SerializeField]
		public SOConfig_AIBrain RelatedBrainSOConfig;
		
		
		
	}
}