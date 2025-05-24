using System;
namespace RPGCore
{
	[Flags]
	public enum FactionTypeEnumFlag
	{
		None_未指定 = 0,
		Player_玩家角色 = 1,
		PlayerAlly_玩家的友军  = 1<<2,
		
		Enemy_敌人   = 1<<3,
	}
}