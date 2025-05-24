using System;
using System.Collections.Generic;
using RPGCore.Buff.Config;
using UnityEngine;
namespace ARPG.Equipment
{
	[Serializable]
	[CreateAssetMenu(fileName = "SOFE-装备Perk匹配", menuName = "#SO Assets#/#材料道具#/SOFE-装备Perk匹配", order = 61)]
	public class SOFE_EquipmentPerkPair : ScriptableObject
	{

		[Serializable]
		public class ConcreteEquipmentPerkPair
		{
			public string PerkID;
			public SOConfig_RPBuff RelatedBuffConfig;
			/// <summary>
			/// <para>1位是武器，2位是防具，3位是饰品</para>
			/// </summary>
			public int MatchingType;
		}
		
		public List<ConcreteEquipmentPerkPair> AllEquipmentPerkInfoList = new List<ConcreteEquipmentPerkPair>();
		
		
		//find by id
		public SOConfig_RPBuff GetBuffConfigByPerkID(string perkID)
		{
			var found = AllEquipmentPerkInfoList.Find((x) => x.PerkID == perkID);
			if (found == null)
			{
				DBug.LogError($"找不到ID为{perkID}的装备Perk");
				return null;
			}

			return found.RelatedBuffConfig;
		}
		
	}
}