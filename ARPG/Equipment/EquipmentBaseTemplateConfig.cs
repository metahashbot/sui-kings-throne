using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Character.Job;
using Global;
using RPGCore.Buff.Config;
using RPGCore.UtilityDataStructure;
using UnityEngine;
namespace ARPG.Equipment
{
	[Serializable]
	public abstract class EquipmentBaseTemplateConfig
    {
        public int UID;
        public string Name;
        public Sprite IconSprite;
        public List<IngredientRequirementPair> IngredientRequirement;
        public string TemplateName;
		public EquipmentSlotTypeEnum SlotType;
		public EquipmentQualityTypeEnum QualityType;
		public float HPMax;
		public float SPMax;
		public float AttackPower;
		public float Defense;
		public float AttackSpeed;
		public float SkillAccelerate;
		public float Toughness;
		public float CriticalRate;
		public float CriticalBonus;
		public float Accuracy;
		public float Dodge;
		public float MoveSpeed;
		public float MainPropertyAddonMax;

		/// <summary>
		/// 匹配职业，如果Count==0就说明不限制
		/// </summary>
		public List<RP_BattleJobTypeEnum> MatchingJobType;
	}

	[Serializable]
	public class IngredientRequirementPair
	{
		public int UID;
		public int Count;
	}


    [Serializable]
	public class EquipmentRawPerkInfoPair
	{
		public string PerkID;
		public int Level;

		public static EquipmentRawPerkInfoPair GetDeepCopy(EquipmentRawPerkInfoPair copyFrom)
		{
			var tmpNew = new EquipmentRawPerkInfoPair();
			tmpNew.PerkID = copyFrom.PerkID.Trim();
			tmpNew.Level = copyFrom.Level;
			return tmpNew;
		}
	}


	public static class EquipmentRawPerkInfoPairExtend
	{
		// public static void FillContentToString(this EquipmentRawPerkInfoPair[] contents,ref  string raw)
		// {
		// 	foreach (EquipmentRawPerkInfoPair perPair in contents)
		// 	{
		// 		SOConfig_RPBuff rawPerk = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_EquipmentPerkPair
		// 			.GetBuffConfigByPerkID(perPair.PerkID);
		// 		rawPerk.ConcreteBuffFunction.FillEffectToString_Summary(ref raw, perPair.Level);
		// 	}
		// }
	}
}