using System;
using System.Collections.Generic;
using Global;
using RPGCore.Buff.Config;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Config
{
	[Serializable]
	[CreateAssetMenu(fileName = "Collection_帧动画信息组", menuName = "#SO Assets#/#角色配置#/#动画#/(S)动画信息组", order = 22)]
	public class SOCollection_SheetAnimationConfigGroup : SOCollectionBase
	{

#if UNITY_EDITOR
		[Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			SheetGroupCollection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_PerSheetAnimationGroup");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_PerSheetAnimationGroup>(perPath);
				SheetGroupCollection.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif
		public List<SOConfig_PerSheetAnimationGroup> SheetGroupCollection = new List<SOConfig_PerSheetAnimationGroup>();


		
		//find by index
		public SOConfig_PerSheetAnimationGroup GetSheetGroupByIndex(int index)
		{
			for (int i = 0; i < SheetGroupCollection.Count; i++)
			{
				if (SheetGroupCollection[i].RelatedCharacterSheetGroupIndex == index)
				{
					return SheetGroupCollection[i];
				}
			}
			DBug.LogError($"找不到索引为{index}的SheetGroup");
			return SheetGroupCollection.Find((group => group.RelatedCharacterSheetGroupIndex == 0));
		}
	}
}