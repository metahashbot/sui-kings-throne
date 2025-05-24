using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Manager.Config;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.BattleActivity.Config.EnemySpawnAddon
{
	[Serializable]
	[TypeInfoBox("！！！进行巡逻的SpawnPresetType应当为【Specific—指定专用】\n")]
	public class ESA_进行巡逻_EnterAsPatrol : BaseEnemySpawnAddon
	{

		[SerializeField, LabelText("包含行为模式切换？")]
		public bool HasBehaviourPatternSwitch = true;

		[SerializeField, LabelText("目标行为模式"), GUIColor(250f / 255f, 113f / 255f, 15f / 255f)]
		[ShowIf(nameof(HasBehaviourPatternSwitch))]
		public string TargetBehaviourPatternID ="巡逻预设";
		
		
		

		[SerializeField, LabelText("巡逻信息们")]
		public List<DH_在点位间巡逻_Patrol.PatrolInfoPair> PatrolInfoPairsList = new List<DH_在点位间巡逻_Patrol.PatrolInfoPair>();
		
		
		
		 
		
		
		
		
		public override void ResetOnReturn()
		{
			
			
			
			
		}
	}
}