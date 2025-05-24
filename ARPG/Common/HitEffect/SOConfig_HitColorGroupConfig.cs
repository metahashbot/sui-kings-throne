using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common.HitEffect
{
	[Serializable]
	[CreateAssetMenu(fileName = "受击染色配置组", menuName = "#SO Assets#/#战斗关卡配置#/受击染色配置组", order = 56)]
	public class SOConfig_HitColorGroupConfig : ScriptableObject
	{
		[LabelText("这套配置的名字"), SerializeField]
		public string ConfigName;


		[LabelText("具体的配置信息们"), SerializeField, ListDrawerSettings(ShowFoldout = true)]
		public List<PerHitColorInfoPair> HitColorConfigList = new List<PerHitColorInfoPair>();
		
		

	}
}