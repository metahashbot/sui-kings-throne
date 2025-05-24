using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common.HitEffect
{
	[Serializable]
	[CreateAssetMenu(fileName = "(S)受击特效配置集合", menuName = "#SO Assets#/#战斗关卡配置#/(S)受击特效配置集合", order = 56)]
	public class SOCollection_HitVFXGroupConfig : SOCollectionBase
	{

#if UNITY_EDITOR
		[Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			Collection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_HitVFXGroupConfig");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_HitVFXGroupConfig>(perPath);
				Collection.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif
		public List<SOConfig_HitVFXGroupConfig> Collection;

		public SOConfig_HitVFXGroupConfig FindConfigByName(string configName)
		{
			return Collection.Find(x => x.name.Equals(configName,StringComparison.OrdinalIgnoreCase));
		}
		


	}
}