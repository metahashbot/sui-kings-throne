using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "Buff Asset Entry",
		menuName = "#SO Assets#/#RPG Core#/[集合]Buff的关联资产", order = -190)]
	public class SOCollection_RPBuffAssetEntry : SOCollectionBase
	{
		#if UNITY_EDITOR
		[Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			AllAssetEntry.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_BuffLoadAssetEntry");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_BuffLoadAssetEntry>(perPath);
				AllAssetEntry.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif
		public List<SOConfig_BuffLoadAssetEntry> AllAssetEntry;

		public SOConfig_BuffLoadAssetEntry GetAssetEntryInfoByType(RolePlay_BuffTypeEnum type)
		{
			foreach (SOConfig_BuffLoadAssetEntry entry in AllAssetEntry)
			{
				if (entry.RelatedBuffType == type)
				{
					return entry;
				}
			}

			return null;
		}
	}
}