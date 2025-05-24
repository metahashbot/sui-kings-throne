using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Inheritance
{
	[Serializable]
	[CreateAssetMenu(fileName = "继承信息总库", menuName = "#SO Assets#/#角色配置#/继承/继承信息总库", order = 21)]
	public class SOCollection_InheritancePerkCollection : SOCollectionBase
	{
#if UNITY_EDITOR
		[Button("刷新：刷新项目中的相关配置到这里来"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			Collection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_PerInheritanceConfig");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PerInheritanceConfig>(perPath);
				Collection.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif
		public List<SOConfig_PerInheritanceConfig> Collection;


	}
}