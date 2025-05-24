using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Config;
using ExcelData;
using Global;
using Global.Character;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ARPG.Character.Config
{
	[TypeInfoBox("来自Excel的，ARPG玩家的初始化配置\n" + "并不是Excel直转，有些数据是内部构建的")]
	[Serializable]
	[CreateAssetMenu(fileName = "所有ARPG角色初始化配置",
		menuName = "#SO Assets#/#ARPG#/#角色#/#玩家#/SOFE_单个ARPG玩家角色初始化配置",
		order = 511)]
	public class SOFE_ARPGCharacterInitConfig : BaseSOFromExcel, I_ConfigCollectionCanBeResetLoadState
	{
		private PerConfigEntryInSOFE _placeHolderRef;

		[Serializable]
		public class PerConfigEntryInSOFE : BaseConcreteDataType
		{
			public string Name;
			public CharacterNamedTypeEnum Type;

			public string AIBrainID;

			public string InitJobStr;

			public string[] CharacterPerks;
			public string[] CharacterExps;

		}


		[Searchable, ListDrawerSettings(ShowFoldout = true)]
		public Dictionary<int, PerConfigEntryInSOFE> AllInfoDict = new Dictionary<int, PerConfigEntryInSOFE>();






		public PerConfigEntryInSOFE GetConfigByBehaviourRef(PlayerARPGConcreteCharacterBehaviour player)
		{
			return AllInfoDict[(int)player.SelfBehaviourNamedType];
		}

		public PerConfigEntryInSOFE GetConfigByType(CharacterNamedTypeEnum type)
		{
			return AllInfoDict[(int)type];
		}
		
		public PerConfigEntryInSOFE GetConfigByType(int type)
		{
			return AllInfoDict[type];
		}

		public PerConfigEntryInSOFE GetConfigByType(string cName)
		{
			return AllInfoDict[(int)CharacterInitExtend.GetCharacterTypeByString(cName)];
		}
		
		
		
		public void ResetOnLoad()
		{

			
		}
	}



}