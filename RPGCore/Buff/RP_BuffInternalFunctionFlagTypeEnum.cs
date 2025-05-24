using System;
namespace RPGCore.Buff
{
	/// <summary>
	/// <para>Buff内部逻辑使用的各种flag</para>
	/// </summary>
	[Flags]
	public enum RP_BuffInternalFunctionFlagTypeEnum
	{
		None = 0,
		ResistByWeakStoic_被弱霸体抵抗 = 1,
		BlockByWeakStoic_被弱霸体屏蔽 = 1 << 2,
		BlockByStrongStoic_被强霸体屏蔽 = 1 << 3, 
		DisableCommonMovement_禁用普通移动 = 1 << 4,
	}
}