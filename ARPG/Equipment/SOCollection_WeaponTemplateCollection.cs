using System;
using System.Collections.Generic;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Equipment
{
	[Serializable]
	[CreateAssetMenu(fileName = "武器游戏性模板", menuName = "#SO Assets#/#Preset SO#/武器游戏性模板", order = 351)]
	public class SOCollection_WeaponTemplateCollection : SOCollectionBase
	{
#if UNITY_EDITOR
		[Button("刷新：将库内容刷新成与文件夹内容一致的样子"), PropertyOrder(2)]
		public override void Refresh()
		{
			UnityEditor.AssetDatabase.Refresh();
			Collection.Clear();
			var path = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_WeaponTemplate");
			foreach (var perGUID in path)
			{
				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
				var perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_WeaponTemplate>(perPath);
				Collection.Add(perSO);
			}
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}

		[Header(("==============================="))]
#endif
		public List<SOConfig_WeaponTemplate> Collection;


		
		
		
		/// <summary>
		/// <para>根据武器的游戏性模板名字（SO配置名）来查找那个SO。这个同样匹配Excel中的WeaponGameplayTemplateName</para>
		/// <para>这个模板只是游戏性模板。获取后还需要向其中添加来自Excel中的数据，尤其是【伤害属性】</para>
		/// </summary>
		public SOConfig_WeaponTemplate GetGameplayWeaponTemplateByGameplayTemplateName(string gameplayTemplateName)
		{
			if (gameplayTemplateName == null || gameplayTemplateName.Length<1)
			{
				return null;
			}
			var findIndex = Collection.FindIndex((template =>
				template.name.Equals(gameplayTemplateName ,StringComparison.OrdinalIgnoreCase)));
			if (findIndex == -1)
			{
				Debug.LogError($"没有找到名为{gameplayTemplateName}的武器游戏性模板");
				return null;
			}

			return Collection[findIndex];
		}
	}
}