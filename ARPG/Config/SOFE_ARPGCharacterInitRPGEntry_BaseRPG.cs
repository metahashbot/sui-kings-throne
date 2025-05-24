using System;
using System.Collections.Generic;
using ARPG.Character.Config;
using Global;
using ICSharpCode.SharpZipLib.Tar;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static Cinemachine.DocumentationSortingAttribute;
using static UnityEngine.EventSystems.EventTrigger;
namespace ARPG.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "SOFE_ARPG角色初始化项_RPG项_基本RPG",
		menuName = "#SO Assets#/#ARPG#/#角色#/#来自Excel#/SOFE_ARPG角色初始化项_RPG项_基本RPG",
		order = 511)]
	public class SOFE_ARPGCharacterInitRPGEntry_BaseRPG : BaseSOFromExcel
    {
        [Serializable]
		public class ARPGCharacterEntry
        {
            public string Name;
            public int Level;
            public int DropExp;
            public int HPMax;
            public int SPMax;
            public float AttackPower;
            public float Defense;
            public float AttackSpeed;
            public float SkillAccelerate;
            public float Toughness;
            public float CriticalRate;
            public float CriticalBonus;
            public float Accuracy;
            public float Dodge;
            public float M_Strength;
            public float M_Dexterity;
            public float M_Vitality;
            public float M_Spirit;
            public float M_Intelligence;
            public float M_Charming;
            public float MoveSpeed;
            public float MassWeight;
            public float CorpseDuration;
            public float OverdamageOverride;
            public List<RolePlay_BuffTypeEnum> BuffTypeList;

            public ARPGCharacterEntry() { }
            public static ARPGCharacterEntry DeepCopy(ARPGCharacterEntry other)
            {
                var result = new ARPGCharacterEntry();
                result.Name = other.Name;
                result.Level = other.Level;
                result.DropExp = other.DropExp;
                result.HPMax = other.HPMax;
                result.SPMax = other.SPMax;
                result.AttackPower = other.AttackPower;
                result.Defense = other.Defense;
                result.AttackSpeed = other.AttackSpeed;
                result.SkillAccelerate = other.SkillAccelerate;
                result.Toughness = other.Toughness;
                result.CriticalRate = other.CriticalRate;
                result.CriticalBonus = other.CriticalBonus;
                result.Accuracy = other.Accuracy;
                result.Dodge = other.Dodge;
                result.M_Strength = other.M_Strength;
                result.M_Dexterity = other.M_Dexterity;
                result.M_Vitality = other.M_Vitality;
                result.M_Spirit = other.M_Spirit;
                result.M_Intelligence = other.M_Intelligence;
                result.M_Charming = other.M_Charming;
                result.MoveSpeed = other.MoveSpeed;
                result.MassWeight = other.MassWeight;
                result.CorpseDuration = other.CorpseDuration;
                result.OverdamageOverride = other.OverdamageOverride;
                result.BuffTypeList = other.BuffTypeList;
                return result;
            }
            public static ARPGCharacterEntry DeepCopy(ARPGHeroEntry other, int level)
            {
                var result = ARPGCharacterEntry.DeepCopy(other as ARPGCharacterEntry);
                result.Level = level;
                result.M_Strength = other.M_Strength + other.M_StrengthGrowth * level;
                result.M_Dexterity = other.M_Dexterity + other.M_DexterityGrowth * level;
                result.M_Vitality = other.M_Vitality + other.M_VitalityGrowth * level;
                result.M_Spirit = other.M_Spirit + other.M_SpiritGrowth * level;
                result.M_Intelligence = other.M_Intelligence + other.M_IntelligenceGrowth * level;
                result.M_Charming = other.M_Charming + other.M_CharmingGrowth * level;
                return result;
            }
        }

        [Serializable]
        public class ARPGHeroEntry : ARPGCharacterEntry
        {
            public float M_StrengthGrowth;
            public float M_DexterityGrowth;
            public float M_VitalityGrowth;
            public float M_SpiritGrowth;
            public float M_IntelligenceGrowth;
            public float M_CharmingGrowth;
        }

        /// <summary>
        /// <para>第一个key是角色的名字(Name，中文的那个)，第二个key是等级</para>
        /// </summary>
        [LabelText("小怪属性数据")]
        [Searchable, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, Dictionary<int, ARPGCharacterEntry>> ARPGMonsterConfig;
        [LabelText("英雄属性数据")]
        [Searchable, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, ARPGHeroEntry> ARPGHeroConfig;

        public void ClearMonsterConfig()
        {
            ARPGMonsterConfig = new Dictionary<string, Dictionary<int, ARPGCharacterEntry>>();
        }

        public void ClearHeroConfig()
        {
            ARPGHeroConfig = new Dictionary<string, ARPGHeroEntry>();
        }

        public void SetMonsterConfig(ARPGCharacterEntry entry)
        {
            if (!ARPGMonsterConfig.ContainsKey(entry.Name))
            {
                ARPGMonsterConfig.Add(entry.Name, new Dictionary<int, ARPGCharacterEntry>());
            }
            ARPGMonsterConfig[entry.Name].Add(entry.Level, entry);
        }

        public void SetHeroConfig(ARPGHeroEntry entry)
        {
            ARPGHeroConfig.Add(entry.Name, entry);
        }

        public ARPGCharacterEntry GetConfigByTypeAndLevel(CharacterNamedTypeEnum type, int level)
		{
			var name = type.GetCharacterNameStringByType();
			if (string.IsNullOrEmpty(name))
			{
				DBug.LogError($"角色枚举{type}没有对应的名字字符串，无法获取配置");
				return null;
			}

            var result = new ARPGCharacterEntry();
            if (ARPGMonsterConfig.ContainsKey(name))
            {
                var singleMonsterConfig = ARPGMonsterConfig[name];
                if (singleMonsterConfig.ContainsKey(level))
                {
                    result = ARPGCharacterEntry.DeepCopy(singleMonsterConfig[level]);
                }
            }
            else if (ARPGHeroConfig.ContainsKey(name))
            {
                result = ARPGCharacterEntry.DeepCopy(ARPGHeroConfig[name], level);
            }

            return result;
		}
	}
}