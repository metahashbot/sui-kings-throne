using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ARPG.Character.Config
{
	[CreateAssetMenu(fileName = "角色升级配置", menuName = "#SO Assets#/#ARPG#/#角色#/SOFE_角色升级配置", order = 511)]
	[Serializable]
	public class SOFE_ARPGLevelUpExpConfig : BaseSOFromExcel
	{
		[LabelText("等级-升级经验值对照表")]
		public Dictionary<int, int> LevelUpExpConfig = new Dictionary<int, int>();
		[LabelText("最大等级")]
		public int MaxLevel = 1;

		public void ClearConfig()
		{
            LevelUpExpConfig.Clear();

        }

		public void AddConfig(int level, int levelUpExp)
		{
			if(level > MaxLevel) MaxLevel = level;
			LevelUpExpConfig[level] = levelUpExp;
		}

		public int GetLevelUpExp(int level)
		{
			if (level > MaxLevel) return int.MaxValue;
			return LevelUpExpConfig[level];
		}
	}
}