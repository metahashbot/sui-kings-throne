namespace ARPG.Equipment
{
	public enum WeaponAttackDirectionTypeEnum
	{
		None_未指定 = 0,
		PointerDirectionRegistered_记录的输入方向 = 1,
		PointerDirectionInstant_瞬时的输入方向 =2,
		RegisteredCharacterMovementDirection_记录的角色移动方向 =3,
		ControlledByHandler_由具体的Handler实现 = 4,
		PointerPositionOnTerrainRegistered_记录的指针位置 = 5,
		PointerPositionOnTerrainInstant_瞬时的指针位置 = 6,
		CharacterPosition_角色位置 = 7,
		RegisteredCharacterMoveDirectionThenPointer_记录的角色移动方向后指针 = 8,
		
	}
}