using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
// using UnityEngine.Pool;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class ESA_调整AI行为模式_SwitchAIBehaviourPattern : BaseEnemySpawnAddon
	{
		
		[LabelText("目标AI行为模式ID"),SerializeField, GUIColor(250f / 255f, 113f / 255f, 15f / 255f)]
		public string TargetBehaviourPatternID;

		[LabelText("行为模式相同时执行吗"),SerializeField]
		public bool SwitchOnSame;




		public override void ResetOnReturn()
		{
			
			GenericPool<ESA_调整AI行为模式_SwitchAIBehaviourPattern>.Release(this);
			
		}
	}
}