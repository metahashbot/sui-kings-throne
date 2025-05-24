using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "ESA_预设的敌人生成附加项_EnemySpawnAddon", menuName = "#SO Assets#/#战斗关卡配置#/敌人生成配置/ESA_预设的敌人生成附加项_EnemySpawnAddon", order = 56)]
	public class SOConfig_EnemySpawnAddonPresetConfig : ScriptableObject
	{
		[SerializeReference,LabelText("附加项内容")]
		public List<BaseEnemySpawnAddon> AddonList_Serialize;

	}
}