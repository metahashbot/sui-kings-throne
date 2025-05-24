using System;
namespace ARPG.Character.Perk
{
	public enum CharacterPerkQualityTag
	{
		None_未指定 =0,
		Grey_灰色 = 1,
		White_白色 = 11,
		Blue_蓝色 = 21,
		Purple_紫色 = 31,
		Gold_金色 = 41,
	}

	
	[Serializable]
	public  class CPC_QualityTag : BaseCharacterPerkComponent
	{
		public CharacterPerkQualityTag QualityTag;

	}
}