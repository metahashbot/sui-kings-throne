using System;
using System.Linq;
using ARPG.Config;
using Global;
namespace ARPG.Character.Config
{
	public static class CharacterInitExtend
	{/// <summary>
	 /// <para>获取角色的三个模板，分别是：初始化配置、RPG初始化配置、角色资源配置。</para>
	 /// <para>这并不是角色实时的配置！如果要实时的，直接从GCSO里面读</para>
	 /// </summary>
	 /// <param name="type"></param>
	 /// <param name="level"></param>
	 /// <returns></returns>
		public static (SOFE_ARPGCharacterInitConfig.PerConfigEntryInSOFE,
			SOFE_ARPGCharacterInitRPGEntry_BaseRPG.ARPGCharacterEntry,
			SOFE_CharacterResourceInfo.PerTypeInfo) GetFullThreeInfoByType(
				this CharacterNamedTypeEnum type,
				int level = 1)
		{
			var gcahh = GlobalConfigurationAssetHolderHelper.GetGCAHH();
			var characterInitConfig = gcahh.FE_ARPGCharacterInitConfig.GetConfigByType(type);
			var characterRPGConfig = gcahh.FE_DataEntryInitConfig_BaseRPG.GetConfigByTypeAndLevel(type, level);
			var characterResourceConfig = gcahh.FE_CharacterResourceInfo.GetConfigByType(type);
			return (characterInitConfig, characterRPGConfig, characterResourceConfig);
		}
		
		
		
		
		
		public static string GetCharacterNameStringByType(this CharacterNamedTypeEnum type)
		{
			var rr = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_CharacterResourceInfo;
			foreach (var perD in rr.AllInfoDict.Values)
			{
				if (perD.CharacterIndex == (int)type)
				{
					return perD.CharacterName;
				}
			}

			return "";

		}
		
		
		


		public static CharacterNamedTypeEnum GetCharacterTypeByString(string name)
		{
			var allDict = GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_ARPGCharacterInitConfig.AllInfoDict;
			var ff = allDict.FirstOrDefault(x => x.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			return ff.Value == null ? CharacterNamedTypeEnum.None : ff.Value.Type;
		}



		public static int GetCharacterIDByString(string name)
		{
			return (int)GetCharacterTypeByString(name);
		}
		
		
		public static string GetCharacterNameByIntID( int id)
		{
			foreach (var VARIABLE in GlobalConfigurationAssetHolderHelper.GetGCAHH().FE_ARPGCharacterInitConfig
				.AllInfoDict.Values)
			{ 
				if ((int)VARIABLE.Type == id)
				{
					return VARIABLE.Name;
				}
				
			}
			return null;
		}

		
		
		

	}
}