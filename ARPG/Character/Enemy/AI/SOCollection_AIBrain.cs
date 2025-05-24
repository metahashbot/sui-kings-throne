using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI
{
	[Serializable]
	[CreateAssetMenu(fileName = "(集合)所有AIBrain", menuName = "#SO Assets#/#敌人AI#/(集合)所有AIBrain", order = 161)]
	public class SOCollection_AIBrain : SOCollectionBase
	{

#if UNITY_EDITOR
		[Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			Collection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIBrain");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIBrain>(perPath);
				Collection.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif

		public List<SOConfig_AIBrain> Collection;


		public SOConfig_AIBrain GetConfigByID(string uid)
		{
			uid = uid.Trim();
			foreach (SOConfig_AIBrain soConfigAibrain in Collection)
			{
				if (soConfigAibrain.ConfigContent.BrainTypeID.Trim().Equals(uid,StringComparison.OrdinalIgnoreCase))
				{
					return soConfigAibrain;
				}
			}
			return null;
		}
	}
}