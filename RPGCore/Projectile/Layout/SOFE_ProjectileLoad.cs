using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace RPGCore.Projectile.Layout
{
	[Serializable]
	[CreateAssetMenu(fileName = "来自Excel_投射物加载", menuName = "#SO Assets#/#投射物#/来自Excel_投射物加载", order = 173)]
	public class SOFE_ProjectileLoad : ScriptableObject
	{
		[Serializable]
		public class LoadContent
		{
			public string ProjectileDisplayName;
			[GUIColor(255f/255f, 186f/255f, 239f/255f)]
			public string ProjectileNameUID;
			public string PrefabAddress;
			public int PreloadCount;
			public string RelationAddress;
			public string AppendVFXAddress;
			

			[NonSerialized]
			public AsyncOperationHandle<GameObject> _op_AppendVFX;
			public string DestroyAppendVFXAddress;
		}

		[OnValueChanged(nameof(_OVC_RefreshDisplayNameContents))]
		public List<LoadContent> Collection = new List<LoadContent>();




		public LoadContent GetLoadContentByUID(string uid)
		{
			var findIndex = Collection.FindIndex((content =>
				content.ProjectileNameUID.Equals(uid, StringComparison.OrdinalIgnoreCase)));
			if (findIndex == -1)
			{
				Debug.LogError($"找不到UID为{uid}的投射物加载配置");
				return null;
			}
			var tmp = Collection[findIndex];
			if (!string.IsNullOrEmpty(tmp.DestroyAppendVFXAddress))
			{
				tmp._op_AppendVFX =
					Addressables.LoadAssetAsync<GameObject>(Collection[findIndex].DestroyAppendVFXAddress);
				tmp._op_AppendVFX.WaitForCompletion();
			}
			return tmp;
		}

		public LoadContent GetLoadContentByDisplayName(string displayName)
		{
			var findIndex = Collection.FindIndex((content =>
				content.ProjectileDisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase)));
			if (findIndex == -1)
			{
				Debug.LogError($"找不到显示名称为{displayName}的投射物加载配置");
				return null;
			}
			var tmp = Collection[findIndex];
			if (!string.IsNullOrEmpty(tmp.DestroyAppendVFXAddress))
			{
				tmp._op_AppendVFX =
					Addressables.LoadAssetAsync<GameObject>(Collection[findIndex].DestroyAppendVFXAddress);
				tmp._op_AppendVFX.WaitForCompletion();
			}
			return tmp;
		}


		public void _OVC_RefreshDisplayNameContents()
		{
			DisplayNameList = new List<string>();
			foreach (LoadContent perContent in Collection)
			{
				if (!DisplayNameList.Contains(perContent.ProjectileDisplayName))
				{
					DisplayNameList.Add(perContent.ProjectileDisplayName);
				}
				else
				{
					DBug.LogError($"在投射物加载容器刷新时，出现了同样的显示名称:{perContent.ProjectileDisplayName}，检查一下");
				}
			}
		}

		[SerializeField, HideInInspector]
		public List<string> DisplayNameList = new List<string>();

		public List<string> GetDisplayNameList()
		{
			return DisplayNameList;
		}



#if UNITY_EDITOR
		// [Button("转换所有版面配置的投射物类型")]
		// public void Convert()
		// {
		// 	var soConfigs = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_ProjectileLayout");
		//
		// 	foreach (var perGUID in soConfigs)
		// 	{
		// 		var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
		// 		SOConfig_ProjectileLayout perSO =
		// 			UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_ProjectileLayout>(perPath);
		// 		List<string> displayNameList = perSO.LayoutContentInSO.RelatedProjectileTypeDisplayNameList;
		// 		displayNameList.Clear();
		// 		foreach (ProjectileTypeEnum perTypeEnum in perSO.LayoutContentInSO.RelatedProjectileList)
		// 		{
		// 			displayNameList.Add(GetLoadContentByUID(perTypeEnum.ToString()).ProjectileDisplayName);
		// 		}
		//
		// 		UnityEditor.EditorUtility.SetDirty(perSO);
		// 	}
		//
		// 	UnityEditor.AssetDatabase.SaveAssets();
		// }
#endif
	}
}