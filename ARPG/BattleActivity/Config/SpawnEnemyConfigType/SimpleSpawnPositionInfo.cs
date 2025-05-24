using System;
using ARPG.Config.BattleLevelConfig;
using Sirenix.OdinInspector;
namespace ARPG.Manager.Config
{
	[Serializable]
	public class SimpleSpawnPositionInfo
	{
		public EnemySpawnPoint_PresetTypeEnum SpawnPresetType = EnemySpawnPoint_PresetTypeEnum.Default_默认通用;
		[InfoBox("如果不是[默认通用]但又没填，则也相当于在这个类型的生成点上随机选")]
		[LabelText("特定生成点ID"), ShowIf(("@this.SpawnPresetType != EnemySpawnPoint_PresetTypeEnum.Default_默认通用"))]
		public string SpecificPointID;
	}
}