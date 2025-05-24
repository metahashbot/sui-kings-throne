using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Character.Player.Ally;
using RPGCore.Buff;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Base
{
	
	
	[Serializable]
	public abstract class BaseCharacterSearchOption 
	{
		
		
		public abstract bool IsCharacterMatched(BaseARPGCharacterBehaviour character);

	}

	[Serializable]
	public sealed class CSO_是指定阵营_Facility : BaseCharacterSearchOption
	{
		public enum RolePlay_FacilityTypeEnum
		{
			[LabelText("敌人")]
			Enemy = 1,
			[LabelText("友军")]
			Ally = 2,
			[LabelText("玩家")]
			Player = 3,
		}
		[LabelText("指定目标阵营")]
		public RolePlay_FacilityTypeEnum FacilityType = RolePlay_FacilityTypeEnum.Player;
		public override bool IsCharacterMatched(BaseARPGCharacterBehaviour character)
		{
			switch (FacilityType)
			{
				case RolePlay_FacilityTypeEnum.Enemy:
					if (character is EnemyARPGCharacterBehaviour enemy)
					{
						return true;
					}
					
					break;
				case RolePlay_FacilityTypeEnum.Ally:
					if (character is AllyARPGCharacterBehaviour)
					{
						return true;
					}
					break;
				case RolePlay_FacilityTypeEnum.Player:
					if(character is PlayerARPGConcreteCharacterBehaviour)
					{
						return true;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return false;
		}
	}
	

	[Serializable]
	public sealed class CSO_包含指定Buff_ContainBuff: BaseCharacterSearchOption
	{
		[Serializable]
		public class PerBuffRequirement
		{
			public RolePlay_BuffTypeEnum BuffType;
			[LabelText("√:必须有效 || 口:存在即可")]
			public bool TimeInOrJustExist = false;
		}

		[SerializeField, LabelText("Buff条件们")]
		public List<PerBuffRequirement> BuffRequirements = new List<PerBuffRequirement>();
		public override bool IsCharacterMatched(BaseARPGCharacterBehaviour character)
		{
			foreach (var perBuffRequirement in BuffRequirements)
			{
				BuffAvailableType result = character.ReceiveBuff_CheckTargetBuff(perBuffRequirement.BuffType);

				if (perBuffRequirement.TimeInOrJustExist &&
				    result == BuffAvailableType.TimeInButNotMeetOtherRequirement)
				{
				}
				else if (!perBuffRequirement.TimeInOrJustExist && result != BuffAvailableType.NotExist)
				{
				}
				else
				{
					return false;
				}
			}
			return true;
		}
	}
	
	
}