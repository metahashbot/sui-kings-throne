using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "某项具体敌人生成配置", menuName = "#SO Assets#/#战斗关卡配置#/敌人生成配置/某项具体敌人生成配置", order = 56)]
	public class SOConfig_PerEnemyTypeSpawnConfig : ScriptableObject
	{

		[LabelText("具体内容"), SerializeField]
		public List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo> EnemyTypeInfoList =
			new List<SOConfig_SpawnEnemyConfig.PerEnemyTypeInfo>();



	}
}