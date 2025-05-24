using System;
namespace ARPG.Character.Enemy.AI.BehaviourPattern
{
	[Flags]
	public enum AIBehaviourPatternCommonFlag
	{
		None_未指定 = 0,
		AsDefaultPatternIfNotSpecific_作为默认模式如果未显式指定其他 = 1 << 1,
		BasePresetIGC_基本预设演出 = 1 << 5,
		BasePresetAfterSpawn_基本预设于生成后 = 1 << 6,
		NormalBattle_常规战斗 = 1 << 7,
		NormalExit_常规退场 = 1 << 8,
        ActivelyBattle_积极战斗 = 1<<9
	}
}